(function () {
    const forms = document.querySelectorAll('[data-auth-modal-form]');

    forms.forEach((form) => {
        form.addEventListener('submit', async function (event) {
            event.preventDefault();

            if (window.jQuery && !window.jQuery(form).valid()) {
                return;
            }

            clearServerErrors(form);

            const submitButton = form.querySelector('[type="submit"]');
            setSubmitting(submitButton, true);

            try {
                const response = await fetch(form.action, {
                    method: 'POST',
                    body: new FormData(form),
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const payload = await response.json();
                if (response.ok && payload.succeeded && payload.redirectUrl) {
                    window.location.assign(payload.redirectUrl);
                    return;
                }

                showServerErrors(form, payload.errors || {});
            } catch {
                showServerErrors(form, {
                    '': ['Could not submit the form. Try again.']
                });
            } finally {
                setSubmitting(submitButton, false);
            }
        });
    });

    function clearServerErrors(form) {
        form.querySelectorAll('[data-valmsg-for]').forEach((message) => {
            message.textContent = '';
            message.classList.remove('field-validation-error');
            message.classList.add('field-validation-valid');
        });

        const summary = form.querySelector('[data-valmsg-summary="true"]');
        if (summary) {
            const list = summary.querySelector('ul');
            if (list) {
                list.innerHTML = '';
            }
        }
    }

    function showServerErrors(form, errors) {
        const summaryMessages = [];

        Object.entries(errors).forEach(([key, messages]) => {
            const fieldMessage = key ? form.querySelector(`[data-valmsg-for="${cssEscape(key)}"]`) : null;
            if (fieldMessage) {
                fieldMessage.textContent = messages.join(' ');
                fieldMessage.classList.remove('field-validation-valid');
                fieldMessage.classList.add('field-validation-error');
                return;
            }

            summaryMessages.push(...messages);
        });

        const summary = form.querySelector('[data-valmsg-summary="true"]');
        const list = summary?.querySelector('ul');
        if (list) {
            list.innerHTML = '';
            summaryMessages.forEach((message) => {
                const item = document.createElement('li');
                item.textContent = message;
                list.appendChild(item);
            });
        }
    }

    function setSubmitting(button, isSubmitting) {
        if (!button) {
            return;
        }

        button.disabled = isSubmitting;
    }

    function cssEscape(value) {
        if (window.CSS && window.CSS.escape) {
            return window.CSS.escape(value);
        }

        return value.replace(/["\\]/g, '\\$&');
    }
})();

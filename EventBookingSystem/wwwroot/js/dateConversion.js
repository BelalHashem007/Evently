document.querySelectorAll("[data-date]").forEach(el => {
    const date = new Date(el.dataset.date);
    el.textContent = date.toLocaleString(undefined, {
        year: "numeric",
        month: "2-digit",
        day: "numeric",
        hour: "2-digit",
        minute: "2-digit",
        hour12: true
    });
})

document.querySelectorAll("[data-date-badge-month]").forEach(el => {
    const date = new Date(el.dataset.dateBadgeMonth);
    console.log(date);
    el.textContent = date.toLocaleDateString(undefined, {
        month: "short"
    });
});

document.querySelectorAll("[data-date-badge-day]").forEach(el => {
    const date = new Date(el.dataset.dateBadgeDay);
    el.textContent = date.toLocaleDateString(undefined, {
        day: "numeric"
    });
});

document.querySelectorAll("[data-date-card]").forEach(el => {
    const date = new Date(el.dataset.dateCard);
    el.textContent = date.toLocaleDateString(undefined, {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
        hour12: true
    });
});

document.querySelectorAll("[data-date-admin]").forEach(el => {
    const date = new Date(el.dataset.dateAdmin);
    el.textContent = date.toLocaleDateString(undefined, {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
    });
});

document.querySelectorAll("[data-date-admin-bookings]").forEach(el => {
    const date = new Date(el.dataset.dateAdminBookings);
    el.textContent = date.toLocaleDateString(undefined, {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
        hour12: true

    });
});
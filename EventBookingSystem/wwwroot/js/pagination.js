(function () {

    //values from api
    const pageInp = document.querySelector("[name=page]");
    const totalInp = document.querySelector("[name=totalItems]");
    const totalPerPageInp = document.querySelector("[name=maxItemsPerPage]");

    // ------- state -----------
    const currentPage = Number(pageInp.getAttribute("value"));
    const totalCount = Number(totalInp.getAttribute("value"));
    const totalPerPageCount = Number(totalPerPageInp.getAttribute("value"));
    console.log(currentPage, totalCount, totalPerPageCount)

    //container
    const paginationContainer = document.getElementById('pagination-container');

    //-------ui------------
    const pageCount = Math.ceil(totalCount / totalPerPageCount);

    //prev button
    paginationContainer.appendChild(createPageButton("Previous", currentPage > 1, currentPage - 1));

    //numbered buttons
    const minNumber = Math.max(1, currentPage - 2); // 2 numbers behind
    const maxNumber = Math.min(currentPage + 2, pageCount) // 2 numbers ahead
    for (let i = minNumber; i <= maxNumber; i++) {
        const btn = createPageButton(i, true, i);
        if (currentPage === i) btn.classList.add('active');
        paginationContainer.appendChild(btn);
    }

    //next button
    paginationContainer.appendChild(createPageButton("Next", currentPage < pageCount, currentPage + 1))

    //function to create a button using bootstrap classes
    function createPageButton(text, isEnabled, pageNum) {
        const li = document.createElement('li');
        li.className = `page-item ${!isEnabled ? 'disabled' : ''}`;
        const a = document.createElement('a');
        a.className = 'page-link';
        a.href = `?page=${pageNum}`;
        a.innerText = text;

        li.appendChild(a);
        return li;
    }

})()
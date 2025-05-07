document.addEventListener("DOMContentLoaded", function () {
    const hash = window.location.hash;
    if (hash === "#completed") {
        const completedTab = document.querySelector('#completed-tab');
        const completedContent = document.querySelector('#completed');
        const upcomingTab = document.querySelector('#upcoming-tab');
        const upcomingContent = document.querySelector('#upcoming');

        completedTab.classList.add("active");
        completedContent.classList.add("show", "active");

        upcomingTab.classList.remove("active");
        upcomingContent.classList.remove("show", "active");
    }
});
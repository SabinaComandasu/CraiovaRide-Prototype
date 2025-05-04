function toggleEdit(field) {
    const display = document.getElementById(field + "-display");
    const input = document.getElementById(field + "-input");

    if (display.classList.contains("d-none")) {
        display.classList.remove("d-none");
        input.classList.add("d-none");
    } else {
        display.classList.add("d-none");
        input.classList.remove("d-none");
        input.focus();
    }
}

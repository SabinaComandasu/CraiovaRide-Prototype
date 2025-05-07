document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("trustedContactForm");
    const input = document.getElementById("trustedEmail");
    const message = document.getElementById("saveMessage");

    // Populate if already saved
    const savedEmail = localStorage.getItem("trustedContactEmail");
    if (savedEmail) input.value = savedEmail;

    form.addEventListener("submit", (e) => {
        e.preventDefault();
        const email = input.value.trim();
        if (email) {
            localStorage.setItem("trustedContactEmail", email);
            message.style.display = "block";
        }
    });
});

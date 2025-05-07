document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("cardForm");
    const successMsg = document.getElementById("successMsg");
    const savedCardSection = document.getElementById("savedCardDetails");

    function renderSavedCard() {
        const card = JSON.parse(localStorage.getItem("savedCard"));
        if (!card) {
            savedCardSection.innerHTML = "";
            form.style.display = "block";
            return;
        }

        form.style.display = "none";
        savedCardSection.innerHTML = `
            <div class="card border border-success p-3 mt-4">
                <h5 class="text-success">Saved Payment Method</h5>
                <p><strong>Card:</strong> **** **** **** ${card.number.slice(-4)}</p>
                <p><strong>Name:</strong> ${card.holder}</p>
                <p><strong>Expires:</strong> ${card.expiry}</p>
                <button id="deleteCardBtn" class="btn btn-danger">Delete</button>
            </div>
        `;

        document.getElementById("deleteCardBtn").addEventListener("click", () => {
            localStorage.removeItem("savedCard");
            renderSavedCard();
        });
    }

    form.addEventListener("submit", function (e) {
        e.preventDefault();

        const card = {
            number: document.getElementById("cardNumber").value,
            holder: document.getElementById("cardHolder").value,
            expiry: document.getElementById("expiryDate").value,
            cvv: document.getElementById("cvv").value
        };

        localStorage.setItem("savedCard", JSON.stringify(card));
        successMsg.style.display = "block";
        renderSavedCard();
    });

    renderSavedCard(); // check on load
});

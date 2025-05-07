// driver-popup.js

let ridePopups = [
    { pickup: "Old Town", destination: "Mall", fare: 24.5 },
    { pickup: "University", destination: "Train Station", fare: 30.0 },
    { pickup: "Electro", destination: "Parc Romanescu", fare: 18.75 },
    { pickup: "Central Station", destination: "Craiova Airport", fare: 45.0 }
];

function showRideRequest() {
    
    let ride = ridePopups[Math.floor(Math.random() * ridePopups.length)];
    let timer = 10; 

    const container = document.createElement("div");
    container.className = "ride-popup border border-dark p-3 bg-light shadow position-fixed";
    container.style.bottom = "20px";
    container.style.right = "20px";
    container.style.zIndex = "9999";
    container.style.width = "300px";

    container.innerHTML = `
    <h5>New Ride Request 🚗</h5>
    <p><strong>Pickup:</strong> ${ride.pickup}</p>
    <p><strong>Destination:</strong> ${ride.destination}</p>
    <p><strong>Fare:</strong> ${ride.fare} RON</p>
    <p><strong>Time to accept:</strong> <span id="ride-timer">${timer}</span>s</p>
    <div class="d-flex justify-content-between gap-2 mt-2">
        <button class="btn btn-success btn-sm w-50" onclick="acceptRide(this)">Accept</button>
        <button class="btn btn-danger btn-sm w-50" onclick="declineRide(this)">Decline</button>
    </div>
`;


    document.body.appendChild(container);

    const interval = setInterval(() => {
        timer--;
        container.querySelector("#ride-timer").innerText = timer;
        if (timer <= 0) {
            clearInterval(interval);
            container.remove();
            triggerNextRide();
        }
    }, 1000);
}

function acceptRide(button) {
    const popup = button.closest(".ride-popup");
    popup.innerHTML = `
        <h5>Ride Accepted ✅</h5>
        <p>Driving to destination...</p>
    `;

    setTimeout(() => {
        popup.innerHTML = `<h5>Ride Completed 🎉</h5>`;
        setTimeout(() => {
            popup.remove();
            window.location.href = "/Home";
        }, 2000);
    }, 3000);
}
function declineRide(button) {
    const popup = button.closest(".ride-popup");
    popup.remove();
    triggerNextRide(); 
}

function triggerNextRide() {
    const nextDelay = Math.floor(Math.random() * 10000) + 7000; // între 7 și 17 secunde
    setTimeout(showRideRequest, nextDelay);
}

// Pornim logica dacă există un container hidden pe pagină care semnalează că e driver
window.addEventListener('DOMContentLoaded', () => {
    const isDriver = document.getElementById("isDriverFlag");
    if (isDriver) {
        triggerNextRide();
    }
});

let rideTimeoutId = null;
let rideInProgress = false;
const SERVICE_AREA = {
    minLat: 44.25,
    maxLat: 44.40,
    minLon: 23.70,
    maxLon: 23.93
};


let map, marker, userLocationMarker, userCoords = null, routingControl = null;

// SIDEBAR
function toggleSidebar() {
    document.getElementById('sidebar').classList.add('show');
    document.getElementById('overlay').classList.add('show');
    document.getElementById('toggleBtn').style.display = 'none';
}
function closeSidebar() {
    document.getElementById('sidebar').classList.remove('show');
    document.getElementById('overlay').classList.remove('show');
    document.getElementById('toggleBtn').style.display = 'block';
}

// INIT MAP
document.addEventListener('DOMContentLoaded', () => {
    map = L.map('map', { zoomControl: false }).setView([44.3302, 23.7949], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors',
        maxZoom: 19
    }).addTo(map);
    L.control.zoom({ position: 'bottomright' }).addTo(map);

    locateUserAutomatically();

    document.addEventListener('click', e => {
        const input = document.getElementById('searchInput');
        const suggestions = document.getElementById('suggestions');
        const rideOptions = document.getElementById('rideOptions');
        const clickedInside = input.contains(e.target) || suggestions.contains(e.target) || rideOptions.contains(e.target);
        if (!clickedInside) suggestions.innerHTML = '';
    });
});

// LOCALIZARE
function locateUserAutomatically() {
    if (!navigator.geolocation) return;
    navigator.geolocation.getCurrentPosition(pos => {
        const lat = pos.coords.latitude, lon = pos.coords.longitude;
        userCoords = [lat, lon];

        const userIcon = L.icon({
            iconUrl: '/images/target.png',
            iconSize: [30, 30],
            iconAnchor: [15, 30],
            popupAnchor: [0, -30]
        });

        userLocationMarker = L.marker(userCoords, { icon: userIcon })
            .addTo(map)
            .bindPopup("You are here")
            .openPopup();
        map.setView(userCoords, 15);
    });
}

// CĂUTARE
function searchLocation() {
    const q = document.getElementById("searchInput").value.trim();
    if (!q) return;

    const bboxUrl = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(q)}&bounded=1&viewbox=23.75,44.35,23.85,44.28&limit=1`;

    fetch(bboxUrl).then(r => r.json()).then(data => {
        if (data.length) {
            updateMap(data[0]);
            saveSearchToHistory(data[0].display_name, data[0].lat, data[0].lon);
        } else {
            const fallbackUrl = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(q)}&limit=1`;
            fetch(fallbackUrl).then(r => r.json()).then(d => {
                if (d.length) {
                    updateMap(d[0]);
                    saveSearchToHistory(d[0].display_name, d[0].lat, d[0].lon);
                } else {
                    alert("No results found.");
                }
            });
        }
    }).catch(err => {
        console.error("Search error:", err);
        alert("Search failed.");
    });
}

// ACTUALIZARE HARTĂ CU TRASEU
function updateMap(loc) {
    const lat = parseFloat(loc.lat);
    const lon = parseFloat(loc.lon);

    if (!isWithinServiceArea(lat, lon)) {
        showServiceAreaError();
        return;
    }


    if (marker) map.removeLayer(marker);
    marker = L.marker([lat, lon])
        .addTo(map)
        .bindPopup(`<b>${loc.display_name}</b>`)
        .openPopup();
    map.setView([lat, lon], 15);

    if (routingControl) {
        map.removeControl(routingControl);
        routingControl = null;
    }

    if (userCoords) {
        createRoute(userCoords, [lat, lon]);
    }

    const rideOptions = document.getElementById('rideOptions');
    document.getElementById('paymentOptions').style.display = 'flex';
    rideOptions.style.display = 'block';
    rideOptions.style.opacity = '1';
    rideOptions.style.visibility = 'visible';
    rideOptions.style.position = 'relative';
    rideOptions.style.zIndex = '1000';
    rideOptions.style.height = 'auto';
    rideOptions.style.overflow = 'visible';
}

// AUTOCOMPLETE
let autocompleteTimer;
document.getElementById("searchInput").addEventListener("input", function () {
    const q = this.value.trim();
    clearTimeout(autocompleteTimer);
    if (q.length < 3) {
        document.getElementById("suggestions").innerHTML = "";
        return;
    }

    const bboxUrl = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(q)}&bounded=1&viewbox=23.75,44.35,23.85,44.28&limit=5`;

    autocompleteTimer = setTimeout(() => {
        fetch(bboxUrl).then(r => r.json()).then(data => {
            if (data.length) {
                showSuggestions(data);
            } else {
                fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(q)}&limit=5`)
                    .then(r => r.json()).then(d2 => showSuggestions(d2));
            }
        });
    }, 300);
});

function showSuggestions(list) {
    const html = list.map(i =>
        `<li onclick="selectSuggestion('${i.display_name.replace(/'/g, "\\'")}', ${i.lat}, ${i.lon})">${i.display_name}</li>`
    ).join("");
    document.getElementById("suggestions").innerHTML = `<ul class="autocomplete-list">${html}</ul>`;
}

function selectSuggestion(name, lat, lon) {
    document.getElementById("searchInput").value = name;
    document.getElementById("suggestions").innerHTML = "";
    updateMap({ display_name: name, lat, lon });
    saveSearchToHistory(name, lat, lon);
}

// ISTORIC
const SEARCH_HISTORY_KEY = "craiovaRideSearchHistory";
const MAX_HISTORY = 5;

function saveSearchToHistory(name, lat, lon) {
    const hist = JSON.parse(localStorage.getItem(SEARCH_HISTORY_KEY)) || [];
    const filtered = hist.filter(e => e.name !== name);
    const updated = [{ name, lat, lon }, ...filtered].slice(0, MAX_HISTORY);
    localStorage.setItem(SEARCH_HISTORY_KEY, JSON.stringify(updated));
}

function showSearchHistory() {
    const hist = JSON.parse(localStorage.getItem(SEARCH_HISTORY_KEY)) || [];
    if (!hist.length) return;
    showSuggestions(hist.map(e => ({
        display_name: e.name,
        lat: e.lat,
        lon: e.lon
    })));
}

document.getElementById("searchInput").addEventListener("focus", function () {
    if (!this.value.trim()) showSearchHistory();
});

// CANCEL RIDE
function cancelRide() {
    document.getElementById('rideOptions').style.display = 'none';
    document.getElementById('paymentOptions').style.display = 'none';
    document.getElementById("searchInput").value = "";

    if (marker) {
        map.removeLayer(marker);
        marker = null;
    }

    if (routingControl) {
        map.removeControl(routingControl);
        routingControl = null;
    }

    if (rideTimeoutId) {
        clearTimeout(rideTimeoutId);
        rideTimeoutId = null;
    }

    rideInProgress = false;
}

// Tarife per km
const RIDE_PRICES = {
    "Standard Ride": 2.5,
    "Eco Ride": 2.0,
    "Luggage": 3.0,
    "Deluxe Ride": 4.5,
    "Women for Women": 2.8
};

function createRoute(startCoords, endCoords) {
    routingControl = L.Routing.control({
        waypoints: [L.latLng(startCoords[0], startCoords[1]), L.latLng(endCoords[0], endCoords[1])],
        routeWhileDragging: false,
        show: false,
        addWaypoints: false,
        draggableWaypoints: false,
        fitSelectedRoutes: true,
        createMarker: () => null,
    })
        .on('routesfound', function (e) {
            const distanceInKm = e.routes[0].summary.totalDistance / 1000;
            updateRidePrices(distanceInKm.toFixed(2));
        })
        .addTo(map);
}

function updateRidePrices(distanceKm) {
    document.querySelectorAll('.ride-option').forEach(card => {
        const title = card.querySelector('h6').innerText.replace(/^[^\w]+/, '').trim();
        const pricePerKm = RIDE_PRICES[title];
        if (pricePerKm) {
            const total = (pricePerKm * distanceKm).toFixed(2);
            const existingPrice = card.querySelector('.card-price');
            if (existingPrice) {
                existingPrice.innerText = `Price: ${total} lei`;
            } else {
                const priceLine = `<p class="card-text text-success fw-bold card-price">Price: ${total} lei</p>`;
                card.querySelector('.card-body').insertAdjacentHTML('beforeend', priceLine);
            }
        }
    });
}

// RIDE SELECTION
function selectRide(rideType) {
    if (rideInProgress) return;

    const selectedPayment = document.querySelector('input[name="paymentMethod"]:checked');
    if (!selectedPayment) {
        document.getElementById('paymentErrorModal').style.display = 'block';
        return;
    }

    rideInProgress = true;

    document.querySelectorAll('.ride-option').forEach(card => card.classList.remove('selected'));
    const selectedCard = Array.from(document.querySelectorAll('.ride-option')).find(card =>
        card.innerText.includes(rideType)
    );
    if (selectedCard) selectedCard.classList.add('selected');

    document.getElementById('rideOptions').style.display = 'none';
    document.getElementById('paymentOptions').style.display = 'none';
    document.getElementById("searchInput").value = "";

    if (marker) {
        map.removeLayer(marker);
        marker = null;
    }

    if (userCoords) {
        map.setView(userCoords, 13);
    }

    document.getElementById("rideConfirmText").innerText =
        `You selected ${rideType}. Payment: ${selectedPayment.value.toUpperCase()}. Your driver will arrive shortly.`;
    document.getElementById("rideConfirmationModal").style.display = "block";
}

function closePaymentModal(event) {
    const modal = document.getElementById("paymentErrorModal");
    if (event.target === modal || event.target.classList.contains("close-btn")) {
        modal.style.display = "none";
    }
}

function closeRideModal(event) {
    const modal = document.getElementById("rideConfirmationModal");
    if (event.target === modal || event.target.classList.contains("close-btn")) {
        modal.style.display = "none";

        // 🔒 Dezactivăm bara de search
        document.getElementById("searchInput").disabled = true;

        const waitTime = Math.floor(Math.random() * 21 + 10) * 1000;
        rideTimeoutId = setTimeout(() => {
            showRatingModal();
        }, waitTime);
    }
}


function showRatingModal() {
    const modal = document.getElementById("rideRatingModal");
    const starsContainer = document.getElementById("ratingStars");
    const submitBtn = document.getElementById("submitRatingBtn");
    starsContainer.innerHTML = "";
    submitBtn.style.display = "none";

    for (let i = 1; i <= 5; i++) {
        const star = document.createElement("i");
        star.classList.add("fa", "fa-star");
        star.dataset.value = i;
        star.addEventListener("click", () => {
            document.querySelectorAll(".rating-stars i").forEach((s, idx) => {
                s.classList.toggle("selected", idx < i);
            });
            submitBtn.dataset.rating = i;
            submitBtn.style.display = "inline-block";
        });
        starsContainer.appendChild(star);
    }

    modal.style.display = "block";
}

function submitRating() {
    const rating = document.getElementById("submitRatingBtn").dataset.rating;
    document.getElementById("rideRatingModal").style.display = "none";

    if (routingControl) {
        map.removeControl(routingControl);
        routingControl = null;
    }

    if (rideTimeoutId) {
        clearTimeout(rideTimeoutId);
        rideTimeoutId = null;
    }

    document.getElementById("searchInput").disabled = false;

    rideInProgress = false;

}


function closeRatingModal(event) {
    const modal = document.getElementById("rideRatingModal");
    if (event.target === modal || event.target.classList.contains("close-btn")) {
        modal.style.display = "none";
    }
}
function isWithinServiceArea(lat, lon) {
    return lat >= SERVICE_AREA.minLat &&
        lat <= SERVICE_AREA.maxLat &&
        lon >= SERVICE_AREA.minLon &&
        lon <= SERVICE_AREA.maxLon;
}
function showServiceAreaError() {
    document.getElementById("serviceAreaModal").style.display = "block";
}
function closeServiceAreaModal(event) {
    const modal = document.getElementById("serviceAreaModal");
    if (event.target === modal || event.target.classList.contains("close-btn")) {
        modal.style.display = "none";
    }
}


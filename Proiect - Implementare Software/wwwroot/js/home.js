let map, marker, userLocationMarker, userCoords = null, polyline = null;

// -------------------------------------
// SIDEBAR TOGGLE
// -------------------------------------
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

// -------------------------------------
// INITIALIZE MAP ON LOAD
// -------------------------------------
document.addEventListener('DOMContentLoaded', () => {
    map = L.map('map', { zoomControl: false }).setView([44.3302, 23.7949], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors',
        maxZoom: 19
    }).addTo(map);
    L.control.zoom({ position: 'bottomright' }).addTo(map);

    locateUserAutomatically();

    // GLOBAL CLICK → hide only suggestions (NOT rideOptions)
    document.addEventListener('click', e => {
        const input = document.getElementById('searchInput');
        const suggestions = document.getElementById('suggestions');
        const rideOptions = document.getElementById('rideOptions');

        const clickedInside = input.contains(e.target)
            || suggestions.contains(e.target)
            || rideOptions.contains(e.target);

        if (!clickedInside) {
            suggestions.innerHTML = '';
        }
    });
});

// -------------------------------------
// USER LOCATION
// -------------------------------------
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

// -------------------------------------
// SEARCH & UPDATE MAP
// -------------------------------------
function searchLocation() {
    const q = document.getElementById("searchInput").value.trim();
    if (!q) return;

    const bboxUrl = `https://nominatim.openstreetmap.org/search?format=json&` +
        `q=${encodeURIComponent(q)}&bounded=1&viewbox=23.75,44.35,23.85,44.28&limit=1`;

    fetch(bboxUrl).then(r => r.json()).then(data => {
        if (data.length) {
            updateMap(data[0]);
            saveSearchToHistory(data[0].display_name, data[0].lat, data[0].lon);
        } else {
            const url = `https://nominatim.openstreetmap.org/search?format=json&` +
                `q=${encodeURIComponent(q)}&limit=1`;
            fetch(url).then(r => r.json()).then(d => {
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

function updateMap(loc) {
    const lat = parseFloat(loc.lat), lon = parseFloat(loc.lon);

    if (marker) map.removeLayer(marker);
    marker = L.marker([lat, lon])
        .addTo(map)
        .bindPopup(`<b>${loc.display_name}</b>`)
        .openPopup();
    map.setView([lat, lon], 15);

    if (polyline) map.removeLayer(polyline);
    if (userCoords) {
        polyline = L.polyline([userCoords, [lat, lon]], {
            color: 'blue', weight: 4, opacity: 0.7
        }).addTo(map);
    }

    // show ride options
    const rideOptions = document.getElementById('rideOptions');
    rideOptions.style.display = 'block';
    rideOptions.style.opacity = '1';
    rideOptions.style.visibility = 'visible';
    rideOptions.style.position = 'relative';
    rideOptions.style.zIndex = '1000';
    rideOptions.style.height = 'auto';
    rideOptions.style.overflow = 'visible';
}

// -------------------------------------
// AUTOCOMPLETE
// -------------------------------------
let autocompleteTimer;
document.getElementById("searchInput").addEventListener("input", function () {
    const q = this.value.trim();
    clearTimeout(autocompleteTimer);
    if (q.length < 3) {
        document.getElementById("suggestions").innerHTML = "";
        return;
    }

    const bboxUrl = `https://nominatim.openstreetmap.org/search?format=json&` +
        `q=${encodeURIComponent(q)}&bounded=1&viewbox=23.75,44.35,23.85,44.28&limit=5`;

    autocompleteTimer = setTimeout(() => {
        fetch(bboxUrl).then(r => r.json()).then(data => {
            if (data.length) {
                showSuggestions(data);
            } else {
                fetch(`https://nominatim.openstreetmap.org/search?format=json&` +
                    `q=${encodeURIComponent(q)}&limit=5`)
                    .then(r => r.json()).then(d2 => showSuggestions(d2));
            }
        });
    }, 300);
});

function showSuggestions(list) {
    const html = list.map(i =>
        `<li onclick="selectSuggestion('${i.display_name.replace(/'/g, "\\'")}',${i.lat},${i.lon})">
       ${i.display_name}
     </li>`
    ).join("");
    document.getElementById("suggestions").innerHTML = `<ul class="autocomplete-list">${html}</ul>`;
}

function selectSuggestion(name, lat, lon) {
    document.getElementById("searchInput").value = name;
    document.getElementById("suggestions").innerHTML = "";
    updateMap({ display_name: name, lat, lon });
    saveSearchToHistory(name, lat, lon);
}

// -------------------------------------
// SEARCH HISTORY
// -------------------------------------
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

function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('overlay');
    const toggleBtn = document.getElementById('toggleBtn');

    sidebar.classList.add('show');
    overlay.classList.add('show');
    toggleBtn.style.display = 'none';
}

function closeSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('overlay');
    const toggleBtn = document.getElementById('toggleBtn');

    sidebar.classList.remove('show');
    overlay.classList.remove('show');
    toggleBtn.style.display = 'block';
}

let map;
let marker;

document.addEventListener('DOMContentLoaded', function () {
    map = L.map('map', { zoomControl: false }).setView([44.3302, 23.7949], 13); // Craiova
    L.control.zoom({ position: 'bottomright' }).addTo(map);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors',
        maxZoom: 19
    }).addTo(map);
});

function searchLocation() {
    const query = document.getElementById("searchInput").value;
    if (!query) return;

    const craiovaUrl = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&bounded=1&viewbox=23.75,44.35,23.85,44.28&limit=1`;

    fetch(craiovaUrl)
        .then(res => res.json())
        .then(data => {
            if (data.length > 0) {
                updateMap(data[0]);
            } else {
                // fallback global
                const fallbackUrl = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=1`;
                fetch(fallbackUrl)
                    .then(res => res.json())
                    .then(data => {
                        if (data.length > 0) updateMap(data[0]);
                        else alert("No results found.");
                    });
            }
        })
        .catch(err => {
            console.error("Search error:", err);
            alert("Failed to search location.");
        });
    saveSearchToHistory(data[0].display_name, lat, lon);

}

function updateMap(location) {
    const lat = parseFloat(location.lat);
    const lon = parseFloat(location.lon);

    if (marker) map.removeLayer(marker);

    marker = L.marker([lat, lon]).addTo(map).bindPopup(`<b>${location.display_name}</b>`).openPopup();
    map.setView([lat, lon], 15);
}

let autocompleteTimer;

document.getElementById("searchInput").addEventListener("input", function () {
    const query = this.value;
    clearTimeout(autocompleteTimer);

    if (query.length < 3) {
        document.getElementById("suggestions").innerHTML = "";
        return;
    }

    const craiovaUrl = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&bounded=1&viewbox=23.75,44.35,23.85,44.28&limit=5`;

    autocompleteTimer = setTimeout(() => {
        fetch(craiovaUrl)
            .then(res => res.json())
            .then(data => {
                if (data.length > 0) {
                    showSuggestions(data);
                } else {
                    // fallback global
                    fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=5`)
                        .then(res => res.json())
                        .then(data => showSuggestions(data));
                }
            });
    }, 400);
});

function showSuggestions(data) {
    const list = data.map(item =>
        `<li onclick="selectSuggestion('${item.display_name.replace(/'/g, "\\'")}', ${item.lat}, ${item.lon})">${item.display_name}</li>`
    ).join("");

    document.getElementById("suggestions").innerHTML = `<ul class="autocomplete-list">${list}</ul>`;
}

function selectSuggestion(name, lat, lon) {
    document.getElementById("searchInput").value = name;
    document.getElementById("suggestions").innerHTML = "";

    if (marker) map.removeLayer(marker);

    marker = L.marker([lat, lon]).addTo(map).bindPopup(`<b>${name}</b>`).openPopup();
    map.setView([lat, lon], 15);
    saveSearchToHistory(name, lat, lon);
}
const SEARCH_HISTORY_KEY = "craiovaRideSearchHistory";
const MAX_HISTORY = 5;

function saveSearchToHistory(name, lat, lon) {
    const history = JSON.parse(localStorage.getItem(SEARCH_HISTORY_KEY)) || [];
    const newEntry = { name, lat, lon };

    // Evită duplicatele
    const filtered = history.filter(item => item.name !== name);

    // Adaugă noua locație la început și taie dacă e prea lung
    const updated = [newEntry, ...filtered].slice(0, MAX_HISTORY);

    localStorage.setItem(SEARCH_HISTORY_KEY, JSON.stringify(updated));
}

function showSearchHistory() {
    const history = JSON.parse(localStorage.getItem(SEARCH_HISTORY_KEY)) || [];
    if (history.length === 0) return;

    const list = history.map(item =>
        `<li onclick="selectSuggestion('${item.name.replace(/'/g, "\\'")}', ${item.lat}, ${item.lon})">${item.name}</li>`
    ).join("");

    document.getElementById("suggestions").innerHTML = `<ul class="autocomplete-list">${list}</ul>`;
}

document.getElementById("searchInput").addEventListener("focus", function () {
    const query = this.value.trim();
    if (query.length === 0) {
        showSearchHistory();
    }
});
// Închide sugestiile când se face click în afara inputului sau listei
document.addEventListener("click", function (event) {
    const input = document.getElementById("searchInput");
    const suggestions = document.getElementById("suggestions");

    if (!input.contains(event.target) && !suggestions.contains(event.target)) {
        suggestions.innerHTML = "";
    }
});

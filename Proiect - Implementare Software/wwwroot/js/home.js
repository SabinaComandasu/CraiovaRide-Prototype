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

// Inițializează harta Leaflet
document.addEventListener('DOMContentLoaded', function () {
    var map = L.map('map', {
        zoomControl: false // dezactivăm zoom-ul standard
    }).setView([44.3302, 23.7949], 13);

    // Adăugăm zoom control manual, în alt colț
    L.control.zoom({
        position: 'bottomright' // poate fi 'bottomleft', 'topright', 'bottomright'
    }).addTo(map);

    // TileLayer rămâne la fel
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);
});

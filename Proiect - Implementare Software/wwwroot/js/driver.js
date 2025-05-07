
setTimeout(function () {
    document.getElementById("waiting").style.display = "none";
    document.getElementById("ridesTable").style.display = "block";
}, Math.floor(Math.random() * 5000) + 3000); // 3–8 secunde

function acceptRide(btn) {
    document.getElementById("ridesTable").style.display = "none";
    document.getElementById("accepted").style.display = "block";

    setTimeout(function () {
        document.getElementById("accepted").style.display = "none";
        document.getElementById("finished").style.display = "block";

        setTimeout(function () {
            window.location.href = "/Home"; 
        }, 3000);
    }, Math.floor(Math.random() * 3000) + 3000); 
}

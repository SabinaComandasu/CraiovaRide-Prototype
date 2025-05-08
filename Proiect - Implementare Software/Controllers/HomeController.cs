using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proiect_Implementare_Software.Models;
using Proiect_Implementare_Software.Data;
using Proiect_Implementare_Software.Services;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Proiect_Implementare_Software.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService;
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;


        public HomeController(
            ILogger<HomeController> logger,
            IHomeService homeService,
            AppDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _homeService = homeService;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (person == null)
                return RedirectToAction("Error", "Home");

            // ✅ Info pentru sidebar (inclusiv poza)
            ViewBag.FullName = person.FullName;
            ViewBag.PersonID = person.PersonID;
            ViewBag.Rating = person.Rating.ToString("0.0");
            ViewBag.AvatarPath = string.IsNullOrEmpty(person.Avatar)
                ? "/images/default-avatar.png"
                : "/images/" + person.Avatar;

            // ✅ Ride notifications
            var now = DateTime.Now;
            var windowStart = now.AddMinutes(-10);
            var windowEnd = now.AddMinutes(10);

            var expiredRides = await _context.Rides
                .Where(r => r.UserID == person.PersonID &&
                            r.RideStatus == "Scheduled" &&
                            r.Date < now)
                .ToListAsync();

            foreach (var ride in expiredRides)
                ride.RideStatus = "Completed";

            if (expiredRides.Count > 0)
                await _context.SaveChangesAsync();

            var upcomingRides = await _context.Rides
                .Where(r => r.UserID == person.PersonID &&
                            r.RideStatus == "Scheduled" &&
                            r.Date >= windowStart &&
                            r.Date <= windowEnd)
                .OrderBy(r => r.Date)
                .ToListAsync();

            ViewBag.RideNotifications = upcomingRides
                .Select(r => $"🚕 You have a scheduled ride at {r.Date:HH:mm}. Get ready!")
                .ToList();

            return View();
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveRide([FromBody] Ride ride)
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null) return Unauthorized();

            var user = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == identityUser.Id);
            if (user == null) return NotFound("User not found.");

            var driver = await _context.Persons
                .Where(p => p.DriverStatus == "Driver" && p.Role == "User")
                .OrderBy(r => Guid.NewGuid())
                .FirstOrDefaultAsync();
            if (driver == null) return BadRequest("No available driver.");

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.DriverID == driver.PersonID);
            if (vehicle == null) return BadRequest("No vehicle for driver.");

            // ❗ Validate basic input
            if (string.IsNullOrWhiteSpace(ride.PickupLocation) || string.IsNullOrWhiteSpace(ride.Destination))
                return BadRequest("Pickup and Destination are required.");

            // ✅ Complete ride details
            ride.UserID = user.PersonID;
            ride.DriverID = driver.PersonID;
            ride.VehicleID = vehicle.VehicleID;
            ride.RideStatus = "Completed";
            ride.Date = DateTime.Now;

            _context.Rides.Add(ride);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ride saved successfully" });
        }


        [HttpGet]
        public async Task<IActionResult> GetAvatar(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null || person.Avatar == null)
                return NotFound();

            var imageBytes = Convert.FromBase64String(person.Avatar); // sau dacă e deja binar, returnezi direct
            return File(imageBytes, "image/png"); // sau image/jpeg
        }
    }
}
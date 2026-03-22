using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_Implementare_Software.Data;
using Proiect_Implementare_Software.Models;
using Proiect_Implementare_Software.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Proiect_Implementare_Software.Controllers
{
    public class RidesController : Controller
    {
        private readonly IRideService _rideService;
        private readonly AppDbContext _context;
        private readonly ILogger<RidesController> _logger;

        public RidesController(IRideService rideService, AppDbContext context, ILogger<RidesController> logger)
        {
            _rideService = rideService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var identityUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _rideService.GetPersonByIdentityUserIdAsync(identityUserId);

            if (user == null)
                return Unauthorized();

            // Automatically mark past scheduled rides as completed
            var allUserRides = await _rideService.GetRidesForUserAsync(user.PersonID);
            var now = DateTime.Now;

            var ridesToComplete = allUserRides
                .Where(r => r.RideStatus == "Scheduled" && r.Date < now)
                .ToList();

            if (ridesToComplete.Any())
            {
                foreach (var ride in ridesToComplete)
                    ride.RideStatus = "Completed";

                await _rideService.UpdateRidesAsync(ridesToComplete);
            }

            var updatedRides = await _rideService.GetRidesForUserAsync(user.PersonID);

            // Pass products for the dropdown
            ViewBag.Products = await _context.Products.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();

            return View(updatedRides);
        }

        [HttpPost]
        public async Task<IActionResult> Index(
            [FromForm] string PickupLocation,
            [FromForm] string Destination,
            [FromForm] DateTime Date,
            [FromForm] int? ProductID)
        {
            var now = DateTime.Now;

            if (Date < now)
            {
                TempData["Error"] = "Date and time cannot be in the past.";
                return RedirectToAction("Index");
            }

            var identityUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _rideService.GetPersonByIdentityUserIdAsync(identityUserId);
            if (user == null) return Unauthorized();

            float fare = 0;
            if (ProductID.HasValue)
            {
                var product = await _context.Products.FindAsync(ProductID.Value);
                if (product != null)
                    fare = (float)product.Price;
            }

            var ride = new Ride
            {
                PickupLocation = PickupLocation,
                Destination = Destination,
                Date = Date,
                UserID = user.PersonID,
                RideStatus = "Scheduled",
                Fare = fare,
                DriverID = null,
                ProductID = ProductID
            };

            await _rideService.CreateRideAsync(ride);
            TempData["Success"] = "Ride scheduled successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var identityUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _rideService.GetPersonByIdentityUserIdAsync(identityUserId);
            if (user == null) return Unauthorized();

            await _rideService.CheckoutAsync(user.PersonID);
            TempData["Success"] = "Checkout successful! All scheduled rides have been confirmed and cleared.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DebugAddRide()
        {
            var identityUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _rideService.GetPersonByIdentityUserIdAsync(identityUserId);

            if (user == null)
                return Unauthorized();

            var ride = new Ride
            {
                PickupLocation = "Test From",
                Destination = "Test To",
                Date = DateTime.Now.AddHours(2),
                RideStatus = "Scheduled",
                Fare = 0,
                UserID = user.PersonID
            };

            await _rideService.CreateRideAsync(ride);

            TempData["Success"] = "Hardcoded ride added.";
            return RedirectToAction("Index");
        }
    }
}

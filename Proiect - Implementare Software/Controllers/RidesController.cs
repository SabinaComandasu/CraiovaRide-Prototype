using Microsoft.AspNetCore.Mvc;
using Proiect_Implementare_Software.Models;
using Proiect_Implementare_Software.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Proiect_Implementare_Software.Controllers
{
    public class RidesController : Controller
    {
        private readonly IRideService _rideService;
        private readonly ILogger<RidesController> _logger;

        public RidesController(IRideService rideService, ILogger<RidesController> logger)
        {
            _rideService = rideService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var identityUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _rideService.GetPersonByIdentityUserIdAsync(identityUserId);

            if (user == null)
                return Unauthorized();

            var rides = await _rideService.GetRidesForUserAsync(user.PersonID);
            return View(rides);
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] string PickupLocation, [FromForm] string Destination, [FromForm] DateTime Date)
        {
            var identityUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _rideService.GetPersonByIdentityUserIdAsync(identityUserId);
            if (user == null) return Unauthorized();

            var ride = new Ride
            {
                PickupLocation = PickupLocation,
                Destination = Destination,
                Date = Date,
                UserID = user.PersonID,
                RideStatus = "Scheduled",
                Fare = 0,
                DriverID = null
            };

            await _rideService.CreateRideAsync(ride);
            TempData["Success"] = "Ride scheduled successfully!";
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

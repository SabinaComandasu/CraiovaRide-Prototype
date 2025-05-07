using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proiect_Implementare_Software.Data;
using Proiect_Implementare_Software.Models;
using System.Security.Claims;

namespace Proiect_Implementare_Software.Controllers
{
    
    public class DriverController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DriverController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Curse simulate — DOAR pentru test local
            var mockRides = new List<Ride>
            {
                new Ride { RideID = 101, PickupLocation = "Centrul Vechi", Destination = "Electro", Fare = 23.50f, Date = DateTime.Now },
                new Ride { RideID = 102, PickupLocation = "Gară", Destination = "Mall",  Fare = 30.00f, Date = DateTime.Now.AddMinutes(10) }
            };

            return View(mockRides);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptRide(int id)
        {
            var userId = _userManager.GetUserId(User);
            var person = _context.Persons.FirstOrDefault(p => p.IdentityUserId == userId);
            if (person == null) return NotFound();

            // Simulăm acceptarea – în realitate, ar trebui să fie o cursă reală în DB
            var newRide = new Ride
            {
                PickupLocation = "Simulare Pickup",
                Destination = "Simulare Dropoff",
                Fare = 20.0f,
                Date = DateTime.Now,
                DriverID = person.PersonID,
                RideStatus = "Accepted",
                UserID = 0 // în caz că nu e relevant
            };

            _context.Rides.Add(newRide);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Cursa a fost acceptată!";
            return RedirectToAction("Index");
        }
    }
}

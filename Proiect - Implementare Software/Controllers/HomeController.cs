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
            var identityUser = await _userManager.GetUserAsync(User);

            if (identityUser != null)
            {
                var person = await _context.Persons
                    .FirstOrDefaultAsync(p => p.IdentityUserId == identityUser.Id);

                if (person != null)
                {
                    var now = DateTime.Now;
                    var windowStart = now.AddMinutes(-10);
                    var windowEnd = now.AddMinutes(10);

                    // 🔄 Update expired scheduled rides
                    var expiredRides = await _context.Rides
                        .Where(r => r.UserID == person.PersonID &&
                                    r.RideStatus == "Scheduled" &&
                                    r.Date < now)
                        .ToListAsync();

                    foreach (var ride in expiredRides)
                    {
                        ride.RideStatus = "Finished";
                    }

                    if (expiredRides.Count > 0)
                        await _context.SaveChangesAsync();

                    // 🔔 Upcoming ride notification
                    var upcomingRides = await _context.Rides
    .Where(r =>
        r.UserID == person.PersonID &&
        r.RideStatus == "Scheduled" &&
        r.Date >= windowStart &&
        r.Date <= windowEnd)
    .OrderBy(r => r.Date)
    .ToListAsync();

                    ViewBag.RideNotifications = upcomingRides
                        .Select(r => $"🚕 You have a scheduled ride at {r.Date:HH:mm}. Get ready!")
                        .ToList();


                    ViewBag.FullName = person.FullName;
                    ViewBag.AvatarPath = string.IsNullOrEmpty(person.Avatar) ? "/images/default-avatar.png" : person.Avatar;
                    ViewBag.Rating = person.Rating;
                }
            }

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

    }
}
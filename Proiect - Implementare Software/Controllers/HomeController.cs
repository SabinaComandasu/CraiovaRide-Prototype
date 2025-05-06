using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proiect_Implementare_Software.Models;
using Proiect_Implementare_Software.Data;
using Proiect_Implementare_Software.Services;
using System.Diagnostics;

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

            if (user != null)
            {
                var person = _context.Persons.FirstOrDefault(p => p.IdentityUserId == user.Id);
                if (person != null)
                {
                    ViewBag.AvatarPath = "/images/" + person.Avatar;
                    ViewBag.FullName = person.FullName;
                    ViewBag.Rating = person.Rating.ToString("0.0") ?? "N/A";
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
    }
}
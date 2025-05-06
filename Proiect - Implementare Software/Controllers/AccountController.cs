using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Proiect_Implementare_Software.Models;
using Proiect_Implementare_Software.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Proiect_Implementare_Software.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        public AccountController(UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /Account/Index
        // GET: /Account/EditInfo
        public IActionResult EditInfo()
        {
            var user = _userManager.GetUserAsync(User).Result; // Get the logged-in user
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if the user is not found
            }

            // Fetch the user's information from the database
            var person = _context.Persons.FirstOrDefault(p => p.IdentityUserId == user.Id);
            if (person == null)
            {
                return RedirectToAction("Error"); // Handle error if Person not found
            }

            // Return the EditInfo view with the user data
            return View(person);
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // Get the logged-in user
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if the user is not found
            }

            // Fetch additional user info from the Person table
            var person = _context.Persons.FirstOrDefault(p => p.IdentityUserId == user.Id);
            if (person == null)
            {
                return RedirectToAction("Error"); // Handle error if Person not found
            }

            // Pass the user data, including avatar, to the view
            ViewBag.AvatarPath = "/images/" + person.Avatar; // Avatar image path stored in 'wwwroot/images/'
            ViewBag.FullName = person.FullName;
            ViewBag.Rating = person.Rating.ToString("0.0") ?? "N/A"; // Provide a default if rating is null

            return View(person); // Pass the person data to the view
        }

        // Other actions like EditInfo(), etc.
    }
}

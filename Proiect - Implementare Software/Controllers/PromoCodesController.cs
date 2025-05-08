using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_Implementare_Software.Data;
using Proiect_Implementare_Software.Models;

namespace Proiect_Implementare_Software.Controllers
{
    [Authorize]
    public class PromoCodesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PromoCodesController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            ViewBag.IsAdmin = isAdmin;

            if (isAdmin)
            {
                var allUsers = await _context.Persons.ToListAsync();
                ViewBag.Users = allUsers;

                var allPromoCodes = await _context.PromoCodes.Include(p => p.Person).ToListAsync();
                return View(allPromoCodes);
            }
            else
            {
                var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
                if (person == null)
                    return NotFound();

                var userPromoCodes = await _context.PromoCodes
                    .Where(p => p.PersonID == person.PersonID)
                    .ToListAsync();

                return View(userPromoCodes);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Add(string code, float discount, int? personId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            int assignedPersonId;

            if (isAdmin)
            {
                if (!personId.HasValue)
                    return BadRequest("Admin must assign a person ID.");

                var assignedPerson = await _context.Persons.FindAsync(personId.Value);
                if (assignedPerson == null)
                    return NotFound("Assigned user not found.");

                assignedPersonId = assignedPerson.PersonID;
            }
            else
            {
                // Force override for regular users
                var currentPerson = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
                if (currentPerson == null) return NotFound("Current user not found.");
                assignedPersonId = currentPerson.PersonID;

                // Ensure that user is not trying to assign to someone else
                if (personId.HasValue && personId != assignedPersonId)
                {
                    return Forbid(); // Prevent spoofing
                }
            }

            var promo = new PromoCode
            {
                Code = code,
                Discount = discount,
                PersonID = assignedPersonId
            };

            _context.PromoCodes.Add(promo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proiect_Implementare_Software.Data;
using Proiect_Implementare_Software.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Proiect_Implementare_Software.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAccountRepository _accountRepository;

        public AccountController(UserManager<IdentityUser> userManager, IAccountRepository accountRepository)
        {
            _userManager = userManager;
            _accountRepository = accountRepository;
        }

        // GET: /Account/Index
        public async Task<IActionResult> Index()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return RedirectToAction("Login", "Account");

            var person = await _accountRepository.GetUserByIdentityUserIdAsync(identityUser.Id);
            if (person == null)
                return RedirectToAction("Error", "Home");

            ViewBag.AvatarPath = "/images/" + (string.IsNullOrEmpty(person.Avatar) ? "default-avatar.png" : person.Avatar);

            ViewBag.FullName = person.FullName;
            ViewBag.Rating = person.Rating.ToString("0.0");

            return View(person);
        }

        // GET: /Account/EditInfo
        [HttpGet]
        public async Task<IActionResult> EditInfo()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            var person = await _accountRepository.GetUserByIdentityUserIdAsync(userId);
            if (person == null) return NotFound();

            return View(person); // <-- aici trimitem modelul complet
        }


        [HttpPost]
        public async Task<IActionResult> EditInfo(Person model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var person = await _accountRepository.GetUserByIdentityUserIdAsync(userId);
            if (person == null) return NotFound();

            // ✅ Actualizăm doar câmpurile relevante
            person.FullName = model.FullName;
            person.PhoneNumber = model.PhoneNumber;

            await _accountRepository.SaveChangesAsync();
            return RedirectToAction("Index");
        }



        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var person = await _accountRepository.GetUserByIdentityUserIdAsync(userId);
            if (person == null) return NotFound();

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                // ✅ Setează doar avatarul fără să atingi alte date
                person.Avatar = fileName;
                await _accountRepository.SaveChangesAsync();
            }

            return RedirectToAction("EditInfo");
        }





    }
}

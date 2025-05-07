using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proiect___Implementare_Software.Services;
using Microsoft.EntityFrameworkCore;
using Proiect_Implementare_Software.Data;
using Proiect_Implementare_Software.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Proiect_Implementare_Software.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            IAccountRepository accountRepository,
            IEmailService emailService)
        {
            _userManager = userManager;
            _accountRepository = accountRepository;
            _emailService = emailService;
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

            return View(person);
        }

        [HttpPost]
        public async Task<IActionResult> EditInfo(Person model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var person = await _accountRepository.GetUserByIdentityUserIdAsync(userId);
            if (person == null) return NotFound();

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

                person.Avatar = fileName;
                await _accountRepository.SaveChangesAsync();
            }

            return RedirectToAction("EditInfo");
        }

        // TRIP SAFETY - GET
        [HttpGet]
        public IActionResult TripSafety()
        {
            return View();
        }
        // GET: /Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ActionName("ChangePassword")]
        public async Task<IActionResult> ChangePasswordPost()
        {
            var currentPassword = Request.Form["currentPassword"];
            var newPassword = Request.Form["newPassword"];
            var confirmPassword = Request.Form["confirmPassword"];

            if (newPassword != confirmPassword)
            {
                ViewBag.StatusMessage = "New password and confirmation do not match.";
                return View();
            }
        // TRIP SAFETY - POST: trimite email
        [HttpPost]
        public async Task<IActionResult> TripSafety(string trustedEmail)
        {
            try
            {
                var subject = "🚨 CraiovaRide Safety Alert";
                var body = "Your friend just started a ride using CraiovaRide. Please stay available in case they need help.";

                await _emailService.SendEmailAsync(trustedEmail, subject, body);
                ViewBag.Success = "Trusted contact has been notified successfully.";
            }
            catch
            {
                ViewBag.Error = "Failed to send alert. Please try again.";
            }

            return View();
        }

        // Optional - POST pentru alt tip de email (ex. confirmare)
        [HttpPost]
        public async Task<IActionResult> SendTrustedContactEmail([FromBody] EmailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            var person = await _accountRepository.GetUserByIdentityUserIdAsync(user.Id);
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string subject = "CraiovaRide - Trusted Contact Alert";
            string message = $"{person.FullName} just started a ride using CraiovaRide. Stay available in case of emergency.";

            await _emailService.SendEmailAsync(dto.Email, subject, message);
            return Ok();
        }

        public class EmailDto
        {
            public string Email { get; set; }
        }
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                ViewBag.StatusMessage = "Password changed successfully.";
                return View();
            }

            ViewBag.StatusMessage = string.Join(" ", result.Errors.Select(e => e.Description));
            return View();
        }


        // GET: /Account/Delete
        [HttpGet]
        public IActionResult Delete()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Identity/Account/Login");

            var person = await _accountRepository.GetUserByIdentityUserIdAsync(user.Id);
            if (person != null)
            {
                _accountRepository.Delete(person);
                await _accountRepository.SaveChangesAsync();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ViewBag.StatusMessage = "Error deleting account.";
                return View("Delete");
            }

            await HttpContext.SignOutAsync();
            return Redirect("/Identity/Account/Login"); // ✅ Redirects to login page
        }


    }

}

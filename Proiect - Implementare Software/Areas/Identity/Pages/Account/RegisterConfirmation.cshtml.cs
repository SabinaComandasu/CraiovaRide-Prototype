using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Proiect___Implementare_Software.Services;  // Assuming you are using your custom EmailService

namespace Proiect___Implementare_Software.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;  // Custom Email service
        private readonly ILogger<RegisterConfirmationModel> _logger;

        public RegisterConfirmationModel(UserManager<IdentityUser> userManager, IEmailService emailService, ILogger<RegisterConfirmationModel> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public string Email { get; set; }
        public bool DisplayConfirmAccountLink { get; set; }
        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }
            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;
            DisplayConfirmAccountLink = true;

            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                EmailConfirmationUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                // Send the confirmation email
                var message = $@"
                    <p>Welcome!</p>
                    <p>You have successfully registered. Please confirm your account by clicking the link below:</p>
                    <p><a href='{HtmlEncoder.Default.Encode(EmailConfirmationUrl)}'>Confirm your email</a></p>
                    <p>If you did not register for this account, please ignore this email.</p>
                    <p>Thank you,<br/>The CraiovaRide Team</p>
                ";

                try
                {
                    // Send the confirmation email
                    await _emailService.SendEmailAsync(Email, "Confirm Your Registration", message);
                }
                catch (Exception ex)
                {
                    // Log any error that occurs while sending the email
                    _logger.LogError(ex, "Error sending confirmation email to {Email}", Email);
                    ModelState.AddModelError(string.Empty, $"An error occurred while sending the email: {ex.Message}");
                    return Page();
                }
            }

            return Page();
        }
    }
}

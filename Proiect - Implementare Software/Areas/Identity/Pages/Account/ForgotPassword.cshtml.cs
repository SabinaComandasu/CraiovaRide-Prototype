using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Proiect___Implementare_Software.Services;  // Assuming you are using your custom EmailService

namespace Proiect___Implementare_Software.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordModel> _logger;  // Add this field for logging

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailService emailService, ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;  // This works ONLY if logger is passed into the constructor parameters
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Generate the password reset token
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                // Generate the callback URL for resetting the password
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // Create the email message
                var message = $@"
<p>Bună,</p>
<p>Ai solicitat resetarea parolei pentru contul tău. Poți reseta parola apăsând pe linkul de mai jos:</p>
<p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Resetează parola</a></p>
<p>Dacă nu tu ai solicitat această acțiune, îți recomandăm să ignori acest email. Dacă observi activități suspecte în contul tău, te rugăm să contactezi imediat un administrator sau echipa de suport.</p>
<p>Îți mulțumim,<br/>Echipa CraiovaRide</p>
";

                try
                {
                    // Send the reset password email using the custom EmailService
                    await _emailService.SendEmailAsync(Input.Email, "Reset Password", message);
                }
                catch (Exception ex)
                {
                    // Log the error using your logging framework
                    _logger.LogError(ex, "Error sending reset password email to {Email}", Input.Email);

                    // Optionally, add the exception message to the model state for debugging
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                    return Page();
                }

                // Redirect to the confirmation page after sending the email
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            // If model state is invalid, return the same page with validation messages
            return Page();
        }

    }
}

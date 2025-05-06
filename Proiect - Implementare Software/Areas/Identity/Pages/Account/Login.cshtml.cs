using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private static Dictionary<string, (int failedAttempts, DateTime? lockoutEnd)> _failedLoginAttempts = new();

    public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    // BindProperty and other properties...
    [BindProperty]
    public InputModel Input { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }
    public string ReturnUrl { get; set; }
    [TempData]
    public string ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    // Add rate-limiting logic in the OnPostAsync method
    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            var userEmail = Input.Email.ToLower();

            if (_failedLoginAttempts.ContainsKey(userEmail) && _failedLoginAttempts[userEmail].failedAttempts >= 5)
            {
                var lockoutEnd = _failedLoginAttempts[userEmail].lockoutEnd;
                if (lockoutEnd.HasValue && lockoutEnd.Value > DateTime.UtcNow)
                {
                    var remainingLockoutTime = lockoutEnd.Value - DateTime.UtcNow;
                    var remainingMinutes = (int)remainingLockoutTime.TotalMinutes;
                    var remainingSeconds = (int)remainingLockoutTime.TotalSeconds % 60;

                    ModelState.AddModelError(string.Empty, $"You have exceeded the maximum login attempts. Please try again in {remainingMinutes} minutes and {remainingSeconds} seconds.");
                    var errors = ModelState
                        .Where(ms => ms.Value.Errors.Count > 0)
                        .SelectMany(ms => ms.Value.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return new JsonResult(new { success = false, errors });
                }
                else
                {
                    _failedLoginAttempts[userEmail] = (0, null);
                }
            }

            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (_failedLoginAttempts.ContainsKey(userEmail))
                {
                    _failedLoginAttempts.Remove(userEmail);
                }
                return new JsonResult(new { success = true, redirectUrl = returnUrl });
            }
            else
            {
                if (!_failedLoginAttempts.ContainsKey(userEmail))
                {
                    _failedLoginAttempts[userEmail] = (0, null);
                }

                var (failedAttempts, _) = _failedLoginAttempts[userEmail];
                _failedLoginAttempts[userEmail] = (failedAttempts + 1, null);

                if (_failedLoginAttempts[userEmail].failedAttempts >= 5)
                {
                    _failedLoginAttempts[userEmail] = (_failedLoginAttempts[userEmail].failedAttempts, DateTime.UtcNow.AddMinutes(5));
                    _logger.LogWarning($"User {userEmail} has exceeded maximum login attempts. Locking out for 5 minutes.");
                }

                ModelState.AddModelError(string.Empty, "Incorrect username or password. Please enter your correct credentials.");

                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Count > 0)
                    .SelectMany(ms => ms.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return new JsonResult(new { success = false, errors });
            }
        }

        var errorMessages = ModelState
            .Where(ms => ms.Value.Errors.Count > 0)
            .SelectMany(ms => ms.Value.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return new JsonResult(new { success = false, errors = errorMessages });
    }



}

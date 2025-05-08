using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;
using Proiect_Implementare_Software.Data;
using Microsoft.EntityFrameworkCore;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private static Dictionary<string, (int failedAttempts, DateTime? lockoutEnd)> _failedLoginAttempts = new();
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public LoginModel(SignInManager<IdentityUser> signInManager,
                      UserManager<IdentityUser> userManager,
                      ILogger<LoginModel> logger,
                      AppDbContext context) // Inject DbContext
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _context = context;
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

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return new JsonResult(new { success = false, errors });
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            return new JsonResult(new
            {
                success = false,
                errors = new[] { "This account does not exist. Please use the 'Register as new user' button down below." }
            });
        }

        var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
        if (person != null && person.Rating < 2.0f)
        {
            return new JsonResult(new
            {
                success = false,
                errors = new[] { "Your rating is too low to access CraiovaRide. Please contact support." }
            });
        }

        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");
            return new JsonResult(new { success = true, redirectUrl = returnUrl });
        }

        if (result.IsLockedOut)
        {
            return new JsonResult(new
            {
                success = false,
                errors = new[] { "Your account is locked out." }
            });
        }

        if (result.RequiresTwoFactor)
        {
            return new JsonResult(new
            {
                success = false,
                errors = new[] { "Two-factor authentication required." }
            });
        }

        return new JsonResult(new
        {
            success = false,
            errors = new[] { "Incorrect username or password. Please enter correct credentials.If you forgot your password, please use the 'Forgot passowrd' button down below "}
        });
    }





}

using Garage.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(
            string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return Page();

            // Find the user by EMAIL
            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return Page();
            }

            // Now check the password for that user
            var result = await _signInManager.PasswordSignInAsync(
                user,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "User {Email} logged in.",
                    Input.Email);

                if (!string.IsNullOrEmpty(ReturnUrl) &&
                    Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }

                return RedirectToAction("Index", "Vehicles");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account is locked.");

                return Page();
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Login is not allowed. Please confirm your email.");

                return Page();
            }

            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

            return Page();
        }
    }
}
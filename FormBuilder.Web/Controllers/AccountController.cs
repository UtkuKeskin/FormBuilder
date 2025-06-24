using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FormBuilder.Core.Entities;
using FormBuilder.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace FormBuilder.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (!ModelState.IsValid)
                return View(model);

            var result = await CreateUserAsync(model);
            
            if (result.Succeeded)
                return await SignInAndRedirect(model.Email, returnUrl);

            AddErrors(result);
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (!ModelState.IsValid)
                return View(model);

            var result = await SignInAsync(model);
            
            if (result.Succeeded)
            {
                await UpdateLastLoginAsync(model.Email);
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // Helper Methods 
        private async Task<IdentityResult> CreateUserAsync(RegisterViewModel model)
        {
            var user = new User 
            { 
                UserName = model.Email, 
                Email = model.Email,
                CreatedAt = DateTime.UtcNow,
                Theme = "light",
                Language = "en"
            };
            
            return await _userManager.CreateAsync(user, model.Password);
        }

        private async Task<IActionResult> SignInAndRedirect(string email, string? returnUrl)
        {
            var user = await _userManager.FindByEmailAsync(email);
            await _signInManager.SignInAsync(user!, isPersistent: false);
            _logger.LogInformation("User created a new account.");
            return RedirectToLocal(returnUrl);
        }

        private async Task<Microsoft.AspNetCore.Identity.SignInResult> SignInAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: false);
        }

        private async Task UpdateLastLoginAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Check if anonymous access is allowed
        public static bool IsAnonymousAllowed(string? area, string controller, string action)
        {
            var allowedActions = new[]
            {
                "Home/Index",
                "Home/Privacy",
                "Template/Index",
                "Template/Details",
                "Search/Results"
            };

            var currentAction = $"{controller}/{action}";
            return allowedActions.Contains(currentAction);
        }
    }
}
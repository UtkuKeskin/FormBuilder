using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FormBuilder.Core.Entities;
using FormBuilder.Web.Extensions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;

namespace FormBuilder.Web.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<SettingsController> _logger;
        
        public SettingsController(UserManager<User> userManager, ILogger<SettingsController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateTheme(string theme)
        {
            if (theme != "light" && theme != "dark")
                return BadRequest();
                
            SetCookie("theme", theme);
            
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    user.Theme = theme;
                    await _userManager.UpdateAsync(user);
                }
            }
            
            return Ok();
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateLanguage(string language)
        {
            if (language != "en" && language != "ru")
                return BadRequest();
                
            SetCookie("language", language);
            
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    user.Language = language;
                    await _userManager.UpdateAsync(user);
                }
            }
            
            return Ok();
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { 
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true
                }
            );

            return LocalRedirect(returnUrl ?? "~/");
        }
        
        private void SetCookie(string key, string value)
        {
            Response.Cookies.Append(key, value, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            });
        }
    }
}
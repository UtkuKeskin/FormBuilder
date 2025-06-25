using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FormBuilder.Core.Entities;
using FormBuilder.Web.Extensions;

namespace FormBuilder.Web.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<User> _userManager;
        
        public SettingsController(UserManager<User> userManager)
        {
            _userManager = userManager;
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
            if (language != "en" && language != "tr") // Add your second language
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
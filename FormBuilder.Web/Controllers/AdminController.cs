using Microsoft.AspNetCore.Mvc;
using FormBuilder.Web.Filters;

namespace FormBuilder.Web.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        
        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }
        
        public IActionResult Index()
        {
            _logger.LogInformation("Admin dashboard accessed by {User}", User.Identity?.Name);
            return View();
        }
    }
}
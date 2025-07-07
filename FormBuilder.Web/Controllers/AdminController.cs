using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FormBuilder.Core.Entities;
using FormBuilder.Web.Filters;
using FormBuilder.Web.ViewModels.Admin;
using FormBuilder.Core.Interfaces;

namespace FormBuilder.Web.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITemplateService _templateService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<User> userManager,
            IUnitOfWork unitOfWork,
            ITemplateService templateService,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _templateService = templateService;
            _logger = logger;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var model = await GetDashboardDataAsync();
            return View(model);
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users(int page = 1, string search = "")
        {
            var model = await GetUsersAsync(page, search);
            return View(model);
        }

        // POST: /Admin/ToggleUserBlock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserBlock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.LockoutEnabled = !user.LockoutEnabled;
            if (user.LockoutEnabled)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            }
            else
            {
                user.LockoutEnd = null;
            }

            await _userManager.UpdateAsync(user);
            _logger.LogInformation("User {UserId} block status changed by admin", userId);

            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/ToggleAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            
            // MENTOR NOTE: Admin CAN remove own admin access
            if (user.IsAdmin)
            {
                user.IsAdmin = false;
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                
                if (userId == currentUserId)
                {
                    _logger.LogWarning("Admin {AdminId} removed their own admin access", userId);
                    // Force re-login to update claims
                    return RedirectToAction("Logout", "Account");
                }
            }
            else
            {
                user.IsAdmin = true;
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account";
                return RedirectToAction(nameof(Users));
            }

            await _userManager.DeleteAsync(user);
            _logger.LogInformation("User {UserId} deleted by admin", userId);

            return RedirectToAction(nameof(Users));
        }

        // GET: /Admin/Templates
        public async Task<IActionResult> Templates(int page = 1, string search = "")
        {
            var model = await GetTemplatesAsync(page, search);
            return View(model);
        }

        // GET: /Admin/EditTemplate/{id}
        public async Task<IActionResult> EditTemplate(int id)
        {
            // MENTOR NOTE: Admin sees all as owner
            return RedirectToAction("Edit", "Template", new { id });
        }

        // Private helper methods - SMALL METHODS
        private async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalTemplates = await _unitOfWork.Templates.GetAll().CountAsync();
            var totalForms = await _unitOfWork.Forms.GetAll().CountAsync();
            var totalAdmins = await _userManager.GetUsersInRoleAsync("Admin");

            // Recent activity
            var recentTemplates = await _unitOfWork.Templates
                .GetAll()
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Include(t => t.User)
                .ToListAsync();

            var recentForms = await _unitOfWork.Forms
                .GetAll()
                .OrderByDescending(f => f.FilledAt)
                .Take(5)
                .Include(f => f.User)
                .Include(f => f.Template)
                .ToListAsync();

            return new DashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalTemplates = totalTemplates,
                TotalForms = totalForms,
                TotalAdmins = totalAdmins.Count,
                RecentTemplates = recentTemplates,
                RecentForms = recentForms,
                DailyStats = await GetDailyStatsAsync()
            };
        }

        private async Task<List<DailyStatViewModel>> GetDailyStatsAsync()
        {
            var lastWeek = DateTime.UtcNow.AddDays(-7);
            
            var dailyForms = await _unitOfWork.Forms
                .GetAll()
                .Where(f => f.FilledAt >= lastWeek)
                .GroupBy(f => f.FilledAt.Date)
                .Select(g => new DailyStatViewModel
                {
                    Date = g.Key,
                    FormCount = g.Count()
                })
                .ToListAsync();

            return dailyForms;
        }

        private async Task<UsersViewModel> GetUsersAsync(int page, string search)
        {
            const int pageSize = 20;
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => 
                    u.Email.Contains(search) || 
                    u.UserName.Contains(search));
            }

            var totalCount = await query.CountAsync();
            
            var users = await query
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // BATCH QUERY 
            var userIds = users.Select(u => u.Id).ToList();
            
            var templateCounts = await _unitOfWork.Templates
                .GetAll()
                .Where(t => userIds.Contains(t.UserId))
                .GroupBy(t => t.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
            
            var formCounts = await _unitOfWork.Forms
                .GetAll()
                .Where(f => userIds.Contains(f.UserId))
                .GroupBy(f => f.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var userViewModels = users.Select(u => new UserItemViewModel
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                IsAdmin = u.IsAdmin,
                IsLocked = u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.Now,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                TemplateCount = templateCounts.GetValueOrDefault(u.Id, 0),
                FormCount = formCounts.GetValueOrDefault(u.Id, 0)
            }).ToList();

            return new UsersViewModel
            {
                Users = userViewModels,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                SearchTerm = search
            };
        }

        private async Task<AdminTemplatesViewModel> GetTemplatesAsync(int page, string search)
        {
            const int pageSize = 20;
            
            var (templates, totalCount) = await _templateService.GetTemplatesAsync(
                null, page, pageSize, "CreatedAt", "desc", search, null);

            var templateItems = templates.Select(t => new AdminTemplateItemViewModel
            {
                Id = t.Id,
                Title = t.Title,
                AuthorEmail = t.User.Email,
                AuthorId = t.UserId,
                IsPublic = t.IsPublic,
                FormCount = t.Forms.Count,
                LikeCount = t.Likes.Count,
                CreatedAt = t.CreatedAt,
                TopicName = t.Topic.Name
            }).ToList();

            return new AdminTemplatesViewModel
            {
                Templates = templateItems,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                SearchTerm = search
            };
        }
    }
}
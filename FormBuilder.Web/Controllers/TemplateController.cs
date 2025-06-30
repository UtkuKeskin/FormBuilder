using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FormBuilder.Core.Entities;
using FormBuilder.Core.Interfaces;
using FormBuilder.Web.ViewModels.Template;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FormBuilder.Web.Controllers
{
    [Authorize]
    public class TemplateController : Controller
    {
        private readonly ITemplateService _templateService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly ILogger<TemplateController> _logger;

        public TemplateController(
            ITemplateService templateService,
            ICloudinaryService cloudinaryService,
            IMapper mapper,
            ILogger<TemplateController> logger)
        {
            _templateService = templateService;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: /Template
        public async Task<IActionResult> Index(
            int page = 1, 
            string sort = "CreatedAt", 
            string order = "desc",
            string search = null,
            int? topic = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (templates, totalCount) = await _templateService.GetTemplatesAsync(
                userId, page, 10, sort, order, search, topic);

            var viewModels = templates.Select(t => {
                var vm = _mapper.Map<TemplateViewModel>(t);
                vm.CanEdit = t.UserId == userId || User.IsInRole("Admin");
                vm.CanDelete = vm.CanEdit;
                vm.IsLikedByCurrentUser = t.Likes.Any(l => l.UserId == userId);
                return vm;
            }).ToList();

            var model = new TemplateListViewModel
            {
                Templates = viewModels,
                CurrentPage = page,
                PageSize = 10,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / 10.0),
                SortBy = sort,
                SortOrder = order,
                SearchTerm = search,
                TopicFilter = topic
            };

            ViewBag.Topics = await _templateService.GetTopicsAsync();
            return View(model);
        }

        // GET: /Template/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Topics = await _templateService.GetTopicsAsync();
            return View(new CreateTemplateViewModel());
        }

        // POST: /Template/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTemplateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Topics = await _templateService.GetTopicsAsync();
                return View(model);
            }

            try
            {
                var template = _mapper.Map<Template>(model);
                template.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                template.CreatedAt = DateTime.UtcNow;
                template.UpdatedAt = DateTime.UtcNow;

                // Handle image upload
                if (model.ImageFile != null)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(
                        model.ImageFile.OpenReadStream(), 
                        model.ImageFile.FileName);
                    template.ImageUrl = uploadResult;
                }

                await _templateService.CreateTemplateAsync(
                    template, model.Tags, model.AllowedUserIds);

                TempData["Success"] = "Template created successfully!";
                return RedirectToAction(nameof(Details), new { id = template.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template");
                ModelState.AddModelError("", "An error occurred while creating the template.");
                ViewBag.Topics = await _templateService.GetTopicsAsync();
                return View(model);
            }
        }

        // GET: /Template/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!await _templateService.CanUserEditTemplateAsync(id, userId, isAdmin))
                return Forbid();

            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound();

            var model = _mapper.Map<EditTemplateViewModel>(template);
            ViewBag.Topics = await _templateService.GetTopicsAsync();
            return View(model);
        }

        // POST: /Template/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTemplateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Topics = await _templateService.GetTopicsAsync();
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!await _templateService.CanUserEditTemplateAsync(model.Id, userId, isAdmin))
                return Forbid();

            try
            {
                var template = await _templateService.GetTemplateByIdAsync(model.Id);
                if (template == null || template.Version != model.Version)
                {
                    ModelState.AddModelError("", "The template has been modified by another user.");
                    ViewBag.Topics = await _templateService.GetTopicsAsync();
                    return View(model);
                }

                _mapper.Map(model, template);
                template.UpdatedAt = DateTime.UtcNow;
                template.Version++;

                // Handle image upload
                if (model.ImageFile != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(template.ImageUrl))
                    {
                        await _cloudinaryService.DeleteImageAsync(template.ImageUrl);
                    }

                    var uploadResult = await _cloudinaryService.UploadImageAsync(
                        model.ImageFile.OpenReadStream(), 
                        model.ImageFile.FileName);
                    template.ImageUrl = uploadResult;
                }

                await _templateService.UpdateTemplateAsync(
                    template, model.Tags, model.AllowedUserIds);

                TempData["Success"] = "Template updated successfully!";
                return RedirectToAction(nameof(Details), new { id = template.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template");
                ModelState.AddModelError("", "An error occurred while updating the template.");
                ViewBag.Topics = await _templateService.GetTopicsAsync();
                return View(model);
            }
        }

        // GET: /Template/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (!template.IsPublic && !User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });

            if (!template.IsPublic && !await _templateService.CanUserAccessTemplateAsync(id, userId))
                return Forbid();

            var model = _mapper.Map<TemplateViewModel>(template);
            model.CanEdit = await _templateService.CanUserEditTemplateAsync(id, userId, User.IsInRole("Admin"));
            model.CanDelete = model.CanEdit;
            model.IsLikedByCurrentUser = template.Likes.Any(l => l.UserId == userId);

            return View(model);
        }

        // POST: /Template/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var result = await _templateService.DeleteTemplateAsync(id, userId, isAdmin);
            
            if (result)
            {
                TempData["Success"] = "Template deleted successfully!";
                return RedirectToAction(nameof(Index));
            }

            return Forbid();
        }

        // GET: /Template/MyTemplates
        public async Task<IActionResult> MyTemplates(
            int page = 1, 
            string sort = "CreatedAt", 
            string order = "desc")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (templates, totalCount) = await _templateService.GetTemplatesAsync(
                userId, page, 10, sort, order);

            var viewModels = templates.Select(t => {
                var vm = _mapper.Map<TemplateViewModel>(t);
                vm.CanEdit = true;
                vm.CanDelete = true;
                return vm;
            }).ToList();

            var model = new TemplateListViewModel
            {
                Templates = viewModels,
                CurrentPage = page,
                PageSize = 10,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / 10.0),
                SortBy = sort,
                SortOrder = order
            };

            return View(model);
        }
    }
}
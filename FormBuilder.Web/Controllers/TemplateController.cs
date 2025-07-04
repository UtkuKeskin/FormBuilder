using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FormBuilder.Core.Entities;
using FormBuilder.Core.Interfaces;
using FormBuilder.Web.ViewModels.Template;
using FormBuilder.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FormBuilder.Web.Controllers
{
    public class TemplateController : Controller
    {
        private readonly ITemplateService _templateService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly ILogger<TemplateController> _logger;
        
        // Constants
        private const int DefaultPageSize = 10;

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
        [AllowAnonymous]
        public async Task<IActionResult> Index(
            int page = 1, 
            string sort = "CreatedAt", 
            string order = "desc",
            string search = null,
            int? topic = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAuthenticated = User.Identity.IsAuthenticated;
            
            var (templates, totalCount) = await _templateService.GetTemplatesAsync(
                null,
                page, DefaultPageSize, sort, order, search, topic);

            var viewModels = MapTemplateViewModels(templates, userId, isAuthenticated);

            var model = new TemplateListViewModel
            {
                Templates = viewModels,
                CurrentPage = page,
                PageSize = DefaultPageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)DefaultPageSize),
                SortBy = sort,
                SortOrder = order,
                SearchTerm = search,
                TopicFilter = topic
            };

            await LoadTopicsAsync();
            ViewBag.IsAuthenticated = isAuthenticated;
            return View(model);
        }

        // GET: /Template/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            await LoadTopicsAsync();
            return View(new CreateTemplateViewModel());
        }

        // POST: /Template/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTemplateViewModel model)
        {
            NormalizeModel(model);
            
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await LoadTopicsAsync();
                return View(model);
            }

            try
            {
                var template = await CreateTemplateAsync(model);
                
                TempData["Success"] = "Template created successfully!";
                return RedirectToAction(nameof(MyTemplates));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template for user {UserId}", 
                    User.FindFirstValue(ClaimTypes.NameIdentifier));
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                await LoadTopicsAsync();
                return View(model);
            }
        }

        // GET: /Template/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(AppConstants.AdminRole);

            if (!await _templateService.CanUserEditTemplateAsync(id, userId, isAdmin))
                return Forbid();

            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound();

            var model = _mapper.Map<EditTemplateViewModel>(template);
            await LoadTopicsAsync();
            return View(model);
        }

        // POST: /Template/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTemplateViewModel model)
        {
            ModelState.Remove("AllowedUserIds");
            
            if (!ModelState.IsValid)
            {
                await LoadTopicsAsync();
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(AppConstants.AdminRole);

            if (!await _templateService.CanUserEditTemplateAsync(model.Id, userId, isAdmin))
                return Forbid();

            try
            {
                var template = await _templateService.GetTemplateByIdAsync(model.Id);
                if (template == null)
                    return NotFound();
                
                // Optimistic locking control
                if (template.Version != model.Version)
                {
                    ModelState.AddModelError(string.Empty, 
                        "The template has been modified by another user. Please refresh and try again.");
                    
                    model = _mapper.Map<EditTemplateViewModel>(template);
                    await LoadTopicsAsync();
                    return View(model);
                }

                await UpdateTemplateAsync(template, model);

                TempData["Success"] = "Template updated successfully!";
                return RedirectToAction(nameof(MyTemplates));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template {TemplateId} for user {UserId}", 
                    model.Id, userId);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the template.");
                await LoadTopicsAsync();
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
            var isAdmin = User.IsInRole(AppConstants.AdminRole);
            var isAuthenticated = User.Identity.IsAuthenticated;
            
            // View permission check
            if (!template.IsPublic && !isAuthenticated)
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });

            if (!template.IsPublic && !await _templateService.CanUserAccessTemplateAsync(id, userId))
                return Forbid();

            var model = await MapTemplateDetailsAsync(template, userId, isAdmin, isAuthenticated);
            
            return View(model);
        }

        // POST: /Template/Delete/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(AppConstants.AdminRole);

            var result = await _templateService.DeleteTemplateAsync(id, userId, isAdmin);
            
            if (result)
            {
                TempData["Success"] = "Template deleted successfully!";
                return RedirectToAction(nameof(Index));
            }

            return Forbid();
        }

        // GET: /Template/MyTemplates
        [Authorize]
        public async Task<IActionResult> MyTemplates(
            int page = 1, 
            string sort = "CreatedAt", 
            string order = "desc")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (templates, totalCount) = await _templateService.GetTemplatesAsync(
                userId, page, DefaultPageSize, sort, order);

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
                PageSize = DefaultPageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)DefaultPageSize),
                SortBy = sort,
                SortOrder = order
            };
            
            await LoadTopicsAsync();
            return View(model);
        }

        #region Private Helper Methods

        private async Task LoadTopicsAsync()
        {
            ViewBag.Topics = await _templateService.GetTopicsAsync();
        }

        private void NormalizeModel(CreateTemplateViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Tags)) 
                model.Tags = null;
            
            model.AllowedUserIds ??= new List<int>();
        }

        private void LogModelStateErrors()
        {
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                foreach (var error in state.Errors)
                {
                    _logger.LogError("Validation error - Field: {Field}, Error: {Error}", 
                        key, error.ErrorMessage);
                }
            }
        }

        private List<TemplateViewModel> MapTemplateViewModels(
            IEnumerable<Template> templates, 
            string userId, 
            bool isAuthenticated)
        {
            return templates.Select(t => {
                var vm = _mapper.Map<TemplateViewModel>(t);
                vm.CanEdit = isAuthenticated && IsOwnerOrAdmin(t.UserId, userId);
                vm.CanDelete = vm.CanEdit;
                vm.IsLikedByCurrentUser = isAuthenticated && t.Likes.Any(l => l.UserId == userId);
                return vm;
            }).ToList();
        }

        private bool IsOwnerOrAdmin(string templateUserId, string currentUserId)
        {
            return templateUserId == currentUserId || User.IsInRole(AppConstants.AdminRole);
        }

        private async Task<Template> CreateTemplateAsync(CreateTemplateViewModel model)
        {
            var template = _mapper.Map<Template>(model);
            template.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;
            
            // Handle image upload
            if (model.ImageFile != null)
            {
                template.ImageUrl = await UploadImageAsync(model.ImageFile);
            }
            else
            {
                template.ImageUrl = string.Empty;
            }
            
            SetEmptyStringsForNullQuestions(template);

            await _templateService.CreateTemplateAsync(
                template, 
                model.Tags ?? string.Empty, 
                model.AllowedUserIds ?? new List<int>());

            return template;
        }

        private async Task UpdateTemplateAsync(Template template, EditTemplateViewModel model)
        {
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

                template.ImageUrl = await UploadImageAsync(model.ImageFile);
            }

            SetEmptyStringsForNullQuestions(template);

            await _templateService.UpdateTemplateAsync(
                template, 
                model.Tags ?? string.Empty, 
                model.AllowedUserIds ?? new List<int>());
        }

        private async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            try
            {
                using (var stream = imageFile.OpenReadStream())
                {
                    return await _cloudinaryService.UploadImageAsync(
                        stream, 
                        imageFile.FileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image upload failed");
                throw new InvalidOperationException("Image upload failed. Please try again.");
            }
        }

        private async Task<TemplateViewModel> MapTemplateDetailsAsync(
            Template template, 
            string userId, 
            bool isAdmin, 
            bool isAuthenticated)
        {
            var model = _mapper.Map<TemplateViewModel>(template);
            
            model.CanEdit = isAuthenticated && 
                await _templateService.CanUserEditTemplateAsync(template.Id, userId, isAdmin);
            model.CanDelete = model.CanEdit;
            model.CanAccess = template.IsPublic || 
                (isAuthenticated && (await _templateService.CanUserAccessTemplateAsync(template.Id, userId) || isAdmin));
            model.IsLikedByCurrentUser = isAuthenticated && 
                template.Likes.Any(l => l.UserId == userId);

            return model;
        }

        private void SetEmptyStringsForNullQuestions(Template template)
        {
            SetEmptyStringsForQuestionType(template, "String", 4);
            SetEmptyStringsForQuestionType(template, "Text", 4);
            SetEmptyStringsForQuestionType(template, "Int", 4);
            SetEmptyStringsForQuestionType(template, "Checkbox", 4);
        }

        private void SetEmptyStringsForQuestionType(Template template, string type, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                var questionProp = template.GetType().GetProperty($"Custom{type}{i}Question");
                var descProp = template.GetType().GetProperty($"Custom{type}{i}Description");
                
                if (questionProp?.GetValue(template) == null)
                    questionProp?.SetValue(template, string.Empty);
                
                if (descProp?.GetValue(template) == null)
                    descProp?.SetValue(template, string.Empty);
            }
        }

        #endregion
    }
}
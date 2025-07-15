using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FormBuilder.Core.Entities;
using FormBuilder.Core.Interfaces;
using FormBuilder.Web.ViewModels;
using FormBuilder.Web.ViewModels.Salesforce;
using AutoMapper;
using System.Security.Claims;

namespace FormBuilder.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ISalesforceService _salesforceService;
        private readonly IApiKeyService _apiKeyService; // ← NEW
        private readonly IMapper _mapper;
        private readonly ILogger<ProfileController> _logger;

        // ← UPDATED CONSTRUCTOR
        public ProfileController(
            UserManager<User> userManager,
            ISalesforceService salesforceService,
            IApiKeyService apiKeyService, // ← NEW
            IMapper mapper,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _salesforceService = salesforceService;
            _apiKeyService = apiKeyService; // ← NEW
            _mapper = mapper;
            _logger = logger;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<UserViewModel>(user);
            return View(model);
        }

        // GET: /Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                Email = user.Email,
                Theme = user.Theme,
                Language = user.Language
            };

            return View(model);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.Theme = model.Theme;
            user.Language = model.Language;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User updated their profile");
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("User changed their password");
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Edit");
        }

        // GET: /Profile/ApiKey
        [HttpGet]
        public async Task<IActionResult> ApiKey()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ApiKeyViewModel
            {
                ApiKey = user.ApiKey,
                GeneratedAt = user.ApiKeyGeneratedAt,
                LastUsedAt = user.ApiKeyLastUsedAt,
                IsEnabled = user.ApiKeyEnabled
            };

            return View(model);
        }

        // POST: /Profile/GenerateApiKey
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateApiKey()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Rate limiting check (simple implementation)
            var user = await _userManager.FindByIdAsync(userId);
            if (user.ApiKeyGeneratedAt.HasValue && 
                user.ApiKeyGeneratedAt.Value > DateTime.UtcNow.AddMinutes(-5))
            {
                TempData["Error"] = "Please wait 5 minutes before generating a new API key.";
                return RedirectToAction(nameof(ApiKey));
            }

            try
            {
                var apiKey = await _apiKeyService.GenerateApiKeyAsync(userId);
                TempData["Success"] = "API key generated successfully!";
                TempData["NewApiKey"] = apiKey; // To show once
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating API key for user {UserId}", userId);
                TempData["Error"] = "Failed to generate API key.";
            }

            return RedirectToAction(nameof(ApiKey));
        }

        // POST: /Profile/RevokeApiKey
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeApiKey()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                await _apiKeyService.RevokeApiKeyAsync(userId);
                TempData["Success"] = "API key revoked successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API key for user {UserId}", userId);
                TempData["Error"] = "Failed to revoke API key.";
            }

            return RedirectToAction(nameof(ApiKey));
        }

        // GET: /Profile/SalesforceIntegration
        [HttpGet]
        public async Task<IActionResult> SalesforceIntegration(string? userId = null)
        {
            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("SalesforceIntegration accessed. CorrelationId: {CorrelationId}", correlationId);

            try
            {
                // Authorization check
                var targetUserId = await GetAuthorizedUserIdAsync(userId);
                if (targetUserId == null)
                {
                    return Forbid();
                }

                var user = await _userManager.FindByIdAsync(targetUserId);
                if (user == null)
                {
                    return NotFound();
                }

                var model = new SalesforceIntegrationViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    IsIntegrated = !string.IsNullOrEmpty(user.SalesforceAccountId),
                    LastSyncDate = user.LastSalesforceSync
                };

                // Pre-fill some fields
                model.FirstName = user.UserName?.Split('@')[0] ?? "";
                model.LastName = "User"; // Default value

                ViewBag.CorrelationId = correlationId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SalesforceIntegration. CorrelationId: {CorrelationId}", correlationId);
                TempData["Error"] = $"An error occurred. Reference: {correlationId}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Profile/CreateSalesforceRecord
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSalesforceRecord(SalesforceIntegrationViewModel model)
        {
            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("CreateSalesforceRecord called. CorrelationId: {CorrelationId}", correlationId);

            if (!ModelState.IsValid)
            {
                ViewBag.CorrelationId = correlationId;
                return View(nameof(SalesforceIntegration), model);
            }

            try
            {
                // Authorization check
                var targetUserId = await GetAuthorizedUserIdAsync(model.UserId);
                if (targetUserId == null || targetUserId != model.UserId)
                {
                    return Forbid();
                }

                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Prepare data
                var accountData = new FormBuilder.Core.Models.Salesforce.SalesforceAccount
                {
                    Name = model.CompanyName,
                    Industry = model.Industry,
                    AnnualRevenue = model.AnnualRevenue,
                    NumberOfEmployees = model.NumberOfEmployees,
                    Phone = model.CompanyPhone,
                    BillingStreet = model.Street,
                    BillingCity = model.City,
                    BillingState = string.IsNullOrWhiteSpace(model.State) ? null : model.State,
                    BillingPostalCode = model.PostalCode,
                    BillingCountry = string.IsNullOrWhiteSpace(model.Country) ? "United States" : model.Country
                };

                var contactData = new FormBuilder.Core.Models.Salesforce.SalesforceContact
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = user.Email,
                    Phone = model.Phone,
                    Title = model.Title,
                    Department = model.Department
                };

                // Check if update or create
                if (!string.IsNullOrEmpty(user.SalesforceAccountId))
                {
                    // UPDATE existing records
                    _logger.LogInformation("Updating existing Salesforce records. AccountId: {AccountId}", user.SalesforceAccountId);
                    
                    var accountUpdateResult = await _salesforceService.UpdateAccountAsync(user.SalesforceAccountId, accountData);
                    if (!accountUpdateResult.success)
                    {
                        var errorMsg = string.Join(", ", accountUpdateResult.errors?.Select(e => e.message) ?? new[] { "Unknown error" });
                        ModelState.AddModelError("", $"Failed to update Salesforce account: {errorMsg}");
                        ViewBag.CorrelationId = correlationId;
                        return View(nameof(SalesforceIntegration), model);
                    }

                    if (!string.IsNullOrEmpty(user.SalesforceContactId))
                    {
                        var contactUpdateResult = await _salesforceService.UpdateContactAsync(user.SalesforceContactId, contactData);
                        if (!contactUpdateResult.success)
                        {
                            var errorMsg = string.Join(", ", contactUpdateResult.errors?.Select(e => e.message) ?? new[] { "Unknown error" });
                            ModelState.AddModelError("", $"Failed to update Salesforce contact: {errorMsg}");
                            ViewBag.CorrelationId = correlationId;
                            return View(nameof(SalesforceIntegration), model);
                        }
                    }

                    TempData["Success"] = "Successfully updated Salesforce records!";
                }
                else
                {
                    // CREATE new records
                    _logger.LogInformation("Creating new Salesforce records");
                    
                    var accountResult = await _salesforceService.CreateAccountAsync(accountData);
                    if (!accountResult.success)
                    {
                        var errorMsg = string.Join(", ", accountResult.errors?.Select(e => e.message) ?? new[] { "Unknown error" });
                        ModelState.AddModelError("", $"Failed to create Salesforce account: {errorMsg}");
                        ViewBag.CorrelationId = correlationId;
                        return View(nameof(SalesforceIntegration), model);
                    }

                    contactData.AccountId = accountResult.id;
                    var contactResult = await _salesforceService.CreateContactAsync(contactData, accountResult.id);
                    if (!contactResult.success)
                    {
                        var errorMsg = string.Join(", ", contactResult.errors?.Select(e => e.message) ?? new[] { "Unknown error" });
                        ModelState.AddModelError("", $"Account created but failed to create contact: {errorMsg}");
                        ViewBag.CorrelationId = correlationId;
                        return View(nameof(SalesforceIntegration), model);
                    }

                    // Update user record
                    user.SalesforceAccountId = accountResult.id;
                    user.SalesforceContactId = contactResult.id;
                    
                    TempData["Success"] = "Successfully exported to Salesforce!";
                }

                // Update sync date
                user.LastSalesforceSync = DateTime.UtcNow;
                user.SalesforceIntegrationEnabled = true;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Failed to update user after Salesforce sync. CorrelationId: {CorrelationId}", correlationId);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Salesforce operation. CorrelationId: {CorrelationId}", correlationId);
                ModelState.AddModelError("", $"An error occurred. Reference: {correlationId}");
                ViewBag.CorrelationId = correlationId;
                return View(nameof(SalesforceIntegration), model);
            }
        }

        // GET: /Profile/SalesforceStatus
        [HttpGet]
        public async Task<IActionResult> SalesforceStatus(string? userId = null)
        {
            try
            {
                var targetUserId = await GetAuthorizedUserIdAsync(userId);
                if (targetUserId == null)
                {
                    return Forbid();
                }

                var user = await _userManager.FindByIdAsync(targetUserId);
                if (user == null)
                {
                    return NotFound();
                }

                var status = new SalesforceStatusViewModel
                {
                    IsIntegrated = !string.IsNullOrEmpty(user.SalesforceAccountId),
                    LastSyncDate = user.LastSalesforceSync,
                    AccountId = user.SalesforceAccountId,
                    ContactId = user.SalesforceContactId
                };

                return Json(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Salesforce status");
                return StatusCode(500);
            }
        }

        // Helper method for authorization
        private async Task<string?> GetAuthorizedUserIdAsync(string? requestedUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // If no specific user requested, use current user
            if (string.IsNullOrEmpty(requestedUserId))
            {
                return currentUserId;
            }

            // Admin can access any profile
            if (isAdmin)
            {
                return requestedUserId;
            }

            if (requestedUserId == currentUserId)
            {
                return currentUserId;
            }

            return null;
        }
    }
}
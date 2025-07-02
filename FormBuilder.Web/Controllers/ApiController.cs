using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FormBuilder.Core.Interfaces;
using System.Security.Claims;

namespace FormBuilder.Web.Controllers
{
    [Authorize]
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly ITemplateService _templateService;
        private readonly ILogger<ApiController> _logger;

        public ApiController(ITemplateService templateService, ILogger<ApiController> logger)
        {
            _templateService = templateService;
            _logger = logger;
        }

        [HttpPost("bulk/delete")]
        public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request)
        {
            if (request?.Items == null || !request.Items.Any())
            {
                return BadRequest("No items selected");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var deletedCount = 0;

            foreach (var itemId in request.Items)
            {
                try
                {
                    if (int.TryParse(itemId, out int templateId))
                    {
                        var result = await _templateService.DeleteTemplateAsync(templateId, userId, isAdmin);
                        if (result) deletedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting template {TemplateId}", itemId);
                }
            }

            return Ok(new { deletedCount, message = $"{deletedCount} templates deleted successfully" });
        }
    }

    public class BulkDeleteRequest
    {
        public string Action { get; set; }
        public List<string> Items { get; set; }
    }
}
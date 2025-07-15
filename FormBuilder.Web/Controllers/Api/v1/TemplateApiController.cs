using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace FormBuilder.Web.Controllers.Api.v1
{
    [ApiController]
    [Route("api/v1/templates")]
    [Authorize(Policy = "ApiKeyPolicy")]
    [Produces("application/json")]
    public class TemplateApiController : ControllerBase
    {
        private readonly ITemplateService _templateService;
        private readonly ILogger<TemplateApiController> _logger;

        public TemplateApiController(
            ITemplateService templateService,
            ILogger<TemplateApiController> logger)
        {
            _templateService = templateService;
            _logger = logger;
        }

        /// <summary>
        /// Get aggregated data for all templates owned by the authenticated user
        /// </summary>
        /// <returns>Aggregated template data including question statistics</returns>
        /// <response code="200">Returns the aggregated template data</response>
        /// <response code="401">If the API key is missing or invalid</response>
        /// <response code="429">If rate limit is exceeded</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpGet("aggregates")]
        [SwaggerOperation(
            Summary = "Get template aggregates",
            Description = "Returns aggregated data for all templates owned by the authenticated user. " +
                         "Includes statistics for numeric questions (average, min, max), " +
                         "top answers for text questions, and percentages for checkbox questions.",
            OperationId = "GetTemplateAggregates",
            Tags = new[] { "Templates" }
        )]
        [ProducesResponseType(typeof(TemplateAggregateResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(429)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAggregates()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("API request for aggregates by user {UserId}", userId);

            try
            {
                var aggregates = await _templateService.GetTemplateAggregatesAsync(userId);
                
                // Add rate limit headers
                Response.Headers.Add("X-Rate-Limit-Limit", "100");
                Response.Headers.Add("X-Rate-Limit-Period", "1h");
                
                return Ok(aggregates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting aggregates for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
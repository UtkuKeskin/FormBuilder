using System;
using System.Threading.Tasks;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using AspNetCoreRateLimit;

namespace FormBuilder.Web.Controllers.Api
{
    [ApiController]
    [Route("api/support")]
    public class SupportController : ControllerBase
    {
        private readonly ISupportTicketService _supportService;
        private readonly ILogger<SupportController> _logger;

        public SupportController(
            ISupportTicketService supportService,
            ILogger<SupportController> logger)
        {
            _supportService = supportService;
            _logger = logger;
        }

        [HttpPost("ticket")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate request
                if (string.IsNullOrWhiteSpace(request.Summary) || 
                    request.Summary.Length < 10 || 
                    request.Summary.Length > 500)
                {
                    return BadRequest(new { error = "Summary must be between 10 and 500 characters" });
                }

                if (request.Priority != "High" && 
                    request.Priority != "Average" && 
                    request.Priority != "Low")
                {
                    return BadRequest(new { error = "Invalid priority value" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                var ticket = await _supportService.CreateTicketAsync(request, userId, HttpContext);
                
                return Ok(new
                {
                    success = true,
                    ticketId = ticket.TicketId,
                    message = "Support ticket created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support ticket");
                return StatusCode(500, new { error = "An error occurred while creating the ticket" });
            }
        }
    }
}
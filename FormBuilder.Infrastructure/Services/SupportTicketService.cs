using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FormBuilder.Core.Entities;

namespace FormBuilder.Infrastructure.Services
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly IDropboxService _dropboxService;
        private readonly UserManager<User> _userManager;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SupportTicketService> _logger;

        public SupportTicketService(
            IDropboxService dropboxService,
            UserManager<User> userManager,
            ITemplateService templateService,
            IConfiguration configuration,
            ILogger<SupportTicketService> logger)
        {
            _dropboxService = dropboxService;
            _userManager = userManager;
            _templateService = templateService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<SupportTicket> CreateTicketAsync(
            CreateTicketRequest request, 
            string userId, 
            HttpContext context)
        {
            var ticket = new SupportTicket
            {
                TicketId = Guid.NewGuid().ToString(),
                Summary = request.Summary,
                Priority = request.Priority,
                PageUrl = request.CurrentUrl,
                CreatedAtUtc = DateTime.UtcNow,
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            };

            // Get user info
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    ticket.ReportedBy = user.UserName;
                    ticket.ReportedByEmail = user.Email;
                }
            }
            else
            {
                ticket.ReportedBy = "Anonymous";
                ticket.ReportedByEmail = "anonymous@formbuilder.com";
            }

            // Get template info if provided
            if (request.TemplateId.HasValue)
            {
                try
                {
                    var template = await _templateService.GetTemplateByIdAsync(request.TemplateId.Value);
                    if (template != null)
                    {
                        ticket.TemplateName = template.Title;
                        ticket.TemplateId = template.Id;
                    }
                }
                catch
                {
                    ticket.TemplateName = "N/A";
                }
            }
            else
            {
                ticket.TemplateName = "N/A";
            }

            // Get admin emails
            ticket.AdminEmails = await GetAdminEmailsAsync();

            // Create folder structure
            var folderPath = $"/SupportTickets/{DateTime.UtcNow:yyyy/MM}";
            await _dropboxService.CreateFolderIfNotExistsAsync(folderPath);

            // Upload to Dropbox
            var fileName = $"/SupportTickets/{DateTime.UtcNow:yyyy/MM}/ticket_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{ticket.TicketId}.json";
            await _dropboxService.UploadJsonAsync(ticket, fileName);

            _logger.LogInformation("Support ticket created: {TicketId}", ticket.TicketId);
            
            return ticket;
        }

        public async Task<List<string>> GetAdminEmailsAsync()
        {
            // Get from configuration first
            var configEmails = _configuration.GetSection("SupportTicket:AdminEmails")
                .Get<List<string>>() ?? new List<string>();

            // Also get users with Admin role
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminEmails = adminUsers.Select(u => u.Email).ToList();

            // Combine and remove duplicates
            return configEmails.Union(adminEmails).Distinct().ToList();
        }
    }
}
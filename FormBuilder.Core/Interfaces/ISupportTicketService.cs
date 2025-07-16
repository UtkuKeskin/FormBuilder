using System.Collections.Generic;
using System.Threading.Tasks;
using FormBuilder.Core.Models;
using Microsoft.AspNetCore.Http;

namespace FormBuilder.Core.Interfaces
{
    public interface ISupportTicketService
    {
        Task<SupportTicket> CreateTicketAsync(CreateTicketRequest request, string userId, HttpContext context);
        Task<List<string>> GetAdminEmailsAsync();
    }
}
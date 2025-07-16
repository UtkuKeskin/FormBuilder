using System;
using System.Collections.Generic;

namespace FormBuilder.Core.Models
{
    public class SupportTicket
    {
        public string TicketId { get; set; }
        public string ReportedBy { get; set; }
        public string ReportedByEmail { get; set; }
        public string TemplateName { get; set; }
        public int? TemplateId { get; set; }
        public string PageUrl { get; set; }
        public string Priority { get; set; }
        public string Summary { get; set; }
        public List<string> AdminEmails { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
    }

    public class CreateTicketRequest
    {
        public string Summary { get; set; }
        public string Priority { get; set; }
        public string CurrentUrl { get; set; }
        public int? TemplateId { get; set; }
    }
}
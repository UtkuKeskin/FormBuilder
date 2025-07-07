using System;
using System.Collections.Generic;

namespace FormBuilder.Web.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalTemplates { get; set; }
        public int TotalForms { get; set; }
        public int TotalAdmins { get; set; }
        public List<FormBuilder.Core.Entities.Template> RecentTemplates { get; set; } = new();
        public List<FormBuilder.Core.Entities.Form> RecentForms { get; set; } = new();
        
        public List<DailyStatViewModel> DailyStats { get; set; } = new();
    }

    public class DailyStatViewModel
    {
        public DateTime Date { get; set; }
        public int FormCount { get; set; }
    }
}
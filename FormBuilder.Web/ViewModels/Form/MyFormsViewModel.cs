using System;
using System.Collections.Generic;

namespace FormBuilder.Web.ViewModels.Form
{
    public class MyFormsViewModel
    {
        public List<FormListItemViewModel> Forms { get; set; } = new();
    }

    public class FormListItemViewModel
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string TemplateTitle { get; set; }
        public DateTime FilledAt { get; set; }
        public Dictionary<string, string> DisplayFields { get; set; } = new();
    }
}
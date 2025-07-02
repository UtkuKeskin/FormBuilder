using System.Collections.Generic;

namespace FormBuilder.Web.ViewModels.Form
{
    public class FillFormViewModel
    {
        public int TemplateId { get; set; }
        public string TemplateTitle { get; set; }
        public string TemplateDescription { get; set; }
        public bool IsAnonymous { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = new();
    }

    public class QuestionViewModel
    {
        public string Type { get; set; }
        public string FieldName { get; set; }
        public string Question { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public int Order { get; set; }
    }
}
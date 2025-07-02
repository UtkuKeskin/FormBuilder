using System.Collections.Generic;

namespace FormBuilder.Web.ViewModels.Form
{
    public class ViewFormViewModel
    {
        public Core.Entities.Form Form { get; set; }
        public Core.Entities.Template Template { get; set; }
        public List<AnswerViewModel> Answers { get; set; } = new();
    }

    public class AnswerViewModel
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Type { get; set; }
    }
}
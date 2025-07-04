using AutoMapper;
using FormBuilder.Core.Entities;
using FormBuilder.Web.ViewModels.Template;
using System.Linq;
using System.Collections.Generic;

namespace FormBuilder.Web.Mappings
{
    public class TemplateMappingProfile : Profile
    {
        public TemplateMappingProfile()
        {
            // Template to TemplateViewModel
            CreateMap<Template, TemplateViewModel>()
                .ForMember(dest => dest.TopicName,
                    opt => opt.MapFrom(src => src.Topic.Name))
                .ForMember(dest => dest.AuthorName,
                    opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.AuthorId,
                    opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FormCount,
                    opt => opt.MapFrom(src => src.Forms.Count))
                .ForMember(dest => dest.LikeCount,
                    opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.Tags,
                    opt => opt.MapFrom(src => src.TemplateTags
                        .Select(tt => tt.Tag.Name).ToList()))
                .ForMember(dest => dest.Questions,
                    opt => opt.MapFrom(src => GetQuestionsFromTemplate(src)))
                .ForMember(dest => dest.Forms,
                    opt => opt.MapFrom(src => src.Forms.Select(f => new ViewModels.Template.FormViewModel
                    {
                        Id = f.Id,
                        UserId = f.UserId,
                        UserName = f.User != null ? f.User.UserName : "Unknown",
                        FilledAt = f.FilledAt,
                        DisplayAnswers = GetDisplayAnswers(f, src)
                    }).ToList()));

            // CreateTemplateViewModel to Template
            CreateMap<CreateTemplateViewModel, Template>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // EditTemplateViewModel to Template
            CreateMap<EditTemplateViewModel, Template>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Template to EditTemplateViewModel
            CreateMap<Template, EditTemplateViewModel>()
                .ForMember(dest => dest.CurrentImageUrl, 
                    opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.Tags, 
                    opt => opt.MapFrom(src => string.Join(", ", 
                        src.TemplateTags.Select(tt => tt.Tag.Name))))
                .ForMember(dest => dest.AllowedUserIds, 
                    opt => opt.MapFrom(src => src.TemplateAccesses
                        .Select(ta => int.Parse(ta.UserId)).ToList()));
        }

        private List<QuestionViewModel> GetQuestionsFromTemplate(Template template)
        {
            var questions = new List<QuestionViewModel>();
            var questionTypes = new[] { "String", "Text", "Int", "Checkbox" };
            
            foreach (var type in questionTypes)
            {
                for (int i = 1; i <= 4; i++)
                {
                    if (IsQuestionActive(template, type, i))
                    {
                        questions.Add(CreateQuestionViewModel(template, type, i, questions.Count + 1));
                    }
                }
            }
            
            return questions;
        }

        private bool IsQuestionActive(Template template, string type, int number)
        {
            var state = GetPropertyValue<bool>(template, $"Custom{type}{number}State");
            var question = GetPropertyValue<string>(template, $"Custom{type}{number}Question");
            return state && !string.IsNullOrEmpty(question);
        }

        private QuestionViewModel CreateQuestionViewModel(Template template, string type, int number, int order)
        {
            return new QuestionViewModel
            {
                Type = type.ToLower() == "int" ? "integer" : type.ToLower(),
                Question = GetPropertyValue<string>(template, $"Custom{type}{number}Question"),
                Description = GetPropertyValue<string>(template, $"Custom{type}{number}Description"),
                ShowInTable = GetPropertyValue<bool>(template, $"Custom{type}{number}ShowInTable"),
                Order = order
            };
        }

        private T GetPropertyValue<T>(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName);
            var value = prop?.GetValue(obj);
            return value is T ? (T)value : default(T);
        }

        //DisplayAnswers Logic
        private Dictionary<string, string> GetDisplayAnswers(Form form, Template template)
        {
            var answers = new Dictionary<string, string>();
            
            // String answers
            for (int i = 1; i <= 4; i++)
            {
                if (GetShowInTable(template, "String", i))
                {
                    var question = GetQuestion(template, "String", i);
                    var answer = GetAnswer(form, "String", i);
                    if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                        answers[question] = answer;
                }
            }
            
            // Text answers
            for (int i = 1; i <= 4; i++)
            {
                if (GetShowInTable(template, "Text", i))
                {
                    var question = GetQuestion(template, "Text", i);
                    var answer = GetAnswer(form, "Text", i);
                    if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                        answers[question] = answer.Length > 50 ? answer.Substring(0, 50) + "..." : answer;
                }
            }
            
            // Integer answers
            for (int i = 1; i <= 4; i++)
            {
                if (GetShowInTable(template, "Int", i))
                {
                    var question = GetQuestion(template, "Int", i);
                    var answer = GetAnswer(form, "Int", i);
                    if (!string.IsNullOrEmpty(question) && answer != null)
                        answers[question] = answer;
                }
            }
            
            // Checkbox answers
            for (int i = 1; i <= 4; i++)
            {
                if (GetShowInTable(template, "Checkbox", i))
                {
                    var question = GetQuestion(template, "Checkbox", i);
                    var answer = GetAnswer(form, "Checkbox", i);
                    if (!string.IsNullOrEmpty(question))
                        answers[question] = answer == "True" ? "Yes" : "No";
                }
            }
            
            return answers;
        }

        //HELPER METHODS
        private bool GetShowInTable(Template template, string type, int number)
        {
            var prop = template.GetType().GetProperty($"Custom{type}{number}ShowInTable");
            return prop?.GetValue(template) as bool? ?? false;
        }

        private string GetQuestion(Template template, string type, int number)
        {
            var prop = template.GetType().GetProperty($"Custom{type}{number}Question");
            return prop?.GetValue(template)?.ToString() ?? "";
        }

        private string GetAnswer(Form form, string type, int number)
        {
            var prop = form.GetType().GetProperty($"Custom{type}{number}Answer");
            return prop?.GetValue(form)?.ToString();
        }
    }
}
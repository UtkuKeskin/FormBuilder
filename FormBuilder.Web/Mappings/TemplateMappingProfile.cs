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
                    opt => opt.MapFrom(src => GetQuestionsFromTemplate(src)));

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
            int order = 1;

            // Define question configurations
            var questionConfigs = new[]
            {
                // String questions
                new { Type = "string", Number = 1, State = template.CustomString1State, 
                      Question = template.CustomString1Question, Description = template.CustomString1Description, 
                      ShowInTable = template.CustomString1ShowInTable },
                new { Type = "string", Number = 2, State = template.CustomString2State, 
                      Question = template.CustomString2Question, Description = template.CustomString2Description, 
                      ShowInTable = template.CustomString2ShowInTable },
                new { Type = "string", Number = 3, State = template.CustomString3State, 
                      Question = template.CustomString3Question, Description = template.CustomString3Description, 
                      ShowInTable = template.CustomString3ShowInTable },
                new { Type = "string", Number = 4, State = template.CustomString4State, 
                      Question = template.CustomString4Question, Description = template.CustomString4Description, 
                      ShowInTable = template.CustomString4ShowInTable },
                
                // Text questions
                new { Type = "text", Number = 1, State = template.CustomText1State, 
                      Question = template.CustomText1Question, Description = template.CustomText1Description, 
                      ShowInTable = template.CustomText1ShowInTable },
                new { Type = "text", Number = 2, State = template.CustomText2State, 
                      Question = template.CustomText2Question, Description = template.CustomText2Description, 
                      ShowInTable = template.CustomText2ShowInTable },
                new { Type = "text", Number = 3, State = template.CustomText3State, 
                      Question = template.CustomText3Question, Description = template.CustomText3Description, 
                      ShowInTable = template.CustomText3ShowInTable },
                new { Type = "text", Number = 4, State = template.CustomText4State, 
                      Question = template.CustomText4Question, Description = template.CustomText4Description, 
                      ShowInTable = template.CustomText4ShowInTable },
                
                // Integer questions
                new { Type = "integer", Number = 1, State = template.CustomInt1State, 
                      Question = template.CustomInt1Question, Description = template.CustomInt1Description, 
                      ShowInTable = template.CustomInt1ShowInTable },
                new { Type = "integer", Number = 2, State = template.CustomInt2State, 
                      Question = template.CustomInt2Question, Description = template.CustomInt2Description, 
                      ShowInTable = template.CustomInt2ShowInTable },
                new { Type = "integer", Number = 3, State = template.CustomInt3State, 
                      Question = template.CustomInt3Question, Description = template.CustomInt3Description, 
                      ShowInTable = template.CustomInt3ShowInTable },
                new { Type = "integer", Number = 4, State = template.CustomInt4State, 
                      Question = template.CustomInt4Question, Description = template.CustomInt4Description, 
                      ShowInTable = template.CustomInt4ShowInTable },
                
                // Checkbox questions
                new { Type = "checkbox", Number = 1, State = template.CustomCheckbox1State, 
                      Question = template.CustomCheckbox1Question, Description = template.CustomCheckbox1Description, 
                      ShowInTable = template.CustomCheckbox1ShowInTable },
                new { Type = "checkbox", Number = 2, State = template.CustomCheckbox2State, 
                      Question = template.CustomCheckbox2Question, Description = template.CustomCheckbox2Description, 
                      ShowInTable = template.CustomCheckbox2ShowInTable },
                new { Type = "checkbox", Number = 3, State = template.CustomCheckbox3State, 
                      Question = template.CustomCheckbox3Question, Description = template.CustomCheckbox3Description, 
                      ShowInTable = template.CustomCheckbox3ShowInTable },
                new { Type = "checkbox", Number = 4, State = template.CustomCheckbox4State, 
                      Question = template.CustomCheckbox4Question, Description = template.CustomCheckbox4Description, 
                      ShowInTable = template.CustomCheckbox4ShowInTable }
            };

            // Filter and map active questions
            return questionConfigs
                .Where(q => q.State && !string.IsNullOrEmpty(q.Question))
                .Select((q, index) => new QuestionViewModel
                {
                    Type = q.Type,
                    Question = q.Question,
                    Description = q.Description,
                    ShowInTable = q.ShowInTable,
                    Order = index + 1
                })
                .ToList();
        }
    }
}
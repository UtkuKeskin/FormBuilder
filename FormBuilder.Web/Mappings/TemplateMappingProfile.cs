using AutoMapper;
using FormBuilder.Core.Entities;
using FormBuilder.Web.ViewModels.Template;
using System.Linq;

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
                        .Select(tt => tt.Tag.Name).ToList()));

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
    }
}
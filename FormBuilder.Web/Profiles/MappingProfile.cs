using AutoMapper;
using FormBuilder.Core.Entities;
using FormBuilder.Web.ViewModels;

namespace FormBuilder.Web.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserViewModel>();
            CreateMap<RegisterViewModel, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
            
            // Template mappings
            CreateMap<Template, TemplateViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.FormCount, opt => opt.MapFrom(src => src.Forms.Count))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.Likes.Count));
            
            CreateMap<CreateTemplateViewModel, Template>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            
            CreateMap<Template, EditTemplateViewModel>();
            CreateMap<EditTemplateViewModel, Template>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            
            // Form mappings
            CreateMap<Form, FormViewModel>()
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.Template.Title));
            
            // Tag mappings
            CreateMap<Tag, TagViewModel>()
                .ForMember(dest => dest.UsageCount, opt => opt.MapFrom(src => src.TemplateTags.Count));
        }
    }
}
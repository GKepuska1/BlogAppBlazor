using AutoMapper;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;

namespace BlogApp.Core.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Blog, BlogDto>().ReverseMap();
            CreateMap<Blog, BlogDtoCreate>().ReverseMap();

            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.IsEdited, opt => opt.MapFrom(src => src.UpdatedAt > src.CreatedAt))
                .ReverseMap();

            CreateMap<Comment, CommentDtoCreate>()
                .ReverseMap();
        }
    }
}

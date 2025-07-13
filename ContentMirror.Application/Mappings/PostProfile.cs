using AutoMapper;
using ContentMirror.Core.Entities;

namespace ContentMirror.Application.Mappings;

public class PostProfile : Profile
{
    public PostProfile()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        CreateMap<NewsEntity, PostEntity>()
            .ForMember(dest => dest.PostTitle, opt => opt.MapFrom(src => src.Preview.Title))
            .ForMember(dest => dest.PostContent, opt => opt.MapFrom(src => src.Article))
            .ForMember(dest => dest.PostExcerpt, opt => opt.MapFrom(src => src.Preview.Description))
            .ForMember(dest => dest.PostSource, opt => opt.MapFrom(src => src.Preview.Url))
            .ForMember(dest => dest.PostPublishDate, opt => opt.MapFrom(src => now))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => now))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => now))
            .ForMember(dest => dest.PostFeaturedImage, opt => opt.MapFrom(src => src.Preview.Image));
    }
}
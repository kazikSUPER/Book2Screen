namespace Book2Screen.Application.Mappings;

using AutoMapper;
using Book2Screen.Application.DTOs;
using Book2Screen.Domain.Entities;

public class AdaptationProfile : Profile
{
    public AdaptationProfile()
    {
        this.CreateMap<Adaptation, AdaptationDto>().ReverseMap();
    }
}

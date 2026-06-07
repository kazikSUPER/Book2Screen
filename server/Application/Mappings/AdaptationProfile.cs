// <copyright file="AdaptationProfile.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Mappings;

using AutoMapper;
using Book2Screen.Application.DTOs;
using Book2Screen.Domain.Entities;

/// <summary>
/// AutoMapper profile for Adaptation entity and DTO.
/// </summary>
public class AdaptationProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptationProfile"/> class.
    /// </summary>
    public AdaptationProfile()
    {
        this.CreateMap<Adaptation, AdaptationDto>()
            .ReverseMap();

        this.CreateMap<Work, BookScreenItemDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AdaptationId, opt => opt.MapFrom(src => src.AdaptationId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Adaptation.ReleaseYear ?? 0))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Adaptation.Country ?? "Unknown"))
            .ForMember(dest => dest.Poster, opt => opt.MapFrom(src => src.Adaptation.PosterUrl ?? "https://via.placeholder.com/300x450"))
            .ForMember(dest => dest.BookRating, opt => opt.MapFrom(src => src.Rating != null ? (double?)src.Rating.BookRating : 0))
            .ForMember(dest => dest.FilmRating, opt => opt.MapFrom(src => src.Rating != null ? (double?)src.Rating.AdaptationRating : 0))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Summary ?? string.Empty))
            .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Book.Genre ?? "Драма"))
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => string.Join(", ", src.Book.Authors.Select(a => a.FullName))))
            .ForMember(dest => dest.FilmYear, opt => opt.MapFrom(src => src.Adaptation.ReleaseYear))
            .ForMember(dest => dest.FilmCountry, opt => opt.MapFrom(src => src.Adaptation.Country))
            .ForMember(dest => dest.FilmPoster, opt => opt.MapFrom(src => src.Adaptation.PosterUrl))
            .ForMember(dest => dest.Director, opt => opt.MapFrom(src => src.Adaptation.Studio))
            .ForMember(dest => dest.BookSummary, opt => opt.MapFrom(src => src.Book.Description))
            .ForMember(dest => dest.FilmSummary, opt => opt.MapFrom(src => src.Adaptation.Description))
            .ForMember(dest => dest.Differences, opt => opt.MapFrom(src => src.DifferenceMap != null ? src.DifferenceMap.Differences : new List<Difference>()))
            .ForMember(dest => dest.HasMap, opt => opt.MapFrom(src => src.DifferenceMap != null));

        this.CreateMap<Difference, DifferenceDto>();
    }
}

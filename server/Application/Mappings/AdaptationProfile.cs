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
        this.CreateMap<Adaptation, AdaptationDto>().ReverseMap();
    }
}

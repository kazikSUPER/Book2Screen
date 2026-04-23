using AutoFilterer.Extensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;
using Book2Screen.Application.Interfaces;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Book2Screen.Application.Services;

public class AdaptationService : IAdaptationService
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;

    public AdaptationService(ApplicationDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<AdaptationDto>> GetAllAdaptationsAsync()
    {
        return await this.context.Adaptations
            .ProjectTo<AdaptationDto>(this.mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<AdaptationDto?> GetAdaptationByIdAsync(int id)
    {
        var adaptation = await this.context.Adaptations.FindAsync(id);
        return adaptation == null ? null : this.mapper.Map<AdaptationDto>(adaptation);
    }

    public async Task<IEnumerable<AdaptationDto>> GetFilteredAdaptationsAsync(AdaptationFilter filter)
    {
        return await this.context.Adaptations
            .ApplyFilter(filter)
            .ProjectTo<AdaptationDto>(this.mapper.ConfigurationProvider)
            .ToListAsync();
    }
}

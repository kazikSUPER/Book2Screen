using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;

namespace Book2Screen.Application.Interfaces;

public interface IAdaptationService
{
    Task<IEnumerable<AdaptationDto>> GetAllAdaptationsAsync();
    Task<AdaptationDto?> GetAdaptationByIdAsync(int id);
    Task<IEnumerable<AdaptationDto>> GetFilteredAdaptationsAsync(AdaptationFilter filter);
}

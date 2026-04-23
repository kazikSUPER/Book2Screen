namespace Book2Screen.Application.Interfaces;

using Book2Screen.Application.DTOs;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginDto loginDto);
    Task<string?> RegisterAsync(RegisterRequest registerRequest);
}

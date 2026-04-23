namespace Book2Screen.Application.DTOs;

/// <summary>
/// Відповідь з JWT токеном.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT токен доступу.
    /// </summary>
    public string Token { get; set; } = null!;
}

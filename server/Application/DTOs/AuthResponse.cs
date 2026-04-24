namespace Book2Screen.Application.DTOs;

/// <summary>
/// Відповідь з JWT токеном.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT токен доступу для авторизації.
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// Унікальний ідентифікатор користувача.
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// Електронна пошта користувача.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Публічне ім'я (нікнейм) користувача.
    /// </summary>
    public string Nickname { get; set; } = null!;
}

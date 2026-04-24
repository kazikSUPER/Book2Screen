namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Авторизація користувача.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Електронна пошта користувача.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Пароль користувача.
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;
}

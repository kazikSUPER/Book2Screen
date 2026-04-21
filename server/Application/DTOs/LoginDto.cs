namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Авторизація користувача.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Логін користувача (пошта або унікальне ім'я).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    /// <summary>
    /// Пароль користувача.
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;
}

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Реєстрація користувача.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Унікальне ім'я користувача.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    /// <summary>
    /// Електронна пошта користувача.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Пароль користувача.
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;
}

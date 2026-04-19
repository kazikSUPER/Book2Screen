namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Користувач системи.
/// </summary>
public class User : BaseEntity
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
    /// Хеш пароля.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Роль користувача (user, admin, moderator).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "user";

    /// <summary>
    /// Статус активності акаунту.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Навігаційні властивості
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}

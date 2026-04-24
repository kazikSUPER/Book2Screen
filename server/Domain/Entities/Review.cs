namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Відгук користувача.
/// </summary>
public class Review : BaseEntity
{
    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public Guid WorkId { get; set; }

    public Work Work { get; set; } = null!;

    /// <summary>
    /// Тип об'єкта відгуку (book, adaptation, comparison).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string TargetType { get; set; } = null!;

    [Required]
    public string Text { get; set; } = null!;

    public bool IsSpoiler { get; set; } = false;

    public double Rating { get; set; } = 0;

    public int LikesCount { get; set; } = 0;
}

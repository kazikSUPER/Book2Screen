namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Голос користувача в опитуванні "Що краще?".
/// </summary>
public class Vote : BaseEntity
{
    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public Guid WorkId { get; set; }

    public Work Work { get; set; } = null!;

    /// <summary>
    /// Обраний варіант (book, adaptation).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string SelectedOption { get; set; } = null!;
}

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Проміжна сутність для зв'язку Акторів та Адаптацій (Many-to-Many з додатковим полем RoleName).
/// </summary>
public class AdaptationActor : BaseEntity
{
    public Guid AdaptationId { get; set; }

    public Adaptation Adaptation { get; set; } = null!;

    public Guid ActorId { get; set; }

    public Actor Actor { get; set; } = null!;

    /// <summary>
    /// Назва ролі актора в цій адаптації.
    /// </summary>
    [MaxLength(150)]
    public string? RoleName { get; set; }
}

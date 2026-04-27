// <copyright file="User.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Користувач системи.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Gets or sets унікальне ім'я користувача.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    /// <summary>
    /// Gets or sets електронну пошту користувача.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets хеш пароля.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Gets or sets роль користувача (user, admin, moderator).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "user";

    /// <summary>
    /// Gets or sets a value indicating whether статус активності акаунту.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets колекцію відгуків користувача.
    /// </summary>
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>
    /// Gets or sets колекцію голосів користувача.
    /// </summary>
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}

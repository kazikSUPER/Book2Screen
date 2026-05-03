// <copyright file="PasswordResetToken.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Токен для відновлення паролю.
/// </summary>
public class PasswordResetToken : BaseEntity
{
    /// <summary>
    /// Gets or sets електронну пошту користувача.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets код відновлення.
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Gets or sets час закінчення дії коду.
    /// </summary>
    public DateTime ExpiryTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether чи був токен використаний.
    /// </summary>
    public bool IsUsed { get; set; } = false;
}

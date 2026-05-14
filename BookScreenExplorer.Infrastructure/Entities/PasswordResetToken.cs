using System;

namespace BookScreenExplorer.Infrastructure.Entities;

public class PasswordResetToken
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public DateTime ExpiryTime { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

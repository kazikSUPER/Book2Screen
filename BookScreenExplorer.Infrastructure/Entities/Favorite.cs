using System;

namespace BookScreenExplorer.Infrastructure.Entities;

public class Favorite
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid WorkId { get; set; }

    public User User { get; set; } = null!;

    public Work Work { get; set; } = null!;
}

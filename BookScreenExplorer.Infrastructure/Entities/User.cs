namespace BookScreenExplorer.Infrastructure.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}

namespace CharacterChronicles.Models;

public class User
{
    public string Id { get; set; } =
        Guid.NewGuid().ToString("N");

    public string DisplayName { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public string NormalizedEmail { get; set; } =
        string.Empty;

    public string PasswordHash { get; set; } =
        string.Empty;

    public string FriendCode { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;
}
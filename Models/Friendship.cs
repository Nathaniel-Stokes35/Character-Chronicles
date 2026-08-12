namespace CharacterChronicles.Models;

public class Friendship
{
    public string UserId { get; set; } =
        string.Empty;

    public string FriendUserId { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;
}
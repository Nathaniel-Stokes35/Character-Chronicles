namespace CharacterChronicles.Models;

public class CampaignMember
{
    public int CampaignId { get; set; }

    public string UserId { get; set; } =
        string.Empty;

    public DateTime JoinedAt { get; set; } =
        DateTime.UtcNow;

    public Campaign? Campaign { get; set; }
}
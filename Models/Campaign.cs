namespace CharacterChronicles.Models;

public class Campaign
{
    public int Id { get; set; }

    // The user who created / owns the campaign.
    public string UserId { get; set; } =
        string.Empty;

    public string Name { get; set; } =
        string.Empty;

    public string Description { get; set; } =
        string.Empty;

    public string Setting { get; set; } =
        string.Empty;

    public string GameSystem { get; set; } =
        "D&D 5e";

    public string SessionSchedule { get; set; } =
        string.Empty;

    public string Status { get; set; } =
        "Active";

    public string DmNotes { get; set; } =
        string.Empty;

    public bool IsActive { get; set; } =
        true;

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } =
        DateTime.UtcNow;

    public List<Character> Characters { get; set; } =
        [];

    public List<CampaignMember> Members { get; set; } =
        [];

    public List<CampaignMetricDefinition>
        MetricDefinitions { get; set; } =
            [];
}
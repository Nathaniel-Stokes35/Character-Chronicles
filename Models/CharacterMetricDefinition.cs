namespace CharacterChronicles.Models;

public enum CharacterMetricDataType
{
    Text,
    Number,
    Boolean
}

/*==============================================================================================================================================================
 | Represents a reusable character metric definition created by a user.                                                                                         |
 | Metric definitions can be enabled across multiple campaigns and used to organize, group, sort, or describe characters within those campaigns.                |
 ==============================================================================================================================================================*/

public class CharacterMetricDefinition
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public CharacterMetricDataType DataType { get; set; }
        = CharacterMetricDataType.Text;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public List<CampaignMetricDefinition> CampaignMetrics { get; set; }
        = [];

    public List<CharacterMetricValue> Values { get; set; }
        = [];
}
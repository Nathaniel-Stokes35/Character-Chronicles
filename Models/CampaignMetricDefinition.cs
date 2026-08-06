namespace CharacterChronicles.Models;

public class CampaignMetricDefinition
{
    public int CampaignId { get; set; }

    public Campaign Campaign { get; set; } = default!;

    public int MetricDefinitionId { get; set; }

    public CharacterMetricDefinition MetricDefinition { get; set; }
        = default!;

    public int SortOrder { get; set; }
}
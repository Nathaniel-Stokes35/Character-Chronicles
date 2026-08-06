namespace CharacterChronicles.Models;

public class CharacterMetricValue
{
    public int Id { get; set; }

    public int CharacterId { get; set; }
    public Character Character { get; set; } = default!;

    public int MetricDefinitionId { get; set; }
    public CharacterMetricDefinition MetricDefinition { get; set; } = default!;

    public string Value { get; set; } = string.Empty;
}
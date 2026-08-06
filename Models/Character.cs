using System.ComponentModel.DataAnnotations.Schema;

namespace CharacterChronicles.Models;

public enum CharacterType
{
    PlayerCharacter,
    NonPlayerCharacter
}

public class Character
{
    public int Id { get; set; }

    // User who owns/manages this record.
    public string UserId { get; set; } = string.Empty;

    // A character may exist before being assigned
    // to a campaign.
    public int? CampaignId { get; set; }

    public Campaign? Campaign { get; set; }

    public CharacterType CharacterType { get; set; } =
        CharacterType.PlayerCharacter;

    public string Name { get; set; } = string.Empty;
    public string Ancestry { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
    public int Level { get; set; } = 1;
    public string Status { get; set; } = "Active";
    public string Theme { get; set; } = "forest";

    [NotMapped]
    public int StrengthModifier => GetModifier(Strength);

    [NotMapped]
    public int DexterityModifier => GetModifier(Dexterity);

    [NotMapped]
    public int ConstitutionModifier => GetModifier(Constitution);

    [NotMapped]
    public int IntelligenceModifier => GetModifier(Intelligence);

    [NotMapped]
    public int WisdomModifier => GetModifier(Wisdom);

    [NotMapped]
    public int CharismaModifier => GetModifier(Charisma);

    public string? Background { get; set; }
    public string? Alignment { get; set; }

    public int ArmorClass { get; set; }
    public int HitPointMaximum { get; set; }
    public int CurrentHitPoints { get; set; }

    public string? Equipment { get; set; }
    public string? FeaturesAndTraits { get; set; }

    public string? PersonalityTraits { get; set; }
    public string? Ideals { get; set; }
    public string? Bonds { get; set; }
    public string? Flaws { get; set; }

private static int GetModifier(int score) =>
    (int)Math.Floor((score - 10) / 2.0);
    
    // Values Specific to this Character as defined by the Campaign DM.
    public List<CharacterMetricValue> MetricValues { get; set; } = [];

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } =
        DateTime.UtcNow;

    [NotMapped]
    public string Initials
    {
        get
        {
            var parts = Name.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            return parts.Length switch
            {
                0 => "?",
                1 => parts[0][..1].ToUpperInvariant(),
                _ => $"{parts[0][0]}{parts[^1][0]}"
                    .ToUpperInvariant()
            };
        }
    }

    [NotMapped]
    public string Updated
    {
        get
        {
            var age =
                DateTime.UtcNow - UpdatedAt;

            if (age.TotalMinutes < 1)
                return "Just now";

            if (age.TotalHours < 1)
                return $"{(int)age.TotalMinutes} minutes ago";

            if (age.TotalDays < 1)
                return $"{(int)age.TotalHours} hours ago";

            if (age.TotalDays < 2)
                return "Yesterday";

            if (age.TotalDays < 7)
                return $"{(int)age.TotalDays} days ago";

            return UpdatedAt
                .ToLocalTime()
                .ToString("MMM d, yyyy");
        }
    }
}
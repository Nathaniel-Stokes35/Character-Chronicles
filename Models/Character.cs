using System.ComponentModel.DataAnnotations.Schema;

namespace CharacterCronicles.Models;

public class Character
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Ancestry { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;

    public int Level { get; set; }

    public string Campaign { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string Theme { get; set; } = "forest";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

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
                _ => $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant()
            };
        }
    }

    [NotMapped]
    public string Updated
    {
        get
        {
            var age = DateTime.UtcNow - UpdatedAt;

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

            return UpdatedAt.ToLocalTime().ToString("MMM d, yyyy");
        }
    }
}
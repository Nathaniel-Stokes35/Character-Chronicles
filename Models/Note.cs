using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CharacterChronicles.Models;

public class Note
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    // Nullable FK — a note can exist without being tied to a character.
    public int? CharacterId { get; set; }

    // Reference navigation only. Character.cs is NOT modified —
    // there is no ICollection<Note> on the Character side.
    [ForeignKey(nameof(CharacterId))]
    public Character? Character { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
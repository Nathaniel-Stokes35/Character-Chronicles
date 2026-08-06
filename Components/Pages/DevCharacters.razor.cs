using CharacterChronicles.Data;
using CharacterChronicles.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace CharacterChronicles.Components.Pages;

public partial class DevCharacters
{
    [Inject]
    private IDbContextFactory<CharacterChroniclesDbContext>
        DbFactory { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment
        Environment { get; set; } = default!;

    private bool _isDevelopment;
    private bool _isLoading = true;

    private List<Character> _characters = [];
    private List<Campaign> _campaigns = [];

    private Character? SelectedCharacter;

    private sealed record AbilityRoll(
        int Id,
        int Value,
        IReadOnlyList<int> Dice);

    private List<AbilityRoll> AbilityRolls = [];

    private int? StrengthRollId;
    private int? DexterityRollId;
    private int? ConstitutionRollId;
    private int? IntelligenceRollId;
    private int? WisdomRollId;
    private int? CharismaRollId;

    private bool IsCreateOpen;

    private string NewName = string.Empty;
    private string NewAncestry = string.Empty;
    private string NewClass = "Fighter";

    private int NewLevel = 1;

    private string NewStatus = "Active";

    private CharacterType NewCharacterType =
        CharacterType.PlayerCharacter;

    private int? NewCampaignId;

    private string? NewPersonalityTraits;
    private string? NewIdeals;
    private string? NewBonds;
    private string? NewFlaws;

    private string CreateError = string.Empty;

    private static AbilityRoll RollAbilityScore(
        int id)
    {
        var dice =
            Enumerable.Range(0, 4)
                .Select(_ =>
                    Random.Shared.Next(1, 7))
                .ToArray();

        var score =
            dice
                .OrderByDescending(value => value)
                .Take(3)
                .Sum();

        return new AbilityRoll(
            id,
            score,
            dice);
    }

    private void RollStats()
    {
        AbilityRolls =
            Enumerable.Range(1, 6)
                .Select(RollAbilityScore)
                .ToList();

        ClearStatAssignments();
    }
    private void ClearStatAssignments()
    {
        StrengthRollId = null;
        DexterityRollId = null;
        ConstitutionRollId = null;
        IntelligenceRollId = null;
        WisdomRollId = null;
        CharismaRollId = null;
    }
    private int HighestLevel =>
        _characters.Count == 0
            ? 0
            : _characters.Max(
                character => character.Level);

    private string NewInitials
    {
        get
        {
            var parts =
                NewName.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            return parts.Length switch
            {
                0 => "?",
                1 => parts[0][..1]
                    .ToUpperInvariant(),

                _ =>
                    $"{parts[0][0]}{parts[^1][0]}"
                        .ToUpperInvariant()
            };
        }
    }
    private int GetRollValue(
        int? rollId)
    {
        if (!rollId.HasValue)
        {
            return 0;
        }

        return AbilityRolls
            .First(roll =>
                roll.Id == rollId.Value)
            .Value;
    }

    private int GetModifier(
        int score)
    {
        return (int)Math.Floor(
            (score - 10) / 2.0);
    }

    private static string FormatModifier(
        int modifier)
    {
        return modifier >= 0
            ? $"+{modifier}"
            : modifier.ToString();
    }

    protected override async Task OnInitializedAsync()
    {
        _isDevelopment =
            Environment.IsDevelopment();

        await LoadCampaignsAsync();
        await LoadCharactersAsync();

        _isLoading = false;
    }

    private async Task LoadCharactersAsync()
    {
        await using var context =
            await DbFactory.CreateDbContextAsync();

        _characters =
            await context.Characters
                .AsNoTracking()
                .Include(character =>
                    character.Campaign)
                .Include(character =>
                    character.MetricValues)
                    .ThenInclude(value =>
                        value.MetricDefinition)
                .OrderBy(character =>
                    character.Name)
                .ToListAsync();
    }

    private async Task LoadCampaignsAsync()
    {
        await using var context =
            await DbFactory.CreateDbContextAsync();

        _campaigns =
            await context.Campaigns
                .AsNoTracking()
                .OrderBy(campaign =>
                    campaign.Name)
                .ToListAsync();
    }

    private void OpenCreatePanel()
    {
        CreateError = string.Empty;
        IsCreateOpen = true;
    }

    private void CloseCreatePanel()
    {
        IsCreateOpen = false;
        CreateError = string.Empty;
    }

    private void ViewCharacter(
    Character character)
    {
        SelectedCharacter = character;
    }

    private void CloseCharacterSheet()
    {
        SelectedCharacter = null;
    }

    private async Task SaveCharacterAsync(
        Character updated)
    {
        await using var context =
            await DbFactory.CreateDbContextAsync();

        var character =
            await context.Characters
                .FirstOrDefaultAsync(
                    x => x.Id == updated.Id);

        if (character is null)
        {
            return;
        }

        character.Name = updated.Name;
        character.Ancestry = updated.Ancestry;
        character.ClassName = updated.ClassName;
        character.Level = updated.Level;

        character.PersonalityTraits =
            updated.PersonalityTraits;

        character.Ideals = updated.Ideals;
        character.Bonds = updated.Bonds;
        character.Flaws = updated.Flaws;

        character.Background = updated.Background;
        character.Alignment = updated.Alignment;

        character.ArmorClass = updated.ArmorClass;

        character.HitPointMaximum =
            updated.HitPointMaximum;

        character.CurrentHitPoints =
            updated.CurrentHitPoints;

        character.Equipment = updated.Equipment;

        character.FeaturesAndTraits =
            updated.FeaturesAndTraits;

        character.UpdatedAt =
            DateTime.UtcNow;

        await context.SaveChangesAsync();

        SelectedCharacter = null;

        await LoadCharactersAsync();
    }
    private async Task DeleteCharacterAsync(
        Character character)
    {
        await using var context =
            await DbFactory.CreateDbContextAsync();

        var existing =
            await context.Characters
                .FirstOrDefaultAsync(
                    x => x.Id == character.Id);

        if (existing is null)
        {
            return;
        }

        context.Characters.Remove(existing);

        await context.SaveChangesAsync();

        SelectedCharacter = null;

        await LoadCharactersAsync();
    }
    private async Task CreateCharacterAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName))
        {
            CreateError =
                "Give the character a name before saving.";

            return;
        }
        if (!TryValidateAbilityScores(
            out var abilityError))
        {
            CreateError = abilityError;
            return;
        }

        await using var context =
            await DbFactory.CreateDbContextAsync();

        var character =
        new Character
        {
            UserId = "development-user",

            CampaignId = NewCampaignId,
            CharacterType = NewCharacterType,

            Name = NewName.Trim(),

            Ancestry =
                string.IsNullOrWhiteSpace(NewAncestry)
                    ? "Unknown"
                    : NewAncestry.Trim(),

            ClassName = NewClass,
            Level = Math.Clamp(NewLevel, 1, 20),

            Strength =
                GetRollValue(StrengthRollId),

            Dexterity =
                GetRollValue(DexterityRollId),

            Constitution =
                GetRollValue(ConstitutionRollId),

            Intelligence =
                GetRollValue(IntelligenceRollId),

            Wisdom =
                GetRollValue(WisdomRollId),

            Charisma =
                GetRollValue(CharismaRollId),

            Status = NewStatus,

            Theme =
                GetThemeForClass(NewClass),

            PersonalityTraits =
                CleanOptionalText(
                    NewPersonalityTraits),

            Ideals =
                CleanOptionalText(NewIdeals),

            Bonds =
                CleanOptionalText(NewBonds),

            Flaws =
                CleanOptionalText(NewFlaws),

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Characters.Add(
            character);

        await context.SaveChangesAsync();

        ResetCreateForm();

        IsCreateOpen = false;

        await LoadCharactersAsync();
    }

    private static string?
        CleanOptionalText(
            string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string GetThemeForClass(
        string className)
    {
        return className switch
        {
            "Wizard" or
            "Sorcerer" =>
                "arcane",

            "Warlock" or
            "Rogue" =>
                "shadow",

            "Paladin" or
            "Cleric" or
            "Monk" =>
                "celestial",

            "Ranger" or
            "Druid" =>
                "forest",

            "Barbarian" or
            "Fighter" =>
                "ember",

            "Bard" =>
                "arcane",

            _ =>
                "forest"
        };
    }
    
    private bool TryValidateAbilityScores(
        out string error)
    {
        var assignments =
            new int?[]
            {
                StrengthRollId,
                DexterityRollId,
                ConstitutionRollId,
                IntelligenceRollId,
                WisdomRollId,
                CharismaRollId
            };

        if (AbilityRolls.Count != 6)
        {
            error =
                "Roll your ability scores before creating the character.";

            return false;
        }

        if (assignments.Any(id =>
            !id.HasValue))
        {
            error =
                "Assign a rolled score to every ability.";

            return false;
        }

        if (assignments
            .Select(id => id!.Value)
            .Distinct()
            .Count() != 6)
        {
            error =
                "Each rolled score can only be assigned once.";

            return false;
        }

        error = string.Empty;
        return true;
    }

    private void ResetCreateForm()
    {
        NewName = string.Empty;
        NewAncestry = string.Empty;
        NewClass = "Fighter";
        NewLevel = 1;
        NewStatus = "Active";

        NewCharacterType =
            CharacterType.PlayerCharacter;

        NewCampaignId = null;

        NewPersonalityTraits = null;
        NewIdeals = null;
        NewBonds = null;
        NewFlaws = null;

        AbilityRolls.Clear();
        ClearStatAssignments();

        CreateError = string.Empty;
    }
}
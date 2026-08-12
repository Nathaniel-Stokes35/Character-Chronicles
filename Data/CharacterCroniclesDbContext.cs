using CharacterChronicles.Models;
using Microsoft.EntityFrameworkCore;

namespace CharacterChronicles.Data;

public class CharacterChroniclesDbContext : DbContext
{
    public CharacterChroniclesDbContext(
        DbContextOptions<CharacterChroniclesDbContext> options)
        : base(options)
    {
    }


    public DbSet<User> Users =>
        Set<User>();

    public DbSet<Friendship> Friendships =>
        Set<Friendship>();

    public DbSet<Character> Characters =>
        Set<Character>();

    public DbSet<Campaign> Campaigns =>
        Set<Campaign>();

    public DbSet<CampaignMember> CampaignMembers =>
        Set<CampaignMember>();

    public DbSet<CharacterMetricDefinition>
        CharacterMetricDefinitions =>
            Set<CharacterMetricDefinition>();

    public DbSet<CampaignMetricDefinition>
        CampaignMetricDefinitions =>
            Set<CampaignMetricDefinition>();

    public DbSet<CharacterMetricValue>
        CharacterMetricValues =>
            Set<CharacterMetricValue>();

    public DbSet<Note> Notes =>
        Set<Note>();


    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        // =================================================
        // USER
        // =================================================

        builder.Entity<User>(entity =>
        {
            entity.HasKey(
                user => user.Id);

            entity.Property(
                    user => user.Id)
                .HasMaxLength(64);

            entity.Property(
                    user => user.DisplayName)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(
                    user => user.Email)
                .HasMaxLength(320)
                .IsRequired();

            entity.Property(
                    user => user.NormalizedEmail)
                .HasMaxLength(320)
                .IsRequired();

            entity.Property(
                    user => user.PasswordHash)
                .IsRequired();

            entity.Property(
                    user => user.FriendCode)
                .HasMaxLength(12)
                .IsRequired();

            entity.Property(
                    user => user.CreatedAt)
                .IsRequired();

            entity.HasIndex(
                    user => user.NormalizedEmail)
                .IsUnique();

            entity.HasIndex(
                    user => user.FriendCode)
                .IsUnique();
        });


        // =================================================
        // FRIENDSHIP
        // =================================================

        builder.Entity<Friendship>(entity =>
        {
            entity.HasKey(
                friendship => new
                {
                    friendship.UserId,
                    friendship.FriendUserId
                });

            entity.Property(
                    friendship => friendship.UserId)
                .HasMaxLength(64);

            entity.Property(
                    friendship => friendship.FriendUserId)
                .HasMaxLength(64);

            entity.HasIndex(
                friendship => friendship.UserId);

            entity.HasIndex(
                friendship => friendship.FriendUserId);
        });


        // =================================================
        // CHARACTER
        // =================================================

        builder.Entity<Character>(entity =>
        {
            entity.HasKey(
                character => character.Id);

            entity.Property(
                    character => character.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(
                    character => character.UserId)
                .IsRequired();

            entity.HasIndex(
                character => character.UserId);

            entity.HasOne(
                    character => character.Campaign)
                .WithMany(
                    campaign => campaign.Characters)
                .HasForeignKey(
                    character => character.CampaignId)
                .OnDelete(
                    DeleteBehavior.SetNull);
        });


        // =================================================
        // CAMPAIGN
        // =================================================

        builder.Entity<Campaign>(entity =>
        {
            entity.HasKey(
                campaign => campaign.Id);

            entity.Property(
                    campaign => campaign.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(
                    campaign => campaign.UserId)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(
                    campaign => campaign.Description)
                .HasDefaultValue(string.Empty);

            entity.Property(
                    campaign => campaign.Setting)
                .HasMaxLength(150)
                .HasDefaultValue(string.Empty);

            entity.Property(
                    campaign => campaign.GameSystem)
                .HasMaxLength(80)
                .HasDefaultValue("D&D 5e");

            entity.Property(
                    campaign => campaign.SessionSchedule)
                .HasMaxLength(200)
                .HasDefaultValue(string.Empty);

            entity.Property(
                    campaign => campaign.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Active");

            entity.Property(
                    campaign => campaign.DmNotes)
                .HasDefaultValue(string.Empty);

            entity.HasIndex(
                campaign => campaign.UserId);
        });


        // =================================================
        // CAMPAIGN MEMBER
        // =================================================

        builder.Entity<CampaignMember>(entity =>
        {
            entity.HasKey(
                member => new
                {
                    member.CampaignId,
                    member.UserId
                });

            entity.Property(
                    member => member.UserId)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(
                member => member.UserId);

            entity.HasOne(
                    member => member.Campaign)
                .WithMany(
                    campaign => campaign.Members)
                .HasForeignKey(
                    member => member.CampaignId)
                .OnDelete(
                    DeleteBehavior.Cascade);
        });


        // =================================================
        // CHARACTER METRIC DEFINITION
        // =================================================

        builder.Entity<CharacterMetricDefinition>(
            entity =>
            {
                entity.HasKey(
                    metric => metric.Id);

                entity.Property(
                        metric => metric.UserId)
                    .IsRequired();

                entity.Property(
                        metric => metric.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(
                        metric => metric.Description)
                    .HasMaxLength(500);

                entity.HasIndex(
                        metric => new
                        {
                            metric.UserId,
                            metric.Name
                        })
                    .IsUnique();
            });


        // =================================================
        // CAMPAIGN METRIC DEFINITION
        // =================================================

        builder.Entity<CampaignMetricDefinition>(
            entity =>
            {
                entity.HasKey(
                    metric => new
                    {
                        metric.CampaignId,
                        metric.MetricDefinitionId
                    });

                entity.HasOne(
                        metric => metric.Campaign)
                    .WithMany(
                        campaign =>
                            campaign.MetricDefinitions)
                    .HasForeignKey(
                        metric => metric.CampaignId)
                    .OnDelete(
                        DeleteBehavior.Cascade);

                entity.HasOne(
                        metric =>
                            metric.MetricDefinition)
                    .WithMany(
                        definition =>
                            definition.CampaignMetrics)
                    .HasForeignKey(
                        metric =>
                            metric.MetricDefinitionId)
                    .OnDelete(
                        DeleteBehavior.Cascade);
            });


        // =================================================
        // CHARACTER METRIC VALUE
        // =================================================

        builder.Entity<CharacterMetricValue>(
            entity =>
            {
                entity.HasKey(
                    value => value.Id);

                entity.HasOne(
                        value => value.Character)
                    .WithMany(
                        character =>
                            character.MetricValues)
                    .HasForeignKey(
                        value =>
                            value.CharacterId)
                    .OnDelete(
                        DeleteBehavior.Cascade);

                entity.HasOne(
                        value =>
                            value.MetricDefinition)
                    .WithMany(
                        definition =>
                            definition.Values)
                    .HasForeignKey(
                        value =>
                            value.MetricDefinitionId)
                    .OnDelete(
                        DeleteBehavior.Cascade);

                entity.HasIndex(
                        value => new
                        {
                            value.CharacterId,
                            value.MetricDefinitionId
                        })
                    .IsUnique();
            });


        // =================================================
        // NOTE
        // =================================================

        builder.Entity<Note>(entity =>
        {
            entity.HasKey(
                note => note.Id);

            entity.Property(
                    note => note.UserId)
                .IsRequired();

            entity.Property(
                    note => note.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(
                    note => note.Content)
                .IsRequired();

            entity.HasIndex(
                note => note.UserId);

            entity.HasOne(
                    note => note.Character)
                .WithMany()
                .HasForeignKey(
                    note => note.CharacterId)
                .OnDelete(
                    DeleteBehavior.Cascade);
        });
    }
}
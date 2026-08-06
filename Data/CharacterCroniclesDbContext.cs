using CharacterChronicles.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CharacterChronicles.Data;

public class CharacterChroniclesDbContext
    : IdentityDbContext<IdentityUser>
{
    public CharacterChroniclesDbContext(
        DbContextOptions<CharacterChroniclesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Character> Characters =>
        Set<Character>();

    public DbSet<Campaign> Campaigns =>
        Set<Campaign>();

    public DbSet<CharacterMetricDefinition>
        CharacterMetricDefinitions =>
            Set<CharacterMetricDefinition>();

    public DbSet<CampaignMetricDefinition>
        CampaignMetricDefinitions =>
            Set<CampaignMetricDefinition>();

    public DbSet<CharacterMetricValue>
        CharacterMetricValues =>
            Set<CharacterMetricValue>();

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* ==========================================================
         | Character                                                 |
         ============================================================ */

        builder.Entity<Character>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.HasIndex(x => x.UserId);

            entity.HasOne(x => x.Campaign)
                .WithMany(x => x.Characters)
                .HasForeignKey(x => x.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        /* ==========================================================
         | Campaign                                                  |
         ============================================================ */

        builder.Entity<Campaign>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.HasIndex(x => x.UserId);
        });

        /* ==========================================================
         | Reusable User Metric Definition                           |
         ============================================================ */

        builder.Entity<CharacterMetricDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            // Prevent a user from creating "Faction" multiple times.
            entity.HasIndex(x => new
            {
                x.UserId,
                x.Name
            })
            .IsUnique();
        });

        /* ==========================================================
         | Campaign <-> Metric Definition                            |
         ============================================================ */

        builder.Entity<CampaignMetricDefinition>(entity =>
        {
            entity.HasKey(x => new
            {
                x.CampaignId,
                x.MetricDefinitionId
            });

            entity.HasOne(x => x.Campaign)
                .WithMany(x => x.MetricDefinitions)
                .HasForeignKey(x => x.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.MetricDefinition)
                .WithMany(x => x.CampaignMetrics)
                .HasForeignKey(x => x.MetricDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        /* ==========================================================
         | Character Metric Value                                    |
         ============================================================ */

        builder.Entity<CharacterMetricValue>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Character)
                .WithMany(x => x.MetricValues)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.MetricDefinition)
                .WithMany(x => x.Values)
                .HasForeignKey(x => x.MetricDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            // One value per metric per character.
            entity.HasIndex(x => new
            {
                x.CharacterId,
                x.MetricDefinitionId
            })
            .IsUnique();
        });
    }
}
using CharacterCronicles.Models;
using Microsoft.EntityFrameworkCore;

namespace CharacterCronicles.Data;

public class CharacterCroniclesDbContext : DbContext
{
    public CharacterCroniclesDbContext(
        DbContextOptions<CharacterCroniclesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Character> Characters => Set<Character>();
}
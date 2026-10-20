using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;

namespace MeepleZone.Systeme.Donnee;

public class MeepleZoneContexte : DbContext
{
    public const string CollationCourriel = "courriel_ci";

    public MeepleZoneContexte(DbContextOptions<MeepleZoneContexte> options)
        : base(options)
    {
    }

    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();

    public DbSet<IdentiteExterne> IdentitesExternes => Set<IdentiteExterne>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCollation(CollationCourriel, locale: "und-u-ks-level2", provider: "icu", deterministic: false);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeepleZoneContexte).Assembly);
    }
}
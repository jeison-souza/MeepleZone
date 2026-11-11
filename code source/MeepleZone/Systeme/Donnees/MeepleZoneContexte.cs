using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;

namespace MeepleZone.Systeme.Donnees;

public class MeepleZoneContexte(DbContextOptions<MeepleZoneContexte> options) : DbContext(options)
{
    public const string CollationCourriel = "courriel_ci";

    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();

    public DbSet<IdentiteExterne> IdentitesExternes => Set<IdentiteExterne>();

    public DbSet<Annexe> Annexes => Set<Annexe>();

    public DbSet<ModeJeu> ModesJeu => Set<ModeJeu>();

    public DbSet<Mecanique> Mecaniques => Set<Mecanique>();

    public DbSet<Jeu> Jeux => Set<Jeu>();

    public DbSet<JeuMecanique> JeuxMecaniques => Set<JeuMecanique>();

    public DbSet<Possession> Possessions => Set<Possession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCollation(CollationCourriel, locale: "und-u-ks-level2", provider: "icu", deterministic: false);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeepleZoneContexte).Assembly);
    }
}
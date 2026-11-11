using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace MeepleZone.Systeme.Donnees.Configurations;

internal class ModeJeuConfiguration : IEntityTypeConfiguration<ModeJeu>
{
    public void Configure(EntityTypeBuilder<ModeJeu> builder)
    {
        builder.ToTable("mode_jeu");

        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.Nom)
            .IsUnique();

        builder.Property(u => u.Nom)
            .IsRequired();

        builder.Property(u => u.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        // Objets anonymes : CreeLe a un set privé, que HasData ne peut pas atteindre depuis l'entité.
        builder.HasData(
            new { Id = new Guid("01a1c7e7-8000-7001-adcc-71b334e95c80"), Nom = "Compétitif", Description = "Chaque joueur joue pour lui-même et cherche à gagner contre les autres.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7002-9c8a-87833414cc9b"), Nom = "Coopératif", Description = "Tous les joueurs gagnent ou perdent ensemble contre le jeu.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7003-9c80-f4cb84b0633c"), Nom = "Semi-coopératif", Description = "Les joueurs coopèrent, mais chacun peut avoir un objectif personnel, ou l'un d'eux peut trahir les autres.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7004-bac3-3649459c0f35"), Nom = "Par équipes", Description = "Les joueurs sont répartis en équipes, et chaque équipe joue contre les autres.", CreeLe = DonneesInitiales.CreeLe });
    }
}

using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeepleZone.Systeme.Donnees.Configurations;

internal class PossessionConfiguration : IEntityTypeConfiguration<Possession>
{
    public void Configure(EntityTypeBuilder<Possession> builder)
    {
        builder.ToTable("possession", t =>
        {
            t.HasCheckConstraint("ck_possession_note", "note BETWEEN 1 AND 10");
            t.HasCheckConstraint("ck_possession_parties_jouees", "parties_jouees >= 0");
        });

        builder.HasKey(p => p.Id);

        builder.HasOne<Utilisateur>()
            .WithMany()
            .HasForeignKey(p => p.UtilisateurId)
            .HasConstraintName("fk_possession_utilisateur_utilisateur_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Jeu>()
            .WithMany()
            .HasForeignKey(p => p.JeuId)
            .HasConstraintName("fk_possession_jeu_jeu_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.UtilisateurId, p.JeuId })
            .IsUnique();

        builder.Property(p => p.UtilisateurId)
            .IsRequired();

        builder.Property(p => p.JeuId)
            .IsRequired();

        builder.Property(p => p.PartiesJouees)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(p => p.ModifieLe)
            .IsRequired()
            .HasDefaultValueSql("now()");
    }
}

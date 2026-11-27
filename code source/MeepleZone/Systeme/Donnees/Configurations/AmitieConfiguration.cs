using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeepleZone.Systeme.Donnees.Configurations;

internal class AmitieConfiguration : IEntityTypeConfiguration<Amitie>
{
    public void Configure(EntityTypeBuilder<Amitie> builder)
    {
        builder.ToTable("amitie", t =>
        {
            t.HasCheckConstraint("ck_amitie_utilisateurs_ordre", "utilisateur_a_id < utilisateur_b_id");
        });

        builder.HasKey(a => new { a.UtilisateurAId, a.UtilisateurBId });

        builder.HasOne<Utilisateur>()
            .WithMany()
            .HasForeignKey(a => a.UtilisateurAId)
            .HasConstraintName("fk_amitie_utilisateur_utilisateur_a_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Utilisateur>()
            .WithMany()
            .HasForeignKey(a => a.UtilisateurBId)
            .HasConstraintName("fk_amitie_utilisateur_utilisateur_b_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");
    }
}

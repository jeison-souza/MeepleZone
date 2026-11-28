using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeepleZone.Systeme.Donnees.Configurations;

internal class DemandeVerificationConfiguration : IEntityTypeConfiguration<DemandeVerification>
{
    public void Configure(EntityTypeBuilder<DemandeVerification> builder)
    {
        builder.ToTable("demande_verification", t =>
        {
            t.HasCheckConstraint("ck_demande_verification_courriel", "courriel = btrim(courriel) AND courriel <> ''");
            t.HasCheckConstraint("ck_demande_verification_duree_validite", "duree_validite > interval '0'");
        });

        builder.HasKey(d => d.Id);

        builder.HasOne<Utilisateur>()
            .WithMany()
            .HasForeignKey(d => d.UtilisateurId)
            .HasConstraintName("fk_demande_verification_utilisateur_utilisateur_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.Courriel);

        builder.Property(d => d.Courriel)
            .IsRequired()
            .UseCollation(MeepleZoneContexte.CollationCourriel);

        builder.Property(d => d.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(d => d.DureeValidite)
            .IsRequired();
    }
}

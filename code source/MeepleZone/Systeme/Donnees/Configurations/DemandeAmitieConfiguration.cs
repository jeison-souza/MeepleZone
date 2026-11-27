using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Entites.Constantes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeepleZone.Systeme.Donnees.Configurations;

internal class DemandeAmitieConfiguration : IEntityTypeConfiguration<DemandeAmitie>
{
    public void Configure(EntityTypeBuilder<DemandeAmitie> builder)
    {
        builder.ToTable("demande_amitie", t =>
        {
            t.HasCheckConstraint("ck_demande_amitie_statut", "statut IN ('en_attente', 'ignoree', 'rejetee')");
            t.HasCheckConstraint("ck_demande_amitie_destinataire", "(destinataire_id IS NULL) <> (courriel_destinataire IS NULL)");
            t.HasCheckConstraint("ck_demande_amitie_demandeur_distinct", "demandeur_id <> destinataire_id");
        });

        builder.HasKey(d => d.Id);

        builder.HasOne<Utilisateur>()
            .WithMany()
            .HasForeignKey(d => d.DemandeurId)
            .HasConstraintName("fk_demande_amitie_utilisateur_demandeur_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Utilisateur>()
            .WithMany()
            .HasForeignKey(d => d.DestinataireId)
            .HasConstraintName("fk_demande_amitie_utilisateur_destinataire_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.DemandeurId, d.DestinataireId })
            .IsUnique();

        builder.HasIndex(d => new { d.DemandeurId, d.CourrielDestinataire })
            .IsUnique();

        builder.HasIndex(nameof(DemandeAmitie.CourrielDestinataire));

        builder.Property(d => d.CourrielDestinataire)
            .UseCollation(MeepleZoneContexte.CollationCourriel);

        builder.Property(d => d.DemandeurId)
            .IsRequired();

        builder.Property(d => d.Statut)
            .IsRequired()
            .HasDefaultValue(StatutsDemandeAmitie.EnAttente);

        builder.Property(d => d.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(d => d.ModifieLe)
            .IsRequired()
            .HasDefaultValueSql("now()");
    }
}

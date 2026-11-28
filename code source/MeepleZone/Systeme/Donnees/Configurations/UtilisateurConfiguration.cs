using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Entites.Constantes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace MeepleZone.Systeme.Donnees.Configurations;

internal class UtilisateurConfiguration : IEntityTypeConfiguration<Utilisateur>
{
    public void Configure(EntityTypeBuilder<Utilisateur> builder)
    {
        builder.ToTable("utilisateur", t =>
        {
            t.HasCheckConstraint("ck_utilisateur_role", "role IN ('membre', 'administrateur', 'systeme')");
            t.HasCheckConstraint("ck_utilisateur_visibilite_collection", "visibilite_collection IN ('privee', 'amis')");
            t.HasCheckConstraint("ck_utilisateur_code_amitie", "code_amitie ~ '^[A-HJ-NP-Z2-9]{8}$'");
            t.HasCheckConstraint("ck_utilisateur_courriel", "courriel = btrim(courriel) AND courriel <> ''");
        });

        builder.HasKey(u => u.Id);

        builder.HasOne<Annexe>()
            .WithMany()
            .HasForeignKey(u => u.PhotoId)
            .HasConstraintName("fk_utilisateur_annexe_photo_id")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(u => u.CodeAmitie)
            .IsUnique();

        builder.HasIndex(u => u.Courriel)
            .IsUnique()
            .HasFilter("courriel_verifie_le IS NOT NULL");

        builder.Property(u => u.Courriel)
            .UseCollation(MeepleZoneContexte.CollationCourriel);

        builder.Property(u => u.CodeAmitie)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasDefaultValue("membre");

        builder.Property(u => u.VisibiliteCollection)
            .IsRequired()
            .HasDefaultValue("privee");

        builder.Property(u => u.NomAffichage)
            .IsRequired();

        builder.Property(u => u.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(u => u.ModifieLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        // Objets anonymes : CreeLe a un set privé, que HasData ne peut pas atteindre depuis l'entité.
        builder.HasData(
            new { Id = ComptesTechniques.SystemeId, NomAffichage = "Système", Courriel = "systeme@meeplezone.ca", Role = "systeme", CodeAmitie = "SYSTEME2", VisibiliteCollection = "privee", CreeLe = DonneesInitiales.CreeLe, ModifieLe = DonneesInitiales.CreeLe },
            new { Id = ComptesTechniques.UtilisateurSupprimeId, NomAffichage = "Utilisateur supprimé", Courriel = "utilisateur-supprime@meeplezone.ca", Role = "systeme", CodeAmitie = "EFFACE22", VisibiliteCollection = "privee", CreeLe = DonneesInitiales.CreeLe, ModifieLe = DonneesInitiales.CreeLe }
        );
    }
}

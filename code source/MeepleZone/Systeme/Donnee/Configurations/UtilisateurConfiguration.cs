using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeepleZone.Systeme.Donnee.Configurations;

internal class UtilisateurConfiguration : IEntityTypeConfiguration<Utilisateur>
{
    public void Configure(EntityTypeBuilder<Utilisateur> builder)
    {
        builder.ToTable("utilisateur", t =>
        {
            t.HasCheckConstraint("ck_utilisateur_role", "role IN ('membre', 'administrateur', 'systeme')");
            t.HasCheckConstraint("ck_utilisateur_visibilite_collection", "visibilite_collection IN ('privee', 'amis')");
            t.HasCheckConstraint("ck_utilisateur_code_amitie", "code_amitie ~ '^[A-HJ-NP-Z2-9]{8}$'");
        });

        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.CodeAmitie)
            .IsUnique();

        builder.HasIndex(u => u.Courriel)
            .IsUnique();

        builder.Property(u => u.Courriel)
            .IsRequired()
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
    }
}

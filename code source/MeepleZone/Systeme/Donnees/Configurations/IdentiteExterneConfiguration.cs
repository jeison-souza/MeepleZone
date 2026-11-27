using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeepleZone.Systeme.Donnees.Configurations;

internal class IdentiteExterneConfiguration : IEntityTypeConfiguration<IdentiteExterne>
{
    public void Configure(EntityTypeBuilder<IdentiteExterne> builder)
    {
        builder.ToTable("identite_externe", t =>
        {
            t.HasCheckConstraint("ck_identite_externe_courriel", "courriel = btrim(courriel) AND courriel <> ''");
        });

        builder.HasKey(i => i.Id);

        builder.HasOne<Utilisateur>()
            .WithMany()
            .HasForeignKey(i => i.UtilisateurId)
            .HasConstraintName("fk_identite_externe_utilisateur_utilisateur_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(nameof(IdentiteExterne.Fournisseur), nameof(IdentiteExterne.IdentifiantCompteExterne))
            .IsUnique();

        builder.HasIndex(nameof(IdentiteExterne.Courriel));

        builder.Property(i => i.UtilisateurId)
            .IsRequired();

        builder.Property(i => i.IdentifiantCompteExterne)
            .IsRequired();

        builder.Property(i => i.Courriel)
            .UseCollation(MeepleZoneContexte.CollationCourriel);

        builder.Property(i => i.DerniereConnexionLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(i => i.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(i => i.ModifieLe)
            .IsRequired()
            .HasDefaultValueSql("now()");
    }
}

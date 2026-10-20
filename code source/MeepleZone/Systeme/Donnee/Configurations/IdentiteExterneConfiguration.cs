using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeepleZone.Systeme.Donnee.Configurations;

internal class IdentiteExterneConfiguration : IEntityTypeConfiguration<IdentiteExterne>
{
    public void Configure(EntityTypeBuilder<IdentiteExterne> builder)
    {
        builder.ToTable("identite_externe");

        builder.HasKey(u => u.Id);
        
        builder.HasOne<Utilisateur>()
            .WithMany()
            .HasForeignKey(ie => ie.UtilisateurId)
            .HasConstraintName("fk_identite_externe_utilisateur_utilisateur_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(nameof(IdentiteExterne.Fournisseur), nameof(IdentiteExterne.IdentifiantCompteExterne))
            .IsUnique();

        builder.Property(u => u.UtilisateurId)
            .IsRequired();

        builder.Property(u => u.IdentifiantCompteExterne)
            .IsRequired();

        builder.Property(u => u.Courriel)
            .IsRequired()
            .UseCollation(MeepleZoneContexte.CollationCourriel);

        builder.Property(u => u.DerniereConnexionLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(u => u.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(u => u.ModifieLe)
            .IsRequired()
            .HasDefaultValueSql("now()");
    }
}

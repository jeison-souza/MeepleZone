using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeepleZone.Systeme.Donnees.Configurations;

internal class AnnexeConfiguration : IEntityTypeConfiguration<Annexe>
{
    public void Configure(EntityTypeBuilder<Annexe> builder)
    {
        builder.ToTable("annexe", t =>
        {
            t.HasCheckConstraint("ck_annexe_nom_fichier", "char_length(nom_fichier) BETWEEN 1 AND 255");
            t.HasCheckConstraint("ck_annexe_taille_octets", "taille_octets > 0");
        });

        builder.HasKey(a => a.Id);

        builder.HasOne<Utilisateur>()
            .WithMany()
            .HasForeignKey(a => a.CreeParId)
            .HasConstraintName("fk_annexe_utilisateur_cree_par_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.NomFichier)
            .IsRequired();

        builder.Property(a => a.Contenu)
            .IsRequired();

        builder.Property(a => a.TypeMime)
            .IsRequired();

        builder.Property(a => a.TailleOctets)
            .IsRequired();

        builder.Property(a => a.CreeParId)
            .IsRequired();

        builder.Property(a => a.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");
    }
}

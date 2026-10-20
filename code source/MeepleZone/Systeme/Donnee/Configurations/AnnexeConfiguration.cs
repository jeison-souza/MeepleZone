using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeepleZone.Systeme.Donnee.Configurations;

internal class AnnexeConfiguration : IEntityTypeConfiguration<Annexe>
{
    public void Configure(EntityTypeBuilder<Annexe> builder)
    {
        builder.ToTable("annexe", t =>
        {
            t.HasCheckConstraint("ck_annexe_nom_fichier", "char_length(nom_fichier) BETWEEN 1 AND 255");
            t.HasCheckConstraint("ck_annexe_taille_octets", "taille_octets > 0");
        });

        builder.HasKey(u => u.Id);

        builder.HasOne<Utilisateur>()
            .WithMany()
            .HasForeignKey(ie => ie.CreeParId)
            .HasConstraintName("fk_annexe_utilisateur_cree_par_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(u => u.NomFichier)
            .IsRequired();

        builder.Property(u => u.Contenu)
            .IsRequired();

        builder.Property(u => u.TypeMime)
            .IsRequired();

        builder.Property(u => u.TailleOctets)
            .IsRequired();

        builder.Property(u => u.CreeParId)
            .IsRequired();

        builder.Property(u => u.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");
    }
}

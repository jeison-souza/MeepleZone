using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeepleZone.Systeme.Donnees.Configurations;

internal class JeuMecaniqueConfiguration : IEntityTypeConfiguration<JeuMecanique>
{
    public void Configure(EntityTypeBuilder<JeuMecanique> builder)
    {
        builder.ToTable("jeu_mecanique");

        builder.HasKey(nameof(JeuMecanique.JeuId), nameof(JeuMecanique.MecaniqueId));

        builder.HasOne<Jeu>()
            .WithMany()
            .HasForeignKey(u => u.JeuId)
            .HasConstraintName("fk_jeu_mecanique_jeu_jeu_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Mecanique>()
            .WithMany()
            .HasForeignKey(u => u.MecaniqueId)
            .HasConstraintName("fk_jeu_mecanique_mecanique_mecanique_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

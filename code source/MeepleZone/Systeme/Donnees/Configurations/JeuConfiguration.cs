using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeepleZone.Systeme.Donnees.Configurations;

internal class JeuConfiguration : IEntityTypeConfiguration<Jeu>
{
    public void Configure(EntityTypeBuilder<Jeu> builder)
    {
        builder.ToTable("jeu", t =>
        {
            t.HasCheckConstraint("ck_jeu_nom", "char_length(nom) BETWEEN 1 AND 255");
            t.HasCheckConstraint("ck_jeu_joueurs_min", "joueurs_min >= 1");
            t.HasCheckConstraint("ck_jeu_joueurs_max", "joueurs_max >= 1");
            t.HasCheckConstraint("ck_jeu_joueurs_meilleur", "joueurs_meilleur >= 1");
            t.HasCheckConstraint("ck_jeu_duree_min_minutes", "duree_min_minutes > 0");
            t.HasCheckConstraint("ck_jeu_duree_max_minutes", "duree_max_minutes > 0");
            t.HasCheckConstraint("ck_jeu_age_min", "age_min >= 0");
            t.HasCheckConstraint("ck_jeu_complexite", "complexite BETWEEN 1 AND 5");
            t.HasCheckConstraint("ck_jeu_statut", "statut IN ('en_attente', 'valide', 'refuse')");
            t.HasCheckConstraint("ck_jeu_joueurs_min_max", "joueurs_min IS NULL OR joueurs_max IS NULL OR joueurs_min <= joueurs_max");
            t.HasCheckConstraint("ck_jeu_joueurs_meilleur_min", "joueurs_meilleur IS NULL OR joueurs_min IS NULL OR joueurs_meilleur >= joueurs_min");
            t.HasCheckConstraint("ck_jeu_joueurs_meilleur_max", "joueurs_meilleur IS NULL OR joueurs_max IS NULL OR joueurs_meilleur <= joueurs_max");
            t.HasCheckConstraint("ck_jeu_duree_min_max", "duree_min_minutes IS NULL OR duree_max_minutes IS NULL OR duree_min_minutes <= duree_max_minutes");
            t.HasCheckConstraint("ck_jeu_couverture", "couverture_id IS NULL OR couverture_url IS NULL");
        });

        builder.HasKey(j => j.Id);

        builder.HasOne<ModeJeu>()
            .WithMany()
            .HasForeignKey(j => j.ModeJeuId)
            .HasConstraintName("fk_jeu_mode_jeu_mode_jeu_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Annexe>()
            .WithMany()
            .HasForeignKey(j => j.CouvertureId)
            .HasConstraintName("fk_jeu_annexe_couverture_id")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Utilisateur>()
            .WithMany()
            .HasForeignKey(j => j.CreeParId)
            .HasConstraintName("fk_jeu_utilisateur_cree_par_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(j => j.BggId)
            .IsUnique();

        builder.Property(j => j.Nom)
            .IsRequired();

        builder.Property(j => j.Complexite)
            .HasPrecision(2, 1);

        builder.Property(j => j.Statut)
            .IsRequired()
            .HasDefaultValue("en_attente");

        builder.Property(j => j.CreeParId)
            .IsRequired();

        builder.Property(j => j.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(j => j.ModifieLe)
            .IsRequired()
            .HasDefaultValueSql("now()");
    }
}

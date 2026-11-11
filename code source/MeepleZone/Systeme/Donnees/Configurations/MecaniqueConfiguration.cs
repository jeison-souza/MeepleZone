using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace MeepleZone.Systeme.Donnees.Configurations;

internal class MecaniqueConfiguration : IEntityTypeConfiguration<Mecanique>
{
    public void Configure(EntityTypeBuilder<Mecanique> builder)
    {
        builder.ToTable("mecanique");

        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.Nom)
            .IsUnique();

        builder.Property(u => u.Nom)
            .IsRequired();

        builder.HasIndex(u => u.BggId)
            .IsUnique();

        builder.Property(u => u.CreeLe)
            .IsRequired()
            .HasDefaultValueSql("now()");

        // Objets anonymes : CreeLe a un set privé, que HasData ne peut pas atteindre depuis l'entité.
        // Liste courte de MeepleZone (docs/schema/mecaniques-initiales.md); bgg_id reste vide pour l'instant.
        builder.HasData(
            new { Id = new Guid("01a1c7e7-8000-7005-bec5-8cc0f7c0f4cf"), Nom = "Pose d'ouvriers", Description = "Chaque joueur place des pions sur des cases d'action, qui ne peuvent souvent servir qu'une fois par tour.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7006-98b6-f2b0c15611b6"), Nom = "Construction de pioche", Description = "Chaque joueur part d'un petit paquet de cartes et en achète d'autres pour l'améliorer au fil de la partie.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7007-99bf-876d55d7363c"), Nom = "Construction de moteur", Description = "Les joueurs assemblent des éléments qui se renforcent entre eux et produisent de plus en plus de ressources.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7008-a934-b970b2133b80"), Nom = "Gestion de main", Description = "Le défi consiste à choisir quelles cartes jouer, garder ou défausser parmi celles qu'on tient en main.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7009-ba51-046f870ca39c"), Nom = "Draft de cartes", Description = "Chaque joueur choisit une carte parmi plusieurs, puis passe le reste à son voisin.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-700a-8b25-b3cebeb27ba4"), Nom = "Lancer de dés", Description = "Le résultat des dés détermine les actions possibles ou le déroulement des événements.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-700b-b27f-b08614a1dfa7"), Nom = "Placement de tuiles", Description = "Les joueurs posent des tuiles pour construire un paysage ou une structure, avec des règles d'adjacence.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-700c-8f50-56c207831c05"), Nom = "Majorité de zones", Description = "Il faut avoir plus de présence que les adversaires dans une zone pour la contrôler et en tirer des points.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-700d-bb72-356f51a22f8c"), Nom = "Collection de séries", Description = "Les joueurs réunissent des ensembles d'objets ou de cartes qui rapportent plus quand ils sont complets.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-700e-9beb-e12f23687eb7"), Nom = "Enchères", Description = "Les joueurs misent pour obtenir des objets, des actions ou l'ordre du tour.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-700f-b915-be3362b46bd9"), Nom = "Échanges et négociation", Description = "Les joueurs discutent, échangent des ressources ou concluent des ententes entre eux.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7010-b25d-e037b6e19408"), Nom = "Rôles cachés", Description = "Chaque joueur reçoit une identité secrète, et les camps s'affrontent sans se connaître.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7011-89ad-524b7c0b520f"), Nom = "Déduction", Description = "Il faut recouper des indices pour deviner une information cachée.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7012-9023-6a29bfaf8326"), Nom = "Bluff", Description = "Les joueurs dissimulent ou inventent de l'information pour tromper les autres.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7013-929f-09f2c4e9a2c6"), Nom = "Pousser sa chance", Description = "On peut continuer à tenter sa chance pour gagner plus, au risque de tout perdre au tour.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7014-865c-fdf4c2f7d63a"), Nom = "Pouvoirs variables", Description = "Chaque joueur dispose de capacités ou de règles qui lui sont propres.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7015-96d9-267515323aad"), Nom = "Plateau modulaire", Description = "Le plateau est assemblé à partir d'éléments qui changent d'une partie à l'autre.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7016-82ae-e33f4a4219c2"), Nom = "Déplacement sur le plateau", Description = "Les pions se déplacent sur des cases, des zones ou des chemins selon des règles de mouvement.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7017-99b9-6516d7a24420"), Nom = "Construction de réseaux", Description = "Les joueurs relient des lieux par des routes, des voies ou des liens pour former un réseau.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7018-8b8c-451f0e5e4d58"), Nom = "Points d'action", Description = "Chaque tour, le joueur dispose d'un nombre limité de points à répartir entre plusieurs actions.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-7019-8c30-d2e6890d11a0"), Nom = "Gestion de ressources", Description = "Il faut produire, échanger et dépenser des ressources de façon à atteindre ses objectifs.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-701a-a45b-2f1506627cc3"), Nom = "Programmation d'actions", Description = "Les joueurs choisissent leurs actions à l'avance, parfois en secret, puis les dévoilent et les exécutent.", CreeLe = DonneesInitiales.CreeLe },
            new { Id = new Guid("01a1c7e7-8000-701b-833a-1a0892bf63b4"), Nom = "Campagne et évolution", Description = "Les parties s'enchaînent, et les choix de l'une modifient le jeu pour les suivantes.", CreeLe = DonneesInitiales.CreeLe });
    }
}

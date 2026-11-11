using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeepleZone.Systeme.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class CreationTablesCatalogue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mecanique",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nom = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    bgg_id = table.Column<int>(type: "integer", nullable: true),
                    cree_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mecanique", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mode_jeu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nom = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    cree_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mode_jeu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jeu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nom = table.Column<string>(type: "text", nullable: false),
                    annee_publication = table.Column<short>(type: "smallint", nullable: true),
                    joueurs_min = table.Column<short>(type: "smallint", nullable: true),
                    joueurs_max = table.Column<short>(type: "smallint", nullable: true),
                    joueurs_meilleur = table.Column<short>(type: "smallint", nullable: true),
                    duree_min_minutes = table.Column<short>(type: "smallint", nullable: true),
                    duree_max_minutes = table.Column<short>(type: "smallint", nullable: true),
                    age_min = table.Column<short>(type: "smallint", nullable: true),
                    complexite = table.Column<decimal>(type: "numeric(2,1)", precision: 2, scale: 1, nullable: true),
                    mode_jeu_id = table.Column<Guid>(type: "uuid", nullable: true),
                    editeur = table.Column<string>(type: "text", nullable: true),
                    bgg_id = table.Column<int>(type: "integer", nullable: true),
                    couverture_id = table.Column<Guid>(type: "uuid", nullable: true),
                    couverture_url = table.Column<string>(type: "text", nullable: true),
                    statut = table.Column<string>(type: "text", nullable: false, defaultValue: "en_attente"),
                    cree_par_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cree_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifie_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jeu", x => x.id);
                    table.CheckConstraint("ck_jeu_age_min", "age_min >= 0");
                    table.CheckConstraint("ck_jeu_complexite", "complexite BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_jeu_couverture", "couverture_id IS NULL OR couverture_url IS NULL");
                    table.CheckConstraint("ck_jeu_duree_max_minutes", "duree_max_minutes > 0");
                    table.CheckConstraint("ck_jeu_duree_min_max", "duree_min_minutes IS NULL OR duree_max_minutes IS NULL OR duree_min_minutes <= duree_max_minutes");
                    table.CheckConstraint("ck_jeu_duree_min_minutes", "duree_min_minutes > 0");
                    table.CheckConstraint("ck_jeu_joueurs_max", "joueurs_max >= 1");
                    table.CheckConstraint("ck_jeu_joueurs_meilleur", "joueurs_meilleur >= 1");
                    table.CheckConstraint("ck_jeu_joueurs_meilleur_max", "joueurs_meilleur IS NULL OR joueurs_max IS NULL OR joueurs_meilleur <= joueurs_max");
                    table.CheckConstraint("ck_jeu_joueurs_meilleur_min", "joueurs_meilleur IS NULL OR joueurs_min IS NULL OR joueurs_meilleur >= joueurs_min");
                    table.CheckConstraint("ck_jeu_joueurs_min", "joueurs_min >= 1");
                    table.CheckConstraint("ck_jeu_joueurs_min_max", "joueurs_min IS NULL OR joueurs_max IS NULL OR joueurs_min <= joueurs_max");
                    table.CheckConstraint("ck_jeu_nom", "char_length(nom) BETWEEN 1 AND 255");
                    table.CheckConstraint("ck_jeu_statut", "statut IN ('en_attente', 'valide', 'refuse')");
                    table.ForeignKey(
                        name: "fk_jeu_annexe_couverture_id",
                        column: x => x.couverture_id,
                        principalTable: "annexe",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_jeu_mode_jeu_mode_jeu_id",
                        column: x => x.mode_jeu_id,
                        principalTable: "mode_jeu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_jeu_utilisateur_cree_par_id",
                        column: x => x.cree_par_id,
                        principalTable: "utilisateur",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "jeu_mecanique",
                columns: table => new
                {
                    jeu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mecanique_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jeu_mecanique", x => new { x.jeu_id, x.mecanique_id });
                    table.ForeignKey(
                        name: "fk_jeu_mecanique_jeu_jeu_id",
                        column: x => x.jeu_id,
                        principalTable: "jeu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_jeu_mecanique_mecanique_mecanique_id",
                        column: x => x.mecanique_id,
                        principalTable: "mecanique",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "mecanique",
                columns: new[] { "id", "bgg_id", "cree_le", "description", "nom" },
                values: new object[,]
                {
                    { new Guid("01a1c7e7-8000-7005-bec5-8cc0f7c0f4cf"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Chaque joueur place des pions sur des cases d'action, qui ne peuvent souvent servir qu'une fois par tour.", "Pose d'ouvriers" },
                    { new Guid("01a1c7e7-8000-7006-98b6-f2b0c15611b6"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Chaque joueur part d'un petit paquet de cartes et en achète d'autres pour l'améliorer au fil de la partie.", "Construction de pioche" },
                    { new Guid("01a1c7e7-8000-7007-99bf-876d55d7363c"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les joueurs assemblent des éléments qui se renforcent entre eux et produisent de plus en plus de ressources.", "Construction de moteur" },
                    { new Guid("01a1c7e7-8000-7008-a934-b970b2133b80"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Le défi consiste à choisir quelles cartes jouer, garder ou défausser parmi celles qu'on tient en main.", "Gestion de main" },
                    { new Guid("01a1c7e7-8000-7009-ba51-046f870ca39c"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Chaque joueur choisit une carte parmi plusieurs, puis passe le reste à son voisin.", "Draft de cartes" },
                    { new Guid("01a1c7e7-8000-700a-8b25-b3cebeb27ba4"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Le résultat des dés détermine les actions possibles ou le déroulement des événements.", "Lancer de dés" },
                    { new Guid("01a1c7e7-8000-700b-b27f-b08614a1dfa7"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les joueurs posent des tuiles pour construire un paysage ou une structure, avec des règles d'adjacence.", "Placement de tuiles" },
                    { new Guid("01a1c7e7-8000-700c-8f50-56c207831c05"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Il faut avoir plus de présence que les adversaires dans une zone pour la contrôler et en tirer des points.", "Majorité de zones" },
                    { new Guid("01a1c7e7-8000-700d-bb72-356f51a22f8c"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les joueurs réunissent des ensembles d'objets ou de cartes qui rapportent plus quand ils sont complets.", "Collection de séries" },
                    { new Guid("01a1c7e7-8000-700e-9beb-e12f23687eb7"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les joueurs misent pour obtenir des objets, des actions ou l'ordre du tour.", "Enchères" },
                    { new Guid("01a1c7e7-8000-700f-b915-be3362b46bd9"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les joueurs discutent, échangent des ressources ou concluent des ententes entre eux.", "Échanges et négociation" },
                    { new Guid("01a1c7e7-8000-7010-b25d-e037b6e19408"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Chaque joueur reçoit une identité secrète, et les camps s'affrontent sans se connaître.", "Rôles cachés" },
                    { new Guid("01a1c7e7-8000-7011-89ad-524b7c0b520f"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Il faut recouper des indices pour deviner une information cachée.", "Déduction" },
                    { new Guid("01a1c7e7-8000-7012-9023-6a29bfaf8326"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les joueurs dissimulent ou inventent de l'information pour tromper les autres.", "Bluff" },
                    { new Guid("01a1c7e7-8000-7013-929f-09f2c4e9a2c6"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "On peut continuer à tenter sa chance pour gagner plus, au risque de tout perdre au tour.", "Pousser sa chance" },
                    { new Guid("01a1c7e7-8000-7014-865c-fdf4c2f7d63a"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Chaque joueur dispose de capacités ou de règles qui lui sont propres.", "Pouvoirs variables" },
                    { new Guid("01a1c7e7-8000-7015-96d9-267515323aad"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Le plateau est assemblé à partir d'éléments qui changent d'une partie à l'autre.", "Plateau modulaire" },
                    { new Guid("01a1c7e7-8000-7016-82ae-e33f4a4219c2"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les pions se déplacent sur des cases, des zones ou des chemins selon des règles de mouvement.", "Déplacement sur le plateau" },
                    { new Guid("01a1c7e7-8000-7017-99b9-6516d7a24420"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les joueurs relient des lieux par des routes, des voies ou des liens pour former un réseau.", "Construction de réseaux" },
                    { new Guid("01a1c7e7-8000-7018-8b8c-451f0e5e4d58"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Chaque tour, le joueur dispose d'un nombre limité de points à répartir entre plusieurs actions.", "Points d'action" },
                    { new Guid("01a1c7e7-8000-7019-8c30-d2e6890d11a0"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Il faut produire, échanger et dépenser des ressources de façon à atteindre ses objectifs.", "Gestion de ressources" },
                    { new Guid("01a1c7e7-8000-701a-a45b-2f1506627cc3"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les joueurs choisissent leurs actions à l'avance, parfois en secret, puis les dévoilent et les exécutent.", "Programmation d'actions" },
                    { new Guid("01a1c7e7-8000-701b-833a-1a0892bf63b4"), null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les parties s'enchaînent, et les choix de l'une modifient le jeu pour les suivantes.", "Campagne et évolution" }
                });

            migrationBuilder.InsertData(
                table: "mode_jeu",
                columns: new[] { "id", "cree_le", "description", "nom" },
                values: new object[,]
                {
                    { new Guid("01a1c7e7-8000-7001-adcc-71b334e95c80"), new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Chaque joueur joue pour lui-même et cherche à gagner contre les autres.", "Compétitif" },
                    { new Guid("01a1c7e7-8000-7002-9c8a-87833414cc9b"), new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Tous les joueurs gagnent ou perdent ensemble contre le jeu.", "Coopératif" },
                    { new Guid("01a1c7e7-8000-7003-9c80-f4cb84b0633c"), new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les joueurs coopèrent, mais chacun peut avoir un objectif personnel, ou l'un d'eux peut trahir les autres.", "Semi-coopératif" },
                    { new Guid("01a1c7e7-8000-7004-bac3-3649459c0f35"), new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Les joueurs sont répartis en équipes, et chaque équipe joue contre les autres.", "Par équipes" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_jeu_bgg_id",
                table: "jeu",
                column: "bgg_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_jeu_couverture_id",
                table: "jeu",
                column: "couverture_id");

            migrationBuilder.CreateIndex(
                name: "ix_jeu_cree_par_id",
                table: "jeu",
                column: "cree_par_id");

            migrationBuilder.CreateIndex(
                name: "ix_jeu_mode_jeu_id",
                table: "jeu",
                column: "mode_jeu_id");

            migrationBuilder.CreateIndex(
                name: "ix_jeu_mecanique_mecanique_id",
                table: "jeu_mecanique",
                column: "mecanique_id");

            migrationBuilder.CreateIndex(
                name: "ix_mecanique_bgg_id",
                table: "mecanique",
                column: "bgg_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mecanique_nom",
                table: "mecanique",
                column: "nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mode_jeu_nom",
                table: "mode_jeu",
                column: "nom",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jeu_mecanique");

            migrationBuilder.DropTable(
                name: "jeu");

            migrationBuilder.DropTable(
                name: "mecanique");

            migrationBuilder.DropTable(
                name: "mode_jeu");
        }
    }
}

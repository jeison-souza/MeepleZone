using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class CreationTablesAmities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "amitie",
                columns: table => new
                {
                    utilisateur_a_id = table.Column<Guid>(type: "uuid", nullable: false),
                    utilisateur_b_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cree_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_amitie", x => new { x.utilisateur_a_id, x.utilisateur_b_id });
                    table.CheckConstraint("ck_amitie_utilisateurs_ordre", "utilisateur_a_id < utilisateur_b_id");
                    table.ForeignKey(
                        name: "fk_amitie_utilisateur_utilisateur_a_id",
                        column: x => x.utilisateur_a_id,
                        principalTable: "utilisateur",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_amitie_utilisateur_utilisateur_b_id",
                        column: x => x.utilisateur_b_id,
                        principalTable: "utilisateur",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "demande_amitie",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    demandeur_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destinataire_id = table.Column<Guid>(type: "uuid", nullable: true),
                    courriel_destinataire = table.Column<string>(type: "text", nullable: true, collation: "courriel_ci"),
                    statut = table.Column<string>(type: "text", nullable: false, defaultValue: "en_attente"),
                    cree_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifie_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_demande_amitie", x => x.id);
                    table.CheckConstraint("ck_demande_amitie_demandeur_distinct", "demandeur_id <> destinataire_id");
                    table.CheckConstraint("ck_demande_amitie_destinataire", "(destinataire_id IS NULL) <> (courriel_destinataire IS NULL)");
                    table.CheckConstraint("ck_demande_amitie_statut", "statut IN ('en_attente', 'ignoree', 'rejetee')");
                    table.ForeignKey(
                        name: "fk_demande_amitie_utilisateur_demandeur_id",
                        column: x => x.demandeur_id,
                        principalTable: "utilisateur",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_demande_amitie_utilisateur_destinataire_id",
                        column: x => x.destinataire_id,
                        principalTable: "utilisateur",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_amitie_utilisateur_b_id",
                table: "amitie",
                column: "utilisateur_b_id");

            migrationBuilder.CreateIndex(
                name: "ix_demande_amitie_courriel_destinataire",
                table: "demande_amitie",
                column: "courriel_destinataire");

            migrationBuilder.CreateIndex(
                name: "ix_demande_amitie_demandeur_id_courriel_destinataire",
                table: "demande_amitie",
                columns: new[] { "demandeur_id", "courriel_destinataire" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_demande_amitie_demandeur_id_destinataire_id",
                table: "demande_amitie",
                columns: new[] { "demandeur_id", "destinataire_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_demande_amitie_destinataire_id",
                table: "demande_amitie",
                column: "destinataire_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "amitie");

            migrationBuilder.DropTable(
                name: "demande_amitie");
        }
    }
}

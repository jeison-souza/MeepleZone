using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class CreationTableDemandeVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "demande_verification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    utilisateur_id = table.Column<Guid>(type: "uuid", nullable: false),
                    courriel = table.Column<string>(type: "text", nullable: false, collation: "courriel_ci"),
                    cree_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    envoye_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    duree_validite = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_demande_verification", x => x.id);
                    table.CheckConstraint("ck_demande_verification_courriel", "courriel = btrim(courriel) AND courriel <> ''");
                    table.CheckConstraint("ck_demande_verification_duree_validite", "duree_validite > interval '0'");
                    table.ForeignKey(
                        name: "fk_demande_verification_utilisateur_utilisateur_id",
                        column: x => x.utilisateur_id,
                        principalTable: "utilisateur",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_demande_verification_courriel",
                table: "demande_verification",
                column: "courriel");

            migrationBuilder.CreateIndex(
                name: "ix_demande_verification_utilisateur_id",
                table: "demande_verification",
                column: "utilisateur_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "demande_verification");
        }
    }
}

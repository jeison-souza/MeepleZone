using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class AjoutCodeDestinataireDemandeAmitie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_demande_amitie_destinataire",
                table: "demande_amitie");

            migrationBuilder.AddColumn<string>(
                name: "code_destinataire",
                table: "demande_amitie",
                type: "text",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_demande_amitie_code_rattache",
                table: "demande_amitie",
                sql: "code_destinataire IS NULL OR destinataire_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_demande_amitie_destinataire",
                table: "demande_amitie",
                sql: "(courriel_destinataire IS NULL) <> (code_destinataire IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_demande_amitie_code_rattache",
                table: "demande_amitie");

            migrationBuilder.DropCheckConstraint(
                name: "ck_demande_amitie_destinataire",
                table: "demande_amitie");

            migrationBuilder.DropColumn(
                name: "code_destinataire",
                table: "demande_amitie");

            migrationBuilder.AddCheckConstraint(
                name: "ck_demande_amitie_destinataire",
                table: "demande_amitie",
                sql: "(destinataire_id IS NULL) <> (courriel_destinataire IS NULL)");
        }
    }
}

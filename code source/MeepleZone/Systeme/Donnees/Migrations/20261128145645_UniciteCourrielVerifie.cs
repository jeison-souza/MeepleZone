using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class UniciteCourrielVerifie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_utilisateur_courriel",
                table: "utilisateur");

            migrationBuilder.CreateIndex(
                name: "ix_utilisateur_courriel",
                table: "utilisateur",
                column: "courriel",
                unique: true,
                filter: "courriel_verifie_le IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_utilisateur_courriel",
                table: "utilisateur");

            migrationBuilder.CreateIndex(
                name: "ix_utilisateur_courriel",
                table: "utilisateur",
                column: "courriel",
                unique: true);
        }
    }
}

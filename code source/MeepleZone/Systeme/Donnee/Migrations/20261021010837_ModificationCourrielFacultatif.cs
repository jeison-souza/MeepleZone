using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnee.Migrations
{
    /// <inheritdoc />
    public partial class ModificationCourrielFacultatif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "courriel",
                table: "utilisateur",
                type: "text",
                nullable: true,
                collation: "courriel_ci",
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "courriel_ci");

            migrationBuilder.AlterColumn<string>(
                name: "courriel",
                table: "identite_externe",
                type: "text",
                nullable: true,
                collation: "courriel_ci",
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "courriel_ci");

            migrationBuilder.AddCheckConstraint(
                name: "ck_utilisateur_courriel",
                table: "utilisateur",
                sql: "courriel = btrim(courriel) AND courriel <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "ck_identite_externe_courriel",
                table: "identite_externe",
                sql: "courriel = btrim(courriel) AND courriel <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_utilisateur_courriel",
                table: "utilisateur");

            migrationBuilder.DropCheckConstraint(
                name: "ck_identite_externe_courriel",
                table: "identite_externe");

            migrationBuilder.AlterColumn<string>(
                name: "courriel",
                table: "utilisateur",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "courriel_ci",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "courriel_ci");

            migrationBuilder.AlterColumn<string>(
                name: "courriel",
                table: "identite_externe",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "courriel_ci",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "courriel_ci");
        }
    }
}

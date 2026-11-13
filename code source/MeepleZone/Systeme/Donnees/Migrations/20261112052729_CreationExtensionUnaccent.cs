using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class CreationExtensionUnaccent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:courriel_ci", "und-u-ks-level2,und-u-ks-level2,icu,False")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .OldAnnotation("Npgsql:CollationDefinition:courriel_ci", "und-u-ks-level2,und-u-ks-level2,icu,False");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:courriel_ci", "und-u-ks-level2,und-u-ks-level2,icu,False")
                .OldAnnotation("Npgsql:CollationDefinition:courriel_ci", "und-u-ks-level2,und-u-ks-level2,icu,False")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }
    }
}

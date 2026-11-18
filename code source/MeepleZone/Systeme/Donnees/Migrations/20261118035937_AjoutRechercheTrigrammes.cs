using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class AjoutRechercheTrigrammes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:courriel_ci", "und-u-ks-level2,und-u-ks-level2,icu,False")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .OldAnnotation("Npgsql:CollationDefinition:courriel_ci", "und-u-ks-level2,und-u-ks-level2,icu,False")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");

            // Seule instruction SQL manuscrite des migrations (ADR 0015, écart assumé à l'ADR 0007) : PostgreSQL refuse
            // dans une colonne calculée ou un index une fonction qui n'est pas immuable, et unaccent ne l'est pas.
            // Cette enveloppe la déclare immuable, avec un dictionnaire et un schéma explicites. Si le dictionnaire
            // d'unaccent change (mise à jour du serveur), l'index doit être reconstruit.
            migrationBuilder.Sql("""
                CREATE FUNCTION unaccent_immuable(texte text) RETURNS text
                LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT
                AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, texte) $$;
                """);

            migrationBuilder.AddColumn<string>(
                name: "nom_recherche",
                table: "jeu",
                type: "text",
                nullable: false,
                computedColumnSql: "lower(unaccent_immuable(nom))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_jeu_nom_recherche",
                table: "jeu",
                column: "nom_recherche")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_jeu_nom_recherche",
                table: "jeu");

            migrationBuilder.DropColumn(
                name: "nom_recherche",
                table: "jeu");

            migrationBuilder.Sql("DROP FUNCTION unaccent_immuable(text);");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:courriel_ci", "und-u-ks-level2,und-u-ks-level2,icu,False")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .OldAnnotation("Npgsql:CollationDefinition:courriel_ci", "und-u-ks-level2,und-u-ks-level2,icu,False")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }
    }
}

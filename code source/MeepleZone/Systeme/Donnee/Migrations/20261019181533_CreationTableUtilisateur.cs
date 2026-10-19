using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnee.Migrations
{
    /// <inheritdoc />
    public partial class CreationTableUtilisateur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:courriel_ci", "und-u-ks-level2,und-u-ks-level2,icu,False");

            migrationBuilder.CreateTable(
                name: "utilisateur",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    courriel = table.Column<string>(type: "text", nullable: false, collation: "courriel_ci"),
                    courriel_verifie_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    nom_affichage = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false, defaultValue: "membre"),
                    photo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code_amitie = table.Column<string>(type: "text", nullable: false),
                    visibilite_collection = table.Column<string>(type: "text", nullable: false, defaultValue: "privee"),
                    cree_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifie_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_utilisateur", x => x.id);
                    table.CheckConstraint("ck_utilisateur_code_amitie", "code_amitie ~ '^[A-HJ-NP-Z2-9]{8}$'");
                    table.CheckConstraint("ck_utilisateur_role", "role IN ('membre', 'administrateur', 'systeme')");
                    table.CheckConstraint("ck_utilisateur_visibilite_collection", "visibilite_collection IN ('privee', 'amis')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_utilisateur_code_amitie",
                table: "utilisateur",
                column: "code_amitie",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_utilisateur_courriel",
                table: "utilisateur",
                column: "courriel",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "utilisateur");
        }
    }
}

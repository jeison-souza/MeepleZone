using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnee.Migrations
{
    /// <inheritdoc />
    public partial class CreationTableIdentiteExterne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "identite_externe",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    utilisateur_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fournisseur = table.Column<string>(type: "text", nullable: false),
                    identifiant_compte_externe = table.Column<string>(type: "text", nullable: false),
                    courriel = table.Column<string>(type: "text", nullable: false, collation: "courriel_ci"),
                    derniere_connexion_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    cree_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifie_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identite_externe", x => x.id);
                    table.ForeignKey(
                        name: "fk_identite_externe_utilisateur_utilisateur_id",
                        column: x => x.utilisateur_id,
                        principalTable: "utilisateur",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_identite_externe_fournisseur_identifiant_compte_externe",
                table: "identite_externe",
                columns: new[] { "fournisseur", "identifiant_compte_externe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identite_externe_utilisateur_id",
                table: "identite_externe",
                column: "utilisateur_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identite_externe");
        }
    }
}

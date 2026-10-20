using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnee.Migrations
{
    /// <inheritdoc />
    public partial class CreationTableAnnexe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "annexe",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nom_fichier = table.Column<string>(type: "text", nullable: false),
                    contenu = table.Column<byte[]>(type: "bytea", nullable: false),
                    type_mime = table.Column<string>(type: "text", nullable: false),
                    taille_octets = table.Column<int>(type: "integer", nullable: false),
                    cree_par_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cree_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_annexe", x => x.id);
                    table.CheckConstraint("ck_annexe_nom_fichier", "char_length(nom_fichier) BETWEEN 1 AND 255");
                    table.CheckConstraint("ck_annexe_taille_octets", "taille_octets > 0");
                    table.ForeignKey(
                        name: "fk_annexe_utilisateur_cree_par_id",
                        column: x => x.cree_par_id,
                        principalTable: "utilisateur",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_utilisateur_photo_id",
                table: "utilisateur",
                column: "photo_id");

            migrationBuilder.CreateIndex(
                name: "ix_annexe_cree_par_id",
                table: "annexe",
                column: "cree_par_id");

            migrationBuilder.AddForeignKey(
                name: "fk_utilisateur_annexe_photo_id",
                table: "utilisateur",
                column: "photo_id",
                principalTable: "annexe",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_utilisateur_annexe_photo_id",
                table: "utilisateur");

            migrationBuilder.DropTable(
                name: "annexe");

            migrationBuilder.DropIndex(
                name: "ix_utilisateur_photo_id",
                table: "utilisateur");
        }
    }
}

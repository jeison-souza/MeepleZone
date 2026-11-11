using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class CreationTablePossession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "possession",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    utilisateur_id = table.Column<Guid>(type: "uuid", nullable: false),
                    jeu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    note = table.Column<short>(type: "smallint", nullable: true),
                    commentaire = table.Column<string>(type: "text", nullable: true),
                    parties_jouees = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    cree_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifie_le = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_possession", x => x.id);
                    table.CheckConstraint("ck_possession_note", "note BETWEEN 1 AND 10");
                    table.CheckConstraint("ck_possession_parties_jouees", "parties_jouees >= 0");
                    table.ForeignKey(
                        name: "fk_possession_jeu_jeu_id",
                        column: x => x.jeu_id,
                        principalTable: "jeu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_possession_utilisateur_utilisateur_id",
                        column: x => x.utilisateur_id,
                        principalTable: "utilisateur",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_possession_jeu_id",
                table: "possession",
                column: "jeu_id");

            migrationBuilder.CreateIndex(
                name: "ix_possession_utilisateur_id_jeu_id",
                table: "possession",
                columns: new[] { "utilisateur_id", "jeu_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "possession");
        }
    }
}

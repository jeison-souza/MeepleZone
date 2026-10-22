using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeepleZone.Systeme.Donnee.Migrations
{
    /// <inheritdoc />
    public partial class AjoutIndexCourrielIdentiteExterne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_identite_externe_courriel",
                table: "identite_externe",
                column: "courriel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_identite_externe_courriel",
                table: "identite_externe");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeepleZone.Systeme.Donnees.Migrations
{
    /// <inheritdoc />
    public partial class AjoutComptesTechniques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "utilisateur",
                columns: new[] { "id", "code_amitie", "courriel", "courriel_verifie_le", "cree_le", "modifie_le", "nom_affichage", "photo_id", "role", "visibilite_collection" },
                values: new object[,]
                {
                    { new Guid("01a1c7e7-8000-7101-8000-000000000001"), "SYSTEME2", "systeme@meeplezone.ca", null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Système", null, "systeme", "privee" },
                    { new Guid("01a1c7e7-8000-7102-8000-000000000002"), "EFFACE22", "utilisateur-supprime@meeplezone.ca", null, new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Utilisateur supprimé", null, "systeme", "privee" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "utilisateur",
                keyColumn: "id",
                keyValue: new Guid("01a1c7e7-8000-7101-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "utilisateur",
                keyColumn: "id",
                keyValue: new Guid("01a1c7e7-8000-7102-8000-000000000002"));
        }
    }
}

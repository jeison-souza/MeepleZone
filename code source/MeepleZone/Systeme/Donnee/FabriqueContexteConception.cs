using MeepleZone.Systeme.Donnee.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeepleZone.Systeme.Donnee;

/// <summary>
/// Utilisée seulement par les outils de EF Core (<c>dotnet ef migrations add</c>, <c>migrations script</c>)
/// pour créer le contexte sans le site. Générer une migration n'ouvre aucune connexion :
/// la chaîne ci-dessous est factice et ne contient aucun secret.
/// </summary>
internal sealed class FabriqueContexteConception : IDesignTimeDbContextFactory<MeepleZoneContexte>
{
    public MeepleZoneContexte CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MeepleZoneContexte>()
            .UtiliserPostgreSql("Host=hote-de-conception")
            .Options;

        return new MeepleZoneContexte(options);
    }
}

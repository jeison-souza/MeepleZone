using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace MeepleZone.Site.Versionnage;

/// <summary>
/// Route <c>/version</c> : le numéro seul, en texte brut. Le pipeline de déploiement le compare au numéro construit,
/// d'où l'interdiction de mise en cache.
/// </summary>
public static class GestionVersion
{
    public const string Route = "/version";

    public static IResult Traiter(HttpResponse reponse, InformationsVersion version)
    {
        reponse.Headers[HeaderNames.CacheControl] = "no-store";

        return Results.Text(version.Numero);
    }
}

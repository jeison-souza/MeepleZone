using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using System.Security.Claims;

namespace MeepleZone.Site.Authentification.Extensions;

public static class MicrosoftAccountOptionsExtensions
{
    public static MicrosoftAccountOptions ConfigurerRevendications(this MicrosoftAccountOptions options)
    {
        options.ClaimActions.Clear();
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "displayName");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "mail");

        return options;
    }
}

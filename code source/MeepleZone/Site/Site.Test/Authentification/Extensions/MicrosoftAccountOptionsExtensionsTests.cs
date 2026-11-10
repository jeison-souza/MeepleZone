using FluentAssertions;
using MeepleZone.Site.Authentification.Extensions;
using MeepleZone.Systeme.BaseTest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Xunit;

namespace MeepleZone.Site.Test.Authentification.Extensions;

public class MicrosoftAccountOptionsExtensionsTests : BaseTest<MicrosoftAccountOptions>
{
    private const string Emetteur = "Microsoft";

    [Fact]
    public void ConfigurerRevendications_IdNomEtMailPresents_CreerLesTroisRevendications()
    {
        // Arrange
        var id = CreerFixture<string>();
        var nomAffichage = CreerFixture<string>();
        var courriel = CreerFixture<string>();
        var json = CreerJson(new Dictionary<string, string?>
        {
            ["id"] = id,
            ["displayName"] = nomAffichage,
            ["mail"] = courriel,
            ["userPrincipalName"] = CreerFixture<string>()
        });

        // Act
        var identite = ExecuterActions(json);

        // Assert
        ValeursDe(identite, ClaimTypes.NameIdentifier).Should().Equal(id);
        ValeursDe(identite, ClaimTypes.Name).Should().Equal(nomAffichage);
        ValeursDe(identite, ClaimTypes.Email).Should().Equal(courriel);
    }

    [Fact]
    public void ConfigurerRevendications_MailAbsentEtUserPrincipalNamePresent_NePasCreerRevendicationCourriel()
    {
        // Arrange
        var json = CreerJson(new Dictionary<string, string?>
        {
            ["id"] = CreerFixture<string>(),
            ["displayName"] = CreerFixture<string>(),
            ["userPrincipalName"] = CreerFixture<string>()
        });

        // Act
        var identite = ExecuterActions(json);

        // Assert
        ValeursDe(identite, ClaimTypes.Email).Should().BeEmpty();
    }

    [Fact]
    public void ConfigurerRevendications_MailNull_NePasCreerRevendicationCourriel()
    {
        // Arrange
        var json = CreerJson(new Dictionary<string, string?>
        {
            ["id"] = CreerFixture<string>(),
            ["displayName"] = CreerFixture<string>(),
            ["mail"] = null,
            ["userPrincipalName"] = CreerFixture<string>()
        });

        // Act
        var identite = ExecuterActions(json);

        // Assert
        ValeursDe(identite, ClaimTypes.Email).Should().BeEmpty();
    }

    [Fact]
    public void ConfigurerRevendications_PrenomEtNomDeFamille_NePasCreerDAutresRevendications()
    {
        // Arrange
        var json = CreerJson(new Dictionary<string, string?>
        {
            ["id"] = CreerFixture<string>(),
            ["displayName"] = CreerFixture<string>(),
            ["givenName"] = CreerFixture<string>(),
            ["surname"] = CreerFixture<string>(),
            ["mail"] = CreerFixture<string>()
        });

        // Act
        var identite = ExecuterActions(json);

        // Assert
        identite.Claims.Select(c => c.Type).Should().BeEquivalentTo(
            [ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Email]);
    }

    private static string CreerJson(Dictionary<string, string?> proprietes)
    {
        return JsonSerializer.Serialize(proprietes);
    }

    private ClaimsIdentity ExecuterActions(string json)
    {
        var options = InstanceTest.ConfigurerRevendications();
        var identite = new ClaimsIdentity();

        using var document = JsonDocument.Parse(json);
        foreach (var action in options.ClaimActions)
        {
            action.Run(document.RootElement, identite, Emetteur);
        }

        return identite;
    }

    private static string[] ValeursDe(ClaimsIdentity identite, string type)
    {
        return identite.FindAll(type).Select(c => c.Value).ToArray();
    }
}

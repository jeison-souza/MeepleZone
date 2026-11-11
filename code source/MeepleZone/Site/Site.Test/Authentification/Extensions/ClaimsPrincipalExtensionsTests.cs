using System;
using System.Security.Claims;
using FluentAssertions;
using MeepleZone.Site.Authentification.Extensions;
using Xunit;

namespace MeepleZone.Site.Test.Authentification.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void Lecture_AvecRevendicationsDuCompte_RetourneLesValeurs()
    {
        // Arrange
        var id = Guid.Parse("3f2b8c1e-5a47-4d0e-9b6a-1c8d7e2f4a90");
        var utilisateur = Creer(
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Name, "Marie Tremblay"),
            new Claim(ClaimTypes.Email, "marie@exemple.ca"));

        // Act & Assert
        utilisateur.Id().Should().Be(id);
        utilisateur.NomAffichage().Should().Be("Marie Tremblay");
        utilisateur.Courriel().Should().Be("marie@exemple.ca");
    }

    [Fact]
    public void Lecture_SansRevendications_RetourneNull()
    {
        // Arrange
        var utilisateur = Creer();

        // Act & Assert
        utilisateur.Id().Should().BeNull();
        utilisateur.NomAffichage().Should().BeNull();
        utilisateur.Courriel().Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("pas-un-guid")]
    public void Id_AvecValeurQuiNEstPasUnGuid_RetourneNull(string valeur)
    {
        // Arrange
        var utilisateur = Creer(new Claim(ClaimTypes.NameIdentifier, valeur));

        // Act
        var id = utilisateur.Id();

        // Assert
        id.Should().BeNull();
    }

    private static ClaimsPrincipal Creer(params Claim[] revendications)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(revendications, "test"));
    }
}

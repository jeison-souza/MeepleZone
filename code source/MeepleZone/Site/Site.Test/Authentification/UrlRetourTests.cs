using FluentAssertions;
using MeepleZone.Site.Authentification;
using Xunit;

namespace MeepleZone.Site.Test.Authentification;

public class UrlRetourTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/profil")]
    [InlineData("/profil?onglet=2")]
    [InlineData("/catalogue/jeux/42#regles")]
    public void Valider_AvecCheminLocal_ConserveLAdresse(string url)
    {
        // Act
        var resultat = UrlRetour.Valider(url);

        // Assert
        resultat.Should().Be(url);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" /profil")]
    [InlineData("profil")]
    [InlineData("//evil.com")]
    [InlineData("/\\evil.com")]
    [InlineData("\\\\evil.com")]
    [InlineData("https://evil.com")]
    [InlineData("http://evil.com/profil")]
    [InlineData("javascript:alert(1)")]
    [InlineData("/profil\r\nSet-Cookie: a=1")]
    [InlineData("/dossier\\fichier")]
    public void Valider_AvecAdresseDangereuseOuInvalide_RetourneAccueil(string? url)
    {
        // Act
        var resultat = UrlRetour.Valider(url);

        // Assert
        resultat.Should().Be(UrlRetour.Defaut);
    }

    [Theory]
    [InlineData("/connexion/Google")]
    [InlineData("/connexion/Microsoft?retour=/profil")]
    [InlineData("/connexion-requise")]
    [InlineData("/Connexion-Requise?motif=erreur")]
    [InlineData("/deconnexion")]
    public void Valider_AvecPageDeConnexion_RetourneAccueil(string url)
    {
        // Act
        var resultat = UrlRetour.Valider(url);

        // Assert
        resultat.Should().Be(UrlRetour.Defaut);
    }

    [Fact]
    public void Valider_AvecAdresseTropLongue_RetourneAccueil()
    {
        // Arrange
        var url = "/" + new string('a', 600);

        // Act
        var resultat = UrlRetour.Valider(url);

        // Assert
        resultat.Should().Be(UrlRetour.Defaut);
    }
}

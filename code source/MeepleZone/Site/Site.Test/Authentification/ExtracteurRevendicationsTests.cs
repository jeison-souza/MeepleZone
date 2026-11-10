using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Site.Authentification;
using MeepleZone.Systeme.BaseTest;
using System;
using System.Security.Claims;
using Xunit;

namespace MeepleZone.Site.Test.Authentification;

public class ExtracteurRevendicationsTests : BaseTest<ExtracteurRevendications>
{
    private const string MessageIdentifiantRequis = "L'identifiant du compte externe est requis.";

    [Fact]
    public void Extraire_ToutesLesClaimsPresents_RetournerRequeteComplete()
    {
        // Arrange
        var fournisseur = CreerFixture<string>();
        var identifiantExterne = CreerFixture<string>();
        var courriel = CreerFixture<string>();
        var nomAffichage = CreerFixture<string>();
        var resultatAttendu = new GererConnexionRequete(fournisseur, identifiantExterne, courriel, nomAffichage, false);
        var principal = CreerPrincipal(
            fournisseur,
            new Claim(ClaimTypes.NameIdentifier, identifiantExterne),
            new Claim(ClaimTypes.Email, courriel),
            new Claim(ClaimTypes.Name, nomAffichage));

        // Act
        var resultat = InstanceTest.Extraire(fournisseur, principal, false);

        // Assert
        resultat.Should().Be(resultatAttendu);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Extraire_ForcerCreation_RepasserLaValeurALaRequete(bool forcerCreation)
    {
        // Arrange
        var fournisseur = CreerFixture<string>();
        var principal = CreerPrincipal(
            fournisseur,
            new Claim(ClaimTypes.NameIdentifier, CreerFixture<string>()));

        // Act
        var resultat = InstanceTest.Extraire(fournisseur, principal, forcerCreation);

        // Assert
        resultat.ForcerCreation.Should().Be(forcerCreation);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Extraire_CourrielVideOuEspaces_RetournerRequeteAvecCourrielNull(string courriel)
    {
        // Arrange
        var fournisseur = CreerFixture<string>();
        var identifiantExterne = CreerFixture<string>();
        var nomAffichage = CreerFixture<string>();
        var resultatAttendu = new GererConnexionRequete(fournisseur, identifiantExterne, null, nomAffichage, false);
        var principal = CreerPrincipal(
            fournisseur,
            new Claim(ClaimTypes.NameIdentifier, identifiantExterne),
            new Claim(ClaimTypes.Email, courriel),
            new Claim(ClaimTypes.Name, nomAffichage));

        // Act
        var resultat = InstanceTest.Extraire(fournisseur, principal, false);

        // Assert
        resultat.Should().Be(resultatAttendu);
    }

    [Fact]
    public void Extraire_ClaimCourrielAbsente_RetournerRequeteAvecCourrielNull()
    {
        // Arrange
        var fournisseur = CreerFixture<string>();
        var identifiantExterne = CreerFixture<string>();
        var nomAffichage = CreerFixture<string>();
        var resultatAttendu = new GererConnexionRequete(fournisseur, identifiantExterne, null, nomAffichage, false);
        var principal = CreerPrincipal(
            fournisseur,
            new Claim(ClaimTypes.NameIdentifier, identifiantExterne),
            new Claim(ClaimTypes.Name, nomAffichage));

        // Act
        var resultat = InstanceTest.Extraire(fournisseur, principal, false);

        // Assert
        resultat.Should().Be(resultatAttendu);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Extraire_NomAffichageVideOuEspaces_RetournerRequeteAvecNomAffichageNull(string nomAffichage)
    {
        // Arrange
        var fournisseur = CreerFixture<string>();
        var identifiantExterne = CreerFixture<string>();
        var courriel = CreerFixture<string>();
        var resultatAttendu = new GererConnexionRequete(fournisseur, identifiantExterne, courriel, null, false);
        var principal = CreerPrincipal(
            fournisseur,
            new Claim(ClaimTypes.NameIdentifier, identifiantExterne),
            new Claim(ClaimTypes.Email, courriel),
            new Claim(ClaimTypes.Name, nomAffichage));

        // Act
        var resultat = InstanceTest.Extraire(fournisseur, principal, false);

        // Assert
        resultat.Should().Be(resultatAttendu);
    }

    [Fact]
    public void Extraire_ClaimNomAffichageAbsente_RetournerRequeteAvecNomAffichageNull()
    {
        // Arrange
        var fournisseur = CreerFixture<string>();
        var identifiantExterne = CreerFixture<string>();
        var courriel = CreerFixture<string>();
        var resultatAttendu = new GererConnexionRequete(fournisseur, identifiantExterne, courriel, null, false);
        var principal = CreerPrincipal(
            fournisseur,
            new Claim(ClaimTypes.NameIdentifier, identifiantExterne),
            new Claim(ClaimTypes.Email, courriel));

        // Act
        var resultat = InstanceTest.Extraire(fournisseur, principal, false);

        // Assert
        resultat.Should().Be(resultatAttendu);
    }

    [Fact]
    public void Extraire_ClaimsNomAffichageEtCourrielAbsentes_RetournerRequeteAvecNomAffichageNullEtCourrielNull()
    {
        // Arrange
        var fournisseur = CreerFixture<string>();
        var identifiantExterne = CreerFixture<string>();
        var resultatAttendu = new GererConnexionRequete(fournisseur, identifiantExterne, null, null, false);
        var principal = CreerPrincipal(
            fournisseur,
            new Claim(ClaimTypes.NameIdentifier, identifiantExterne));

        // Act
        var resultat = InstanceTest.Extraire(fournisseur, principal, false);

        // Assert
        resultat.Should().Be(resultatAttendu);
    }

    [Fact]
    public void Extraire_ClaimsDAutresTypes_IgnorerLesClaimsEtRetournerNomAffichageNullEtCourrielNull()
    {
        // Arrange
        var fournisseur = CreerFixture<string>();
        var identifiantExterne = CreerFixture<string>();
        var resultatAttendu = new GererConnexionRequete(fournisseur, identifiantExterne, null, null, false);
        var principal = CreerPrincipal(
            fournisseur,
            new Claim(ClaimTypes.NameIdentifier, identifiantExterne),
            new Claim(ClaimTypes.Role, CreerFixture<string>()),
            new Claim(ClaimTypes.Surname, CreerFixture<string>()));

        // Act
        var resultat = InstanceTest.Extraire(fournisseur, principal, false);

        // Assert
        resultat.Should().Be(resultatAttendu);
    }

    [Fact]
    public void Extraire_ClaimNameIdentifierAbsente_LancerException()
    {
        // Arrange
        var fournisseur = CreerFixture<string>();
        var principal = CreerPrincipal(
            fournisseur,
            new Claim(ClaimTypes.Email, CreerFixture<string>()),
            new Claim(ClaimTypes.Name, CreerFixture<string>()));

        // Act
        Action acte = () => InstanceTest.Extraire(fournisseur, principal, false);

        // Assert
        acte.Should().Throw<InvalidOperationException>()
            .WithMessage(MessageIdentifiantRequis);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Extraire_ClaimNameIdentifierVideOuEspaces_LancerException(string identifiantExterne)
    {
        // Arrange
        var fournisseur = CreerFixture<string>();
        var principal = CreerPrincipal(
            fournisseur,
            new Claim(ClaimTypes.NameIdentifier, identifiantExterne),
            new Claim(ClaimTypes.Email, CreerFixture<string>()),
            new Claim(ClaimTypes.Name, CreerFixture<string>()));

        // Act
        Action acte = () => InstanceTest.Extraire(fournisseur, principal, false);

        // Assert
        acte.Should().Throw<InvalidOperationException>()
            .WithMessage(MessageIdentifiantRequis);
    }

    private static ClaimsPrincipal CreerPrincipal(string fournisseur, params Claim[] revendications)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(revendications, fournisseur));
    }
}

using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Site.Authentification;
using MeepleZone.Systeme.BaseTest;
using System;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace MeepleZone.Site.Test.Authentification;

public class ConstructeurIdentiteTests : BaseTest<ConstructeurIdentite>
{
    [Fact]
    public void Construire_UtilisateurComplet_CreerLesQuatreRevendications()
    {
        // Arrange
        var utilisateur = new UtilisateurConnecteDto(
            CreerFixture<Guid>(),
            CreerFixture<string>(),
            CreerFixture<string>(),
            CreerFixture<string>());
        var nomSchema = CreerFixture<string>();

        // Act
        var principal = InstanceTest.Construire(utilisateur, nomSchema);

        // Assert
        principal.FindAll(ClaimTypes.NameIdentifier).Select(c => c.Value).Should().Equal(utilisateur.Id.ToString());
        principal.FindAll(ClaimTypes.Name).Select(c => c.Value).Should().Equal(utilisateur.NomAffichage);
        principal.FindAll(ClaimTypes.Email).Select(c => c.Value).Should().Equal(utilisateur.Courriel);
        principal.FindAll(ClaimTypes.Role).Select(c => c.Value).Should().Equal(utilisateur.Role);
    }

    [Fact]
    public void Construire_CourrielNull_NePasCreerRevendicationCourriel()
    {
        // Arrange
        var utilisateur = new UtilisateurConnecteDto(
            CreerFixture<Guid>(),
            CreerFixture<string>(),
            null,
            CreerFixture<string>());

        // Act
        var principal = InstanceTest.Construire(utilisateur, CreerFixture<string>());

        // Assert
        principal.FindAll(ClaimTypes.Email).Should().BeEmpty();
    }

    [Fact]
    public void Construire_UtilisateurValide_CreerIdentiteAuthentifieeAvecLeSchemaDonne()
    {
        // Arrange
        var utilisateur = new UtilisateurConnecteDto(
            CreerFixture<Guid>(),
            CreerFixture<string>(),
            CreerFixture<string>(),
            CreerFixture<string>());
        var nomSchema = CreerFixture<string>();

        // Act
        var principal = InstanceTest.Construire(utilisateur, nomSchema);

        // Assert
        principal.Identity!.IsAuthenticated.Should().BeTrue();
        principal.Identity.AuthenticationType.Should().Be(nomSchema);
    }

    [Fact]
    public void Construire_UtilisateurValide_ExposerLeNomEtLeRoleDeLIdentite()
    {
        // Arrange
        var utilisateur = new UtilisateurConnecteDto(
            CreerFixture<Guid>(),
            CreerFixture<string>(),
            CreerFixture<string>(),
            CreerFixture<string>());

        // Act
        var principal = InstanceTest.Construire(utilisateur, CreerFixture<string>());

        // Assert
        principal.Identity!.Name.Should().Be(utilisateur.NomAffichage);
        principal.IsInRole(utilisateur.Role).Should().BeTrue();
    }

    [Fact]
    public void Construire_UtilisateurComplet_NeCreerQueLesQuatreTypesDeRevendications()
    {
        // Arrange
        var utilisateur = new UtilisateurConnecteDto(
            CreerFixture<Guid>(),
            CreerFixture<string>(),
            CreerFixture<string>(),
            CreerFixture<string>());

        // Act
        var principal = InstanceTest.Construire(utilisateur, CreerFixture<string>());

        // Assert
        principal.Claims.Select(c => c.Type).Should().BeEquivalentTo(
            [ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Email, ClaimTypes.Role]);
    }
}

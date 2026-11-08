using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Extensions;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Entites;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Extensions;

public class UtilisateurExtensionsTest : BaseTest<Utilisateur>
{
    [Fact]
    public void VersDto_EnToutTemps_RetournerUtilisateurConnecteDto()
    {
        // Arrange
        var utilisateurAttendue = new UtilisateurConnecteDto(
            Id: InstanceTest.Id,
            NomAffichage: InstanceTest.NomAffichage,
            Courriel: InstanceTest.Courriel,
            Role: InstanceTest.Role);

        // Act
        var result = InstanceTest.VersDto();

        // Assert
        result.Should().BeEquivalentTo(utilisateurAttendue);
    }
}

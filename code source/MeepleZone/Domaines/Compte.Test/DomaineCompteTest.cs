using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test;

public class DomaineCompteTest : BaseTest<DomaineCompte>
{
    private Fake<IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete>> _gestionnaireGererConnexion = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _gestionnaireGererConnexion = CreerFausse<IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete>>();
    }

    [Fact]
    public async Task GererConnexionAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<GererConnexionRequete>();
        var reponseAttendue = CreerFixture<GererConnexionReponse>();
        _gestionnaireGererConnexion.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.GererConnexionAsync(requete);

        // Assert
        _gestionnaireGererConnexion.CallsTo(g => g.TraiterAsync(A<GererConnexionRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }
}

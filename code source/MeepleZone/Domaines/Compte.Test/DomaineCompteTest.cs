using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test;

public class DomaineCompteTest : BaseTest<DomaineCompte>
{
    private Fake<IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete>> _gestionnaireGererConnexion = null!;

    private Fake<IGestionnaireRequete<IReadOnlyList<ConnexionDto>, ListerConnexionsRequete>> _gestionnaireListerConnexions = null!;

    private Fake<IGestionnaireRequete<LierFournisseurReponse, LierFournisseurRequete>> _gestionnaireLierFournisseur = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _gestionnaireLierFournisseur = CreerFausse<IGestionnaireRequete<LierFournisseurReponse, LierFournisseurRequete>>();
        _gestionnaireGererConnexion = CreerFausse<IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete>>();
        _gestionnaireListerConnexions = CreerFausse<IGestionnaireRequete<IReadOnlyList<ConnexionDto>, ListerConnexionsRequete>>();
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

    [Fact]
    public async Task LierFournisseurAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<LierFournisseurRequete>();
        var reponseAttendue = CreerFixture<LierFournisseurReponse>();
        _gestionnaireLierFournisseur.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.LierFournisseurAsync(requete);

        // Assert
        _gestionnaireLierFournisseur.CallsTo(g => g.TraiterAsync(A<LierFournisseurRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }

    [Fact]
    public async Task ListerConnexionsAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<ListerConnexionsRequete>();
        IReadOnlyList<ConnexionDto> reponseAttendue = [.. Fixture.CreateMany<ConnexionDto>(2)];
        _gestionnaireListerConnexions.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.ListerConnexionsAsync(requete);

        // Assert
        _gestionnaireListerConnexions.CallsTo(g => g.TraiterAsync(A<ListerConnexionsRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }
}

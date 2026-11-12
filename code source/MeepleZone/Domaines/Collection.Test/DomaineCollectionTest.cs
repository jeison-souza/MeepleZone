using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Collection.Test;

public class DomaineCollectionTest : BaseTest<DomaineCollection>
{
    private Fake<IGestionnaireRequete<ListerCollectionReponse, ListerCollectionRequete>> _gestionnaireCollection = null!;

    private Fake<IGestionnaireRequete<IReadOnlyList<MecaniqueDto>, ListerMecaniquesRequete>> _gestionnaireMecaniques = null!;

    private Fake<IGestionnaireRequete<IReadOnlyList<ModeJeuDto>, ListerModesJeuRequete>> _gestionnaireModesJeu = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _gestionnaireCollection = CreerFausse<IGestionnaireRequete<ListerCollectionReponse, ListerCollectionRequete>>();
        _gestionnaireMecaniques = CreerFausse<IGestionnaireRequete<IReadOnlyList<MecaniqueDto>, ListerMecaniquesRequete>>();
        _gestionnaireModesJeu = CreerFausse<IGestionnaireRequete<IReadOnlyList<ModeJeuDto>, ListerModesJeuRequete>>();
    }

    [Fact]
    public async Task ListerCollectionAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<ListerCollectionRequete>();
        var reponseAttendue = CreerFixture<ListerCollectionReponse>();
        _gestionnaireCollection.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.ListerCollectionAsync(requete);

        // Assert
        _gestionnaireCollection.CallsTo(g => g.TraiterAsync(A<ListerCollectionRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }

    [Fact]
    public async Task ListerMecaniquesAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<ListerMecaniquesRequete>();
        var reponseAttendue = CreerFixture<IReadOnlyList<MecaniqueDto>>();
        _gestionnaireMecaniques.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.ListerMecaniquesAsync(requete);

        // Assert
        _gestionnaireMecaniques.CallsTo(g => g.TraiterAsync(A<ListerMecaniquesRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }

    [Fact]
    public async Task ListerModesJeuAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<ListerModesJeuRequete>();
        var reponseAttendue = CreerFixture<IReadOnlyList<ModeJeuDto>>();
        _gestionnaireModesJeu.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.ListerModesJeuAsync(requete);

        // Assert
        _gestionnaireModesJeu.CallsTo(g => g.TraiterAsync(A<ListerModesJeuRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }
}

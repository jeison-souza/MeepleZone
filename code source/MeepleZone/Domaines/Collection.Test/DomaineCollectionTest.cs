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

    private Fake<IGestionnaireRequete<RechercherJeuxReponse, RechercherJeuxRequete>> _gestionnaireRechercherJeux = null!;

    private Fake<IGestionnaireRequete<ModifierPossessionReponse, ModifierPossessionRequete>> _gestionnaireModifierPossession = null!;

    private Fake<IGestionnaireRequete<RetirerJeuReponse, RetirerJeuRequete>> _gestionnaireRetirerJeu = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _gestionnaireCollection = CreerFausse<IGestionnaireRequete<ListerCollectionReponse, ListerCollectionRequete>>();
        _gestionnaireMecaniques = CreerFausse<IGestionnaireRequete<IReadOnlyList<MecaniqueDto>, ListerMecaniquesRequete>>();
        _gestionnaireModesJeu = CreerFausse<IGestionnaireRequete<IReadOnlyList<ModeJeuDto>, ListerModesJeuRequete>>();
        _gestionnaireRechercherJeux = CreerFausse<IGestionnaireRequete<RechercherJeuxReponse, RechercherJeuxRequete>>();
        _gestionnaireModifierPossession = CreerFausse<IGestionnaireRequete<ModifierPossessionReponse, ModifierPossessionRequete>>();
        _gestionnaireRetirerJeu = CreerFausse<IGestionnaireRequete<RetirerJeuReponse, RetirerJeuRequete>>();
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

    [Fact]
    public async Task RechercherJeuxAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<RechercherJeuxRequete>();
        var reponseAttendue = CreerFixture<RechercherJeuxReponse>();
        _gestionnaireRechercherJeux.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.RechercherJeuxAsync(requete);

        // Assert
        _gestionnaireRechercherJeux.CallsTo(g => g.TraiterAsync(A<RechercherJeuxRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }

    [Fact]
    public async Task ModifierPossessionAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<ModifierPossessionRequete>();
        var reponseAttendue = CreerFixture<ModifierPossessionReponse>();
        _gestionnaireModifierPossession.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.ModifierPossessionAsync(requete);

        // Assert
        _gestionnaireModifierPossession.CallsTo(g => g.TraiterAsync(A<ModifierPossessionRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }

    [Fact]
    public async Task RetirerJeuAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<RetirerJeuRequete>();
        var reponseAttendue = CreerFixture<RetirerJeuReponse>();
        _gestionnaireRetirerJeu.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.RetirerJeuAsync(requete);

        // Assert
        _gestionnaireRetirerJeu.CallsTo(g => g.TraiterAsync(A<RetirerJeuRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }
}

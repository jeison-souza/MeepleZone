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

    private Fake<IGestionnaireRequete<ProfilDto?, ObtenirProfilRequete>> _gestionnaireObtenirProfil = null!;

    private Fake<IGestionnaireRequete<SupprimerCompteReponse, SupprimerCompteRequete>> _gestionnaireSupprimerCompte = null!;

    private Fake<IGestionnaireRequete<ModifierVisibiliteReponse, ModifierVisibiliteRequete>> _gestionnaireModifierVisibilite = null!;

    private Fake<IGestionnaireRequete<RegenererCodeAmitieReponse, RegenererCodeAmitieRequete>> _gestionnaireRegenererCodeAmitie = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _gestionnaireModifierVisibilite = CreerFausse<IGestionnaireRequete<ModifierVisibiliteReponse, ModifierVisibiliteRequete>>();
        _gestionnaireRegenererCodeAmitie = CreerFausse<IGestionnaireRequete<RegenererCodeAmitieReponse, RegenererCodeAmitieRequete>>();
        _gestionnaireSupprimerCompte = CreerFausse<IGestionnaireRequete<SupprimerCompteReponse, SupprimerCompteRequete>>();
        _gestionnaireObtenirProfil = CreerFausse<IGestionnaireRequete<ProfilDto?, ObtenirProfilRequete>>();
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
    public async Task ObtenirProfilAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<ObtenirProfilRequete>();
        var reponseAttendue = CreerFixture<ProfilDto>();
        _gestionnaireObtenirProfil.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.ObtenirProfilAsync(requete);

        // Assert
        _gestionnaireObtenirProfil.CallsTo(g => g.TraiterAsync(A<ObtenirProfilRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }

    [Fact]
    public async Task ModifierVisibiliteAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<ModifierVisibiliteRequete>();
        var reponseAttendue = CreerFixture<ModifierVisibiliteReponse>();
        _gestionnaireModifierVisibilite.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.ModifierVisibiliteAsync(requete);

        // Assert
        _gestionnaireModifierVisibilite.CallsTo(g => g.TraiterAsync(A<ModifierVisibiliteRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }

    [Fact]
    public async Task RegenererCodeAmitieAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<RegenererCodeAmitieRequete>();
        var reponseAttendue = CreerFixture<RegenererCodeAmitieReponse>();
        _gestionnaireRegenererCodeAmitie.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.RegenererCodeAmitieAsync(requete);

        // Assert
        _gestionnaireRegenererCodeAmitie.CallsTo(g => g.TraiterAsync(A<RegenererCodeAmitieRequete>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeSameAs(reponseAttendue);
    }

    [Fact]
    public async Task SupprimerCompteAsync_EnToutTemps_DeleguerAuGestionnaireEtRetournerSaReponse()
    {
        // Arrange
        var requete = CreerFixture<SupprimerCompteRequete>();
        var reponseAttendue = CreerFixture<SupprimerCompteReponse>();
        _gestionnaireSupprimerCompte.CallsTo(g => g.TraiterAsync(requete))
            .Returns(reponseAttendue);

        // Act
        var result = await InstanceTest.SupprimerCompteAsync(requete);

        // Assert
        _gestionnaireSupprimerCompte.CallsTo(g => g.TraiterAsync(A<SupprimerCompteRequete>._))
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

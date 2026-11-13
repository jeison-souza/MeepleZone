using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Collection.Contrats.Enums;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Collection.Test.Gestionnaires;

public class GestionnaireRequeteAjouterJeuTests : BaseTest<GestionnaireRequeteAjouterJeu>
{
    private Fake<IDepotCollection> _depotCollection = null!;

    private Fake<IUtilisateurCourant> _utilisateurCourant = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCollection = CreerFausse<IDepotCollection>();
        _utilisateurCourant = CreerFausse<IUtilisateurCourant>();
    }

    [Fact]
    public async Task TraiterAsync_JeuNonVisible_RetournerJeuIntrouvableSansEcrire()
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var jeuId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.TrouverJeuVisibleAsync(utilisateurId, jeuId))
            .Returns<Jeu?>(null);

        // Act
        var result = await InstanceTest.TraiterAsync(new AjouterJeuRequete(jeuId));

        // Assert
        result.Statut.Should().Be(StatutAjout.JeuIntrouvable);
        result.Item.Should().BeNull();
        _depotCollection
            .CallsTo(x => x.AjouterPossessionAsync(A<Possession>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_JeuAjoute_RetournerAjouteAvecLItem()
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var jeu = CreerFixture<Jeu>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.TrouverJeuVisibleAsync(utilisateurId, jeu.Id))
            .Returns<Jeu?>(jeu);
        Possession? enregistree = null;
        _depotCollection
            .CallsTo(x => x.AjouterPossessionAsync(A<Possession>._))
            .Invokes((Possession p) => enregistree = p)
            .Returns(ResultatAjoutPossession.Ajoutee);

        // Act
        var result = await InstanceTest.TraiterAsync(new AjouterJeuRequete(jeu.Id));

        // Assert
        enregistree.Should().NotBeNull();
        result.Statut.Should().Be(StatutAjout.Ajoute);
        result.Item.Should().BeEquivalentTo(new ItemCollectionDto(
            enregistree!.Id,
            jeu.Id,
            jeu.Nom,
            jeu.AnneePublication,
            jeu.JoueursMin,
            jeu.JoueursMax,
            jeu.DureeMinMinutes,
            jeu.DureeMaxMinutes,
            jeu.CouvertureId,
            jeu.CouvertureUrl,
            null,
            0,
            null,
            jeu.Statut));
    }

    [Fact]
    public async Task TraiterAsync_JeuAjoute_EnvoyerAuDepotUneNouvellePossessionSansJeuRattache()
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var jeu = CreerFixture<Jeu>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.TrouverJeuVisibleAsync(utilisateurId, jeu.Id))
            .Returns<Jeu?>(jeu);
        Possession? enregistree = null;
        Jeu? jeuRattacheALEnvoi = jeu;
        _depotCollection
            .CallsTo(x => x.AjouterPossessionAsync(A<Possession>._))
            .Invokes((Possession p) =>
            {
                enregistree = p;
                jeuRattacheALEnvoi = p.Jeu;
            })
            .Returns(ResultatAjoutPossession.Ajoutee);

        // Act
        await InstanceTest.TraiterAsync(new AjouterJeuRequete(jeu.Id));

        // Assert
        enregistree.Should().NotBeNull();
        enregistree!.UtilisateurId.Should().Be(utilisateurId);
        enregistree.JeuId.Should().Be(jeu.Id);
        enregistree.PartiesJouees.Should().Be(0);
        enregistree.Note.Should().BeNull();
        enregistree.Commentaire.Should().BeNull();
        enregistree.Id.Version.Should().Be(7);
        jeuRattacheALEnvoi.Should().BeNull();
    }

    [Theory]
    [InlineData(ResultatAjoutPossession.DejaPossedee, StatutAjout.DejaPossede)]
    [InlineData(ResultatAjoutPossession.JeuInexistant, StatutAjout.JeuIntrouvable)]
    public async Task TraiterAsync_ResultatDuDepotNonAjoute_RetournerLeStatutCorrespondantSansItem(ResultatAjoutPossession resultatDepot, StatutAjout attendu)
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var jeu = CreerFixture<Jeu>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.TrouverJeuVisibleAsync(utilisateurId, jeu.Id))
            .Returns<Jeu?>(jeu);
        _depotCollection
            .CallsTo(x => x.AjouterPossessionAsync(A<Possession>._))
            .Returns(resultatDepot);

        // Act
        var result = await InstanceTest.TraiterAsync(new AjouterJeuRequete(jeu.Id));

        // Assert
        result.Statut.Should().Be(attendu);
        result.Item.Should().BeNull();
    }

    [Fact]
    public async Task TraiterAsync_ResultatDuDepotInattendu_LeverInvalidOperationException()
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var jeu = CreerFixture<Jeu>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.TrouverJeuVisibleAsync(utilisateurId, jeu.Id))
            .Returns<Jeu?>(jeu);
        _depotCollection
            .CallsTo(x => x.AjouterPossessionAsync(A<Possession>._))
            .Returns((ResultatAjoutPossession)999);

        // Act
        var act = () => InstanceTest.TraiterAsync(new AjouterJeuRequete(jeu.Id));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

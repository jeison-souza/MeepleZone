using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Modeles;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Collection.Test.Gestionnaires;

public class GestionnaireRequeteListerCollectionTests : BaseTest<GestionnaireRequeteListerCollection>
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
    public async Task TraiterAsync_EnToutTemps_RetournerItemsEtTotal()
    {
        // Arrange
        var requete = new ListerCollectionRequete(null, 1, 24);
        var utilisateurId = CreerFixture<Guid>();
        var total = CreerFixture<int>();
        List<ResultatPossession> resultats = [.. Fixture.CreateMany<ResultatPossession>(3)];
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(A<Guid>._, A<string?>._, A<FiltrePossessions?>._, A<int>._, A<int>._))
            .Returns(new PageDeResultats<ResultatPossession>(resultats, total));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Total.Should().Be(total);
        result.Items.Should().BeEquivalentTo(resultats.Select(r => new ItemCollectionDto(
            PossessionId: r.Possession.Id,
            JeuId: r.Possession.Jeu!.Id,
            AnneePublication: r.Possession.Jeu.AnneePublication,
            Commentaire: r.Possession.Commentaire,
            CouvertureId: r.Possession.Jeu.CouvertureId,
            CouvertureUrl: r.Possession.Jeu.CouvertureUrl,
            DureeMinMinutes: r.Possession.Jeu.DureeMinMinutes,
            DureeMaxMinutes: r.Possession.Jeu.DureeMaxMinutes,
            JoueursMin: r.Possession.Jeu.JoueursMin,
            JoueursMax: r.Possession.Jeu.JoueursMax,
            JoueursMeilleur: r.Possession.Jeu.JoueursMeilleur,
            MecaniqueNoms: r.MecaniqueNoms,
            ModeJeuNom: r.ModeJeuNom,
            Nom: r.Possession.Jeu.Nom,
            Note: r.Possession.Note,
            PartiesJouees: r.Possession.PartiesJouees,
            StatutJeu: r.Possession.Jeu.Statut)),
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task TraiterAsync_JeuNonCharge_LeverInvalidOperationException()
    {
        // Arrange
        var requete = new ListerCollectionRequete(null, 1, 24);
        var possession = Fixture.Build<Possession>().Without(p => p.Jeu).Create();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(CreerFixture<Guid>());
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(A<Guid>._, A<string?>._, A<FiltrePossessions?>._, A<int>._, A<int>._))
            .Returns(new PageDeResultats<ResultatPossession>([new ResultatPossession(possession, null, [])], 1));

        // Act
        var act = () => InstanceTest.TraiterAsync(requete);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task TraiterAsync_EnToutTemps_TransmettreUtilisateurRechercheSautEtTailleAuDepot()
    {
        // Arrange
        var requete = new ListerCollectionRequete("catan", 3, 24);
        var utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, "catan", null, 48, 24))
            .Returns(new PageDeResultats<ResultatPossession>([], 0));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, "catan", null, 48, 24))
            .MustHaveHappenedOnceExactly();
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("  catan  ", "catan")]
    public async Task TraiterAsync_RechercheAvecEspacesOuVide_NormaliserAvantLeDepot(string? recherche, string? attendue)
    {
        // Arrange
        var requete = new ListerCollectionRequete(recherche, 1, 24);
        var utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, attendue, null, 0, 24))
            .Returns(new PageDeResultats<ResultatPossession>([], 0));

        // Act
        await InstanceTest.TraiterAsync(requete);

        // Assert
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, attendue, null, 0, 24))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TraiterAsync_TaillePageMaximale_Accepter()
    {
        // Arrange
        var requete = new ListerCollectionRequete(null, 1, Pagination.TaillePageMaximale);
        var utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, null, null, 0, 50))
            .Returns(new PageDeResultats<ResultatPossession>([], 0));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 24)]
    [InlineData(-1, 24)]
    [InlineData(Pagination.PageMaximale + 1, 24)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    [InlineData(1, Pagination.TaillePageMaximale + 1)]
    public async Task TraiterAsync_PageOuTaillePageHorsLimites_LeverArgumentOutOfRangeException(int page, int taillePage)
    {
        // Arrange
        var requete = new ListerCollectionRequete(null, page, taillePage);

        // Act
        var act = () => InstanceTest.TraiterAsync(requete);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task TraiterAsync_FiltreComplet_TransmettreLeFiltreAuDepotSansDoublonsDeMecaniques()
    {
        // Arrange
        var modeJeuId = CreerFixture<Guid>();
        var mecaniqueA = CreerFixture<Guid>();
        var mecaniqueB = CreerFixture<Guid>();
        var filtre = new FiltreCollection(4, 4, 60, 7, 1, 20, modeJeuId, [mecaniqueA, mecaniqueB, mecaniqueA]);
        var requete = new ListerCollectionRequete(null, 1, 24, filtre);
        var utilisateurId = CreerFixture<Guid>();
        FiltrePossessions? filtreRecu = null;
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(A<Guid>._, A<string?>._, A<FiltrePossessions?>._, A<int>._, A<int>._))
            .Invokes((Guid _, string? _, FiltrePossessions? f, int _, int _) => filtreRecu = f)
            .Returns(new PageDeResultats<ResultatPossession>([], 0));

        // Act
        await InstanceTest.TraiterAsync(requete);

        // Assert
        filtreRecu.Should().NotBeNull();
        filtreRecu!.NombreJoueurs.Should().Be(4);
        filtreRecu.JoueursMeilleur.Should().Be(4);
        filtreRecu.DureeMaxMinutes.Should().Be(60);
        filtreRecu.NoteMin.Should().Be(7);
        filtreRecu.PartiesJoueesMin.Should().Be(1);
        filtreRecu.PartiesJoueesMax.Should().Be(20);
        filtreRecu.ModeJeuId.Should().Be(modeJeuId);
        filtreRecu.MecaniqueIds.Should().Equal(mecaniqueA, mecaniqueB);
    }

    [Theory]
    [InlineData(0, null, null, null, null, null)]
    [InlineData(-1, null, null, null, null, null)]
    [InlineData(null, 0, null, null, null, null)]
    [InlineData(null, null, 0, null, null, null)]
    [InlineData(null, null, null, 0, null, null)]
    [InlineData(null, null, null, 11, null, null)]
    [InlineData(null, null, null, null, -1, null)]
    [InlineData(null, null, null, null, null, -1)]
    [InlineData(null, null, null, null, 5, 4)]
    public async Task TraiterAsync_FiltreHorsLimites_LeverArgumentOutOfRangeExceptionSansAppelerLeDepot(
        int? nombreJoueurs, int? meilleur, int? dureeMax, int? noteMin, int? partiesMin, int? partiesMax)
    {
        // Arrange
        var filtre = new FiltreCollection((short?)nombreJoueurs, (short?)meilleur, (short?)dureeMax, (short?)noteMin, partiesMin, partiesMax, null, null);
        var requete = new ListerCollectionRequete(null, 1, 24, filtre);

        // Act
        var act = () => InstanceTest.TraiterAsync(requete);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(A<Guid>._, A<string?>._, A<FiltrePossessions?>._, A<int>._, A<int>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(1, 1, 1, 1, 0, 0)]
    [InlineData(null, null, null, 10, 3, 3)]
    public async Task TraiterAsync_FiltreSurLesBornes_Accepter(
        int? nombreJoueurs, int? meilleur, int? dureeMax, int? noteMin, int? partiesMin, int? partiesMax)
    {
        // Arrange
        var filtre = new FiltreCollection((short?)nombreJoueurs, (short?)meilleur, (short?)dureeMax, (short?)noteMin, partiesMin, partiesMax, null, null);
        var requete = new ListerCollectionRequete(null, 1, 24, filtre);
        _utilisateurCourant.CallsTo(x => x.Id).Returns(CreerFixture<Guid>());
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(A<Guid>._, A<string?>._, A<FiltrePossessions?>._, A<int>._, A<int>._))
            .Returns(new PageDeResultats<ResultatPossession>([], 0));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Items.Should().BeEmpty();
    }
}

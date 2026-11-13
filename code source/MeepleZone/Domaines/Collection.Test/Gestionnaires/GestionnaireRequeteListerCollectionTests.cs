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
        List<Possession> possessions = [.. Fixture.CreateMany<Possession>(3)];
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(A<Guid>._, A<string?>._, A<int>._, A<int>._))
            .Returns(new PageDeResultats<Possession>(possessions, total));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Total.Should().Be(total);
        result.Items.Should().BeEquivalentTo(possessions.Select(p => new ItemCollectionDto(
            p.Id,
            p.Jeu!.Id,
            p.Jeu.Nom,
            p.Jeu.AnneePublication,
            p.Jeu.JoueursMin,
            p.Jeu.JoueursMax,
            p.Jeu.DureeMinMinutes,
            p.Jeu.DureeMaxMinutes,
            p.Jeu.CouvertureId,
            p.Jeu.CouvertureUrl,
            p.Note,
            p.PartiesJouees,
            p.Commentaire,
            p.Jeu.Statut)),
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
            .CallsTo(x => x.ListerPossessionsAsync(A<Guid>._, A<string?>._, A<int>._, A<int>._))
            .Returns(new PageDeResultats<Possession>([possession], 1));

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
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, "catan", 48, 24))
            .Returns(new PageDeResultats<Possession>([], 0));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, "catan", 48, 24))
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
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, attendue, 0, 24))
            .Returns(new PageDeResultats<Possession>([], 0));

        // Act
        await InstanceTest.TraiterAsync(requete);

        // Assert
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, attendue, 0, 24))
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
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, null, 0, 50))
            .Returns(new PageDeResultats<Possession>([], 0));

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
}

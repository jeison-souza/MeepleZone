using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
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
        List<PossessionAvecJeu> possessions = [.. Fixture.CreateMany<PossessionAvecJeu>(3)];
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(A<Guid>._, A<string?>._, A<int>._, A<int>._))
            .Returns(new PageDePossessions(possessions, total));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Total.Should().Be(total);
        result.Items.Should().BeEquivalentTo(possessions.Select(p => new ItemCollectionDto(
            p.Possession.Id,
            p.Jeu.Id,
            p.Jeu.Nom,
            p.Jeu.AnneePublication,
            p.Jeu.JoueursMin,
            p.Jeu.JoueursMax,
            p.Jeu.DureeMinMinutes,
            p.Jeu.DureeMaxMinutes,
            p.Jeu.CouvertureId,
            p.Jeu.CouvertureUrl,
            p.Possession.Note,
            p.Possession.PartiesJouees,
            p.Possession.Commentaire,
            p.Jeu.Statut)),
            options => options.WithStrictOrdering());
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
            .Returns(new PageDePossessions([], 0));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, "catan", 48, 24))
            .MustHaveHappenedOnceExactly();
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task TraiterAsync_TaillePageMaximale_Accepter()
    {
        // Arrange
        var requete = new ListerCollectionRequete(null, 1, GestionnaireRequeteListerCollection.TaillePageMaximale);
        var utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ListerPossessionsAsync(utilisateurId, null, 0, 50))
            .Returns(new PageDePossessions([], 0));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 24)]
    [InlineData(-1, 24)]
    [InlineData(GestionnaireRequeteListerCollection.PageMaximale + 1, 24)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    [InlineData(1, GestionnaireRequeteListerCollection.TaillePageMaximale + 1)]
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

using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Modeles;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Collection.Test.Gestionnaires;

public class GestionnaireRequeteRechercherJeuxTests : BaseTest<GestionnaireRequeteRechercherJeux>
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
    public async Task TraiterAsync_EnToutTemps_RetournerJeuxEtTotal()
    {
        // Arrange
        var requete = new RechercherJeuxRequete("catan", 1, 10);
        var utilisateurId = CreerFixture<Guid>();
        var total = CreerFixture<int>();
        List<ResultatRechercheJeu> jeux = [.. Fixture.CreateMany<ResultatRechercheJeu>(3)];
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.RechercherJeuxAsync(A<Guid>._, A<string>._, A<int>._, A<int>._))
            .Returns(new PageDeResultats<ResultatRechercheJeu>(jeux, total));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Total.Should().Be(total);
        result.Jeux.Should().BeEquivalentTo(
            jeux.Select(j => new JeuResumeDto(j.Jeu.Id, j.Jeu.Nom, j.Jeu.AnneePublication, j.Jeu.Editeur, j.DejaPossede)),
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task TraiterAsync_EnToutTemps_TransmettreUtilisateurTexteSautEtTailleAuDepot()
    {
        // Arrange
        var requete = new RechercherJeuxRequete("  catan  ", 3, 10);
        var utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.RechercherJeuxAsync(utilisateurId, "catan", 20, 10))
            .Returns(new PageDeResultats<ResultatRechercheJeu>([], 0));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        _depotCollection
            .CallsTo(x => x.RechercherJeuxAsync(utilisateurId, "catan", 20, 10))
            .MustHaveHappenedOnceExactly();
        result.Jeux.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TraiterAsync_TexteVideOuEspaces_RetournerReponseVideSansInterrogerLeDepot(string texte)
    {
        // Arrange
        var requete = new RechercherJeuxRequete(texte, 1, 10);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Jeux.Should().BeEmpty();
        result.Total.Should().Be(0);
        _depotCollection.CallsTo(x => x.RechercherJeuxAsync(A<Guid>._, A<string>._, A<int>._, A<int>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, Pagination.TaillePageMaximale + 1)]
    public async Task TraiterAsync_PageOuTaillePageHorsLimites_LeverArgumentOutOfRangeException(int page, int taillePage)
    {
        // Arrange
        var requete = new RechercherJeuxRequete("catan", page, taillePage);

        // Act
        var act = () => InstanceTest.TraiterAsync(requete);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}

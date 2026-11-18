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

public class GestionnaireRequeteDetecterDoublonsTests : BaseTest<GestionnaireRequeteDetecterDoublons>
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
    public async Task TraiterAsync_EnToutTemps_RetournerLesJeuxSemblablesDansLOrdreDuDepot()
    {
        // Arrange
        var requete = new DetecterDoublonsRequete("Catann");
        List<ResultatRechercheJeu> resultats = [.. Fixture.CreateMany<ResultatRechercheJeu>(3)];
        _utilisateurCourant.CallsTo(x => x.Id).Returns(CreerFixture<Guid>());
        _depotCollection
            .CallsTo(x => x.DetecterDoublonsAsync(A<Guid>._, A<string>._, A<int>._))
            .Returns(resultats);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Should().BeEquivalentTo(
            resultats.Select(r => new JeuResumeDto(r.Jeu.Id, r.Jeu.Nom, r.Jeu.AnneePublication, r.Jeu.Editeur, r.DejaPossede)),
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task TraiterAsync_NomAvecEspaces_TransmettreUtilisateurNomSansEspacesEtLimiteAuDepot()
    {
        // Arrange
        var requete = new DetecterDoublonsRequete("  Catann  ");
        var utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.DetecterDoublonsAsync(utilisateurId, "Catann", GestionnaireRequeteDetecterDoublons.LimiteResultats))
            .Returns(new List<ResultatRechercheJeu>());

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        _depotCollection
            .CallsTo(x => x.DetecterDoublonsAsync(utilisateurId, "Catann", GestionnaireRequeteDetecterDoublons.LimiteResultats))
            .MustHaveHappenedOnceExactly();
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TraiterAsync_NomVideOuEspaces_RetournerListeVideSansInterrogerLeDepot(string nom)
    {
        // Arrange
        var requete = new DetecterDoublonsRequete(nom);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Should().BeEmpty();
        _depotCollection.CallsTo(x => x.DetecterDoublonsAsync(A<Guid>._, A<string>._, A<int>._))
            .MustNotHaveHappened();
    }
}

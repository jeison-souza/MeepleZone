using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Gestionnaires;

public class GestionnaireRequeteListerConnexionsTest : BaseTest<GestionnaireRequeteListerConnexions>
{
    private Fake<IDepotCompte> _depotCompte = null!;

    private Fake<IUtilisateurCourant> _utilisateurCourant = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCompte = CreerFausse<IDepotCompte>();
        _utilisateurCourant = CreerFausse<IUtilisateurCourant>();
    }

    [Fact]
    public async Task TraiterAsync_EnToutTemps_RetournerLesConnexionsDansLOrdreDuDepot()
    {
        // Arrange
        var requete = CreerFixture<ListerConnexionsRequete>();
        var utilisateurId = CreerFixture<Guid>();
        List<IdentiteExterne> identites = [.. Fixture.CreateMany<IdentiteExterne>(3)];
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCompte.CallsTo(x => x.ListerIdentitesAsync(utilisateurId)).Returns(identites);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Should().BeEquivalentTo(
            identites.Select(i => new ConnexionDto(i.Fournisseur, i.Courriel, i.CreeLe)),
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task TraiterAsync_EnToutTemps_DemanderLesIdentitesDeLUtilisateurCourantSeulement()
    {
        // Arrange
        var requete = CreerFixture<ListerConnexionsRequete>();
        var utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCompte.CallsTo(x => x.ListerIdentitesAsync(A<Guid>._)).Returns(new List<IdentiteExterne>());

        // Act
        await InstanceTest.TraiterAsync(requete);

        // Assert
        _depotCompte.CallsTo(x => x.ListerIdentitesAsync(utilisateurId)).MustHaveHappenedOnceExactly();
        _depotCompte.CallsTo(x => x.ListerIdentitesAsync(A<Guid>.That.Not.IsEqualTo(utilisateurId))).MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_AucuneIdentite_RetournerListeVide()
    {
        // Arrange
        var requete = CreerFixture<ListerConnexionsRequete>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(CreerFixture<Guid>());
        _depotCompte.CallsTo(x => x.ListerIdentitesAsync(A<Guid>._)).Returns(new List<IdentiteExterne>());

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task TraiterAsync_DeuxConnexionsDuMemeFournisseur_ToutesLesDeux()
    {
        // Arrange
        var requete = CreerFixture<ListerConnexionsRequete>();
        var fournisseur = CreerFixture<string>();
        List<IdentiteExterne> identites =
        [
            Fixture.Build<IdentiteExterne>().With(i => i.Fournisseur, fournisseur).Create(),
            Fixture.Build<IdentiteExterne>().With(i => i.Fournisseur, fournisseur).Create(),
        ];
        _utilisateurCourant.CallsTo(x => x.Id).Returns(CreerFixture<Guid>());
        _depotCompte.CallsTo(x => x.ListerIdentitesAsync(A<Guid>._)).Returns(identites);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Fournisseur == fournisseur);
    }
}

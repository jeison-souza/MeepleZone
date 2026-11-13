using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Collection.Contrats.Enums;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Collection.Test.Gestionnaires;

public class GestionnaireRequeteRetirerJeuTests : BaseTest<GestionnaireRequeteRetirerJeu>
{
    private Fake<IDepotCollection> _depotCollection = null!;

    private Fake<IUtilisateurCourant> _utilisateurCourant = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCollection = CreerFausse<IDepotCollection>();
        _utilisateurCourant = CreerFausse<IUtilisateurCourant>();
    }

    [Theory]
    [InlineData(true, StatutRetrait.Retire)]
    [InlineData(false, StatutRetrait.Introuvable)]
    public async Task TraiterAsync_ResultatDuDepot_RetournerLeStatutCorrespondant(bool retiree, StatutRetrait attendu)
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var possessionId = CreerFixture<Guid>();
        var requete = new RetirerJeuRequete(possessionId);
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.RetirerPossessionAsync(utilisateurId, possessionId))
            .Returns(retiree);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(attendu);
        _depotCollection
            .CallsTo(x => x.RetirerPossessionAsync(utilisateurId, possessionId))
            .MustHaveHappenedOnceExactly();
    }
}

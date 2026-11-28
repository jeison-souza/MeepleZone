using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Gestionnaires;

public class GestionnaireRequeteModifierVisibiliteTest : BaseTest<GestionnaireRequeteModifierVisibilite>
{
    private Fake<IDepotCompte> _depotCompte = null!;

    private Fake<IUtilisateurCourant> _utilisateurCourant = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCompte = CreerFausse<IDepotCompte>();
        _utilisateurCourant = CreerFausse<IUtilisateurCourant>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(Guid.CreateVersion7());
    }

    [Theory]
    [InlineData(VisibiliteCollection.Privee, "privee")]
    [InlineData(VisibiliteCollection.Amis, "amis")]
    public async Task TraiterAsync_EnToutTemps_ModifierLaVisibiliteDeLUtilisateurCourant(VisibiliteCollection visibilite, string valeur)
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCompte.CallsTo(x => x.ModifierVisibiliteAsync(utilisateurId, valeur)).Returns(ResultatModificationVisibilite.Modifiee);

        // Act
        await InstanceTest.TraiterAsync(new ModifierVisibiliteRequete(visibilite));

        // Assert
        _depotCompte.CallsTo(x => x.ModifierVisibiliteAsync(utilisateurId, valeur)).MustHaveHappenedOnceExactly();
        _depotCompte.CallsTo(x => x.ModifierVisibiliteAsync(A<Guid>._, A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(ResultatModificationVisibilite.Modifiee, StatutModificationVisibilite.Modifiee)]
    [InlineData(ResultatModificationVisibilite.Introuvable, StatutModificationVisibilite.CompteIntrouvable)]
    public async Task TraiterAsync_SelonLeResultatDuDepot_RetournerLeStatut(ResultatModificationVisibilite resultat, StatutModificationVisibilite attendu)
    {
        // Arrange
        var requete = CreerFixture<ModifierVisibiliteRequete>();
        _depotCompte.CallsTo(x => x.ModifierVisibiliteAsync(A<Guid>._, A<string>._)).Returns(resultat);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(attendu);
    }

    [Fact]
    public async Task TraiterAsync_VisibiliteInconnue_LancerUneExceptionSansRienEnregistrer()
    {
        // Arrange
        var requete = new ModifierVisibiliteRequete((VisibiliteCollection)99);

        // Act
        var action = () => InstanceTest.TraiterAsync(requete);

        // Assert
        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
        _depotCompte.CallsTo(x => x.ModifierVisibiliteAsync(A<Guid>._, A<string>._)).MustNotHaveHappened();
    }
}

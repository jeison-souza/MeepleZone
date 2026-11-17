using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Gestionnaires;

public class GestionnaireRequeteLierFournisseurTest : BaseTest<GestionnaireRequeteLierFournisseur>
{
    private Fake<IDepotCompte> _depotCompte = null!;

    private Fake<IUtilisateurCourant> _utilisateurCourant = null!;

    private Guid _utilisateurId;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCompte = CreerFausse<IDepotCompte>();
        _utilisateurCourant = CreerFausse<IUtilisateurCourant>();
        _utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(_utilisateurId);
    }

    [Fact]
    public async Task TraiterAsync_IdentiteInconnue_AjouterEtRetournerLiee()
    {
        // Arrange
        var requete = new LierFournisseurRequete("Google", "id-externe-1", "Test@Exemple.COM");
        IdentiteExterne? ajoutee = null;
        SimulerRechercheSansResultat(requete);
        _depotCompte.CallsTo(x => x.AjouterIdentiteAsync(A<IdentiteExterne>._))
            .Invokes((IdentiteExterne i) => ajoutee = i)
            .Returns(ResultatAjoutIdentite.Ajoutee);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutLiaison.Liee);
        ajoutee.Should().NotBeNull();
        ajoutee!.UtilisateurId.Should().Be(_utilisateurId);
        ajoutee.Fournisseur.Should().Be("Google");
        ajoutee.IdentifiantCompteExterne.Should().Be("id-externe-1");
        ajoutee.Courriel.Should().Be("test@exemple.com");
        ajoutee.DerniereConnexionLe.Should().Be(FakeTimeProvider.GetUtcNow());
        ajoutee.Id.Version.Should().Be(7);
    }

    [Fact]
    public async Task TraiterAsync_IdentiteDejaLieeAuCompteCourant_RetournerDejaLieeAuMemeCompteSansAjouter()
    {
        // Arrange
        var requete = new LierFournisseurRequete("Google", "id-externe-1", null);
        SimulerRechercheAvecResultat(requete, _utilisateurId);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutLiaison.DejaLieeAuMemeCompte);
        _depotCompte.CallsTo(x => x.AjouterIdentiteAsync(A<IdentiteExterne>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_IdentiteDejaLieeAUnAutreCompte_RetournerDejaUtiliseeParAutreCompteSansAjouter()
    {
        // Arrange
        var requete = new LierFournisseurRequete("Microsoft", "id-externe-2", null);
        SimulerRechercheAvecResultat(requete, CreerFixture<Guid>());

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutLiaison.DejaUtiliseeParAutreCompte);
        _depotCompte.CallsTo(x => x.AjouterIdentiteAsync(A<IdentiteExterne>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_AutreIdentiteDuMemeFournisseur_ChercherLIdentifiantExactEtLier()
    {
        // Arrange
        var requete = new LierFournisseurRequete("Google", "deuxieme-compte-google", null);
        SimulerRechercheSansResultat(requete);
        _depotCompte.CallsTo(x => x.AjouterIdentiteAsync(A<IdentiteExterne>._))
            .Returns(ResultatAjoutIdentite.Ajoutee);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutLiaison.Liee);
        _depotCompte.CallsTo(x => x.TrouverIdentiteAsync("Google", "deuxieme-compte-google")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TraiterAsync_AjoutEnParalleleParUnAutreCompte_RetournerDejaUtiliseeParAutreCompte()
    {
        // Arrange
        var requete = new LierFournisseurRequete("Google", "id-externe-1", null);
        var identiteAutreCompte = Fixture.Build<IdentiteExterne>().With(i => i.UtilisateurId, CreerFixture<Guid>()).Create();
        _depotCompte.CallsTo(x => x.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne))
            .Returns((IdentiteExterne?)null).Once()
            .Then.Returns(identiteAutreCompte);
        _depotCompte.CallsTo(x => x.AjouterIdentiteAsync(A<IdentiteExterne>._))
            .Returns(ResultatAjoutIdentite.IdentiteDejaExistante);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutLiaison.DejaUtiliseeParAutreCompte);
    }

    [Fact]
    public async Task TraiterAsync_AjoutEnParalleleParLeMemeCompte_RetournerDejaLieeAuMemeCompte()
    {
        // Arrange
        var requete = new LierFournisseurRequete("Google", "id-externe-1", null);
        var identiteMemeCompte = Fixture.Build<IdentiteExterne>().With(i => i.UtilisateurId, _utilisateurId).Create();
        _depotCompte.CallsTo(x => x.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne))
            .Returns((IdentiteExterne?)null).Once()
            .Then.Returns(identiteMemeCompte);
        _depotCompte.CallsTo(x => x.AjouterIdentiteAsync(A<IdentiteExterne>._))
            .Returns(ResultatAjoutIdentite.IdentiteDejaExistante);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutLiaison.DejaLieeAuMemeCompte);
    }

    [Fact]
    public async Task TraiterAsync_AjoutEnParalleleIntrouvable_LeverInvalidOperationException()
    {
        // Arrange
        var requete = new LierFournisseurRequete("Google", "id-externe-1", null);
        SimulerRechercheSansResultat(requete);
        _depotCompte.CallsTo(x => x.AjouterIdentiteAsync(A<IdentiteExterne>._))
            .Returns(ResultatAjoutIdentite.IdentiteDejaExistante);

        // Act
        var act = () => InstanceTest.TraiterAsync(requete);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task TraiterAsync_ResultatDAjoutInconnu_LeverInvalidOperationException()
    {
        // Arrange
        var requete = new LierFournisseurRequete("Google", "id-externe-1", null);
        SimulerRechercheSansResultat(requete);
        _depotCompte.CallsTo(x => x.AjouterIdentiteAsync(A<IdentiteExterne>._))
            .Returns((ResultatAjoutIdentite)99);

        // Act
        var act = () => InstanceTest.TraiterAsync(requete);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("  Nom@Exemple.CA  ", "nom@exemple.ca")]
    public async Task TraiterAsync_Courriel_ConserverSeulementUnCourrielNormalise(string? courriel, string? attendu)
    {
        // Arrange
        var requete = new LierFournisseurRequete("Google", "id-externe-1", courriel);
        IdentiteExterne? ajoutee = null;
        SimulerRechercheSansResultat(requete);
        _depotCompte.CallsTo(x => x.AjouterIdentiteAsync(A<IdentiteExterne>._))
            .Invokes((IdentiteExterne i) => ajoutee = i)
            .Returns(ResultatAjoutIdentite.Ajoutee);

        // Act
        await InstanceTest.TraiterAsync(requete);

        // Assert
        ajoutee!.Courriel.Should().Be(attendu);
    }

    [Theory]
    [InlineData(null, "id")]
    [InlineData("", "id")]
    [InlineData("  ", "id")]
    [InlineData("Google", null)]
    [InlineData("Google", "")]
    [InlineData("Google", "  ")]
    public async Task TraiterAsync_FournisseurOuIdentifiantVide_LeverArgumentException(string? fournisseur, string? identifiant)
    {
        // Arrange
        var requete = new LierFournisseurRequete(fournisseur!, identifiant!, null);

        // Act
        var act = () => InstanceTest.TraiterAsync(requete);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task TraiterAsync_RequeteNulle_LeverArgumentNullException()
    {
        // Act
        var act = () => InstanceTest.TraiterAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private void SimulerRechercheSansResultat(LierFournisseurRequete requete)
    {
        _depotCompte.CallsTo(x => x.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne))
            .Returns((IdentiteExterne?)null);
    }

    private void SimulerRechercheAvecResultat(LierFournisseurRequete requete, Guid utilisateurId)
    {
        var identite = Fixture.Build<IdentiteExterne>().With(i => i.UtilisateurId, utilisateurId).Create();
        _depotCompte.CallsTo(x => x.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne))
            .Returns(identite);
    }
}

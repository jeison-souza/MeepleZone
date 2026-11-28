using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Gestionnaires;

public class GestionnaireRequeteObtenirProfilTest : BaseTest<GestionnaireRequeteObtenirProfil>
{
    private Fake<IDepotCompte> _depotCompte = null!;

    private Fake<IUtilisateurCourant> _utilisateurCourant = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCompte = CreerFausse<IDepotCompte>();
        _utilisateurCourant = CreerFausse<IUtilisateurCourant>();
    }

    // Les valeurs de la colonne viennent du schéma publié (schema-donnees.md).
    [Theory]
    [InlineData("privee", VisibiliteCollection.Privee)]
    [InlineData("amis", VisibiliteCollection.Amis)]
    public async Task TraiterAsync_UtilisateurExistant_RetournerSonProfil(string valeur, VisibiliteCollection attendue)
    {
        // Arrange
        var requete = CreerFixture<ObtenirProfilRequete>();
        var utilisateur = CreerFixture<Utilisateur>();
        utilisateur.VisibiliteCollection = valeur;
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateur.Id);
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(utilisateur.Id)).Returns(utilisateur);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Should().Be(new ProfilDto(utilisateur.NomAffichage, utilisateur.Courriel, utilisateur.CodeAmitie, attendue));
    }

    [Fact]
    public async Task TraiterAsync_EnToutTemps_ChercherSeulementLUtilisateurCourant()
    {
        // Arrange
        var requete = CreerFixture<ObtenirProfilRequete>();
        var utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(A<Guid>._)).Returns((Utilisateur?)null);

        // Act
        await InstanceTest.TraiterAsync(requete);

        // Assert
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(utilisateurId)).MustHaveHappenedOnceExactly();
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(A<Guid>.That.Not.IsEqualTo(utilisateurId))).MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_UtilisateurInexistant_RetournerNull()
    {
        // Arrange
        var requete = CreerFixture<ObtenirProfilRequete>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(CreerFixture<Guid>());
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(A<Guid>._)).Returns((Utilisateur?)null);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task TraiterAsync_CourrielAbsent_RetournerProfilSansCourriel()
    {
        // Arrange
        var requete = CreerFixture<ObtenirProfilRequete>();
        var utilisateur = CreerFixture<Utilisateur>();
        utilisateur.Courriel = null;
        utilisateur.VisibiliteCollection = "privee";
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateur.Id);
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(utilisateur.Id)).Returns(utilisateur);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result!.Courriel.Should().BeNull();
    }
}

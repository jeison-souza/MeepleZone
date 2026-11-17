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
using MeepleZone.Systeme.Entites.Constantes;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Gestionnaires;

public class GestionnaireRequeteSupprimerCompteTest : BaseTest<GestionnaireRequeteSupprimerCompte>
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

    [Theory]
    [InlineData(ResultatSuppressionCompte.Supprime, StatutSuppressionCompte.Supprime)]
    [InlineData(ResultatSuppressionCompte.CompteProtege, StatutSuppressionCompte.CompteProtege)]
    [InlineData(ResultatSuppressionCompte.Introuvable, StatutSuppressionCompte.Introuvable)]
    [InlineData(ResultatSuppressionCompte.Conflit, StatutSuppressionCompte.Conflit)]
    public async Task TraiterAsync_MembreExistant_RetournerLeStatutCorrespondantAuResultatDuDepot(
        ResultatSuppressionCompte resultatDepot,
        StatutSuppressionCompte statutAttendu)
    {
        // Arrange
        SimulerUtilisateur(Roles.Membre);
        _depotCompte.CallsTo(x => x.SupprimerCompteAsync(A<Guid>._, A<Guid>._)).Returns(resultatDepot);

        // Act
        var result = await InstanceTest.TraiterAsync(new SupprimerCompteRequete());

        // Assert
        result.Statut.Should().Be(statutAttendu);
    }

    [Fact]
    public async Task TraiterAsync_MembreExistant_SupprimerLeCompteCourantEtRattacherAuCompteTechnique()
    {
        // Arrange
        SimulerUtilisateur(Roles.Membre);
        _depotCompte.CallsTo(x => x.SupprimerCompteAsync(A<Guid>._, A<Guid>._)).Returns(ResultatSuppressionCompte.Supprime);

        // Act
        await InstanceTest.TraiterAsync(new SupprimerCompteRequete());

        // Assert
        _depotCompte.CallsTo(x => x.SupprimerCompteAsync(_utilisateurId, ComptesTechniques.UtilisateurSupprimeId))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(Roles.Administrateur)]
    [InlineData(Roles.Systeme)]
    public async Task TraiterAsync_RoleProtege_RetournerCompteProtegeSansSupprimer(string role)
    {
        // Arrange
        SimulerUtilisateur(role);

        // Act
        var result = await InstanceTest.TraiterAsync(new SupprimerCompteRequete());

        // Assert
        result.Statut.Should().Be(StatutSuppressionCompte.CompteProtege);
        _depotCompte.CallsTo(x => x.SupprimerCompteAsync(A<Guid>._, A<Guid>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_UtilisateurInexistant_RetournerIntrouvableSansSupprimer()
    {
        // Arrange
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(A<Guid>._)).Returns((Utilisateur?)null);

        // Act
        var result = await InstanceTest.TraiterAsync(new SupprimerCompteRequete());

        // Assert
        result.Statut.Should().Be(StatutSuppressionCompte.Introuvable);
        _depotCompte.CallsTo(x => x.SupprimerCompteAsync(A<Guid>._, A<Guid>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_EnToutTemps_ChercherSeulementLUtilisateurCourant()
    {
        // Arrange
        SimulerUtilisateur(Roles.Membre);
        _depotCompte.CallsTo(x => x.SupprimerCompteAsync(A<Guid>._, A<Guid>._)).Returns(ResultatSuppressionCompte.Supprime);

        // Act
        await InstanceTest.TraiterAsync(new SupprimerCompteRequete());

        // Assert
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(_utilisateurId)).MustHaveHappenedOnceExactly();
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(A<Guid>.That.Not.IsEqualTo(_utilisateurId))).MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_ResultatDuDepotInconnu_Lancer()
    {
        // Arrange
        SimulerUtilisateur(Roles.Membre);
        _depotCompte.CallsTo(x => x.SupprimerCompteAsync(A<Guid>._, A<Guid>._)).Returns((ResultatSuppressionCompte)999);

        // Act
        var act = () => InstanceTest.TraiterAsync(new SupprimerCompteRequete());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private void SimulerUtilisateur(string role)
    {
        var utilisateur = Fixture.Build<Utilisateur>()
            .With(u => u.Id, _utilisateurId)
            .With(u => u.Role, role)
            .Create();
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(_utilisateurId)).Returns(utilisateur);
    }
}

using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Extensions;
using MeepleZone.Domaines.Compte.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Gestionnaires;

public class GestionnaireRequeteGererConnexionTest : BaseTest<GestionnaireRequeteGererConnexion>
{
    private Fake<IDepotCompte> _depotCompte = null!;
    private Fake<IGenerateurCodeAmitie> _generateurCodeAmitie = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCompte = CreerFausse<IDepotCompte>();
        _generateurCodeAmitie = CreerFausse<IGenerateurCodeAmitie>();
    }

    [Fact]
    public async Task TraiterAsync_QuandIdentiteExterneExisteEtUtilisateurExiste_RetournerUtilisateurDtoEtActualiserConnexion()
    {
        // Arrange
        var requete = CreerFixture<GererConnexionRequete>();
        var identiteExterne = CreerFixture<IdentiteExterne>();
        var dateConnexionDepart = identiteExterne.DerniereConnexionLe;
        var utilisateurAttendue = CreerFixture<Utilisateur>();
        _depotCompte
            .CallsTo(x => x.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne))
            .Returns(identiteExterne);
        _depotCompte
            .CallsTo(x => x.MettreAJourConnexionAsync(identiteExterne))
            .Returns(Task.CompletedTask);
        _depotCompte
            .CallsTo(x => x.TrouverUtilisateurAsync(identiteExterne.UtilisateurId))
            .Returns(utilisateurAttendue);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 1, mettreAJour: 1, verifierCourriel: 0, genererCode: 0, enregistrerNouveau: 0);
        result.Statut.Should().Be(StatutConnexion.Connecte);
        result.Utilisateur.Should().BeEquivalentTo(utilisateurAttendue.VersDto());
        identiteExterne.DerniereConnexionLe.Should().Be(FakeTimeProvider.GetUtcNow());
        identiteExterne.DerniereConnexionLe.Should().NotBe(dateConnexionDepart);
        identiteExterne.Courriel.Should().Be(requete.Courriel!.ToLowerInvariant());
    }

    [Fact]
    public async Task TraiterAsync_QuandIdentiteExterneExisteEtUtilisateurNeExistePas_LancerInvalidOperationException()
    {
        // Arrange
        var requete = CreerFixture<GererConnexionRequete>();
        var identiteExterne = CreerFixture<IdentiteExterne>();
        var identiteExterneCourrielDepart = identiteExterne.Courriel;
        var identiteExterneDateConnexionDepart = identiteExterne.DerniereConnexionLe;

        _depotCompte
            .CallsTo(x => x.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne))
            .Returns(identiteExterne);
        _depotCompte
            .CallsTo(x => x.TrouverUtilisateurAsync(identiteExterne.UtilisateurId))
            .Returns(Task.FromResult<Utilisateur?>(null));

        // Act
        Func<Task> action = () => InstanceTest.TraiterAsync(requete);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Impossible de trouver l'utilisateur lié au compte.");
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 1, mettreAJour: 0, verifierCourriel: 0, genererCode: 0, enregistrerNouveau: 0);
        identiteExterne.DerniereConnexionLe.Should().NotBe(FakeTimeProvider.GetUtcNow());
        identiteExterne.DerniereConnexionLe.Should().Be(identiteExterneDateConnexionDepart);
        identiteExterne.Courriel.Should().NotBe(requete.Courriel);
        identiteExterne.Courriel.Should().Be(identiteExterneCourrielDepart);
    }

    [Fact]
    public async Task TraiterAsync_QuandIdentiteExterneExisteEtRequeteSansCourriel_ActualiserConnexionEtConserverCourrielExistant()
    {
        // Arrange
        var requete = CreerFixture<GererConnexionRequete>() with { Courriel = null };
        var identiteExterne = CreerFixture<IdentiteExterne>();
        var dateConnexionDepart = identiteExterne.DerniereConnexionLe;
        var courrielDepart = identiteExterne.Courriel;
        var utilisateurAttendue = CreerFixture<Utilisateur>();
        _depotCompte
            .CallsTo(x => x.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne))
            .Returns(identiteExterne);
        _depotCompte
            .CallsTo(x => x.MettreAJourConnexionAsync(identiteExterne))
            .Returns(Task.CompletedTask);
        _depotCompte
            .CallsTo(x => x.TrouverUtilisateurAsync(identiteExterne.UtilisateurId))
            .Returns(utilisateurAttendue);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 1, mettreAJour: 1, verifierCourriel: 0, genererCode: 0, enregistrerNouveau: 0);
        result.Statut.Should().Be(StatutConnexion.Connecte);
        result.Utilisateur.Should().BeEquivalentTo(utilisateurAttendue.VersDto());
        identiteExterne.DerniereConnexionLe.Should().Be(FakeTimeProvider.GetUtcNow());
        identiteExterne.DerniereConnexionLe.Should().NotBe(dateConnexionDepart);
        identiteExterne.Courriel.Should().Be(courrielDepart);
    }

    [Fact]
    public async Task TraiterAsync_QuandNouvelleIdentiteEtCourrielDejaUtiliseEtSansForcerCreation_RetournerCompteProbableSansEnregistrer()
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { ForcerCreation = false };
        ConfigurerIdentiteInconnue(requete);
        ConfigurerVerificationCourriel(requete.Courriel!.ToLowerInvariant(), existe: true);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 1, genererCode: 0, enregistrerNouveau: 0);
        result.Statut.Should().Be(StatutConnexion.CompteProbable);
        result.Utilisateur.Should().BeNull();
    }

    [Fact]
    public async Task TraiterAsync_QuandNouvelleIdentiteEtCourrielLibreEtSansForcerCreation_CreerCompteAvecCourrielEtRetournerConnecte()
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { ForcerCreation = false };
        var codeAmitie = Fixture.Create<string>();
        var courrielNormalise = requete.Courriel!.ToLowerInvariant();
        ConfigurerIdentiteInconnue(requete);
        ConfigurerVerificationCourriel(courrielNormalise, existe: false);
        var compte = ConfigurerEnregistrement(codeAmitie, ResultatEnregistrementCompte.Enregistre);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 1, genererCode: 1, enregistrerNouveau: 1);
        ValiderCompteCree(result, compte, requete, codeAmitie, requete.NomAffichage!, courrielUtilisateur: courrielNormalise, courrielIdentite: courrielNormalise);
    }

    [Fact]
    public async Task TraiterAsync_QuandNouvelleIdentiteEtCourrielLibreEtAvecForcerCreation_CreerCompteAvecCourrielEtRetournerConnecte()
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { ForcerCreation = true };
        var codeAmitie = Fixture.Create<string>();
        var courrielNormalise = requete.Courriel!.ToLowerInvariant();
        ConfigurerIdentiteInconnue(requete);
        ConfigurerVerificationCourriel(courrielNormalise, existe: false);
        var compte = ConfigurerEnregistrement(codeAmitie, ResultatEnregistrementCompte.Enregistre);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 1, genererCode: 1, enregistrerNouveau: 1);
        ValiderCompteCree(result, compte, requete, codeAmitie, requete.NomAffichage!, courrielUtilisateur: courrielNormalise, courrielIdentite: courrielNormalise);
    }

    [Fact]
    public async Task TraiterAsync_QuandNouvelleIdentiteEtCourrielDejaUtiliseEtAvecForcerCreation_CreerCompteSansCourrielUtilisateurEtRetournerConnecte()
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { ForcerCreation = true };
        var codeAmitie = Fixture.Create<string>();
        var courrielNormalise = requete.Courriel!.ToLowerInvariant();
        ConfigurerIdentiteInconnue(requete);
        ConfigurerVerificationCourriel(courrielNormalise, existe: true);
        var compte = ConfigurerEnregistrement(codeAmitie, ResultatEnregistrementCompte.Enregistre);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 1, genererCode: 1, enregistrerNouveau: 1);
        ValiderCompteCree(result, compte, requete, codeAmitie, requete.NomAffichage!, courrielUtilisateur: null, courrielIdentite: courrielNormalise);
    }

    [Fact]
    public async Task TraiterAsync_QuandNouvelleIdentiteEtSansCourriel_CreerCompteSansCourrielEtRetournerConnecte()
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { Courriel = null };
        var codeAmitie = Fixture.Create<string>();
        ConfigurerIdentiteInconnue(requete);
        var compte = ConfigurerEnregistrement(codeAmitie, ResultatEnregistrementCompte.Enregistre);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 0, genererCode: 1, enregistrerNouveau: 1);
        ValiderCompteCree(result, compte, requete, codeAmitie, requete.NomAffichage!, courrielUtilisateur: null, courrielIdentite: null);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TraiterAsync_QuandNouvelleIdentiteEtCourrielBlanc_CreerCompteSansCourrielEtRetournerConnecte(string courrielBlanc)
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { Courriel = courrielBlanc };
        var codeAmitie = Fixture.Create<string>();
        ConfigurerIdentiteInconnue(requete);
        var compte = ConfigurerEnregistrement(codeAmitie, ResultatEnregistrementCompte.Enregistre);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 0, genererCode: 1, enregistrerNouveau: 1);
        ValiderCompteCree(result, compte, requete, codeAmitie, requete.NomAffichage!, courrielUtilisateur: null, courrielIdentite: null);
    }

    [Fact]
    public async Task TraiterAsync_QuandNouvelleIdentiteEtCourrielAvecEspaces_VerifierEtEnregistrerCourrielNettoye()
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { Courriel = "  Test@Example.Com  ", ForcerCreation = false };
        var codeAmitie = Fixture.Create<string>();
        var courrielNormalise = "test@example.com";
        ConfigurerIdentiteInconnue(requete);
        ConfigurerVerificationCourriel(courrielNormalise, existe: false);
        var compte = ConfigurerEnregistrement(codeAmitie, ResultatEnregistrementCompte.Enregistre);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 1, genererCode: 1, enregistrerNouveau: 1);
        ValiderCompteCree(result, compte, requete, codeAmitie, requete.NomAffichage!, courrielUtilisateur: courrielNormalise, courrielIdentite: courrielNormalise);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TraiterAsync_QuandNouvelleIdentiteEtSansNomAffichage_CreerCompteAvecNomJoueurSuiviDuCodeAmitie(string? nomBlanc)
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { NomAffichage = nomBlanc, ForcerCreation = false };
        var codeAmitie = Fixture.Create<string>();
        var courrielNormalise = requete.Courriel!.ToLowerInvariant();
        ConfigurerIdentiteInconnue(requete);
        ConfigurerVerificationCourriel(courrielNormalise, existe: false);
        var compte = ConfigurerEnregistrement(codeAmitie, ResultatEnregistrementCompte.Enregistre);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 1, genererCode: 1, enregistrerNouveau: 1);
        ValiderCompteCree(result, compte, requete, codeAmitie, $"Joueur {codeAmitie}", courrielUtilisateur: courrielNormalise, courrielIdentite: courrielNormalise);
    }

    [Fact]
    public async Task TraiterAsync_QuandIdentiteEnregistreeEnParallele_ActualiserConnexionDeLIdentiteExistanteEtRetournerConnecte()
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { ForcerCreation = false };
        var courrielNormalise = requete.Courriel!.ToLowerInvariant();
        var identiteExterne = CreerFixture<IdentiteExterne>();
        var dateConnexionDepart = identiteExterne.DerniereConnexionLe;
        var utilisateurAttendu = CreerFixture<Utilisateur>();
        _depotCompte
            .CallsTo(x => x.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne))
            .ReturnsNextFromSequence(Task.FromResult<IdentiteExterne?>(null), Task.FromResult<IdentiteExterne?>(identiteExterne));
        ConfigurerVerificationCourriel(courrielNormalise, existe: false);
        ConfigurerEnregistrement(Fixture.Create<string>(), ResultatEnregistrementCompte.IdentiteDejaExistante);
        _depotCompte
            .CallsTo(x => x.TrouverUtilisateurAsync(identiteExterne.UtilisateurId))
            .Returns(utilisateurAttendu);
        _depotCompte
            .CallsTo(x => x.MettreAJourConnexionAsync(identiteExterne))
            .Returns(Task.CompletedTask);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 2, trouverUtilisateur: 1, mettreAJour: 1, verifierCourriel: 1, genererCode: 1, enregistrerNouveau: 1);
        result.Statut.Should().Be(StatutConnexion.Connecte);
        result.Utilisateur.Should().BeEquivalentTo(utilisateurAttendu.VersDto());
        identiteExterne.DerniereConnexionLe.Should().Be(FakeTimeProvider.GetUtcNow());
        identiteExterne.DerniereConnexionLe.Should().NotBe(dateConnexionDepart);
        identiteExterne.Courriel.Should().Be(courrielNormalise);
    }

    [Fact]
    public async Task TraiterAsync_QuandIdentiteEnregistreeEnParalleleEtIntrouvableEnsuite_LancerInvalidOperationException()
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { ForcerCreation = false };
        var courrielNormalise = requete.Courriel!.ToLowerInvariant();
        _depotCompte
            .CallsTo(x => x.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne))
            .Returns(Task.FromResult<IdentiteExterne?>(null));
        ConfigurerVerificationCourriel(courrielNormalise, existe: false);
        ConfigurerEnregistrement(Fixture.Create<string>(), ResultatEnregistrementCompte.IdentiteDejaExistante);

        // Act
        Func<Task> action = () => InstanceTest.TraiterAsync(requete);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Impossible de retrouver la connexion enregistrée en parallèle.");
        ValiderNombreAppels(trouverIdentite: 2, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 1, genererCode: 1, enregistrerNouveau: 1);
    }

    [Fact]
    public async Task TraiterAsync_QuandCourrielPrisEnParalleleEtSansForcerCreation_RetournerCompteProbableSansNouvelEssai()
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { ForcerCreation = false };
        var courrielNormalise = requete.Courriel!.ToLowerInvariant();
        ConfigurerIdentiteInconnue(requete);
        ConfigurerVerificationCourriel(courrielNormalise, existe: false);
        ConfigurerEnregistrement(Fixture.Create<string>(), ResultatEnregistrementCompte.CourrielDejaUtilise);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 1, genererCode: 1, enregistrerNouveau: 1);
        result.Statut.Should().Be(StatutConnexion.CompteProbable);
        result.Utilisateur.Should().BeNull();
    }

    [Fact]
    public async Task TraiterAsync_QuandCourrielPrisEnParalleleEtAvecForcerCreation_ReessayerUneFoisSansCourrielUtilisateurEtRetournerConnecte()
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { ForcerCreation = true };
        var courrielNormalise = requete.Courriel!.ToLowerInvariant();
        var codesAmitie = new[] { Fixture.Create<string>(), Fixture.Create<string>() };
        ConfigurerIdentiteInconnue(requete);
        ConfigurerVerificationCourriel(courrielNormalise, existe: false);
        var comptes = ConfigurerEnregistrementsSuccessifs(
            codesAmitie,
            ResultatEnregistrementCompte.CourrielDejaUtilise,
            ResultatEnregistrementCompte.Enregistre);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 1, genererCode: 2, enregistrerNouveau: 2);
        comptes.Should().HaveCount(2);
        comptes[0].Utilisateur.Courriel.Should().Be(courrielNormalise);
        comptes[1].Utilisateur.Id.Should().NotBe(comptes[0].Utilisateur.Id);
        ValiderCompteCree(result, comptes[1], requete, codesAmitie[1], requete.NomAffichage!, courrielUtilisateur: null, courrielIdentite: courrielNormalise);
    }

    [Fact]
    public async Task TraiterAsync_QuandCourrielPrisEnParalleleEtNouvelEssaiEchoueAussi_LancerInvalidOperationException()
    {
        // Arrange
        var requete = Fixture.Create<GererConnexionRequete>() with { ForcerCreation = true };
        var courrielNormalise = requete.Courriel!.ToLowerInvariant();
        ConfigurerIdentiteInconnue(requete);
        ConfigurerVerificationCourriel(courrielNormalise, existe: false);
        ConfigurerEnregistrement(Fixture.Create<string>(), ResultatEnregistrementCompte.CourrielDejaUtilise);

        // Act
        Func<Task> action = () => InstanceTest.TraiterAsync(requete);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Résultat d'enregistrement inattendu*");
        ValiderNombreAppels(trouverIdentite: 1, trouverUtilisateur: 0, mettreAJour: 0, verifierCourriel: 1, genererCode: 2, enregistrerNouveau: 2);
    }

    private sealed class CompteEnregistre
    {
        public Utilisateur Utilisateur { get; set; } = null!;

        public IdentiteExterne Identite { get; set; } = null!;
    }

    private void ConfigurerIdentiteInconnue(GererConnexionRequete requete)
    {
        _depotCompte
            .CallsTo(x => x.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne))
            .Returns(Task.FromResult<IdentiteExterne?>(null));
    }

    private void ConfigurerVerificationCourriel(string courrielNormalise, bool existe)
    {
        _depotCompte
            .CallsTo(x => x.VerifierExistenceCourrielAsync(courrielNormalise))
            .Returns(Task.FromResult(existe));
    }

    private CompteEnregistre ConfigurerEnregistrement(string codeAmitie, ResultatEnregistrementCompte resultat)
    {
        var compte = new CompteEnregistre();
        _generateurCodeAmitie
            .CallsTo(x => x.GenererAsync())
            .Returns(codeAmitie);
        _depotCompte
            .CallsTo(x => x.EnregistrerNouveauCompteAsync(A<Utilisateur>.Ignored, A<IdentiteExterne>.Ignored))
            .Invokes((Utilisateur u, IdentiteExterne i) =>
            {
                compte.Utilisateur = u;
                compte.Identite = i;
            })
            .Returns(Task.FromResult(resultat));
        return compte;
    }

    private List<CompteEnregistre> ConfigurerEnregistrementsSuccessifs(string[] codesAmitie, params ResultatEnregistrementCompte[] resultats)
    {
        var comptes = new List<CompteEnregistre>();
        _generateurCodeAmitie
            .CallsTo(x => x.GenererAsync())
            .ReturnsNextFromSequence(Array.ConvertAll(codesAmitie, code => Task.FromResult(code)));
        _depotCompte
            .CallsTo(x => x.EnregistrerNouveauCompteAsync(A<Utilisateur>.Ignored, A<IdentiteExterne>.Ignored))
            .ReturnsLazily((Utilisateur u, IdentiteExterne i) =>
            {
                comptes.Add(new CompteEnregistre { Utilisateur = u, Identite = i });
                return Task.FromResult(resultats[comptes.Count - 1]);
            });
        return comptes;
    }

    private void ValiderCompteCree(
        GererConnexionReponse reponse,
        CompteEnregistre compte,
        GererConnexionRequete requete,
        string codeAmitie,
        string nomAffichage,
        string? courrielUtilisateur,
        string? courrielIdentite)
    {
        reponse.Statut.Should().Be(StatutConnexion.Connecte);
        reponse.Utilisateur.Should().BeEquivalentTo(compte.Utilisateur.VersDto());
        compte.Utilisateur.Id.Version.Should().Be(7);
        compte.Utilisateur.Role.Should().Be("membre");
        compte.Utilisateur.VisibiliteCollection.Should().Be("privee");
        compte.Utilisateur.CodeAmitie.Should().Be(codeAmitie);
        compte.Utilisateur.Courriel.Should().Be(courrielUtilisateur);
        compte.Utilisateur.NomAffichage.Should().Be(nomAffichage);
        compte.Identite.Id.Version.Should().Be(7);
        compte.Identite.UtilisateurId.Should().Be(compte.Utilisateur.Id);
        compte.Identite.DerniereConnexionLe.Should().Be(FakeTimeProvider.GetUtcNow());
        compte.Identite.Fournisseur.Should().Be(requete.Fournisseur);
        compte.Identite.IdentifiantCompteExterne.Should().Be(requete.IdentifiantCompteExterne);
        compte.Identite.Courriel.Should().Be(courrielIdentite);
    }

    private void ValiderNombreAppels(int trouverIdentite, int trouverUtilisateur, int mettreAJour, int verifierCourriel, int genererCode, int enregistrerNouveau)
    {
        _depotCompte
            .CallsTo(x => x.TrouverIdentiteAsync(A<string>.Ignored, A<string>.Ignored))
            .MustHaveHappenedANumberOfTimesMatching(n => n == trouverIdentite);
        _depotCompte
            .CallsTo(x => x.TrouverUtilisateurAsync(A<Guid>.Ignored))
            .MustHaveHappenedANumberOfTimesMatching(n => n == trouverUtilisateur);
        _depotCompte
            .CallsTo(x => x.MettreAJourConnexionAsync(A<IdentiteExterne>.Ignored))
            .MustHaveHappenedANumberOfTimesMatching(n => n == mettreAJour);
        _depotCompte
            .CallsTo(x => x.VerifierExistenceCourrielAsync(A<string>.Ignored))
            .MustHaveHappenedANumberOfTimesMatching(n => n == verifierCourriel);
        _generateurCodeAmitie
            .CallsTo(x => x.GenererAsync())
            .MustHaveHappenedANumberOfTimesMatching(n => n == genererCode);
        _depotCompte
            .CallsTo(x => x.EnregistrerNouveauCompteAsync(A<Utilisateur>.Ignored, A<IdentiteExterne>.Ignored))
            .MustHaveHappenedANumberOfTimesMatching(n => n == enregistrerNouveau);
    }
}

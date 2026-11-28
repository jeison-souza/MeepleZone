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

public class GestionnaireRequeteEnregistrerCourrielTest : BaseTest<GestionnaireRequeteEnregistrerCourriel>
{
    private const string NouveauCourriel = "nouveau@exemple.ca";

    private Fake<IDepotCompte> _depotCompte = null!;

    private Fake<IUtilisateurCourant> _utilisateurCourant = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCompte = CreerFausse<IDepotCompte>();
        _utilisateurCourant = CreerFausse<IUtilisateurCourant>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("jean")]
    [InlineData("jean@")]
    [InlineData("@exemple.ca")]
    [InlineData("jean@exemple")]
    [InlineData("jean@exemple.")]
    [InlineData("jean tremblay@exemple.ca")]
    [InlineData("Jean <jean@exemple.ca>")]
    [InlineData("jean@exemple.ca, autre@exemple.ca")]
    public async Task TraiterAsync_SaisieInvalide_RetournerSaisieInvalideSansConsulterLeDepot(string courriel)
    {
        // Arrange
        var requete = new EnregistrerCourrielRequete(courriel);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutEnregistrementCourriel.SaisieInvalide);
        _depotCompte.CallsTo(x => x.ModifierCourrielAsync(A<Guid>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_CourrielTropLong_RetournerSaisieInvalide()
    {
        // Arrange
        var requete = new EnregistrerCourrielRequete(new string('a', 250) + "@exemple.ca");

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutEnregistrementCourriel.SaisieInvalide);
    }

    [Fact]
    public async Task TraiterAsync_CompteSupprime_RetournerCompteIntrouvable()
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(utilisateurId)).Returns(Task.FromResult<Utilisateur?>(null));

        // Act
        var result = await InstanceTest.TraiterAsync(new EnregistrerCourrielRequete(NouveauCourriel));

        // Assert
        result.Statut.Should().Be(StatutEnregistrementCourriel.CompteIntrouvable);
        _depotCompte.CallsTo(x => x.ModifierCourrielAsync(A<Guid>._, A<string>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("nouveau@exemple.ca")]
    [InlineData("  Nouveau@Exemple.CA  ")]
    public async Task TraiterAsync_MemeCourriel_RetournerInchangeSansEnregistrer(string saisie)
    {
        // Arrange
        ConfigurerUtilisateurCourant(NouveauCourriel);

        // Act
        var result = await InstanceTest.TraiterAsync(new EnregistrerCourrielRequete(saisie));

        // Assert
        result.Statut.Should().Be(StatutEnregistrementCourriel.Inchange);
        _depotCompte.CallsTo(x => x.ModifierCourrielAsync(A<Guid>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_CourrielVerifieParUnAutreCompte_RetournerCourrielDejaUtiliseSansEnregistrer()
    {
        // Arrange
        ConfigurerUtilisateurCourant("ancien@exemple.ca");
        _depotCompte.CallsTo(x => x.VerifierPresenceCourrielAsync(NouveauCourriel)).Returns(PresenceCourriel.Verifiee);

        // Act
        var result = await InstanceTest.TraiterAsync(new EnregistrerCourrielRequete(NouveauCourriel));

        // Assert
        result.Statut.Should().Be(StatutEnregistrementCourriel.CourrielDejaUtilise);
        _depotCompte.CallsTo(x => x.ModifierCourrielAsync(A<Guid>._, A<string>._)).MustNotHaveHappened();
    }

    // Courriel nul : un ajout. Sinon : une modification. Un courriel non vérifié ailleurs ne bloque pas (ADR 0004, règle 1).
    [Theory]
    [InlineData(null, PresenceCourriel.Aucune)]
    [InlineData("ancien@exemple.ca", PresenceCourriel.Aucune)]
    [InlineData("ancien@exemple.ca", PresenceCourriel.NonVerifiee)]
    public async Task TraiterAsync_CourrielLibre_EnregistrerLeCourrielNettoye(string? courrielActuel, PresenceCourriel presence)
    {
        // Arrange
        var utilisateur = ConfigurerUtilisateurCourant(courrielActuel);
        _depotCompte.CallsTo(x => x.VerifierPresenceCourrielAsync(NouveauCourriel)).Returns(presence);
        _depotCompte.CallsTo(x => x.ModifierCourrielAsync(utilisateur.Id, NouveauCourriel)).Returns(ResultatModificationCourriel.Modifie);

        // Act
        var result = await InstanceTest.TraiterAsync(new EnregistrerCourrielRequete("  Nouveau@Exemple.CA "));

        // Assert
        result.Statut.Should().Be(StatutEnregistrementCourriel.Enregistre);
        _depotCompte.CallsTo(x => x.ModifierCourrielAsync(utilisateur.Id, NouveauCourriel)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TraiterAsync_CompteSupprimePendantLEnregistrement_RetournerCompteIntrouvable()
    {
        // Arrange
        var utilisateur = ConfigurerUtilisateurCourant("ancien@exemple.ca");
        _depotCompte.CallsTo(x => x.VerifierPresenceCourrielAsync(NouveauCourriel)).Returns(PresenceCourriel.Aucune);
        _depotCompte.CallsTo(x => x.ModifierCourrielAsync(utilisateur.Id, NouveauCourriel)).Returns(ResultatModificationCourriel.Introuvable);

        // Act
        var result = await InstanceTest.TraiterAsync(new EnregistrerCourrielRequete(NouveauCourriel));

        // Assert
        result.Statut.Should().Be(StatutEnregistrementCourriel.CompteIntrouvable);
    }

    private Utilisateur ConfigurerUtilisateurCourant(string? courriel)
    {
        var utilisateur = CreerFixture<Utilisateur>();
        utilisateur.Courriel = courriel;
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateur.Id);
        _depotCompte.CallsTo(x => x.TrouverUtilisateurAsync(utilisateur.Id)).Returns(utilisateur);
        return utilisateur;
    }
}

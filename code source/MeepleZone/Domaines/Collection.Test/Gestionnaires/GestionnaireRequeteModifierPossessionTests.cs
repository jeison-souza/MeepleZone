using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Collection.Contrats.Enums;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Collection.Test.Gestionnaires;

public class GestionnaireRequeteModifierPossessionTests : BaseTest<GestionnaireRequeteModifierPossession>
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
    public async Task TraiterAsync_PossessionTrouvee_RetournerModifieAvecLItem()
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var possession = CreerFixture<Possession>();
        var requete = new ModifierPossessionRequete(possession.Id, 7, "Très bon", 12);
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ModifierPossessionAsync(utilisateurId, possession.Id, 7, "Très bon", 12))
            .Returns<Possession?>(possession);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutModification.Modifie);
        result.Item.Should().BeEquivalentTo(new ItemCollectionDto(
            possession.Id,
            possession.Jeu!.Id,
            possession.Jeu.Nom,
            possession.Jeu.AnneePublication,
            possession.Jeu.JoueursMin,
            possession.Jeu.JoueursMax,
            possession.Jeu.DureeMinMinutes,
            possession.Jeu.DureeMaxMinutes,
            possession.Jeu.CouvertureId,
            possession.Jeu.CouvertureUrl,
            possession.Note,
            possession.PartiesJouees,
            possession.Commentaire,
            possession.Jeu.Statut));
    }

    [Fact]
    public async Task TraiterAsync_PossessionIntrouvable_RetournerIntrouvableSansItem()
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var possessionId = CreerFixture<Guid>();
        var requete = new ModifierPossessionRequete(possessionId, 5, null, 0);
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ModifierPossessionAsync(utilisateurId, possessionId, 5, null, 0))
            .Returns<Possession?>(null);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutModification.Introuvable);
        result.Item.Should().BeNull();
    }

    [Theory]
    [InlineData("  bien  ", "bien")]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    public async Task TraiterAsync_Commentaire_NormaliserAvantLeDepot(string? commentaire, string? attendu)
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var possession = CreerFixture<Possession>();
        var requete = new ModifierPossessionRequete(possession.Id, null, commentaire, 1);
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ModifierPossessionAsync(utilisateurId, possession.Id, null, attendu, 1))
            .Returns<Possession?>(possession);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutModification.Modifie);
        _depotCollection
            .CallsTo(x => x.ModifierPossessionAsync(utilisateurId, possession.Id, null, attendu, 1))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData(ValidationPossession.NoteMin, 0)]
    [InlineData(ValidationPossession.NoteMax, 0)]
    [InlineData((short)5, 1)]
    public async Task TraiterAsync_ValeursALaLimite_Accepter(short? note, int partiesJouees)
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var possession = CreerFixture<Possession>();
        var requete = new ModifierPossessionRequete(possession.Id, note, null, partiesJouees);
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ModifierPossessionAsync(utilisateurId, possession.Id, note, null, partiesJouees))
            .Returns<Possession?>(possession);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutModification.Modifie);
    }

    [Fact]
    public async Task TraiterAsync_CommentaireALaLongueurMaximale_Accepter()
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var possession = CreerFixture<Possession>();
        var commentaire = new string('a', ValidationPossession.CommentaireLongueurMax);
        var requete = new ModifierPossessionRequete(possession.Id, null, commentaire, 0);
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ModifierPossessionAsync(utilisateurId, possession.Id, null, commentaire, 0))
            .Returns<Possession?>(possession);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutModification.Modifie);
    }

    [Fact]
    public async Task TraiterAsync_CommentaireAvecEspacesAuxExtremitesApresRognage_Accepter()
    {
        // Arrange : 2000 caractères utiles entourés d'espaces dépassent 2000 avant le rognage seulement
        var utilisateurId = CreerFixture<Guid>();
        var possession = CreerFixture<Possession>();
        var utile = new string('a', ValidationPossession.CommentaireLongueurMax);
        var requete = new ModifierPossessionRequete(possession.Id, null, $"  {utile}  ", 0);
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _depotCollection
            .CallsTo(x => x.ModifierPossessionAsync(utilisateurId, possession.Id, null, utile, 0))
            .Returns<Possession?>(possession);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutModification.Modifie);
    }

    [Theory]
    [InlineData(0, 0, "ok")]
    [InlineData(-1, 0, "ok")]
    [InlineData(ValidationPossession.NoteMax + 1, 0, "ok")]
    [InlineData(5, -1, "ok")]
    public async Task TraiterAsync_NoteOuPartiesHorsLimites_RetournerDonneesInvalidesSansInterrogerLeDepot(short note, int partiesJouees, string commentaire)
    {
        // Arrange
        var requete = new ModifierPossessionRequete(CreerFixture<Guid>(), note, commentaire, partiesJouees);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutModification.DonneesInvalides);
        result.Item.Should().BeNull();
        _depotCollection
            .CallsTo(x => x.ModifierPossessionAsync(A<Guid>._, A<Guid>._, A<short?>._, A<string?>._, A<int>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_CommentaireTropLong_RetournerDonneesInvalidesSansInterrogerLeDepot()
    {
        // Arrange
        var commentaire = new string('a', ValidationPossession.CommentaireLongueurMax + 1);
        var requete = new ModifierPossessionRequete(CreerFixture<Guid>(), null, commentaire, 0);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutModification.DonneesInvalides);
        result.Item.Should().BeNull();
        _depotCollection
            .CallsTo(x => x.ModifierPossessionAsync(A<Guid>._, A<Guid>._, A<short?>._, A<string?>._, A<int>._))
            .MustNotHaveHappened();
    }
}

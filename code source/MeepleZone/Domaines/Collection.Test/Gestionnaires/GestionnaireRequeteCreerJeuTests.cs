using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Collection.Contrats.Enums;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Collection.Test.Gestionnaires;

public class GestionnaireRequeteCreerJeuTests : BaseTest<GestionnaireRequeteCreerJeu>
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
    public async Task TraiterAsync_DonneesValides_EnregistrerLeJeuEnAttenteSesMecaniquesEtLaPossession()
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        var modeJeuId = CreerFixture<Guid>();
        var mecanique1 = CreerFixture<Guid>();
        var mecanique2 = CreerFixture<Guid>();
        var requete = Requete(nom: "  Catan  ", editeur: "  Kosmos  ", modeJeuId: modeJeuId, mecaniqueIds: [mecanique1, mecanique2, mecanique1]);
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);

        Jeu? jeuEnvoye = null;
        IReadOnlyList<Guid>? mecaniquesEnvoyees = null;
        Possession? possessionEnvoyee = null;
        Jeu? jeuRattacheALEnvoi = CreerFixture<Jeu>();
        _depotCollection
            .CallsTo(x => x.CreerJeuAsync(A<Jeu>._, A<IReadOnlyList<Guid>>._, A<Possession>._))
            .Invokes((Jeu j, IReadOnlyList<Guid> m, Possession p) =>
            {
                jeuEnvoye = j;
                mecaniquesEnvoyees = m;
                possessionEnvoyee = p;
                jeuRattacheALEnvoi = p.Jeu;
            })
            .Returns(ResultatCreationJeu.Cree);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert : le jeu
        jeuEnvoye.Should().NotBeNull();
        jeuEnvoye!.Id.Version.Should().Be(7);
        jeuEnvoye.Nom.Should().Be("Catan");
        jeuEnvoye.Editeur.Should().Be("Kosmos");
        jeuEnvoye.Statut.Should().Be("en_attente");
        jeuEnvoye.CreeParId.Should().Be(utilisateurId);
        jeuEnvoye.ModeJeuId.Should().Be(modeJeuId);
        jeuEnvoye.AnneePublication.Should().Be(1995);
        jeuEnvoye.JoueursMin.Should().Be(3);
        jeuEnvoye.JoueursMax.Should().Be(4);
        jeuEnvoye.JoueursMeilleur.Should().Be(4);
        jeuEnvoye.DureeMinMinutes.Should().Be(60);
        jeuEnvoye.DureeMaxMinutes.Should().Be(120);
        jeuEnvoye.AgeMin.Should().Be(10);
        jeuEnvoye.Complexite.Should().Be(2.5m);

        // Assert : les mécaniques (sans doublon) et la possession du créateur
        mecaniquesEnvoyees.Should().BeEquivalentTo([mecanique1, mecanique2], options => options.WithStrictOrdering());
        possessionEnvoyee.Should().NotBeNull();
        possessionEnvoyee!.Id.Version.Should().Be(7);
        possessionEnvoyee.UtilisateurId.Should().Be(utilisateurId);
        possessionEnvoyee.JeuId.Should().Be(jeuEnvoye.Id);
        possessionEnvoyee.PartiesJouees.Should().Be(0);
        possessionEnvoyee.Note.Should().BeNull();
        possessionEnvoyee.Commentaire.Should().BeNull();
        jeuRattacheALEnvoi.Should().BeNull();

        // Assert : la réponse, avec l'item montré à la main
        result.Statut.Should().Be(StatutCreationJeu.Cree);
        result.Item.Should().BeEquivalentTo(new ItemCollectionDto(
            possessionEnvoyee.Id,
            jeuEnvoye.Id,
            "Catan",
            1995,
            3,
            4,
            60,
            120,
            null,
            null,
            null,
            0,
            null,
            "en_attente"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TraiterAsync_NomVideOuEspaces_RetournerDonneesInvalidesSansEcrire(string nom)
    {
        // Arrange
        var requete = Requete(nom: nom);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutCreationJeu.DonneesInvalides);
        result.Item.Should().BeNull();
        _depotCollection
            .CallsTo(x => x.CreerJeuAsync(A<Jeu>._, A<IReadOnlyList<Guid>>._, A<Possession>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task TraiterAsync_EditeurVideOuEspaces_EnregistrerUnEditeurNul(string? editeur)
    {
        // Arrange
        var requete = Requete(editeur: editeur);
        _utilisateurCourant.CallsTo(x => x.Id).Returns(CreerFixture<Guid>());
        Jeu? jeuEnvoye = null;
        _depotCollection
            .CallsTo(x => x.CreerJeuAsync(A<Jeu>._, A<IReadOnlyList<Guid>>._, A<Possession>._))
            .Invokes((Jeu j, IReadOnlyList<Guid> m, Possession p) => jeuEnvoye = j)
            .Returns(ResultatCreationJeu.Cree);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutCreationJeu.Cree);
        jeuEnvoye!.Editeur.Should().BeNull();
    }

    [Theory]
    [InlineData(0, null, null, null, null, null, null)]
    [InlineData(null, 0, null, null, null, null, null)]
    [InlineData(null, null, 0, null, null, null, null)]
    [InlineData(null, null, null, 0, null, null, null)]
    [InlineData(null, null, null, null, 0, null, null)]
    [InlineData(null, null, null, null, null, -1, null)]
    [InlineData(null, null, null, null, null, null, 0.9)]
    [InlineData(null, null, null, null, null, null, 5.1)]
    [InlineData(4, 2, null, null, null, null, null)]
    [InlineData(3, 4, 2, null, null, null, null)]
    [InlineData(3, 4, 5, null, null, null, null)]
    [InlineData(null, null, null, 60, 30, null, null)]
    public async Task TraiterAsync_NombresHorsLimitesOuIncoherents_RetournerDonneesInvalidesSansEcrire(
        int? joueursMin,
        int? joueursMax,
        int? joueursMeilleur,
        int? dureeMin,
        int? dureeMax,
        int? ageMin,
        double? complexite)
    {
        // Arrange
        var requete = Requete(joueursMin: joueursMin, joueursMax: joueursMax, joueursMeilleur: joueursMeilleur, dureeMin: dureeMin, dureeMax: dureeMax, ageMin: ageMin, complexite: complexite);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutCreationJeu.DonneesInvalides);
        result.Item.Should().BeNull();
        _depotCollection
            .CallsTo(x => x.CreerJeuAsync(A<Jeu>._, A<IReadOnlyList<Guid>>._, A<Possession>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(null, null, null, null, null, null, null)]
    [InlineData(1, 1, 1, null, null, null, null)]
    [InlineData(null, null, null, 1, 1, 0, null)]
    [InlineData(null, null, null, null, null, null, 1.0)]
    [InlineData(null, null, null, null, null, null, 5.0)]
    [InlineData(3, 4, 3, null, null, null, null)]
    [InlineData(3, 4, 4, null, null, null, null)]
    [InlineData(null, null, null, 60, 60, null, null)]
    public async Task TraiterAsync_NombresALaLimite_Accepter(
        int? joueursMin,
        int? joueursMax,
        int? joueursMeilleur,
        int? dureeMin,
        int? dureeMax,
        int? ageMin,
        double? complexite)
    {
        // Arrange
        var requete = Requete(joueursMin: joueursMin, joueursMax: joueursMax, joueursMeilleur: joueursMeilleur, dureeMin: dureeMin, dureeMax: dureeMax, ageMin: ageMin, complexite: complexite);
        _utilisateurCourant.CallsTo(x => x.Id).Returns(CreerFixture<Guid>());
        _depotCollection
            .CallsTo(x => x.CreerJeuAsync(A<Jeu>._, A<IReadOnlyList<Guid>>._, A<Possession>._))
            .Returns(ResultatCreationJeu.Cree);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutCreationJeu.Cree);
    }

    [Fact]
    public async Task TraiterAsync_NomALaLongueurMaximale_Accepter()
    {
        // Arrange
        var requete = Requete(nom: new string('a', ValidationJeu.NomLongueurMax));
        _utilisateurCourant.CallsTo(x => x.Id).Returns(CreerFixture<Guid>());
        _depotCollection
            .CallsTo(x => x.CreerJeuAsync(A<Jeu>._, A<IReadOnlyList<Guid>>._, A<Possession>._))
            .Returns(ResultatCreationJeu.Cree);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutCreationJeu.Cree);
    }

    [Fact]
    public async Task TraiterAsync_NomTropLong_RetournerDonneesInvalidesSansEcrire()
    {
        // Arrange
        var requete = Requete(nom: new string('a', ValidationJeu.NomLongueurMax + 1));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutCreationJeu.DonneesInvalides);
        _depotCollection
            .CallsTo(x => x.CreerJeuAsync(A<Jeu>._, A<IReadOnlyList<Guid>>._, A<Possession>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_EditeurTropLong_RetournerDonneesInvalidesSansEcrire()
    {
        // Arrange
        var requete = Requete(editeur: new string('a', ValidationJeu.EditeurLongueurMax + 1));

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Statut.Should().Be(StatutCreationJeu.DonneesInvalides);
        _depotCollection
            .CallsTo(x => x.CreerJeuAsync(A<Jeu>._, A<IReadOnlyList<Guid>>._, A<Possession>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task TraiterAsync_ReferenceInvalide_RetournerDonneesInvalidesSansItem()
    {
        // Arrange
        _utilisateurCourant.CallsTo(x => x.Id).Returns(CreerFixture<Guid>());
        _depotCollection
            .CallsTo(x => x.CreerJeuAsync(A<Jeu>._, A<IReadOnlyList<Guid>>._, A<Possession>._))
            .Returns(ResultatCreationJeu.ReferenceInvalide);

        // Act
        var result = await InstanceTest.TraiterAsync(Requete());

        // Assert
        result.Statut.Should().Be(StatutCreationJeu.DonneesInvalides);
        result.Item.Should().BeNull();
    }

    [Fact]
    public async Task TraiterAsync_ResultatDuDepotInattendu_LeverInvalidOperationException()
    {
        // Arrange
        _utilisateurCourant.CallsTo(x => x.Id).Returns(CreerFixture<Guid>());
        _depotCollection
            .CallsTo(x => x.CreerJeuAsync(A<Jeu>._, A<IReadOnlyList<Guid>>._, A<Possession>._))
            .Returns((ResultatCreationJeu)999);

        // Act
        var act = () => InstanceTest.TraiterAsync(Requete());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static CreerJeuRequete Requete(
        string nom = "Catan",
        int? joueursMin = 3,
        int? joueursMax = 4,
        int? joueursMeilleur = 4,
        int? dureeMin = 60,
        int? dureeMax = 120,
        int? ageMin = 10,
        double? complexite = 2.5,
        string? editeur = "Kosmos",
        Guid? modeJeuId = null,
        IReadOnlyList<Guid>? mecaniqueIds = null)
    {
        return new CreerJeuRequete(
            nom,
            1995,
            (short?)joueursMin,
            (short?)joueursMax,
            (short?)joueursMeilleur,
            (short?)dureeMin,
            (short?)dureeMax,
            (short?)ageMin,
            (decimal?)complexite,
            modeJeuId,
            editeur,
            mecaniqueIds ?? []);
    }
}

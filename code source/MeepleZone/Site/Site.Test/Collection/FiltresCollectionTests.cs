using FluentAssertions;
using MeepleZone.Site.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MeepleZone.Site.Test.Collection;

public class FiltresCollectionTests
{
    private static readonly Guid Mode = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MecaniqueA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid MecaniqueB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void SansFiltre_RienNestActifEtLeContratEstNul()
    {
        // Arrange
        var filtres = new FiltresCollection();

        // Act / Assert
        filtres.EstActif.Should().BeFalse();
        filtres.VersContrat().Should().BeNull();
    }

    [Fact]
    public void VersContrat_NombreDeJoueurs_ExigeQueLeJeuSeJoueACeNombre()
    {
        // Arrange
        var filtres = new FiltresCollection { NombreJoueurs = 4 };

        // Act
        var contrat = filtres.VersContrat();

        // Assert
        contrat!.NombreJoueurs.Should().Be(4);
        contrat.JoueursMeilleur.Should().BeNull();
    }

    [Fact]
    public void VersContrat_Ideal_TransmetSeulementLeMeilleurNombre()
    {
        // Arrange
        var filtres = new FiltresCollection { NombreJoueurs = 3, Optimal = true };

        // Act
        var contrat = filtres.VersContrat();

        // Assert
        contrat!.JoueursMeilleur.Should().Be(3);
        contrat.NombreJoueurs.Should().BeNull();
    }

    [Fact]
    public void VersContrat_TransmetLesAutresCriteres()
    {
        // Arrange
        var filtres = new FiltresCollection
        {
            DureeMax = 90,
            NoteMin = 6,
            PartiesMin = 1,
            PartiesMax = 10,
            ModeJeuId = Mode,
            MecaniqueIds = [MecaniqueA, MecaniqueB],
        };

        // Act
        var contrat = filtres.VersContrat();

        // Assert
        contrat!.DureeMaxMinutes.Should().Be(90);
        contrat.NoteMin.Should().Be(6);
        contrat.PartiesJoueesMin.Should().Be(1);
        contrat.PartiesJoueesMax.Should().Be(10);
        contrat.ModeJeuId.Should().Be(Mode);
        contrat.MecaniqueIds.Should().Equal(MecaniqueA, MecaniqueB);
    }

    [Fact]
    public void VersContrat_SansMecanique_TransmetNulPlutotQuuneListeVide()
    {
        // Arrange
        var filtres = new FiltresCollection { NoteMin = 5 };

        // Act / Assert
        filtres.VersContrat()!.MecaniqueIds.Should().BeNull();
    }

    [Fact]
    public void JamaisJoue_PartiesMaxEgalZero_EstTransmisTelQuel()
    {
        // Arrange
        var filtres = new FiltresCollection { PartiesMax = 0 };

        // Act / Assert
        filtres.VersContrat()!.PartiesJoueesMax.Should().Be(0);
    }

    [Fact]
    public void AvecPartiesMin_AuDessusDuMaximum_FaitSuivreLeMaximum()
    {
        // Arrange
        var filtres = new FiltresCollection { PartiesMax = 3 };

        // Act
        var change = filtres.AvecPartiesMin(5);

        // Assert
        change.PartiesMin.Should().Be(5);
        change.PartiesMax.Should().Be(5);
    }

    [Fact]
    public void AvecPartiesMax_EnDessousDuMinimum_FaitSuivreLeMinimum()
    {
        // Arrange
        var filtres = new FiltresCollection { PartiesMin = 5 };

        // Act
        var change = filtres.AvecPartiesMax(2);

        // Assert
        change.PartiesMax.Should().Be(2);
        change.PartiesMin.Should().Be(2);
    }

    [Fact]
    public void AvecPartiesMin_SansMaximum_LaisseLeMaximumVide()
    {
        // Act
        var change = new FiltresCollection().AvecPartiesMin(4);

        // Assert
        change.PartiesMin.Should().Be(4);
        change.PartiesMax.Should().BeNull();
    }

    [Fact]
    public void DepuisParametres_MinimumAuDessusDuMaximum_EstRamene()
    {
        // Act
        var filtres = FiltresCollection.DepuisParametres(null, null, null, null, "8", "3", null, null);

        // Assert
        filtres.PartiesMin.Should().BeLessThanOrEqualTo(filtres.PartiesMax!.Value);
    }

    [Fact]
    public void Parametres_AllerRetour_RetrouveLesMemesFiltres()
    {
        // Arrange
        var filtres = new FiltresCollection
        {
            NombreJoueurs = 4,
            Optimal = true,
            DureeMax = 90,
            NoteMin = 6,
            PartiesMin = 1,
            PartiesMax = 10,
            ModeJeuId = Mode,
            MecaniqueIds = [MecaniqueA, MecaniqueB],
        };

        // Act
        var p = filtres.VersParametres();
        var relus = FiltresCollection.DepuisParametres(
            p.GetValueOrDefault("joueurs"), p.GetValueOrDefault("optimal"), p.GetValueOrDefault("duree"), p.GetValueOrDefault("note"),
            p.GetValueOrDefault("parties-min"), p.GetValueOrDefault("parties-max"), p.GetValueOrDefault("mode"), p.GetValueOrDefault("mecaniques"));

        // Assert
        relus.EstIdentique(filtres).Should().BeTrue();
        relus.MecaniqueIds.Should().Equal(MecaniqueA, MecaniqueB);
    }

    [Fact]
    public void Parametres_SansFiltre_NeProduitRien()
    {
        new FiltresCollection().VersParametres().Should().BeEmpty();
    }

    [Theory]
    [InlineData("0")]
    [InlineData("100")]
    [InlineData("-2")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void DepuisParametres_JoueursInvalidesOuHorsLimites_SontIgnores(string? joueurs)
    {
        // Act
        var filtres = FiltresCollection.DepuisParametres(joueurs, "1", null, null, null, null, null, null);

        // Assert
        filtres.NombreJoueurs.Should().BeNull();
        filtres.Optimal.Should().BeFalse();
    }

    [Fact]
    public void DepuisParametres_OptimalSansJoueurs_EstIgnore()
    {
        FiltresCollection.DepuisParametres(null, "1", null, null, null, null, null, null).Optimal.Should().BeFalse();
    }

    [Fact]
    public void DepuisParametres_IdentifiantsInvalides_SontIgnores()
    {
        // Act
        var filtres = FiltresCollection.DepuisParametres(
            null, null, null, null, null, null, "pas-un-guid", $"{MecaniqueA}, nimporte quoi ,{MecaniqueA}");

        // Assert
        filtres.ModeJeuId.Should().BeNull();
        filtres.MecaniqueIds.Should().Equal(MecaniqueA);
    }

    [Fact]
    public void Chips_ListentLesCriteresActifsAvecLesNomsConnus()
    {
        // Arrange
        var filtres = new FiltresCollection
        {
            NombreJoueurs = 3,
            Optimal = true,
            DureeMax = 60,
            NoteMin = 7,
            PartiesMax = 0,
            ModeJeuId = Mode,
            MecaniqueIds = [MecaniqueA],
        };
        var modes = new Dictionary<Guid, string> { [Mode] = "Coopératif" };
        var mecaniques = new Dictionary<Guid, string> { [MecaniqueA] = "Deck-building" };

        // Act
        var chips = filtres.Chips(modes, mecaniques);

        // Assert
        chips.Select(c => c.Libelle).Should().Equal(
            "Idéal à 3 joueurs", "60 min ou moins", "Note d'au moins 7", "Jamais joué", "Coopératif", "Deck-building");
        filtres.NombreCriteres.Should().Be(6);
    }

    [Theory]
    [InlineData(null, 0, "Jamais joué")]
    [InlineData(0, 0, "Jamais joué")]
    [InlineData(null, 5, "5 parties jouées ou moins")]
    [InlineData(3, null, "3 parties jouées ou plus")]
    [InlineData(2, 6, "2 à 6 parties jouées")]
    [InlineData(4, 4, "4 parties jouées")]
    public void Chips_LibelleDesParties(int? minimum, int? maximum, string attendu)
    {
        // Arrange
        var filtres = new FiltresCollection { PartiesMin = minimum, PartiesMax = maximum };

        // Act
        var chip = filtres.Chips(new Dictionary<Guid, string>(), new Dictionary<Guid, string>()).Single();

        // Assert
        chip.Libelle.Should().Be(attendu);
    }

    [Fact]
    public void Sans_RetireSeulementLeCritereDuChip()
    {
        // Arrange
        var filtres = new FiltresCollection
        {
            NombreJoueurs = 3,
            Optimal = true,
            NoteMin = 7,
            MecaniqueIds = [MecaniqueA, MecaniqueB],
        };

        // Act
        var sansJoueurs = filtres.Sans(FiltresCollection.CleJoueurs);
        var sansMecaniqueA = filtres.Sans($"{FiltresCollection.CleMecaniques}:{MecaniqueA}");

        // Assert
        sansJoueurs.NombreJoueurs.Should().BeNull();
        sansJoueurs.Optimal.Should().BeFalse();
        sansJoueurs.NoteMin.Should().Be(7);
        sansMecaniqueA.MecaniqueIds.Should().Equal(MecaniqueB);
        sansMecaniqueA.NombreJoueurs.Should().Be(3);
    }

    [Fact]
    public void Sans_CleInconnue_NeChangeRien()
    {
        // Arrange
        var filtres = new FiltresCollection { NoteMin = 7 };

        // Act / Assert
        filtres.Sans("inconnue").EstIdentique(filtres).Should().BeTrue();
    }
}

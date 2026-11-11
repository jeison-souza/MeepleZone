using FluentAssertions;
using MeepleZone.Site.Affichage;
using Xunit;

namespace MeepleZone.Site.Test.Affichage;

public class FormatageJeuTests
{
    [Theory]
    [InlineData(2, 4, "2 à 4 joueurs")]
    [InlineData(1, 5, "1 à 5 joueurs")]
    [InlineData(2, 2, "2 joueurs")]
    [InlineData(1, 1, "1 joueur")]
    [InlineData(3, null, "3 joueurs")]
    [InlineData(null, 4, "4 joueurs")]
    public void Joueurs_AvecAuMoinsUneBorne_RetourneLeTexteAttendu(int? minimum, int? maximum, string attendu)
    {
        // Act
        var texte = FormatageJeu.Joueurs((short?)minimum, (short?)maximum);

        // Assert
        texte.Should().Be(attendu);
    }

    [Theory]
    [InlineData(30, 60, "30 à 60 min")]
    [InlineData(45, 45, "45 min")]
    [InlineData(20, null, "20 min")]
    [InlineData(null, 90, "90 min")]
    public void Duree_AvecAuMoinsUneBorne_RetourneLeTexteAttendu(int? minimum, int? maximum, string attendu)
    {
        // Act
        var texte = FormatageJeu.Duree((short?)minimum, (short?)maximum);

        // Assert
        texte.Should().Be(attendu);
    }

    [Fact]
    public void Joueurs_SansAucuneBorne_RetourneNull()
    {
        // Act
        var texte = FormatageJeu.Joueurs(null, null);

        // Assert
        texte.Should().BeNull();
    }

    [Fact]
    public void Duree_SansAucuneBorne_RetourneNull()
    {
        // Act
        var texte = FormatageJeu.Duree(null, null);

        // Assert
        texte.Should().BeNull();
    }
}

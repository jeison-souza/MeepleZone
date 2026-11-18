using FluentAssertions;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Site.Collection;
using System;
using Xunit;

namespace MeepleZone.Site.Test.Collection;

public class PresentationJeuTests
{
    [Theory]
    [InlineData(2015, "Matagot", "2015 · Matagot")]
    [InlineData(2015, null, "2015")]
    [InlineData(null, "Matagot", "Matagot")]
    [InlineData(null, " ", "Année et éditeur inconnus")]
    [InlineData(null, null, "Année et éditeur inconnus")]
    public void Detail_ListeAnneeEtEditeurConnus(int? annee, string? editeur, string attendu)
    {
        // Arrange
        var jeu = new JeuResumeDto(Guid.NewGuid(), "Pandemic", (short?)annee, editeur, false);

        // Act / Assert
        PresentationJeu.Detail(jeu).Should().Be(attendu);
    }
}

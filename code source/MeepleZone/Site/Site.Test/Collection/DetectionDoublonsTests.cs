using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Collection;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Site.Collection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Site.Test.Collection;

public class DetectionDoublonsTests
{
    private static JeuResumeDto Jeu(string nom, short? annee = null, string? editeur = null)
    {
        return new JeuResumeDto(Guid.NewGuid(), nom, annee, editeur, false);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ca")]
    [InlineData(" Ca ")]
    public async Task TrouverAsync_TexteTropCourt_NeCherchePas(string? nom)
    {
        // Arrange
        var domaine = A.Fake<IDomaineCollection>();
        var detection = new DetectionDoublonsParRecherche(domaine);

        // Act
        var resultat = await detection.TrouverAsync(nom!);

        // Assert
        resultat.Should().BeEmpty();
        A.CallTo(domaine).MustNotHaveHappened();
    }

    [Fact]
    public async Task TrouverAsync_CherchePremiereAPageAvecLeTexteSansEspaces()
    {
        // Arrange
        var domaine = A.Fake<IDomaineCollection>();
        var catan = Jeu("Catan");
        A.CallTo(() => domaine.RechercherJeuxAsync(A<RechercherJeuxRequete>._))
            .Returns(new RechercherJeuxReponse([catan], 1));
        var detection = new DetectionDoublonsParRecherche(domaine);

        // Act
        var resultat = await detection.TrouverAsync("  Catan ");

        // Assert
        resultat.Should().Equal(catan);
        A.CallTo(() => domaine.RechercherJeuxAsync(
                A<RechercherJeuxRequete>.That.Matches(r => r.Texte == "Catan" && r.Page == 1 && r.TaillePage == DetectionDoublonsParRecherche.NombreMaximal)))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(2015, "Matagot", "2015 · Matagot")]
    [InlineData(2015, null, "2015")]
    [InlineData(null, "Matagot", "Matagot")]
    [InlineData(null, " ", "Année et éditeur inconnus")]
    [InlineData(null, null, "Année et éditeur inconnus")]
    public void Detail_ListeAnneeEtEditeurConnus(int? annee, string? editeur, string attendu)
    {
        // Arrange
        var jeu = Jeu("Pandemic", (short?)annee, editeur);

        // Act / Assert
        PresentationJeu.Detail(jeu).Should().Be(attendu);
    }
}

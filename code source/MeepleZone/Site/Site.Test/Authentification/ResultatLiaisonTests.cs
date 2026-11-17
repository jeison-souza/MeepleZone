using FluentAssertions;
using MeepleZone.Site.Authentification.Liaison;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Site.Test.Authentification;

public class ResultatLiaisonTests
{
    [Theory]
    [InlineData(ResultatLiaison.Reussie, "reussie")]
    [InlineData(ResultatLiaison.DejaLiee, "deja-liee")]
    [InlineData(ResultatLiaison.DejaUtilisee, "deja-utilisee")]
    [InlineData(ResultatLiaison.Echec, "echec")]
    public void Valeur_RetourneLeTexteDeLAdresse(ResultatLiaison resultat, string attendu)
    {
        // Act
        var valeur = resultat.Valeur();

        // Assert
        valeur.Should().Be(attendu);
    }

    [Theory]
    [InlineData(ResultatLiaison.Reussie)]
    [InlineData(ResultatLiaison.DejaLiee)]
    [InlineData(ResultatLiaison.DejaUtilisee)]
    [InlineData(ResultatLiaison.Echec)]
    public void Lire_AvecLaValeurDeLIssue_RetourneLIssue(ResultatLiaison resultat)
    {
        // Act
        var lu = ResultatLiaisonExtensions.Lire(resultat.Valeur());

        // Assert
        lu.Should().Be(resultat);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Reussie")]
    [InlineData("<script>alert(1)</script>")]
    public void Lire_AvecValeurAbsenteOuInconnue_RetourneNull(string? valeur)
    {
        // Act
        var lu = ResultatLiaisonExtensions.Lire(valeur);

        // Assert
        lu.Should().BeNull();
    }

    [Theory]
    [InlineData(ResultatLiaison.Reussie, "est maintenant associé")]
    [InlineData(ResultatLiaison.DejaLiee, "déjà associé à votre compte")]
    [InlineData(ResultatLiaison.DejaUtilisee, "supprimez le compte MeepleZone le plus récent")]
    [InlineData(ResultatLiaison.Echec, "n'a pas pu être faite")]
    public void Message_MentionneLeFournisseurEtLIssue(ResultatLiaison resultat, string fragment)
    {
        // Act
        var message = resultat.Message("Microsoft");

        // Assert
        message.Should().Contain("Microsoft").And.Contain(fragment);
    }

    [Fact]
    public async Task Fictif_ListeDeuxConnexionsDuMemeFournisseur()
    {
        // Arrange
        IFournisseursLies source = new FournisseursLiesFictif();

        // Act
        var liste = await source.ListerAsync();

        // Assert
        liste.Should().HaveCount(2).And.OnlyContain(l => l.Nom == "Google");
    }
}

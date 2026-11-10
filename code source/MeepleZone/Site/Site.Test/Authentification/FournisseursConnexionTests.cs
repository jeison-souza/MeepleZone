using System.Linq;
using FluentAssertions;
using MeepleZone.Site.Authentification;
using Xunit;

namespace MeepleZone.Site.Test.Authentification;

public class FournisseursConnexionTests
{
    [Theory]
    [InlineData("Google")]
    [InlineData("google")]
    [InlineData("Microsoft")]
    [InlineData("MICROSOFT")]
    public void Trouver_AvecNomDeLaListe_RetourneLeFournisseur(string nom)
    {
        // Act
        var fournisseur = FournisseursConnexion.Trouver(nom);

        // Assert
        fournisseur.Should().NotBeNull();
        fournisseur!.Nom.Should().BeEquivalentTo(nom);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Facebook")]
    [InlineData("Google ")]
    [InlineData("<script>alert(1)</script>")]
    public void Trouver_AvecNomAbsentDeLaListe_RetourneNull(string? nom)
    {
        // Act
        var fournisseur = FournisseursConnexion.Trouver(nom);

        // Assert
        fournisseur.Should().BeNull();
    }

    [Theory]
    [InlineData("Google", "Microsoft")]
    [InlineData("microsoft", "Google")]
    public void Autres_AvecFournisseurConnu_ExclutCeluiQuiVientDEtreUtilise(string utilise, string attendu)
    {
        // Act
        var autres = FournisseursConnexion.Autres(utilise);

        // Assert
        autres.Select(fournisseur => fournisseur.Nom).Should().Equal(attendu);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Facebook")]
    public void Autres_AvecFournisseurInconnu_RetourneLaListeComplete(string? utilise)
    {
        // Act
        var autres = FournisseursConnexion.Autres(utilise);

        // Assert
        autres.Should().Equal(FournisseursConnexion.Tous);
    }
}

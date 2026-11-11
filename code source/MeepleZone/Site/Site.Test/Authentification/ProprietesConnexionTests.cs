using FluentAssertions;
using MeepleZone.Site.Authentification;
using Xunit;

namespace MeepleZone.Site.Test.Authentification;

public class ProprietesConnexionTests
{
    [Fact]
    public void Creer_AvecCreationTrue_PoseLeMarqueurDeCreation()
    {
        // Act
        var proprietes = ProprietesConnexion.Creer("/profil", "true");

        // Assert
        proprietes.Items.Should().Contain(GestionConnexionCompte.CleForcerCreation, GestionConnexionCompte.ValeurForcerCreation);
        proprietes.RedirectUri.Should().Be("/profil");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("false")]
    [InlineData("True")]
    [InlineData("1")]
    [InlineData("true ")]
    public void Creer_AvecAutreValeurQueTrue_NePosePasLeMarqueur(string? creation)
    {
        // Act
        var proprietes = ProprietesConnexion.Creer("/profil", creation);

        // Assert
        proprietes.Items.Should().NotContainKey(GestionConnexionCompte.CleForcerCreation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("//evil.com")]
    [InlineData("https://evil.com")]
    public void Creer_AvecRetourInvalide_RevientALAccueil(string? retour)
    {
        // Act
        var proprietes = ProprietesConnexion.Creer(retour, "true");

        // Assert
        proprietes.RedirectUri.Should().Be("/");
    }
}

using FluentAssertions;
using MeepleZone.Systeme.Entites.Attributs;
using System.Reflection;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class DemandeVerificationTests
{
    [Fact]
    public void DemandeVerification_CreeLe_SetEstPrivee()
    {
        // Arrange
        var propriete = typeof(DemandeVerification).GetProperty(nameof(DemandeVerification.CreeLe));

        // Act
        var setPublic = propriete!.GetSetMethod();
        var setPrive = propriete!.GetSetMethod(nonPublic: true);

        // Assert
        setPublic.Should().BeNull();
        setPrive.Should().NotBeNull();
    }

    [Theory]
    [InlineData(nameof(DemandeVerification.Id), false)]
    [InlineData(nameof(DemandeVerification.UtilisateurId), false)]
    [InlineData(nameof(DemandeVerification.Courriel), false)]
    [InlineData(nameof(DemandeVerification.CreeLe), false)]
    [InlineData(nameof(DemandeVerification.EnvoyeLe), false)]
    [InlineData(nameof(DemandeVerification.DureeValidite), false)]
    public void DemandeVerification_Propriete_PossedeAttributTechnique(string nom, bool attendu)
    {
        // Arrange
        var propriete = typeof(DemandeVerification).GetProperty(nom);

        // Act
        var possedeAttributTechnique = propriete!.GetCustomAttribute<AttributTechnique>() != null;

        // Assert
        possedeAttributTechnique.Should().Be(attendu);
    }
}

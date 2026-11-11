using FluentAssertions;
using MeepleZone.Systeme.Entites.Attributs;
using System.Reflection;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class ModeJeuTests
{
    [Theory]
    [InlineData(nameof(ModeJeu.CreeLe))]
    public void ModeJeu_Propriete_SetEstPrivee(string nom)
    {
        // Arrange
        var propriete = typeof(ModeJeu).GetProperty(nom);

        // Act
        var setPublic = propriete!.GetSetMethod();
        var setPrive = propriete!.GetSetMethod(nonPublic: true);

        // Assert
        setPublic.Should().BeNull();
        setPrive.Should().NotBeNull();
    }

    [Theory]
    [InlineData(nameof(ModeJeu.Id), false)]
    [InlineData(nameof(ModeJeu.Nom), false)]
    [InlineData(nameof(ModeJeu.Description), false)]
    [InlineData(nameof(ModeJeu.CreeLe), false)]
    public void ModeJeu_Propriete_PossedeAttributTechnique(string nom, bool attendu)
    {
        // Arrange
        var propriete = typeof(ModeJeu).GetProperty(nom);

        // Act
        var possedeAttributTechnique = propriete!.GetCustomAttribute<AttributTechnique>() != null;

        // Assert
        possedeAttributTechnique.Should().Be(attendu);
    }
}

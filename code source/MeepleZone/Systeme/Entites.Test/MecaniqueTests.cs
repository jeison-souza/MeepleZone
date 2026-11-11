using FluentAssertions;
using MeepleZone.Systeme.Entites.Attributs;
using System.Reflection;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class MecaniqueTests
{
    [Theory]
    [InlineData(nameof(Mecanique.CreeLe))]
    public void Mecanique_Propriete_SetEstPrivee(string nom)
    {
        // Arrange
        var propriete = typeof(Mecanique).GetProperty(nom);

        // Act
        var setPublic = propriete!.GetSetMethod();
        var setPrive = propriete!.GetSetMethod(nonPublic: true);

        // Assert
        setPublic.Should().BeNull();
        setPrive.Should().NotBeNull();
    }

    [Theory]
    [InlineData(nameof(Mecanique.Id), false)]
    [InlineData(nameof(Mecanique.Nom), false)]
    [InlineData(nameof(Mecanique.Description), false)]
    [InlineData(nameof(Mecanique.BggId), false)]
    [InlineData(nameof(Mecanique.CreeLe), false)]
    public void Mecanique_Propriete_PossedeAttributTechnique(string nom, bool attendu)
    {
        // Arrange
        var propriete = typeof(Mecanique).GetProperty(nom);

        // Act
        var possedeAttributTechnique = propriete!.GetCustomAttribute<AttributTechnique>() != null;

        // Assert
        possedeAttributTechnique.Should().Be(attendu);
    }
}

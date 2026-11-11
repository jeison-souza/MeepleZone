using FluentAssertions;
using MeepleZone.Systeme.Entites.Attributs;
using System.Reflection;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class JeuMecaniqueTests
{
    [Theory]
    [InlineData(nameof(JeuMecanique.JeuId), false)]
    [InlineData(nameof(JeuMecanique.MecaniqueId), false)]
    public void JeuMecanique_Propriete_PossedeAttributTechnique(string nom, bool attendu)
    {
        // Arrange
        var propriete = typeof(JeuMecanique).GetProperty(nom);

        // Act
        var possedeAttributTechnique = propriete!.GetCustomAttribute<AttributTechnique>() != null;

        // Assert
        possedeAttributTechnique.Should().Be(attendu);
    }
}

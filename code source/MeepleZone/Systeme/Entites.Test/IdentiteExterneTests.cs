using FluentAssertions;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class IdentiteExterneTests
{
    [Theory]
    [InlineData(nameof(IdentiteExterne.CreeLe))]
    [InlineData(nameof(IdentiteExterne.ModifieLe))]
    public void IdentiteExterne_Propriete_SetEstPrivee(string nom)
    {
        // Arrange
        var propriete = typeof(IdentiteExterne).GetProperty(nom);

        // Act
        var setPublic = propriete!.GetSetMethod();
        var setPrive = propriete!.GetSetMethod(nonPublic: true);

        // Assert
        setPublic.Should().BeNull();
        setPrive.Should().NotBeNull();
    }
}

using FluentAssertions;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class AnnexeTests
{
    [Theory]
    [InlineData(nameof(Annexe.CreeLe))]
    public void Annexe_Propriete_SetEstPrivee(string nom)
    {
        // Arrange
        var propriete = typeof(Annexe).GetProperty(nom);

        // Act
        var setPublic = propriete!.GetSetMethod();
        var setPrive = propriete!.GetSetMethod(nonPublic: true);

        // Assert
        setPublic.Should().BeNull();
        setPrive.Should().NotBeNull();
    }
}

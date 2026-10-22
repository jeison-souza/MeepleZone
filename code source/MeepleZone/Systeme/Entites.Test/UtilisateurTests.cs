using FluentAssertions;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class UtilisateurTests
{
    [Theory]
    [InlineData(nameof(Utilisateur.CreeLe))]
    [InlineData(nameof(Utilisateur.ModifieLe))]
    public void Utilisateur_Propriete_SetEstPrivee(string nom)
    {
        // Arrange
        var propriete = typeof(Utilisateur).GetProperty(nom);

        // Act
        var setPublic = propriete!.GetSetMethod();
        var setPrive = propriete!.GetSetMethod(nonPublic: true);

        // Assert
        setPublic.Should().BeNull();
        setPrive.Should().NotBeNull();
    }
}

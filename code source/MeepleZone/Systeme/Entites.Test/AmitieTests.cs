using FluentAssertions;
using MeepleZone.Systeme.Entites.Attributs;
using MeepleZone.Systeme.Entites.Interfaces;
using System.Reflection;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class AmitieTests
{
    [Fact]
    public void Amitie_CreeLe_SetEstPrivee()
    {
        // Arrange
        var propriete = typeof(Amitie).GetProperty(nameof(Amitie.CreeLe));

        // Act
        var setPublic = propriete!.GetSetMethod();
        var setPrive = propriete!.GetSetMethod(nonPublic: true);

        // Assert
        setPublic.Should().BeNull();
        setPrive.Should().NotBeNull();
    }

    [Theory]
    [InlineData(nameof(Amitie.UtilisateurAId), false)]
    [InlineData(nameof(Amitie.UtilisateurBId), false)]
    [InlineData(nameof(Amitie.CreeLe), false)]
    public void Amitie_Propriete_PossedeAttributTechnique(string nom, bool attendu)
    {
        // Arrange
        var propriete = typeof(Amitie).GetProperty(nom);

        // Act
        var possedeAttributTechnique = propriete!.GetCustomAttribute<AttributTechnique>() != null;

        // Assert
        possedeAttributTechnique.Should().Be(attendu);
    }

    [Fact]
    public void Amitie_Type_NImplementePasIModifieLe()
    {
        // Arrange
        var type = typeof(Amitie);

        // Act
        var estModifiable = typeof(IModifieLe).IsAssignableFrom(type);

        // Assert
        estModifiable.Should().BeFalse();
    }
}

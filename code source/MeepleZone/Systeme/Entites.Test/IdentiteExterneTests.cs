using FluentAssertions;
using MeepleZone.Systeme.Entites.Attributs;
using System.Reflection;
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

    [Theory]
    [InlineData(nameof(IdentiteExterne.Id), false)]
    [InlineData(nameof(IdentiteExterne.UtilisateurId), false)]
    [InlineData(nameof(IdentiteExterne.Fournisseur), false)]
    [InlineData(nameof(IdentiteExterne.IdentifiantCompteExterne), false)]
    [InlineData(nameof(IdentiteExterne.Courriel), false)]
    [InlineData(nameof(IdentiteExterne.DerniereConnexionLe), true)]
    [InlineData(nameof(IdentiteExterne.CreeLe), false)]
    [InlineData(nameof(IdentiteExterne.ModifieLe), false)]
    public void IdentiteExterne_Propriete_PossedeAttributTechnique(string nom, bool attendu)
    {
        // Arrange
        var propriete = typeof(IdentiteExterne).GetProperty(nom);

        // Act
        var possedeAttributTechnique = propriete!.GetCustomAttribute<AttributTechnique>() != null;

        // Assert
        possedeAttributTechnique.Should().Be(attendu);
    }
}

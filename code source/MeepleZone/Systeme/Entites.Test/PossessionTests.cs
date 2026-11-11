using FluentAssertions;
using MeepleZone.Systeme.Entites.Attributs;
using System.Reflection;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class PossessionTests
{
    [Theory]
    [InlineData(nameof(Possession.CreeLe))]
    [InlineData(nameof(Possession.ModifieLe))]
    public void Possession_Propriete_SetEstPrivee(string nom)
    {
        // Arrange
        var propriete = typeof(Possession).GetProperty(nom);

        // Act
        var setPublic = propriete!.GetSetMethod();
        var setPrive = propriete!.GetSetMethod(nonPublic: true);

        // Assert
        setPublic.Should().BeNull();
        setPrive.Should().NotBeNull();
    }

    [Theory]
    [InlineData(nameof(Possession.Id), false)]
    [InlineData(nameof(Possession.UtilisateurId), false)]
    [InlineData(nameof(Possession.JeuId), false)]
    [InlineData(nameof(Possession.Note), false)]
    [InlineData(nameof(Possession.Commentaire), false)]
    [InlineData(nameof(Possession.PartiesJouees), false)]
    [InlineData(nameof(Possession.CreeLe), false)]
    [InlineData(nameof(Possession.ModifieLe), false)]
    public void Possession_Propriete_PossedeAttributTechnique(string nom, bool attendu)
    {
        // Arrange
        var propriete = typeof(Possession).GetProperty(nom);

        // Act
        var possedeAttributTechnique = propriete!.GetCustomAttribute<AttributTechnique>() != null;

        // Assert
        possedeAttributTechnique.Should().Be(attendu);
    }
}

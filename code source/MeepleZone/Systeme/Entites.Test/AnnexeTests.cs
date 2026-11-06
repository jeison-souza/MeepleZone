using FluentAssertions;
using MeepleZone.Systeme.Entites.Attributs;
using System.Reflection;
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

    [Theory]
    [InlineData(nameof(Annexe.Id), false)]
    [InlineData(nameof(Annexe.NomFichier), false)]
    [InlineData(nameof(Annexe.Contenu), false)]
    [InlineData(nameof(Annexe.TypeMime), false)]
    [InlineData(nameof(Annexe.TailleOctets), false)]
    [InlineData(nameof(Annexe.CreeParId), false)]
    [InlineData(nameof(Annexe.CreeLe), false)]
    public void Annexe_Propriete_PossedeAttributTechnique(string nom, bool attendu)
    {
        // Arrange
        var propriete = typeof(Annexe).GetProperty(nom);

        // Act
        var possedeAttributTechnique = propriete!.GetCustomAttribute<AttributTechnique>() != null;

        // Assert
        possedeAttributTechnique.Should().Be(attendu);
    }
}

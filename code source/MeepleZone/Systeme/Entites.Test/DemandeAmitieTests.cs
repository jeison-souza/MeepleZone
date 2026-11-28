using FluentAssertions;
using MeepleZone.Systeme.Entites.Attributs;
using System.Reflection;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class DemandeAmitieTests
{
    [Theory]
    [InlineData(nameof(DemandeAmitie.CreeLe))]
    [InlineData(nameof(DemandeAmitie.ModifieLe))]
    public void DemandeAmitie_Propriete_SetEstPrivee(string nom)
    {
        // Arrange
        var propriete = typeof(DemandeAmitie).GetProperty(nom);

        // Act
        var setPublic = propriete!.GetSetMethod();
        var setPrive = propriete!.GetSetMethod(nonPublic: true);

        // Assert
        setPublic.Should().BeNull();
        setPrive.Should().NotBeNull();
    }

    [Theory]
    [InlineData(nameof(DemandeAmitie.Id), false)]
    [InlineData(nameof(DemandeAmitie.DemandeurId), false)]
    [InlineData(nameof(DemandeAmitie.DestinataireId), false)]
    [InlineData(nameof(DemandeAmitie.CourrielDestinataire), false)]
    [InlineData(nameof(DemandeAmitie.CodeDestinataire), false)]
    [InlineData(nameof(DemandeAmitie.Statut), false)]
    [InlineData(nameof(DemandeAmitie.CreeLe), false)]
    [InlineData(nameof(DemandeAmitie.ModifieLe), false)]
    public void DemandeAmitie_Propriete_PossedeAttributTechnique(string nom, bool attendu)
    {
        // Arrange
        var propriete = typeof(DemandeAmitie).GetProperty(nom);

        // Act
        var possedeAttributTechnique = propriete!.GetCustomAttribute<AttributTechnique>() != null;

        // Assert
        possedeAttributTechnique.Should().Be(attendu);
    }
}

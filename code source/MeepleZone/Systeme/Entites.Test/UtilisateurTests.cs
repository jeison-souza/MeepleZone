using FluentAssertions;
using MeepleZone.Systeme.Entites.Attributs;
using System.Reflection;
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

    [Theory]
    [InlineData(nameof(Utilisateur.Id), false)]
    [InlineData(nameof(Utilisateur.Courriel), false)]
    [InlineData(nameof(Utilisateur.CodeAmitie), false)]
    [InlineData(nameof(Utilisateur.NomAffichage), false)]
    [InlineData(nameof(Utilisateur.Role), false)]
    [InlineData(nameof(Utilisateur.VisibiliteCollection), false)]
    [InlineData(nameof(Utilisateur.CreeLe), false)]
    [InlineData(nameof(Utilisateur.ModifieLe), false)]
    [InlineData(nameof(Utilisateur.CourrielVerifieLe), false)]
    [InlineData(nameof(Utilisateur.PhotoId), false)]
    public void Utilisateur_Propriete_PossedeAttributTechnique(string nom, bool attendu)
    {
        // Arrange
        var propriete = typeof(Utilisateur).GetProperty(nom);

        // Act
        var possedeAttributTechnique = propriete!.GetCustomAttribute<AttributTechnique>() != null;

        // Assert
        possedeAttributTechnique.Should().Be(attendu);
    }
}

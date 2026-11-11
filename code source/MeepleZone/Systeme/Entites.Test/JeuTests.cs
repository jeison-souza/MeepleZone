using FluentAssertions;
using MeepleZone.Systeme.Entites.Attributs;
using System.Reflection;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class JeuTests
{
    [Theory]
    [InlineData(nameof(Jeu.CreeLe))]
    [InlineData(nameof(Jeu.ModifieLe))]
    public void Jeu_Propriete_SetEstPrivee(string nom)
    {
        // Arrange
        var propriete = typeof(Jeu).GetProperty(nom);

        // Act
        var setPublic = propriete!.GetSetMethod();
        var setPrive = propriete!.GetSetMethod(nonPublic: true);

        // Assert
        setPublic.Should().BeNull();
        setPrive.Should().NotBeNull();
    }

    [Theory]
    [InlineData(nameof(Jeu.Id), false)]
    [InlineData(nameof(Jeu.Nom), false)]
    [InlineData(nameof(Jeu.AnneePublication), false)]
    [InlineData(nameof(Jeu.JoueursMin), false)]
    [InlineData(nameof(Jeu.JoueursMax), false)]
    [InlineData(nameof(Jeu.JoueursMeilleur), false)]
    [InlineData(nameof(Jeu.DureeMinMinutes), false)]
    [InlineData(nameof(Jeu.DureeMaxMinutes), false)]
    [InlineData(nameof(Jeu.AgeMin), false)]
    [InlineData(nameof(Jeu.Complexite), false)]
    [InlineData(nameof(Jeu.ModeJeuId), false)]
    [InlineData(nameof(Jeu.Editeur), false)]
    [InlineData(nameof(Jeu.BggId), false)]
    [InlineData(nameof(Jeu.CouvertureId), false)]
    [InlineData(nameof(Jeu.CouvertureUrl), false)]
    [InlineData(nameof(Jeu.Statut), false)]
    [InlineData(nameof(Jeu.CreeParId), false)]
    [InlineData(nameof(Jeu.CreeLe), false)]
    [InlineData(nameof(Jeu.ModifieLe), false)]
    public void Jeu_Propriete_PossedeAttributTechnique(string nom, bool attendu)
    {
        // Arrange
        var propriete = typeof(Jeu).GetProperty(nom);

        // Act
        var possedeAttributTechnique = propriete!.GetCustomAttribute<AttributTechnique>() != null;

        // Assert
        possedeAttributTechnique.Should().Be(attendu);
    }
}

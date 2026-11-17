using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Extensions;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Entites;
using System;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Extensions;

public class IdentiteExterneExtensionsTest : BaseTest<IdentiteExterne>
{
    [Fact]
    public void VersDto_EnToutTemps_RetournerConnexionDto()
    {
        // Arrange
        var fournisseur = CreerFixture<string>();
        var courriel = CreerFixture<string>();
        var creeLe = new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);
        InstanceTest.Fournisseur = fournisseur;
        InstanceTest.Courriel = courriel;
        // CreeLe et ModifieLe ont un set privé : posés par réflexion avec des valeurs distinctes pour détecter une confusion de colonne.
        FixerDate(InstanceTest, nameof(IdentiteExterne.CreeLe), creeLe);
        FixerDate(InstanceTest, nameof(IdentiteExterne.ModifieLe), creeLe.AddDays(1));
        InstanceTest.DerniereConnexionLe = creeLe.AddDays(2);
        var attendu = new ConnexionDto(fournisseur, courriel, creeLe);

        // Act
        var result = InstanceTest.VersDto();

        // Assert
        result.Should().Be(attendu);
    }

    [Fact]
    public void VersDto_CourrielAbsent_ConserverLAbsenceDeCourriel()
    {
        // Arrange
        InstanceTest.Courriel = null;

        // Act
        var result = InstanceTest.VersDto();

        // Assert
        result.Courriel.Should().BeNull();
    }

    private static void FixerDate(IdentiteExterne identite, string propriete, DateTimeOffset valeur)
    {
        typeof(IdentiteExterne).GetProperty(propriete)!.SetValue(identite, valeur);
    }
}

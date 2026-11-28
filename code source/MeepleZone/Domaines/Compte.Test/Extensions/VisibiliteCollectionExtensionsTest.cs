using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Extensions;
using System;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Extensions;

public class VisibiliteCollectionExtensionsTest
{
    // Les valeurs viennent de la contrainte CHECK de la colonne visibilite_collection (schema-donnees.md), pas du code testé.
    [Theory]
    [InlineData(VisibiliteCollection.Privee, "privee")]
    [InlineData(VisibiliteCollection.Amis, "amis")]
    public void VersValeur_VisibiliteConnue_RetournerLaValeurDuSchema(VisibiliteCollection visibilite, string attendu)
    {
        // Act
        var result = visibilite.VersValeur();

        // Assert
        result.Should().Be(attendu);
    }

    [Fact]
    public void VersValeur_VisibiliteInconnue_LancerUneException()
    {
        // Arrange
        var visibilite = (VisibiliteCollection)99;

        // Act
        var action = () => visibilite.VersValeur();

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("privee", VisibiliteCollection.Privee)]
    [InlineData("amis", VisibiliteCollection.Amis)]
    public void VersVisibiliteCollection_ValeurConnue_RetournerLaVisibilite(string valeur, VisibiliteCollection attendue)
    {
        // Act
        var result = valeur.VersVisibiliteCollection();

        // Assert
        result.Should().Be(attendue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("publique")]
    [InlineData("Amis")]
    public void VersVisibiliteCollection_ValeurInconnue_LancerUneException(string valeur)
    {
        // Act
        var action = () => valeur.VersVisibiliteCollection();

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}

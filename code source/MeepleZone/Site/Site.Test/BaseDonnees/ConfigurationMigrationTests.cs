using FakeItEasy;
using FluentAssertions;
using MeepleZone.Site.BaseDonnees;
using MeepleZone.Systeme.BaseTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using Xunit;

namespace MeepleZone.Site.Test.BaseDonnees;

public class ConfigurationMigrationTests : BaseTest<object>
{
    private const string CleChaine = "ConnectionStrings:MeepleZoneMigration";

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void LireChaineConnexion_ChainePresente_RetourneLaChaine(string nomEnvironnement)
    {
        // Arrange
        var chaineAttendue = CreerFixture<string>();
        var configuration = CreerConfiguration(new()
        {
            [CleChaine] = chaineAttendue
        });
        var environnement = CreerEnvironnement(nomEnvironnement);

        // Act
        var chaine = ConfigurationMigration.LireChaineConnexion(configuration, environnement);

        // Assert
        chaine.Should().Be(chaineAttendue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void LireChaineConnexion_ChaineAbsenteEnDeveloppement_RetourneNull(string? valeur)
    {
        // Arrange
        var configuration = CreerConfiguration(new()
        {
            [CleChaine] = valeur
        });
        var environnement = CreerEnvironnement("Development");

        // Act
        var chaine = ConfigurationMigration.LireChaineConnexion(configuration, environnement);

        // Assert
        chaine.Should().BeNull();
    }

    [Theory]
    [InlineData("Production", null)]
    [InlineData("Production", "")]
    [InlineData("Production", "   ")]
    [InlineData("Staging", null)]
    [InlineData("Staging", "")]
    [InlineData("Staging", "   ")]
    public void LireChaineConnexion_ChaineAbsenteHorsDeveloppement_LanceException(string nomEnvironnement, string? valeur)
    {
        // Arrange
        var configuration = CreerConfiguration(new()
        {
            [CleChaine] = valeur
        });
        var environnement = CreerEnvironnement(nomEnvironnement);

        // Act
        Action action = () => ConfigurationMigration.LireChaineConnexion(configuration, environnement);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage($"*{CleChaine}*");
    }

    [Fact]
    public void LireChaineConnexion_ChaineInexistanteEnDeveloppement_RetourneNull()
    {
        // Arrange
        var configuration = CreerConfiguration(new());
        var environnement = CreerEnvironnement("Development");

        // Act
        var chaine = ConfigurationMigration.LireChaineConnexion(configuration, environnement);

        // Assert
        chaine.Should().BeNull();
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void LireChaineConnexion_ChaineInexistanteHorsDeveloppement_LanceException(string nomEnvironnement)
    {
        // Arrange
        var configuration = CreerConfiguration(new());
        var environnement = CreerEnvironnement(nomEnvironnement);

        // Act
        Action action = () => ConfigurationMigration.LireChaineConnexion(configuration, environnement);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage($"*{CleChaine}*");
    }

    private IHostEnvironment CreerEnvironnement(string nom)
    {
        var environnement = CreerFausse<IHostEnvironment>();
        environnement.CallsTo(x => x.EnvironmentName).Returns(nom);
        return environnement.FakedObject;
    }

    private static IConfiguration CreerConfiguration(Dictionary<string, string?> valeurs)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(valeurs).Build();
    }
}

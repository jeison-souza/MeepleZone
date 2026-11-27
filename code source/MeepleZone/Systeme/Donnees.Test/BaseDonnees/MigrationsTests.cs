using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.BaseDonnees;

public class MigrationsTests(ModeleDonneesFixture fixture) : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _fixture = fixture;

    // L'application migre au démarrage : un modèle sans migration l'empêcherait de démarrer en production.
    [Fact]
    public void Migrations_ModeleActuel_AucunChangementEnAttente()
    {
        // Act
        var changementsEnAttente = _fixture.Contexte.Database.HasPendingModelChanges();

        // Assert
        changementsEnAttente.Should().BeFalse("chaque changement du modèle doit avoir sa migration");
    }
}

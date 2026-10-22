using AutoFixture.Xunit3;
using FluentAssertions;
using MeepleZone.Systeme.Entites;
using System;
using Xunit;

namespace MeepleZone.Systeme.Donnee.Test;

public class EntitesTests : IClassFixture<ModeleDonneesFixture>, IDisposable
{
    private readonly ModeleDonneesFixture _modele;

    public EntitesTests(ModeleDonneesFixture modele)
    {
        _modele = modele;
    }

    public void Dispose()
    {
        _modele.Contexte.ChangeTracker.Clear();
    }

    [Theory]
    [InlineAutoData(nameof(Utilisateur.CreeLe))]
    [InlineAutoData(nameof(Utilisateur.ModifieLe))]
    public void Utilisateur_Date_EstEcriteParLeContexteMalgreLeSetPrive(string nom, DateTimeOffset valeurAttendue)
    {
        // Arranger
        var entite = new Utilisateur { Id = Guid.NewGuid() };
        _modele.Contexte.Attach(entite);

        // Agir
        _modele.Contexte.Entry(entite).Property(nom).CurrentValue = valeurAttendue;

        // Assertion
        var valeurEnMemoire = entite.GetType().GetProperty(nom)!.GetValue(entite);
        valeurEnMemoire.Should().Be(valeurAttendue);
        _modele.Contexte.ChangeTracker.Clear();
    }

    [Theory]
    [InlineAutoData(nameof(IdentiteExterne.CreeLe))]
    [InlineAutoData(nameof(IdentiteExterne.ModifieLe))]
    public void IdentiteExterne_Date_EstEcriteParLeContexteMalgreLeSetPrive(string nom, DateTimeOffset valeurAttendue)
    {
        // Arranger
        var entite = new IdentiteExterne { Id = Guid.NewGuid() };
        _modele.Contexte.Attach(entite);

        // Agir
        _modele.Contexte.Entry(entite).Property(nom).CurrentValue = valeurAttendue;

        // Assertion
        var valeurEnMemoire = entite.GetType().GetProperty(nom)!.GetValue(entite);
        valeurEnMemoire.Should().Be(valeurAttendue);
        _modele.Contexte.ChangeTracker.Clear();
    }

    [Theory]
    [InlineAutoData(nameof(Annexe.CreeLe))]
    public void Annexe_Date_EstEcriteParLeContexteMalgreLeSetPrive(string nom, DateTimeOffset valeurAttendue)
    {
        // Arranger
        var entite = new Annexe { Id = Guid.NewGuid() };
        _modele.Contexte.Attach(entite);

        // Agir
        _modele.Contexte.Entry(entite).Property(nom).CurrentValue = valeurAttendue;

        // Assertion
        var valeurEnMemoire = entite.GetType().GetProperty(nom)!.GetValue(entite);
        valeurEnMemoire.Should().Be(valeurAttendue);
        _modele.Contexte.ChangeTracker.Clear();
    }
}

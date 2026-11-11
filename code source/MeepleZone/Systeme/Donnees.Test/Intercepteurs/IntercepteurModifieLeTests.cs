using AutoFixture;
using FluentAssertions;
using MeepleZone.Systeme.Donnees.Intercepteurs;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Entites.Interfaces;
using Microsoft.Extensions.Time.Testing;
using System;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.Intercepteurs;

public class IntercepteurModifieLeTests(ModeleDonneesFixture modele) : IClassFixture<ModeleDonneesFixture>, IDisposable
{
    private readonly ModeleDonneesFixture _modele = modele;

    private readonly Fixture _fixture = new();

    public void Dispose() => _modele.Contexte.ChangeTracker.Clear();

    [Fact]
    public void AppliquerModifierLe_ColonneDeMetierModifiee_EcritModifieLe()
    {
        // Arranger
        var fournisseurTemps = new FakeTimeProvider();
        var intercepteur = new IntercepteurModifieLe(fournisseurTemps);
        var entite = new IdentiteExterne()
        {
            Id = Guid.NewGuid()
        };
        _modele.Contexte.Attach(entite);
        entite.IdentifiantCompteExterne = _fixture.Create<string>();

        // Agir
        intercepteur.AppliquerModifierLe(_modele.Contexte.ChangeTracker);

        // Assertion
        entite.ModifieLe.Should().Be(fournisseurTemps.GetUtcNow());
    }

    [Fact]
    public void AppliquerModifierLe_SeulementColonneTechniqueModifiee_NeEcritPasModifieLe()
    {
        // Arranger
        var fournisseurTemps = new FakeTimeProvider();
        var intercepteur = new IntercepteurModifieLe(fournisseurTemps);
        var entite = new IdentiteExterne()
        {
            Id = Guid.NewGuid()
        };
        _modele.Contexte.Attach(entite);
        entite.DerniereConnexionLe = _fixture.Create<DateTimeOffset>();

        // Agir
        intercepteur.AppliquerModifierLe(_modele.Contexte.ChangeTracker);

        // Assertion
        entite.ModifieLe.Should().Be(default);
    }

    [Fact]
    public void AppliquerModifierLe_ColonneDeMetierEtTechniqueModifiees_EcritModifieLe()
    {
        // Arranger
        var fournisseurTemps = new FakeTimeProvider();
        var intercepteur = new IntercepteurModifieLe(fournisseurTemps);
        var entite = new IdentiteExterne()
        {
            Id = Guid.NewGuid()
        };
        _modele.Contexte.Attach(entite);
        entite.DerniereConnexionLe = _fixture.Create<DateTimeOffset>();
        entite.Courriel = _fixture.Create<string>();

        // Agir
        intercepteur.AppliquerModifierLe(_modele.Contexte.ChangeTracker);

        // Assertion
        entite.ModifieLe.Should().Be(fournisseurTemps.GetUtcNow());
    }

    [Fact]
    public void AppliquerModifierLe_EntiteAjoutee_NeEcritPasModifieLe()
    {
        // Arranger
        var fournisseurTemps = new FakeTimeProvider();
        var intercepteur = new IntercepteurModifieLe(fournisseurTemps);
        var entite = new IdentiteExterne()
        {
            Id = Guid.NewGuid(),
            DerniereConnexionLe = _fixture.Create<DateTimeOffset>(),
            Courriel = _fixture.Create<string>(),
            IdentifiantCompteExterne = _fixture.Create<string>()
        };
        _modele.Contexte.Add(entite);

        // Agir
        intercepteur.AppliquerModifierLe(_modele.Contexte.ChangeTracker);

        // Assertion
        entite.ModifieLe.Should().Be(default);
    }

    [Fact]
    public void AppliquerModifierLe_EntitePasModifiee_NeEcritPasModifieLe()
    {
        // Arranger
        var fournisseurTemps = new FakeTimeProvider();
        var intercepteur = new IntercepteurModifieLe(fournisseurTemps);
        var entite = new IdentiteExterne()
        {
            Id = Guid.NewGuid(),
            DerniereConnexionLe = _fixture.Create<DateTimeOffset>(),
            Courriel = _fixture.Create<string>(),
            IdentifiantCompteExterne = _fixture.Create<string>()
        };
        _modele.Contexte.Attach(entite);

        // Agir
        intercepteur.AppliquerModifierLe(_modele.Contexte.ChangeTracker);

        // Assertion
        entite.ModifieLe.Should().Be(default);
    }

    [Fact]
    public void AppliquerModifierLe_EntiteSansIModifieLe_NeLancePasException()
    {
        // Arranger
        var fournisseurTemps = new FakeTimeProvider();
        var intercepteur = new IntercepteurModifieLe(fournisseurTemps);
        var entite = new Annexe()
        {
            Id = Guid.NewGuid(),
        };
        _modele.Contexte.Attach(entite);
        entite.NomFichier = _fixture.Create<string>();
        entite.TailleOctets = _fixture.Create<int>();
        entite.TypeMime = _fixture.Create<string>();

        // Agir
        var action = () => intercepteur.AppliquerModifierLe(_modele.Contexte.ChangeTracker);

        // Assertion
        action.Should().NotThrow();
    }

    [Fact]
    public void AppliquerModifierLe_EntiteModifieeApresUnDelai_EcritLHeureCourante()
    {
        // Arranger
        var fournisseurTemps = new FakeTimeProvider();
        var intercepteur = new IntercepteurModifieLe(fournisseurTemps);
        var tempsEcoule = TimeSpan.FromMinutes(_fixture.Create<ushort>() + 1);
        var entite = new Utilisateur()
        {
            Id = Guid.NewGuid(),
        };
        _modele.Contexte.Attach(entite);
        entite.NomAffichage = _fixture.Create<string>();
        fournisseurTemps.Advance(tempsEcoule);

        // Agir
        intercepteur.AppliquerModifierLe(_modele.Contexte.ChangeTracker);

        // Assertion
        entite.ModifieLe.Should().Be(fournisseurTemps.GetUtcNow());
    }

    [Fact]
    public void AppliquerModifierLe_SeulementModifieLeEColonneTechniqueModifiees_NeReecritPasModifieLe()
    {
        // Arranger
        var fournisseurTemps = new FakeTimeProvider();
        var intercepteur = new IntercepteurModifieLe(fournisseurTemps);
        var dateExistante = _fixture.Create<DateTimeOffset>();
        var entite = new IdentiteExterne { Id = Guid.NewGuid() };
        _modele.Contexte.Attach(entite);
        _modele.Contexte.Entry(entite).Property(nameof(IModifieLe.ModifieLe)).CurrentValue = dateExistante;
        entite.DerniereConnexionLe = _fixture.Create<DateTimeOffset>();

        // Agir
        intercepteur.AppliquerModifierLe(_modele.Contexte.ChangeTracker);

        // Assertion
        entite.ModifieLe.Should().Be(dateExistante);
    }

    public static TheoryData<IModifieLe> EntitesIModifieLe =>
    [
        new IdentiteExterne { Id = Guid.NewGuid() },
        new Utilisateur { Id = Guid.NewGuid() }
    ];
}

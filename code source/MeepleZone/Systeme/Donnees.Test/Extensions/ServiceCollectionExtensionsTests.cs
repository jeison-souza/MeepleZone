using FluentAssertions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Donnees.Depots;
using MeepleZone.Systeme.Donnees.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using System;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.Extensions;

public class ServiceCollectionExtensionsTests
{
    private const string ChaineConnexionFausse = "Host=hote-inexistant";

    [Fact]
    public void AjouterSystemeDonnees_EnToutTemps_RegistreLeDepotDeComptes()
    {
        // Arranger
        var services = new ServiceCollection();

        // Agir
        var resultat = services.AjouterSystemeDonnees(ChaineConnexionFausse);

        // Verifier
        using var fournisseur = resultat.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        using var portee = fournisseur.CreateScope();
        portee.ServiceProvider.GetRequiredService<IDepotCompte>().Should().BeOfType<DepotCompte>();
    }

    [Fact]
    public void AjouterSystemeDonnees_EnToutTemps_RegistreLaFabriqueDeContextes()
    {
        // Arranger
        var services = new ServiceCollection();

        // Agir
        services.AjouterSystemeDonnees(ChaineConnexionFausse);

        // Verifier
        using var fournisseur = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        var fabrique = fournisseur.GetRequiredService<IDbContextFactory<MeepleZoneContexte>>();
        using var contexte = fabrique.CreateDbContext();
        contexte.Should().BeOfType<MeepleZoneContexte>();
    }

    [Fact]
    public void AjouterSystemeDonnees_AucunTimeProviderRegistre_RegistreTimeProviderSysteme()
    {
        // Arranger
        var services = new ServiceCollection();

        // Agir
        services.AjouterSystemeDonnees(ChaineConnexionFausse);

        // Verifier
        using var fournisseur = services.BuildServiceProvider();
        fournisseur.GetRequiredService<TimeProvider>().Should().BeSameAs(TimeProvider.System);
    }

    [Fact]
    public void AjouterSystemeDonnees_TimeProviderDejaRegistre_ConserveLeTimeProviderExistant()
    {
        // Arranger
        var existant = new FakeTimeProvider();
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(existant);

        // Agir
        services.AjouterSystemeDonnees(ChaineConnexionFausse);

        // Verifier
        using var fournisseur = services.BuildServiceProvider();
        fournisseur.GetRequiredService<TimeProvider>().Should().BeSameAs(existant);
    }
}

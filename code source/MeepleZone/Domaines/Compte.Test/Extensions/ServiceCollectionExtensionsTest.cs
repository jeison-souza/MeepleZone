using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Extensions;
using MeepleZone.Domaines.Compte.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Extensions;

public class ServiceCollectionExtensionsTest : BaseTest<ServiceCollection>
{
    private Fake<IDepotCompte> _depotCompte = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCompte = CreerFausse<IDepotCompte>();
    }

    [Fact]
    public void AjouterDomaineCompte_EnToutTemps_RetourneAvecClassesRegistres()
    {
        // Arrange
        InstanceTest.AddSingleton<TimeProvider>(FakeTimeProvider);
        InstanceTest.AddSingleton<IDepotCompte>(_depotCompte.FakedObject);

        // Act
        var result = InstanceTest.AjouterDomaineCompte();

        // Assert
        using var fournisseur = result.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        using var portee = fournisseur.CreateScope();
        portee.ServiceProvider.GetRequiredService<IDomaineCompte>().Should().BeOfType<DomaineCompte>();
        portee.ServiceProvider.GetRequiredService<IGenerateurCodeAmitie>().Should().BeOfType<GenerateurCodeAmitie>();
        portee.ServiceProvider.GetRequiredService<IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete>>().Should().BeOfType<GestionnaireRequeteGererConnexion>();
    }
}

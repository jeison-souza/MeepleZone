using FluentAssertions;
using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Systeme.BaseTest;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace MeepleZone.Domaines.Collection.Test.Extensions;

public class ServiceCollectionExtensionsTest : BaseTest<ServiceCollection>
{
    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
    }

    [Fact]
    public void AjouterDomaineCollection_EnToutTemps_RetourneAvecClassesRegistres()
    {
        // Arrange
        InstanceTest.AddSingleton<TimeProvider>(FakeTimeProvider);

        // Act
        var result = InstanceTest.AjouterDomaineCollection();

        // Assert
        using var fournisseur = result.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        using var portee = fournisseur.CreateScope();
        portee.ServiceProvider.GetRequiredService<IDomaineCollection>().Should().BeOfType<DomaineCollection>();
    }
}

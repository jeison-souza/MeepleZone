using FluentAssertions;
using MeepleZone.Site.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace MeepleZone.Site.Test.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ConfigurerEnTetesTransmis_ProxyDeclare_NeFaitConfianceQuACeProxy()
    {
        // Arrange
        var configuration = CreerConfiguration(new()
        {
            ["EnTetesTransmis:Proxys:0"] = "192.0.2.10"
        });

        // Act
        var options = ObtenirOptions(configuration);

        // Assert
        options.KnownProxies.Should().Equal(IPAddress.Parse("192.0.2.10"));
        options.KnownIPNetworks.Should().BeEmpty();
    }

    [Fact]
    public void ConfigurerEnTetesTransmis_ReseauDeclare_AjouteLaPlage()
    {
        // Arrange
        var configuration = CreerConfiguration(new()
        {
            ["EnTetesTransmis:Reseaux:0"] = "198.51.100.0/24"
        });

        // Act
        var options = ObtenirOptions(configuration);

        // Assert
        options.KnownIPNetworks.Should().ContainSingle()
            .Which.Contains(IPAddress.Parse("198.51.100.7")).Should().BeTrue();
        options.KnownProxies.Should().BeEmpty();
    }

    [Fact]
    public void ConfigurerEnTetesTransmis_AucunProxyDeclare_GardeLesValeursParDefautDuFramework()
    {
        // Arrange
        var configuration = CreerConfiguration(new());

        // Act
        var options = ObtenirOptions(configuration);

        // Assert
        options.KnownProxies.Should().Contain(IPAddress.IPv6Loopback);
    }

    [Fact]
    public void ConfigurerEnTetesTransmis_Toujours_TransmetLeSchemaEtLAdresseSeulement()
    {
        // Arrange
        var configuration = CreerConfiguration(new());

        // Act
        var options = ObtenirOptions(configuration);

        // Assert
        options.ForwardedHeaders.Should().Be(ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
    }

    [Fact]
    public void ConfigurerProtectionDonnees_RepertoireDeclare_ConserveLesClesDansCeRepertoire()
    {
        // Arrange
        var repertoire = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        var configuration = CreerConfiguration(new()
        {
            ["ProtectionDonnees:Repertoire"] = repertoire
        });
        var services = new ServiceCollection();

        // Act
        services.ConfigurerProtectionDonnees(configuration);
        var options = services.BuildServiceProvider()
            .GetRequiredService<IOptions<Microsoft.AspNetCore.DataProtection.KeyManagement.KeyManagementOptions>>().Value;

        // Assert
        options.XmlRepository.Should().NotBeNull();
        options.XmlRepository!.Should().BeOfType<Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository>();
    }

    private static IConfiguration CreerConfiguration(Dictionary<string, string?> valeurs)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(valeurs).Build();
    }

    private static ForwardedHeadersOptions ObtenirOptions(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.ConfigurerEnTetesTransmis(configuration);

        return services.BuildServiceProvider().GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
    }
}

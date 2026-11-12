using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Domaines.Collection.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace MeepleZone.Domaines.Collection.Test.Extensions;

public class ServiceCollectionExtensionsTest : BaseTest<ServiceCollection>
{
    private Fake<IDepotCatalogue> _depotCatalogue = null!;

    private Fake<IDepotCollection> _depotCollection = null!;

    private Fake<IUtilisateurCourant> _utilisateurCourant = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCatalogue = CreerFausse<IDepotCatalogue>();
        _depotCollection = CreerFausse<IDepotCollection>();
        _utilisateurCourant = CreerFausse<IUtilisateurCourant>();
    }

    [Fact]
    public void AjouterDomaineCollection_EnToutTemps_RetourneAvecClassesRegistres()
    {
        // Arrange
        InstanceTest.AddSingleton<TimeProvider>(FakeTimeProvider);
        InstanceTest.AddSingleton<IDepotCatalogue>(_depotCatalogue.FakedObject);
        InstanceTest.AddSingleton<IDepotCollection>(_depotCollection.FakedObject);
        InstanceTest.AddSingleton<IUtilisateurCourant>(_utilisateurCourant.FakedObject);

        // Act
        var result = InstanceTest.AjouterDomaineCollection();

        // Assert
        using var fournisseur = result.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        using var portee = fournisseur.CreateScope();
        portee.ServiceProvider.GetRequiredService<IDomaineCollection>().Should().BeOfType<DomaineCollection>();
        portee.ServiceProvider.GetRequiredService<IGestionnaireRequete<ListerCollectionReponse, ListerCollectionRequete>>().Should().BeOfType<GestionnaireRequeteListerCollection>();
        portee.ServiceProvider.GetRequiredService<IGestionnaireRequete<IReadOnlyList<MecaniqueDto>, ListerMecaniquesRequete>>().Should().BeOfType<GestionnaireRequeteListerMecaniques>();
        portee.ServiceProvider.GetRequiredService<IGestionnaireRequete<IReadOnlyList<ModeJeuDto>, ListerModesJeuRequete>>().Should().BeOfType<GestionnaireRequeteListerModesJeu>();
    }
}

using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Extensions;
using MeepleZone.Domaines.Compte.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Extensions;

public class ServiceCollectionExtensionsTest : BaseTest<ServiceCollection>
{
    private Fake<IDepotCompte> _depotCompte = null!;

    private Fake<IUtilisateurCourant> _utilisateurCourant = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCompte = CreerFausse<IDepotCompte>();
        _utilisateurCourant = CreerFausse<IUtilisateurCourant>();
    }

    [Fact]
    public void AjouterDomaineCompte_EnToutTemps_RetourneAvecClassesRegistres()
    {
        // Arrange
        InstanceTest.AddSingleton<TimeProvider>(FakeTimeProvider);
        InstanceTest.AddSingleton<IDepotCompte>(_depotCompte.FakedObject);
        InstanceTest.AddSingleton<IUtilisateurCourant>(_utilisateurCourant.FakedObject);

        // Act
        var result = InstanceTest.AjouterDomaineCompte();

        // Assert
        using var fournisseur = result.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        using var portee = fournisseur.CreateScope();
        portee.ServiceProvider.GetRequiredService<IDomaineCompte>().Should().BeOfType<DomaineCompte>();
        portee.ServiceProvider.GetRequiredService<IGenerateurCodeAmitie>().Should().BeOfType<GenerateurCodeAmitie>();
        portee.ServiceProvider.GetRequiredService<IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete>>().Should().BeOfType<GestionnaireRequeteGererConnexion>();
        portee.ServiceProvider.GetRequiredService<IGestionnaireRequete<IReadOnlyList<ConnexionDto>, ListerConnexionsRequete>>().Should().BeOfType<GestionnaireRequeteListerConnexions>();
        portee.ServiceProvider.GetRequiredService<IGestionnaireRequete<LierFournisseurReponse, LierFournisseurRequete>>().Should().BeOfType<GestionnaireRequeteLierFournisseur>();
        portee.ServiceProvider.GetRequiredService<IGestionnaireRequete<ProfilDto?, ObtenirProfilRequete>>().Should().BeOfType<GestionnaireRequeteObtenirProfil>();
        portee.ServiceProvider.GetRequiredService<IGestionnaireRequete<SupprimerCompteReponse, SupprimerCompteRequete>>().Should().BeOfType<GestionnaireRequeteSupprimerCompte>();
    }
}

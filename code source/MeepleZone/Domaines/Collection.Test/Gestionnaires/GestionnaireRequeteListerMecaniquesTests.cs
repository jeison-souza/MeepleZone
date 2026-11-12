using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Entites;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Collection.Test.Gestionnaires;

public class GestionnaireRequeteListerMecaniquesTests : BaseTest<GestionnaireRequeteListerMecaniques>
{
    private Fake<IDepotCatalogue> _depotCatalogue = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCatalogue = CreerFausse<IDepotCatalogue>();
    }

    [Fact]
    public async Task TraiterAsync_EnToutTemps_RetournerListeMecaniqueDtos()
    {
        // Arrange
        var requete = CreerFixture<ListerMecaniquesRequete>();
        var qte = CreerFixture<int>();
        List<Mecanique> mecaniquesAttendues = [.. Fixture.CreateMany<Mecanique>(qte)];
        _depotCatalogue
            .CallsTo(x => x.ListerMecaniquesAsync())
            .Returns(mecaniquesAttendues);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Should().HaveCount(qte);
        result.Should().BeEquivalentTo(mecaniquesAttendues.Select(m => new MecaniqueDto(m.Id, m.Nom, m.Description)));
    }
}

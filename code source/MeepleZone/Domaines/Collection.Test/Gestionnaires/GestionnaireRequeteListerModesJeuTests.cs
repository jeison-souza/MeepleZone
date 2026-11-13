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

public class GestionnaireRequeteListerModesJeuTests : BaseTest<GestionnaireRequeteListerModesJeu>
{
    private Fake<IDepotCatalogue> _depotCatalogue = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCatalogue = CreerFausse<IDepotCatalogue>();
    }

    [Fact]
    public async Task TraiterAsync_EnToutTemps_RetournerListeModeJeuDtos()
    {
        // Arrange
        var requete = CreerFixture<ListerModesJeuRequete>();
        var qte = CreerFixture<int>();
        List<ModeJeu> modesJeuAttendus = [.. Fixture.CreateMany<ModeJeu>(qte)];
        _depotCatalogue
            .CallsTo(x => x.ListerModesJeuAsync())
            .Returns(modesJeuAttendus);

        // Act
        var result = await InstanceTest.TraiterAsync(requete);

        // Assert
        result.Should().HaveCount(qte);
        result.Should().BeEquivalentTo(modesJeuAttendus.Select(m => new ModeJeuDto(m.Id, m.Nom, m.Description)));
    }
}

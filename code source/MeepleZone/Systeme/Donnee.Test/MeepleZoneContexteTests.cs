using FluentAssertions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnee.Test;

public class MeepleZoneContexteTests : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele;

    public MeepleZoneContexteTests(ModeleDonneesFixture modele)
    {
        _modele = modele;
    }

    [Fact]
    public void Utilisateurs_Ensemble_EstLieALEntiteUtilisateur()
    {
        // Arrange + Act : le type est lu dans le modèle, sans interroger la base
        var type = _modele.Contexte.Utilisateurs.EntityType;

        // Assert
        type.ClrType.Should().Be(typeof(Utilisateur));
    }

    [Fact]
    public void Modele_Collations_DoitPossederCollationCourriel()
    {
        // Arrange + Act : la collation est lue dans le modèle, sans interroger la base
        var collation = _modele.Modele.GetCollations().SingleOrDefault(c => c.Name == MeepleZoneContexte.CollationCourriel);

        // Assert
        collation.Should().NotBeNull();
        collation!.IsDeterministic.Should().BeFalse();
        collation.Provider.Should().Be("icu");
        collation.LcCollate.Should().Be("und-u-ks-level2");
        collation.LcCtype.Should().Be("und-u-ks-level2");
    }
}

using FluentAssertions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test;

public class MeepleZoneContexteTests(ModeleDonneesFixture modele) : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele = modele;

    [Fact]
    public void Utilisateurs_Ensemble_EstLieALEntiteUtilisateur()
    {
        // Arrange + Act : le type est lu dans le modèle, sans interroger la base
        var type = _modele.Contexte.Utilisateurs.EntityType;

        // Assert
        type.ClrType.Should().Be<Utilisateur>();
    }

    [Fact]
    public void IdentitesExternes_Ensemble_EstLieALEntiteIdentiteExterne()
    {
        // Arrange + Act : le type est lu dans le modèle, sans interroger la base
        var type = _modele.Contexte.IdentitesExternes.EntityType;

        // Assert
        type.ClrType.Should().Be<IdentiteExterne>();
    }

    [Fact]
    public void Annexes_Ensemble_EstLieALEntiteAnnexe()
    {
        // Arrange + Act : le type est lu dans le modèle, sans interroger la base
        var type = _modele.Contexte.Annexes.EntityType;

        // Assert
        type.ClrType.Should().Be<Annexe>();
    }

    [Fact]
    public void ModeJeu_Ensemble_EstLieALEntiteModeJeu()
    {
        // Arrange + Act : le type est lu dans le modèle, sans interroger la base
        var type = _modele.Contexte.ModesJeu.EntityType;

        // Assert
        type.ClrType.Should().Be<ModeJeu>();
    }

    [Fact]
    public void Mecaniques_Ensemble_EstLieALEntiteMecanique()
    {
        // Arrange + Act : le type est lu dans le modèle, sans interroger la base
        var type = _modele.Contexte.Mecaniques.EntityType;

        // Assert
        type.ClrType.Should().Be<Mecanique>();
    }

    [Fact]
    public void Jeux_Ensemble_EstLieALEntiteJeu()
    {
        // Arrange + Act : le type est lu dans le modèle, sans interroger la base
        var type = _modele.Contexte.Jeux.EntityType;

        // Assert
        type.ClrType.Should().Be<Jeu>();
    }

    [Fact]
    public void JeuxMecanique_Ensemble_EstLieALEntiteJeuMecanique()
    {
        // Arrange + Act : le type est lu dans le modèle, sans interroger la base
        var type = _modele.Contexte.JeuxMecaniques.EntityType;

        // Assert
        type.ClrType.Should().Be<JeuMecanique>();
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

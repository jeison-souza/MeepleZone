using FluentAssertions;
using MeepleZone.Systeme.Donnees.Test.Extensions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.Configurations;

public class MecaniqueConfigurationTests(ModeleDonneesFixture modele) : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele = modele;

    [Fact]
    public void Mecanique_Table_PorteLeNomMecanique()
    {
        // Arrange
        var type = _modele.TypeEntite<Mecanique>();

        // Act
        var nomTable = type.GetTableName();

        // Assert
        nomTable.Should().Be("mecanique");
    }

    [Fact]
    public void Mecanique_ClePrimaire_EstComposeeDeId()
    {
        // Arrange
        var type = _modele.TypeEntite<Mecanique>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.Properties.Select(p => p.Name).Should().Equal(nameof(Mecanique.Id));
    }

    [Theory]
    [InlineData(nameof(Mecanique.Id), false)]
    [InlineData(nameof(Mecanique.Nom), false)]
    [InlineData(nameof(Mecanique.Description), true)]
    [InlineData(nameof(Mecanique.BggId), true)]
    [InlineData(nameof(Mecanique.CreeLe), false)]
    public void Mecanique_Propriete_RespecteLaNullabilite(string nom, bool peutEtreNulle)
    {
        // Arrange
        var propriete = _modele.Propriete<Mecanique>(nom);

        // Act
        var accepteNull = propriete.IsNullable;

        // Assert
        accepteNull.Should().Be(peutEtreNulle);
    }

    [Theory]
    [InlineData(nameof(Mecanique.Id), false)]
    [InlineData(nameof(Mecanique.Nom), true)]
    [InlineData(nameof(Mecanique.Description), false)]
    [InlineData(nameof(Mecanique.BggId), true)]
    [InlineData(nameof(Mecanique.CreeLe), false)]
    public void Mecanique_Propriete_PossedeUnIndexUnique(string nom, bool doitEtreUnique)
    {
        // Arrange
        var propriete = _modele.Propriete<Mecanique>(nom);

        // Act
        var estUnique = propriete.IsUniqueIndex();

        // Assert
        estUnique.Should().Be(doitEtreUnique);
    }

    [Theory]
    [InlineData(nameof(Mecanique.CreeLe), "now()")]
    public void Mecanique_Propriete_PorteLaValeurSqlParDefaut(string nom, string sqlAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<Mecanique>(nom);

        // Act
        var sql = propriete.GetDefaultValueSql();

        // Assert
        sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void Mecanique_ClePrimaire_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<Mecanique>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.GetName().Should().Be("pk_mecanique");
    }

    [Theory]
    [InlineData(nameof(Mecanique.Nom), "ix_mecanique_nom")]
    [InlineData(nameof(Mecanique.BggId), "ix_mecanique_bgg_id")]
    public void Mecanique_Index_PorteLeNomDeLaConvention(string nom, string nomAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<Mecanique>();
        var propriete = _modele.Propriete<Mecanique>(nom);

        // Act
        var index = type.FindIndex(propriete);

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be(nomAttendu);
    }

    [Theory]
    [InlineData("ix_mecanique_nom", true)]
    [InlineData("ix_mecanique_bgg_id", true)]
    public void Mecanique_Index_PorteLaBonneUnicite(string nomIndex, bool estUnique)
    {
        // Arrange
        var type = _modele.TypeEntite<Mecanique>();

        // Act
        var index = type.GetIndexes().SingleOrDefault(i => i.GetDatabaseName() == nomIndex);

        // Assert
        index.Should().NotBeNull();
        index!.IsUnique.Should().Be(estUnique);
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(Mecanique), MemberType = typeof(TestsExtensions))]
    public void Mecanique_Propriete_NePossedePasDeCollation(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Mecanique>(nom);

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(Mecanique), MemberType = typeof(TestsExtensions))]
    public void Mecanique_Propriete_PorteLeNomDeColonneDuSchema(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Mecanique>(nom);
        var valeurAttendue = nom.EnSnakeCase();

        // Act
        var colonne = propriete.GetColumnName();

        // Assert
        colonne.Should().Be(valeurAttendue);
    }

    [Fact]
    public void Mecanique_DonneesInitiales_ContiennentLes23MecaniquesDuDocument()
    {
        // Arrange
        var type = _modele.TypeEntite<Mecanique>();
        var nomsAttendus = new[]
        {
            "Pose d'ouvriers", "Construction de pioche", "Construction de moteur", "Gestion de main",
            "Draft de cartes", "Lancer de dés", "Placement de tuiles", "Majorité de zones",
            "Collection de séries", "Enchères", "Échanges et négociation", "Rôles cachés",
            "Déduction", "Bluff", "Pousser sa chance", "Pouvoirs variables",
            "Plateau modulaire", "Déplacement sur le plateau", "Construction de réseaux", "Points d'action",
            "Gestion de ressources", "Programmation d'actions", "Campagne et évolution"
        };

        // Act
        var lignes = type.GetSeedData().ToList();

        // Assert
        lignes.Select(l => (string)l[nameof(Mecanique.Nom)]!).Should().BeEquivalentTo(nomsAttendus);
    }

    [Fact]
    public void Mecanique_DonneesInitiales_ONtDesIdentifiantsFixesUniquesDeVersion7()
    {
        // Arrange
        var type = _modele.TypeEntite<Mecanique>();

        // Act
        var identifiants = type.GetSeedData().Select(l => (Guid)l[nameof(Mecanique.Id)]!).ToList();

        // Assert
        identifiants.Should().OnlyHaveUniqueItems();
        identifiants.Should().OnlyContain(i => i.Version == 7);
    }

    [Fact]
    public void Mecanique_DonneesInitiales_ONtUneDescriptionEtUneDateDeCreation()
    {
        // Arrange
        var type = _modele.TypeEntite<Mecanique>();

        // Act
        var lignes = type.GetSeedData().ToList();

        // Assert
        lignes.Should().OnlyContain(l => !string.IsNullOrWhiteSpace((string?)l[nameof(Mecanique.Description)]));
        lignes.Should().OnlyContain(l => (DateTimeOffset)l[nameof(Mecanique.CreeLe)]! != default);
    }

    [Fact]
    public void Mecanique_DonneesInitiales_LaissentBggIdVide()
    {
        // Arrange
        var type = _modele.TypeEntite<Mecanique>();

        // Act
        var lignes = type.GetSeedData().ToList();

        // Assert
        lignes.Should().OnlyContain(l => !l.ContainsKey(nameof(Mecanique.BggId)) || l[nameof(Mecanique.BggId)] == null);
    }

    [Theory]
    [InlineData(nameof(Mecanique.Id), "uuid")]
    [InlineData(nameof(Mecanique.Nom), "text")]
    [InlineData(nameof(Mecanique.Description), "text")]
    [InlineData(nameof(Mecanique.BggId), "integer")]
    [InlineData(nameof(Mecanique.CreeLe), "timestamp with time zone")]
    public void Mecanique_Propriete_PorteLeBonTypeDeColonneDansSchema(string nom, string typeAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<Mecanique>(nom);

        // Act
        var colonne = propriete.GetColumnType();

        // Assert
        colonne.Should().Be(typeAttendu);
    }
}

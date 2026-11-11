using FluentAssertions;
using MeepleZone.Systeme.Donnees.Test.Extensions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.Configurations;

public class ModeJeuConfigurationTests(ModeleDonneesFixture modele) : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele = modele;

    [Fact]
    public void ModeJeu_Table_PorteLeNomModeJeu()
    {
        // Arrange
        var type = _modele.TypeEntite<ModeJeu>();

        // Act
        var nomTable = type.GetTableName();

        // Assert
        nomTable.Should().Be("mode_jeu");
    }

    [Fact]
    public void ModeJeu_ClePrimaire_EstComposeeDeId()
    {
        // Arrange
        var type = _modele.TypeEntite<ModeJeu>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.Properties.Select(p => p.Name).Should().Equal(nameof(ModeJeu.Id));
    }

    [Theory]
    [InlineData(nameof(ModeJeu.Id), false)]
    [InlineData(nameof(ModeJeu.Nom), false)]
    [InlineData(nameof(ModeJeu.Description), true)]
    [InlineData(nameof(ModeJeu.CreeLe), false)]
    public void ModeJeu_Propriete_RespecteLaNullabilite(string nom, bool peutEtreNulle)
    {
        // Arrange
        var propriete = _modele.Propriete<ModeJeu>(nom);

        // Act
        var accepteNull = propriete.IsNullable;

        // Assert
        accepteNull.Should().Be(peutEtreNulle);
    }

    [Theory]
    [InlineData(nameof(ModeJeu.Id), false)]
    [InlineData(nameof(ModeJeu.Nom), true)]
    [InlineData(nameof(ModeJeu.Description), false)]
    [InlineData(nameof(ModeJeu.CreeLe), false)]
    public void ModeJeu_Propriete_PossedeUnIndexUnique(string nom, bool doitEtreUnique)
    {
        // Arrange
        var propriete = _modele.Propriete<ModeJeu>(nom);

        // Act
        var estUnique = propriete.IsUniqueIndex();

        // Assert
        estUnique.Should().Be(doitEtreUnique);
    }

    [Theory]
    [InlineData(nameof(ModeJeu.CreeLe), "now()")]
    public void ModeJeu_Propriete_PorteLaValeurSqlParDefaut(string nom, string sqlAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<ModeJeu>(nom);

        // Act
        var sql = propriete.GetDefaultValueSql();

        // Assert
        sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void ModeJeu_ClePrimaire_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<ModeJeu>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.GetName().Should().Be("pk_mode_jeu");
    }

    [Theory]
    [InlineData(nameof(ModeJeu.Nom), "ix_mode_jeu_nom")]
    public void ModeJeu_Index_PorteLeNomDeLaConvention(string nom, string nomAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<ModeJeu>();
        var propriete = _modele.Propriete<ModeJeu>(nom);

        // Act
        var index = type.FindIndex(propriete);

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be(nomAttendu);
    }

    [Theory]
    [InlineData("ix_mode_jeu_nom", true)]
    public void ModeJeu_Index_PorteLaBonneUnicite(string nomIndex, bool estUnique)
    {
        // Arrange
        var type = _modele.TypeEntite<ModeJeu>();

        // Act
        var index = type.GetIndexes().SingleOrDefault(i => i.GetDatabaseName() == nomIndex);

        // Assert
        index.Should().NotBeNull();
        index!.IsUnique.Should().Be(estUnique);
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(ModeJeu), MemberType = typeof(TestsExtensions))]
    public void ModeJeu_Propriete_NePossedePasDeCollation(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<ModeJeu>(nom);

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(ModeJeu), MemberType = typeof(TestsExtensions))]
    public void ModeJeu_Propriete_PorteLeNomDeColonneDuSchema(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<ModeJeu>(nom);
        var valeurAttendue = nom.EnSnakeCase();

        // Act
        var colonne = propriete.GetColumnName();

        // Assert
        colonne.Should().Be(valeurAttendue);
    }

    [Fact]
    public void ModeJeu_DonneesInitiales_ContiennentLesQuatreModesDuSchema()
    {
        // Arrange
        var type = _modele.TypeEntite<ModeJeu>();
        var nomsAttendus = new[] { "Compétitif", "Coopératif", "Semi-coopératif", "Par équipes" };

        // Act
        var lignes = type.GetSeedData().ToList();

        // Assert
        lignes.Select(l => (string)l[nameof(ModeJeu.Nom)]!).Should().BeEquivalentTo(nomsAttendus);
    }

    [Fact]
    public void ModeJeu_DonneesInitiales_ONtDesIdentifiantsFixesUniquesDeVersion7()
    {
        // Arrange
        var type = _modele.TypeEntite<ModeJeu>();

        // Act
        var identifiants = type.GetSeedData().Select(l => (Guid)l[nameof(ModeJeu.Id)]!).ToList();

        // Assert
        identifiants.Should().OnlyHaveUniqueItems();
        identifiants.Should().OnlyContain(i => i.Version == 7);
    }

    [Fact]
    public void ModeJeu_DonneesInitiales_ONtUneDescriptionEtUneDateDeCreation()
    {
        // Arrange
        var type = _modele.TypeEntite<ModeJeu>();

        // Act
        var lignes = type.GetSeedData().ToList();

        // Assert
        lignes.Should().OnlyContain(l => !string.IsNullOrWhiteSpace((string?)l[nameof(ModeJeu.Description)]));
        lignes.Should().OnlyContain(l => (DateTimeOffset)l[nameof(ModeJeu.CreeLe)]! != default);
    }

    [Theory]
    [InlineData(nameof(ModeJeu.Id), "uuid")]
    [InlineData(nameof(ModeJeu.Nom), "text")]
    [InlineData(nameof(ModeJeu.Description), "text")]
    [InlineData(nameof(ModeJeu.CreeLe), "timestamp with time zone")]
    public void ModeJeu_Propriete_PorteLeBonTypeDeColonneDansSchema(string nom, string typeAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<ModeJeu>(nom);

        // Act
        var colonne = propriete.GetColumnType();

        // Assert
        colonne.Should().Be(typeAttendu);
    }
}

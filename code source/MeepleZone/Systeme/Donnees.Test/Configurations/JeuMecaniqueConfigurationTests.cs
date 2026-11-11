using FluentAssertions;
using MeepleZone.Systeme.Donnees.Test.Extensions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.Configurations;

public class JeuMecaniqueConfigurationTests(ModeleDonneesFixture modele) : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele = modele;

    [Fact]
    public void JeuMecanique_Table_PorteLeNomJeuMecanique()
    {
        // Arrange
        var type = _modele.TypeEntite<JeuMecanique>();

        // Act
        var nomTable = type.GetTableName();

        // Assert
        nomTable.Should().Be("jeu_mecanique");
    }

    [Fact]
    public void JeuMecanique_ClePrimaire_EstComposeeDeJeuIdEtMecaniqueId()
    {
        // Arrange
        var type = _modele.TypeEntite<JeuMecanique>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle.Properties.Select(p => p.Name).Should().Equal([nameof(JeuMecanique.JeuId), nameof(JeuMecanique.MecaniqueId)]);
    }

    [Theory]
    [InlineData(nameof(JeuMecanique.JeuId), false)]
    [InlineData(nameof(JeuMecanique.MecaniqueId), false)]
    public void JeuMecanique_Propriete_RespecteLaNullabilite(string nom, bool peutEtreNulle)
    {
        // Arrange
        var propriete = _modele.Propriete<JeuMecanique>(nom);

        // Act
        var accepteNull = propriete.IsNullable;

        // Assert
        accepteNull.Should().Be(peutEtreNulle);
    }

    [Theory]
    [InlineData(nameof(JeuMecanique.JeuId), false)]
    [InlineData(nameof(JeuMecanique.MecaniqueId), false)]
    public void JeuMecanique_Propriete_PossedeUnIndexUnique(string nom, bool doitEtreUnique)
    {
        // Arrange
        var propriete = _modele.Propriete<JeuMecanique>(nom);

        // Act
        var estUnique = propriete.IsUniqueIndex();

        // Assert
        estUnique.Should().Be(doitEtreUnique);
    }

    [Fact]
    public void JeuMecanique_ClePrimaire_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<JeuMecanique>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.GetName().Should().Be("pk_jeu_mecanique");
    }

    [Fact]
    public void JeuMecanique_CleEtrangereJeuId_PossedeLaBonneConfiguration()
    {
        // Arrange
        var type = _modele.TypeEntite<JeuMecanique>();
        var propriete = _modele.Propriete<JeuMecanique>(nameof(JeuMecanique.JeuId));

        // Act
        var cle = type.GetForeignKeys().SingleOrDefault(c => c.Properties.Contains(propriete));

        // Assert
        cle.Should().NotBeNull();
        cle.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
        cle.GetConstraintName().Should().Be("fk_jeu_mecanique_jeu_jeu_id");
        cle.PrincipalEntityType.ClrType.Should().Be<Jeu>();
    }

    [Fact]
    public void JeuMecanique_CleEtrangereMecanbiqueId_PossedeLaBonneConfiguration()
    {
        // Arrange
        var type = _modele.TypeEntite<JeuMecanique>();
        var propriete = _modele.Propriete<JeuMecanique>(nameof(JeuMecanique.MecaniqueId));

        // Act
        var cle = type.GetForeignKeys().SingleOrDefault(c => c.Properties.Contains(propriete));

        // Assert
        cle.Should().NotBeNull();
        cle.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
        cle.GetConstraintName().Should().Be("fk_jeu_mecanique_mecanique_mecanique_id");
        cle.PrincipalEntityType.ClrType.Should().Be<Mecanique>();
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(JeuMecanique), MemberType = typeof(TestsExtensions))]
    public void JeuMecanique_Propriete_NePossedePasDeCollation(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<JeuMecanique>(nom);

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(JeuMecanique), MemberType = typeof(TestsExtensions))]
    public void JeuMecanique_Propriete_PorteLeNomDeColonneDuSchema(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<JeuMecanique>(nom);
        var valeurAttendue = nom.EnSnakeCase();

        // Act
        var colonne = propriete.GetColumnName();

        // Assert
        colonne.Should().Be(valeurAttendue);
    }

    [Theory]
    [InlineData(nameof(JeuMecanique.JeuId), "uuid")]
    [InlineData(nameof(JeuMecanique.MecaniqueId), "uuid")]
    public void JeuMecanique_Propriete_PorteLeBonTypeDeColonneDansSchema(string nom, string typeAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<JeuMecanique>(nom);

        // Act
        var colonne = propriete.GetColumnType();

        // Assert
        colonne.Should().Be(typeAttendu);
    }
}

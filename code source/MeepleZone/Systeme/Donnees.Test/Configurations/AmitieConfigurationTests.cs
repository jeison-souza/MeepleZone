using FluentAssertions;
using MeepleZone.Systeme.Donnees.Test.Extensions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.Configurations;

public class AmitieConfigurationTests(ModeleDonneesFixture modele) : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele = modele;

    [Fact]
    public void Amitie_Table_PorteLeNomAmitie()
    {
        // Arrange
        var type = _modele.TypeEntite<Amitie>();

        // Act
        var nomTable = type.GetTableName();

        // Assert
        nomTable.Should().Be("amitie");
    }

    [Fact]
    public void Amitie_ClePrimaire_EstComposeeDesDeuxUtilisateurs()
    {
        // Arrange
        var type = _modele.TypeEntite<Amitie>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.Properties.Select(p => p.Name).Should().Equal([nameof(Amitie.UtilisateurAId), nameof(Amitie.UtilisateurBId)]);
    }

    [Fact]
    public void Amitie_ClePrimaire_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<Amitie>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.GetName().Should().Be("pk_amitie");
    }

    [Fact]
    public void Amitie_ContrainteCheck_ImposeLOrdreDesUtilisateurs()
    {
        // Arrange
        var type = _modele.TypeEntite<Amitie>();

        // Act
        var contrainte = type.GetCheckConstraints().SingleOrDefault(c => c.Name == "ck_amitie_utilisateurs_ordre");

        // Assert
        contrainte.Should().NotBeNull();
        contrainte!.Sql.Should().Be("utilisateur_a_id < utilisateur_b_id");
    }

    [Theory]
    [InlineData(nameof(Amitie.UtilisateurAId), "fk_amitie_utilisateur_utilisateur_a_id")]
    [InlineData(nameof(Amitie.UtilisateurBId), "fk_amitie_utilisateur_utilisateur_b_id")]
    public void Amitie_CleEtrangere_PossedeLaBonneConfiguration(string nom, string nomAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<Amitie>();
        var propriete = _modele.Propriete<Amitie>(nom);

        // Act
        var cle = type.GetForeignKeys().SingleOrDefault(c => c.Properties.Contains(propriete));

        // Assert
        cle.Should().NotBeNull();
        cle!.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
        cle.GetConstraintName().Should().Be(nomAttendu);
        cle.PrincipalEntityType.ClrType.Should().Be<Utilisateur>();
    }

    [Fact]
    public void Amitie_Index_PorteSeulementSurUtilisateurBId()
    {
        // Arrange
        var type = _modele.TypeEntite<Amitie>();

        // Act
        var index = type.GetIndexes().ToList();

        // Assert
        index.Should().ContainSingle();
        index[0].Properties.Select(p => p.Name).Should().Equal([nameof(Amitie.UtilisateurBId)]);
        index[0].GetDatabaseName().Should().Be("ix_amitie_utilisateur_b_id");
        index[0].IsUnique.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(Amitie), MemberType = typeof(TestsExtensions))]
    public void Amitie_Propriete_NAcceptePasNull(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Amitie>(nom);

        // Act
        var accepteNull = propriete.IsNullable;

        // Assert
        accepteNull.Should().BeFalse();
    }

    [Fact]
    public void Amitie_CreeLe_PorteLaValeurSqlParDefaut()
    {
        // Arrange
        var propriete = _modele.Propriete<Amitie>(nameof(Amitie.CreeLe));

        // Act
        var sql = propriete.GetDefaultValueSql();

        // Assert
        sql.Should().Be("now()");
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(Amitie), MemberType = typeof(TestsExtensions))]
    public void Amitie_Propriete_NePossedePasDeCollation(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Amitie>(nom);

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(Amitie), MemberType = typeof(TestsExtensions))]
    public void Amitie_Propriete_PorteLeNomDeColonneDuSchema(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Amitie>(nom);
        var valeurAttendue = nom.EnSnakeCase();

        // Act
        var colonne = propriete.GetColumnName();

        // Assert
        colonne.Should().Be(valeurAttendue);
    }

    [Theory]
    [InlineData(nameof(Amitie.UtilisateurAId), "uuid")]
    [InlineData(nameof(Amitie.UtilisateurBId), "uuid")]
    [InlineData(nameof(Amitie.CreeLe), "timestamp with time zone")]
    public void Amitie_Propriete_PorteLeBonTypeDeColonneDansSchema(string nom, string typeAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<Amitie>(nom);

        // Act
        var colonne = propriete.GetColumnType();

        // Assert
        colonne.Should().Be(typeAttendu);
    }
}

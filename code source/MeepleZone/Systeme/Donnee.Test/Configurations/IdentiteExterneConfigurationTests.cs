using FluentAssertions;
using MeepleZone.Systeme.Donnee.Test.Extensions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnee.Test.Configurations;

public class IdentiteExterneConfigurationTests : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele;

    public IdentiteExterneConfigurationTests(ModeleDonneesFixture modele)
    {
        _modele = modele;
    }

    [Fact]
    public void IdentiteExterne_Table_PorteLeNomIdentiteExterne()
    {
        // Arrange
        var type = _modele.TypeEntite<IdentiteExterne>();

        // Act
        var nomTable = type.GetTableName();

        // Assert
        nomTable.Should().Be("identite_externe");
    }

    [Theory]
    [InlineData("ck_identite_externe_courriel", "courriel = btrim(courriel) AND courriel <> ''")]
    public void IdentiteExterne_ContrainteCheck_EstDeclareeAvecSonSql(string nom, string sqlAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<IdentiteExterne>();

        // Act
        var contrainte = type.GetCheckConstraints().SingleOrDefault(c => c.Name == nom);

        // Assert
        contrainte.Should().NotBeNull();
        contrainte!.Sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void IdentiteExterne_ClePrimaire_EstComposeeDeId()
    {
        // Arrange
        var type = _modele.TypeEntite<IdentiteExterne>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.Properties.Select(p => p.Name).Should().Equal(nameof(IdentiteExterne.Id));
    }

    [Fact]
    public void IdentiteExterne_FournisseurIdentifiantCompteExterne_EstUnique()
    {
        // Arrange
        var type = _modele.TypeEntite<IdentiteExterne>();
        var fournisseur = _modele.Propriete<IdentiteExterne>(nameof(IdentiteExterne.Fournisseur));
        var identifiant = _modele.Propriete<IdentiteExterne>(nameof(IdentiteExterne.IdentifiantCompteExterne));

        // Act
        var index = type.FindIndex(new[] { fournisseur, identifiant });

        // Assert
        index.Should().NotBeNull();
        index!.IsUnique.Should().BeTrue();
        index.GetDatabaseName().Should().Be("ix_identite_externe_fournisseur_identifiant_compte_externe");
    }

    [Theory]
    [InlineData(nameof(IdentiteExterne.Id), false)]
    [InlineData(nameof(IdentiteExterne.UtilisateurId), false)]
    [InlineData(nameof(IdentiteExterne.Fournisseur), false)]
    [InlineData(nameof(IdentiteExterne.IdentifiantCompteExterne), false)]
    [InlineData(nameof(IdentiteExterne.Courriel), true)]
    [InlineData(nameof(IdentiteExterne.DerniereConnexionLe), false)]
    [InlineData(nameof(IdentiteExterne.CreeLe), false)]
    [InlineData(nameof(IdentiteExterne.ModifieLe), false)]
    public void IdentiteExterne_Propriete_RespecteLaNullabilite(string nom, bool peutEtreNulle)
    {
        // Arrange
        var propriete = _modele.Propriete<IdentiteExterne>(nom);

        // Act
        var accepteNull = propriete.IsNullable;

        // Assert
        accepteNull.Should().Be(peutEtreNulle);
    }

    [Theory]
    [InlineData(nameof(IdentiteExterne.Id), false)]
    [InlineData(nameof(IdentiteExterne.UtilisateurId), false)]
    [InlineData(nameof(IdentiteExterne.Fournisseur), true)]
    [InlineData(nameof(IdentiteExterne.IdentifiantCompteExterne), true)]
    [InlineData(nameof(IdentiteExterne.Courriel), false)]
    [InlineData(nameof(IdentiteExterne.DerniereConnexionLe), false)]
    [InlineData(nameof(IdentiteExterne.CreeLe), false)]
    [InlineData(nameof(IdentiteExterne.ModifieLe), false)]
    public void IdentiteExterne_Propriete_PossedeUnIndexUnique(string nom, bool doitEtreUnique)
    {
        // Arrange
        var propriete = _modele.Propriete<IdentiteExterne>(nom);

        // Act
        var estUnique = propriete.IsUniqueIndex();

        // Assert
        estUnique.Should().Be(doitEtreUnique);
    }

    [Theory]
    [InlineData(nameof(IdentiteExterne.DerniereConnexionLe), "now()")]
    [InlineData(nameof(IdentiteExterne.CreeLe), "now()")]
    [InlineData(nameof(IdentiteExterne.ModifieLe), "now()")]
    public void IdentiteExterne_Propriete_PorteLaValeurSqlParDefaut(string nom, string sqlAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<IdentiteExterne>(nom);

        // Act
        var sql = propriete.GetDefaultValueSql();

        // Assert
        sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void IdentiteExterne_ClePrimaire_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<IdentiteExterne>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.GetName().Should().Be("pk_identite_externe");
    }

    [Fact]
    public void IdentiteExterne_CleEtrangereUtilisateurId_PossedeLaBonneConfiguration()
    {
        // Arrange
        var type = _modele.TypeEntite<IdentiteExterne>();
        var propriete = _modele.Propriete<IdentiteExterne>(nameof(IdentiteExterne.UtilisateurId));

        // Act
        var cle = type.GetForeignKeys().SingleOrDefault(c => c.Properties.Contains(propriete));

        // Assert
        cle.Should().NotBeNull();
        cle.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
        cle.GetConstraintName().Should().Be("fk_identite_externe_utilisateur_utilisateur_id");
        cle.PrincipalEntityType.ClrType.Should().Be(typeof(Utilisateur));
    }

    [Theory]
    [InlineData(nameof(IdentiteExterne.UtilisateurId), "ix_identite_externe_utilisateur_id")]
    public void IdentiteExterne_Index_PorteLeNomDeLaConvention(string nom, string nomAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<IdentiteExterne>();
        var propriete = _modele.Propriete<IdentiteExterne>(nom);

        // Act
        var index = type.FindIndex(propriete);

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be(nomAttendu);
    }

    [Fact]
    public void IdentiteExterne_Courriel_UtiliseLaCollationDuCourriel()
    {
        // Arrange
        var propriete = _modele.Propriete<IdentiteExterne>(nameof(IdentiteExterne.Courriel));

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().Be(MeepleZoneContexte.CollationCourriel);
    }

    [Theory]
    [MemberData(nameof(NomsDesProprietesSauf), [new[] { nameof(IdentiteExterne.Courriel) }])]
    public void IdentiteExterne_Propriete_NePossedePasDeCollation(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<IdentiteExterne>(nom);

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(NomsDesProprietes))]
    public void IdentiteExterne_Propriete_PorteLeNomDeColonneDuSchema(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<IdentiteExterne>(nom);
        var valeurAttendue = nom.EnSnakeCase();

        // Act
        var colonne = propriete.GetColumnName();

        // Assert
        colonne.Should().Be(valeurAttendue);
    }

    [Theory]
    [InlineData(nameof(IdentiteExterne.Id), "uuid")]
    [InlineData(nameof(IdentiteExterne.UtilisateurId), "uuid")]
    [InlineData(nameof(IdentiteExterne.Fournisseur), "text")]
    [InlineData(nameof(IdentiteExterne.IdentifiantCompteExterne), "text")]
    [InlineData(nameof(IdentiteExterne.Courriel), "text")]
    [InlineData(nameof(IdentiteExterne.DerniereConnexionLe), "timestamp with time zone")]
    [InlineData(nameof(IdentiteExterne.CreeLe), "timestamp with time zone")]
    [InlineData(nameof(IdentiteExterne.ModifieLe), "timestamp with time zone")]
    public void IdentiteExterne_Propriete_PorteLeBonTypeDeColonneDansSchema(string nom, string typeAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<IdentiteExterne>(nom);

        // Act
        var colonne = propriete.GetColumnType();

        // Assert
        colonne.Should().Be(typeAttendu);
    }

    public static TheoryData<string> NomsDesProprietes()
    {
        return NomsDesProprietesSauf();
    }

    // Seules les propriétés simples (texte, nombre, date, Guid) sont des colonnes :
    // une collection ou une propriété de navigation (relation) ne l'est pas.
    public static TheoryData<string> NomsDesProprietesSauf(params string[] exceptes)
    {
        var donnees = new TheoryData<string>();

        foreach (var propriete in typeof(IdentiteExterne).GetProperties())
        {
            var estSimple = propriete.PropertyType.IsValueType || propriete.PropertyType == typeof(string);

            if (estSimple && !exceptes.Contains(propriete.Name))
            {
                donnees.Add(propriete.Name);
            }
        }

        return donnees;
    }
}

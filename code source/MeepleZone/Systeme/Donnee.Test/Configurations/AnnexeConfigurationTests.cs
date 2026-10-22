using FluentAssertions;
using MeepleZone.Systeme.Donnee.Test.Extensions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnee.Test.Configurations;

public class AnnexeConfigurationTests : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele;

    public AnnexeConfigurationTests(ModeleDonneesFixture modele)
    {
        _modele = modele;
    }

    [Fact]
    public void Annexe_Table_PorteLeNomAnnexe()
    {
        // Arrange
        var type = _modele.TypeEntite<Annexe>();

        // Act
        var nomTable = type.GetTableName();

        // Assert
        nomTable.Should().Be("annexe");
    }

    [Theory]
    [InlineData("ck_annexe_nom_fichier", "char_length(nom_fichier) BETWEEN 1 AND 255")]
    [InlineData("ck_annexe_taille_octets", "taille_octets > 0")]
    public void Annexe_ContrainteCheck_EstDeclareeAvecSonSql(string nom, string sqlAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<Annexe>();

        // Act
        var contrainte = type.GetCheckConstraints().SingleOrDefault(c => c.Name == nom);

        // Assert
        contrainte.Should().NotBeNull();
        contrainte!.Sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void Annexe_ClePrimaire_EstComposeeDeId()
    {
        // Arrange
        var type = _modele.TypeEntite<Annexe>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.Properties.Select(p => p.Name).Should().Equal(nameof(Annexe.Id));
    }

    [Theory]
    [InlineData(nameof(Annexe.Id), false)]
    [InlineData(nameof(Annexe.NomFichier), false)]
    [InlineData(nameof(Annexe.Contenu), false)]
    [InlineData(nameof(Annexe.TypeMime), false)]
    [InlineData(nameof(Annexe.TailleOctets), false)]
    [InlineData(nameof(Annexe.CreeParId), false)]
    [InlineData(nameof(Annexe.CreeLe), false)]
    public void Annexe_Propriete_RespecteLaNullabilite(string nom, bool peutEtreNulle)
    {
        // Arrange
        var propriete = _modele.Propriete<Annexe>(nom);

        // Act
        var accepteNull = propriete.IsNullable;

        // Assert
        accepteNull.Should().Be(peutEtreNulle);
    }

    [Theory]
    [InlineData(nameof(Annexe.Id), false)]
    [InlineData(nameof(Annexe.NomFichier), false)]
    [InlineData(nameof(Annexe.Contenu), false)]
    [InlineData(nameof(Annexe.TypeMime), false)]
    [InlineData(nameof(Annexe.TailleOctets), false)]
    [InlineData(nameof(Annexe.CreeParId), false)]
    [InlineData(nameof(Annexe.CreeLe), false)]
    public void Annexe_Propriete_PossedeUnIndexUnique(string nom, bool doitEtreUnique)
    {
        // Arrange
        var propriete = _modele.Propriete<Annexe>(nom);

        // Act
        var estUnique = propriete.IsUniqueIndex();

        // Assert
        estUnique.Should().Be(doitEtreUnique);
    }

    [Theory]
    [InlineData(nameof(Annexe.CreeLe), "now()")]
    public void Annexe_Propriete_PorteLaValeurSqlParDefaut(string nom, string sqlAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<Annexe>(nom);

        // Act
        var sql = propriete.GetDefaultValueSql();

        // Assert
        sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void Annexe_ClePrimaire_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<Annexe>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.GetName().Should().Be("pk_annexe");
    }

    [Fact]
    public void Annexe_CleEtrangereCreeParId_PossedeLaBonneConfiguration()
    {
        // Arrange
        var type = _modele.TypeEntite<Annexe>();
        var propriete = _modele.Propriete<Annexe>(nameof(Annexe.CreeParId));

        // Act
        var cle = type.GetForeignKeys().SingleOrDefault(c => c.Properties.Contains(propriete));

        // Assert
        cle.Should().NotBeNull();
        cle.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
        cle.GetConstraintName().Should().Be("fk_annexe_utilisateur_cree_par_id");
        cle.PrincipalEntityType.ClrType.Should().Be(typeof(Utilisateur));
    }

    [Theory]
    [InlineData(nameof(Annexe.CreeParId), "ix_annexe_cree_par_id")]
    public void Annexe_Index_PorteLeNomDeLaConvention(string nom, string nomAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<Annexe>();
        var propriete = _modele.Propriete<Annexe>(nom);

        // Act
        var index = type.FindIndex(propriete);

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be(nomAttendu);
    }

    [Theory]
    [InlineData("ix_annexe_cree_par_id", false)]
    public void Annexe_Index_PorteLaBonneUnicite(string nomIndex, bool estUnique)
    {
        // Arrange
        var type = _modele.TypeEntite<Annexe>();

        // Act
        var index = type.GetIndexes().SingleOrDefault(i => i.GetDatabaseName() == nomIndex);

        // Assert
        index.Should().NotBeNull();
        index!.IsUnique.Should().Be(estUnique);
    }

    [Theory]
    [MemberData(nameof(NomsDesProprietes))]
    public void Annexe_Propriete_NePossedePasDeCollation(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Annexe>(nom);

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(NomsDesProprietes))]
    public void Annexe_Propriete_PorteLeNomDeColonneDuSchema(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Annexe>(nom);
        var valeurAttendue = nom.EnSnakeCase();

        // Act
        var colonne = propriete.GetColumnName();

        // Assert
        colonne.Should().Be(valeurAttendue);
    }

    [Theory]
    [InlineData(nameof(Annexe.Id), "uuid")]
    [InlineData(nameof(Annexe.NomFichier), "text")]
    [InlineData(nameof(Annexe.Contenu), "bytea")]
    [InlineData(nameof(Annexe.TypeMime), "text")]
    [InlineData(nameof(Annexe.TailleOctets), "integer")]
    [InlineData(nameof(Annexe.CreeParId), "uuid")]
    [InlineData(nameof(Annexe.CreeLe), "timestamp with time zone")]
    public void Annexe_Propriete_PorteLeBonTypeDeColonneDansSchema(string nom, string typeAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<Annexe>(nom);

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

        foreach (var propriete in typeof(Annexe).GetProperties())
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

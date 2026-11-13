using FluentAssertions;
using MeepleZone.Systeme.Donnees.Test.Extensions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.Configurations;

public class PossessionConfigurationTests(ModeleDonneesFixture modele) : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele = modele;

    [Fact]
    public void Possession_Table_PorteLeNomPossession()
    {
        // Arrange
        var type = _modele.TypeEntite<Possession>();

        // Act
        var nomTable = type.GetTableName();

        // Assert
        nomTable.Should().Be("possession");
    }

    [Theory]
    [InlineData("ck_possession_note", "note BETWEEN 1 AND 10")]
    [InlineData("ck_possession_parties_jouees", "parties_jouees >= 0")]
    public void Possession_ContrainteCheck_EstDeclareeAvecSonSql(string nom, string sqlAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<Possession>();

        // Act
        var contrainte = type.GetCheckConstraints().SingleOrDefault(c => c.Name == nom);

        // Assert
        contrainte.Should().NotBeNull();
        contrainte!.Sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void Possession_ClePrimaire_EstComposeeDeId()
    {
        // Arrange
        var type = _modele.TypeEntite<Possession>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.Properties.Select(p => p.Name).Should().Equal(nameof(Possession.Id));
    }

    [Fact]
    public void Possession_ClePrimaire_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<Possession>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.GetName().Should().Be("pk_possession");
    }

    [Theory]
    [InlineData(nameof(Possession.Id), false)]
    [InlineData(nameof(Possession.UtilisateurId), false)]
    [InlineData(nameof(Possession.JeuId), false)]
    [InlineData(nameof(Possession.Note), true)]
    [InlineData(nameof(Possession.Commentaire), true)]
    [InlineData(nameof(Possession.PartiesJouees), false)]
    [InlineData(nameof(Possession.CreeLe), false)]
    [InlineData(nameof(Possession.ModifieLe), false)]
    public void Possession_Propriete_RespecteLaNullabilite(string nom, bool peutEtreNulle)
    {
        // Arrange
        var propriete = _modele.Propriete<Possession>(nom);

        // Act
        var accepteNull = propriete.IsNullable;

        // Assert
        accepteNull.Should().Be(peutEtreNulle);
    }

    [Fact]
    public void Possession_PartiesJouees_PorteLaValeurParDefaut()
    {
        // Arrange
        var propriete = _modele.Propriete<Possession>(nameof(Possession.PartiesJouees));

        // Act
        var valeur = propriete.GetDefaultValue();

        // Assert
        valeur.Should().Be(0);
    }

    [Theory]
    [InlineData(nameof(Possession.CreeLe), "now()")]
    [InlineData(nameof(Possession.ModifieLe), "now()")]
    public void Possession_Propriete_PorteLaValeurSqlParDefaut(string nom, string sqlAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<Possession>(nom);

        // Act
        var sql = propriete.GetDefaultValueSql();

        // Assert
        sql.Should().Be(sqlAttendu);
    }

    [Theory]
    [InlineData(nameof(Possession.UtilisateurId), DeleteBehavior.Cascade, "fk_possession_utilisateur_utilisateur_id", typeof(Utilisateur))]
    [InlineData(nameof(Possession.JeuId), DeleteBehavior.Restrict, "fk_possession_jeu_jeu_id", typeof(Jeu))]
    public void Possession_CleEtrangere_PossedeLaBonneConfiguration(string nom, DeleteBehavior suppressionAttendue, string nomContrainteAttendu, Type principalAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<Possession>();
        var propriete = _modele.Propriete<Possession>(nom);

        // Act
        var cle = type.GetForeignKeys().SingleOrDefault(c => c.Properties.Contains(propriete));

        // Assert
        cle.Should().NotBeNull();
        cle!.DeleteBehavior.Should().Be(suppressionAttendue);
        cle.GetConstraintName().Should().Be(nomContrainteAttendu);
        cle.PrincipalEntityType.ClrType.Should().Be(principalAttendu);
    }

    [Fact]
    public void Possession_UtilisateurIdJeuId_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<Possession>();
        var utilisateur = _modele.Propriete<Possession>(nameof(Possession.UtilisateurId));
        var jeu = _modele.Propriete<Possession>(nameof(Possession.JeuId));

        // Act
        var index = type.FindIndex([utilisateur, jeu]);

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be("ix_possession_utilisateur_id_jeu_id");
    }

    [Theory]
    [InlineData("ix_possession_utilisateur_id_jeu_id", true)]
    [InlineData("ix_possession_jeu_id", false)]
    public void Possession_Index_PorteLaBonneUnicite(string nomIndex, bool estUnique)
    {
        // Arrange
        var type = _modele.TypeEntite<Possession>();

        // Act
        var index = type.GetIndexes().SingleOrDefault(i => i.GetDatabaseName() == nomIndex);

        // Assert
        index.Should().NotBeNull();
        index!.IsUnique.Should().Be(estUnique);
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(Possession), MemberType = typeof(TestsExtensions))]
    public void Possession_Propriete_NePossedePasDeCollation(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Possession>(nom);

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(Possession), MemberType = typeof(TestsExtensions))]
    public void Possession_Propriete_PorteLeNomDeColonneDuSchema(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Possession>(nom);
        var valeurAttendue = nom.EnSnakeCase();

        // Act
        var colonne = propriete.GetColumnName();

        // Assert
        colonne.Should().Be(valeurAttendue);
    }

    [Theory]
    [InlineData(nameof(Possession.Id), "uuid")]
    [InlineData(nameof(Possession.UtilisateurId), "uuid")]
    [InlineData(nameof(Possession.JeuId), "uuid")]
    [InlineData(nameof(Possession.Note), "smallint")]
    [InlineData(nameof(Possession.Commentaire), "text")]
    [InlineData(nameof(Possession.PartiesJouees), "integer")]
    [InlineData(nameof(Possession.CreeLe), "timestamp with time zone")]
    [InlineData(nameof(Possession.ModifieLe), "timestamp with time zone")]
    public void Possession_Propriete_PorteLeBonTypeDeColonneDansSchema(string nom, string typeAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<Possession>(nom);

        // Act
        var colonne = propriete.GetColumnType();

        // Assert
        colonne.Should().Be(typeAttendu);
    }
}

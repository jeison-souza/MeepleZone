using FluentAssertions;
using MeepleZone.Systeme.Donnee.Test.Extensions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnee.Test.Configurations;

public class UtilisateurConfigurationTests : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele;

    public UtilisateurConfigurationTests(ModeleDonneesFixture modele)
    {
        _modele = modele;
    }

    [Fact]
    public void Utilisateur_Table_PorteLeNomUtilisateur()
    {
        // Arrange
        var type = _modele.TypeEntite<Utilisateur>();

        // Act
        var nomTable = type.GetTableName();

        // Assert
        nomTable.Should().Be("utilisateur");
    }

    [Theory]
    [InlineData("ck_utilisateur_role", "role IN ('membre', 'administrateur', 'systeme')")]
    [InlineData("ck_utilisateur_visibilite_collection", "visibilite_collection IN ('privee', 'amis')")]
    [InlineData("ck_utilisateur_code_amitie", "code_amitie ~ '^[A-HJ-NP-Z2-9]{8}$'")]
    public void Utilisateur_ContrainteCheck_EstDeclareeAvecSonSql(string nom, string sqlAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<Utilisateur>();

        // Act
        var contrainte = type.GetCheckConstraints().SingleOrDefault(c => c.Name == nom);

        // Assert
        contrainte.Should().NotBeNull();
        contrainte!.Sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void Utilisateur_ClePrimaire_EstComposeeDeId()
    {
        // Arrange
        var type = _modele.TypeEntite<Utilisateur>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.Properties.Select(p => p.Name).Should().Equal(nameof(Utilisateur.Id));
    }

    [Theory]
    [InlineData(nameof(Utilisateur.CodeAmitie))]
    [InlineData(nameof(Utilisateur.Courriel))]
    public void Utilisateur_Propriete_EstUnique(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Utilisateur>(nom);

        // Act
        var estUnique = propriete.IsUniqueIndex();

        // Assert
        estUnique.Should().BeTrue();
    }

    [Theory]
    [InlineData(nameof(Utilisateur.Id), false)]
    [InlineData(nameof(Utilisateur.Courriel), false)]
    [InlineData(nameof(Utilisateur.CodeAmitie), false)]
    [InlineData(nameof(Utilisateur.NomAffichage), false)]
    [InlineData(nameof(Utilisateur.Role), false)]
    [InlineData(nameof(Utilisateur.VisibiliteCollection), false)]
    [InlineData(nameof(Utilisateur.CreeLe), false)]
    [InlineData(nameof(Utilisateur.ModifieLe), false)]
    [InlineData(nameof(Utilisateur.CourrielVerifieLe), true)]
    [InlineData(nameof(Utilisateur.PhotoId), true)]
    public void Utilisateur_Propriete_RespecteLaNullabilite(string nom, bool peutEtreNulle)
    {
        // Arrange
        var propriete = _modele.Propriete<Utilisateur>(nom);

        // Act
        var accepteNull = propriete.IsNullable;

        // Assert
        accepteNull.Should().Be(peutEtreNulle);
    }

    [Theory]
    [InlineData(nameof(Utilisateur.Role), "membre")]
    [InlineData(nameof(Utilisateur.VisibiliteCollection), "privee")]
    public void Utilisateur_Propriete_PorteLaValeurParDefaut(string nom, string valeurAttendue)
    {
        // Arrange
        var propriete = _modele.Propriete<Utilisateur>(nom);

        // Act
        var valeur = propriete.GetDefaultValue();

        // Assert
        valeur.Should().Be(valeurAttendue);
    }

    [Theory]
    [InlineData(nameof(Utilisateur.CreeLe), "now()")]
    [InlineData(nameof(Utilisateur.ModifieLe), "now()")]
    public void Utilisateur_Propriete_PorteLaValeurSqlParDefaut(string nom, string sqlAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<Utilisateur>(nom);

        // Act
        var sql = propriete.GetDefaultValueSql();

        // Assert
        sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void Utilisateur_ClePrimaire_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<Utilisateur>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.GetName().Should().Be("pk_utilisateur");
    }

    [Theory]
    [InlineData(nameof(Utilisateur.CodeAmitie), "ix_utilisateur_code_amitie")]
    [InlineData(nameof(Utilisateur.Courriel), "ix_utilisateur_courriel")]
    public void Utilisateur_Index_PorteLeNomDeLaConvention(string nom, string nomAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<Utilisateur>();
        var propriete = _modele.Propriete<Utilisateur>(nom);

        // Act
        var index = type.FindIndex(propriete);

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be(nomAttendu);
    }

    [Fact]
    public void Utilisateur_Courriel_UtiliseLaCollationDuCourriel()
    {
        // Arrange
        var propriete = _modele.Propriete<Utilisateur>(nameof(Utilisateur.Courriel));

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().Be(MeepleZoneContexte.CollationCourriel);
    }

    [Theory]
    [MemberData(nameof(NomsDesProprietesSauf), [new[] { nameof(Utilisateur.Courriel) }])]
    public void Utilisateur_Propriete_NePossedePasDeCollation(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Utilisateur>(nom);

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(NomsDesProprietes))]
    public void Utilisateur_Propriete_PorteLeNomDeColonneDuSchema(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Utilisateur>(nom);
        var valeurAttendue = nom.EnSnakeCase();

        // Act
        var colonne = propriete.GetColumnName();

        // Assert
        colonne.Should().Be(valeurAttendue);
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

        foreach (var propriete in typeof(Utilisateur).GetProperties())
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
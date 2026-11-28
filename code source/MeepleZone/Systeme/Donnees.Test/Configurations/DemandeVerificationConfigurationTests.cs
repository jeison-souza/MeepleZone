using FluentAssertions;
using MeepleZone.Systeme.Donnees.Test.Extensions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.Configurations;

public class DemandeVerificationConfigurationTests(ModeleDonneesFixture modele) : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele = modele;

    [Fact]
    public void DemandeVerification_Table_PorteLeNomDemandeVerification()
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeVerification>();

        // Act
        var nomTable = type.GetTableName();

        // Assert
        nomTable.Should().Be("demande_verification");
    }

    [Fact]
    public void DemandeVerification_ClePrimaire_EstComposeeDeIdEtPorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeVerification>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.Properties.Select(p => p.Name).Should().Equal(nameof(DemandeVerification.Id));
        cle.GetName().Should().Be("pk_demande_verification");
    }

    [Theory]
    [InlineData("ck_demande_verification_courriel", "courriel = btrim(courriel) AND courriel <> ''")]
    [InlineData("ck_demande_verification_duree_validite", "duree_validite > interval '0'")]
    public void DemandeVerification_ContrainteCheck_EstDeclareeAvecSonSql(string nom, string sqlAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeVerification>();

        // Act
        var contrainte = type.GetCheckConstraints().SingleOrDefault(c => c.Name == nom);

        // Assert
        contrainte.Should().NotBeNull();
        contrainte!.Sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void DemandeVerification_CleEtrangereUtilisateurId_PossedeLaBonneConfiguration()
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeVerification>();
        var propriete = _modele.Propriete<DemandeVerification>(nameof(DemandeVerification.UtilisateurId));

        // Act
        var cle = type.GetForeignKeys().SingleOrDefault(c => c.Properties.Contains(propriete));

        // Assert
        cle.Should().NotBeNull();
        cle.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
        cle.GetConstraintName().Should().Be("fk_demande_verification_utilisateur_utilisateur_id");
        cle.PrincipalEntityType.ClrType.Should().Be<Utilisateur>();
    }

    [Theory]
    [InlineData(nameof(DemandeVerification.UtilisateurId), "ix_demande_verification_utilisateur_id")]
    [InlineData(nameof(DemandeVerification.Courriel), "ix_demande_verification_courriel")]
    public void DemandeVerification_Index_EstNonUniqueEtPorteLeNomDeLaConvention(string nom, string nomAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeVerification>();
        var propriete = _modele.Propriete<DemandeVerification>(nom);

        // Act
        var index = type.FindIndex(propriete);

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be(nomAttendu);
        index.IsUnique.Should().BeFalse();
    }

    [Theory]
    [InlineData(nameof(DemandeVerification.Id), false)]
    [InlineData(nameof(DemandeVerification.UtilisateurId), false)]
    [InlineData(nameof(DemandeVerification.Courriel), false)]
    [InlineData(nameof(DemandeVerification.CreeLe), false)]
    [InlineData(nameof(DemandeVerification.EnvoyeLe), true)]
    [InlineData(nameof(DemandeVerification.DureeValidite), false)]
    public void DemandeVerification_Propriete_RespecteLaNullabilite(string nom, bool peutEtreNulle)
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeVerification>(nom);

        // Act
        var accepteNull = propriete.IsNullable;

        // Assert
        accepteNull.Should().Be(peutEtreNulle);
    }

    [Fact]
    public void DemandeVerification_CreeLe_PorteLaValeurSqlParDefaut()
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeVerification>(nameof(DemandeVerification.CreeLe));

        // Act
        var sql = propriete.GetDefaultValueSql();

        // Assert
        sql.Should().Be("now()");
    }

    [Fact]
    public void DemandeVerification_Courriel_UtiliseLaCollationDuCourriel()
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeVerification>(nameof(DemandeVerification.Courriel));

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().Be(MeepleZoneContexte.CollationCourriel);
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietesSauf), typeof(DemandeVerification), new string[] { nameof(DemandeVerification.Courriel) }, MemberType = typeof(TestsExtensions))]
    public void DemandeVerification_Propriete_NePossedePasDeCollation(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeVerification>(nom);

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(DemandeVerification), MemberType = typeof(TestsExtensions))]
    public void DemandeVerification_Propriete_PorteLeNomDeColonneDuSchema(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeVerification>(nom);
        var valeurAttendue = nom.EnSnakeCase();

        // Act
        var colonne = propriete.GetColumnName();

        // Assert
        colonne.Should().Be(valeurAttendue);
    }

    [Theory]
    [InlineData(nameof(DemandeVerification.Id), "uuid")]
    [InlineData(nameof(DemandeVerification.UtilisateurId), "uuid")]
    [InlineData(nameof(DemandeVerification.Courriel), "text")]
    [InlineData(nameof(DemandeVerification.CreeLe), "timestamp with time zone")]
    [InlineData(nameof(DemandeVerification.EnvoyeLe), "timestamp with time zone")]
    [InlineData(nameof(DemandeVerification.DureeValidite), "interval")]
    public void DemandeVerification_Propriete_PorteLeBonTypeDeColonneDansSchema(string nom, string typeAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeVerification>(nom);

        // Act
        var colonne = propriete.GetColumnType();

        // Assert
        colonne.Should().Be(typeAttendu);
    }
}

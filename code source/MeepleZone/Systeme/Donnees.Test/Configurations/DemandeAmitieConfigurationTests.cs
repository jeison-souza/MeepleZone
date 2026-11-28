using FluentAssertions;
using MeepleZone.Systeme.Donnees.Test.Extensions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.Configurations;

public class DemandeAmitieConfigurationTests(ModeleDonneesFixture modele) : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele = modele;

    [Fact]
    public void DemandeAmitie_Table_PorteLeNomDemandeAmitie()
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeAmitie>();

        // Act
        var nomTable = type.GetTableName();

        // Assert
        nomTable.Should().Be("demande_amitie");
    }

    [Fact]
    public void DemandeAmitie_ClePrimaire_EstComposeeDeId()
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeAmitie>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.Properties.Select(p => p.Name).Should().Equal(nameof(DemandeAmitie.Id));
    }

    [Fact]
    public void DemandeAmitie_ClePrimaire_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeAmitie>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.GetName().Should().Be("pk_demande_amitie");
    }

    [Theory]
    [InlineData("ck_demande_amitie_statut", "statut IN ('en_attente', 'ignoree', 'rejetee')")]
    [InlineData("ck_demande_amitie_destinataire", "(courriel_destinataire IS NULL) <> (code_destinataire IS NULL)")]
    [InlineData("ck_demande_amitie_code_rattache", "code_destinataire IS NULL OR destinataire_id IS NOT NULL")]
    [InlineData("ck_demande_amitie_demandeur_distinct", "demandeur_id <> destinataire_id")]
    public void DemandeAmitie_ContrainteCheck_EstDeclareeAvecSonSql(string nom, string sqlAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeAmitie>();

        // Act
        var contrainte = type.GetCheckConstraints().SingleOrDefault(c => c.Name == nom);

        // Assert
        contrainte.Should().NotBeNull();
        contrainte!.Sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void DemandeAmitie_CleEtrangereDemandeurId_PossedeLaBonneConfiguration()
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeAmitie>();
        var propriete = _modele.Propriete<DemandeAmitie>(nameof(DemandeAmitie.DemandeurId));

        // Act
        var cle = type.GetForeignKeys().SingleOrDefault(c => c.Properties.Contains(propriete));

        // Assert
        cle.Should().NotBeNull();
        cle.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
        cle.GetConstraintName().Should().Be("fk_demande_amitie_utilisateur_demandeur_id");
        cle.PrincipalEntityType.ClrType.Should().Be<Utilisateur>();
    }

    [Fact]
    public void DemandeAmitie_CleEtrangereDestinataireId_PossedeLaBonneConfiguration()
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeAmitie>();
        var propriete = _modele.Propriete<DemandeAmitie>(nameof(DemandeAmitie.DestinataireId));

        // Act
        var cle = type.GetForeignKeys().SingleOrDefault(c => c.Properties.Contains(propriete));

        // Assert
        cle.Should().NotBeNull();
        cle.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
        cle.GetConstraintName().Should().Be("fk_demande_amitie_utilisateur_destinataire_id");
        cle.PrincipalEntityType.ClrType.Should().Be<Utilisateur>();
    }

    [Theory]
    [InlineData("ix_demande_amitie_demandeur_id_destinataire_id", true)]
    [InlineData("ix_demande_amitie_demandeur_id_courriel_destinataire", true)]
    [InlineData("ix_demande_amitie_destinataire_id", false)]
    [InlineData("ix_demande_amitie_courriel_destinataire", false)]
    public void DemandeAmitie_Index_PorteLaBonneUnicite(string nomIndex, bool estUnique)
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeAmitie>();

        // Act
        var index = type.GetIndexes().SingleOrDefault(i => i.GetDatabaseName() == nomIndex);

        // Assert
        index.Should().NotBeNull();
        index!.IsUnique.Should().Be(estUnique);
    }

    [Fact]
    public void DemandeAmitie_DemandeurIdDestinataireId_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeAmitie>();
        var demandeur = _modele.Propriete<DemandeAmitie>(nameof(DemandeAmitie.DemandeurId));
        var destinataire = _modele.Propriete<DemandeAmitie>(nameof(DemandeAmitie.DestinataireId));

        // Act
        var index = type.FindIndex([demandeur, destinataire]);

        // Assert
        index.Should().NotBeNull();
        index.GetDatabaseName().Should().Be("ix_demande_amitie_demandeur_id_destinataire_id");
    }

    [Fact]
    public void DemandeAmitie_DemandeurIdCourrielDestinataire_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeAmitie>();
        var demandeur = _modele.Propriete<DemandeAmitie>(nameof(DemandeAmitie.DemandeurId));
        var courriel = _modele.Propriete<DemandeAmitie>(nameof(DemandeAmitie.CourrielDestinataire));

        // Act
        var index = type.FindIndex([demandeur, courriel]);

        // Assert
        index.Should().NotBeNull();
        index.GetDatabaseName().Should().Be("ix_demande_amitie_demandeur_id_courriel_destinataire");
    }

    [Theory]
    [InlineData(nameof(DemandeAmitie.DestinataireId), "ix_demande_amitie_destinataire_id")]
    [InlineData(nameof(DemandeAmitie.CourrielDestinataire), "ix_demande_amitie_courriel_destinataire")]
    public void DemandeAmitie_Index_PorteLeNomDeLaConvention(string nom, string nomAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<DemandeAmitie>();
        var propriete = _modele.Propriete<DemandeAmitie>(nom);

        // Act
        var index = type.FindIndex(propriete);

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be(nomAttendu);
    }

    [Theory]
    [InlineData(nameof(DemandeAmitie.Id), false)]
    [InlineData(nameof(DemandeAmitie.DemandeurId), false)]
    [InlineData(nameof(DemandeAmitie.DestinataireId), true)]
    [InlineData(nameof(DemandeAmitie.CourrielDestinataire), true)]
    [InlineData(nameof(DemandeAmitie.CodeDestinataire), true)]
    [InlineData(nameof(DemandeAmitie.Statut), false)]
    [InlineData(nameof(DemandeAmitie.CreeLe), false)]
    [InlineData(nameof(DemandeAmitie.ModifieLe), false)]
    public void DemandeAmitie_Propriete_RespecteLaNullabilite(string nom, bool peutEtreNulle)
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeAmitie>(nom);

        // Act
        var accepteNull = propriete.IsNullable;

        // Assert
        accepteNull.Should().Be(peutEtreNulle);
    }

    [Theory]
    [InlineData(nameof(DemandeAmitie.Id), false)]
    [InlineData(nameof(DemandeAmitie.DemandeurId), true)]
    [InlineData(nameof(DemandeAmitie.DestinataireId), true)]
    [InlineData(nameof(DemandeAmitie.CourrielDestinataire), true)]
    [InlineData(nameof(DemandeAmitie.CodeDestinataire), false)]
    [InlineData(nameof(DemandeAmitie.Statut), false)]
    [InlineData(nameof(DemandeAmitie.CreeLe), false)]
    [InlineData(nameof(DemandeAmitie.ModifieLe), false)]
    public void DemandeAmitie_Propriete_PossedeUnIndexUnique(string nom, bool doitEtreUnique)
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeAmitie>(nom);

        // Act
        var estUnique = propriete.IsUniqueIndex();

        // Assert
        estUnique.Should().Be(doitEtreUnique);
    }

    [Theory]
    [InlineData(nameof(DemandeAmitie.Statut), "en_attente")]
    public void DemandeAmitie_Propriete_PorteLaValeurParDefaut(string nom, string valeurAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeAmitie>(nom);

        // Act
        var sql = propriete.GetDefaultValue();

        // Assert
        sql.Should().Be(valeurAttendu);
    }

    [Theory]
    [InlineData(nameof(DemandeAmitie.CreeLe), "now()")]
    [InlineData(nameof(DemandeAmitie.ModifieLe), "now()")]
    public void DemandeAmitie_Propriete_PorteLaValeurSqlParDefaut(string nom, string sqlAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeAmitie>(nom);

        // Act
        var sql = propriete.GetDefaultValueSql();

        // Assert
        sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void DemandeAmitie_CourrielDestinataire_UtiliseLaCollationDuCourriel()
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeAmitie>(nameof(DemandeAmitie.CourrielDestinataire));

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().Be(MeepleZoneContexte.CollationCourriel);
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietesSauf), typeof(DemandeAmitie), new string[] { nameof(DemandeAmitie.CourrielDestinataire) }, MemberType = typeof(TestsExtensions))]
    public void DemandeAmitie_Propriete_NePossedePasDeCollation(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeAmitie>(nom);

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(DemandeAmitie), MemberType = typeof(TestsExtensions))]
    public void DemandeAmitie_Propriete_PorteLeNomDeColonneDuSchema(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeAmitie>(nom);
        var valeurAttendue = nom.EnSnakeCase();

        // Act
        var colonne = propriete.GetColumnName();

        // Assert
        colonne.Should().Be(valeurAttendue);
    }

    [Theory]
    [InlineData(nameof(DemandeAmitie.Id), "uuid")]
    [InlineData(nameof(DemandeAmitie.DemandeurId), "uuid")]
    [InlineData(nameof(DemandeAmitie.DestinataireId), "uuid")]
    [InlineData(nameof(DemandeAmitie.CourrielDestinataire), "text")]
    [InlineData(nameof(DemandeAmitie.CodeDestinataire), "text")]
    [InlineData(nameof(DemandeAmitie.Statut), "text")]
    [InlineData(nameof(DemandeAmitie.CreeLe), "timestamp with time zone")]
    [InlineData(nameof(DemandeAmitie.ModifieLe), "timestamp with time zone")]
    public void DemandeAmitie_Propriete_PorteLeBonTypeDeColonneDansSchema(string nom, string typeAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<DemandeAmitie>(nom);

        // Act
        var colonne = propriete.GetColumnType();

        // Assert
        colonne.Should().Be(typeAttendu);
    }
}

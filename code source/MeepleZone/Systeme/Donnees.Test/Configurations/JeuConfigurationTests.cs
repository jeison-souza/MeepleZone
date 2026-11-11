using FluentAssertions;
using MeepleZone.Systeme.Donnees.Test.Extensions;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.Configurations;

public class JeuConfigurationTests(ModeleDonneesFixture modele) : IClassFixture<ModeleDonneesFixture>
{
    private readonly ModeleDonneesFixture _modele = modele;

    [Fact]
    public void Jeu_Table_PorteLeNomJeu()
    {
        // Arrange
        var type = _modele.TypeEntite<Jeu>();

        // Act
        var nomTable = type.GetTableName();

        // Assert
        nomTable.Should().Be("jeu");
    }

    [Theory]
    [InlineData("ck_jeu_nom", "char_length(nom) BETWEEN 1 AND 255")]
    [InlineData("ck_jeu_joueurs_min", "joueurs_min >= 1")]
    [InlineData("ck_jeu_joueurs_max", "joueurs_max >= 1")]
    [InlineData("ck_jeu_joueurs_meilleur", "joueurs_meilleur >= 1")]
    [InlineData("ck_jeu_duree_min_minutes", "duree_min_minutes > 0")]
    [InlineData("ck_jeu_duree_max_minutes", "duree_max_minutes > 0")]
    [InlineData("ck_jeu_age_min", "age_min >= 0")]
    [InlineData("ck_jeu_complexite", "complexite BETWEEN 1 AND 5")]
    [InlineData("ck_jeu_statut", "statut IN ('en_attente', 'valide', 'refuse')")]
    [InlineData("ck_jeu_joueurs_min_max", "joueurs_min IS NULL OR joueurs_max IS NULL OR joueurs_min <= joueurs_max")]
    [InlineData("ck_jeu_joueurs_meilleur_min", "joueurs_meilleur IS NULL OR joueurs_min IS NULL OR joueurs_meilleur >= joueurs_min")]
    [InlineData("ck_jeu_joueurs_meilleur_max", "joueurs_meilleur IS NULL OR joueurs_max IS NULL OR joueurs_meilleur <= joueurs_max")]
    [InlineData("ck_jeu_duree_min_max", "duree_min_minutes IS NULL OR duree_max_minutes IS NULL OR duree_min_minutes <= duree_max_minutes")]
    [InlineData("ck_jeu_couverture", "couverture_id IS NULL OR couverture_url IS NULL")]
    public void Jeu_ContrainteCheck_EstDeclareeAvecSonSql(string nom, string sqlAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<Jeu>();

        // Act
        var contrainte = type.GetCheckConstraints().SingleOrDefault(c => c.Name == nom);

        // Assert
        contrainte.Should().NotBeNull();
        contrainte!.Sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void Jeu_ClePrimaire_EstComposeeDeId()
    {
        // Arrange
        var type = _modele.TypeEntite<Jeu>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.Properties.Select(p => p.Name).Should().Equal(nameof(Jeu.Id));
    }

    [Theory]
    [InlineData(nameof(Jeu.Id), false)]
    [InlineData(nameof(Jeu.Nom), false)]
    [InlineData(nameof(Jeu.AnneePublication), true)]
    [InlineData(nameof(Jeu.JoueursMin), true)]
    [InlineData(nameof(Jeu.JoueursMax), true)]
    [InlineData(nameof(Jeu.JoueursMeilleur), true)]
    [InlineData(nameof(Jeu.DureeMinMinutes), true)]
    [InlineData(nameof(Jeu.DureeMaxMinutes), true)]
    [InlineData(nameof(Jeu.AgeMin), true)]
    [InlineData(nameof(Jeu.Complexite), true)]
    [InlineData(nameof(Jeu.ModeJeuId), true)]
    [InlineData(nameof(Jeu.Editeur), true)]
    [InlineData(nameof(Jeu.BggId), true)]
    [InlineData(nameof(Jeu.CouvertureId), true)]
    [InlineData(nameof(Jeu.CouvertureUrl), true)]
    [InlineData(nameof(Jeu.Statut), false)]
    [InlineData(nameof(Jeu.CreeParId), false)]
    [InlineData(nameof(Jeu.CreeLe), false)]
    [InlineData(nameof(Jeu.ModifieLe), false)]
    public void Jeu_Propriete_RespecteLaNullabilite(string nom, bool peutEtreNulle)
    {
        // Arrange
        var propriete = _modele.Propriete<Jeu>(nom);

        // Act
        var accepteNull = propriete.IsNullable;

        // Assert
        accepteNull.Should().Be(peutEtreNulle);
    }

    [Theory]
    [InlineData(nameof(Jeu.Id), false)]
    [InlineData(nameof(Jeu.Nom), false)]
    [InlineData(nameof(Jeu.BggId), true)]
    [InlineData(nameof(Jeu.ModeJeuId), false)]
    [InlineData(nameof(Jeu.CouvertureId), false)]
    [InlineData(nameof(Jeu.CreeParId), false)]
    public void Jeu_Propriete_PossedeUnIndexUnique(string nom, bool doitEtreUnique)
    {
        // Arrange
        var propriete = _modele.Propriete<Jeu>(nom);

        // Act
        var estUnique = propriete.IsUniqueIndex();

        // Assert
        estUnique.Should().Be(doitEtreUnique);
    }

    [Fact]
    public void Jeu_Statut_PorteLaValeurParDefaut()
    {
        // Arrange
        var propriete = _modele.Propriete<Jeu>(nameof(Jeu.Statut));

        // Act
        var valeur = propriete.GetDefaultValue();

        // Assert
        valeur.Should().Be("en_attente");
    }

    [Theory]
    [InlineData(nameof(Jeu.CreeLe), "now()")]
    [InlineData(nameof(Jeu.ModifieLe), "now()")]
    public void Jeu_Propriete_PorteLaValeurSqlParDefaut(string nom, string sqlAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<Jeu>(nom);

        // Act
        var sql = propriete.GetDefaultValueSql();

        // Assert
        sql.Should().Be(sqlAttendu);
    }

    [Fact]
    public void Jeu_ClePrimaire_PorteLeNomDeLaConvention()
    {
        // Arrange
        var type = _modele.TypeEntite<Jeu>();

        // Act
        var cle = type.FindPrimaryKey();

        // Assert
        cle.Should().NotBeNull();
        cle!.GetName().Should().Be("pk_jeu");
    }

    [Theory]
    [InlineData(nameof(Jeu.ModeJeuId), DeleteBehavior.Restrict, "fk_jeu_mode_jeu_mode_jeu_id", typeof(ModeJeu))]
    [InlineData(nameof(Jeu.CouvertureId), DeleteBehavior.SetNull, "fk_jeu_annexe_couverture_id", typeof(Annexe))]
    [InlineData(nameof(Jeu.CreeParId), DeleteBehavior.Restrict, "fk_jeu_utilisateur_cree_par_id", typeof(Utilisateur))]
    public void Jeu_CleEtrangere_PossedeLaBonneConfiguration(string nom, DeleteBehavior suppressionAttendue, string nomContrainteAttendu, System.Type principalAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<Jeu>();
        var propriete = _modele.Propriete<Jeu>(nom);

        // Act
        var cle = type.GetForeignKeys().SingleOrDefault(c => c.Properties.Contains(propriete));

        // Assert
        cle.Should().NotBeNull();
        cle!.DeleteBehavior.Should().Be(suppressionAttendue);
        cle.GetConstraintName().Should().Be(nomContrainteAttendu);
        cle.PrincipalEntityType.ClrType.Should().Be(principalAttendu);
    }

    [Theory]
    [InlineData(nameof(Jeu.BggId), "ix_jeu_bgg_id")]
    [InlineData(nameof(Jeu.ModeJeuId), "ix_jeu_mode_jeu_id")]
    [InlineData(nameof(Jeu.CouvertureId), "ix_jeu_couverture_id")]
    [InlineData(nameof(Jeu.CreeParId), "ix_jeu_cree_par_id")]
    public void Jeu_Index_PorteLeNomDeLaConvention(string nom, string nomAttendu)
    {
        // Arrange
        var type = _modele.TypeEntite<Jeu>();
        var propriete = _modele.Propriete<Jeu>(nom);

        // Act
        var index = type.FindIndex(propriete);

        // Assert
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be(nomAttendu);
    }

    [Theory]
    [InlineData("ix_jeu_bgg_id", true)]
    [InlineData("ix_jeu_mode_jeu_id", false)]
    [InlineData("ix_jeu_couverture_id", false)]
    [InlineData("ix_jeu_cree_par_id", false)]
    public void Jeu_Index_PorteLaBonneUnicite(string nomIndex, bool estUnique)
    {
        // Arrange
        var type = _modele.TypeEntite<Jeu>();

        // Act
        var index = type.GetIndexes().SingleOrDefault(i => i.GetDatabaseName() == nomIndex);

        // Assert
        index.Should().NotBeNull();
        index!.IsUnique.Should().Be(estUnique);
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(Jeu), MemberType = typeof(TestsExtensions))]
    public void Jeu_Propriete_NePossedePasDeCollation(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Jeu>(nom);

        // Act
        var collation = propriete.GetCollation();

        // Assert
        collation.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestsExtensions.NomsDesProprietes), typeof(Jeu), MemberType = typeof(TestsExtensions))]
    public void Jeu_Propriete_PorteLeNomDeColonneDuSchema(string nom)
    {
        // Arrange
        var propriete = _modele.Propriete<Jeu>(nom);
        var valeurAttendue = nom.EnSnakeCase();

        // Act
        var colonne = propriete.GetColumnName();

        // Assert
        colonne.Should().Be(valeurAttendue);
    }

    [Theory]
    [InlineData(nameof(Jeu.Id), "uuid")]
    [InlineData(nameof(Jeu.Nom), "text")]
    [InlineData(nameof(Jeu.AnneePublication), "smallint")]
    [InlineData(nameof(Jeu.JoueursMin), "smallint")]
    [InlineData(nameof(Jeu.JoueursMax), "smallint")]
    [InlineData(nameof(Jeu.JoueursMeilleur), "smallint")]
    [InlineData(nameof(Jeu.DureeMinMinutes), "smallint")]
    [InlineData(nameof(Jeu.DureeMaxMinutes), "smallint")]
    [InlineData(nameof(Jeu.AgeMin), "smallint")]
    [InlineData(nameof(Jeu.Complexite), "numeric(2,1)")]
    [InlineData(nameof(Jeu.ModeJeuId), "uuid")]
    [InlineData(nameof(Jeu.Editeur), "text")]
    [InlineData(nameof(Jeu.BggId), "integer")]
    [InlineData(nameof(Jeu.CouvertureId), "uuid")]
    [InlineData(nameof(Jeu.CouvertureUrl), "text")]
    [InlineData(nameof(Jeu.Statut), "text")]
    [InlineData(nameof(Jeu.CreeParId), "uuid")]
    [InlineData(nameof(Jeu.CreeLe), "timestamp with time zone")]
    [InlineData(nameof(Jeu.ModifieLe), "timestamp with time zone")]
    public void Jeu_Propriete_PorteLeBonTypeDeColonneDansSchema(string nom, string typeAttendu)
    {
        // Arrange
        var propriete = _modele.Propriete<Jeu>(nom);

        // Act
        var colonne = propriete.GetColumnType();

        // Assert
        colonne.Should().Be(typeAttendu);
    }
}

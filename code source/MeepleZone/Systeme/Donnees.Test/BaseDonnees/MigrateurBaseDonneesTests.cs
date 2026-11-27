using FluentAssertions;
using MeepleZone.Systeme.Donnees.BaseDonnees;
using Npgsql;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.BaseDonnees;

public class MigrateurBaseDonneesTests
{
    private const string ChaineValide =
        "Host=hote-fictif;Database=base-fictive;Username=compte-fictif;Password=mot-de-passe-fictif;SSL Mode=VerifyFull;Pooling=true;Command Timeout=30";

    [Fact]
    public void PreparerChaine_ChaineValide_DesactiveLePooling()
    {
        // Act
        var resultat = new NpgsqlConnectionStringBuilder(MigrateurBaseDonnees.PreparerChaine(ChaineValide));

        // Assert
        resultat.Pooling.Should().BeFalse();
    }

    [Fact]
    public void PreparerChaine_ChaineValide_FixeLeDelaiDeCommande()
    {
        // Act
        var resultat = new NpgsqlConnectionStringBuilder(MigrateurBaseDonnees.PreparerChaine(ChaineValide));

        // Assert
        resultat.CommandTimeout.Should().Be(120);
    }

    [Fact]
    public void PreparerChaine_ChaineValide_PreserveLesAutresParametres()
    {
        // Act
        var resultat = new NpgsqlConnectionStringBuilder(MigrateurBaseDonnees.PreparerChaine(ChaineValide));

        // Assert
        resultat.Host.Should().Be("hote-fictif");
        resultat.Database.Should().Be("base-fictive");
        resultat.Username.Should().Be("compte-fictif");
        resultat.Password.Should().Be("mot-de-passe-fictif");
        resultat.SslMode.Should().Be(SslMode.VerifyFull);
    }
}

using FluentAssertions;
using MeepleZone.Site.Versionnage;
using Xunit;

namespace MeepleZone.Site.Test.Versionnage;

public class InformationsVersionTests
{
    private const string Sha = "82716290123456789abcdef0123456789abcdef0";

    [Theory]
    [InlineData("2026.11.25.158")]
    [InlineData("0.3.1")]
    public void Lire_AvecNumero_LeConserveTelQuel(string numero)
    {
        // Act
        var version = InformationsVersion.Lire($"{numero}+{Sha}");

        // Assert
        version.Numero.Should().Be(numero);
        version.Commit.Should().Be(Sha);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("+")]
    public void Lire_ValeursAbsentesOuVides_ValentDev(string? valeur)
    {
        // Act
        var version = InformationsVersion.Lire(valeur);

        // Assert
        version.Numero.Should().Be("dev");
        version.Commit.Should().Be("dev");
    }

    [Fact]
    public void Lire_AssemblyConstruitHorsDuPipeline_VautDev()
    {
        // Act
        var version = InformationsVersion.Lire(typeof(InformationsVersion).Assembly);

        // Assert
        version.Numero.Should().Be("dev");
        version.Commit.Should().Be("dev");
    }

    [Fact]
    public void Libelle_AvecNumeroEtCommit_AfficheSeptCaracteresDuCommit()
    {
        // Act / Assert
        InformationsVersion.Lire($"2026.11.25.158+{Sha}").Libelle.Should().Be("Version 2026.11.25.158 · 8271629");
    }

    [Theory]
    [InlineData(null, "Version dev")]
    [InlineData("dev", "Version dev")]
    [InlineData("0.3.1", "Version 0.3.1")]
    [InlineData("0.3.1+", "Version 0.3.1")]
    [InlineData("dev+dev", "Version dev")]
    public void Libelle_SansCommit_NAfficheQueLeNumero(string? versionInformationnelle, string attendu)
    {
        // Act / Assert
        InformationsVersion.Lire(versionInformationnelle).Libelle.Should().Be(attendu);
    }
}

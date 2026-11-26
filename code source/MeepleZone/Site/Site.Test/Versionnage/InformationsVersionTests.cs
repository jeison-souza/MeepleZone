using FluentAssertions;
using MeepleZone.Site.Versionnage;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
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
        var version = Lire(numero, Sha);

        // Assert
        version.Numero.Should().Be(numero);
        version.Commit.Should().Be(Sha);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Lire_ValeursAbsentesOuVides_ValentDev(string? valeur)
    {
        // Act
        var version = Lire(valeur, valeur);

        // Assert
        version.Numero.Should().Be("dev");
        version.Commit.Should().Be("dev");
    }

    [Fact]
    public void Libelle_AvecNumeroEtCommit_AfficheSeptCaracteresDuCommit()
    {
        // Act / Assert
        Lire("2026.11.25.158", Sha).Libelle.Should().Be("Version 2026.11.25.158 · 8271629");
    }

    [Theory]
    [InlineData(null, null, "Version dev")]
    [InlineData("", "", "Version dev")]
    [InlineData("0.3.1", null, "Version 0.3.1")]
    [InlineData(null, "dev", "Version dev")]
    public void Libelle_SansCommit_NAfficheQueLeNumero(string? numero, string? commit, string attendu)
    {
        // Act / Assert
        Lire(numero, commit).Libelle.Should().Be(attendu);
    }

    private static InformationsVersion Lire(string? numero, string? commit)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Version:Numero"] = numero,
                ["Version:Commit"] = commit,
            })
            .Build();

        return InformationsVersion.Lire(configuration);
    }
}

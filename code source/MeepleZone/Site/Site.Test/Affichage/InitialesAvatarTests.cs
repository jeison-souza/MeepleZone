using FluentAssertions;
using MeepleZone.Site.Affichage;
using Xunit;

namespace MeepleZone.Site.Test.Affichage;

public class InitialesAvatarTests
{
    [Theory]
    [InlineData("Joao vicente de paula", "JP")]
    [InlineData("Marie Tremblay", "MT")]
    [InlineData("marie tremblay", "MT")]
    [InlineData("Jean Pierre Louis Gagnon", "JG")]
    [InlineData("  Marie     Tremblay  ", "MT")]
    [InlineData("J. Silva", "JS")]
    [InlineData("Ana-Maria O'Brien", "AO")]
    public void Calculer_AvecPlusieursMots_RetournePremiereLettreDuPremierEtDuDernierMot(string nom, string attendu)
    {
        // Act
        var initiales = InitialesAvatar.Calculer(nom);

        // Assert
        initiales.Should().Be(attendu);
    }

    [Theory]
    [InlineData("Marcelo", "MA")]
    [InlineData("  marcelo  ", "MA")]
    [InlineData("Ana-Maria", "AN")]
    public void Calculer_AvecUnSeulMot_RetourneSesDeuxPremieresLettres(string nom, string attendu)
    {
        // Act
        var initiales = InitialesAvatar.Calculer(nom);

        // Assert
        initiales.Should().Be(attendu);
    }

    [Theory]
    [InlineData("José Álvares", "JA")]
    [InlineData("Élodie Côté", "EC")]
    [InlineData("François", "FR")]
    public void Calculer_AvecAccents_RetireLesAccents(string nom, string attendu)
    {
        // Act
        var initiales = InitialesAvatar.Calculer(nom);

        // Assert
        initiales.Should().Be(attendu);
    }

    [Theory]
    [InlineData("Ana 🎲", "AN")]
    [InlineData("🎲 Ana Souza", "AS")]
    [InlineData("Ana 田中 Souza", "AS")]
    [InlineData("Ana 2 Souza", "AS")]
    public void Calculer_AvecMotsSansLettre_IgnoreCesMots(string nom, string attendu)
    {
        // Act
        var initiales = InitialesAvatar.Calculer(nom);

        // Assert
        initiales.Should().Be(attendu);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData("🎲")]
    [InlineData("🎲 🎯")]
    [InlineData("田中 太郎")]
    [InlineData("1234")]
    public void Calculer_SansLettreExploitable_RetourneNull(string? nom)
    {
        // Act
        var initiales = InitialesAvatar.Calculer(nom);

        // Assert
        initiales.Should().BeNull();
    }
}

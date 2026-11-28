using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte;
using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Site.Authentification.Suppression;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Site.Test.Authentification;

public class GestionSuppressionCompteTests
{
    private const string Code = "ABCD2345";

    [Theory]
    [InlineData("ABCD2345", true)]
    [InlineData("abcd2345", true)]
    [InlineData("  ABCD2345 ", true)]
    [InlineData("ABCD2346", false)]
    [InlineData("ABCD234", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void ConfirmationValide_ComparaisonAuCodeAttendu(string? saisie, bool attendu)
    {
        // Act
        var resultat = GestionSuppressionCompte.ConfirmationValide(saisie, Code);

        // Assert
        resultat.Should().Be(attendu);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ConfirmationValide_SansCodeAttendu_RetourneFaux(string? codeAttendu)
    {
        // Act / Assert
        GestionSuppressionCompte.ConfirmationValide("", codeAttendu).Should().BeFalse();
        GestionSuppressionCompte.ConfirmationValide("ABCD2345", codeAttendu).Should().BeFalse();
    }

    [Fact]
    public async Task Traiter_AvecMauvaisCode_RevientAuProfilSansSupprimer()
    {
        // Arrange
        var domaine = CreerDomaine(StatutSuppressionCompte.Supprime);

        // Act
        var resultat = await GestionSuppressionCompte.Traiter("AAAAAAAA", domaine, A.Fake<ILogger>());

        // Assert
        resultat.Should().BeOfType<RedirectHttpResult>().Which.Url.Should().Be("/profil?suppression=confirmation");
        A.CallTo(() => domaine.SupprimerCompteAsync(A<SupprimerCompteRequete>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Traiter_Supprime_RetireLeCookieEtMeneALaPageDAdieu()
    {
        // Arrange
        var domaine = CreerDomaine(StatutSuppressionCompte.Supprime);

        // Act
        var resultat = await GestionSuppressionCompte.Traiter(Code, domaine, A.Fake<ILogger>());

        // Assert
        resultat.Should().BeOfType<SignOutHttpResult>().Which.Properties!.RedirectUri.Should().Be("/compte-supprime");
        A.CallTo(() => domaine.SupprimerCompteAsync(new SupprimerCompteRequete())).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(StatutSuppressionCompte.CompteProtege, "/profil?suppression=protege")]
    [InlineData(StatutSuppressionCompte.Conflit, "/profil?suppression=conflit")]
    public async Task Traiter_StatutDeRefus_RevientAuProfilAvecLeMotif(StatutSuppressionCompte statut, string attendu)
    {
        // Act
        var resultat = await GestionSuppressionCompte.Traiter(Code, CreerDomaine(statut), A.Fake<ILogger>());

        // Assert
        resultat.Should().BeOfType<RedirectHttpResult>().Which.Url.Should().Be(attendu);
    }

    [Fact]
    public async Task Traiter_CompteIntrouvable_RetireLeCookieEtMeneALAccueil()
    {
        // Act
        var resultat = await GestionSuppressionCompte.Traiter(Code, CreerDomaine(StatutSuppressionCompte.Introuvable), A.Fake<ILogger>());

        // Assert
        resultat.Should().BeOfType<SignOutHttpResult>().Which.Properties!.RedirectUri.Should().Be("/");
    }

    [Fact]
    public async Task Traiter_ProfilIntrouvable_RetireLeCookieSansSupprimer()
    {
        // Arrange
        var domaine = A.Fake<IDomaineCompte>();
        A.CallTo(() => domaine.ObtenirProfilAsync(A<ObtenirProfilRequete>._)).Returns(Task.FromResult<ProfilDto?>(null));

        // Act
        var resultat = await GestionSuppressionCompte.Traiter(Code, domaine, A.Fake<ILogger>());

        // Assert
        resultat.Should().BeOfType<SignOutHttpResult>().Which.Properties!.RedirectUri.Should().Be("/");
        A.CallTo(() => domaine.SupprimerCompteAsync(A<SupprimerCompteRequete>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Traiter_QuandLeDomaineEchoue_RevientAuProfilAvecErreur()
    {
        // Arrange
        var domaine = CreerDomaine(StatutSuppressionCompte.Supprime);
        A.CallTo(() => domaine.SupprimerCompteAsync(A<SupprimerCompteRequete>._)).Throws<InvalidOperationException>();

        // Act
        var resultat = await GestionSuppressionCompte.Traiter(Code, domaine, A.Fake<ILogger>());

        // Assert
        resultat.Should().BeOfType<RedirectHttpResult>().Which.Url.Should().Be("/profil?suppression=erreur");
    }

    [Theory]
    [InlineData("confirmation")]
    [InlineData("protege")]
    [InlineData("conflit")]
    [InlineData("erreur")]
    public void Message_ConnaitLesMotifsDeLAdresse(string motif)
    {
        // Act / Assert
        GestionSuppressionCompte.Message(motif).Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("<script>alert(1)</script>")]
    public void Message_AvecMotifAbsentOuInconnu_RetourneNull(string? motif)
    {
        // Act / Assert
        GestionSuppressionCompte.Message(motif).Should().BeNull();
    }

    private static IDomaineCompte CreerDomaine(StatutSuppressionCompte statut)
    {
        var domaine = A.Fake<IDomaineCompte>();
        A.CallTo(() => domaine.ObtenirProfilAsync(A<ObtenirProfilRequete>._))
            .Returns(Task.FromResult<ProfilDto?>(new ProfilDto("Test", null, Code, VisibiliteCollection.Privee)));
        A.CallTo(() => domaine.SupprimerCompteAsync(A<SupprimerCompteRequete>._)).Returns(new SupprimerCompteReponse(statut));

        return domaine;
    }
}

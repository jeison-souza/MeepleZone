using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Site.Authentification;
using MeepleZone.Site.Authentification.Liaison;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Site.Test.Authentification;

public class GestionLiaisonCompteTests
{
    private static readonly AuthenticationScheme SchemaGoogle = new("Google", "Google", typeof(GoogleHandler));

    [Fact]
    public void CreerProprietes_PoseLeMarqueurEtRevientAuProfil()
    {
        // Act
        var proprietes = GestionLiaisonCompte.CreerProprietes();

        // Assert
        proprietes.Items.Should().Contain(GestionLiaisonCompte.CleLiaison, GestionLiaisonCompte.ValeurLiaison);
        proprietes.RedirectUri.Should().Be("/profil");
    }

    [Fact]
    public void EstLiaison_AvecProprietesDeLiaison_RetourneVrai()
    {
        // Act / Assert
        GestionLiaisonCompte.EstLiaison(GestionLiaisonCompte.CreerProprietes()).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("True")]
    [InlineData("1")]
    public void EstLiaison_SansLeMarqueurExact_RetourneFaux(string? valeur)
    {
        // Arrange
        var proprietes = new AuthenticationProperties();

        if (valeur is not null)
        {
            proprietes.Items[GestionLiaisonCompte.CleLiaison] = valeur;
        }

        // Act / Assert
        GestionLiaisonCompte.EstLiaison(proprietes).Should().BeFalse();
        GestionLiaisonCompte.EstLiaison(null).Should().BeFalse();
    }

    [Theory]
    [InlineData(ResultatLiaison.Reussie, "Google", "/profil?liaison=reussie&fournisseur=Google")]
    [InlineData(ResultatLiaison.DejaUtilisee, "microsoft", "/profil?liaison=deja-utilisee&fournisseur=Microsoft")]
    [InlineData(ResultatLiaison.Echec, "Facebook", "/profil?liaison=echec")]
    [InlineData(ResultatLiaison.Echec, null, "/profil?liaison=echec")]
    public void UrlRetour_NeRetientQueLesFournisseursConnus(ResultatLiaison resultat, string? fournisseur, string attendu)
    {
        // Act
        var url = GestionLiaisonCompte.UrlRetour(resultat, fournisseur);

        // Assert
        url.Should().Be(attendu);
    }

    [Fact]
    public async Task Traiter_HorsLiaison_NeFaitRien()
    {
        // Arrange
        var liaison = A.Fake<ILiaisonFournisseur>();
        var http = CreerContexteHttp(liaison, A.Fake<IAuthenticationService>());
        var contexte = CreerContexte(http, new AuthenticationProperties());

        // Act
        await GestionLiaisonCompte.Traiter(contexte);

        // Assert
        contexte.Result.Should().BeNull();
        A.CallTo(liaison).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(StatutLiaison.Liee, "/profil?liaison=reussie&fournisseur=Google")]
    [InlineData(StatutLiaison.DejaLieeAuMemeCompte, "/profil?liaison=deja-liee&fournisseur=Google")]
    [InlineData(StatutLiaison.DejaUtiliseeParAutreCompte, "/profil?liaison=deja-utilisee&fournisseur=Google")]
    public async Task Traiter_UtilisateurConnecte_LieEtRevientAuProfilSansCreerDeCookie(StatutLiaison statut, string attendu)
    {
        // Arrange
        var utilisateur = CreerPrincipal(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString());
        var liaison = A.Fake<ILiaisonFournisseur>();
        A.CallTo(() => liaison.LierAsync(new LierFournisseurRequete("Google", "id-externe", "externe@exemple.ca")))
            .Returns(new LierFournisseurReponse(statut));
        var http = CreerContexteHttp(liaison, CreerServiceAuthentification(utilisateur));
        var contexte = CreerContexte(http, GestionLiaisonCompte.CreerProprietes());

        // Act
        await GestionLiaisonCompte.Traiter(contexte);

        // Assert
        http.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        http.Response.Headers.Location.ToString().Should().Be(attendu);
        contexte.Result!.Handled.Should().BeTrue();
        http.User.Should().BeSameAs(utilisateur);
    }

    [Fact]
    public async Task Traiter_SansUtilisateurConnecte_RedirigeVersLaConnexionSansLier()
    {
        // Arrange
        var liaison = A.Fake<ILiaisonFournisseur>();
        var http = CreerContexteHttp(liaison, CreerServiceAuthentification(null));
        var contexte = CreerContexte(http, GestionLiaisonCompte.CreerProprietes());

        // Act
        await GestionLiaisonCompte.Traiter(contexte);

        // Assert
        http.Response.Headers.Location.ToString().Should().Be("/connexion-requise");
        contexte.Result!.Handled.Should().BeTrue();
        A.CallTo(liaison).MustNotHaveHappened();
    }

    [Fact]
    public async Task Traiter_QuandLeDomaineEchoue_RevientAuProfilAvecEchec()
    {
        // Arrange
        var utilisateur = CreerPrincipal(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString());
        var liaison = A.Fake<ILiaisonFournisseur>();
        A.CallTo(() => liaison.LierAsync(A<LierFournisseurRequete>._)).Throws<InvalidOperationException>();
        var http = CreerContexteHttp(liaison, CreerServiceAuthentification(utilisateur));
        var contexte = CreerContexte(http, GestionLiaisonCompte.CreerProprietes());

        // Act
        await GestionLiaisonCompte.Traiter(contexte);

        // Assert
        http.Response.Headers.Location.ToString().Should().Be("/profil?liaison=echec&fournisseur=Google");
        contexte.Result!.Handled.Should().BeTrue();
    }

    [Fact]
    public async Task EchecConnexion_PendantUneLiaison_RevientAuProfil()
    {
        // Arrange
        var http = CreerContexteHttp(A.Fake<ILiaisonFournisseur>(), A.Fake<IAuthenticationService>());
        var echec = new RemoteFailureContext(http, SchemaGoogle, new GoogleOptions(), new AuthenticationFailureException("The oauth state was missing or invalid."))
        {
            Properties = GestionLiaisonCompte.CreerProprietes(),
        };

        // Act
        await GestionEchecConnexion.Traiter(echec);

        // Assert
        http.Response.Headers.Location.ToString().Should().Be("/profil?liaison=echec&fournisseur=Google");
    }

    [Fact]
    public async Task RefusPendantUneLiaison_RevientAuProfilSansMessage()
    {
        // Arrange
        var http = CreerContexteHttp(A.Fake<ILiaisonFournisseur>(), A.Fake<IAuthenticationService>());
        var refus = new AccessDeniedContext(http, SchemaGoogle, new GoogleOptions())
        {
            Properties = GestionLiaisonCompte.CreerProprietes(),
        };

        // Act
        await GestionEchecConnexion.TraiterRefus(refus);

        // Assert
        http.Response.Headers.Location.ToString().Should().Be("/profil");
    }

    private static ClaimsPrincipal CreerPrincipal(string type, string valeur)
    {
        return new ClaimsPrincipal(new ClaimsIdentity([new Claim(type, valeur)], "test"));
    }

    private static IAuthenticationService CreerServiceAuthentification(ClaimsPrincipal? utilisateurCookie)
    {
        var service = A.Fake<IAuthenticationService>();
        var resultat = utilisateurCookie is null
            ? AuthenticateResult.NoResult()
            : AuthenticateResult.Success(new AuthenticationTicket(utilisateurCookie, CookieAuthenticationDefaults.AuthenticationScheme));

        A.CallTo(() => service.AuthenticateAsync(A<HttpContext>._, CookieAuthenticationDefaults.AuthenticationScheme)).Returns(resultat);

        return service;
    }

    private static DefaultHttpContext CreerContexteHttp(ILiaisonFournisseur liaison, IAuthenticationService authentification)
    {
        var services = new ServiceCollection()
            .AddSingleton(liaison)
            .AddSingleton(authentification)
            .AddLogging()
            .BuildServiceProvider();

        return new DefaultHttpContext { RequestServices = services };
    }

    private static TicketReceivedContext CreerContexte(HttpContext http, AuthenticationProperties proprietes)
    {
        var principalFournisseur = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "id-externe"),
            new Claim(ClaimTypes.Email, "externe@exemple.ca"),
            new Claim(ClaimTypes.Name, "Nom Externe"),
        ], "Google"));

        var ticket = new AuthenticationTicket(principalFournisseur, proprietes, "Google");

        return new TicketReceivedContext(http, SchemaGoogle, new GoogleOptions(), ticket) { Properties = proprietes };
    }
}

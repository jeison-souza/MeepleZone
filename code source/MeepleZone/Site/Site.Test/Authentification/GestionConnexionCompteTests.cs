using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte;
using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Site.Authentification;
using MeepleZone.Systeme.BaseTest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Site.Test.Authentification;

public class GestionConnexionCompteTests : BaseTest<object>
{
    private const string NomSchema = "Google";

    private static readonly AuthenticationScheme SchemaGoogle = new(NomSchema, NomSchema, typeof(GoogleHandler));

    [Fact]
    public async Task Traiter_Connecte_RemplacerLePrincipalParLIdentiteInterne()
    {
        // Arrange
        var identifiantExterne = CreerFixture<string>();
        var utilisateur = CreerFixture<UtilisateurConnecteDto>();
        var domaine = CreerDomaine(new GererConnexionReponse(StatutConnexion.Connecte, utilisateur));
        var contexte = CreerContexte(domaine, CreerPrincipalFournisseur(identifiantExterne));

        // Act
        await GestionConnexionCompte.Traiter(contexte);

        // Assert
        var principal = contexte.Principal!;
        principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be(utilisateur.Id.ToString());
        principal.FindFirst(ClaimTypes.Name)!.Value.Should().Be(utilisateur.NomAffichage);
        principal.FindFirst(ClaimTypes.Email)!.Value.Should().Be(utilisateur.Courriel);
        principal.FindFirst(ClaimTypes.Role)!.Value.Should().Be(utilisateur.Role);
        principal.Claims.Should().NotContain(c => c.Value == identifiantExterne);
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task Traiter_ClaimsDuFournisseur_AppelerLeDomaineAvecLaRequeteAttendue()
    {
        // Arrange
        var identifiantExterne = CreerFixture<string>();
        var courriel = CreerFixture<string>();
        var nomAffichage = CreerFixture<string>();
        var requeteAttendue = new GererConnexionRequete(NomSchema, identifiantExterne, courriel, nomAffichage, false);
        var domaine = CreerDomaine(new GererConnexionReponse(StatutConnexion.Connecte, CreerFixture<UtilisateurConnecteDto>()));
        var contexte = CreerContexte(domaine, CreerPrincipalFournisseur(identifiantExterne, courriel, nomAffichage));

        // Act
        await GestionConnexionCompte.Traiter(contexte);

        // Assert
        A.CallTo(() => domaine.GererConnexionAsync(requeteAttendue)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("TRUE", false)]
    [InlineData("1", false)]
    [InlineData("", false)]
    [InlineData(" true", false)]
    [InlineData("autre", false)]
    public async Task Traiter_MarqueurForcerCreation_NeForcerQueAvecLaValeurExacte(string valeur, bool forcerCreationAttendu)
    {
        // Arrange
        var identifiantExterne = CreerFixture<string>();
        var requeteAttendue = new GererConnexionRequete(NomSchema, identifiantExterne, null, null, forcerCreationAttendu);
        var domaine = CreerDomaine(new GererConnexionReponse(StatutConnexion.Connecte, CreerFixture<UtilisateurConnecteDto>()));
        var proprietes = new AuthenticationProperties();
        proprietes.Items[GestionConnexionCompte.CleForcerCreation] = valeur;
        var contexte = CreerContexte(domaine, CreerPrincipalFournisseur(identifiantExterne, null, null), proprietes);

        // Act
        await GestionConnexionCompte.Traiter(contexte);

        // Assert
        A.CallTo(() => domaine.GererConnexionAsync(requeteAttendue)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Traiter_MarqueurAbsent_NePasForcerLaCreation()
    {
        // Arrange
        var identifiantExterne = CreerFixture<string>();
        var requeteAttendue = new GererConnexionRequete(NomSchema, identifiantExterne, null, null, false);
        var domaine = CreerDomaine(new GererConnexionReponse(StatutConnexion.Connecte, CreerFixture<UtilisateurConnecteDto>()));
        var contexte = CreerContexte(domaine, CreerPrincipalFournisseur(identifiantExterne, null, null));

        // Act
        await GestionConnexionCompte.Traiter(contexte);

        // Assert
        A.CallTo(() => domaine.GererConnexionAsync(requeteAttendue)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Traiter_CompteProbable_LancerExceptionEtGarderLePrincipalDuFournisseur()
    {
        // Arrange
        var principalFournisseur = CreerPrincipalFournisseur(CreerFixture<string>());
        var domaine = CreerDomaine(new GererConnexionReponse(StatutConnexion.CompteProbable, null));
        var contexte = CreerContexte(domaine, principalFournisseur);

        // Act
        Func<Task> acte = () => GestionConnexionCompte.Traiter(contexte);

        // Assert
        await acte.Should().ThrowExactlyAsync<CompteProbableException>();
        contexte.Principal.Should().BeSameAs(principalFournisseur);
    }

    [Fact]
    public async Task Traiter_ConnecteSansUtilisateur_LancerInvalidOperationException()
    {
        // Arrange
        var domaine = CreerDomaine(new GererConnexionReponse(StatutConnexion.Connecte, null));
        var contexte = CreerContexte(domaine, CreerPrincipalFournisseur(CreerFixture<string>()));

        // Act
        Func<Task> acte = () => GestionConnexionCompte.Traiter(contexte);

        // Assert
        await acte.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Traiter_IdentifiantDuFournisseurAbsent_LancerExceptionSansAppelerLeDomaine()
    {
        // Arrange
        var domaine = CreerDomaine(new GererConnexionReponse(StatutConnexion.Connecte, CreerFixture<UtilisateurConnecteDto>()));
        var principalSansIdentifiant = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, CreerFixture<string>())], NomSchema));
        var contexte = CreerContexte(domaine, principalSansIdentifiant);

        // Act
        Func<Task> acte = () => GestionConnexionCompte.Traiter(contexte);

        // Assert
        await acte.Should().ThrowExactlyAsync<InvalidOperationException>();
        A.CallTo(() => domaine.GererConnexionAsync(A<GererConnexionRequete>._)).MustNotHaveHappened();
    }

    private IDomaineCompte CreerDomaine(GererConnexionReponse reponse)
    {
        var fausse = CreerFausse<IDomaineCompte>(strict: false).FakedObject;
        A.CallTo(() => fausse.GererConnexionAsync(A<GererConnexionRequete>._)).Returns(Task.FromResult(reponse));

        return fausse;
    }

    private static ClaimsPrincipal CreerPrincipalFournisseur(string identifiantExterne, string? courriel = null, string? nomAffichage = null)
    {
        var revendications = new System.Collections.Generic.List<Claim> { new(ClaimTypes.NameIdentifier, identifiantExterne) };

        if (courriel is not null)
        {
            revendications.Add(new Claim(ClaimTypes.Email, courriel));
        }

        if (nomAffichage is not null)
        {
            revendications.Add(new Claim(ClaimTypes.Name, nomAffichage));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(revendications, NomSchema));
    }

    private static OAuthCreatingTicketContext CreerContexte(IDomaineCompte domaine, ClaimsPrincipal principal, AuthenticationProperties? proprietes = null)
    {
        var services = new ServiceCollection()
            .AddSingleton(domaine)
            .BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var jetons = OAuthTokenResponse.Success(JsonDocument.Parse("""{"access_token":"jeton"}"""));
        var utilisateurJson = JsonDocument.Parse("{}").RootElement;

        return new OAuthCreatingTicketContext(
            principal,
            proprietes ?? new AuthenticationProperties(),
            http,
            SchemaGoogle,
            new GoogleOptions(),
            new HttpClient(),
            jetons,
            utilisateurJson);
    }
}

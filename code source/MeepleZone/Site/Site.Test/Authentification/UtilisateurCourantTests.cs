using FakeItEasy;
using FluentAssertions;
using MeepleZone.Site.Authentification;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Site.Test.Authentification;

public class UtilisateurCourantTests
{
    [Fact]
    public void Id_AvecUtilisateurDeLaRequeteHttp_LitCetUtilisateur()
    {
        // Arrange
        var id = Guid.NewGuid();
        var accesHttp = A.Fake<IHttpContextAccessor>();
        A.CallTo(() => accesHttp.HttpContext).Returns(new DefaultHttpContext { User = CreerPrincipal(id) });
        var etat = A.Fake<AuthenticationStateProvider>();

        // Act
        var resultat = new UtilisateurCourant(etat, accesHttp).Id;

        // Assert
        resultat.Should().Be(id);
        A.CallTo(etat).MustNotHaveHappened();
    }

    [Fact]
    public void Id_SansRequeteHttp_LitLEtatDAuthentificationDeBlazor()
    {
        // Arrange
        var id = Guid.NewGuid();
        var etat = A.Fake<AuthenticationStateProvider>();
        A.CallTo(() => etat.GetAuthenticationStateAsync()).Returns(Task.FromResult(new AuthenticationState(CreerPrincipal(id))));
        var accesHttp = A.Fake<IHttpContextAccessor>();

        // Act
        var resultat = new UtilisateurCourant(etat, accesHttp).Id;

        // Assert
        resultat.Should().Be(id);
    }

    [Fact]
    public void Id_SansUtilisateurAuthentifie_Leve()
    {
        // Arrange
        var etat = A.Fake<AuthenticationStateProvider>();
        A.CallTo(() => etat.GetAuthenticationStateAsync()).Returns(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        var utilisateur = new UtilisateurCourant(etat, A.Fake<IHttpContextAccessor>());

        // Act
        var act = () => utilisateur.Id;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    private static ClaimsPrincipal CreerPrincipal(Guid id)
    {
        return new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id.ToString())], "test"));
    }
}

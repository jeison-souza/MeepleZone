using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MeepleZone.Site.Authentification;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MeepleZone.Site.Test.Authentification;

public class GestionEchecConnexionTests
{
    private static readonly AuthenticationScheme SchemaGoogle = new("Google", "Google", typeof(GoogleHandler));

    [Theory]
    [InlineData("access_denied;Description=La personne a refuse l'acces.")]
    [InlineData("consent_required;Description=AADSTS65004: User declined to consent to access the app.")]
    public async Task Traiter_AvecCodeDAnnulation_RedirigeVersAnnuleeSansJournaliser(string message)
    {
        // Arrange
        var journal = new CollecteurJournal();
        var http = CreerContexteHttp(journal);
        var contexte = new RemoteFailureContext(http, SchemaGoogle, new GoogleOptions(), new AuthenticationFailureException(message));

        // Act
        await GestionEchecConnexion.Traiter(contexte);

        // Assert
        http.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        http.Response.Headers.Location.ToString().Should().Be("/connexion-requise?motif=annulee");
        contexte.Result!.Handled.Should().BeTrue();
        journal.Entrees.Should().BeEmpty();
    }

    [Fact]
    public async Task Traiter_AvecErreurDuFournisseur_RedirigeVersErreurEtJournalise()
    {
        // Arrange
        var journal = new CollecteurJournal();
        var http = CreerContexteHttp(journal);
        var echec = new AuthenticationFailureException("The oauth state was missing or invalid.");
        var contexte = new RemoteFailureContext(http, SchemaGoogle, new GoogleOptions(), echec);

        // Act
        await GestionEchecConnexion.Traiter(contexte);

        // Assert
        http.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        http.Response.Headers.Location.ToString().Should().Be("/connexion-requise?motif=erreur");
        contexte.Result!.Handled.Should().BeTrue();
        journal.Entrees.Should().ContainSingle(e => e.Niveau == LogLevel.Warning && e.Exception == echec);
    }

    [Fact]
    public async Task TraiterRefus_AvecRefusExplicite_RedirigeVersAnnulee()
    {
        // Arrange
        var http = CreerContexteHttp(new CollecteurJournal());
        var contexte = new AccessDeniedContext(http, SchemaGoogle, new GoogleOptions());

        // Act
        await GestionEchecConnexion.TraiterRefus(contexte);

        // Assert
        http.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        http.Response.Headers.Location.ToString().Should().Be("/connexion-requise?motif=annulee");
        contexte.Result!.Handled.Should().BeTrue();
    }

    private static DefaultHttpContext CreerContexteHttp(CollecteurJournal journal)
    {
        var services = new ServiceCollection()
            .AddLogging(builder => builder.AddProvider(journal))
            .BuildServiceProvider();

        return new DefaultHttpContext { RequestServices = services };
    }

    /// <summary>Journal minimal qui garde les entrées reçues, pour vérifier ce que le code consigne.</summary>
    private sealed class CollecteurJournal : ILoggerProvider, ILogger
    {
        public List<(LogLevel Niveau, Exception? Exception)> Entrees { get; } = [];

        public ILogger CreateLogger(string categorie) => this;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel niveau) => true;

        public void Log<TState>(LogLevel niveau, EventId evenement, TState etat, Exception? exception, Func<TState, Exception?, string> formateur)
            => Entrees.Add((niveau, exception));

        public void Dispose()
        {
        }
    }
}

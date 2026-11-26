using FluentAssertions;
using MeepleZone.Site.Versionnage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Site.Test.Versionnage;

public class GestionVersionTests
{
    [Fact]
    public async Task Traiter_RepondLeNumeroSeulEnTexteBrutSansMiseEnCache()
    {
        // Arrange
        var version = InformationsVersion.Lire("2026.11.25.158+82716290123456789abcdef0123456789abcdef0");
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        using var corps = new MemoryStream();
        var contexte = new DefaultHttpContext { RequestServices = services };
        contexte.Response.Body = corps;

        // Act
        var resultat = GestionVersion.Traiter(contexte.Response, version);
        await resultat.ExecuteAsync(contexte);

        // Assert
        Encoding.UTF8.GetString(corps.ToArray()).Should().Be("2026.11.25.158");
        contexte.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        contexte.Response.ContentType.Should().StartWith("text/plain");
        contexte.Response.Headers.CacheControl.ToString().Should().Be("no-store");
    }

    [Fact]
    public void Route_EstCelleAttendueParLePipeline()
    {
        // Act / Assert
        GestionVersion.Route.Should().Be("/version");
    }
}

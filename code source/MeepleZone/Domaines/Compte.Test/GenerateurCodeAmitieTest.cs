using FakeItEasy;
using FluentAssertions;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Interfaces.Depots;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test;

public class GenerateurCodeAmitieTest : BaseTest<GenerateurCodeAmitie>
{
    private Fake<IDepotCompte> _depotCompte = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCompte = CreerFausse<IDepotCompte>();
    }

    [Fact]
    public async Task GenererAsync_QuandCodeNExistePas_RetournerCode()
    {
        // Arrange
        string codeRecu = null!;

        _depotCompte
            .CallsTo(x => x.VerifierExistenceCodeAmitieAsync(A<string>.Ignored))
            .Invokes((string code) => codeRecu = code)
            .Returns(false);

        // Act
        string result = await InstanceTest.GenererAsync();

        // Assert
        _depotCompte
            .CallsTo(x => x.VerifierExistenceCodeAmitieAsync(A<string>.Ignored))
            .MustHaveHappenedOnceExactly();
        result.Should().Be(codeRecu);
    }

    [Fact]
    public async Task GenererAsync_QuandCodeExiste_RetournerCodeApresLoop()
    {
        // Arrange
        string codeRecu = null!;

        _depotCompte
            .CallsTo(x => x.VerifierExistenceCodeAmitieAsync(A<string>.Ignored))
            .Returns(true).Once()
            .Then.Invokes((string code) => codeRecu = code)
            .Returns(false);

        // Act
        string result = await InstanceTest.GenererAsync();

        // Assert
        _depotCompte
            .CallsTo(x => x.VerifierExistenceCodeAmitieAsync(A<string>.Ignored))
            .MustHaveHappenedTwiceExactly();
        result.Should().Be(codeRecu);
    }

    [Fact]
    public async Task GenererAsync_QuandCodeExisteToujours_LanceInvalidOperationException()
    {
        // Arrange
        _depotCompte
            .CallsTo(x => x.VerifierExistenceCodeAmitieAsync(A<string>.Ignored))
            .Returns(true);

        // Act
        Func<Task> action = () => InstanceTest.GenererAsync();

        // Assert
        await action.Should().ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Impossible de générer un code d'amitié unique après 5 tentatives.");
        _depotCompte
            .CallsTo(x => x.VerifierExistenceCodeAmitieAsync(A<string>.Ignored))
            .MustHaveHappened(5, Times.Exactly);
    }

    [Fact]
    public async Task GenererAsync_QuandCodeNExistePas_RetournerCodeBonFormat()
    {
        // Arrange
        _depotCompte
            .CallsTo(x => x.VerifierExistenceCodeAmitieAsync(A<string>.Ignored))
            .Returns(false);
        var codes = new List<string>();

        // Act
        for (var i = 0; i < 1000; i++)
        {
            codes.Add(await InstanceTest.GenererAsync());
        }

        // Assert
        _depotCompte
            .CallsTo(x => x.VerifierExistenceCodeAmitieAsync(A<string>.Ignored))
            .MustHaveHappened(1000, Times.Exactly);
        codes.Should().AllSatisfy(code => code.Should().MatchRegex("^[A-HJ-NP-Z2-9]{8}$"));
    }
}

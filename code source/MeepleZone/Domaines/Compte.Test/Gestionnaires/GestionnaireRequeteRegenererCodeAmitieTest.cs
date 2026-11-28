using FakeItEasy;
using FluentAssertions;
using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Gestionnaires;
using MeepleZone.Systeme.BaseTest;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Domaines.Compte.Test.Gestionnaires;

public class GestionnaireRequeteRegenererCodeAmitieTest : BaseTest<GestionnaireRequeteRegenererCodeAmitie>
{
    private const string PremierCode = "AB3K9XQ2";

    private const string SecondCode = "CD4M8YR3";

    private Fake<IDepotCompte> _depotCompte = null!;

    private Fake<IGenerateurCodeAmitie> _generateurCodeAmitie = null!;

    private Fake<IUtilisateurCourant> _utilisateurCourant = null!;

    public override void InitialiserDependence()
    {
        base.InitialiserDependence();
        _depotCompte = CreerFausse<IDepotCompte>();
        _generateurCodeAmitie = CreerFausse<IGenerateurCodeAmitie>();
        _utilisateurCourant = CreerFausse<IUtilisateurCourant>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(Guid.CreateVersion7());
    }

    [Fact]
    public async Task TraiterAsync_CodeLibre_EnregistrerPourLUtilisateurCourantEtRetournerLeCode()
    {
        // Arrange
        var utilisateurId = CreerFixture<Guid>();
        _utilisateurCourant.CallsTo(x => x.Id).Returns(utilisateurId);
        _generateurCodeAmitie.CallsTo(x => x.GenererAsync()).Returns(PremierCode);
        _depotCompte.CallsTo(x => x.ModifierCodeAmitieAsync(utilisateurId, PremierCode)).Returns(ResultatModificationCodeAmitie.Modifie);

        // Act
        var result = await InstanceTest.TraiterAsync(new RegenererCodeAmitieRequete());

        // Assert
        result.Should().Be(new RegenererCodeAmitieReponse(StatutRegenerationCode.Regenere, PremierCode));
        _depotCompte.CallsTo(x => x.ModifierCodeAmitieAsync(A<Guid>._, A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TraiterAsync_CompteIntrouvable_RetournerSansCodeNiNouvelEssai()
    {
        // Arrange
        _generateurCodeAmitie.CallsTo(x => x.GenererAsync()).Returns(PremierCode);
        _depotCompte.CallsTo(x => x.ModifierCodeAmitieAsync(A<Guid>._, A<string>._)).Returns(ResultatModificationCodeAmitie.Introuvable);

        // Act
        var result = await InstanceTest.TraiterAsync(new RegenererCodeAmitieRequete());

        // Assert
        result.Should().Be(new RegenererCodeAmitieReponse(StatutRegenerationCode.CompteIntrouvable, null));
        _depotCompte.CallsTo(x => x.ModifierCodeAmitieAsync(A<Guid>._, A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TraiterAsync_PremierCodePrisEntreTemps_EnregistrerUnSecondCode()
    {
        // Arrange
        _generateurCodeAmitie.CallsTo(x => x.GenererAsync()).ReturnsNextFromSequence(PremierCode, SecondCode);
        _depotCompte.CallsTo(x => x.ModifierCodeAmitieAsync(A<Guid>._, PremierCode)).Returns(ResultatModificationCodeAmitie.CodeDejaUtilise);
        _depotCompte.CallsTo(x => x.ModifierCodeAmitieAsync(A<Guid>._, SecondCode)).Returns(ResultatModificationCodeAmitie.Modifie);

        // Act
        var result = await InstanceTest.TraiterAsync(new RegenererCodeAmitieRequete());

        // Assert
        result.Should().Be(new RegenererCodeAmitieReponse(StatutRegenerationCode.Regenere, SecondCode));
    }

    [Fact]
    public async Task TraiterAsync_DeuxCodesPris_RetournerConflitSansTroisiemeEssai()
    {
        // Arrange
        _generateurCodeAmitie.CallsTo(x => x.GenererAsync()).ReturnsNextFromSequence(PremierCode, SecondCode);
        _depotCompte.CallsTo(x => x.ModifierCodeAmitieAsync(A<Guid>._, A<string>._)).Returns(ResultatModificationCodeAmitie.CodeDejaUtilise);

        // Act
        var result = await InstanceTest.TraiterAsync(new RegenererCodeAmitieRequete());

        // Assert
        result.Should().Be(new RegenererCodeAmitieReponse(StatutRegenerationCode.Conflit, null));
        _depotCompte.CallsTo(x => x.ModifierCodeAmitieAsync(A<Guid>._, A<string>._)).MustHaveHappenedTwiceExactly();
    }
}

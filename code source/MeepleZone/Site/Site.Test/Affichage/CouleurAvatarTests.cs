using System;
using FluentAssertions;
using MeepleZone.Site.Affichage;
using Xunit;

namespace MeepleZone.Site.Test.Affichage;

public class CouleurAvatarTests
{
    // ToByteArray() range le premier groupe du texte en ordre inverse : son premier octet est la fin de ce groupe (0a = 10, ff = 255).
    [Theory]
    [InlineData("0000000a-0000-0000-0000-000000000000", 12, 10)]
    [InlineData("0000000a-0000-0000-0000-000000000000", 8, 2)]
    [InlineData("000000ff-0000-0000-0000-000000000000", 12, 3)]
    [InlineData("00000000-0000-0000-0000-000000000000", 12, 0)]
    public void Index_AvecGuidConnu_RetourneLeResteDuPremierOctet(string guid, int nombreCouleurs, int attendu)
    {
        // Act
        var index = CouleurAvatar.Index(Guid.Parse(guid), nombreCouleurs);

        // Assert
        index.Should().Be(attendu);
    }

    [Fact]
    public void Index_AvecLeMemeId_RetourneToujoursLeMemeIndex()
    {
        // Arrange
        var id = Guid.Parse("3f2b8c1e-5a47-4d0e-9b6a-1c8d7e2f4a90");

        // Act
        var premier = CouleurAvatar.Index(id, 12);
        var second = CouleurAvatar.Index(id, 12);

        // Assert
        second.Should().Be(premier);
    }

    [Fact]
    public void Index_AvecDesIdentifiantsVaries_RestePresDansLaPalette()
    {
        // Act & Assert
        for (var i = 0; i < 200; i++)
        {
            CouleurAvatar.Index(Guid.NewGuid(), 12).Should().BeInRange(0, 11);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Index_AvecNombreDeCouleursInvalide_Leve(int nombreCouleurs)
    {
        // Act
        var action = () => CouleurAvatar.Index(Guid.NewGuid(), nombreCouleurs);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}

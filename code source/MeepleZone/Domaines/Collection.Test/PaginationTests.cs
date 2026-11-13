using FluentAssertions;
using System;
using Xunit;

namespace MeepleZone.Domaines.Collection.Test;

public class PaginationTests
{
    [Theory]
    [InlineData(1, 24, 0)]
    [InlineData(2, 24, 24)]
    [InlineData(3, 10, 20)]
    [InlineData(Pagination.PageMaximale, Pagination.TaillePageMaximale, (Pagination.PageMaximale - 1) * Pagination.TaillePageMaximale)]
    public void CalculerSaut_ValeursValides_RetournerNombreElementsASauter(int page, int taillePage, int sautAttendu)
    {
        // Act
        var result = Pagination.CalculerSaut(page, taillePage);

        // Assert
        result.Should().Be(sautAttendu);
    }

    [Theory]
    [InlineData(0, 24)]
    [InlineData(-1, 24)]
    [InlineData(Pagination.PageMaximale + 1, 24)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    [InlineData(1, Pagination.TaillePageMaximale + 1)]
    public void CalculerSaut_ValeursHorsLimites_LeverArgumentOutOfRangeException(int page, int taillePage)
    {
        // Act
        var act = () => Pagination.CalculerSaut(page, taillePage);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

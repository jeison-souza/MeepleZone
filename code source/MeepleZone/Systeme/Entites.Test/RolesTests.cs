using FluentAssertions;
using MeepleZone.Systeme.Entites.Constantes;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class RolesTests
{
    // Les valeurs viennent de la contrainte CHECK de la colonne role (schema-donnees.md), pas du code testé.
    [Theory]
    [InlineData(Roles.Administrateur, true)]
    [InlineData(Roles.Systeme, true)]
    [InlineData(Roles.Membre, false)]
    [InlineData("inconnu", false)]
    [InlineData("", false)]
    public void EstProtege_SelonLeRole_RetournerLeResultatAttendu(string role, bool attendu)
    {
        // Act
        var result = Roles.EstProtege(role);

        // Assert
        result.Should().Be(attendu);
    }

    [Theory]
    [InlineData(Roles.Membre, "membre")]
    [InlineData(Roles.Administrateur, "administrateur")]
    [InlineData(Roles.Systeme, "systeme")]
    public void Constantes_EnToutTemps_CorrespondentAuxValeursDuSchema(string constante, string attendu)
    {
        constante.Should().Be(attendu);
    }
}

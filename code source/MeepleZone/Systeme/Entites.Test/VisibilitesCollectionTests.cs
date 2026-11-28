using FluentAssertions;
using MeepleZone.Systeme.Entites.Constantes;
using Xunit;

namespace MeepleZone.Systeme.Entites.Test;

public class VisibilitesCollectionTests
{
    // Les valeurs viennent de la contrainte CHECK de la colonne visibilite_collection (schema-donnees.md), pas du code testé.
    [Theory]
    [InlineData(VisibilitesCollection.Privee, "privee")]
    [InlineData(VisibilitesCollection.Amis, "amis")]
    public void Constantes_EnToutTemps_CorrespondentAuxValeursDuSchema(string constante, string attendu)
    {
        constante.Should().Be(attendu);
    }
}

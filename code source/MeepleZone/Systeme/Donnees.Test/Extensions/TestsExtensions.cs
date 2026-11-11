using System;
using Xunit;

namespace MeepleZone.Systeme.Donnees.Test.Extensions;

public class TestsExtensions
{
    public static TheoryData<string> NomsDesProprietes(Type typeEntite)
    {
        return NomsDesProprietesSauf(typeEntite);
    }

    // Seules les propriétés simples (texte, nombre, date, Guid) sont des colonnes :
    // une collection ou une propriété de navigation (relation) ne l'est pas.
    public static TheoryData<string> NomsDesProprietesSauf(Type typeEntite, params string[] exceptes)
    {
        var donnees = new TheoryData<string>();

        foreach (var propriete in typeEntite.GetProperties())
        {
            var estSimple = propriete.PropertyType.IsValueType || propriete.PropertyType == typeof(string);

            if (estSimple && !exceptes.Contains(propriete.Name))
            {
                donnees.Add(propriete.Name);
            }
        }

        return donnees;
    }
}

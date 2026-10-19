using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace MeepleZone.Systeme.Donnee.Test;

/// <summary>
/// Construit le contexte et son modèle une seule fois pour tous les tests d'une classe.
/// La connexion n'est jamais ouverte : le modèle se construit sans base de données.
/// Ces options devront venir d'un seul endroit quand la fabrique de la migration existera.
/// </summary>
public sealed class ModeleDonneesFixture : IDisposable
{
    public ModeleDonneesFixture()
    {
        var options = new DbContextOptionsBuilder<MeepleZoneContexte>()
            .UseNpgsql("Host=hote-inexistant")
            .UseSnakeCaseNamingConvention()
            .Options;

        Contexte = new MeepleZoneContexte(options);

        // Le modèle d'exécution (Contexte.Model) n'a pas la collation ni les contraintes CHECK :
        // seul le modèle de conception les contient, et c'est lui que la migration utilise.
        Modele = Contexte.GetService<IDesignTimeModel>().Model;
    }

    public MeepleZoneContexte Contexte { get; }

    public IModel Modele { get; }

    public IEntityType TypeEntite<T>()
    {
        return Modele.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} est absent du modèle.");
    }

    public IProperty Propriete<T>(string nom)
    {
        return TypeEntite<T>().FindProperty(nom)
            ?? throw new InvalidOperationException($"{typeof(T).Name}.{nom} est absent du modèle.");
    }

    public void Dispose()
    {
        Contexte.Dispose();
    }
}

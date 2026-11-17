using MeepleZone.Domaines.Compte.Contrats.Modeles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Site.Authentification.Liaison;

/// <summary>Implémentation provisoire : deux connexions du même fournisseur, toujours les mêmes. À retirer avec le branchement au domaine.</summary>
public sealed class FournisseursLiesFictif : IFournisseursLies
{
    public Task<IReadOnlyList<ConnexionDto>> ListerAsync()
    {
        IReadOnlyList<ConnexionDto> liste =
        [
            new ConnexionDto("Google", "personne@exemple.ca", DateTimeOffset.UtcNow.AddDays(-30)),
            new ConnexionDto("Google", null, DateTimeOffset.UtcNow),
        ];

        return Task.FromResult(liste);
    }
}

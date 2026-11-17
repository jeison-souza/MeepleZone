using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using System.Threading.Tasks;

namespace MeepleZone.Site.Authentification.Liaison;

/// <summary>Implémentation provisoire : la liaison réussit toujours, sans rien enregistrer. À retirer avec le branchement au domaine.</summary>
public sealed class LiaisonFournisseurFictive : ILiaisonFournisseur
{
    public Task<LierFournisseurReponse> LierAsync(LierFournisseurRequete requete)
    {
        return Task.FromResult(new LierFournisseurReponse(StatutLiaison.Liee));
    }
}

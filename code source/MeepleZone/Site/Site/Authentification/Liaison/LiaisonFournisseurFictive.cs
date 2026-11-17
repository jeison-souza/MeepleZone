using System.Threading.Tasks;

namespace MeepleZone.Site.Authentification.Liaison;

/// <summary>Implémentation provisoire : la liaison réussit toujours, sans rien enregistrer. À retirer avec le branchement au domaine.</summary>
public sealed class LiaisonFournisseurFictive : ILiaisonFournisseur
{
    public Task<ResultatLiaison> LierAsync(string fournisseur, string identifiantCompteExterne, string? courriel)
    {
        return Task.FromResult(ResultatLiaison.Reussie);
    }
}

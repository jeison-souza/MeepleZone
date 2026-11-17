using MeepleZone.Domaines.Compte.Contrats.Modeles;
using System.Threading.Tasks;

namespace MeepleZone.Site.Authentification.Liaison;

/// <summary>
/// Associe un compte d'un fournisseur de connexion à l'utilisateur courant. Sera remplacée par
/// <c>LierFournisseurAsync</c> du domaine Compte (voir <see cref="LiaisonFournisseurFictive"/>).
/// </summary>
public interface ILiaisonFournisseur
{
    Task<LierFournisseurReponse> LierAsync(LierFournisseurRequete requete);
}

using System.Threading.Tasks;

namespace MeepleZone.Site.Authentification.Liaison;

/// <summary>
/// Associe un compte d'un fournisseur de connexion à l'utilisateur courant. Sera remplacée par l'opération
/// <c>LierFournisseur</c> du domaine Compte quand elle existera (voir <see cref="LiaisonFournisseurFictive"/>).
/// </summary>
public interface ILiaisonFournisseur
{
    Task<ResultatLiaison> LierAsync(string fournisseur, string identifiantCompteExterne, string? courriel);
}

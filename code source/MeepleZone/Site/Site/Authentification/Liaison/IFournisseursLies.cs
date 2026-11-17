using MeepleZone.Domaines.Compte.Contrats.Modeles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Site.Authentification.Liaison;

/// <summary>
/// Connexions liées au compte courant, pour la section « Fournisseurs associés » du profil. Un même fournisseur
/// peut apparaître plusieurs fois. Sera remplacée par <c>ListerConnexionsAsync</c> du domaine Compte
/// (voir <see cref="FournisseursLiesFictif"/>).
/// </summary>
public interface IFournisseursLies
{
    Task<IReadOnlyList<ConnexionDto>> ListerAsync();
}

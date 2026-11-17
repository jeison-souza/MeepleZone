using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Site.Authentification.Liaison;

/// <summary>
/// Source des fournisseurs liés au compte courant, pour la section « Fournisseurs associés » du profil.
/// Sera remplacée par l'opération du domaine Compte quand elle existera (voir <see cref="FournisseursLiesFictif"/>).
/// </summary>
public interface IFournisseursLies
{
    Task<IReadOnlyList<FournisseurLie>> ListerAsync();
}

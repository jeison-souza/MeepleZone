using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.Entites;

namespace MeepleZone.Domaines.Compte.Extensions;

public static class UtilisateurExtensions
{
    extension(Utilisateur utilisateur)
    {
        public UtilisateurConnecteDto VersDto()
        {
            return new UtilisateurConnecteDto(utilisateur.Id, utilisateur.NomAffichage, utilisateur.Courriel, utilisateur.Role);
        }
    }
}

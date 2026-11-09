using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Depots;

public interface IDepotCompte
{
    Task<IdentiteExterne?> TrouverIdentiteAsync(string fournisseur, string identifiant);

    Task<Utilisateur?> TrouverUtilisateurAsync(Guid identifiant);

    Task<bool> VerifierExistenceCourrielAsync(string courriel);

    Task<bool> VerifierExistenceCodeAmitieAsync(string code);

    Task<ResultatEnregistrementCompte> EnregistrerNouveauCompteAsync(Utilisateur utilisateur, IdentiteExterne identite);

    Task MettreAJourConnexionAsync(IdentiteExterne identite);
}

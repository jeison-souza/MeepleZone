using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Depots;

public interface IDepotCompte
{
    Task<ResultatEnregistrementCompte> EnregistrerNouveauCompteAsync(Utilisateur utilisateur, IdentiteExterne identite);

    Task<IReadOnlyList<IdentiteExterne>> ListerIdentitesAsync(Guid utilisateurId);

    Task MettreAJourConnexionAsync(IdentiteExterne identite);

    Task<IdentiteExterne?> TrouverIdentiteAsync(string fournisseur, string identifiant);

    Task<Utilisateur?> TrouverUtilisateurAsync(Guid identifiant);

    Task<bool> VerifierExistenceCodeAmitieAsync(string code);

    Task<bool> VerifierExistenceCourrielAsync(string courriel);
}

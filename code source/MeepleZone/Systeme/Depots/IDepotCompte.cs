using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Depots;

public interface IDepotCompte
{
    Task<ResultatAjoutIdentite> AjouterIdentiteAsync(IdentiteExterne identite);

    Task<ResultatEnregistrementCompte> EnregistrerNouveauCompteAsync(Utilisateur utilisateur, IdentiteExterne identite);

    Task<IReadOnlyList<IdentiteExterne>> ListerIdentitesAsync(Guid utilisateurId);

    Task MettreAJourConnexionAsync(IdentiteExterne identite);

    Task<ResultatModificationCodeAmitie> ModifierCodeAmitieAsync(Guid utilisateurId, string code);

    /// <summary>
    /// Enregistre le courriel du compte et efface la date de sa vérification.
    /// </summary>
    Task<ResultatModificationCourriel> ModifierCourrielAsync(Guid utilisateurId, string courriel);

    Task<ResultatModificationVisibilite> ModifierVisibiliteAsync(Guid utilisateurId, string visibilite);

    Task<ResultatSuppressionCompte> SupprimerCompteAsync(Guid utilisateurId, Guid compteSupprimeId);

    Task<IdentiteExterne?> TrouverIdentiteAsync(string fournisseur, string identifiant);

    Task<Utilisateur?> TrouverUtilisateurAsync(Guid identifiant);

    Task<bool> VerifierExistenceCodeAmitieAsync(string code);

    Task<PresenceCourriel> VerifierPresenceCourrielAsync(string courriel);
}

using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites.Constantes;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte.Gestionnaires;

public class GestionnaireRequeteSupprimerCompte(IDepotCompte depotCompte, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<SupprimerCompteReponse, SupprimerCompteRequete>
{
    private readonly IDepotCompte _depotCompte = depotCompte;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<SupprimerCompteReponse> TraiterAsync(SupprimerCompteRequete requete)
    {
        var utilisateurId = _utilisateurCourant.Id;
        var utilisateur = await _depotCompte.TrouverUtilisateurAsync(utilisateurId);

        if (utilisateur == null)
        {
            return new SupprimerCompteReponse(StatutSuppressionCompte.Introuvable);
        }

        if (Roles.EstProtege(utilisateur.Role))
        {
            return new SupprimerCompteReponse(StatutSuppressionCompte.CompteProtege);
        }

        var resultat = await _depotCompte.SupprimerCompteAsync(utilisateurId, ComptesTechniques.UtilisateurSupprimeId);

        return new SupprimerCompteReponse(resultat switch
        {
            ResultatSuppressionCompte.Supprime => StatutSuppressionCompte.Supprime,
            ResultatSuppressionCompte.CompteProtege => StatutSuppressionCompte.CompteProtege,
            ResultatSuppressionCompte.Introuvable => StatutSuppressionCompte.Introuvable,
            ResultatSuppressionCompte.Conflit => StatutSuppressionCompte.Conflit,
            _ => throw new InvalidOperationException($"Résultat de suppression inattendu : {resultat}."),
        });
    }
}

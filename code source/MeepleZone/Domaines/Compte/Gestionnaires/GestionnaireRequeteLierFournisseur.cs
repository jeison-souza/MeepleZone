using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte.Gestionnaires;

public class GestionnaireRequeteLierFournisseur(IDepotCompte depotCompte, IUtilisateurCourant utilisateurCourant, TimeProvider timeProvider)
    : IGestionnaireRequete<LierFournisseurReponse, LierFournisseurRequete>
{
    private readonly IDepotCompte _depotCompte = depotCompte;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<LierFournisseurReponse> TraiterAsync(LierFournisseurRequete requete)
    {
        ArgumentNullException.ThrowIfNull(requete);
        ArgumentException.ThrowIfNullOrWhiteSpace(requete.Fournisseur);
        ArgumentException.ThrowIfNullOrWhiteSpace(requete.IdentifiantCompteExterne);

        var utilisateurId = _utilisateurCourant.Id;

        var statutExistant = await DeterminerStatutIdentiteExistanteAsync(requete, utilisateurId);
        if (statutExistant != null)
        {
            return new LierFournisseurReponse(statutExistant.Value);
        }

        var dateCreation = _timeProvider.GetUtcNow();
        var identiteExterne = new IdentiteExterne()
        {
            Courriel = NormaliserCourriel(requete.Courriel),
            DerniereConnexionLe = dateCreation,
            Fournisseur = requete.Fournisseur,
            Id = Guid.CreateVersion7(dateCreation),
            IdentifiantCompteExterne = requete.IdentifiantCompteExterne,
            UtilisateurId = utilisateurId,
        };

        var resultat = await _depotCompte.AjouterIdentiteAsync(identiteExterne);
        switch (resultat)
        {
            case ResultatAjoutIdentite.Ajoutee:
                return new LierFournisseurReponse(StatutLiaison.Liee);

            case ResultatAjoutIdentite.IdentiteDejaExistante:
                // Une autre connexion a lié la même identité entre la recherche et l'ajout : reprendre la décision avec ce qui existe maintenant.
                var statutParallele = await DeterminerStatutIdentiteExistanteAsync(requete, utilisateurId);
                if (statutParallele == null)
                {
                    throw new InvalidOperationException("Impossible de retrouver l'identité externe ajoutée en parallèle.");
                }

                return new LierFournisseurReponse(statutParallele.Value);

            default:
                throw new InvalidOperationException($"Résultat d'ajout inattendu : {resultat}.");
        }
    }

    private async Task<StatutLiaison?> DeterminerStatutIdentiteExistanteAsync(LierFournisseurRequete requete, Guid utilisateurId)
    {
        var identiteExistante = await _depotCompte.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne);
        if (identiteExistante == null)
        {
            return null;
        }

        return identiteExistante.UtilisateurId == utilisateurId
            ? StatutLiaison.DejaLieeAuMemeCompte
            : StatutLiaison.DejaUtiliseeParAutreCompte;
    }

    private static string? NormaliserCourriel(string? courriel)
    {
        if (string.IsNullOrWhiteSpace(courriel))
        {
            return null;
        }

        return courriel.Trim().ToLowerInvariant();
    }
}

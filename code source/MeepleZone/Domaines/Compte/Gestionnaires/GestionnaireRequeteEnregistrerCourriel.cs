using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte.Gestionnaires;

public class GestionnaireRequeteEnregistrerCourriel(IDepotCompte depotCompte, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<EnregistrerCourrielReponse, EnregistrerCourrielRequete>
{
    private const int LongueurMaximale = 254;

    private readonly IDepotCompte _depotCompte = depotCompte;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<EnregistrerCourrielReponse> TraiterAsync(EnregistrerCourrielRequete requete)
    {
        ArgumentNullException.ThrowIfNull(requete);

        var courriel = (requete.Courriel ?? string.Empty).Trim().ToLowerInvariant();
        if (!EstUnCourriel(courriel))
        {
            return new EnregistrerCourrielReponse(StatutEnregistrementCourriel.SaisieInvalide);
        }

        var utilisateur = await _depotCompte.TrouverUtilisateurAsync(_utilisateurCourant.Id);
        if (utilisateur == null)
        {
            return new EnregistrerCourrielReponse(StatutEnregistrementCourriel.CompteIntrouvable);
        }

        // Le même courriel ne change rien : sa vérification est conservée (ADR 0016).
        if (string.Equals(utilisateur.Courriel, courriel, StringComparison.OrdinalIgnoreCase))
        {
            return new EnregistrerCourrielReponse(StatutEnregistrementCourriel.Inchange);
        }

        var presence = await _depotCompte.VerifierPresenceCourrielAsync(courriel);
        if (presence == PresenceCourriel.Verifiee)
        {
            return new EnregistrerCourrielReponse(StatutEnregistrementCourriel.CourrielDejaUtilise);
        }

        var resultat = await _depotCompte.ModifierCourrielAsync(utilisateur.Id, courriel);

        return resultat switch
        {
            ResultatModificationCourriel.Modifie => new EnregistrerCourrielReponse(StatutEnregistrementCourriel.Enregistre),
            ResultatModificationCourriel.Introuvable => new EnregistrerCourrielReponse(StatutEnregistrementCourriel.CompteIntrouvable),
            _ => throw new InvalidOperationException($"Résultat de modification inattendu : {resultat}."),
        };
    }

    /// <summary>
    /// Une seule adresse, sans nom ni espaces, avec un domaine qui comporte un point.
    /// La preuve que l'adresse existe relève de la vérification (ADR 0017).
    /// </summary>
    private static bool EstUnCourriel(string courriel)
    {
        if (courriel.Length > LongueurMaximale || !MailAddress.TryCreate(courriel, out var adresse))
        {
            return false;
        }

        var domaine = adresse.Host;
        return adresse.Address == courriel
            && domaine.Contains('.')
            && !domaine.StartsWith('.')
            && !domaine.EndsWith('.');
    }
}

using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte.Gestionnaires;

public class GestionnaireRequeteGererConnexion(IDepotCompte depotCompte, IGenerateurCodeAmitie generateurCodeAmitie, TimeProvider timeProvider)
    : IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete>
{
    private readonly IDepotCompte _depotCompte = depotCompte;

    private readonly IGenerateurCodeAmitie _generateurCodeAmitie = generateurCodeAmitie;

    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<GererConnexionReponse> TraiterAsync(GererConnexionRequete requete)
    {
        var courrielNormalise = NormaliserTexte(requete.Courriel)?.ToLowerInvariant();
        var reponseExistante = await TenterConnexionExistanteAsync(requete, courrielNormalise);
        if (reponseExistante != null)
        {
            return reponseExistante;
        }

        string? courrielUtilisateur = null;
        if (courrielNormalise != null)
        {
            var courrielExiste = await _depotCompte.VerifierExistenceCourrielAsync(courrielNormalise);
            if (!courrielExiste)
            {
                courrielUtilisateur = courrielNormalise;
            }
            else if (!requete.ForcerCreation)
            {
                return new GererConnexionReponse(StatutConnexion.CompteProbable, null);
            }
        }

        var compte = await ConstruireCompteAsync(requete, courrielUtilisateur, courrielNormalise);
        var resultat = await _depotCompte.EnregistrerNouveauCompteAsync(compte.Utilisateur, compte.Identite);

        // Un courriel pris entre la vérification et l'enregistrement : l'utilisateur a déjà demandé la création, un seul nouvel essai sans courriel.
        if (resultat == ResultatEnregistrementCompte.CourrielDejaUtilise && requete.ForcerCreation && courrielUtilisateur != null)
        {
            compte = await ConstruireCompteAsync(requete, null, courrielNormalise);
            resultat = await _depotCompte.EnregistrerNouveauCompteAsync(compte.Utilisateur, compte.Identite);
        }

        switch (resultat)
        {
            case ResultatEnregistrementCompte.Enregistre:
                return new GererConnexionReponse(StatutConnexion.Connecte, compte.Utilisateur.VersDto());

            case ResultatEnregistrementCompte.IdentiteDejaExistante:
                // Une autre connexion a créé la même identité entre-temps : reprendre le chemin du compte connu.
                var reponseParallele = await TenterConnexionExistanteAsync(requete, courrielNormalise);
                if (reponseParallele == null)
                {
                    throw new InvalidOperationException("Impossible de retrouver la connexion enregistrée en parallèle.");
                }

                return reponseParallele;

            case ResultatEnregistrementCompte.CourrielDejaUtilise when !requete.ForcerCreation:
                return new GererConnexionReponse(StatutConnexion.CompteProbable, null);

            default:
                throw new InvalidOperationException($"Résultat d'enregistrement inattendu : {resultat}.");
        }
    }

    private async Task<GererConnexionReponse?> TenterConnexionExistanteAsync(GererConnexionRequete requete, string? courriel)
    {
        var identiteExterne = await _depotCompte.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne);
        if (identiteExterne == null)
        {
            return null;
        }

        return await ActualiserConnexionEtPreparerReponse(courriel, identiteExterne);
    }

    private async Task<(Utilisateur Utilisateur, IdentiteExterne Identite)> ConstruireCompteAsync(GererConnexionRequete requete, string? courrielUtilisateur, string? courrielIdentite)
    {
        var code = await _generateurCodeAmitie.GenererAsync();
        var nomNormalise = NormaliserTexte(requete.NomAffichage);
        var nomAffichage = nomNormalise ?? $"Joueur {code}";
        var dateCreation = _timeProvider.GetUtcNow();

        var utilisateur = new Utilisateur
        {
            Id = Guid.CreateVersion7(dateCreation),
            Role = "membre",
            VisibiliteCollection = "privee",
            NomAffichage = nomAffichage,
            CodeAmitie = code,
            Courriel = courrielUtilisateur,
        };

        var identiteExterne = new IdentiteExterne()
        {
            Courriel = courrielIdentite,
            DerniereConnexionLe = dateCreation,
            Fournisseur = requete.Fournisseur,
            Id = Guid.CreateVersion7(dateCreation),
            IdentifiantCompteExterne = requete.IdentifiantCompteExterne,
            UtilisateurId = utilisateur.Id,
        };

        return (utilisateur, identiteExterne);
    }

    private async Task<GererConnexionReponse> ActualiserConnexionEtPreparerReponse(string? courriel, IdentiteExterne identiteExterne)
    {
        var utilisateur = await _depotCompte.TrouverUtilisateurAsync(identiteExterne.UtilisateurId);
        if (utilisateur == null)
        {
            throw new InvalidOperationException("Impossible de trouver l'utilisateur lié au compte.");
        }

        if (courriel != null)
        {
            identiteExterne.Courriel = courriel;
        }

        identiteExterne.DerniereConnexionLe = _timeProvider.GetUtcNow();
        await _depotCompte.MettreAJourConnexionAsync(identiteExterne);
        var reponse = new GererConnexionReponse(StatutConnexion.Connecte, utilisateur.VersDto());
        return reponse;
    }

    private static string? NormaliserTexte(string? texte)
    {
        if (string.IsNullOrWhiteSpace(texte))
        {
            return null;
        }

        texte = texte.Trim();
        return texte;
    }
}

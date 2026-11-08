using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Extensions;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces.Depots;
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
        var identiteExterne = await _depotCompte.TrouverIdentiteAsync(requete.Fournisseur, requete.IdentifiantCompteExterne);
        if (identiteExterne != null)
        {
            return await ActualiserConnexionEtPreparerReponse(courrielNormalise, identiteExterne);
        }

        if (courrielNormalise == null)
        {
            return await CreerUtilisateurEtPreparerReponse(null, courrielNormalise, requete);
        }

        var courrielExiste = await _depotCompte.VerifierExistenceCourrielAsync(courrielNormalise);
        if (courrielExiste == false)
        {
            return await CreerUtilisateurEtPreparerReponse(courrielNormalise, courrielNormalise, requete);
        }

        if (requete.ForcerCreation)
        {
            return await CreerUtilisateurEtPreparerReponse(null, courrielNormalise, requete);
        }

        return new GererConnexionReponse(StatutConnexion.CompteProbable, null);
    }

    private async Task<GererConnexionReponse> CreerUtilisateurEtPreparerReponse(string? courrielUtilisateur, string? courrielIdentite, GererConnexionRequete requete)
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

        await _depotCompte.EnregistrerNouveauCompteAsync(utilisateur, identiteExterne);
        var reponse = new GererConnexionReponse(StatutConnexion.Connecte, utilisateur.VersDto());
        return reponse;
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

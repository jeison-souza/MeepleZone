using MeepleZone.Domaines.Collection.Contrats.Enums;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection.Gestionnaires;

public class GestionnaireRequeteCreerJeu(IDepotCollection depotCollection, IUtilisateurCourant utilisateurCourant, TimeProvider timeProvider)
    : IGestionnaireRequete<CreerJeuReponse, CreerJeuRequete>
{
    private const string StatutEnAttente = "en_attente";

    private readonly IDepotCollection _depotCollection = depotCollection;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<CreerJeuReponse> TraiterAsync(CreerJeuRequete requete)
    {
        var nom = NormaliserTexte(requete.Nom);
        var editeur = NormaliserTexte(requete.Editeur);
        if (!ValidationJeu.EstValide(requete, nom, editeur))
        {
            return new CreerJeuReponse(StatutCreationJeu.DonneesInvalides, null);
        }

        var utilisateurId = _utilisateurCourant.Id;
        var maintenant = _timeProvider.GetUtcNow();

        var jeu = new Jeu
        {
            Id = Guid.CreateVersion7(maintenant),
            Nom = nom!,
            AnneePublication = requete.AnneePublication,
            JoueursMin = requete.JoueursMin,
            JoueursMax = requete.JoueursMax,
            JoueursMeilleur = requete.JoueursMeilleur,
            DureeMinMinutes = requete.DureeMinMinutes,
            DureeMaxMinutes = requete.DureeMaxMinutes,
            AgeMin = requete.AgeMin,
            Complexite = requete.Complexite,
            ModeJeuId = requete.ModeJeuId,
            Editeur = editeur,
            Statut = StatutEnAttente,
            CreeParId = utilisateurId,
        };

        // Le jeu n'est pas rattaché à la possession avant l'enregistrement : le contexte tenterait de l'insérer deux fois.
        var possession = new Possession
        {
            Id = Guid.CreateVersion7(maintenant),
            UtilisateurId = utilisateurId,
            JeuId = jeu.Id,
            PartiesJouees = 0,
        };

        var mecaniqueIds = requete.MecaniqueIds.Distinct().ToList();
        var resultat = await _depotCollection.CreerJeuAsync(jeu, mecaniqueIds, possession);

        switch (resultat)
        {
            case ResultatCreationJeu.Cree:
                possession.Jeu = jeu;
                return new CreerJeuReponse(StatutCreationJeu.Cree, possession.VersDto());

            case ResultatCreationJeu.ReferenceInvalide:
                return new CreerJeuReponse(StatutCreationJeu.DonneesInvalides, null);

            default:
                throw new InvalidOperationException($"Résultat de création inattendu : {resultat}.");
        }
    }

    private static string? NormaliserTexte(string? texte)
    {
        if (string.IsNullOrWhiteSpace(texte))
        {
            return null;
        }

        return texte.Trim();
    }
}

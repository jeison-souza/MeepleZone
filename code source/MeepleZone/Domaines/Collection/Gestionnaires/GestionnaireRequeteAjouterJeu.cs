using MeepleZone.Domaines.Collection.Contrats.Enums;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection.Gestionnaires;

public class GestionnaireRequeteAjouterJeu(IDepotCollection depotCollection, IUtilisateurCourant utilisateurCourant, TimeProvider timeProvider)
    : IGestionnaireRequete<AjouterJeuReponse, AjouterJeuRequete>
{
    private readonly IDepotCollection _depotCollection = depotCollection;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<AjouterJeuReponse> TraiterAsync(AjouterJeuRequete requete)
    {
        var utilisateurId = _utilisateurCourant.Id;
        var jeu = await _depotCollection.TrouverJeuVisibleAsync(utilisateurId, requete.JeuId);
        if (jeu == null)
        {
            return new AjouterJeuReponse(StatutAjout.JeuIntrouvable, null);
        }

        // Le jeu n'est pas rattaché à la possession avant l'enregistrement : le contexte tenterait de l'insérer.
        var possession = new Possession
        {
            Id = Guid.CreateVersion7(_timeProvider.GetUtcNow()),
            UtilisateurId = utilisateurId,
            JeuId = jeu.Id,
            PartiesJouees = 0,
        };

        var resultat = await _depotCollection.AjouterPossessionAsync(possession);

        switch (resultat)
        {
            case ResultatAjoutPossession.Ajoutee:
                possession.Jeu = jeu;
                return new AjouterJeuReponse(StatutAjout.Ajoute, possession.VersDto());

            case ResultatAjoutPossession.DejaPossedee:
                return new AjouterJeuReponse(StatutAjout.DejaPossede, null);

            case ResultatAjoutPossession.JeuInexistant:
                return new AjouterJeuReponse(StatutAjout.JeuIntrouvable, null);

            default:
                throw new InvalidOperationException($"Résultat d'ajout inattendu : {resultat}.");
        }
    }
}

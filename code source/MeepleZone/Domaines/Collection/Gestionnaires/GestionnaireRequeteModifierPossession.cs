using MeepleZone.Domaines.Collection.Contrats.Enums;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection.Gestionnaires;

public class GestionnaireRequeteModifierPossession(IDepotCollection depotCollection, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<ModifierPossessionReponse, ModifierPossessionRequete>
{
    private readonly IDepotCollection _depotCollection = depotCollection;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<ModifierPossessionReponse> TraiterAsync(ModifierPossessionRequete requete)
    {
        var commentaire = NormaliserTexte(requete.Commentaire);
        if (!ValiderRequete(requete, commentaire))
        {
            return new ModifierPossessionReponse(StatutModification.DonneesInvalides, null);
        }

        var possession = await _depotCollection.ModifierPossessionAsync(_utilisateurCourant.Id, requete.PossessionId, requete.Note, commentaire, requete.PartiesJouees);

        return new ModifierPossessionReponse(possession == null ? StatutModification.Introuvable : StatutModification.Modifie, possession?.VersDto());
    }

    private static string? NormaliserTexte(string? texte)
    {
        if (string.IsNullOrWhiteSpace(texte))
        {
            return null;
        }

        return texte.Trim();
    }

    private static bool ValiderRequete(ModifierPossessionRequete requete, string? commentaire)
    {
        if (requete.PartiesJouees < 0)
        {
            return false;
        }

        if (requete.Note.HasValue && (requete.Note < ValidationPossession.NoteMin || requete.Note > ValidationPossession.NoteMax))
        {
            return false;
        }

        if (commentaire != null && commentaire.Length > ValidationPossession.CommentaireLongueurMax)
        {
            return false;
        }

        return true;
    }
}

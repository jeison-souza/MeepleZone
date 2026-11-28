using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte.Gestionnaires;

public class GestionnaireRequeteModifierVisibilite(IDepotCompte depotCompte, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<ModifierVisibiliteReponse, ModifierVisibiliteRequete>
{
    private readonly IDepotCompte _depotCompte = depotCompte;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<ModifierVisibiliteReponse> TraiterAsync(ModifierVisibiliteRequete requete)
    {
        ArgumentNullException.ThrowIfNull(requete);

        var resultat = await _depotCompte.ModifierVisibiliteAsync(_utilisateurCourant.Id, requete.Visibilite.VersValeur());

        return resultat switch
        {
            ResultatModificationVisibilite.Modifiee => new ModifierVisibiliteReponse(StatutModificationVisibilite.Modifiee),
            ResultatModificationVisibilite.Introuvable => new ModifierVisibiliteReponse(StatutModificationVisibilite.CompteIntrouvable),
            _ => throw new InvalidOperationException($"Résultat de modification inattendu : {resultat}."),
        };
    }
}

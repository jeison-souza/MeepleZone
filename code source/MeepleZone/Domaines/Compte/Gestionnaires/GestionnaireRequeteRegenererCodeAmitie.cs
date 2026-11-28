using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte.Gestionnaires;

public class GestionnaireRequeteRegenererCodeAmitie(IDepotCompte depotCompte, IGenerateurCodeAmitie generateurCodeAmitie, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<RegenererCodeAmitieReponse, RegenererCodeAmitieRequete>
{
    // Un code pris entre la génération et l'enregistrement : un seul nouvel essai.
    private const int EssaisMax = 2;

    private readonly IDepotCompte _depotCompte = depotCompte;

    private readonly IGenerateurCodeAmitie _generateurCodeAmitie = generateurCodeAmitie;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<RegenererCodeAmitieReponse> TraiterAsync(RegenererCodeAmitieRequete requete)
    {
        ArgumentNullException.ThrowIfNull(requete);

        var utilisateurId = _utilisateurCourant.Id;

        for (var essai = 0; essai < EssaisMax; essai++)
        {
            var code = await _generateurCodeAmitie.GenererAsync();
            var resultat = await _depotCompte.ModifierCodeAmitieAsync(utilisateurId, code);

            switch (resultat)
            {
                case ResultatModificationCodeAmitie.Modifie:
                    return new RegenererCodeAmitieReponse(StatutRegenerationCode.Regenere, code);

                case ResultatModificationCodeAmitie.Introuvable:
                    return new RegenererCodeAmitieReponse(StatutRegenerationCode.CompteIntrouvable, null);

                case ResultatModificationCodeAmitie.CodeDejaUtilise:
                    break;

                default:
                    throw new InvalidOperationException($"Résultat de modification inattendu : {resultat}.");
            }
        }

        return new RegenererCodeAmitieReponse(StatutRegenerationCode.Conflit, null);
    }
}

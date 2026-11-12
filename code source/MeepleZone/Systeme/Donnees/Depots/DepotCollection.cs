using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Modeles;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Donnees.Depots;

public class DepotCollection(IDbContextFactory<MeepleZoneContexte> fabrique) : IDepotCollection
{
    private const string StatutValide = "valide";

    private const string StatutEnAttente = "en_attente";

    private readonly IDbContextFactory<MeepleZoneContexte> _fabrique = fabrique;

    public async Task<PageDeResultats<Possession>> ListerPossessionsAsync(Guid utilisateurId, string? recherche, int saut, int prendre)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        var possessions = contexte.Possessions
            .AsNoTracking()
            .Include(p => p.Jeu)
            .Where(p => p.UtilisateurId == utilisateurId);

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            possessions = possessions.Where(p => EF.Functions.Unaccent(p.Jeu!.Nom).ToLower().Contains(EF.Functions.Unaccent(recherche).ToLower()));
        }

        var total = await possessions.CountAsync();

        var items = await possessions
            .OrderBy(p => p.Jeu!.Nom)
            .ThenBy(p => p.Id)
            .Skip(saut)
            .Take(prendre)
            .ToListAsync();

        return new PageDeResultats<Possession>(items, total);
    }

    public async Task<PageDeResultats<ResultatRechercheJeu>> RechercherJeuxAsync(Guid utilisateurId, string texte, int saut, int prendre)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        var jeux = contexte.Jeux
            .AsNoTracking()
            .Where(j => j.Statut == StatutValide || (j.Statut == StatutEnAttente && j.CreeParId == utilisateurId))
            .Where(j => EF.Functions.Unaccent(j.Nom).ToLower().Contains(EF.Functions.Unaccent(texte).ToLower()));

        var total = await jeux.CountAsync();

        var items = await jeux
            .OrderBy(j => j.Nom)
            .ThenBy(j => j.Id)
            .Skip(saut)
            .Take(prendre)
            .Select(j => new
            {
                Jeu = j,
                DejaPossede = contexte.Possessions.Any(p => p.UtilisateurId == utilisateurId && p.JeuId == j.Id)
            })
            .ToListAsync();

        return new PageDeResultats<ResultatRechercheJeu>([.. items.Select(x => new ResultatRechercheJeu(x.Jeu, x.DejaPossede))], total);
    }
}

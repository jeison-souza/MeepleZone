using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Donnees.Depots;

public class DepotCompte(IDbContextFactory<MeepleZoneContexte> fabrique) : IDepotCompte
{
    private const string IndexIdentiteExterneUnique = "ix_identite_externe_fournisseur_identifiant_compte_externe";

    private const string IndexUtilisateurCourriel = "ix_utilisateur_courriel";

    private readonly IDbContextFactory<MeepleZoneContexte> _fabrique = fabrique;

    public async Task<ResultatEnregistrementCompte> EnregistrerNouveauCompteAsync(Utilisateur utilisateur, IdentiteExterne identite)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        try
        {
            contexte.Utilisateurs.Add(utilisateur);
            contexte.IdentitesExternes.Add(identite);
            await contexte.SaveChangesAsync();
            return ResultatEnregistrementCompte.Enregistre;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } violation)
        {
            switch (violation.ConstraintName)
            {
                case IndexIdentiteExterneUnique:
                    return ResultatEnregistrementCompte.IdentiteDejaExistante;

                case IndexUtilisateurCourriel:
                    return ResultatEnregistrementCompte.CourrielDejaUtilise;

                default:
                    throw;
            }
        }
    }

    public async Task MettreAJourConnexionAsync(IdentiteExterne identite)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        contexte.IdentitesExternes.Update(identite);
        await contexte.SaveChangesAsync();
    }

    public async Task<IdentiteExterne?> TrouverIdentiteAsync(string fournisseur, string identifiant)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        return await contexte.IdentitesExternes
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Fournisseur == fournisseur &&
                                      i.IdentifiantCompteExterne == identifiant);
    }

    public async Task<Utilisateur?> TrouverUtilisateurAsync(Guid identifiant)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        return await contexte.Utilisateurs
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == identifiant);
    }

    public async Task<bool> VerifierExistenceCodeAmitieAsync(string code)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        return await contexte.Utilisateurs
            .AsNoTracking()
            .AnyAsync(u => u.CodeAmitie == code);
    }

    public async Task<bool> VerifierExistenceCourrielAsync(string courriel)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        if (await contexte.Utilisateurs.AsNoTracking().AnyAsync(u => u.Courriel == courriel))
        {
            return true;
        }

        return await contexte.IdentitesExternes.AsNoTracking().AnyAsync(i => i.Courriel == courriel);
    }
}

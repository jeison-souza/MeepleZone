using MeepleZone.Systeme.Depots;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte;

public class GenerateurCodeAmitie(IDepotCompte depotCompte) : IGenerateurCodeAmitie
{
    private readonly IDepotCompte _depotCompte = depotCompte;
    private const string ChaineCaracteres = "ABCDEFGH" + "JKLMN" + "PQRSTUVWXYZ" + "23456789";
    private const int LongueurCode = 8;
    private const int TentativesMax = 5;

    public async Task<string> GenererAsync()
    {
        for (var tentatives = 0; tentatives < TentativesMax; tentatives++)
        {
            var code = RandomNumberGenerator.GetString(ChaineCaracteres, LongueurCode);
            var existe = await _depotCompte.VerifierExistenceCodeAmitieAsync(code);
            if (!existe)
            {
                return code;
            }
        }

        throw new InvalidOperationException($"Impossible de générer un code d'amitié unique après {TentativesMax} tentatives.");
    }
}

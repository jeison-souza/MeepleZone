using System.Threading.Tasks;

namespace MeepleZone.Systeme.Interfaces.Gestionnaires;

public interface IGestionnaireRequete<TRetour, TRequete>
{
    Task<TRetour> TraiterAsync(TRequete requete);
}

using System.Threading.Tasks;

namespace MeepleZone.Systeme.Interfaces.Gestionnaires;

public interface IGestionnaireCommande<TCommande>
{
    Task TraiterAsync(TCommande commande);
}

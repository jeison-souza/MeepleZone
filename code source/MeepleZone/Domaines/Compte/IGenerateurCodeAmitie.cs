using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte;

public interface IGenerateurCodeAmitie
{
    Task<string> GenererAsync();
}

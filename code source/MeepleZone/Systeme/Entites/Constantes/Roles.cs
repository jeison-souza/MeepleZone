namespace MeepleZone.Systeme.Entites.Constantes;

public static class Roles
{
    public const string Membre = "membre";

    public const string Administrateur = "administrateur";

    public const string Systeme = "systeme";

    public static bool EstProtege(string role)
    {
        return role == Administrateur || role == Systeme;
    }
}

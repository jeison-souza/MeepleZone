namespace MeepleZone.Systeme.Depots.Enums;

/// <summary>
/// Où un courriel est déjà connu : <c>Verifiee</c> quand un compte l'a vérifié, <c>NonVerifiee</c> quand il
/// ne figure que dans des connexions externes. Un courriel non vérifié d'un compte ne compte pas.
/// </summary>
public enum PresenceCourriel
{
    Aucune,
    NonVerifiee,
    Verifiee,
}

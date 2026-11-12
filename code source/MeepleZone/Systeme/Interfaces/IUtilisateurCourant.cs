using System;

namespace MeepleZone.Systeme.Interfaces;

public interface IUtilisateurCourant
{
    /// <summary>
    /// Identifiant de l'utilisateur authentifié. Lève une <see cref="InvalidOperationException"/> s'il n'y en a aucun.
    /// </summary>
    Guid Id { get; }
}

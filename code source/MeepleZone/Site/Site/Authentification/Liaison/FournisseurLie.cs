using System;

namespace MeepleZone.Site.Authentification.Liaison;

/// <summary>
/// Une connexion déjà liée au compte courant. Un même fournisseur peut apparaître plusieurs fois
/// (deux comptes Google, par exemple).
/// </summary>
public sealed record FournisseurLie(string Nom, string? Courriel, DateTimeOffset DateLiaison);

namespace MeepleZone.Site.Authentification;

/// <summary>
/// Un fournisseur de connexion offert par le site. <paramref name="Nom"/> est le nom du schéma d'authentification
/// (et la fin de l'adresse <c>/connexion/{Nom}</c>); <paramref name="Libelle"/> est le texte affiché.
/// </summary>
public sealed record FournisseurConnexion(string Nom, string Libelle);

# ADR 0012 : Identité de la session et extraction des revendications du fournisseur

- **Statut :** accepté, amendé le 2026-11-12 (personne connectée dans les domaines)
- **Date :** 2026-11-10

## Contexte

Les utilisateurs se connectent uniquement par Google ou Microsoft ([ADR 0001](0001-authentification.md)). Une fois la connexion terminée, le site crée un cookie d'authentification. Il fallait décider de ce que ce cookie contient, du moment où le compte de la base de données entre en jeu, et de la façon de lire les informations que chaque fournisseur communique.

Contraintes :

- L'identifiant d'un utilisateur dans l'application est le GUID interne ([ADR 0010](0010-identifiants-et-horodatages.md)), et non l'identifiant du fournisseur. Celui-ci ne sert qu'à retrouver la connexion externe ([ADR 0004](0004-liaison-comptes-fournisseurs.md)).
- Le courriel est facultatif ([ADR 0004](0004-liaison-comptes-fournisseurs.md), règle 5) et ne sert jamais à identifier la personne (règle 3).
- Le domaine `Compte` ne doit pas connaître ASP.NET Core ni les revendications (`claims`) : il reçoit une requête et retourne une réponse ([ADR 0009](0009-organisation-de-la-solution.md)).
- Une sonde des revendications, faite avec de vrais comptes, a montré que Google et Microsoft donnent les mêmes types de revendications une fois traduits par ASP.NET Core, mais pas de la même façon : chez Microsoft, le traitement par défaut de l'intergiciel remplit la revendication de courriel avec le champ `userPrincipalName` quand le champ `mail` est absent. Or le `userPrincipalName` est un nom de connexion, pas une adresse de courriel au sens de l'ADR 0004.

## Options considérées

### Contenu du cookie et moment de la création du compte

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Garder les revendications du fournisseur dans le cookie; créer le compte plus tard (première page protégée) | Aucun changement au mécanisme de l'intergiciel | Le cookie porte l'identifiant du fournisseur, pas celui de l'application; chaque page doit chercher le compte à partir de l'identifiant externe; l'enregistrement du compte serait éparpillé hors de la connexion |
| B. Garder les revendications du fournisseur; chercher l'utilisateur dans la base à chaque requête | Le nom et le rôle sont toujours à jour | Un aller-retour à la base par requête; une base indisponible bloque aussi les pages publiques; l'interface Blazor Server interroge l'état plusieurs fois par page |
| C. Traiter la connexion dans l'événement `OnCreatingTicket`, appeler le domaine, et remplacer l'identité par une identité construite à partir du compte de la base | Le compte existe dès que la personne est connectée; le cookie porte l'identifiant interne; le domaine décide (création, compte probable) avant que le cookie existe | Le nom et le rôle du cookie restent figés jusqu'à la connexion suivante; l'événement de l'intergiciel porte la logique de connexion |

### Courriel de Microsoft

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Laisser le gestionnaire par défaut et accepter le `userPrincipalName` comme courriel | Aucune configuration | Un nom de connexion est enregistré comme courriel; il peut être une adresse que la personne n'utilise pas, ou une adresse d'organisation; l'index d'unicité du courriel et la détection de compte probable comparent alors des valeurs qui ne sont pas des courriels |
| B. Ne lire que le champ `mail` : vider les actions de revendications du gestionnaire et les remapper (`id`, `displayName`, `mail`) | Un compte sans `mail` naît sans courriel, ce que l'ADR 0004 prévoit; l'identifiant est celui du service Microsoft, comme l'ADR 0004 le décrit | Une configuration propre à Microsoft à maintenir; les actions par défaut ajoutées dans une version future ne s'appliquent plus |
| C. Demander une étendue (`scope`) de plus ou appeler un autre point d'accès pour retrouver le courriel | Plus de chances d'obtenir une adresse | Une dépendance de plus; une demande de consentement plus large pour la personne, sans besoin établi |

### Compte probable

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Exception levée dans `OnCreatingTicket`, interceptée par `OnRemoteFailure` | L'intergiciel arrête proprement la création du ticket et le cookie n'est jamais écrit; le chemin de redirection existe déjà pour les échecs | Une exception sert à signaler un cas attendu; il faut la distinguer des vraies erreurs |
| B. Écrire un cookie temporaire et rediriger vers l'écran de choix | Pas d'exception | Un cookie d'identité partielle existe, qu'il faut ensuite supprimer; le risque d'accès à une zone protégée avec une identité incomplète |

## Décision

- **Contenu du cookie : option C.** La logique de connexion s'exécute dans l'événement `OnCreatingTicket` des deux fournisseurs. Elle extrait la requête des revendications du fournisseur, appelle le domaine `Compte` (une seule opération, `GererConnexion`) et, si la personne est connectée, remplace l'identité par celle du compte de la base. Le cookie contient seulement : l'identifiant interne (type `NameIdentifier`), le nom d'affichage, le rôle et, s'il existe, le courriel. Une revendication de courriel vide n'est pas écrite. Les revendications du fournisseur n'entrent pas dans le cookie.
- **Une seule extraction pour les deux fournisseurs.** Une classe lit l'identifiant, le courriel et le nom sur les types de revendications d'ASP.NET Core ; l'identifiant est obligatoire (son absence lève une exception) ; les textes vides ou faits d'espaces deviennent absents ; le nettoyage (retirer les espaces, mettre le courriel en minuscules) appartient au gestionnaire du domaine, non à l'extracteur.
- **Microsoft : option B.** Les actions de revendications par défaut sont retirées et trois sont déclarées : `id` pour l'identifiant, `displayName` pour le nom et `mail` pour le courriel. Le `userPrincipalName` n'est jamais utilisé comme courriel. Un compte Microsoft sans `mail` donne un compte sans courriel.
- **Compte probable : option A.** Quand le domaine répond « compte probable », une exception sans données (`CompteProbableException`) est levée; le traitement des échecs la reconnaît, ne la consigne pas comme une erreur et redirige vers `/connexion-requise?motif=compte-probable`. L'adresse ne contient aucune donnée de la personne : seulement le motif, le fournisseur (pris dans la liste fermée de l'application) et une page de retour revalidée.
- **Marqueur de création forcée.** L'écran de choix permet de créer un nouveau compte; le défi de connexion reprend alors avec un marqueur dans `AuthenticationProperties.Items` (clé `ForcerCreation`, valeur exacte `true`; toute autre valeur est ignorée). `OnCreatingTicket` le lit et le transmet au domaine. Le marqueur n'est pas un contrôle de sécurité : l'authentification reste chez le fournisseur, et le forçage ne contourne que la détection de compte probable ([ADR 0004](0004-liaison-comptes-fournisseurs.md), règle 1).
- **Affichage.** Le menu et le profil lisent seulement le cookie. La photo n'est pas conservée : l'interface dessine les initiales du nom.
- **Personne connectée dans les domaines (amendement du 2026-11-12).** Un domaine qui agit pour la personne connectée (la collection) ne reçoit jamais son identifiant dans les requêtes : il dépend de l'interface `IUtilisateurCourant` (`Systeme/Interfaces`), qui expose le GUID interne (`Id`) et lève une exception quand personne n'est authentifié. Le site l'implémente à partir de la revendication `NameIdentifier` du cookie, avec une durée de vie par étendue. Une requête ne peut donc pas viser une autre personne, et le domaine ne connaît ni ASP.NET Core ni les revendications. Une possession qui n'appartient pas à la personne est traitée comme introuvable, sans révéler qu'elle existe.

## Conséquences

### Positives

- Aucune lecture de la base pour connaître la personne connectée : tout ce que l'interface affiche vient du cookie.
- Le compte existe dès la fin de la connexion; aucune page n'a à le créer ni à traiter un compte absent.
- Le cookie porte l'identifiant interne, ce qui évite de mélanger identifiant du fournisseur et identifiant de l'application.
- Aucun courriel ne provient d'un nom de connexion : l'unicité et la détection de compte probable comparent de vrais courriels.
- Le domaine `Compte` ne connaît pas ASP.NET Core; l'extraction, la construction de l'identité et le branchement sont des classes du site, testées séparément (le gestionnaire sans base de données, l'extraction et la configuration Microsoft sur des données de la forme du service).
- Un seul chemin de connexion pour les deux fournisseurs; un troisième fournisseur ne demande que son enregistrement et, au besoin, sa configuration de revendications.
- Aucune requête de collection ne porte d'identifiant de personne : un client ne peut pas agir au nom d'une autre personne, et les tests du domaine remplacent la personne connectée par un faux `IUtilisateurCourant`.

### Négatives

- Le nom et le rôle du cookie ne changent pas avant la connexion suivante : un changement de rôle ou de nom n'est pas pris en compte pendant la durée de vie du cookie. Si le besoin se présente (rôle d'administration, changement de nom), il faudra relire le compte ou invalider le cookie.
- Le travail du domaine et de la base de données se fait pendant le retour du fournisseur : la connexion est plus lente (surtout la première, à cause de la première connexion à la base), et une base indisponible fait échouer la connexion.
- Une exception sert à signaler un cas attendu (compte probable). Elle est limitée à ce cas et traitée à un seul endroit.
- `IUtilisateurCourant` échoue hors d'un contexte authentifié : les opérations de la collection ne s'appellent que depuis des pages protégées. L'identifiant vient du cookie, donc un compte supprimé n'est vu qu'à la connexion suivante, comme le nom et le rôle.
- La configuration Microsoft remplace les actions par défaut : une revendication ajoutée par une version future de la bibliothèque ne sera pas lue sans intervention.
- Le `userPrincipalName` ignoré peut être la seule adresse connue d'un compte d'organisation : la personne se retrouve alors sans courriel de compte, et doit en ajouter un depuis son profil pour recevoir des communications ([ADR 0004](0004-liaison-comptes-fournisseurs.md), règle 5).
- L'abandon du forçage en cours de route n'est pas géré : si la personne ferme la fenêtre après avoir choisi de créer un compte, elle revient simplement à l'écran de choix à la connexion suivante.

## Décisions liées

- [ADR 0001](0001-authentification.md) : connexion par fournisseurs externes seulement.
- [ADR 0004](0004-liaison-comptes-fournisseurs.md) : règles 1, 3 et 5 (compte probable, courriel jamais une identité, courriel facultatif).
- [ADR 0009](0009-organisation-de-la-solution.md) : le site appelle seulement l'interface d'entrée du domaine.
- [ADR 0010](0010-identifiants-et-horodatages.md) : GUID interne de version 7.
- [ADR 0011](0011-concurrence-et-contrats-de-depot.md) : concurrence à l'enregistrement du compte.

## Sources

- [Authentification externe dans ASP.NET Core (Microsoft)](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/)
- [Cookies d'authentification dans ASP.NET Core (Microsoft)](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie)

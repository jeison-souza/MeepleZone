# ADR 0014 : Navigation entre entités et modèles de lecture des dépôts

- **Statut :** accepté
- **Date :** 2026-11-12

## Contexte

Les entités de `Systeme/Entites` portent des données et rien d'autre : pas d'attributs d'EF Core, pas de règles, et jusqu'ici aucune propriété qui désigne une autre entité (`IdentiteExterne` ne porte que `UtilisateurId`, sans propriété `Utilisateur`). Les relations existent entièrement dans le modèle d'EF Core (clé étrangère, comportement à la suppression), et les requêtes qui lisent deux tables passent par un `Join` explicite dans le dépôt. Les raisons de ce choix initial étaient : des entités sans lien entre elles (les données de personnes sortent par des objets de transfert), pas de récursion dans la génération de données de test, et rien d'utile à charger.

La collection d'une personne est le premier cas qui lit deux entités liées : afficher une possession demande aussi le jeu qu'elle désigne (nom, durée, couverture). La recherche de jeux demande en plus une valeur qui n'est pas une relation : « cette personne possède déjà ce jeu ». Il fallait décider comment le dépôt retourne ces lectures, sachant que :

- le dépôt vit dans `Systeme/Depots`, sous le domaine, et ne peut pas connaître les objets de transfert des contrats ([ADR 0009](0009-organisation-de-la-solution.md), règles 6 et 7, amendées par l'[ADR 0011](0011-concurrence-et-contrats-de-depot.md)) ;
- le contexte de données est de courte durée ([ADR 0013](0013-contexte-de-donnees-court-et-enregistrement-des-services.md)) : une entité retournée est détachée, et rien n'est chargé après coup ;
- la logique d'affaires doit se tester sans base de données ([ADR 0008](0008-logique-d-affaires-testable.md)), avec des entités fabriquées automatiquement.

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Aucune navigation; un type d'association par requête (`PossessionAvecJeu`, `JeuAvecModeJeu`, etc.) | Entités simples; rien n'est jamais « non chargé » | Le nombre de types croît avec chaque requête à deux entités; chacun ne fait que juxtaposer deux entités |
| B. Navigation dans les deux sens, avec collections (`Jeu.Possessions`, `Possession.Jeu`) | Modèle naturel; `Include` partout | Boucles dans la génération de données de test; une collection chargée par mégarde ramène beaucoup de lignes; chaque entité connaît ses dépendantes |
| C. Navigation seulement vers le « parent » (plusieurs à un), dans un seul sens, sans collection; modèles de lecture nommés d'après le cas d'usage pour les valeurs calculées | Supprime les types d'association; pas de boucle; le dépôt choisit ce qu'il charge | Une propriété peut valoir `null` quand le dépôt ne l'a pas incluse; il faut le documenter |
| D. Le dépôt retourne directement les objets de transfert du domaine | Aucun type intermédiaire | Le dépôt dépendrait des contrats du domaine, ce que l'ADR 0009 interdit |

## Décision

- **Option C.** Une entité reçoit une propriété de navigation vers l'entité parente quand un cas d'usage en a besoin, dans un seul sens et sans collection. La première est `Possession.Jeu` (nullable), configurée par `HasOne(p => p.Jeu).WithMany()` : le nom de la contrainte et le comportement à la suppression ne changent pas, et la base de données n'est pas modifiée. Les autres entités restent sans navigation tant qu'aucun cas d'usage n'en demande une.
- **Valeur « non chargée » explicite.** La propriété vaut `null` quand le dépôt ne l'a pas incluse (`Include`). Le contrat du dépôt dit ce qui est chargé (« avec leur jeu chargé »), et la conversion en objet de transfert lève une exception si la propriété est absente, au lieu de retourner des données vides.
- **Valeurs calculées : modèles de lecture.** Ce qui n'est pas une relation (par exemple l'indicateur de possession de la recherche) est retourné par un type de `Systeme/Depots/Modeles`, nommé d'après le cas d'usage (`ResultatRechercheJeu`) et non d'après sa composition. Les pages (éléments et total) partagent un type générique, `PageDeResultats<T>`.
- **Écritures.** Elles ne changent pas : une possession se crée à partir de `JeuId`. L'entité transmise au dépôt pour un enregistrement ne doit pas avoir sa navigation remplie, sinon le contexte tente d'insérer aussi le jeu ; le gestionnaire la remplit après l'enregistrement, pour construire sa réponse.

## Conséquences

### Positives

- Plus de types d'association à écrire pour chaque requête à deux entités ; les dépôts retournent des entités.
- Pas de boucle pour la génération de données de test : la navigation va dans un seul sens, et un test qui veut le cas « non chargé » le construit en retirant la propriété.
- Le dépôt reste indépendant du domaine : il retourne des entités et des modèles de lecture, et le gestionnaire les convertit.
- Le schéma de la base ne change pas : seul le modèle d'EF Core connaît la navigation.

### Négatives

- Une propriété qui peut être nulle selon l'appelant est un piège : l'oublier donne une exception à l'exécution, non une erreur de compilation. La convention (documenter le contrat du dépôt, convertir en échouant) la rend visible, sans l'éliminer.
- `Include` charge toutes les colonnes de l'entité liée, plus qu'une projection n'en demanderait. Sans conséquence aux volumes visés ; si cela change, le dépôt peut projeter vers un modèle de lecture sans modifier son contrat.
- Les entités ne sont plus entièrement sans lien : la règle initiale est amendée, et chaque nouvelle navigation reste à justifier par un cas d'usage.
- Un modèle de lecture de plus par requête qui calcule une valeur : leur nombre croît avec les écrans, ce qui est le coût de nommer le cas d'usage.

## Décisions liées

- [ADR 0008](0008-logique-d-affaires-testable.md) : logique d'affaires testable sans base de données.
- [ADR 0009](0009-organisation-de-la-solution.md) : règles 6 et 7 (dépôts et sens des dépendances).
- [ADR 0011](0011-concurrence-et-contrats-de-depot.md) : emplacement des contrats de dépôt.
- [ADR 0013](0013-contexte-de-donnees-court-et-enregistrement-des-services.md) : contexte de données de courte durée.

## Sources

- [Chargement des données liées dans EF Core (Microsoft)](https://learn.microsoft.com/en-us/ef/core/querying/related-data/)

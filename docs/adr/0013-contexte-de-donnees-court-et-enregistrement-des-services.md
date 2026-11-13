# ADR 0013 : Contexte de données de courte durée et enregistrement des services par balayage

- **Statut :** accepté, amendé le 2026-11-12 (suppression par `ExecuteDelete`, domaine Collection)
- **Date :** 2026-11-10

## Contexte

Les dépôts de `Systeme/Donnees` ([ADR 0009](0009-organisation-de-la-solution.md), règle 6) accèdent à la base par un `DbContext` d'EF Core. Il fallait décider de la durée de vie de ce contexte et de la façon dont les services (dépôts, façades, gestionnaires) sont enregistrés auprès du conteneur d'injection de dépendances.

Contraintes :

- Le site est une application Blazor en mode interactif serveur : l'étendue (`scope`) d'un composant dure autant que la connexion de la personne (un « circuit »), pas autant qu'une requête HTTP. Un `DbContext` n'est pas prévu pour être partagé par plusieurs opérations simultanées ni pour vivre longtemps : il garde en mémoire les entités qu'il suit, et ces données deviennent périmées.
- Le gestionnaire de connexion ([ADR 0012](0012-identite-dans-la-session.md)) s'exécute dans l'événement `OnCreatingTicket`, pendant le retour du fournisseur, hors de l'interface.
- Le champ `modifie_le` est mis à jour par un intercepteur de `SaveChanges` qui utilise un `TimeProvider` ([ADR 0010](0010-identifiants-et-horodatages.md)). Une écriture qui ne passe pas par `SaveChanges` échappe à cet intercepteur.
- La logique d'affaires doit se tester sans base de données ([ADR 0008](0008-logique-d-affaires-testable.md)).
- Le projet comptera plusieurs domaines (comptes, jeux, collection, amitiés), chacun avec ses gestionnaires, sa façade et son dépôt, tous suivant les mêmes conventions de nommage et d'emplacement.

## Options considérées

### Durée de vie du contexte de données

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Un contexte par étendue (`AddDbContext`), injecté dans les dépôts | Configuration la plus courante; suivi des entités partagé dans l'étendue | Avec Blazor, l'étendue dure toute la connexion : le contexte vieillit, accumule des entités et des données périmées; deux opérations simultanées dans le même circuit peuvent le partager, ce qu'il ne supporte pas |
| B. Fabrique de contextes (`IDbContextFactory`), un contexte créé et libéré à l'intérieur de chaque méthode du dépôt | Le contexte ne vit que le temps d'une opération; aucun partage possible; fonctionne de la même façon dans le site, dans l'événement de connexion et dans un outil de console | Aucun suivi partagé entre deux appels du dépôt (une entité lue puis modifiée doit être rattachée de nouveau); création d'un contexte à chaque appel (peu coûteuse, le modèle étant mis en cache) |
| C. Un contexte unique pour toute l'application | Simple à écrire | Incompatible avec l'utilisation simultanée; données périmées; à écarter |

### Enregistrement des services

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Enregistrement manuel, une ligne par service | Explicite; rien n'est enregistré par inadvertance | Chaque nouvelle classe demande une ligne; un oubli ne se voit qu'à l'exécution |
| B. Balayage de l'assemblage (bibliothèque Scrutor), limité à l'espace de noms voulu | Un nouveau gestionnaire ou dépôt est enregistré sans intervention, selon la convention d'emplacement; une seule méthode d'extension par projet | Une classe publique dans l'espace de noms est enregistrée même si elle ne devrait pas l'être; dépendance supplémentaire; le résultat est moins visible à la lecture |
| C. Balayage de toute l'assemblage | Aucun filtre à écrire | Enregistrerait aussi le contexte de données, les configurations et l'intercepteur, qui ont leur propre enregistrement |

## Décision

- **Contexte de données : option B.** Les dépôts reçoivent un `IDbContextFactory<MeepleZoneContexte>`. Chaque méthode crée son contexte, l'utilise et le libère (`await using`). Les lectures utilisent `AsNoTracking`. L'enregistrement d'un compte se fait dans un seul contexte et un seul appel à `SaveChanges` (voir [ADR 0011](0011-concurrence-et-contrats-de-depot.md)). Les mises à jour passent par une entité suivie par le contexte de l'appel (lue dans ce même contexte) ou par `Update`, suivi de `SaveChanges`, jamais par `ExecuteUpdate`, qui ne passe pas par l'intercepteur de `modifie_le`. **Amendement du 2026-11-12 :** `ExecuteDelete` est permis pour supprimer, car une suppression n'a aucune date à mettre à jour; il évite de lire la ligne avant de l'effacer (retrait d'un jeu de la collection, filtré par identifiant et par utilisateur).
- **Intercepteur et horloge.** La fabrique est configurée avec la surcharge qui reçoit le fournisseur de services, pour créer l'intercepteur avec le `TimeProvider` du conteneur. Le `TimeProvider` réel est enregistré seulement s'il n'y en a pas déjà un (`TryAddSingleton`), ce qui laisse les tests en imposer un.
- **Enregistrement : option B.** Chaque projet expose une méthode d'extension (`AjouterSystemeDonnees(chaineConnexion)`, `AjouterDomaineCompte()`, `AjouterDomaineCollection()`), appelée au démarrage du site. Elle balaie l'assemblage avec Scrutor, restreint à l'espace de noms d'une classe repère (`DepotCompte` pour les dépôts, `DomaineCompte` pour le domaine, sous-espaces de noms compris), associe chaque classe à ses interfaces et les enregistre avec une durée de vie par étendue. Le filtre exclut le contexte, ses configurations et l'intercepteur, qui s'enregistrent autrement.
- **Test de l'enregistrement.** Un test du conteneur résout la façade du domaine en n'enregistrant que le `TimeProvider` et un faux dépôt : il ne doit enregistrer ni gestionnaire ni générateur factice, qui cacheraient un balayage défaillant.

## Conséquences

### Positives

- Pas de contexte périmé ni partagé : chaque opération voit l'état courant de la base, et deux opérations simultanées d'un même circuit ne se gênent pas.
- Le même dépôt sert dans l'interface, dans l'événement de connexion et dans les outils, sans configuration d'étendue particulière.
- L'intercepteur de `modifie_le` couvre toutes les écritures, puisqu'elles passent toutes par `SaveChanges`.
- Ajouter un gestionnaire ou un dépôt ne demande aucune modification de l'enregistrement : il suffit de le placer dans le bon espace de noms.
- Le balayage devance un besoin inévitable : les domaines à venir (jeux, collection, amitiés) auront chacun leurs gestionnaires et leur dépôt. La convention est en place avant leur arrivée, même si un enregistrement manuel aurait suffi pour le seul gestionnaire actuel.
- La méthode d'extension reste l'unique point où le site connaît les adaptateurs ([ADR 0009](0009-organisation-de-la-solution.md), règle 7).

### Négatives

- Aucun suivi partagé entre deux appels : une entité lue par un appel et modifiée par un autre est rattachée de nouveau (`Update`), ce qui écrit toutes ses colonnes. Acceptable pour des entités de petite taille; à reconsidérer si une opération doit lire puis modifier plusieurs entités de façon atomique (elle devra alors vivre dans un seul appel du dépôt, ce que la règle 4 de l'ADR 0009 favorise déjà).
- Un contexte est créé à chaque appel du dépôt. Le coût est faible, mais une opération qui enchaîne plusieurs appels ouvre plusieurs connexions successives au regroupement de connexions.
- Le balayage enregistre toute classe publique de l'espace de noms visé : une classe utilitaire placée là par erreur devient un service. La convention d'emplacement est donc une règle à respecter, et le test du conteneur à conserver.
- Scrutor est une dépendance de plus, utilisée dans deux projets.
- Le lien entre une interface et son implémentation n'apparaît plus à un seul endroit du code : il faut connaître la convention pour le retrouver.

## Décisions liées

- [ADR 0008](0008-logique-d-affaires-testable.md) : logique d'affaires testable sans base de données.
- [ADR 0009](0009-organisation-de-la-solution.md) : règles 6 et 7 (dépôts, sens des dépendances).
- [ADR 0010](0010-identifiants-et-horodatages.md) : `TimeProvider` et intercepteur de `modifie_le`.
- [ADR 0011](0011-concurrence-et-contrats-de-depot.md) : un seul enregistrement par opération, avec traduction des violations d'unicité.
- [ADR 0012](0012-identite-dans-la-session.md) : le gestionnaire de connexion s'exécute dans l'événement `OnCreatingTicket`.

## Sources

- [Utiliser EF Core avec Blazor (Microsoft)](https://learn.microsoft.com/en-us/aspnet/core/blazor/blazor-ef-core)
- [Durée de vie d'un DbContext (Microsoft)](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [Scrutor (dépôt du projet)](https://github.com/khellang/Scrutor)

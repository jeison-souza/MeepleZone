# ADR 0008 : Logique d'affaires dans le code, testable sans base de données

- **Statut :** accepté
- **Date :** 2026-10-15

## Contexte

MeepleZone est une application .NET qui utilise EF Core et PostgreSQL ([ADR 0005](0005-base-de-donnees.md)). Deux questions se posent ensemble : où vivent les règles d'affaires (limite de cinq demandes d'amitié en attente, validation d'un jeu créé à la main, suppression d'un compte, etc.), et comment le code accède aux données.

Le projet fait l'objet d'une démarche de développement piloté par les tests, mais il est maintenu par une seule personne et les tests contre une vraie base de données alourdissent l'outillage et le temps d'exécution. Une règle d'affaires doit donc pouvoir se tester vite, sans base.

Le schéma impose déjà des contraintes dans la base (`NOT NULL`, `CHECK`, `UNIQUE`, clés étrangères). Il faut préciser leur rôle par rapport au code, ainsi que celui des déclencheurs et des procédures stockées.

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. EF Core utilisé directement dans la logique d'affaires (le `DbContext` est injecté partout) | Peu de code; direct | La logique ne se teste qu'avec une base réelle ou avec un substitut dont le comportement diffère de la base de production; couplage fort à EF Core |
| B. Interfaces de dépôt par domaine, implémentées avec EF Core dans un projet de données; la logique ne connaît que les interfaces | La logique se teste avec des dépôts factices; le domaine ne dépend pas d'EF Core; un seul endroit pour les requêtes d'un domaine | Plus de code qu'en A; les dépôts eux-mêmes ne sont pas testés sans base; un dépôt peut grossir |
| C. Infrastructure de médiation (médiateur, décorateurs) qui envoie chaque requête ou commande à son gestionnaire | Très modulaire; permet d'envelopper chaque opération (cache, journalisation) | Infrastructure supplémentaire sans besoin actuel; lourd pour un projet à une personne |
| D. Règles d'affaires dans la base (déclencheurs, procédures stockées) | Appliquées quelle que soit l'origine de la modification | Logique cachée, plus difficile à lire, à déboguer et à tester; EF Core ne crée pas de déclencheurs et les migrations cesseraient d'être entièrement générées |

## Décision

**Option B**, avec les règles suivantes :

- Les règles d'affaires vivent **dans le code du domaine**, qui ne référence pas EF Core. Le code accède aux données par des interfaces de dépôt, une par domaine (comptes, jeux, collection, amitiés), avec les méthodes dont ce domaine a besoin. Les implémentations, qui utilisent EF Core, sont dans un projet de données.
- **Pas de dépôt générique**, pas de médiateur ni de décorateurs tant qu'un besoin réel ne les justifie (YAGNI). Les opérations d'un domaine sont portées par des gestionnaires de commande et de requête, un par opération ([ADR 0009](0009-organisation-de-la-solution.md)). Une opération qui touche un autre domaine (accepter une demande d'amitié, supprimer un compte) passe par le dépôt de son propre domaine, sans que les domaines se référencent entre eux.
- **Pas de déclencheurs ni de procédures stockées.** La date de modification (`modifie_le`) est mise à jour par le code, à l'aide d'un intercepteur d'EF Core et d'une horloge injectable.
- **Deux niveaux de validation.** Les contraintes de la base sont la dernière protection (erreurs de code, modifications manuelles). Toute règle qui a un sens pour le métier est d'abord exprimée dans le code, avec un test automatique. Les clés étrangères et l'unicité ne sont garanties que par la base; le code transforme leur violation en message clair.
- **Tests automatisés :** ils portent sur la logique d'affaires, avec des dépôts factices et sans base réelle.

## Conséquences

### Positives

- Les règles se lisent et se testent rapidement, sans base ni conteneur.
- Le domaine dépend d'abstractions et non d'EF Core : l'accès aux données peut changer sans toucher aux règles.
- Les migrations restent entièrement générées par l'outil ([ADR 0007](0007-strategie-de-migrations.md)).
- La structure reste simple pour une personne seule, et peut s'enrichir (décorateurs, séparation des requêtes et des commandes) si un besoin réel apparaît.

### Négatives

- Les dépôts, qui contiennent les requêtes EF Core, ne sont pas testés automatiquement. Il faut les garder minces et sans logique d'affaires. La documentation d'EF Core recommande de tester contre le vrai système de base de données : cette recommandation est écartée pour la logique, mais des tests contre une base réelle, par exemple dans la chaîne d'intégration continue, pourront couvrir les dépôts plus tard.
- Les contraintes de la base et les règles du code doivent rester cohérentes : une règle écrite aux deux endroits peut diverger. Un test sur le modèle d'EF Core (ses métadonnées, sans connexion) pourra comparer les deux.
- Une modification faite à la main en SQL, hors de l'application, ne met pas `modifie_le` à jour et contourne les règles du code. Ce champ ne sert pas à l'audit.
- Un dépôt peut grossir : au-delà d'une dizaine de méthodes environ, il se divise.
- Il y a deux horloges : `cree_le` vient de la base et `modifie_le` de l'application. L'écart est négligeable pour l'usage prévu.

## Sources

- [Choisir une stratégie de test avec EF Core (Microsoft)](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy)
- [Intercepteurs d'EF Core (Microsoft)](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)

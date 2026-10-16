# ADR 0009 : Organisation de la solution .NET en projets par domaine

- **Statut :** accepté
- **Date :** 2026-10-15

## Contexte

La solution compte aujourd'hui un projet d'interface (`MeepleZone`, Blazor Web App), un projet WebAssembly et un projet de tests. L'implémentation de la base de données et de la logique d'affaires ([ADR 0005](0005-base-de-donnees.md), [ADR 0007](0007-strategie-de-migrations.md), [ADR 0008](0008-logique-d-affaires-testable.md)) oblige à décider où chaque partie du code vivra avant d'écrire la première classe.

Une première proposition (14 octobre 2026) prévoyait un seul projet `Domaine`, avec un dossier par domaine (comptes, jeux, collection, amitiés). Elle a été revue le 15 octobre 2026, après l'examen d'une structure plus fine, qui donne au compilateur le rôle de faire respecter la séparation entre les domaines.

Contraintes : projet maintenu par une seule personne, logique d'affaires testable sans base de données (ADR 0008), aucune infrastructure ni aucun projet créé avant le besoin (YAGNI).

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Un seul projet `Domaine`, un dossier par domaine, un projet de tests | Peu de projets; démarrage simple | La séparation entre domaines repose sur la discipline : rien n'empêche un domaine d'utiliser les classes d'un autre; les tests de tous les domaines se mélangent |
| B. Un projet par domaine, chacun avec son projet de tests; un regroupement `Systeme` pour le transversal et les adaptateurs vers l'extérieur | Le compilateur interdit les références entre domaines; chaque test vit à côté de ce qu'il vérifie; structure répétable; les projets se créent un à un, au besoin | Davantage de projets (la plupart ne sont créés qu'au moment du besoin); des règles de placement à respecter (voir la décision) |
| C. Couches classiques (présentation, affaires, données) sans découpage par domaine | Connu; peu de projets | Les domaines se mélangent dans chaque couche; le projet d'affaires grossit sans frontière interne |

## Décision

**Option B.** L'arborescence cible est la suivante. Un projet n'est créé qu'au moment où l'on commence à travailler dessus.

```text
MeepleZone/                         (racine de la solution; MeepleZone.slnx)
├─ Directory.Build.props, Directory.Packages.props
├─ Site/
│  ├─ Site/                         interface (Blazor Web App)
│  ├─ Site.Test/                    (seulement si le besoin se présente)
│  ├─ Site.Client/                  WebAssembly
│  └─ Site.Client.Test/             (seulement si le besoin se présente)
├─ Domaines/
│  ├─ Compte/    ├─ Compte/   └─ Compte.Test/
│  ├─ Jeux/      ├─ Jeux/     └─ Jeux.Test/
│  ├─ Collection/├─ Collection/ └─ Collection.Test/
│  └─ Amitie/    ├─ Amitie/   └─ Amitie.Test/
└─ Systeme/
   ├─ Entites/                      entités de données, partagées
   ├─ Interfaces/                   interfaces partagées (dépôts, gestionnaires)
   ├─ Contrats/                     contrats partagés par plusieurs domaines (au moment du besoin)
   ├─ TestBase/                     classe de base des tests (projet de tests seulement)
   ├─ Noyau/                        éléments transversaux de base
   ├─ Donnee/    ├─ Donnee/  (EF Core)   └─ Donnee.Test/ (seulement si le besoin se présente)
   ├─ Bgg/  Images/  Courriel/      adaptateurs vers l'extérieur, un par sujet
   └─ <Domaine>/                    capacités transversales (voir ci-dessous)
```

Règles :

1. **Un projet par domaine**, avec son projet de tests à côté. Les classes d'un domaine ne référencent jamais celles d'un autre domaine.
2. **Où va le code.** Ce qui est petit et utilisé par plusieurs domaines va dans `Systeme`. Ce qui est complexe, ou utilisé par un seul domaine, reste dans `Domaines`. Un élément naît dans son domaine et monte vers `Systeme` quand un deuxième domaine en a besoin.
3. **Entrées et noms.** Chaque domaine expose une interface d'entrée, `IDomaine<Domaine>`, qui est la seule chose que le site appelle. Une capacité transversale placée dans `Systeme` porte le nom du domaine sans le mot « Domaine » (`I<Domaine>`) et n'est jamais appelée directement par le site.
4. **Gestionnaires.** Les opérations d'un domaine sont portées par des gestionnaires de commande (qui modifient) et de requête (qui lisent), un par opération, du genre `GestionnaireCommande<Action>` et `GestionnaireRequete<Consultation>`. Le projet `Systeme` peut en contenir aussi pour les capacités transversales. L'interface d'entrée `IDomaine<Domaine>` est une façade mince qui délègue à ces gestionnaires : une page n'injecte ainsi qu'un ou deux `IDomaine<Domaine>` plutôt qu'une dizaine de gestionnaires. Ce choix s'écarte en partie de SOLID (l'interface de la façade regroupe plusieurs opérations et change à chaque opération ajoutée); l'écart est accepté pour garder le site simple, et la façade ne contient aucune règle d'affaires.
5. **Entités.** Les entités sont des classes de données simples, partagées, dans `Systeme/Entites`, sans attributs EF Core ni règles d'affaires. Leur configuration est dans `Systeme/Donnee`. Les règles vivent dans les gestionnaires. **Contrats :** chaque domaine a un projet frère `<Domaine>.Contrats`, sans aucune référence, qui contient ce que le site crée ou reçoit : commandes, requêtes et DTO de ce domaine (jamais les entités). Le mappeur entité → DTO reste dans le projet du domaine. Un contrat utilisé par plus d'un domaine monte vers `Systeme/Contrats`, créé à ce moment seulement. Les données de personnes passent toujours par un DTO; une entité peut être retournée telle quelle pour des données publiques (catalogue de jeux); l'écriture passe toujours par des commandes, jamais par un formulaire lié à une entité. Créer le projet `Contrats` dès le départ est un écart assumé au principe YAGNI : c'est une structure sans logique, qui évite d'extraire la frontière plus tard (le site client en navigateur ne référencerait que les contrats).
6. **Accès aux données.** L'interface de dépôt de chaque domaine (`IDepot<Domaine>`), avec les méthodes dont ce domaine a besoin, se trouve dans `Systeme/Interfaces`. L'implémentation, avec EF Core, est dans `Systeme/Donnee`, seul projet qui référence EF Core. Un dépôt ne contient **aucune règle d'affaires** : il répond à « qu'est-ce qui est stocké? » et enregistre, tandis que le gestionnaire décide. Il ne retourne jamais d'`IQueryable` (seulement des entités ou des valeurs matérialisées) et n'a pas de méthode générique. Un dépôt peut lire les tables d'autres domaines : un gestionnaire d'amitié obtient ainsi l'utilisateur visé par une demande, puis applique lui-même la règle « un compte `systeme` n'a pas d'amis », sans référencer le domaine des comptes.
7. **Sens des dépendances : les domaines dépendent de `Systeme`, jamais l'inverse.** Aucun projet de `Systeme` ne référence un domaine. Les domaines dépendent de `Systeme/Entites`, `Systeme/Interfaces`, `Systeme/Noyau` et des capacités transversales. Les adaptateurs vers l'extérieur (`Systeme/Donnee`, `Bgg`, `Images`, `Courriel`) implémentent les interfaces de `Systeme/Interfaces`, sans connaître les domaines. Le site dépend des domaines, et des adaptateurs seulement pour les enregistrer au démarrage, par des méthodes d'extension.
8. **Pas de dépôt générique, pas de médiateur, pas de décorateurs**, et pas de dossier fourre-tout (`Helpers`, `Utils`, `Tools`). Un besoin réel justifie leur ajout.
9. **Configuration commune.** Les propriétés partagées (version du cadre, nullabilité) et les versions des paquets sont centralisées dans `Directory.Build.props` et `Directory.Packages.props`, à la racine de la solution.
10. **Conventions de tests.** Les noms suivent toujours la forme `Methode_Condition_Resultat`, y compris pour les gestionnaires (dont la méthode est toujours `Traiter`). Une classe de base générique mince, `BaseTest<T>` (fixture AutoFixture avec AutoFakeItEasy, instance testée, accès aux fausses dépendances), vit dans un projet réservé aux tests, `Systeme/TestBase`, et non dans `Systeme/Noyau`, afin que les paquets de test n'entrent pas dans l'application publiée. Un domaine qui grossit peut être divisé en sous-domaines (`IDomaine<Domaine><SousDomaine>`) dans le même projet, sans créer de projet.
11. **Tests, en deux temps.** D'abord, les gestionnaires sont testés avec des dépôts factices, sans base de données réelle ([ADR 0008](0008-logique-d-affaires-testable.md)). Plus tard, les dépôts eux-mêmes seront testés contre un vrai PostgreSQL dans la chaîne d'intégration continue, car la documentation d'EF Core déconseille de simuler `DbSet` ou le fournisseur en mémoire pour les requêtes (comportement différent, notamment pour la casse du texte).
11. **Variante écartée : gestionnaires qui interrogent directement `DbContext`**, sans dépôt (approche courante lorsque la couche d'accès est déjà une interface simulable). Avec EF Core, il faudrait simuler `DbSet`, ce que la documentation déconseille; les requêtes seraient en outre dispersées dans les gestionnaires. Le coût d'une couche de plus est accepté.

## Conséquences

### Positives

- La séparation entre les domaines est vérifiée par le compilateur, et non par la discipline seule.
- Le placement du code suit une règle courte (petit et partagé, ou propre à un domaine) et chaque test se trouve à côté de ce qu'il vérifie.
- Les entités partagées et les dépôts par domaine permettent de vérifier un autre domaine sans créer de dépendance entre domaines.
- La structure est répétable : ajouter un domaine consiste à créer deux projets au même endroit.

### Négatives

- Le nombre de projets augmente avec chaque domaine. Seuls ceux dont le besoin est réel sont créés, mais le `Dockerfile` et la chaîne d'intégration doivent connaître chaque projet.
- Les entités sont de simples porteuses de données : une règle n'accompagne pas la donnée. Elle est dans le gestionnaire, qu'il faut donc retrouver.
- Une règle qui touche un autre domaine peut se retrouver dupliquée dans un dépôt si elle n'est pas montée vers `Systeme`. La règle 2 sert à limiter ce risque.
- La frontière entre « petit et partagé » et « complexe » est un jugement, pas une mesure.

## Décisions liées

- [ADR 0005](0005-base-de-donnees.md), [ADR 0007](0007-strategie-de-migrations.md) et [ADR 0008](0008-logique-d-affaires-testable.md).
- Cette décision remplace la proposition du 14 octobre 2026 (un seul projet `Domaine`).

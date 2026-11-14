# ADR 0015 : Recherche de jeux sans accents, tolérante aux fautes, et détection des doublons

- **Statut :** accepté
- **Date :** 2026-11-14

## Contexte

Le site est en français : les noms de jeux portent des accents (« Éléphant », « Café ») et la personne qui cherche ne les tape pas toujours. La recherche de jeux (ajout à la collection) et le filtre de la collection doivent retrouver « Éléphant » en tapant « elephant » ou « ELEPHANT », et « Café Crème » en tapant « creme ».

Sous PostgreSQL ([ADR 0005](0005-base-de-donnees.md)), la comparaison de texte est sensible à la casse et aux accents par défaut. Une collation ICU non déterministe existe déjà pour les courriels (ADR 0005, collation du courriel), mais elle est appliquée à des colonnes précises pour des comparaisons d'égalité et d'unicité, non pour une recherche « contient ».

Un second besoin s'ajoute : les jeux peuvent être créés à la main. Le schéma accepte volontairement plusieurs jeux de même nom (deux jeux différents peuvent se ressembler), il n'y a donc aucune contrainte contre les doublons. La seule protection possible est de montrer, au moment de la création, les jeux déjà présents dont le nom ressemble à celui qui est tapé, y compris avec une faute de frappe, pour que la personne ajoute l'existant plutôt que d'en créer un autre. La recherche sans accents, seule, ne le permet pas : « Catann » ne retrouve pas « Catan ».

Contraintes :

- le comportement doit être le même dans tous les environnements, sans règle propre à une base ;
- le schéma est produit par des migrations générées, sans gâchettes ([ADR 0007](0007-strategie-de-migrations.md)) ;
- le texte saisi par la personne ne doit jamais agir comme un motif (`%` et `_` sont des caractères comme les autres) ;
- le catalogue est petit au départ (centaines de jeux), mais une importation depuis BoardGameGeek le ferait passer à plusieurs dizaines de milliers.

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Extension `unaccent` seule : comparer `unaccent(nom)` en minuscules au texte traité de la même façon | Un « contient » sans accents ni casse; aucune modification des colonnes; l'extension est fournie avec PostgreSQL et installable par le compte de déploiement | Ni tolérance aux fautes de frappe, ni classement par pertinence, ni détection de doublons; la fonction n'est pas immuable, donc la recherche lit toute la table des jeux visibles |
| B. Collation ICU non déterministe sur la colonne `nom` | Aucune extension | Le support de `LIKE` avec une collation non déterministe dépend de la version du serveur; aucune tolérance aux fautes; la collation s'applique à toute comparaison de la colonne |
| C. Filtrer en mémoire dans l'application | Aucune dépendance à la base; toute logique de similarité est possible en code | Charge tout le catalogue à chaque recherche; ne passe pas à l'échelle; la pagination et le total se feraient aussi en mémoire |
| D. Colonne de nom normalisé tenue à jour par le code (par un intercepteur de `SaveChanges` et une interface, comme `modifie_le`) | Entièrement générée par le modèle; la normalisation est une fonction pure, testable sans base; pas de fonction SQL | Un champ dérivé que le code doit maintenir à chaque écriture (création, modification, importation) : une écriture directe dans la base le laisse incohérent; ne donne ni similarité ni classement à elle seule |
| E. `unaccent` et `pg_trgm` : colonne calculée stockée sur le nom sans accents ni casse, avec un index de trigrammes | Recherche « contient », tolérance aux fautes, classement par similarité et détection de doublons; l'index accélère la recherche; la cohérence est garantie par la base, sans code d'écriture | Une extension de plus; une fonction enveloppe déclarée immuable (une instruction SQL manuscrite dans la migration); des seuils de similarité à régler; des faux positifs possibles sur les noms courts |
| F. Moteur de recherche externe (Meilisearch, Typesense, Elasticsearch) | Tolérance aux fautes et pertinence prêtes à l'emploi | Un service de plus à exploiter, à synchroniser avec la base et à sécuriser, disproportionné pour ce catalogue |

## Décision

**Option E.**

- **Extensions.** `unaccent` et `pg_trgm`, déclarées dans le modèle par `HasPostgresExtension` et créées par migration. Toutes deux sont de confiance : le compte de déploiement les installe sans privilège de superutilisateur, et le compte de l'application n'a besoin d'aucun droit particulier pour s'en servir.
- **Colonne calculée stockée** `nom_recherche`, définie dans le modèle par `HasComputedColumnSql(..., stored: true)` : le nom du jeu en minuscules et sans accents, avec un **index de trigrammes** (`gin_trgm_ops`) généré par le modèle. La base garde la colonne cohérente à chaque écriture, sans gâchette ni code d'écriture, ce qui couvre aussi l'importation depuis BoardGameGeek et les scripts.
- **Fonction enveloppe immuable.** `unaccent` n'est pas déclarée immuable, et PostgreSQL refuse une fonction qui ne l'est pas dans une colonne calculée ou dans un index. Une fonction SQL (`unaccent_immuable`) qui l'enveloppe et se déclare immuable est créée par **une seule instruction SQL manuscrite**, placée dans la migration avant la colonne et supprimée par sa méthode `Down`. C'est le seul écart au principe de migrations entièrement générées (ADR 0007), assumé parce que le modèle d'EF Core ne sait pas créer une fonction, et documenté dans la migration.
- **Texte saisi.** Il est un paramètre, jamais concaténé dans un motif : les jokers sont des caractères littéraux. Un texte absent, vide ou fait d'espaces ne filtre rien dans la collection et ne déclenche aucune requête dans la recherche de jeux ; le gestionnaire retire les espaces aux extrémités avant d'appeler le dépôt. Le filtre porte sur le nom du jeu seulement.
- **Recherche.** Le dépôt interroge `nom_recherche` : les jeux dont le nom contient le texte d'abord, puis ceux dont un mot est semblable (similarité par mots), classés par pertinence, avec le nom et l'identifiant comme critères de départage pour garder une pagination stable. Un texte de moins de trois caractères produit trop peu de trigrammes : la recherche se limite alors à « contient ».
- **Détection de doublons.** À la création d'un jeu, l'interface montre les jeux visibles dont le **nom entier** est semblable à celui tapé, et propose de les ajouter plutôt que d'en créer un nouveau. Le même calcul peut signaler les doublons probables à l'administrateur au moment de la validation d'un jeu (`en_attente` vers `valide`). Aucune contrainte de base de données : le schéma permet des noms identiques.
- **Deux seuils distincts.** La recherche privilégie le rappel (retrouver largement) et la détection de doublons la précision (ne pas accuser à tort) : la détection utilise donc un seuil plus élevé et la similarité du nom entier, non celle des mots. Les seuils sont des constantes nommées, réglées sur des données réelles et validées contre la base (scripts de `docs/validation`). Le mode de réglage (paramètre du rôle de l'application, ou comparaison explicite sans index) se tranche à la mise en œuvre, avec mesure.

## Conséquences

### Positives

- « elephant », « ELEPHANT » et « Éléphant » retrouvent les mêmes jeux ; la recherche partielle (milieu du nom) fonctionne.
- La tolérance aux fautes (« Catann » retrouve « Catan ») et le classement par pertinence sont absorbés par la base, et la création manuelle peut prévenir les doublons, ce que ni la collation ni `unaccent` seule ne permettent.
- L'index de trigrammes accélère la recherche et le calcul de similarité : la lecture complète de la table disparaît, ce qui compte si le catalogue grossit avec une importation.
- La cohérence de la colonne de recherche est garantie par la base : aucun chemin d'écriture ne peut l'oublier.
- La règle de recherche est dans une requête lisible, et sa validation contre une base réelle est reproductible (`docs/validation`).

### Négatives

- Toute base (développement, production, intégration continue qui exécute les migrations) doit pouvoir créer les deux extensions. Le paquet de contributions de PostgreSQL qui les fournit doit exister sur le serveur, ce qui se vérifie par la migration elle-même.
- Une instruction SQL manuscrite subsiste, la fonction enveloppe. Déclarer une fonction immuable est une promesse : si le dictionnaire de `unaccent` change (mise à jour du serveur), l'index peut devenir incohérent et doit être reconstruit. Si la fonction posait un jour problème, l'option D (normalisation par le code, avec un intercepteur et une interface comme `modifie_le`, [ADR 0010](0010-identifiants-et-horodatages.md)) serait le repli, et `unaccent` deviendrait inutile.
- Les seuils de similarité sont à régler, et les noms courts ou proches (« Azul » et « Azur ») produisent des faux positifs ; la détection de doublons avertit sans bloquer.
- L'index de trigrammes coûte un peu d'espace et d'écriture, négligeables à ce volume.
- Le SQL réellement produit par EF Core pour les fonctions de similarité n'est pas couvert par les tests unitaires (qui n'utilisent pas de base) : seuls les scripts de validation contre une base réelle l'exercent. Les exécuter dans l'intégration continue ([ADR 0009](0009-organisation-de-la-solution.md), règle 11) lèverait cette limite.

## Décisions liées

- [ADR 0005](0005-base-de-donnees.md) : PostgreSQL et sensibilité à la casse du texte.
- [ADR 0007](0007-strategie-de-migrations.md) : migrations générées, sans gâchettes, appliquées par le compte de déploiement.
- [ADR 0009](0009-organisation-de-la-solution.md) : règle 11, tests des dépôts contre un vrai PostgreSQL.
- [ADR 0010](0010-identifiants-et-horodatages.md) : intercepteur de `modifie_le`, modèle de l'option D.
- [ADR 0014](0014-navigation-entre-entites-et-modeles-de-lecture.md) : la recherche retourne un modèle de lecture (`ResultatRechercheJeu`).

## Sources

- [Extension `unaccent` (PostgreSQL)](https://www.postgresql.org/docs/current/unaccent.html)
- [Extension `pg_trgm` (PostgreSQL)](https://www.postgresql.org/docs/current/pgtrgm.html)
- [Colonnes générées (PostgreSQL)](https://www.postgresql.org/docs/current/ddl-generated-columns.html)
- [Extensions de confiance (PostgreSQL, `CREATE EXTENSION`)](https://www.postgresql.org/docs/current/sql-createextension.html)

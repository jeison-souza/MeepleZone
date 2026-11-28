# Validation du schéma contre une base PostgreSQL réelle

Les tests unitaires du projet n'utilisent pas de base de données ([ADR 0008](../adr/0008-logique-d-affaires-testable.md)) : ils vérifient la logique d'affaires et le modèle d'EF Core, mais ni les contraintes réelles de PostgreSQL ni les requêtes que les dépôts exécutent. Les scripts de ce dossier comblent cet écart : appliqués à une base de développement une fois les migrations passées, ils exécutent les contraintes et les requêtes contre un PostgreSQL réel et comparent le résultat à ce que le schéma publié annonce. Ils documentent la validation demandée par l'issue #88 (valider le schéma contre une base de données réelle).

## Les scripts

| Script | Ce qu'il vérifie | Cas |
|---|---|---|
| [`01-catalogue.sql`](01-catalogue.sql) | Tables `jeu`, `mode_jeu`, `mecanique` et `jeu_mecanique` : contraintes `CHECK`, unicité, clés étrangères, `RESTRICT`, `CASCADE` et `SET NULL`, valeurs par défaut | 45 |
| [`02-possession.sql`](02-possession.sql) | Table `possession` : note de 1 à 10, parties jouées, unicité du couple (utilisateur, jeu), clés étrangères et suppressions | 14 |
| [`03-collection-lecture.sql`](03-collection-lecture.sql) | Lecture de la collection : total par utilisateur, tri stable (noms identiques départagés par l'identifiant), pagination, isolement entre utilisateurs | 10 |
| [`04-recherche.sql`](04-recherche.sql) | Recherche sans accents ni casse (extension `unaccent`), visibilité des jeux (validés, ou en attente du créateur seulement), indicateur de possession, jokers saisis traités comme des caractères | 15 |
| [`05-possession-ecritures.sql`](05-possession-ecritures.sql) | Modification et retrait d'une possession : le filtre par identifiant et utilisateur ne touche rien chez un autre utilisateur ; limites de la note, des parties jouées et du commentaire | 19 |
| [`06-ajout-jeu.sql`](06-ajout-jeu.sql) | Ajout d'un jeu à la collection : visibilité du jeu, et noms des violations d'unicité et de clé étrangère que le dépôt traduit en résultat | 12 |
| [`07-creation-jeu.sql`](07-creation-jeu.sql) | Création d'un jeu : chaque règle du domaine correspond à la contrainte `CHECK` attendue, noms des clés étrangères traduites, atomicité de la création du jeu, de ses mécaniques et de la possession | 30 |
| [`08-comptes-techniques.sql`](08-comptes-techniques.sql) | Comptes techniques « Système » et « Utilisateur supprimé » : contenu des données initiales, codes d'amitié réservés, format du code d'amitié, rôles, rattachement du contenu partagé à la suppression d'un compte | 16 |
| [`09-liaison-fournisseurs.sql`](09-liaison-fournisseurs.sql) | Liaison des fournisseurs (table `identite_externe`) : unicité de l'identité entre comptes (ADR 0004, règle 2), plusieurs connexions du même fournisseur, courriel facultatif et ses contraintes, clé étrangère, tri et isolement de la liste des connexions, suppression en cascade | 14 |
| [`10-suppression-compte.sql`](10-suppression-compte.sql) | Suppression de compte (ADR 0006) : raisons de l'ordre des étapes (`RESTRICT` sur les jeux, les annexes et les possessions), séquence complète dans une transaction, conservation du contenu partagé par rattachement au compte technique, suppression des possessions, des jeux non validés sans autre possession, des couvertures, de la photo et des connexions | 14 |
| [`11-filtre-collection.sql`](11-filtre-collection.sql) | Filtre de la collection (issue #13) : nombre de joueurs, meilleur nombre, durée maximale, note minimale, parties jouées, mode de jeu et mécaniques au choix, valeurs inconnues exclues, critères combinés, isolement entre personnes, noms des mécaniques triés | 16 |
| [`12-colonne-nom-recherche.sql`](12-colonne-nom-recherche.sql) | Colonne `jeu.nom_recherche` (ADR 0015) : fonction `unaccent_immuable` immuable, colonne générée stockée, index de trigrammes, extensions, valeurs sans accents ni casse, recalcul au changement de nom, écriture directe refusée, recherche « contient » et similarité de trigrammes | 15 |
| [`13-recherche-trigrammes.sql`](13-recherche-trigrammes.sql) | Recherche de jeux et détection des doublons (ADR 0015) : « contient » sans accents ni casse, tolérance aux fautes de frappe, texte trop court limité à « contient », jokers traités comme des caractères, similarité du nom entier pour les doublons avec un seuil plus strict que la recherche, nom court comparé à l'identique | 14 |
| [`14-amities.sql`](14-amities.sql) | Tables `demande_amitie` et `amitie` : une seule saisie (courriel ou code), code toujours rattaché à un compte, demande à soi-même refusée, statuts permis, une seule demande par personne visée, courriel sans égard à la casse, rattachement d'une demande par courriel à un compte avec saisie conservée, paire d'amis ordonnée et unique, clés étrangères et suppression en cascade d'un compte | 43 |
| [`15-courriel-verifie.sql`](15-courriel-verifie.sql) | Unicité du seul courriel vérifié ([ADR 0004](../adr/0004-liaison-comptes-fournisseurs.md), règle 1) : index partiel, copies non vérifiées acceptées, un seul compte par courriel vérifié sans égard à la casse, adresse libérée au changement, recherche d'un courriel à la création d'un compte | 14 |

## Exécution

1. Appliquer les migrations avec le compte de déploiement : en démarrant l'application avec la chaîne `ConnectionStrings:MeepleZoneMigration` dans les secrets de l'utilisateur, ou en exécutant `docs/schema/schema-idempotent.sql` (deux fois : la seconde ne doit rien changer). Les migrations ne s'appliquent jamais avec un compte d'administration du serveur : les tables en deviendraient propriétaires et le compte de l'application n'y aurait pas accès ([ADR 0007](../adr/0007-strategie-de-migrations.md)).
2. Exécuter chaque script **en entier**, avec le même compte. Les scripts qui l'indiquent commencent par afficher le compte utilisé.
3. La dernière requête de chaque script doit afficher `ok = true` à toutes les lignes.

Chaque script fonctionne dans une transaction qui se termine par un `ROLLBACK` : rien ne reste dans la base. Une fonction temporaire (`essayer`) exécute chaque cas et note soit `ok`, soit le nom de la contrainte violée (ou le code d'erreur) ; le résultat attendu est le nom de la contrainte que le schéma publié annonce, jamais une valeur copiée du code testé.

## Dernière exécution

Les 291 cas ont été exécutés du 11 au 28 novembre 2026 contre la base de développement, chacun après l'application des migrations qu'il vérifie. Tous sont conformes. Une première exécution a échoué sur un problème de droits : les tables du catalogue et de `possession` avaient été créées avec un compte d'administration, ce qui les rendait inaccessibles au compte de l'application. Le script a révélé l'erreur, qui a été corrigée (propriétaire et privilèges) avant que le site ne s'en serve.

La migration des amitiés, vérifiée par `14-amities.sql` le 27 novembre, a été appliquée par l'application à son démarrage. Ce script a été exécuté de nouveau le 27 novembre, après l'ajout de la colonne `demande_amitie.code_destinataire`.

La migration qui limite l'unicité aux courriels vérifiés a été appliquée par l'application à son démarrage, puis vérifiée le 28 novembre par `08-comptes-techniques.sql` et `15-courriel-verifie.sql`.

## Ce que ces scripts ne prouvent pas

- **Le SQL réellement produit par EF Core.** Les scripts reproduisent en SQL la logique des requêtes ; ils ne passent pas par le fournisseur Npgsql. La traduction de `Contains` combiné à `Unaccent`, par exemple, n'est confirmée que par l'usage.
- **La mise à jour de `modifie_le`.** Elle est faite par un intercepteur de l'application, non par la base.
- **L'ordre alphabétique des noms**, qui dépend de la collation du serveur : les scripts regroupent les résultats par identifiant lorsque l'ordre n'est pas ce qui est vérifié.

Ces points relèvent d'un test d'intégration contre un PostgreSQL réel, prévu avec l'exécution des migrations en intégration continue.

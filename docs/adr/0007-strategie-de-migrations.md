# ADR 0007 : Stratégie de migrations et rôles de la base de données

- **Statut :** accepté, amendé le 2026-10-15 (chiffrement de la connexion) et le 2026-11-24 (application des migrations au démarrage)
- **Date :** 2026-10-14

## Contexte

Le [schéma de données](../schema/schema-donnees.md) de MeepleZone repose sur PostgreSQL 18 et sur EF Core avec le connecteur Npgsql ([ADR 0005](0005-base-de-donnees.md)). Il faut décider comment la base est créée et comment son schéma évolue, comment les changements sont appliqués lors d'un déploiement, avec quels comptes de base de données l'application et les migrations s'exécutent, et comment la connexion entre l'application et la base est protégée.

Le schéma contient des données initiales (modes de jeu, mécaniques, comptes techniques) et une collation non déterministe pour le courriel ([ADR 0004](0004-liaison-comptes-fournisseurs.md)), que le modèle d'EF Core sait exprimer. Il n'a pas de déclencheur : `modifie_le` est mis à jour par le code, parce que la logique placée dans la base est plus difficile à voir, à déboguer et à tester sans base réelle, et qu'EF Core ne crée pas de déclencheurs.

**Amendement du 2026-11-24.** La première décision était d'exécuter un script SQL avant chaque déploiement, à la main tant que le déploiement l'était aussi. Avec le déploiement continu du jalon 4, une fusion dans la branche principale met la nouvelle version en production sans intervention : l'application des migrations ne peut plus dépendre d'une étape manuelle.

## Options considérées

### Création et évolution du schéma

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Migrations EF Core générées | Une seule source (le modèle C#); historique et état de la base gérés par l'outil; aucun SQL à écrire | Ne crée pas de déclencheurs ni d'autres objets hors du modèle |
| B. Scripts SQL écrits à la main | Contrôle total du SQL; le schéma se lit directement en DDL | Deux sources à garder cohérentes (le SQL et le modèle C#); exécuteur de scripts à ajouter ou à écrire; numérotation et historique à maintenir |
| C. Migrations EF Core avec SQL brut quand nécessaire (hybride) | Une source principale; couvre aussi ce qu'EF Core ne génère pas | Le SQL brut n'est pas vérifié par le modèle; il doit être relu; inutile tant qu'aucun besoin ne l'exige |

### Application des migrations lors du déploiement

| Option | Avantages | Inconvénients |
|---|---|---|
| 1. Au démarrage de l'application, avec une connexion distincte réservée aux migrations | Aucune étape de plus; le code et le schéma sont livrés ensemble; l'accès aux données garde un rôle sans droit sur la structure | Le secret du rôle de déploiement est accessible à l'application; une migration en échec empêche l'application de démarrer |
| 2. Script SQL généré, exécuté avant le déploiement | L'application n'a aucun droit sur la structure; le script se relit avant de s'exécuter | Une étape de plus dans le déploiement, manuelle ou à automatiser |
| 3. Paquet exécutable d'EF Core (`migrations bundle`) | Même séparation que le script, sans lire le SQL | Plus opaque; un exécutable de plus à produire et à lancer au bon moment |
| 4. Image distincte qui applique le script, puis vérification de version par l'application (examinée le 2026-11-24) | L'application ne reçoit jamais le secret de déploiement | Une image, un service et un contrôle de version de plus à construire et à maintenir |

### Chiffrement de la connexion à la base (amendement du 2026-10-15)

| Option | Avantages | Inconvénients |
|---|---|---|
| a. Aucun chiffrement | Rien à configurer | Les requêtes et les données personnelles (courriels, notes, commentaires) circulent en clair entre l'application et la base |
| b. TLS sans vérification du serveur (modes `Prefer` ou `Require` de Npgsql) | Les données sont chiffrées | Aucune garantie de joindre le bon serveur; avec `Prefer`, la connexion peut retomber en clair sans avertissement |
| c. TLS avec vérification du serveur (`VerifyFull`), certificat d'une autorité de certification privée au projet | Chiffrement et identité du serveur vérifiés; aucun coût de certificat | Une autorité à créer et à protéger; un certificat qui expire coupe la connexion |
| d. TLS avec vérification, certificat d'une autorité publique | Aucune autorité à gérer | Renouvellement fréquent et mise en place plus lourde pour un serveur qui n'est pas exposé à Internet |

## Décision

**Création et évolution : option A, avec C comme issue de secours.** Les migrations d'EF Core sont la source de vérité et sont entièrement générées par l'outil. Les données initiales se déclarent dans le modèle. La collation du courriel est déclarée une seule fois dans le modèle et appliquée aux trois colonnes concernées : PostgreSQL n'offre pas de collation non déterministe au niveau de la base. Si un besoin futur dépasse le modèle, du SQL brut peut s'ajouter à une migration (`migrationBuilder.Sql`). Un script `schema.sql`, généré à partir des migrations, présente le schéma en DDL.

**Application : option 1 (amendement du 2026-11-24, qui remplace l'option 2).**

- L'application applique les migrations en attente à son démarrage, avant d'accepter des requêtes (`MigrateAsync`). EF Core verrouille la table d'historique pendant l'opération, ce qui empêche deux applications concurrentes d'appliquer les mêmes migrations.
- Elle utilise pour cela une **chaîne de connexion distincte** (`ConnectionStrings:MeepleZoneMigration`), avec le rôle de déploiement, un contexte propre à l'opération, sans bassin de connexions, afin que la connexion soit fermée ensuite. Tout le reste de l'application utilise le rôle de l'application.
- Hors de l'environnement de développement, l'absence de cette chaîne empêche l'application de démarrer. En développement, elle est facultative : sans elle, aucune migration n'est appliquée et un avertissement est journalisé.
- Une migration doit rester **compatible avec la version précédente** de l'application (ajouter d'abord, retirer dans une version ultérieure), pour qu'un retour à l'image précédente reste possible.
- Un test sans base de données échoue si le modèle a des changements sans migration, pour que l'erreur soit détectée par l'intégration continue plutôt qu'au démarrage en production.
- Les scripts SQL restent générés à partir des migrations, pour la révision et pour une application manuelle.

**Rôles :** deux rôles PostgreSQL par environnement. Le rôle de déploiement, propriétaire de la base, modifie la structure. Le rôle de l'application peut seulement lire et écrire les données. Le superutilisateur sert une seule fois, à créer les rôles et la base vide. Les chaînes de connexion sont dans les secrets de l'utilisateur (`user-secrets`) en développement et dans des variables d'environnement en production, jamais dans le dépôt. Depuis l'amendement du 2026-11-24, la chaîne du rôle de déploiement figure dans la configuration de l'application, pour le seul usage des migrations. Le développement et la production ont des bases et des rôles distincts.

**Chiffrement : option c.** Les bases de MeepleZone n'acceptent que des connexions chiffrées (TLS), depuis des adresses autorisées, et l'application vérifie le serveur (`SSL Mode=VerifyFull`) avec le certificat d'une autorité de certification privée au projet. Le mode implicite de Npgsql (`Prefer`) n'est jamais laissé tel quel. La clé de l'autorité est conservée hors du serveur, dans un gestionnaire de mots de passe.

## Conséquences

### Positives

- Une seule source de vérité pour la structure, avec un historique géré par l'outil.
- Aucune étape manuelle au déploiement : le code et le schéma qu'il attend sont livrés ensemble.
- L'accès courant aux données se fait avec un rôle qui ne peut pas modifier la structure : une injection SQL, par exemple, reste limitée aux données.
- Une migration PostgreSQL est transactionnelle : une migration en échec est annulée entièrement.
- Le `schema.sql` généré donne une lecture directe du schéma et de ses changements, sans maintenir un second jeu de scripts.
- Les données personnelles sont chiffrées en transit et l'identité du serveur est vérifiée, ce qui répond aussi à l'exigence des services de base de données gérés, si le projet change un jour d'hébergement.

### Négatives

- **Risque accepté :** le secret du rôle de déploiement est accessible au conteneur de l'application. Une personne qui parviendrait à exécuter du code dans l'application pourrait modifier ou supprimer la structure de la base de l'application. Le risque est réduit par le retrait des privilèges superflus du conteneur et par le fait que ce rôle n'a aucun droit sur le reste du serveur. Les dégâts se répareraient par une sauvegarde, dont la restauration reste à essayer.
- **Risque accepté :** une migration en échec laisse l'application incapable de démarrer, alors que la version précédente a déjà été remplacée. La révision de la demande de fusion et l'essai sur la base de développement en réduisent la probabilité.
- EF Core 10 exécute une transaction par migration : si plusieurs migrations sont en attente et que l'une échoue, les précédentes restent appliquées.
- Le SQL exécuté en production n'est plus lancé à la main : il doit être relu dans la demande de fusion (différence de `schema.sql`), un SQL produit par l'outil pouvant être inattendu (par exemple une colonne recréée plutôt que renommée).
- La compatibilité avec la version précédente repose sur la discipline : aucun outil ne la vérifie.
- Les tests automatisés portent sur la logique d'affaires et sur le modèle, sans base réelle : les contraintes, les suppressions en cascade et l'exécution des migrations ne sont pas testées automatiquement. Cette limite est acceptée pour le moment; des tests contre une base réelle pourront s'ajouter plus tard, par exemple dans la chaîne d'intégration continue.
- Deux rôles et deux chaînes de connexion à gérer par environnement.
- Le certificat du serveur a une durée de vie limitée (trois ans) : son expiration doit être suivie, car elle couperait la connexion de l'application. Une configuration de règles d'accès modifiée n'est active qu'après un rechargement explicite du serveur.
- La vérification du serveur dépend de l'application et du client utilisé : certains clients chiffrent la connexion sans vérifier le serveur lorsqu'aucun fichier d'autorité n'est fourni.

## Sources

- [Migrations d'EF Core (Microsoft)](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)
- [Changements cassants d'EF Core 9 (Microsoft)](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes), qui signale le retour d'EF Core 10 à une transaction par migration
- [Collations et sensibilité à la casse avec Npgsql (EF Core)](https://www.npgsql.org/efcore/misc/collations-and-case-sensitivity.html)
- [CREATE DATABASE, PostgreSQL 18](https://www.postgresql.org/docs/18/sql-createdatabase.html) et [collations non déterministes](https://www.postgresql.org/docs/18/collation.html)
- [Rôles de PostgreSQL 18](https://www.postgresql.org/docs/18/user-manag.html)
- [Sécurité et SSL Mode de Npgsql](https://www.npgsql.org/doc/security.html) et [connexions TCP chiffrées avec SSL, PostgreSQL 18](https://www.postgresql.org/docs/18/ssl-tcp.html)

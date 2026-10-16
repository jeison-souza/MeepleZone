# ADR 0007 : Stratégie de migrations et rôles de la base de données

- **Statut :** accepté, amendé le 2026-10-15 (chiffrement de la connexion)
- **Date :** 2026-10-14

## Contexte

Le [schéma de données](../schema/schema-donnees.md) de MeepleZone repose sur PostgreSQL 18 et sur EF Core avec le connecteur Npgsql ([ADR 0005](0005-base-de-donnees.md)). Il faut décider comment la base est créée et comment son schéma évolue, comment les changements sont appliqués lors d'un déploiement, avec quels comptes de base de données l'application et les migrations s'exécutent, et comment la connexion entre l'application et la base est protégée.

Le schéma contient des données initiales (modes de jeu, mécaniques, comptes techniques) et une collation non déterministe pour le courriel ([ADR 0004](0004-liaison-comptes-fournisseurs.md)), que le modèle d'EF Core sait exprimer. Il n'a pas de déclencheur : `modifie_le` est mis à jour par le code, parce que la logique placée dans la base est plus difficile à voir, à déboguer et à tester sans base réelle, et qu'EF Core ne crée pas de déclencheurs. Le déploiement actuel est manuel; un déploiement continu est prévu pour le jalon 4.

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
| 1. Au démarrage de l'application | Aucune étape de plus | Le compte de l'application doit pouvoir modifier la structure; une migration en échec empêche l'application de démarrer |
| 2. Script SQL généré, exécuté avant le déploiement | L'application n'a aucun droit sur la structure; le script se relit avant de s'exécuter | Une étape de plus dans le déploiement |
| 3. Paquet exécutable d'EF Core (`migrations bundle`) | Même séparation que le script, sans lire le SQL | Plus opaque; un exécutable de plus à produire |

### Chiffrement de la connexion à la base (amendement du 2026-10-15)

| Option | Avantages | Inconvénients |
|---|---|---|
| a. Aucun chiffrement | Rien à configurer | Les requêtes et les données personnelles (courriels, notes, commentaires) circulent en clair entre l'application et la base |
| b. TLS sans vérification du serveur (modes `Prefer` ou `Require` de Npgsql) | Les données sont chiffrées | Aucune garantie de joindre le bon serveur; avec `Prefer`, la connexion retombe en clair sans avertissement si le serveur n'a pas de TLS |
| c. TLS avec vérification du serveur (`VerifyFull`), certificat d'une autorité de certification privée au projet | Chiffrement et identité du serveur vérifiés; aucun coût de certificat | Une autorité à créer et à protéger; un certificat qui expire coupe la connexion |
| d. TLS avec vérification, certificat d'une autorité publique | Aucune autorité à gérer | Renouvellement fréquent et mise en place plus lourde pour un serveur qui n'est pas exposé à Internet |

## Décision

**Création et évolution : option A, avec C comme issue de secours.** Les migrations d'EF Core sont la source de vérité et sont entièrement générées par l'outil. Les données initiales se déclarent dans le modèle. La collation du courriel est déclarée une seule fois dans le modèle et appliquée aux trois colonnes concernées : PostgreSQL n'offre pas de collation non déterministe au niveau de la base. Si un besoin futur dépasse le modèle, du SQL brut peut s'ajouter à une migration (`migrationBuilder.Sql`). Un script `schema.sql`, généré à partir des migrations, présente le schéma en DDL.

**Application : option 2.** Un script SQL idempotent est généré à partir des migrations et exécuté avant le déploiement par un rôle qui possède la base. Il est lancé à la main pour l'instant et deviendra une étape du déploiement continu. L'application ne migre jamais elle-même.

**Rôles :** deux rôles PostgreSQL par environnement. Le rôle de déploiement, propriétaire de la base, modifie la structure. Le rôle de l'application peut seulement lire et écrire les données. Le superutilisateur sert une seule fois, à créer les rôles et la base vide. Les chaînes de connexion sont dans les secrets de l'utilisateur (`user-secrets`) en développement et dans des variables d'environnement en production, jamais dans le dépôt. La chaîne du rôle de déploiement ne se trouve jamais dans la configuration de l'application. Le développement et la production ont des bases et des rôles distincts.

**Chiffrement : option c.** Les bases de MeepleZone n'acceptent que des connexions chiffrées (TLS), depuis des adresses autorisées, et l'application vérifie le serveur (`SSL Mode=VerifyFull`) avec le certificat d'une autorité de certification privée au projet. Le mode implicite de Npgsql (`Prefer`) n'est jamais laissé tel quel. La clé de l'autorité est conservée hors du serveur, dans un gestionnaire de mots de passe.

## Conséquences

### Positives

- Une seule source de vérité pour la structure, avec un historique géré par l'outil.
- Le script à exécuter peut être relu avant chaque déploiement.
- Une application compromise ne peut pas modifier ni supprimer la structure de la base.
- Une migration PostgreSQL est transactionnelle : en cas d'échec, rien n'est appliqué.
- Le `schema.sql` généré donne une lecture directe du schéma, sans maintenir un second jeu de scripts.
- Les données personnelles sont chiffrées en transit et l'identité du serveur est vérifiée, ce qui répond aussi à l'exigence des services de base de données gérés, si le projet change un jour d'hébergement.

### Négatives

- Une étape manuelle de plus à chaque changement de schéma, jusqu'à l'automatisation par le déploiement continu.
- Le script généré est relu avant chaque déploiement : un SQL produit par l'outil peut être inattendu (par exemple une colonne recréée plutôt que renommée).
- Les tests automatisés portent sur la logique d'affaires, sans base réelle : les contraintes et les suppressions en cascade ne sont pas testées automatiquement. Cette limite est acceptée pour le moment; des tests contre une base réelle pourront s'ajouter plus tard, par exemple dans la chaîne d'intégration continue.
- Deux rôles et deux chaînes de connexion à gérer par environnement.
- Le certificat du serveur a une durée de vie limitée (trois ans) : son expiration doit être suivie, car elle couperait la connexion de l'application. Une configuration de règles d'accès modifiée n'est active qu'après un rechargement explicite du serveur.
- La vérification du serveur dépend de l'application et du client utilisé : certains clients chiffrent la connexion sans vérifier le serveur lorsqu'aucun fichier d'autorité n'est fourni.

## Sources

- [Migrations d'EF Core (Microsoft)](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)
- [Collations et sensibilité à la casse avec Npgsql (EF Core)](https://www.npgsql.org/efcore/misc/collations-and-case-sensitivity.html)
- [CREATE DATABASE, PostgreSQL 18](https://www.postgresql.org/docs/18/sql-createdatabase.html) et [collations non déterministes](https://www.postgresql.org/docs/18/collation.html)
- [Rôles de PostgreSQL 18](https://www.postgresql.org/docs/18/user-manag.html)
- [Sécurité et SSL Mode de Npgsql](https://www.npgsql.org/doc/security.html) et [connexions TCP chiffrées avec SSL, PostgreSQL 18](https://www.postgresql.org/docs/18/ssl-tcp.html)

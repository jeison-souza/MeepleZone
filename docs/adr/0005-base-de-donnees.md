# ADR 0005 : Base de données

- **Statut :** accepté
- **Date :** 2026-10-10

## Contexte

MeepleZone doit conserver des données relationnelles : utilisateurs, catalogue de jeux, collections personnelles, relations d'amitié et, plus tard, les soirées. Ces données ont des liens nets entre elles (un utilisateur possède plusieurs jeux, une amitié relie deux utilisateurs), ce qui oriente vers une base de données relationnelle.

L'application est écrite en C# sous .NET 10. L'accès aux données passera par un connecteur compatible avec cette version; la qualité et la maintenance de ce connecteur sont le critère principal du choix.

Les autres critères sont les fonctionnalités offertes par chaque système, la portabilité vers des services infonuagiques gérés si l'application quitte un jour son hébergement initial, et la capacité de croissance. Le choix ne repose pas sur l'environnement d'hébergement actuel : les deux systèmes retenus peuvent s'installer dans un conteneur Docker.

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. MySQL | Très répandu; léger; connecteur officiel d'Oracle compatible avec EF Core 10; offert comme service géré par les quatre grands fournisseurs consultés | Fonctionnalités SQL plus limitées que PostgreSQL; propriété d'Oracle |
| B. MariaDB | Très répandu; léger; dérivé de MySQL, gouverné par une fondation | Seul un connecteur communautaire (Microting) prend en charge EF Core 10; service géré moins répandu (AWS RDS l'offre, Azure l'a retiré en 2025); a divergé de MySQL |
| C. PostgreSQL | Connecteur Npgsql compatible avec EF Core 10 et activement maintenu; riche en fonctionnalités (types, index, extensions, recherche textuelle); offert comme service géré par tous les fournisseurs consultés; modifications de schéma transactionnelles | Un peu plus lourd à administrer; comparaisons de texte sensibles à la casse par défaut |

MariaDB est un dérivé de MySQL dont les fonctionnalités ont divergé : ce qui fonctionne sur l'un n'est pas garanti sur l'autre. Le connecteur d'Oracle cible MySQL et ne documente pas MariaDB.

## Comparaison des trois systèmes

| Critère | MySQL | MariaDB | PostgreSQL |
|---|---|---|---|
| Nature | Système de gestion de bases de données relationnelles (SGBDR) | SGBDR, dérivé de MySQL | SGBDR avec des fonctionnalités objet en plus (types personnalisés, tableaux, héritage de tables) |
| Connecteur EF Core 10 | Oracle (officiel) | Microting (communautaire) | Npgsql (communautaire, référence de fait) |
| Service géré | AWS, Azure, Google Cloud SQL, DigitalOcean | AWS RDS; retiré d'Azure le 2025-09-19; non listé chez Google Cloud SQL ni DigitalOcean | AWS, Azure, Google Cloud SQL, DigitalOcean |
| Licence et gouvernance | GPL, propriété d'Oracle | GPL, fondation MariaDB | Licence PostgreSQL (permissive), communautaire |
| Types et fonctions | JSON natif; types plus classiques | JSON stocké comme texte validé; types plus classiques | Types riches (JSONB, tableaux, plages), index avancés, extensions, recherche plein texte |
| Modifications de schéma (migrations) | Non transactionnelles : une migration interrompue peut laisser le schéma à moitié modifié | Idem MySQL | Transactionnelles : une migration en échec est annulée |
| Comparaison de texte | Insensible à la casse par défaut | Idem MySQL | Sensible à la casse par défaut. Pas de réglage au niveau de la base : on applique une collation insensible à la casse colonne par colonne (nécessaire pour l'unicité des courriels, voir l'[ADR 0004](0004-liaison-comptes-fournisseurs.md)) |
| Croissance | Réplicas de lecture et service géré disponibles | Idem | Idem; meilleur pour les requêtes complexes |
| Avantages pour le projet | Connecteur officiel; simple; très répandu | Simple; très répandu | Connecteur solide; schéma plus sûr; portabilité infonuagique maximale |
| Inconvénients pour le projet | Moins riche; migrations non transactionnelles | Connecteur communautaire seulement; hébergement géré réduit | Plus lourd; sensibilité à la casse à gérer |

## Vérification des connecteurs (2026-10-11)

| Connecteur | Prise en charge d'EF Core 10 | État de maintenance |
|---|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 | Oui (cible net10.0, EF Core 10.0.4 et plus) | Actif : une version 11 est déjà en candidate de livraison (2026-09-24) |
| `MySql.EntityFrameworkCore` (Oracle) 10.0.9 | Oui (net10.0, 2026-07-29) | Maintenu par Oracle; cible MySQL, aucune mention de MariaDB sur sa page |
| `Microting.EntityFrameworkCore.MySql` 10.0.12 | Oui (fork de Pomelo, 2026-09-18) | Actif (versions fréquentes, licence MIT, bâti sur MySqlConnector, MySQL et MariaDB pris en charge); communautaire et non officiel; environ 1,1 million de téléchargements, du même ordre que les versions 10.x du connecteur d'Oracle |
| `Pomelo.EntityFrameworkCore.MySql` (communautaire, projet d'origine du précédent) | Non : dernière version stable 9.0.0 (2025-08-17), pour EF Core 9 seulement | Écarté : le projet semble à l'arrêt (dernière annonce du responsable le 2025-10-27, sans suite; erreur au démarrage signalée avec Pomelo 9 sur EF Core 10). C'est de loin le plus téléchargé (environ 140 millions), mais surtout pour des versions antérieures d'EF Core |

Conclusion de la vérification : les deux options disposent d'un connecteur compatible avec EF Core 10. Celui de PostgreSQL est communautaire mais très répandu et activement maintenu; pour MySQL, le connecteur officiel d'Oracle existe, et pour MariaDB seul le connecteur communautaire Microting est disponible.

## Outils de migration

Les migrations sont gérées par l'outil commun d'EF Core (`dotnet ef migrations`), identique quel que soit le connecteur. Le choix de la base ne change donc pas la façon de faire évoluer le schéma, seulement le code SQL généré.

## Tests automatisés

La logique d'affaires est testée sans base de données réelle, avec des dépôts factices (voir l'[ADR 0007](0007-strategie-de-migrations.md)). Cette approche ne dépend pas du système retenu.

## Décision

Option C : PostgreSQL 18, avec le connecteur Npgsql (`Npgsql.EntityFrameworkCore.PostgreSQL`) et les migrations d'EF Core.

Raisons principales :
- un serveur PostgreSQL 18 est déjà disponible dans l'environnement d'hébergement initial (image `postgres:18-alpine`), ce qui facilite le déploiement initial;
- le connecteur est compatible avec EF Core 10 et activement maintenu;
- le service géré existe chez tous les fournisseurs infonuagiques consultés, ce qui facilite un déménagement éventuel;
- les modifications de schéma sont transactionnelles : une migration en échec est annulée;
- les types et les index plus riches laissent de la marge pour la croissance (catalogue de jeux, recherche textuelle).

Les comparaisons de texte insensibles à la casse (courriel unique, voir l'[ADR 0004](0004-liaison-comptes-fournisseurs.md)) se règlent par une collation non déterministe appliquée aux colonnes concernées.

## Conséquences

### Positives

- Connecteur solide, qui suit de près les versions d'EF Core et de .NET.
- Migrations sûres : un échec ne laisse pas le schéma à moitié modifié.
- Portabilité vers les services gérés des principaux fournisseurs infonuagiques, sans changer de système.
- Types, index et extensions disponibles si les besoins augmentent.

### Négatives

- Le connecteur est communautaire (équipe Npgsql) et non fourni par le projet PostgreSQL ni par Microsoft.
- Les comparaisons de texte sont sensibles à la casse par défaut : une collation doit être déclarée pour les colonnes concernées (courriel, au minimum). Les collations non déterministes ont un coût : performance moindre, index B-tree sans déduplication et « certaines opérations de recherche par motif » impossibles selon la documentation (`LIKE` est pris en charge depuis la version 18, retenue ici). Elles ne conviennent donc qu'aux colonnes qui en ont besoin.
- Système un peu plus lourd à administrer que MySQL.

## Sources

- [Fournisseurs de base de données d'EF Core (Microsoft)](https://learn.microsoft.com/en-us/ef/core/providers/) (le tableau n'est pas à jour pour Npgsql, vérifié sur NuGet)
- [Npgsql.EntityFrameworkCore.PostgreSQL (NuGet)](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL)
- [Pomelo.EntityFrameworkCore.MySql (NuGet)](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql) et [question sur la prise en charge d'EF Core 10](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/2007)
- [Microting.EntityFrameworkCore.MySql (NuGet)](https://www.nuget.org/packages/Microting.EntityFrameworkCore.MySql)
- [MySql.EntityFrameworkCore (NuGet)](https://www.nuget.org/packages/MySql.EntityFrameworkCore)
- [Retrait d'Azure Database for MariaDB (Microsoft)](https://azure.microsoft.com/updates/azure-database-for-mariadb-will-be-retired-on-19-september-2025-migrate-to-azure-database-for-mysql-flexible-server) et [Amazon RDS pour MariaDB (AWS)](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/CHAP_MariaDB.html)
- [Bases de données gérées de DigitalOcean](https://docs.digitalocean.com/products/databases) (MariaDB absent de la liste des moteurs). Pour Google Cloud SQL, des sources tierces donnent MySQL, PostgreSQL et SQL Server; à confirmer dans la documentation de Google
- [Collations et sensibilité à la casse avec Npgsql (EF Core)](https://www.npgsql.org/efcore/misc/collations-and-case-sensitivity.html)
- [Notes de version de PostgreSQL 18](https://www.postgresql.org/docs/18/release-18.html) et [collations non déterministes](https://www.postgresql.org/docs/18/collation.html)

# ADR 0005 : Base de données

- **Statut :** brouillon (décision à prendre)
- **Date :** 2026-10-10

## Contexte

MeepleZone doit conserver des données relationnelles : utilisateurs (voir l'[ADR 0001](0001-authentification.md)), catalogue de jeux, collections personnelles, relations d'amitié et, plus tard, soirées et prêts. Ces données ont des liens nets entre elles (un utilisateur possède plusieurs jeux, une amitié relie deux utilisateurs), ce qui oriente vers une base de données relationnelle.

L'application est écrite en C# sous .NET 10. L'accès aux données passera par un connecteur compatible avec cette version; la qualité et la maintenance de ce connecteur sont le critère principal du choix. Le serveur de base de données doit aussi pouvoir tourner dans l'environnement d'hébergement (conteneurs Docker).

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. MariaDB / MySQL | Très répandu; léger; le plan d'origine du projet | Connecteur Entity Framework Core à vérifier pour .NET 10 (fournisseur communautaire ou fournisseur d'Oracle); fonctionnalités SQL plus limitées que PostgreSQL |
| B. PostgreSQL | Fournisseur Entity Framework Core (Npgsql) très actif et largement utilisé; riche en fonctionnalités (types, contraintes, recherche textuelle) | Un peu plus lourd à administrer; nouveau pour l'auteur |
| C. SQLite | Aucun serveur à installer ni à protéger; simple pour le développement et les tests | Écritures concurrentes limitées; sauvegarde et évolution moins adaptées à un site multiutilisateur |

Points à vérifier avant de trancher : compatibilité et état de maintenance de chaque connecteur avec .NET 10, outils de migration, et facilité de test automatisé.

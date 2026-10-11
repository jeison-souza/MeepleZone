# ADR 0001 : Authentification des utilisateurs

- **Statut :** accepté
- **Date :** 2026-10-10

## Contexte

MeepleZone gère des collections personnelles de jeux de société et un volet social (amis, partage de collections). Chaque utilisateur doit donc être identifié de façon fiable, et seuls ses amis doivent pouvoir voir sa collection.

L'application est une application Blazor (rendu interactif côté serveur) sous .NET 10. Le projet est réalisé en solo, avec peu de temps : la gestion des comptes ne doit pas devenir un chantier à part entière.

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Comptes locaux (courriel et mot de passe, ASP.NET Core Identity) | Aucune dépendance externe; contrôle complet de l'expérience | Stockage des mots de passe à sécuriser; réinitialisation, changement de mot de passe, vérification du courriel et verrouillage à construire et à maintenir; surface d'attaque plus grande |
| B. Fournisseurs d'identité externes seulement (OAuth 2.0 / OpenID Connect) | Aucun mot de passe à stocker ni à gérer; mécanismes de sécurité (authentification à deux facteurs, détection d'anomalies) délégués aux fournisseurs; inscription rapide; fournisseurs ajoutables au fil du temps | Dépendance aux fournisseurs; un utilisateur sans compte chez l'un des fournisseurs offerts ne peut pas s'inscrire; configuration de chaque fournisseur |
| C. Fournisseurs externes et comptes locaux | Couvre tous les cas | Cumule la complexité de A et de B; liaison de comptes à gérer (un même courriel via plusieurs méthodes) |
| D. Service d'identité tiers (Auth0, Keycloak, etc.) | Fonctionnalités complètes sans code d'authentification | Service de plus à héberger ou à payer; surdimensionné pour le projet |

## Décision

Option B (décidée le 2026-10-05) : connexion uniquement par des fournisseurs d'identité externes, sans compte local. Aucun mot de passe n'est géré par l'application, ce qui retire du périmètre les fonctions les plus coûteuses et les plus risquées (stockage, réinitialisation, verrouillage).

Le projet débute avec Google et Microsoft, pour la simplicité de mise en œuvre (gestionnaires fournis par ASP.NET Core) et parce que ces deux fournisseurs sont très répandus. D'autres fournisseurs (GitHub, Facebook, etc.) pourront s'ajouter avec le temps : chacun est un gestionnaire d'authentification indépendant, et l'identifiant stable d'un utilisateur, le couple fournisseur + identifiant du sujet (`sub`) et non le courriel, qui peut changer, permet d'en ajouter sans modifier la structure des utilisateurs. Chaque ajout demande tout de même d'enregistrer l'application chez le fournisseur et de configurer ses secrets. La décision pourra être révisée si un besoin réel de comptes locaux apparaît.

Mise en œuvre prévue : authentification par cookie dans l'application, avec les gestionnaires de Google et de Microsoft fournis par ASP.NET Core. La structure de la table des utilisateurs dépend du choix de la base de données (voir l'[ADR 0005](0005-base-de-donnees.md)).

## Conséquences

### Positives

- Aucune table de mots de passe, aucun flux de réinitialisation à développer ni à tester.
- Les mécanismes de sécurité des comptes (authentification à deux facteurs, détection d'anomalies) sont assurés par les fournisseurs.
- L'application ne stocke que le minimum de données personnelles (nom d'affichage et courriel fournis par le fournisseur).

### Négatives

- Dépendance à Google et à Microsoft : un fournisseur indisponible empêche la connexion, et une personne sans compte chez l'un des deux ne peut pas s'inscrire.
- Il faut enregistrer l'application chez chaque fournisseur, avec des URL de redirection stables par environnement (voir l'[ADR 0003](0003-domaine-canonique.md) sur le domaine canonique), et conserver les secrets hors du dépôt.
- Les tests automatisés ne peuvent pas passer par les vrais fournisseurs : un schéma d'authentification de test est nécessaire.
- Une personne qui se connecte avec plusieurs fournisseurs pose la question de la liaison des comptes, traitée dans l'[ADR 0004](0004-liaison-comptes-fournisseurs.md), et cette question se complique à chaque fournisseur ajouté. La suppression d'un compte et de ses données est traitée dans l'[ADR 0006](0006-suppression-de-compte.md).

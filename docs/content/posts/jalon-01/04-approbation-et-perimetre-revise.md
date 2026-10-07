---
title: "Projet approuvé : le volet social entre dans le MVP"
date: 2026-10-06
draft: false
tags: ["Jalon 1", "Fondation", "MoSCoW"]
categories: ["Documentation"]
---

## 1. Projet approuvé

Le projet a été approuvé à l'issue du Jalon 1. Le retour de l'enseignant est positif sur la vision, les *user stories* et la chaîne d'intégration continue (CI). Il relève aussi des points à corriger, dont l'un touche directement au périmètre.

## 2. Changement de périmètre : le social fait partie du MVP

Dans la priorisation MoSCoW du billet précédent, le catalogue personnel était le seul élément « Must have ». Le volet social (amis, partage des collections) était classé « Should have ».

La remarque de l'enseignant est que le catalogue seul ne suffit pas à faire une application transactionnelle à plusieurs utilisateurs. Sans interaction entre les utilisateurs, chacun se retrouve avec un inventaire isolé, ce qui est justement le problème que le projet cherche à résoudre. Le volet social est donc **intégré au cœur livré**.

### Nouvelle priorisation

**Must have (MVP)**

* Authentification déléguée à Google ou Microsoft (#9).
* Catalogue personnel : ajouter, modifier, retirer, filtrer et classer ses jeux (#10).
* Connexion sociale : ajouter des amis (#26, #36).
* Partage de collections : consulter la ludothèque d'un ami, dans le respect de ses préférences de confidentialité (#27).

**Should have**

* Prêts croisés entre amis.
* Choix des jeux pour une soirée (#45).

**Could have**

* Planificateur de soirées : calendrier de disponibilité et croisement des agendas (#43, #28, #44).
* Statistiques de jeu.

### Pourquoi ne pas monter aussi les « Should have »

Le temps disponible est limité et le MVP s'est déjà élargi. Remonter aussi les prêts et le choix des jeux mettrait en péril la qualité du catalogue et du volet social, qui sont maintenant le noyau. Ces deux éléments restent donc « Should have » et seront abordés seulement si le noyau est solide.

## 3. Conséquences sur le calendrier

* **Jalon 2 (modèle de données) :** le schéma doit prévoir dès maintenant les utilisateurs, les liens d'amitié et la visibilité d'une collection. Le travail de modélisation devient plus important que prévu.
* **Ordre de développement :** authentification, puis catalogue, puis amitiés, puis consultation de la collection d'un ami. Le volet social commence une fois le catalogue solide.
* **Jalons :** les tâches sociales (#26, #27, #36) restent pour l'instant au Jalon 4. Ce placement sera réévalué à la fin du Jalon 2, quand le schéma de données et l'effort réel seront mieux connus.

## 4. Autres remarques à traiter

* **Tests :** les tests actuels sont des exemples qui ne touchent aucun code de l'application. Ils seront remplacés par des tests du domaine dès que le catalogue sera implanté.
* **Blogue :** les alternatives écartées seront discutées plus en détail dans les prochains posts (avantages, inconvénients et critère de choix), plutôt que simplement nommées.

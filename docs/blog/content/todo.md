---
title: "Notes et To-Do List"
menus:
  main:
    name: "ToDo"
    weight: 99
date: 2026-10-06
draft: true
hidden: false
---

## 📌 Planification (périmètre révisé)

### Jalon 2 : Architecture et données (02/11)
- [x] Choisir la base de données : PostgreSQL (ADR 0005)
- [x] Schéma de données commenté, dans `docs/schema/` : version 0.3 publiée, avec toutes les tables du produit minimal viable (comptes, jeux, collection, amitiés)
- [x] Source de données des jeux : base locale alimentée par les utilisateurs; BoardGameGeek hors du produit minimal viable
- [ ] Authentification déléguée (Google et Microsoft) : décision prise (ADR 0001), mise en œuvre à faire
- [x] Rédiger 2 à 3 ADR dans `docs/adr/` (ADR 0001 à 0009 acceptés; l'ADR 0004 a été amendé le 14/10 et l'ADR 0007 le 15/10, pour le chiffrement de la connexion)
- [x] Stratégie de migrations et comptes de base de données : migrations EF Core générées, appliquées par script, deux comptes distincts pour le déploiement et pour l'application (ADR 0007)
- [x] Serveur PostgreSQL préparé : connexion chiffrée (TLS) vérifiée, comptes séparés, bases de développement et de production
- [x] Organisation de la solution en projets par domaine (ADR 0009, logique d'affaires testable : ADR 0008); le dossier du site est réorganisé (`Site/Site`, `Site/Site.Client`, `Site/Site.Test`), les autres projets seront créés au moment du besoin
- [x] Acheter le domaine et configurer Cloudflare
- [x] Valider l'exécution du CI sur GitHub Actions (le nom `ci.yaml` est conservé)
- [ ] Billet de blogue du Jalon 2
- [ ] Réévaluer le placement du volet social à la fin du jalon

### Jalon 3 : Développement (16/11)
- [x] Interface en français, page À propos et pages d'erreur illustrées (404 et 500, avec scénarios tirés au hasard)
- [ ] Première migration EF Core appliquée à la base de développement (valide aussi la collation du courriel)
- [ ] Connexion de l'application à la base (Npgsql, certificat du serveur vérifié)
- [ ] Catalogue : ajouter, modifier, retirer, noter, filtrer
- [ ] Tests du domaine (remplacer les tests d'exemple)
- [ ] Commits graduels et PR avec description

### Jalon 4 : Livraison finale (07/12)
- [ ] Volet social (MVP) : amis, partage de collections (#26, #27, #36)
- [ ] Déploiement continu à chaque push sur `main`
- [ ] Application accessible en ligne pour la correction
- [ ] Should : choix des jeux pour une soirée
- [ ] Could : planificateur de soirées, statistiques

## 📅 Entrevues
- [ ] Mi-parcours : 09/11
- [ ] Finale : 14/12

## 💡 Idées / Rappels
- Ne pas oublier de vérifier la mise en page sur mobile.
- Ajuster les balises YAML si un nouveau menu est requis.
- Tester menu ToDo quand généré via Github.io

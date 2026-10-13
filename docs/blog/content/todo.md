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
- [ ] Schéma de données commenté, dans `docs/schema/` : version 0.1 faite (utilisateurs, connexions externes, annexes); restent les jeux, la collection, les amitiés et la visibilité
- [x] Source de données des jeux : base locale alimentée par les utilisateurs; BoardGameGeek hors du produit minimal viable
- [ ] Authentification déléguée (Google et Microsoft) : décision prise (ADR 0001), mise en œuvre à faire
- [x] Rédiger 2 à 3 ADR dans `docs/adr/` (ADR 0001 à 0005 acceptés, 0006 en rédaction)
- [x] Acheter le domaine et configurer Cloudflare
- [ ] Valider l'exécution du CI sur GitHub Actions (le nom `ci.yaml` est conservé)
- [ ] Billet de blogue du Jalon 2
- [ ] Réévaluer le placement du volet social à la fin du jalon

### Jalon 3 : Développement (16/11)
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

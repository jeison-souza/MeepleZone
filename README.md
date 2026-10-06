# MeepleZone 🎲

Application web de gestion de collection de jeux de société et de mise en réseau sociale pour les passionnés de ludisme. Projet développé dans le cadre du cours INF1410 (Génie logiciel).

---

## 📖 Journal de bord (Blog technique)

Retrouvez le suivi des notes de développement, les choix d'architecture et l'avancement du projet sur mon [blogue technique hébergé sur GitHub Pages](https://jeison-souza.github.io/MeepleZone/).

---

## 🎯 Vision

En tant que passionné de jeux de société, je me retrouve souvent confronté au même défi : ma collection grandit au fil des acquisitions, mais gérer ce petit inventaire personnel devient rapidement un véritable casse-tête. Entre la difficulté de trier mes boîtes selon des critères précis (comme les mécaniques, la durée ou le nombre de joueurs), l'impossibilité de savoir d'un coup d'œil ce que possèdent mes amis sans leur demander un inventaire manuel, et la complexité de synchroniser nos agendas pour organiser une soirée ludique, les frictions s'accumulent. C’est précisément pour répondre à ce besoin d'organisation et de partage que je développe **MeepleZone**.

L'idée derrière cette application est de me simplifier la vie en centralisant tout ce dont un joueur a besoin sur une seule plateforme intuitive. Le cœur du projet repose sur un **catalogue personnel** qui me permet d'enregistrer, de filtrer et de classer toute la ludothèque selon certaines critères, comme par exemple, par nombre de joueurs, durée d'une partie. Mais l'expérience va plus loin grâce à un **réseau social intégré** : on peux me connecter avec nos amis, explorer leurs collections (selon leurs préférences de confidentialité) pour découvrir de nouveaux titres ou imaginer des prêts croisés, et utiliser un **planificateur de soirées intelligent** pour coordonner nos disponibilités en un clin d'œil, éliminant ainsi les interminables discussions de groupe. 

En somme, **MeepleZone** transforme la gestion de nos loisirs en une activité aussi fluide et agréable que d'ouvrir une boîte de jeu un vendredi soir.

---

Pour tout amateur de jeux de société, posséder, entretenir et faire grandir une ludothèque est une véritable passion. Cependant, à mesure que la collection s'élargit au fil des acquisitions, la gérer au quotidien se transforme rapidement en un défi logistique complexe. Entre la dispersion des informations, l'isolement des collections personnelles et le casse-tête de la planification, les frictions s'accumulent pour les passionnés. 

Pour éliminer ces obstacles et transformer la gestion des loisirs en une expérience fluide, **MeepleZone** propose une plateforme web centralisée et intuitive qui répond directement aux principaux problèmes du quotidien à travers les axes suivants :

* **Catalogue personnel de jeux (MVP - Core) :** 
  * *Le problème :* Il est difficile de trier, filtrer et classer rapidement ses boîtes selon des critères spécifiques (mécaniques, nombre de joueurs, durée ou complexité) lorsque la collection grandit.
  * *La solution :* Un espace unifié pour enregistrer et organiser toute sa ludothèque selon ses propres attributs.
* **Réseau social et partage d'amis (SH - Social) :** 
  * *Le problème :* Les collections restent isolées, rendant impossible la découverte des jeux détenus par ses proches sans passer par des inventaires manuels et fastidieux.
  * *La solution :* Une dimension collaborative permettant de se connecter avec ses cercles d'amis, de consulter leurs ludothèques (dans le strict respect des paramètres de confidentialité) pour stimuler les découvertes, faciliter les prêts croisés et simplifier le choix des jeux lors de l'organisation des soirées entre amis.
* **Planificateur de soirées de jeux (CH - Calendrier) :** 
  * *Le problème :* Organiser une soirée ludique se solde souvent par des discussions interminables sur les applications de messagerie pour concilier les agendas de tout le monde.
  * *La solution :* Un module de calendrier intelligent et intégré qui permet de synchroniser les disponibilités du groupe pour planifier la prochaine table de jeu idéale sans efforts.
* **Statistiques de jeu (CH - Statistiques) :** 
  * *Le problème :* Sans historique, il est difficile de savoir quels jeux de la ludothèque sont réellement joués et à quelle fréquence.
  * *La solution :* Un suivi de l'historique et de la fréquence des parties pour chaque jeu.

En somme, **MeepleZone** rassemble tout ce dont un joueur a besoin pour que le partage et l'organisation soient aussi agréables que de s'installer autour d'une table pour lancer une nouvelle partie.

---

## 🛠️ Stack Technique (Aperçu)
* **Frontend & Backend :** Blazor Web App (.NET 10) avec composants UI MudBlazor.
* **Architecture :** Solution full-stack intégrée (Server/WebAssembly) organisée en couches.
* **Tests :** Tests unitaires automatisés (TDD).
* **Déploiement :** Conteneurisation Docker, CI/CD via GitHub Actions.
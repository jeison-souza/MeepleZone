---
title: "Jalon 2 : une base choisie, un schéma, et un menu qui ne s'ouvrait pas"
date: 2026-11-10
draft: false
tags: ["Jalon 2", "Architecture", "PostgreSQL", "Schéma de données", "Intelligence artificielle"]
categories: ["Documentation"]
---

## 1. La décision principale : PostgreSQL

Trois systèmes ont été comparés dans l'[ADR 0005](https://github.com/jeison-souza/MeepleZone/blob/main/docs/adr/0005-base-de-donnees.md) : MySQL, MariaDB et PostgreSQL. SQLite avait été écarté d'entrée : MeepleZone est une application à plusieurs utilisateurs qui doit tourner derrière un serveur, pas un fichier local.

Le critère principal était le connecteur pour .NET 10, plus que les fonctionnalités de la base :

- **MySQL** : connecteur officiel d'Oracle, compatible avec EF Core 10, mais des modifications de schéma non transactionnelles.
- **MariaDB** : seul un connecteur communautaire (Microting) prend en charge EF Core 10, et le service géré est moins répandu (Azure l'a retiré en 2025).
- **Pomelo**, le connecteur communautaire de référence pour MySQL et MariaDB, a été écarté en cours de route : il n'a pas de version pour EF Core 10 et le projet semble à l'arrêt.
- **PostgreSQL** : Npgsql est compatible avec EF Core 10 et activement maintenu (une version 11 est déjà en candidate de livraison), le service géré existe chez tous les fournisseurs consultés, et une migration qui échoue est annulée au lieu de laisser le schéma à moitié modifié.

Le coût accepté est la sensibilité à la casse. PostgreSQL compare le texte en distinguant majuscules et minuscules, ce qui est un problème pour le courriel, qui doit être unique sans égard à la casse. L'idée de départ était un réglage global pour toute la base. La documentation a corrigé l'idée : `CREATE DATABASE` n'offre pas de collation non déterministe par défaut. La collation est donc déclarée une fois dans le modèle et appliquée colonne par colonne, aux trois colonnes de courriel.

Un détail est venu avec l'image Docker légère (Alpine) : la collation affichée est `en_US.utf8`, mais `'a' < 'B'` est faux, car la bibliothèque `musl` trie par code et non par langue. Un `ORDER BY nom` serait faux en français, avec les lettres accentuées après le `z`. Les bases sont donc créées avec une collation ICU `fr-CA` par défaut, la seule chose qui ne se change qu'à la création de la base.

Autres décisions du même ensemble : pas de déclencheurs (la logique cachée dans la base est difficile à tester, et EF Core n'en génère pas), des migrations entièrement générées et appliquées par un script, et deux comptes de base de données distincts, un pour le déploiement et un pour l'application ([ADR 0007](https://github.com/jeison-souza/MeepleZone/blob/main/docs/adr/0007-strategie-de-migrations.md)).

## 2. L'organisation de la solution

La question suivante était de savoir où vivrait le code. Deux contraintes pesaient : le projet est maintenu par une seule personne, et une règle d'affaires doit pouvoir se tester vite, sans base de données réelle ([ADR 0008](https://github.com/jeison-souza/MeepleZone/blob/main/docs/adr/0008-logique-d-affaires-testable.md)). Trois organisations ont été comparées dans l'[ADR 0009](https://github.com/jeison-souza/MeepleZone/blob/main/docs/adr/0009-organisation-de-la-solution.md) :

- **Un seul projet `Domaine`**, avec un dossier par domaine. Simple au départ, mais rien n'empêche un domaine d'utiliser les classes d'un autre : la séparation repose sur la discipline.
- **Les couches classiques** (présentation, affaires, données). Connu, mais les domaines se mélangent dans chaque couche.
- **Un projet par domaine**, chacun avec son projet de tests, et un regroupement `Systeme` pour ce qui est transversal. C'est l'option retenue : le compilateur interdit les références entre domaines, et chaque test vit à côté de ce qu'il vérifie.

La première proposition (14 octobre) était la première option. Elle a été revue le lendemain, en examinant ce que le compilateur pouvait faire respecter à la place de la discipline.

Concrètement, chaque domaine (`Compte`, puis `Jeux`, `Collection`, `Amitie`) a un projet pour sa logique et un projet frère de contrats, que le site peut voir. Les entités sont de simples classes de données, partagées, sans règles. Les règles vivent dans des gestionnaires, un par opération, derrière une façade mince par domaine. L'accès aux données passe par des interfaces de dépôt; seul le projet `Systeme/Donnees` connaît EF Core. Les dépendances vont toujours des domaines vers `Systeme`, jamais l'inverse.
Le coût est connu : plus de projets, des entités qui n'emportent pas leurs règles avec elles, et une frontière entre « petit et partagé » et « propre à un domaine » qui relève du jugement. La façade s'écarte aussi en partie des principes SOLID; l'écart est accepté pour garder le site simple.

La structure n'est pas restée figée en contact avec le code. Pour écrire le premier domaine, `Compte`, il a fallu décider où loger le type du résultat d'un enregistrement renvoyé par le dépôt, un type que le domaine et l'implémentation EF doivent tous deux connaître sans se référencer. La réponse a été un nouveau projet, `Systeme/Depots`, qui contient les interfaces de dépôt et le vocabulaire de leurs signatures. L'ADR 0009 a été amendé en conséquence, deux fois en tout (21 octobre et 8 novembre), plutôt que d'être réécrit après coup.

## 3. Un obstacle concret : un menu qui ne s'ouvrait pas

### Le contexte

La structure de base du module de connexion était en place : connexion par Google et Microsoft, retour à la page d'origine, et un menu de l'utilisateur dans la barre du haut (un avatar, avec « Mon profil » et « Se déconnecter »). Au moment des essais, le menu ne s'ouvrait pas.

Pour la suite, il faut présenter l'autre acteur de cette histoire. Claude est l'assistant d'IA qui m'accompagne dans la construction de cette application. Il s'exécute dans une machine virtuelle isolée de mon poste, d'où il lit et modifie le code, lance l'application et pilote un navigateur. Dans cette période, une instance de Claude s'occupait exclusivement de l'interface utilisateur, c'est-à-dire de son apparence et de son comportement, tandis que je travaillais sur le projet EF, côté serveur.

### Le symptôme

Un clic sur l'avatar ne faisait rien : aucune erreur dans la console du navigateur, aucun trafic réseau, aucun message dans le journal du serveur. Le comportement était le même dans deux navigateurs : celui de Claude, dans la machine virtuelle, et le mien, sur mon propre poste.

### Les fausses pistes

Le composant d'autorisation autour du menu, le formulaire de déconnexion avec son jeton antiforgery, la barre du haut, le rendu interactif de Blazor, le navigateur. L'absence d'erreur a rallongé la recherche : un composant qui ne répond pas, sans message, ne donne aucun indice sur ce qui manque.

En parallèle, l'application démarrait de façon intermittente dans la machine virtuelle : un processus actif, sans journal et sans port ouvert. La cause n'a jamais été trouvée. Pendant plus de quatre heures, l'instance de Claude chargée de l'interface utilisateur a cherché seule, en relançant l'application, pendant que je terminais mon module du côté serveur.

Quand j'ai eu fini, j'ai interrompu la boucle, pour comprendre ce qui se passait et donner de nouvelles instructions : avant de relancer quoi que ce soit, Claude devait expliquer le problème, et j'allais reproduire les essais sur mon propre PC. À partir de là, le problème a été débogué à deux, en équipe.

### Ce qui a fonctionné

- **Reproduire ailleurs.** Lancé dans mon environnement de développement, avec une vraie connexion Google, le symptôme était présent : ce n'était donc pas un problème de la machine virtuelle.
- **Isoler par comparaison.** Claude a écrit une page de test temporaire avec plusieurs menus côte à côte (activateur textuel, avec icône, avec avatar, avec le contenu complet). Je les ai essayés un à un et j'ai rapporté lesquels s'ouvraient. Une seule ronde a suffi : les menus à activateur standard s'ouvraient; ceux à contenu d'activateur personnalisé ne s'ouvraient pas, le bouton montrant l'effet visuel du clic sans ouvrir le menu.
- **Lire la documentation avant d'abandonner.** Quand Claude a parlé de renoncer à l'avatar, j'ai demandé de vérifier d'abord si une configuration n'avait pas été oubliée. Le site de documentation ne se chargeait pas avec ses outils; il a donc lu le code source du composant. Dans cette version de MudBlazor, un menu à activateur personnalisé ne branche pas le clic : le contenu doit appeler lui-même `ToggleAsync`.
- **Corriger et vérifier.** Un bouton natif autour de l'avatar appelle `ToggleAsync`. J'ai confirmé que le clic, la touche Entrée et la touche Espace ouvrent le menu.

Un second assistant, chargé de la configuration des permissions du poste, a pour sa part ajusté des règles qui ralentissaient les modifications de fichiers.

### Une erreur de Claude, rattrapée

Claude avait attribué au menu une « protection de 50 ms » contre le double clic. La lecture du code montrait autre chose : elle ignore le clic synthétique que le navigateur produit après une activation au clavier. Un double clic ouvre puis ferme le menu, comme dans les menus standards. L'erreur est apparue en comparant la description de mon essai au code. Claude avait aussi supposé un risque avec la touche Espace; mon essai l'a écarté. L'explication d'un comportement se vérifie dans la source et par un essai, pas dans la mémoire de l'assistant.

La veille, un problème de la même famille : annuler la connexion chez Google produisait une erreur 500, alors que le même geste chez Microsoft était géré. Le refus explicite de Google passe par un événement distinct de celui des échecs. Il a été découvert en essayant le vrai parcours, non par un test automatisé : un test écrit par Claude aurait encodé son hypothèse, qui était fausse. Le test est venu ensuite, pour figer le comportement une fois compris.

### Ce qui a été retenu

- Un composant « mort » sans erreur signale souvent un mécanisme jamais branché; comparer des variantes côte à côte est plus rapide que chercher la faute dans l'ensemble.
- Ne pas insister sur une même solution qui semble tourner en boucle : demander de l'aide. Travailler en équipe apporte toujours des points de vue différents et facilite la recherche d'une solution.
- Rester aux commandes : la responsabilité du projet reste à la personne, pas à l'assistant. C'est elle qui arrête les boucles, exige une explication avant toute nouvelle exécution et valide chaque étape dans son propre environnement.
- Des rôles différents valent mieux qu'un seul assistant : l'un écrit et diagnostique, un autre règle le poste, et la personne arbitre et teste. Les affirmations d'un assistant se vérifient, car il peut se tromper avec assurance.

## 4. Ce que le schéma révèle du domaine

Le [schéma de données](https://github.com/jeison-souza/MeepleZone/blob/main/docs/schema/schema-donnees.md), onze tables, a surtout obligé à trancher des questions de vocabulaire. Quelques-unes en disent plus sur le domaine que sur la base.

- **Une personne n'est pas un compte chez un fournisseur.** `utilisateur` et `identite_externe` sont deux tables. Les collections et les amitiés se rattachent à la personne, jamais à Google ou à Microsoft : quelqu'un qui se connecte tantôt avec l'un, tantôt avec l'autre reste le même utilisateur.
- **Le courriel est facultatif.** L'hypothèse de départ était qu'un compte a toujours un courriel. Elle est tombée : un fournisseur peut ne pas en communiquer, et la connexion ne doit pas en dépendre. Le courriel sert à joindre la personne et à la retrouver pour une demande d'amitié, il n'identifie pas la connexion. Le compte sans courriel est donc permis, avec un ajout confirmé par code plus tard (ADR 0004).
- **Un jeu est un fait, une possession est personnelle.** Le catalogue est partagé, sans copie : chaque jeu y existe une fois. Ce qui est propre à une personne (la note, le commentaire, le nombre de parties) vit dans `possession`, la liaison entre l'utilisateur et le jeu. Il n'y a pas de table « collection » : la collection est l'ensemble des possessions.
- **Un jeu vient de BoardGameGeek ou d'un utilisateur.** Un jeu créé à la main commence « en attente » et n'est visible que de son créateur jusqu'à sa validation. Les jeux importés restent en lecture seule, ce que les conditions d'utilisation de l'API de BoardGameGeek exigent.
- **Une amitié est symétrique, une demande ne l'est pas.** La demande sait qui a demandé; l'amitié acceptée n'existe qu'une fois, dans un ordre fixe (`utilisateur_a_id < utilisateur_b_id`). La personne qui demande ne voit jamais si l'autre l'a ignorée, ni si le courriel saisi correspond à un compte : sans cela, la demande révélerait qui est inscrit.
- **Ce qui appartient à la personne, et ce qui appartient à tous.** Les clés étrangères vers `utilisateur` ne se comportent pas toutes de la même façon, et il a fallu décider, donnée par donnée, de quel côté elle tombe. Les possessions, les amitiés, les demandes d'amitié et les connexions externes sont personnelles : elles disparaissent avec le compte (`ON DELETE CASCADE`). Les jeux et les couvertures qu'une personne a créés, eux, servent maintenant à tout le monde : la base refuse de les supprimer avec elle (`ON DELETE RESTRICT` sur `cree_par_id`) et le code les rattache à un compte technique « Utilisateur supprimé » (ADR 0006).

## 5. Ce qui reste incertain

- Les migrations existent pour trois des onze tables (`utilisateur`, `identite_externe` et `annexe`), validées contre la base de développement; le reste du schéma n'a pas encore été confronté à une base réelle.
- La modification du profil et l'enregistrement de la photo restent à faire.
- La mise en production de la connexion (adresses de retour, en-têtes du proxy) reste à faire.
- Le déploiement continu n'est pas encore en place.
- Le placement du volet social dans les jalons sera réévalué à la fin de ce jalon.

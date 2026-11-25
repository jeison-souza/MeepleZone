---
title: "Jalon 3 : le développement avance, et la production aussi"
date: 2026-11-24
tags: ["Jalon 3", "Développement", "Production", "PostgreSQL"]
categories: ["Livrable", "Carnet de route"]
---

Cet article répond aux trois questions du Jalon 3 : ce qui a fonctionné depuis le Jalon 2, un moment difficile (la mise en production) et ce que je ferais autrement. Deux autres articles racontent à part [un problème de gestion du code source]({{< relref "/posts/jalon-03/04-une-ligne-de-trop-reecrire-la-branche-principale.md" >}}) et [un problème de logique localisé avec le débogueur]({{< relref "/posts/jalon-03/05-retour-page-adieu-debogueur" >}}).

## 1. Ce qui a fonctionné

**La connexion enregistre la personne.** Depuis le Jalon 2, le login Google et Microsoft crée et retrouve l'utilisateur dans la base (étapes C1 à C9, [issue #9](https://github.com/jeison-souza/MeepleZone/issues/9)), avec un écran de choix quand un compte semble déjà exister et un avatar fait des initiales de la personne.

**Les comptes se gèrent.** Une personne peut lier plusieurs connexions à son compte ([issue #59](https://github.com/jeison-souza/MeepleZone/issues/59)) et supprimer son compte et ses données ([issue #57](https://github.com/jeison-souza/MeepleZone/issues/57)). La suppression a été vérifiée de bout en bout dans l'interface, sur quatre cas : un jeu non validé, un jeu validé, un jeu utilisé par une autre personne et un administrateur protégé.

**Le catalogue et la collection existent.** Les tables sont migrées ([issue #11](https://github.com/jeison-souza/MeepleZone/issues/11)), le domaine Collection est écrit ([issue #10](https://github.com/jeison-souza/MeepleZone/issues/10)) et la page « Ma collection » l'utilise (issues [#37](https://github.com/jeison-souza/MeepleZone/issues/37) et [#38](https://github.com/jeison-souza/MeepleZone/issues/38)), avec ses filtres ([issue #13](https://github.com/jeison-souza/MeepleZone/issues/13)), une recherche sans accents et tolérante aux fautes de frappe, et la détection des jeux semblables à la création d'un jeu ([issue #134](https://github.com/jeison-souza/MeepleZone/issues/134)).

**Le schéma a été validé contre une vraie base.** Treize scripts SQL et 236 cas, publiés dans `docs/validation` ([issue #88](https://github.com/jeison-souza/MeepleZone/issues/88)), disent si PostgreSQL applique vraiment les contraintes annoncées par le schéma. Chaque script s'exécute dans une transaction annulée à la fin. Le détail est dans l'article [Tester EF Core : le modèle sans base, le schéma avec une vraie base]({{< relref "/posts/jalon-03/03-tester-ef-core-modele-sans-base-schema-avec-base.md" >}}).

**Les décisions sont écrites.** Cinq nouveaux ADR (0011 à 0015, [issue #65](https://github.com/jeison-souza/MeepleZone/issues/65)) documentent la concurrence et les contrats de dépôt, l'identité dans la session, le contexte de données de courte durée, la navigation entre entités et la recherche sans accents.

**L'application tourne en production.** La version actuelle est publiée. Le fournisseur Google est passé du mode test au mode production : n'importe quel compte peut se connecter, pas seulement le mien. La connexion Microsoft fonctionnait déjà. Le prochain chapitre raconte ce que cette mise en production a demandé.

**Un historique lisible.** Trente PR ont été fusionnés entre le 11 et le 18 novembre et dix issues fermées, une branche et un PR par étape, avec une description et un `Refs` ou un `Closes`. Un exemple : l'écran de choix du compte probable ([PR #118](https://github.com/jeison-souza/MeepleZone/pull/118)) tient en six *commits*, chacun un changement compréhensible (déplacer les pages du compte, la liste fermée des fournisseurs, l'écran, le routeur, la transmission du fournisseur, la création forcée).

Le travail avec l'IA a aussi fait l'objet de deux articles : [premiers pas et précautions]({{< relref "/posts/jalon-03/01-utiliser-ia-premiers-pas-et-precautions.md" >}}) et [travailler en équipe]({{< relref "/posts/jalon-03/02-utiliser-ia-travailler-en-equipe.md" >}}).

## 2. Un moment difficile : la mise en production

Les 15 et 16 novembre, j'ai déployé en production la première version fonctionnelle du site (avec connexion, base de données et enregistrement des utilisateurs), à la main : publier l'application, la copier dans le contenant Docker, le redémarrer, puis tout vérifier de l'extérieur. Sur le papier, une soirée de travail. Dans les faits, chaque étape m'a appris quelque chose, et chaque problème réglé en cachait un autre.

### Les petits accrocs de configuration

Le site était déjà en ligne depuis le Jalon 2, mais ce n'était qu'un squelette : une page d'accueil qui ne se connectait ni à une base de données ni à quoi que ce soit d'autre. La connexion avec Google et Microsoft, la base PostgreSQL et l'enregistrement des utilisateurs sont arrivés avec le Jalon 3. Publier cette nouvelle version, c'était donc la première fois que l'application avait vraiment besoin de son environnement : le proxy, le pare-feu, la base. Les premiers accrocs viennent de là.

Le premier est de ceux qu'un poste de développement ne montre jamais. Après une mise à jour, le site refusait les formulaires et les personnes connectées perdaient leur session. Une mise à jour détruit le contenant et en crée un nouveau : les clés qui protègent les cookies de connexion, gardées à l'intérieur, disparaissaient avec lui. Je les ai déplacées vers un espace de stockage extérieur au contenant, qui survit aux mises à jour ([PR #135](https://github.com/jeison-souza/MeepleZone/pull/135)).

Le deuxième est venu de l'adresse de retour du login. Google et Microsoft refusaient la connexion : dans l'URL de l'appel au fournisseur, le paramètre `redirect_uri` commençait par `http` au lieu de `https`. Derrière un proxy, l'application ne voit que du HTTP, et elle ne croit l'indication « le client est en HTTPS » que si elle vient d'un proxy qu'elle connaît, une adresse déclarée dans la configuration. L'adresse que j'y avais mise n'était pas la bonne : le contenant du proxy a **deux cartes réseau**, et l'application voyait l'adresse de l'autre. En passant le journal de cette partie de l'application au niveau `Debug` (`Logging__LogLevel__Microsoft.AspNetCore.HttpOverrides`), j'ai vu l'adresse qu'elle voyait vraiment. Il a suffi de remplacer la valeur, et l'adresse de retour s'est mise à afficher `https`.

### Quand la connexion n'enregistre plus personne

Le login passait enfin chez Google et Microsoft, mais la personne n'arrivait pas dans le site : elle revenait sur la page « La connexion a échoué » de MeepleZone.

{{< imagerepliable summary="Voir la page d'erreur" src="connexion-a-echoue.png" alt="La page « La connexion a échoué » de MeepleZone : un message d'avertissement et les boutons « Continuer avec Google » et « Continuer avec Microsoft »." >}}

Dans le journal, l'enregistrement de l'utilisateur dans la base échouait : la connexion à la base n'aboutissait pas. Ce problème n'est apparu qu'avec le [PR #116](https://github.com/jeison-souza/MeepleZone/pull/116), qui ajoute cet enregistrement au moment de la connexion : avant, le site n'écrivait rien dans la base.

Le pare-feu était le premier suspect, et avec raison : une règle fixait le port **source** au port de la base, au lieu d'accepter n'importe quel port source. Une fois la règle corrigée, le site a pu communiquer avec la base. Problème réglé, ou du moins je le croyais.

Dès les premiers essais, l'erreur est revenue, cette fois sous la forme d'une exception de dépassement de délai : « Timeout during connection attempt ». J'ai repris l'enquête. En répétant dix essais d'ouverture de connexion, à une seconde d'intervalle, depuis le contenant du site, j'ai vu le motif : un essai passait, les suivants tombaient en *timeout*. Le système de prévention d'intrusion du pare-feu prenait ces connexions rapprochées pour un balayage de ports et les bloquait, et son journal l'a confirmé. Le correctif a été d'ajouter une exception : la détection de balayage de ports est supprimée pour le serveur du site seulement, et le reste de la protection reste actif. Après cela, dix essais sur dix ont réussi, et le site a enregistré l'utilisateur.

### La lenteur, le vrai problème

Le site écrivait enfin dans la base, mais lentement, puis un `COMMIT` a dépassé 30 secondes. Ce n'était plus de la configuration. Le test d'écriture durable fourni avec PostgreSQL (`fsync`) a montré des écritures qui duraient jusqu'à 8 secondes. Le stockage partagé, à disques mécaniques, reçoit de l'écriture continue de plusieurs autres contenants, et chaque validation d'une transaction attend la confirmation du disque. Je n'ai pas encore identifié lesquels sont responsables.

Mon palliatif actuel est de dire à PostgreSQL de ne plus attendre cette confirmation à chaque validation (`synchronous_commit = off`). Après une panne du serveur, les toutes dernières transactions peuvent être perdues, sans corrompre la base. Les écritures observées sont revenues à quelques millisecondes (un échantillon, pas une moyenne). Mais ce n'est **pas** une solution : le problème est à l'infrastructure, il n'est pas réglé, et un changement d'infrastructure va bientôt s'imposer, par exemple un volume plus rapide réservé aux bases. Le retour au réglage normal est suivi dans l'[issue #168](https://github.com/jeison-souza/MeepleZone/issues/168).

## 3. Deux transcriptions : une décision d'architecture et un débogage guidé

### La première transcription : la décision de l'ADR 0015

Deux des livrables du Jalon 3 sont des transcriptions de sessions de travail avec l'assistant, choisies parce qu'elles montrent quelque chose. La première montre une décision d'architecture qui change pendant la conversation.

Elle est publiée ici : [extrait de la session sur l'ADR 0015](https://gist.github.com/jeison-souza/0876ce2255bf4a8ed3aa88f46d1e4b14) (affichage plus lisible : [version à consulter](https://gisthost.github.io/?0876ce2255bf4a8ed3aa88f46d1e4b14/index.html)). L'extrait va de 13 h 56, le 13 novembre, à 12 h 03, le 14 novembre, heure du Québec.

L'ADR décrivait la recherche sans accents avec l'extension `unaccent`. En relisant, j'ai posé une question sur les fautes de frappe, et l'option `pg_trgm` (la recherche par trigrammes, qui tolère les fautes) s'est révélée absente du tableau des options. Ce qui change le plus la décision tient en une phrase de la conversation :

> « Puisque les jeux pourront être saisis à la main, une recherche par similarité me paraît importante : elle éviterait de créer des doublons quand la personne ne tape pas correctement sa recherche. »

Le motif principal est devenu la prévention des doublons, et non le confort de la recherche. Plus loin, l'assistant avait proposé de décrire l'ADR « en deux temps », la décision puis l'état de la mise en œuvre. Je l'ai corrigé :

> « Je ne suis pas sûr que le concept de “deux temps” s'applique vraiment à un ADR : ça fait bizarre. Je pense qu'un ADR décrit seulement ce qui a été décidé : aucun autre ADR ne parlait de ce qui était déjà mis en œuvre ou non. »

L'ADR consigne donc la décision, et l'état d'avancement vit ailleurs : l'[issue #134](https://github.com/jeison-souza/MeepleZone/issues/134), réalisée ensuite.

Les passages cités sont traduits du portugais, la langue de la conversation. Certains éléments de l'extrait ont été remplacés par des marqueurs entre crochets (chemins, noms de comptes, adresse du serveur privé, identifiants GitHub), et les avis automatiques de l'outil ont été retirés. Rien d'autre n'a été modifié.

### La deuxième transcription : un débogage guidé

La seconde montre une autre façon de travailler avec l'assistant : le débogage du [bogue #159](https://github.com/jeison-souza/MeepleZone/issues/159) (la page d'adieu qui réapparaît après une nouvelle connexion). Ici, c'est la personne qui développe qui déboguait dans Visual Studio, qui posait les points d'arrêt, lisait le panneau des variables et la pile d'appels, puis écrivait le test et le correctif. L'assistant guidait, expliquait le pourquoi de chaque étape, vérifiait les captures d'écran et relisait le résultat, sans écrire le code de production à sa place.

Elle est publiée en entier, en français : [transcription du débogage du bogue #159](https://gist.github.com/jeison-souza/d05c2d38df0d8727f8a020bb4f2b0a45) (affichage plus lisible : [version à consulter](https://gisthost.github.io/?d05c2d38df0d8727f8a020bb4f2b0a45/index.html)). Elle va de 11 h 17, le 19 novembre, à 23 h 21 (heure du Québec). Le contenu des notes de mémoire du projet qui y apparaissaient a été remplacé par une mention, ainsi qu'une capture d'écran qui montrait d'autres personnes; les mêmes marqueurs et le même retrait des avis automatiques s'appliquent. Le récit du débogage lui-même, avec les captures, est dans l'article [Un retour vers la page d'adieu]({{< relref "/posts/jalon-03/05-retour-page-adieu-debogueur" >}}).

## 4. Ce que je ferais autrement

- **Choisir une technologie plus simple, avec moins de couches.** Deux choix de départ, un pour l'interface et un pour la partie serveur, ont le même défaut : ils ajoutent des couches que je n'avais pas vraiment le temps de maîtriser.
  - *L'interface.* J'ai retenu MudBlazor sur la recommandation de l'IA, en me disant que j'aurais le temps d'approfondir une bibliothèque que je connais mal. Ce temps n'est pas venu, et il viendra peut-être plus tard, après le cours. Résultat : j'ai dû confier tout le visuel à l'IA. L'écran fonctionne et il a bonne allure, mais je ne suis pas en mesure de juger si ce qui est produit est bien fait, ni de le corriger avec assurance quand quelque chose cloche. C'est un point faible que je préfère nommer plutôt que cacher.
  - *La partie serveur.* Ici, c'est le contraire : le code C# est mon terrain, et je l'ai quand même rendu plus compliqué qu'il ne devait l'être. J'ai choisi une architecture avec beaucoup de séparations entre les couches : entités, contrats, dépôts, domaines, interfaces. Chacune se défend, et elle a permis d'écrire près de mille tests sans base de données. Mais pour un projet de cette taille, une opération simple traverse les contrats, le domaine, le dépôt et leurs tests avant d'arriver à l'écran, et le coût se paie en vitesse. En recommençant, la partie serveur serait un peu plus simple, avec moins de couches et moins de cérémonie.
- **Apprendre d'abord à mieux travailler avec un outil d'IA.** Je suis une formation accélérée en avançant : j'ai appris à utiliser l'outil en le faisant, au fil des problèmes. Plusieurs pratiques ont fini par s'imposer, comme garder les consignes du projet dans un fichier, tenir des notes de mémoire d'une session à l'autre ou régler les permissions dans la configuration. Je les ai découvertes en cours de route; elles auraient aidé dès le début. Faute de savoir où configurer l'outil pour qu'une consigne reste active dans toutes les sessions, j'ai dû en répéter plusieurs, session après session, et la façon d'organiser les fichiers de mémoire s'est mise en place peu à peu, par essais et erreurs. Si je pouvais recommencer, j'étudierais d'abord les façons de travailler efficacement avec ces outils, avant d'écrire la première ligne du projet.

## 5. Ce qui reste

Pour le Jalon 4 : les amitiés (`demande_amitie` et `amitie`), l'interface du filtre et des jeux semblables, le choix du compte chez Google et Microsoft ([issue #161](https://github.com/jeison-souza/MeepleZone/issues/161), découverte pendant le débogage), deux améliorations laissées de côté lors de la correction du [bogue #159](https://github.com/jeison-souza/MeepleZone/issues/159) (issues [#162](https://github.com/jeison-souza/MeepleZone/issues/162) et [#163](https://github.com/jeison-souza/MeepleZone/issues/163)) et surtout le **déploiement continu** : le déploiement manuel en production est fait, l'automatisation reste à faire. Le problème de lenteur du stockage n'est pas réglé : un changement d'infrastructure est à décider bientôt, et le rétablissement de `synchronous_commit` est suivi dans l'[issue #168](https://github.com/jeison-souza/MeepleZone/issues/168).

---
title: "Une ligne de trop dans l'historique : réécrire la branche principale, deux fois"
date: 2026-11-19
draft: false
tags: ["Jalon 3", "Git", "Flux de travail", "IA"]
categories: ["Livrable", "Carnet de route"]
---

Cet article raconte la fois où, dans le dépôt public du projet, une ligne que je ne voulais pas s'est retrouvée dans l'historique, et ce qu'il a fallu faire pour la retirer. L'incident est arrivé deux fois, à trois semaines d'intervalle, et la deuxième fois m'a appris plus que la première.

## Le contexte : un assistant qui obéit à deux voix

Une partie du code et des documents du projet est rédigée avec l'aide d'un assistant d'IA (Claude), dans des sessions que je dirige. L'usage est assumé : il est raconté dans ce blogue et les transcriptions de sessions sont publiées. Il y a pourtant une chose que j'avais décidée : les messages de *commit* et les descriptions de PR du dépôt public ne portent pas de mention de l'IA. L'attribution passe par le blogue, pas par une ligne dans chaque *commit*.

Le problème, c'est que l'assistant reçoit de son propre outil, au début de chaque session, un rappel automatique qui lui demande de terminer chaque *commit* par une ligne `Co-Authored-By: Claude…` et chaque PR par un petit pied de page « Generated with Claude Code ». Ma consigne était écrite dans le fichier de règles du projet et dans les notes de l'assistant. Quand les deux voix se contredisent, c'est parfois le rappel qui gagne.

## Première fois : le PR #105 (20 octobre)

Vers 22 h, en revoyant un PR déjà fusionné, je remarque la ligne : trois *commits* du PR #105 (le courriel devenu facultatif) portent `Co-Authored-By: Claude`, et la description du PR a son pied de page. Je réagis avec agacement, et la réponse de l'assistant est franche : la règle était écrite, il a suivi le rappel de l'outil au lieu de la règle. Je soupçonne que la compaction de la conversation (le moment où la session résume ce qui précède pour ne pas dépasser sa mémoire) lui avait fait perdre le fil de ses propres notes.

Les trois *commits* étaient déjà sur `main`. Je choisis de les retirer plutôt que de laisser la ligne. Le plan, décidé vers 22 h 05 : ramener `main` juste avant le PR #105 et refaire le travail sans la ligne.

Les étapes, dans l'ordre :

1. **Vérifier ce qui protège `main`.** Un jeu de règles interdit le *force push*. C'est une protection de sécurité du dépôt : je la désactive moi-même, à la main, dans les réglages de GitHub.

   ```bash
   gh api repos/…/branches/main
   gh api repos/…/rulesets
   ```

2. **Confirmer que le dépôt local est propre**, car la prochaine commande détruit des modifications :

   ```bash
   git status --short --branch
   ```

3. **Revenir en arrière en local**, au dernier *commit* avant le PR #105 :

   ```bash
   git reset --hard 00544a3
   ```

4. **Pousser, avec une sécurité.** L'option `--force-with-lease` est le point important : le *push* n'écrase la branche distante que si elle est encore à `14e1913` (la fusion du PR #105). Si quelqu'un avait poussé entre-temps, Git aurait refusé au lieu d'écraser son travail. Il était 22 h 11.

   ```bash
   git push --force-with-lease=main:14e1913 origin main
   ```

5. **Refaire le travail :** créer un nouveau *worktree* à partir de `00544a3`, reprendre le contenu de l'ancien *commit* sans son message, puis faire un nouveau *commit* propre.

   ```bash
   git worktree add … -b features/courriel-facultatif 00544a3
   git cherry-pick --no-commit 2270a39
   ```

   Le jeu de règles a été réactivé tout de suite après; il reste actif en tout temps, et je ne le désactive que le temps d'une réécriture de ce genre.

Une limite que l'assistant m'avait annoncée dans son plan s'est confirmée : le PR #105 existe toujours sur GitHub, avec ses anciens *commits*. GitHub ne permet pas de supprimer un PR fusionné; on peut seulement en modifier la description. L'affaire s'arrêtait là, et je la croyais réglée.

## Deuxième fois : les PR #118 et #121 (10 novembre)

Le soir du 10 novembre, même chose, mais en plus grand. Une session avait livré l'écran de choix d'un compte probable (PR #118, six *commits*), puis l'avatar avec les initiales (PR #121). Les neuf *commits* portaient la ligne, et les descriptions des deux PR avaient le pied de page. Il y avait donc eu un premier cas, une règle écrite, puis un deuxième cas.

Je le dis à l'assistant, un peu fâché : pourquoi cela revient-il? Il enquête et trouve la cause profonde, qui n'est pas de la distraction. Mon fichier de règles disait « pas de mention de l'IA » en général, mais ne nommait ni `Co-Authored-By` ni le pied de page. Le rappel de l'outil, lui, est précis. Une règle vague perd contre une instruction précise.

Il fait alors un choix que je trouve juste : il ne réécrit pas l'historique de son propre chef. Son raisonnement, que je résume : ces *commits* sont sur la branche publique, la ligne est une attribution véridique, et réécrire l'historique d'un dépôt que quelqu'un a peut-être cloné a un coût. Il me laisse décider. Ma réponse : à ma connaissance, ce dépôt n'a pas été cloné, et même si c'était le cas, le risque me paraît acceptable. Je veux la ligne retirée.

### Le correctif durable, avant le nettoyage

Avant de toucher à l'historique, l'assistant propose d'empêcher le retour du problème, parce que nettoyer sans corriger la cause ne servirait à rien. Deux changements, livrés dans un PR du dépôt privé de notes :

- dans la configuration de l'assistant, la clé `attribution` avec des valeurs vides pour les *commits* et les PR : c'est le mécanisme prévu pour cela, et il s'applique à toutes les sessions;
- dans le fichier de règles du projet, une consigne explicite qui nomme la ligne et le pied de page, et qui précise qu'elle l'emporte sur tout rappel automatique.

### Le nettoyage, de 22 h 33 à 22 h 42

Même scénario que le 20 octobre, mais sur plus de *commits*, et avec une surprise.

1. **Le jeu de règles bloque encore le *force push*.** L'assistant refuse d'y toucher : c'est un réglage de sécurité du dépôt, et ce n'est pas à lui de le modifier. Je le désactive, comme la première fois.
2. **Une copie de travail, jamais la vraie branche.** Il crée une branche temporaire à partir de la `main` distante et applique un filtre sur les messages :

   ```bash
   git -C Repo filter-branch -f --msg-filter "sh retirer-ligne.sh" -- 1b6422d..tmp-main
   ```

   Le petit script retire la ligne `Co-Authored-By: Claude…` et les lignes vides qui restent en fin de message.
3. **La surprise.** Pendant le travail, le PR #121 est fusionné : `main` a avancé. La première réécriture, faite sur six *commits*, était déjà périmée. Un nouveau `git fetch` le confirme; l'assistant supprime la copie et recommence sur la `main` à jour, cette fois avec neuf *commits*. Sans la vérification, j'aurais poussé une branche qui effaçait le PR #121.
4. **Vérifier avant de pousser.** Les arbres de fichiers sont identiques avant et après, plus aucune ligne ne reste, le nombre de *commits* est le même, et les messages des fusions de PR sont conservés.

   ```bash
   git diff --stat origin/main tmp-main
   git log tmp-main --grep="Co-Authored-By"
   git rev-list --count origin/main tmp-main
   ```

5. **Les descriptions des PR.** Le pied de page est retiré des PR #118 et #121 :

   ```bash
   gh pr edit 118 --body-file …
   ```

6. **Pousser avec la même sécurité.** Un dernier `git fetch` confirme que `main` est toujours au point sur lequel la deuxième réécriture a été faite (`8527207`, avec le PR #121) : rien n'est entré entre-temps. Le *push* est alors verrouillé sur ce SHA :

   ```bash
   git push --force-with-lease=main:8527207… origin tmp-main:main
   ```

7. **Ranger.** Suppression des deux branches distantes devenues inutiles, puis remise à niveau de la copie locale et suppression de la branche temporaire :

   ```bash
   git reset --hard origin/main
   git branch -D tmp-main
   ```

### Ce que la réécriture ne peut pas défaire

L'assistant me l'avait dit avant de pousser, et c'est bien le cas :

- tous les identifiants de *commit* changent à partir du premier *commit* touché, y compris ceux des fusions de PR, qui perdent aussi le sceau « Verified » de GitHub;
- les copies de travail et les branches qui partaient de l'ancienne `main` doivent être rebasées;
- **les pages des PR fusionnés gardent les anciens *commits*.** J'ai vérifié : l'API de GitHub liste encore les six *commits* du PR #118 avec la ligne. Elle a disparu de la branche, pas de l'historique du PR. Seul le soutien technique de GitHub pourrait y toucher.

## Ce que j'en retiens

1. **Une règle écrite pour une personne perd contre un rappel automatique.** Ce qui a vraiment réglé le problème, c'est le réglage de configuration (`attribution` vide), pas ma consigne dans un fichier. Quand une règle doit tenir, elle va dans la configuration de l'outil, et elle nomme précisément ce qu'elle interdit.
2. **Avant un *force push*, trois gestes.** Savoir ce qui protège la branche et ne désactiver cette protection que sciemment, le temps de l'opération; verrouiller le *push* sur l'état attendu (`--force-with-lease` avec le SHA, jamais `--force` nu); vérifier que les arbres de fichiers sont identiques. Le 10 novembre, ces habitudes ont évité d'effacer un PR fusionné pendant le nettoyage.
3. **Réécrire l'historique n'efface pas tout.** Les pages de PR, les copies déjà clonées et les branches basées sur l'ancien historique gardent la trace. Mieux vaut empêcher l'erreur que la corriger.
4. **Vérifier sur la référence distante, pas sur sa copie locale.** Pendant l'enquête, une session m'a dit que la branche principale était propre, et une recherche par message de *commit* faite depuis ma copie locale a donné le même faux négatif, parce que cette copie était déjà sur la version réécrite. Seule une requête sur le dépôt distant (GitHub) a montré ce qui restait.

## Pourquoi je préfère ne pas mettre la ligne

Retirer la ligne n'efface pas l'usage de l'IA dans le projet, et ce n'est pas pour le cacher. J'utilise l'IA pour rédiger certains textes et pour m'aider à organiser le travail : préparer les *commits* et les PR, gérer les tâches du tableau du projet, entre autres. Mais tout n'est pas de l'IA dans ce projet, et une ligne `Co-Authored-By` ne dit rien de cela : elle ne précise pas qui a fait quoi.

Aujourd'hui, le code généré par IA reste un sujet sensible, presque un tabou. J'ai l'impression, peut-être fausse, que dès que quelqu'un voit « Co-Authored-By: Claude », il conclut d'emblée que tout a été fait en *vibe coding* et que je n'ai touché à rien. Ce n'est pas le cas. Je préfère donc le dire de façon organisée, dans un article comme celui-ci ou dans un document où j'ai la place d'expliquer qui a fait quelle partie, plutôt que dans une ligne de pied de page, sans nuance. C'est aussi pour cela que les transcriptions des sessions sont publiées avec le blogue.

## Deux petits accrocs, la même semaine

Ils n'ont rien de dramatique, mais ils ont la même racine : plusieurs copies de travail et un dépôt distant qui avance pendant que l'on travaille.

**Un `checkout` refusé (16 novembre).** En ouvrant un PR de notes, le script s'est arrêté : `Your local changes to the following files would be overwritten by checkout`. La branche locale était en retard sur le dépôt distant, et un fichier avait été modifié des deux côtés. `git merge-base main claude/main` a donné exactement le *commit* de ma `main` locale : elle était un ancêtre de la branche distante. Plutôt que forcer, j'ai créé une copie de travail partie directement de la branche distante :

```bash
git worktree add Worktrees/nota-xxx-wt -b features/nota-xxx claude/main
```

**Emporter du travail non commité (17 novembre).** Même scénario, avec des modifications sur le disque dans une copie restée sur une branche déjà fusionnée. Une seule commande a créé la nouvelle branche en emportant les modifications, avec une fusion à trois voies, sans `stash` ni `reset` :

```bash
git switch --merge -c features/nota-yyy claude/main
```

Dans les deux cas, le refus de Git était une protection, pas un obstacle.

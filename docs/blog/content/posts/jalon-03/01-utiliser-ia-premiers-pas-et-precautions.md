---
title: "Utiliser l'IA : premiers pas et précautions"
date: 2026-11-13
draft: false
tags: ["Intelligence artificielle", "Processus", "Sécurité", "Jalon 3"]
categories: ["Carnet de route"]
---

## 1. Une première fois, et un choix d'y aller lentement

Le cours permet et encourage l'emploi de l'IA, à condition de dire comment elle est utilisée et de savoir expliquer chaque choix à l'entrevue. MeepleZone est la première occasion d'utiliser une IA de ce niveau. Jusque-là, l'expérience se limitait aux agents conversationnels : poser une question, obtenir une réponse, la copier ailleurs. L'outil utilisé ici est Claude Code, un assistant de programmation qui lit le dépôt, propose, modifie des fichiers et exécute des commandes, sous des permissions que le développeur contrôle. C'est une autre sorte d'outil, et la différence se sent dès les premières sessions. Une conversation consacrée au code est beaucoup plus puissante, et le gain de temps est évident pour un projet mené par une seule personne, avec un calendrier serré.

Cette puissance a justifié un choix délibéré : y aller lentement, en mode apprentissage, c'est-à-dire apprendre à utiliser l'outil et à travailler avec lui. Plutôt que de tout déléguer pour aller vite, l'idée était de découvrir peu à peu les avantages, les inconvénients et les dangers de ce genre d'outil, en apprenant ses règles (permissions, outils, fichier de consignes, mémoire) au fil des sessions. Ce qui suit est le récit de cet apprentissage, erreurs comprises, et non un mode d'emploi.

Dès les premières configurations (la connexion avec les fournisseurs d'identité, par exemple), le mode de réponse éducatif de l'application, qui explique ce qu'il fait, a été posé comme règle : chaque étape est expliquée et discutée à fond, en explorant chacune des solutions alternatives et en comprenant bien chaque choix technique avant de passer au code. La raison est simple : c'est un projet de cours, et chaque décision devra être défendue à l'entrevue.

## 2. Le premier faux pas : des documents lus trop largement

La plus mauvaise expérience est venue tôt. Juste après l'installation de l'application Claude Code, pendant les premières interactions et avant toute configuration détaillée des limites, l'assistant a lu une grande quantité de documents personnels du développeur, dont des notes sur son infrastructure domestique, et une partie de leur contenu a quitté le poste vers les serveurs du fournisseur sans que ce soit voulu. Aucune attaque n'était en cause : l'outil faisait ce qu'il était censé faire, avec une liberté que personne n'avait encore encadrée.

La réaction a d'abord été la panique, et la question « que faire maintenant? » a occupé toute une fin de semaine. Des limites ont été imposées, des explications ont été demandées pour approfondir les connaissances, et des barrières ont été construites, avec l'aide de l'assistant lui-même, qui a participé à la conception des solutions. Elles forment aujourd'hui les précautions décrites plus bas.

Ce qui en est retenu :

- des garde-fous sévères dans la mémoire de l'assistant, plutôt qu'une confiance par défaut;
- des consignes claires, répétées et renforcées;
- une règle explicite : ne faire aucune présomption sur ses droits d'accès.

Un outil fantastique, donc, mais potentiellement dangereux si ses limites ne sont pas posées.

## 3. Les précautions mises en place

- **Secrets.** Les valeurs sensibles (clés, secrets d'application, mots de passe) ne passent jamais par la conversation. Elles restent dans le gestionnaire de secrets du développeur ou dans des variables d'environnement. L'assistant ne lit pas ces valeurs et n'en demande pas.
- **Permissions.** Les approbations sont manuelles, sans contournement. Seules des commandes de lecture sont autorisées d'avance; le reste demande un accord.
- **Dossier de travail.** L'assistant ne lit ni ne modifie rien en dehors des dossiers autorisés sans demande explicite. Un chemin mentionné dans un document n'est pas une autorisation.
- **Deux dépôts séparés.** Le dépôt évalué ne contient que les livrables. Le matériel de travail (contexte, notes, mémoire) vit dans un dépôt privé distinct, où l'assistant ne peut pas écrire directement sur la branche principale.
- **Mode de travail.** Des plans d'action sont élaborés, puis validés avant de passer à leur exécution.
- **Ce qui est publié.** Une attention particulière est portée à ce qui est versé dans le dépôt, afin de s'assurer qu'aucune adresse réseau ni aucun nom de matériel ou de service interne n'y figure.

## 4. Vérifier plutôt que croire

L'assistant est un outil génératif, conçu pour plaire, et les hallucinations ne sont pas exclues. Ce qu'il propose sort de ses connaissances, pas d'une source : toute affirmation technique importante se vérifie dans la documentation officielle ou par un essai réel. Trois exemples du Jalon 2 :

- Une hypothèse sur le tri du texte dans la base de données a été confirmée par un essai : la même comparaison donne deux résultats selon la collation.
- Une règle de collation envisagée a été écartée après la lecture de la documentation de PostgreSQL.
- Pour les menus de MudBlazor, l'assistant était sûr de la façon de les utiliser. La lecture de la documentation a montré qu'il y avait des ajustements à faire. Ce même menu est au centre du problème raconté à la section suivante.

Les pratiques qui en découlent : demander qu'une idée soit réellement vérifiée, compiler et exécuter les tests après chaque modification et contrôler les chiffres et les numéros de version.

L'outil est remarquable, mais pas infaillible. Par moments, l'assistant ouvre des pistes qu'on n'aurait pas imaginées possibles et propose des améliorations. Il faut toutefois vérifier : lui demander de confronter l'idée à ce que d'autres personnes ont fait, s'assurer qu'une approche est vraiment la meilleure option et qu'elle convient à ce qui est en train d'être fait et à l'envergure du projet. Il ne faut pas hésiter à exiger une lecture de la documentation officielle. Les connaissances de base de l'outil fonctionnent, mais elles perdent parfois des détails importants, qui n'existent par exemple que dans une version précise d'un composant. La règle d'or est donc de valider, et de valider beaucoup.

Il vaut aussi la peine de prendre le temps nécessaire pour les décisions : avant le code, tout coûte moins cher à corriger et à modifier. Et si quelque chose paraît louche, il faut poser la question, demander que le concept soit expliqué plus en détail, exiger une justification et une vérification. Le temps est bien investi.

## 5. Les problèmes vécus

- **Un fichier supprimé par une commande.** Une commande de remplacement de texte a effacé un fichier de consignes à cause d'une erreur de droits. Le contrôle de version a permis de le restaurer, et les modifications ont été reprises avec l'outil d'édition, qui montre les différences. La préférence pour cet outil date de là.
- **Trop de structure.** L'assistant a jugé sérieuse une dépendance entre domaines qui n'en était pas une : il présente parfois une contrainte comme un problème incontournable ou trop grand. La discussion a réglé la question, et le principe de ne rien créer avant le besoin sert de contrepoids.
- **Trop de demandes d'approbation.** Un compromis a réduit les interruptions sans perdre le contrôle : des scripts PowerShell ont été créés pour regrouper en une seule commande, avec une seule approbation, plusieurs actions qui se répétaient.
- **Un contexte qui vieillit.** Des notes sont devenues fausses (une décision modifiée, un chiffre dépassé). La note se met à jour au moment de la décision.
- **Un composant sans erreur, et une boucle de plusieurs heures.** Un menu du site, celui de la section précédente, ne s'ouvrait pas, sans aucun message, et l'environnement de l'assistant démarrait l'application de façon intermittente. L'assistant a relancé l'application pendant plus de quatre heures avant que la répétition soit interrompue. Le récit complet est dans [l'article du Jalon 2]({{< relref "/posts/jalon-02/01-architecture-et-donnees.md" >}}).
- **La fatigue de lecture.** Beaucoup de texte à relire. La parade : demander des synthèses courtes et garder les détails dans les documents.

## 6. Les limites de l'outil

- Il peut se tromper avec assurance. Le doute raisonnable fait partie du processus.
- Il ne voit pas les fenêtres d'approbation : il ne peut pas savoir si une commande a été autorisée d'avance ou acceptée par la personne. Il a affirmé à tort qu'une modification passait sans demande, et c'est le développeur, qui voit l'écran, qui doit rester attentif.
- La mémoire contextuelle de l'assistant ne dépasse pas la session : chaque nouvelle conversation repart de zéro. Pour que le contexte tienne à l'échelle du projet et non d'une session, il faut une structure de fichiers Markdown (décisions, conventions, pièges déjà rencontrés, état d'avancement) et des consignes explicites lui demandant de la relire, surtout au début d'une nouvelle session.
- Les délais dépendent de la machine : une machine virtuelle lente a rallongé les compilations.

Ces limites ne remettent pas en cause l'intérêt de l'outil : elles disent où il faut rester attentif, et c'est en les découvrant peu à peu qu'on apprend à s'en servir.

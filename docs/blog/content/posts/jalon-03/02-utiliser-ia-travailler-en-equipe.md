---
title: "Utiliser l'IA : travailler en équipe"
date: 2026-11-14
draft: false
tags: ["Intelligence artificielle", "Processus", "Jalon 3"]
categories: ["Carnet de route"]
---

L'[article précédent]({{< relref "/posts/jalon-03/01-utiliser-ia-premiers-pas-et-precautions.md" >}}) racontait les premiers pas et les précautions. Celui-ci porte sur le quotidien. Il décrit d'abord comment le travail s'organise avec l'assistant, puis ce que cela change dans la vie de tous les jours, et enfin ce que cela coûte et ce que cela veut dire.

## 1. Un collègue, mais une page blanche

L'assistant d'IA est comme un collègue d'équipe dédié, qui ne se fatigue pas et ne juge pas, ce qui est inédit. Il porte attention à des détails que j'oublie parfois de considérer, et je repère parfois ce qu'il laisse passer.

Ce collègue repart toutefois de zéro. Chaque session est une page blanche : l'assistant ne sait rien du projet, ne se souvient pas de ce qui a été fait la veille et doit être alimenté. Ce qu'il connaît au début d'une session se limite à ce qu'il lit. D'où l'importance de commencer chaque nouvelle session par une consigne de départ appropriée, ou par un fichier à lire en premier, pour qu'elle connaisse le projet et sache se situer rapidement. Le contexte est donc un travail à part entière, et, dans ce projet, il a été découpé en deux dépôts : l'un répond aux exigences du cours (les livrables), l'autre garde bien organisés les documents de travail du développement.

- **Un dépôt GitHub** contient les livrables évalués : code, documentation, ADR et blogue.
- **Un dépôt Gitea privé** (un serveur Git auto-hébergé) contient tout le reste : contexte, mémoire, brouillons et notes de travail.

Le dépôt privé garde, entre autres :

- un fichier de consignes à la racine : pile technologique, structure des dossiers, langue (français du Québec), règles de travail;
- les objectifs du projet, les échéances, les exigences du cours et les commentaires d'évaluation reçus;
- des notes de mémoire sur les décisions, les conventions et les pièges déjà rencontrés;
- une feuille de route : l'ordre des étapes, leurs dépendances, ce qui se fait maintenant, ce qui attend, et pourquoi dans cet ordre;
- les décisions prises qui ne sont pas devenues des ADR;
- un registre unique des décisions en suspens, pour reprendre d'une session à l'autre sans rien perdre.

Cette mémoire s'écrit au fil du travail, et la quantité de documents produits en coulisses surprend. Chaque session de discussion technique laisse un compte rendu, et l'assistant se crée des notes pour lui-même. Au fil du temps, des dizaines, puis des centaines de fichiers s'accumulent : des documents de travail qui évoluent, puis deviennent des ADR, des schémas, de la documentation d'architecture et des manuels, presque prêts à livrer et peu retouchés, parce que le sujet a déjà été pensé et discuté. Même ceux qui ne deviennent pas des livrables permettent au travail de garder son fil du début à la fin : plus le contexte est précis, moins l'assistant devine, et le travail peut reprendre des jours plus tard, là où il s'était arrêté, sans perte d'information.

Ces fichiers vieillissent, cependant : il faut les relire et les corriger, sans quoi ils deviennent une source d'erreurs. Heureusement, cet entretien peut lui aussi être confié à l'assistant, sous ma supervision.

## 2. Penser avant d'écrire : les conversations d'architecture

Une bonne part de mon temps passe désormais en conversations avec l'agent d'IA d'architecture plutôt qu'à écrire du code. Le mode de travail ressemble à une séance de remue-méninges : l'architecte et moi, face à face, discutons avant d'écrire. L'architecte n'est pas un autre outil : c'est une session de l'assistant à qui ce rôle a été confié, avec ses propres consignes et son propre dossier de travail. Dans ces sessions, l'architecte et moi tenons le même rôle, celui de co-architectes : chacun propose, conteste et défend ses idées.

L'échange suit un rythme simple. L'architecte propose, je contre-argumente, et il ajuste sa recommandation si l'argument est bon. Demander les pours et les contres, puis les contester, s'est révélé plus utile que demander une réponse. Il me permet aussi de demander ce que d'autres équipes font dans des contextes que je ne connais pas, de lancer des idées différentes, de garder le meilleur de chacune et d'arriver à quelque chose de nouveau qui respecte l'architecture.

Les décisions, elles, restent humaines. L'architecte compare des options et recommande, mais le choix, et sa justification, me reviennent. Mon expérience professionnelle corrige d'ailleurs parfois l'assistant. Par exemple, un risque de dépendances entre deux modules avait été présenté comme sérieux, et l'expérience acquise sur des projets semblables a permis de le ramener à sa juste taille.

Le message clé est de rester aux commandes et de suivre le travail. Une chose est de dire « fais-le » sans s'informer ni garder la maîtrise; une autre est de mener la discussion d'architecture jusqu'à la compréhension. Le responsable du projet, c'est moi, pas l'assistant.

Les premières sessions d'architecture ont duré des jours. Au début, le problème est vu de haut : quelle technologie utiliser, comment, avec quels pours et quels contres. L'objectif était d'arriver à une solution que je juge acceptable et que l'architecte juge viable, sans conséquences indésirables, avant d'écrire les ADR. Même à la fin, le travail n'est pas terminé. D'autres sessions, plus courtes et plus ponctuelles, sont inévitables et naturelles : au fil du trajet, les détails du problème apparaissent et sont attaqués un à un. Ces sessions servent aussi à lire des manuels et à consolider l'information technique.

Une décision se prend donc en session, puis se consigne : registre des décisions, ADR, documentation d'architecture. Elle n'est jamais gravée dans le marbre : une meilleure idée peut survenir le lendemain et l'amender. De petits ajustements chaque jour valent mieux qu'une grande correction à la fin.

## 3. Avancer par petites étapes, avec des sessions spécialisées

**Des étapes petites et vérifiables.** Le plan établi par l'architecture est taillé en petites étapes, chacune assez petite pour être testée et relue avant de passer à la suivante. Chaque étape aboutit à un envoi vers le dépôt, accompagné d'une demande de fusion : par exemple, une table de la base de données avec ses tests, ou une page du site. C'est ce rythme qui rend le travail vérifiable et l'historique Git lisible.

**Des sessions spécialisées.** Je suis allé plus loin en répartissant le travail entre des sessions spécialisées, chacune avec son rôle, son dossier de travail, sa copie de travail Git et ses règles. Par exemple : une session qui valide et supervise le code que j'écris, une autre dédiée à la partie visuelle du site, une autre à la rédaction des issues et au suivi de la feuille de route. Dans MeepleZone, le travail s'est aussi réparti par domaine : une session pour le modèle de données (EF Core), une autre pour les pages du site (interface utilisateur).

**Je coordonne.** C'est moi qui décide ce que fait chaque session, et dans quel ordre. Une session ne sait pas, d'elle-même, ce que les autres ont fait : je leur demande parfois de se parler, en lisant les notes ou les résultats d'une autre, mais c'est moi qui garde le fil.

Il arrive aussi qu'une session attende que j'aie terminé ma part : le code C# de production, par exemple, c'est moi qui l'écris, et la session du modèle de données le vérifie ensuite.

Je teste aussi dans un vrai environnement, j'interromps les boucles et je pose les questions qui font changer de méthode. Pour éviter que les sessions se contredisent ou touchent aux mêmes fichiers, chacune travaille dans sa propre copie de travail Git, et une seule à la fois modifie les notes.

## 4. Un travail qui suit la personne

Récemment, dans une salle d'attente, des idées ont commencé à affluer. Le téléphone a servi à les consigner sur-le-champ, en vrac, à mesure qu'elles arrivaient, pendant que l'assistant les classait et les organisait en artefacts. Il joue ainsi le rôle d'un adjoint qui note : écrire beaucoup de choses sans souci d'ordre laisse la créativité circuler, et les morceaux se cousent à la fin. La liste d'idées de ce blogue est née de cette façon.

Le soir, de retour à la maison, avec l'accès au code et à tout le reste, ces idées ont été reprises avec l'assistant d'architecture, puis améliorées. Certaines ont été gardées dans le carnet de produit (*backlog*), la liste des choses qui pourraient servir plus tard, d'autres mises en pratique tout de suite ou planifiées pour les prochaines itérations, et d'autres complètement écartées.

Ce que j'ai trouvé intéressant, c'est la facilité de capter sur-le-champ des idées fraîches, sans courir le risque de les oublier au fil de la journée, pris par les soucis du quotidien. La réflexion avance hors du bureau; l'exécution attend le retour devant le poste. La limite : sans accès aux documents du dépôt de contexte, les décisions se discutent et se consignent, mais les révisions sérieuses attendent.

## 5. Travailler dans sa langue

Même en vivant au Québec depuis plus de quinze ans et en m'exprimant couramment en français, une journée remplie de réunions, d'architecture et de discussions laisse une fatigue mentale importante. L'assistant me permet, entre autres, de communiquer dans ma langue maternelle. Malgré mon aisance en français, le portugais m'offre une plus grande richesse de mots, des nuances plus faciles à exprimer et des idées qui viennent plus naturellement, donc une fatigue bien moindre.

En réalité, la conversation n'a pas de langue vraiment définie : le français, l'anglais et le portugais s'y mélangent, avec plus de 90 % en portugais. Cela compliquera la production des transcriptions pour l'enseignant, mais c'est un problème pour plus tard. Pour le moment, j'en profite.

L'assistant joue aussi le rôle de secrétaire : il rédige la plupart des documents en français et révise les textes finaux une, deux, trois fois, ce qui réduit le nombre d'erreurs et valide en plus la fluidité du texte. Rédiger les livrables est d'ailleurs un travail à part : produire le contenu, par la discussion et les réunions, est une chose; mettre en forme l'enchevêtrement d'idées et de décisions finales en un document à livrer en est une autre, très différente.

## 6. L'épuisement, et la question de déléguer

L'assistant est infatigable; ce n'est pas mon cas. Je travaille surtout le soir, parfois jusque tard dans la nuit, et le nombre de décisions à prendre sur-le-champ est parfois bouleversant. Même quand tout a été décidé en amont, en architecture, il reste des détails : comment nommer telle classe, faut-il une variable pour garder telle information, où la placer. Ce sont des mini-architectures, menées en continu. Tout cela est parfois épuisant.

C'est peut-être trop d'exigence envers moi-même pour tout garder sous contrôle, mais c'est la même chose avec des collègues humains. D'où une question de fond, qui reste ouverte : faut-il déléguer davantage et se concentrer sur autre chose? Si je déléguais davantage et participais moins, que ressentirais-je : serait-ce encore mon travail?

## 7. « Utiliser l'IA, c'est de la paresse? »

Certains disent que c'est de la paresse et que le travail produit avec l'IA n'est pas vraiment celui de la personne qui l'utilise. Je ne suis pas d'accord : je n'ai pas le sentiment de tout remettre en attendant des résultats. J'utilise un outil très puissant qui augmente ma productivité et me permet de tenir un objectif dans un délai très court.

L'outil m'aide aussi à produire des documents riches et complets qui n'existeraient pas autrement. Seul, je documenterais de façon bien plus simple, avec moins de richesse, moins de liens vers d'autres travaux et moins de détails, ou j'y passerais des mois sans arriver nulle part.

## 8. Apprendre où est la limite

Savoir où se situe la limite entre l'acceptable et l'excès, savoir quand s'arrêter et quand reprendre, fait partie de l'apprentissage. Le cours le demande explicitement : où s'arrête l'aide et où commence l'apprentissage?

C'est un monde nouveau en pleine évolution, comme l'a été la naissance des réseaux sociaux et de Wikipédia, quand on est passé des encyclopédies à l'information en ligne au bout des doigts. Il a un côté brillant et un côté sombre, mais on finit par apprendre à marcher entre les deux.

# Données initiales du catalogue : mécaniques et modes de jeu

**Statut :** proposition à réviser · **Proposée le :** 13 octobre 2026

Données initiales de la table `mecanique` (voir le [schéma de données](schema-donnees.md#table-mecanique)) : une liste courte, choisie par MeepleZone, en français, plutôt que la liste complète de BoardGameGeek. Le champ `bgg_id` reste vide pour l'instant : le lien avec les mécaniques de BoardGameGeek se fera à partir de la réponse de leur API, une fois la clé d'accès obtenue.

| `nom` | `description` |
|---|---|
| Pose d'ouvriers | Chaque joueur place des pions sur des cases d'action, qui ne peuvent souvent servir qu'une fois par tour. |
| Construction de pioche | Chaque joueur part d'un petit paquet de cartes et en achète d'autres pour l'améliorer au fil de la partie. |
| Construction de moteur | Les joueurs assemblent des éléments qui se renforcent entre eux et produisent de plus en plus de ressources. |
| Gestion de main | Le défi consiste à choisir quelles cartes jouer, garder ou défausser parmi celles qu'on tient en main. |
| Draft de cartes | Chaque joueur choisit une carte parmi plusieurs, puis passe le reste à son voisin. |
| Lancer de dés | Le résultat des dés détermine les actions possibles ou le déroulement des événements. |
| Placement de tuiles | Les joueurs posent des tuiles pour construire un paysage ou une structure, avec des règles d'adjacence. |
| Majorité de zones | Il faut avoir plus de présence que les adversaires dans une zone pour la contrôler et en tirer des points. |
| Collection de séries | Les joueurs réunissent des ensembles d'objets ou de cartes qui rapportent plus quand ils sont complets. |
| Enchères | Les joueurs misent pour obtenir des objets, des actions ou l'ordre du tour. |
| Échanges et négociation | Les joueurs discutent, échangent des ressources ou concluent des ententes entre eux. |
| Rôles cachés | Chaque joueur reçoit une identité secrète, et les camps s'affrontent sans se connaître. |
| Déduction | Il faut recouper des indices pour deviner une information cachée. |
| Bluff | Les joueurs dissimulent ou inventent de l'information pour tromper les autres. |
| Pousser sa chance | On peut continuer à tenter sa chance pour gagner plus, au risque de tout perdre au tour. |
| Pouvoirs variables | Chaque joueur dispose de capacités ou de règles qui lui sont propres. |
| Plateau modulaire | Le plateau est assemblé à partir d'éléments qui changent d'une partie à l'autre. |
| Déplacement sur le plateau | Les pions se déplacent sur des cases, des zones ou des chemins selon des règles de mouvement. |
| Construction de réseaux | Les joueurs relient des lieux par des routes, des voies ou des liens pour former un réseau. |
| Points d'action | Chaque tour, le joueur dispose d'un nombre limité de points à répartir entre plusieurs actions. |
| Gestion de ressources | Il faut produire, échanger et dépenser des ressources de façon à atteindre ses objectifs. |
| Programmation d'actions | Les joueurs choisissent leurs actions à l'avance, parfois en secret, puis les dévoilent et les exécutent. |
| Campagne et évolution | Les parties s'enchaînent, et les choix de l'une modifient le jeu pour les suivantes. |

## Modes de jeu initiaux

Données initiales de la table `mode_jeu`. Le coopératif et le jeu par équipes sont des modes, et non des mécaniques : ils ne figurent donc pas dans la liste ci-dessus.

| `nom` | `description` |
|---|---|
| Compétitif | Chaque joueur joue pour lui-même et cherche à gagner contre les autres. |
| Coopératif | Tous les joueurs gagnent ou perdent ensemble contre le jeu. |
| Semi-coopératif | Les joueurs coopèrent, mais chacun peut avoir un objectif personnel, ou l'un d'eux peut trahir les autres. |
| Par équipes | Les joueurs sont répartis en équipes, et chaque équipe joue contre les autres. |

## À considérer plus tard

Temps réel, mémoire, course, mise en place variable, catégories de jeu (thème, genre).

## Reste à décider

- Confirmer, retirer ou ajouter des mécaniques à la liste.
- Relier chaque mécanique à son `bgg_id` à partir de la réponse de l'API.

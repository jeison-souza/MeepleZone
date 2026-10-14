# ADR 0006 : Suppression de compte et des données associées

- **Statut :** accepté
- **Date :** 2026-10-14

## Contexte

Un utilisateur doit pouvoir quitter MeepleZone et obtenir que ses données personnelles soient supprimées. L'application ne conserve que le minimum (nom d'affichage et courriel fournis par le fournisseur d'identité, voir l'[ADR 0001](0001-authentification.md)), mais aussi sa collection, ses amitiés et, plus tard, ses participations à des soirées.

Le projet s'inspire de la Loi 25 (Loi modernisant des dispositions législatives en matière de protection des renseignements personnels, qui modifie notamment la Loi sur la protection des renseignements personnels dans le secteur privé) comme cadre de référence, sans prétendre que la loi s'applique formellement à un projet de cours. L'article 23 de cette loi prévoit que les renseignements personnels sont détruits ou rendus anonymes lorsque les fins pour lesquelles ils ont été recueillis sont accomplies. La page de confidentialité du site doit décrire ce qui arrive à un compte supprimé.

Certaines données concernent aussi d'autres personnes (une amitié, un jeu du catalogue commun ajouté par la personne, plus tard une soirée à laquelle d'autres participent) : les effacer sans réflexion peut abîmer ce que les autres utilisent.

Le cas d'un compte récent et vide, supprimé par la personne pour pouvoir lier ses fournisseurs (voir l'[ADR 0004](0004-liaison-comptes-fournisseurs.md)), doit être simple et immédiat.

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Suppression complète et immédiate de tout ce que la personne a créé | Simple à expliquer; aucune donnée résiduelle | Irréversible, y compris en cas d'erreur; retire du catalogue des jeux que d'autres utilisateurs ont dans leur collection |
| B. Suppression immédiate des données personnelles; contenu partagé conservé et rattaché à un compte technique « Utilisateur supprimé » | Données personnelles réellement détruites; le catalogue commun reste intact; aucun délai ni tâche planifiée | Plus de règles à développer et à tester; il faut définir précisément ce qui est « personnel » |
| C. Désactivation avec délai de grâce, puis suppression ou anonymisation | Permet de revenir sur une décision prise par erreur | Données conservées pendant le délai; tâche planifiée à prévoir; plus de cas à tester; sans intérêt pour le compte récent et vide décrit plus haut |

## Décision

Option B. La suppression est immédiate et se fait en une seule transaction : si une étape échoue, rien n'est supprimé. Une confirmation explicite est demandée à la personne avant de commencer, puisque l'opération est irréversible.

**Supprimé définitivement :**

- le compte (`utilisateur`) et ses connexions externes (`identite_externe`, par `ON DELETE CASCADE`);
- la collection de la personne (`possession`), avec les notes et commentaires;
- ses demandes d'amitié envoyées et reçues et ses amitiés (`demande_amitie`, `amitie`);
- sa photo de profil (`annexe`);
- les jeux qu'elle a ajoutés et qui n'ont jamais été validés, avec leur couverture, **à condition** qu'aucun autre utilisateur ne les ait dans sa collection. Ces jeux sont supprimés avant le compte, une fois sa propre possession retirée, car `possession.jeu_id` est en `RESTRICT`.

**Conservé, sans lien avec la personne :** les jeux validés qu'elle a ajoutés, ainsi que les jeux non validés que d'autres utilisent, et leurs annexes (couvertures). Dans `jeu` et `annexe`, la colonne `cree_par_id` est `NOT NULL` avec `ON DELETE RESTRICT` : la base refuse de supprimer un compte tant qu'il reste du contenu rattaché. Le code réaffecte donc d'abord ce contenu au compte technique **« Utilisateur supprimé »** (rôle `systeme`, créé par seed, sans connexion externe), puis supprime le compte. Ce compte est unique et partagé par tous les anciens utilisateurs : le système ne conserve aucune table ni aucun champ indiquant qui était qui.

**Comptes protégés :** les comptes de rôle `administrateur` et `systeme` ne se suppriment pas par ce parcours. Un administrateur doit d'abord perdre son rôle, par une intervention manuelle.

**Après la suppression :** le courriel et les identifiants du fournisseur sont libérés; la personne peut se reconnecter plus tard et obtenir un compte neuf, sans lien avec l'ancien.

**Sauvegardes :** les sauvegardes de la base peuvent contenir temporairement des données supprimées ensuite; elles ne sont pas modifiées individuellement. La page de confidentialité le dit en termes généraux, sans préciser de durée. Si une sauvegarde est restaurée, les suppressions faites depuis sa création sont réappliquées à partir d'un journal tenu hors de la base, qui ne contient que l'identifiant interne du compte supprimé et la date.

## Conséquences

### Positives

- Les données personnelles sont détruites sans délai, conformément à l'esprit de l'article 23 de la Loi 25.
- Le catalogue de jeux reste complet : supprimer son compte ne retire rien à la collection des autres.
- Aucune tâche planifiée ni état intermédiaire (« compte en attente de suppression »).
- Le cas du compte récent et vide (ADR 0004) se règle en quelques secondes.

### Négatives

- Une suppression faite par erreur ne se défait pas; seule la confirmation explicite en limite le risque.
- La logique de suppression (réaffectation, suppression conditionnelle des jeux non validés, comptes protégés) est à développer et à tester avec soin, y compris le cas d'un échec en cours de transaction.
- Le compte « Utilisateur supprimé » devient une donnée de référence à créer par seed et à ne jamais supprimer.
- Le journal de restauration est une pièce de plus à maintenir hors de la base.

### À reprendre plus tard

- Le traitement des participations à des soirées sera précisé avec le schéma des soirées (priorité « Could »).
- Un délai de grâce (option C) pourra être ajouté si l'usage montre que les suppressions par erreur sont fréquentes.

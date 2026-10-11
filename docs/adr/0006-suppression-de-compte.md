# ADR 0006 : Suppression de compte et des données associées (Loi 25)

- **Statut :** brouillon (décision à prendre)
- **Date :** 2026-10-10

## Contexte

Un utilisateur doit pouvoir quitter MeepleZone et obtenir que ses données personnelles soient supprimées. L'application ne conserve que le minimum (nom d'affichage et courriel fournis par le fournisseur d'identité, voir l'[ADR 0001](0001-authentification.md)), mais aussi sa collection, ses amitiés et, plus tard, ses participations à des soirées et ses prêts.

La Loi 25 (Loi modernisant des dispositions législatives en matière de protection des renseignements personnels, qui modifie notamment la Loi sur la protection des renseignements personnels dans le secteur privé) encadre la conservation et la destruction de ces données. Elle prévoit en particulier que les renseignements personnels doivent être détruits ou anonymisés de façon irréversible lorsque les fins pour lesquelles ils ont été recueillis sont accomplies. Le projet s'en inspire comme cadre de référence, que son application stricte à un projet de cours soit établie ou non. La page de confidentialité du site doit décrire ce qui arrive à un compte supprimé.

Certaines données concernent aussi d'autres personnes (une amitié, un prêt entre deux utilisateurs, une soirée à laquelle d'autres participent) : les effacer sans réflexion peut abîmer l'historique des autres.

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Suppression complète et immédiate | Simple à expliquer; aucune donnée résiduelle | Irréversible, y compris en cas d'erreur; casse l'historique partagé avec d'autres utilisateurs (prêts, soirées) |
| B. Anonymisation : les données personnelles sont effacées, les enregistrements partagés sont conservés sans identité | Préserve l'historique des autres; données personnelles bien supprimées | Plus complexe à développer; il faut définir précisément ce qui est « personnel » |
| C. Désactivation avec délai de grâce, puis suppression ou anonymisation | Permet de revenir sur une décision prise trop vite ou par erreur | Données conservées pendant le délai; tâche planifiée à prévoir; plus de cas à tester |

Le cas d'un compte récent et vide, supprimé par la personne pour pouvoir lier ses fournisseurs (voir l'[ADR 0004](0004-liaison-comptes-fournisseurs.md)), doit être simple et immédiat : un délai de grâce n'y a pas de sens.

Points à préciser avant de trancher : durée éventuelle du délai de grâce, traitement des données partagées avec d'autres utilisateurs, et effet sur les sauvegardes de la base de données.

# ADR 0016 : Ajout et modification du courriel du compte

- **Statut :** accepté
- **Date :** 2026-11-28

## Contexte

Le courriel du compte est facultatif. Il est copié de la première connexion quand le fournisseur en communique un, et la personne peut ensuite le changer ou l'ajouter depuis son profil ([ADR 0004](0004-liaison-comptes-fournisseurs.md), règles 4 et 5). La connexion ne dépend pas de ce courriel, et seul un courriel vérifié réserve une adresse (ADR 0004, règle 1).

L'ADR 0004 porte sur la liaison de plusieurs fournisseurs à un même compte. L'ajout et la modification du courriel sont traités ici, et sa vérification dans l'[ADR 0017](0017-verification-du-courriel.md). Il faut décider à quel moment un courriel saisi par la personne est enregistré dans son compte.

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Après la vérification seulement | Le compte garde son courriel actuel jusqu'à la preuve; une erreur de saisie est sans effet | Le courriel en attente doit être conservé à part (table ou lien signé avec échéance); l'ajout et la modification dépendent de la vérification |
| B. Dès la saisie, sans date de vérification | Un seul état à gérer, que le courriel vienne d'un fournisseur ou de la personne; aucun endroit à prévoir pour un courriel en attente; indépendant de la vérification | Le courriel précédent est perdu, même après une erreur de saisie |

## Décision

Option B. La personne peut ajouter ou modifier son courriel depuis son profil. Il est enregistré dès la saisie, et `courriel_verifie_le` est vidé jusqu'à la vérification décrite dans l'[ADR 0017](0017-verification-du-courriel.md). La saisie est refusée si le texte n'est pas un courriel valide ou si un autre compte a déjà vérifié cette adresse. Enregistrer de nouveau le même courriel ne change rien.

Avant la modification d'un courriel vérifié, l'écran du profil avertit la personne, en lui présentant l'ancien et le nouveau courriel : si elle continue, le nouveau courriel devra être vérifié avant de recevoir les communications et les demandes d'amitié faites à cette adresse. Elle peut annuler, ou continuer en connaissance de cause. L'ajout d'un courriel et la modification d'un courriel non vérifié sont enregistrés sans avertissement. Cet avertissement atténue les inconvénients de l'option B.

## Conséquences

### Positives

- Un courriel suit le même parcours, qu'il vienne d'un fournisseur ou de la personne : il est enregistré, puis vérifié.
- L'ajout et la modification peuvent être mis en œuvre avant l'envoi de courriels.
- Aucun courriel en attente n'est à conserver à part.
- Une adresse saisie sans preuve ne bloque personne (ADR 0004, règle 1).

### Négatives

- Une erreur de saisie remplace un courriel vérifié : il faut le corriger, puis le vérifier de nouveau. L'avertissement avant la modification, qui présente l'ancien et le nouveau courriel, réduit ce risque. La double saisie n'a pas été retenue : elle alourdit le parcours, surtout sur un appareil mobile, pour une erreur qui se corrige.
- Une personne peut saisir l'adresse d'une autre : la même adresse peut alors figurer dans plusieurs comptes, jusqu'à ce que l'un d'eux la vérifie. Le risque est jugé faible, puisque aucune communication ni demande d'amitié par courriel n'est rattachée à un compte sans cette vérification.
- Entre la saisie et la vérification, la personne ne reçoit ni les communications par courriel ni les demandes d'amitié faites à ce courriel.
- La base de données ne garantit l'unicité que des courriels vérifiés : le refus de saisir ou de copier un courriel vérifié par un autre compte repose sur le code.

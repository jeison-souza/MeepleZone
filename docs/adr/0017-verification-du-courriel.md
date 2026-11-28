# ADR 0017 : Vérification du courriel du compte

- **Statut :** accepté
- **Date :** 2026-11-28

## Contexte

Un courriel n'est pas vérifié du seul fait qu'un fournisseur le communique ou que la personne le saisit. Une vérification propre à l'application, enregistrée dans `utilisateur.courriel_verifie_le`, est nécessaire pour les communications par courriel ([ADR 0004](0004-liaison-comptes-fournisseurs.md), règle 3), et un courriel vérifié ne peut appartenir qu'à un seul compte (règle 1). L'ajout et la modification du courriel sont traités dans l'[ADR 0016](0016-ajout-et-modification-du-courriel.md).

Il faut décider par quel moyen un courriel est vérifié, combien de temps une demande de vérification reste valide et comment les abus sont limités.

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| Code envoyé par courriel, saisi sur une page de l'application | Même parcours pour tous les fournisseurs et pour un courriel saisi; le code est saisi dans la session qui l'a demandé, même si le courriel est lu sur un autre appareil; la page de saisie pourrait resservir pour une authentification multifacteur | Dépend de l'envoi de courriels; une page de saisie à développer; le code et le nombre d'essais sont à conserver dans la base |
| Lien envoyé par courriel, qui désigne la demande de vérification sous forme chiffrée et signée | Aucun code à saisir ni à conserver; le lien ne contient ni le courriel ni l'échéance (protection des données d'ASP.NET Core) | Dépend de l'envoi de courriels; ouvert sur un autre appareil, il oblige à s'y connecter |
| Preuve d'un fournisseur qui vérifie les courriels, en complément de l'envoi | Aucun courriel à envoyer pour ces comptes | Tous les fournisseurs ne l'offrent pas de façon fiable (c'est le cas de Microsoft); la règle 3 de l'ADR 0004 est à modifier |

## Décision

La vérification se fait par un lien envoyé au courriel. Chaque demande est enregistrée dans la table [`demande_verification`](../schema/schema-donnees.md#table-demande_verification), et le lien ne contient que l'identifiant de la demande, chiffré et signé : l'identifiant seul ne suffirait pas à prouver que le courriel a été reçu. La confirmation est acceptée si la demande existe, appartient au compte connecté, porte sur le courriel actuel du compte et n'est pas échue (`cree_le` plus `duree_validite`). La saisie d'un code n'a pas été retenue : elle demande une page de saisie et un suivi des essais, et la réutilisation pour une authentification multifacteur pèse peu dans une application où la connexion relève des fournisseurs externes.

Quel que soit le moyen, la page qui reçoit la confirmation exige une session authentifiée, celle du compte qui a fait la demande : recevoir le courriel ne suffit pas à vérifier une adresse pour le compte d'une autre personne.

La vérification porte sur le courriel enregistré dans le compte ([ADR 0016](0016-ajout-et-modification-du-courriel.md)). Sa confirmation est traitée à un seul endroit, dans le domaine `Compte`, quel que soit le moyen de vérification. Elle est refusée si un autre compte a déjà vérifié ce courriel. Sinon, elle retire aussi ce courriel des autres comptes, qui l'ont sans l'avoir vérifié : le premier compte qui le vérifie l'obtient. Ce retrait fait partie de la même transaction. La date de vérification et les demandes d'amitié touchées sont enregistrées dans une seule transaction. Le sort de ces demandes est décrit dans le [schéma de données](../schema/schema-donnees.md#table-demande_amitie).

La preuve d'un fournisseur ne complète pas le lien : tous les fournisseurs ne l'offrent pas de façon fiable, et un seul parcours est à développer et à tester.

Une demande est valide 24 heures. Cette durée est un paramètre de configuration. Une nouvelle demande n'annule pas les précédentes : si un courriel tarde, chacun des liens reçus convient. Un délai de quelques minutes serait trop court quand le courriel tarde ou est lu plus tard, et le risque d'un délai plus long est limité, puisque le lien ne sert qu'au compte connecté et à son courriel actuel.

Les envois sont limités, pour éviter de remplir la boîte d'une autre personne, d'envoyer des courriels à des inconnus et de nuire à la réputation du domaine. Les valeurs sont des paramètres de configuration :

| Limite | Valeur |
|---|---|
| Intervalle minimal entre deux demandes d'un même compte | 1 minute |
| Demandes d'un même compte en 24 heures | 5 |
| Demandes vers une même adresse en 24 heures, tous comptes confondus | 3 |

La table des demandes sert aussi à ces limites et au suivi des envois. Les limites comptent toutes les demandes, même celles dont l'envoi a échoué. Les demandes de plus de 24 heures qui sont échues sont supprimées lors d'une demande suivante, sans tâche planifiée.

## Conséquences

### Positives

- Le moyen de vérification peut évoluer sans modifier la décision sur la liaison des comptes.
- La vérification ne demande ni page de saisie, ni suivi des essais.
- Les limites d'envoi valent pour l'ensemble du site, quel que soit le nombre d'instances.
- L'échéance a une seule source, la table, et sa règle se teste dans le domaine sans base de données ([ADR 0008](0008-logique-d-affaires-testable.md)).
- Une demande peut être annulée en supprimant sa ligne.
- La table permettrait de passer un jour à un code saisi, en y ajoutant le code et le nombre d'essais.

### Négatives

- Un compte peut perdre son courriel non vérifié, sans en être averti, quand un autre compte le vérifie.
- Tant que l'envoi de courriels n'est pas en place, aucun courriel ne peut être vérifié : les demandes d'amitié faites par courriel à un compte non vérifié restent sans destinataire.
- Ouvert sur un appareil où la personne n'est pas connectée, le lien oblige à s'y connecter.
- Les clés qui protègent le lien doivent être conservées d'un déploiement à l'autre, sans quoi les liens en cours cessent de valoir.
- La vérification et les limites d'envoi demandent une table et une migration. Les adresses saisies y restent au moins 24 heures.

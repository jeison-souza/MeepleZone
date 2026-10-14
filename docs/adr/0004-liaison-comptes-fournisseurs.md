# ADR 0004 : Liaison des comptes de plusieurs fournisseurs d'identité

- **Statut :** accepté, amendé le 2026-10-14 (règle 3)
- **Date :** 2026-10-10

## Contexte

Selon l'[ADR 0001](0001-authentification.md), les utilisateurs se connectent uniquement par des fournisseurs d'identité externes (Google et Microsoft au départ, d'autres plus tard). Une même personne peut posséder un compte chez plusieurs de ces fournisseurs, et se connecter tantôt avec l'un, tantôt avec l'autre.

Un utilisateur est identifié par le couple fournisseur + identifiant du compte chez ce fournisseur (`identifiant_compte_externe` : le `sub` chez Google, l'`oid` chez Microsoft). Sans traitement particulier, se connecter avec un second fournisseur crée donc un second compte, sans collection ni amis.

Il faut décider si ces comptes sont reliés et comment. Le courriel ne peut pas servir de preuve d'identité à lui seul : certains fournisseurs ne garantissent pas qu'il a été vérifié, et il peut être défini librement dans certains contextes (comptes d'organisation Microsoft). Un courriel usurpé permettrait de prendre le contrôle du compte d'une autre personne.

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Liaison automatique par courriel | Transparente pour l'utilisateur; aucune action requise | Risque de prise de contrôle de compte si le courriel n'est pas vérifié ou est usurpé; comportement difficile à prévoir quand le courriel diffère d'un fournisseur à l'autre |
| B. Liaison explicite, à l'initiative de l'utilisateur connecté | Aucun risque d'usurpation : la personne prouve qu'elle contrôle les deux comptes; comportement prévisible | Une action de l'utilisateur est requise; une personne qui ne lie pas ses comptes en aura deux |
| C. Aucune liaison : un compte par fournisseur | Le plus simple à développer | Collection et amis éclatés entre plusieurs comptes; mauvaise expérience pour l'utilisateur |

## Décision

Option B : liaison explicite, sans fusion automatique à partir du courriel. Depuis son profil, un utilisateur déjà connecté choisit d'associer un autre fournisseur, termine la connexion chez celui-ci, et le couple fournisseur + `identifiant_compte_externe` correspondant est ajouté à son compte. Le modèle de données prévoit un utilisateur et plusieurs connexions externes qui lui sont rattachées (relation un à plusieurs).

Quatre règles complètent la décision :

1. **Le courriel est unique.** Deux comptes ne peuvent pas partager le même courriel. Si une personne se connecte avec un fournisseur dont le courriel appartient déjà à un compte existant, aucun compte n'est créé : un message l'invite à se connecter avec le fournisseur d'origine, puis à associer le nouveau fournisseur depuis son profil.
2. **Aucune liaison vers un fournisseur déjà utilisé par un autre compte.** Si le fournisseur à associer est déjà rattaché à un autre compte (par exemple parce que la personne s'y est connectée d'abord et a créé un compte), la liaison est refusée. Le système demande de supprimer l'un des deux comptes (normalement le plus récent, encore sans données), puis de se reconnecter avec le compte d'origine pour faire l'association (voir l'[ADR 0006](0006-suppression-de-compte.md)).
3. **La connexion ne dépend que du fournisseur.** Si le fournisseur a authentifié la personne, elle entre; l'application n'exige pas de confirmation du courriel de la part du fournisseur, que certains n'offrent pas de façon fiable (c'est le cas de Microsoft). Le courriel ne sert jamais à identifier la personne ni à lier des comptes : seul `identifiant_compte_externe` identifie. Une vérification propre à l'application, par un code envoyé au courriel, est réservée aux communications par courriel (par exemple les demandes d'amitié) et enregistrée dans `courriel_verifie_le`; sans elle, la personne se connecte normalement mais ne reçoit pas ces communications.
4. **Le courriel est conservé pour chaque connexion externe.** Il n'est pas figé à la création du compte, car il peut changer chez le fournisseur. La comparaison ignore la casse, et l'unicité vaut pour l'ensemble des courriels enregistrés : un changement de courriel chez un fournisseur qui entrerait en conflit avec un autre compte est refusé, avec le même message que pour la règle 1.

## Conséquences

### Positives

- Aucune possibilité de prendre le contrôle d'un compte en usurpant un courriel.
- Un courriel correspond à un seul compte, et un fournisseur à un seul compte : aucune ambiguïté sur l'identité de la personne qui se connecte.
- Une personne n'est jamais bloquée à la connexion parce que son fournisseur ne confirme pas son courriel.
- L'ajout futur de fournisseurs (voir l'ADR 0001) suit le même mécanisme, sans règle spéciale.

### Négatives

- Le système ne peut pas détecter qu'une même personne a deux comptes quand les courriels diffèrent (par exemple un compte Google avec une adresse Gmail et un compte Microsoft avec une adresse Hotmail) : deux comptes distincts sont alors possibles, jusqu'à ce que la personne les règle elle-même.
- Une personne qui a déjà utilisé les deux fournisseurs doit supprimer un compte avant de les lier, et la fusion de deux comptes contenant des données n'est pas prévue.
- Comme le courriel d'une connexion n'est pas garanti vérifié, quelqu'un pourrait inscrire l'adresse d'une autre personne et lui bloquer l'inscription (règle 1). Le risque est jugé faible pour ce projet; la personne lésée s'adresse au soutien, qui règle le cas à la main.
- Un écran de gestion des fournisseurs associés (ajout, retrait) est à développer et à tester, y compris le cas où le dernier fournisseur d'un compte ne peut pas être retiré.
- La vérification du courriel par code dépend de l'envoi de courriels depuis le domaine, qui reste à mettre en place.

# ADR 0003 : Domaine canonique

- **Statut :** accepté
- **Date :** 2026-10-10

## Contexte

Deux noms de domaine ont été achetés : `meeplezone.com` et `meeplezone.ca`. L'authentification par Google et Microsoft (voir l'[ADR 0001](0001-authentification.md)) exige des URL de redirection stables, déclarées chez chaque fournisseur. Les cookies de session sont liés au nom d'hôte, et les moteurs de recherche pénalisent le contenu dupliqué sur plusieurs adresses.

Il faut décider si les deux domaines servent le site ou si un seul est l'adresse officielle.

## Options considérées

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Un domaine canonique, l'autre redirige | Un seul hôte à déclarer chez les fournisseurs OAuth; une seule session; pas de contenu dupliqué | Une règle de redirection à maintenir; un domaine reste « secondaire » |
| B. Les deux domaines servent le site | Aucune redirection; les deux adresses fonctionnent telles quelles | URL de redirection OAuth à doubler; sessions distinctes selon le domaine d'arrivée; plus de certificats à gérer; contenu dupliqué |

## Décision

Option A : `meeplezone.ca` est le domaine canonique, et `meeplezone.com` redirige vers celui-ci (règle de redirection Cloudflare). La redirection est d'abord temporaire (302), pour qu'aucun navigateur ne garde en mémoire une destination qui pourrait encore changer; elle devient permanente (301) une fois le choix du domaine canonique jugé définitif.

Le choix du `.ca` s'explique par la langue : le site est en français et s'adresse d'abord à un public québécois et canadien. Le `.com` reste utile comme adresse de repli que les gens saisissent par réflexe.

## Conséquences

### Positives

- Un seul hôte à enregistrer chez Google et Microsoft, une seule session par utilisateur, et un référencement non dispersé.
- L'adresse officielle correspond à la langue et au public visés.
- Le changement de domaine canonique, si nécessaire plus tard, se limite à la règle de redirection et aux URL déclarées chez les fournisseurs.

### Négatives

- Le `.com` n'est qu'un alias : les liens partagés avec celui-ci passent par une redirection supplémentaire.
- Le `.ca` associe le projet à un seul pays. Si le site prenait de l'ampleur au-delà du Canada francophone, il faudrait revoir ce choix, ainsi que d'autres aspects du projet, dont la prise en charge de plusieurs langues.

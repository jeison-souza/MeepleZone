# ADR 0002 : Mode d'entrée depuis Internet

- **Statut :** accepté
- **Date :** 2026-10-10

## Contexte

MeepleZone sera hébergé, dans un premier temps, dans un conteneur Docker sur une infrastructure personnelle, et doit être joignable depuis Internet en HTTPS. L'application utilise Blazor Server, donc des connexions WebSocket (SignalR) persistantes. La zone DNS des domaines du projet est gérée par Cloudflare (plan gratuit).

Il faut choisir comment le trafic public atteint l'application.

## Options considérées

| Option | Principe | Avantages | Inconvénients |
|---|---|---|---|
| A. Proxy inverse exposé | Le trafic HTTPS arrive par un port ouvert (443) et un proxy inverse le transmet à l'application | Simple et éprouvé; aucun composant tiers supplémentaire | Un port ouvert à la périphérie du réseau; liste des adresses de Cloudflare à tenir à jour si l'accès est restreint à celles-ci; dépend d'une adresse IP stable ou mise à jour automatiquement; inutilisable derrière un CGNAT |
| B. Tunnel sortant vers l'application | Un connecteur (`cloudflared`) ouvre une connexion sortante vers Cloudflare, qui relaie le trafic public directement à l'application | Aucun port ouvert; fonctionne derrière un CGNAT; pas de proxy inverse à maintenir pour ce service | Dépendance à Cloudflare; un tunnel ou une règle d'acheminement par service; un proxy inverse reste nécessaire pour l'accès depuis le réseau interne : deux chemins d'accès au même service à maintenir |
| C. Tunnel sortant vers un proxy inverse | Même connecteur, mais le trafic aboutit à un proxy inverse interne, qui l'achemine vers l'application | Aucun port ouvert; fonctionne derrière un CGNAT; point d'entrée unique pour l'accès interne et externe, réutilisable pour d'autres services | Dépendance à Cloudflare; deux composants à maintenir (connecteur et proxy) |

## Décision

Option A au départ, puis migration progressive vers l'option C.

L'option A correspond à la solution déjà en place sur l'infrastructure, ce qui a permis de mettre le site en ligne rapidement. L'accès est restreint aux adresses publiées par Cloudflare pour limiter l'exposition. L'infrastructure sera ensuite migrée graduellement vers l'option C (tunnel sortant vers le proxy inverse existant), en commençant par MeepleZone comme projet pilote. L'option B est écartée : elle obligerait à maintenir en parallèle un proxy inverse pour l'accès interne.

## Conséquences

### Positives

- Mise en ligne immédiate, sans composant à ajouter, et migration vers l'option C possible sans modifier l'application : seul l'acheminement du trafic change.
- Une fois la migration faite, plus aucun port ouvert ni adresse IP à publier ou à mettre à jour, et la solution continue de fonctionner derrière un CGNAT.

### Négatives

- Tant que la migration n'est pas faite, un port reste ouvert à la périphérie du réseau et la liste des adresses de Cloudflare doit être tenue à jour.
- Après la migration, dépendance à Cloudflare pour l'accès public (panne ou changement de conditions du plan gratuit, dont les limites de taille de requête et la durée des connexions WebSocket), et un conteneur `cloudflared` de plus à déployer, avec un jeton conservé hors du dépôt.
- L'application doit tenir compte du proxy (en-têtes `X-Forwarded-*`) pour construire correctement ses URL, notamment celles de redirection de l'authentification.

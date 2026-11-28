# ADR 0011 : Concurrence à l'enregistrement d'un compte et contrats de dépôt

- **Statut :** accepté, amendé le 2026-11-28
- **Date :** 2026-11-10

## Contexte

La première connexion d'une personne crée un utilisateur et sa connexion externe ([ADR 0004](0004-liaison-comptes-fournisseurs.md)). L'opération est un « trouver ou créer » ([ADR 0009](0009-organisation-de-la-solution.md), règle 4) : le gestionnaire vérifie d'abord que la connexion et le courriel sont inconnus, puis enregistre. Entre la vérification et l'enregistrement, une autre requête peut avoir créé la même connexion (deux onglets, un double clic sur le bouton de connexion) ou pris le même courriel. La base de données refuse alors le second enregistrement par une contrainte d'unicité : l'index sur le couple fournisseur + identifiant externe de `identite_externe`, ou l'index sur `utilisateur.courriel`. Depuis le 2026-11-28, seul un courriel vérifié est unique ([ADR 0004](0004-liaison-comptes-fournisseurs.md), règle 1) : un nouveau compte n'ayant jamais un courriel vérifié, seul le conflit sur la connexion externe subsiste.

Ces deux cas ne sont pas des erreurs de programmation. Ils sont attendus, ont chacun une suite précise dans les règles d'affaires, et doivent être testables sans base de données ([ADR 0008](0008-logique-d-affaires-testable.md)). Or la violation de la contrainte est un détail du fournisseur de base de données (`DbUpdateException` d'EF Core qui enveloppe une `PostgresException` de code `23505`) que le gestionnaire ne doit pas connaître.

Une seconde question s'est posée au moment d'écrire le dépôt : où placer le type qui exprime ces issues. Le gestionnaire (dans `Domaines/Compte`) en a besoin dans la signature de `IDepotCompte`, et l'implémentation (dans `Systeme/Donnees`) doit le produire. L'ADR 0009 plaçait l'interface de dépôt dans `Systeme/Interfaces`, qui est aussi le projet des interfaces de gestionnaires; un type d'issue partagé n'y avait pas de place naturelle.

## Options considérées

### Façon de signaler la concurrence

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Le dépôt laisse passer l'exception d'EF Core | Aucun code de traduction | Le domaine devrait référencer EF Core et Npgsql, ce que l'ADR 0009 interdit; l'exception de la base fuit dans la logique d'affaires; les tests du gestionnaire devraient fabriquer une `DbUpdateException` |
| B. Le dépôt traduit en exception propre au domaine (par exemple `CourrielDejaUtiliseException`) | Le domaine ne connaît plus EF Core | Deux issues prévues sont traitées comme des erreurs; le flux de la logique passe par des `try`/`catch` dans le gestionnaire; une exception non prévue se confond avec les prévues |
| C. Le dépôt traduit en résultat énuméré | Les issues prévues apparaissent dans la signature; le gestionnaire les traite par un `switch` visible; un test fait simplement retourner la valeur au faux dépôt | Un type de plus; le dépôt doit reconnaître les contraintes par leur nom |
| D. Le gestionnaire vérifie puis enregistre dans une transaction sérialisable | Pas de traduction de contrainte | Plus lourd; la contrainte unique reste nécessaire comme dernier filet; les tests ne couvrent plus la règle sans base réelle |

### Emplacement du contrat du dépôt et de son résultat

| Option | Avantages | Inconvénients |
|---|---|---|
| A. `Systeme/Interfaces` avec les interfaces de gestionnaires | Un seul projet de contrats | Mélange deux sortes de contrats (ce que le site appelle, ce que le domaine exige du stockage); le résultat du dépôt y côtoie des interfaces sans rapport |
| B. `Systeme/Donnees`, à côté de l'implémentation | Contrat et implémentation ensemble | Le domaine devrait référencer le projet qui contient EF Core, ce qui contredit la règle 7 de l'ADR 0009 |
| C. `<Domaine>.Contrats` | Déjà sans référence | C'est le projet que le site voit; le site n'a pas à connaître les dépôts |
| D. Nouveau projet `Systeme/Depots`, sans EF Core | Le domaine et `Systeme/Donnees` dépendent tous deux de lui, et non l'un de l'autre; le résultat reste avec l'interface qui le porte | Un projet de plus (à peu près vide : une interface et une énumération par domaine) |

## Décision

- **Signalement : option C.** `IDepotCompte.EnregistrerNouveauCompteAsync` retourne une valeur de l'énumération `ResultatEnregistrementCompte` : `Enregistre` ou `IdentiteDejaExistante`. Le dépôt n'appelle `SaveChanges` qu'une fois, pour l'utilisateur et sa connexion ensemble : l'enregistrement est atomique et un refus ne laisse aucune ligne partielle. Il traduit la violation de contrainte (`DbUpdateException` dont l'exception interne est une `PostgresException` de code `23505`) en lisant le nom de la contrainte : `ix_identite_externe_fournisseur_identifiant_compte_externe` donne `IdentiteDejaExistante`. Toute autre violation d'unicité (par exemple celle du code d'amitié) est relancée telle quelle : elle n'a pas de suite prévue dans les règles.
- **Suite décidée par le gestionnaire.** Le dépôt ne décide rien ; le gestionnaire réagit à chaque issue :
  - `Enregistre` : la personne est connectée avec le compte créé.
  - `IdentiteDejaExistante` : une autre requête a créé la même connexion entre-temps; le gestionnaire reprend le chemin « connexion connue » (retrouver, mettre à jour la date de connexion, retourner le compte). Si la connexion reste introuvable, c'est une anomalie, et il lève une exception.
- **Emplacement : option D.** L'interface `IDepotCompte` et l'énumération vivent dans un nouveau projet, `Systeme/Depots`, qui ne référence que `Systeme/Entites`. Le principe : le consommateur définit le contrat qu'il utilise. Le sens des dépendances est `Compte` → `Depots` ← `Donnees`; l'ADR 0009 est amendé en conséquence (règles 6 et 7). `Systeme/Interfaces` ne conserve que les interfaces de gestionnaires. Les deux vocabulaires restent distincts : `StatutConnexion` est la réponse du domaine vers le site, `ResultatEnregistrementCompte` est la réponse du dépôt vers le domaine.
- **Lectures et écritures.** L'enregistrement est un seul appel du dépôt (jamais une lecture suivie d'une écriture décidée par l'appelant); la recherche préalable du courriel décide du compte probable et de la copie du courriel. Si un autre compte vérifie ce courriel entre la recherche et l'enregistrement, le nouveau compte est tout de même créé avec ce courriel, non vérifié. Cet écart à la règle 1 de l'ADR 0004 est accepté : la fenêtre est très courte, et cette copie ne reçoit rien et ne peut pas être vérifiée.

## Conséquences

### Positives

- Les issues attendues de la concurrence sont visibles dans la signature et dans le `switch` du gestionnaire; elles se testent en faisant retourner la valeur voulue au faux dépôt, sans base de données ni exception fabriquée. Des tests couvrent la connexion créée en parallèle, dont le cas où elle reste introuvable.
- Le domaine ne référence ni EF Core ni Npgsql.
- La base reste l'arbitre final : même si la vérification préalable laisse passer une course, aucune connexion en double n'est enregistrée.
- Un refus ne laisse aucune ligne partielle (un seul `SaveChanges`); cela a été vérifié à la main contre la base de développement.
- Une personne qui double-clique sur le bouton de connexion aboutit dans son compte, sans erreur.

### Négatives

- Le dépôt dépend du nom d'un index : le renommer sans mettre à jour la constante casse la traduction (le cas tombe alors dans « relancer » et se voit dans les journaux, mais pas dans les tests sans base). Un test contre un PostgreSQL réel, prévu plus tard (ADR 0009, règle 11), couvrira ce point.
- Un projet de plus (`Systeme/Depots`), presque sans code, que le `Dockerfile` et la solution doivent connaître.
- Le gestionnaire compte plusieurs chemins (existant, nouveau, concurrence) : il a été écrit comme un enchaînement plat d'étapes pour rester lisible, mais reste la classe la plus dense du domaine.
- Une autre violation d'unicité que celle prévue (code d'amitié) n'est pas traitée : elle remonte comme erreur. La génération du code d'amitié ne gère pas la collision pour l'instant.

## Décisions liées

- [ADR 0004](0004-liaison-comptes-fournisseurs.md) : règles sur le courriel et le compte probable.
- [ADR 0008](0008-logique-d-affaires-testable.md) : logique d'affaires testable sans base de données.
- [ADR 0009](0009-organisation-de-la-solution.md) : amendé le 2026-11-08 (règles 6 et 7) pour créer `Systeme/Depots`.
- [ADR 0010](0010-identifiants-et-horodatages.md) : les identifiants sont connus avant l'enregistrement, ce qui permet de créer l'utilisateur et sa connexion dans un seul enregistrement.

## Sources

- [Codes d'erreur de PostgreSQL (23505, violation d'unicité)](https://www.postgresql.org/docs/current/errcodes-appendix.html)

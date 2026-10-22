# ADR 0010 : Identifiants et horodatages

- **Statut :** accepté
- **Date :** 2026-10-21

## Contexte

Le schéma de données ([ADR 0005](0005-base-de-donnees.md)) donne à chaque table une clé primaire et, selon le cas, des colonnes de date (`cree_le`, `modifie_le`, `derniere_connexion_le`, etc.). Il faut préciser le type des identifiants, le type C# des dates, la façon dont l'application obtient l'heure et l'endroit où une heure est convertie en heure locale.

Contraintes :

- PostgreSQL range un `timestamptz` comme un instant en UTC; l'écart d'origine n'est pas conservé. Le fournisseur Npgsql refuse d'écrire dans cette colonne une valeur qui n'est pas en UTC.
- Une heure doit s'afficher selon le fuseau de la personne, et non celui du serveur.
- La logique d'affaires doit se tester sans base de données ([ADR 0008](0008-logique-d-affaires-testable.md)) : l'heure doit pouvoir être simulée.
- Une même clé est parfois nécessaire avant l'enregistrement (la connexion externe et l'annexe d'un compte référencent l'utilisateur qui vient d'être créé).

## Options considérées

### Identifiants

| Option | Avantages | Inconvénients |
|---|---|---|
| A. Entier généré par la base | Compact; ordonné | Devinable dans une adresse; révèle le nombre d'enregistrements; connu seulement après l'enregistrement |
| B. GUID aléatoire (version 4) | Très difficile à deviner; généré avant l'enregistrement | Insertions dispersées dans les index, donc plus de fragmentation |
| C. GUID de version 7 généré par le code | Difficile à deviner (la partie aléatoire reste grande); connu avant l'enregistrement; ordonné dans le temps, donc insertions en fin d'index | Contient l'heure de création (à la milliseconde); 16 octets |
| D. GUID de version 7 généré par la base (`DEFAULT uuidv7()`, PostgreSQL 18) | Moins de code; toute insertion manuelle en SQL reçoit une clé; mêmes avantages d'ordre que C | Clé connue seulement après l'enregistrement : un dépôt factice doit alors simuler sa génération dans les tests (faisable, mais le test ne vérifie plus que le code crée la clé); sans propriété de navigation, créer un utilisateur puis sa connexion externe demande deux enregistrements; lie le code à PostgreSQL 18 |

### Dates

| Option | Avantages | Inconvénients |
|---|---|---|
| A. `DateTime` en UTC par convention | Connu de tous | Le type ne porte qu'une étiquette (`Kind`) qui se perd à la sérialisation; la comparaison de deux valeurs ignore l'étiquette; la discipline repose sur chaque ligne de code |
| B. `DateTimeOffset` en UTC | L'écart fait partie de la valeur; la comparaison porte sur l'instant; compatible avec `TimeProvider`; le fournisseur refuse tout écart non nul | Type moins familier; l'écart d'origine n'est pas conservé par la base |
| C. `Instant` de la bibliothèque NodaTime | Modèle le plus rigoureux du temps | Dépendance supplémentaire, sans besoin réel pour le projet |
| D. Heure fournie seulement par la base (`now()`), sans horloge dans le code | Aucune horloge à injecter | Dans les tests sans base, l'heure doit être simulée par le dépôt factice, ce qui est moins direct qu'une horloge injectée; `modifie_le` et `derniere_connexion_le` seraient de toute façon écrits par le code |

## Décision

- **Identifiants : option C.** Les clés primaires sont des GUID de version 7, créés par le code (`Guid.CreateVersion7()`) au moment de la création de l'entité, jamais par la base. L'option D (`uuidv7()` de PostgreSQL 18) est écartée parce que connaître la clé avant l'enregistrement simplifie la logique d'affaires : un dépôt factice pourrait simuler la génération, mais le test ne vérifierait plus que le code crée la clé, et la connexion externe d'un nouveau compte, qui référence l'utilisateur, exigerait un second enregistrement. Une valeur par défaut `uuidv7()` dans la colonne, en filet de sécurité pour les insertions manuelles, n'est pas retenue : elle donnerait deux sources pour la même valeur. Le code d'amitié, et non l'identifiant, sert à retrouver une personne.
- **Dates : option B.** Toutes les dates du code sont des `DateTimeOffset` en UTC; les colonnes sont de type `timestamptz`.
- **Horloge :** le code obtient l'heure par un `TimeProvider` injecté : le fournisseur réel de .NET (`TimeProvider.System`) dans l'application, quel que soit l'environnement, et un `FakeTimeProvider` dans les tests.
- **Horodatages :** `cree_le` est rempli par la base (`DEFAULT now()`). `modifie_le` reçoit `now()` à l'insertion, puis est mis à jour par un intercepteur de `SaveChanges` qui utilise le `TimeProvider` et ignore les modifications qui ne touchent que des colonnes techniques comme `derniere_connexion_le` ([ADR 0008](0008-logique-d-affaires-testable.md)). Dans les entités, `CreeLe` et `ModifieLe` n'ont pas de mutateur public : seul le projet de données les écrit.
- **Conversion en heure locale :** elle se fait uniquement à l'affichage, dans l'interface, selon le fuseau horaire du navigateur de la personne.

## Conséquences

### Positives

- Confondre une heure locale et une heure UTC devient très improbable : le type porte l'écart et la base refuse tout autre écart que zéro.
- Les insertions se font à la fin des index (GUID version 7), ce qui limite la fragmentation.
- Une clé est connue avant l'enregistrement : l'utilisateur et sa connexion externe se créent dans la même opération, sans second aller-retour.
- Le temps se simule dans les tests (logique d'affaires et intercepteur), sans base de données.
- Seul le projet de données peut modifier `CreeLe` et `ModifieLe` : le compilateur l'impose.

### Négatives

- Un GUID de version 7 révèle le moment de création de la ligne. Acceptable tant que les identifiants ne sont pas des secrets ni affichés en clair à d'autres personnes.
- `DateTimeOffset` est moins familier que `DateTime`, et l'écart d'origine n'est pas conservé : à la lecture, il vaut toujours zéro.
- Il y a deux horloges : `cree_le` vient de la base, `modifie_le` du code. L'écart est négligeable pour l'usage prévu, et `modifie_le` ne sert pas à l'audit (une modification manuelle en SQL ne le met pas à jour).
- Les tests qui ont besoin d'un `ModifieLe` précis doivent passer par l'entrée du suivi des changements d'EF Core, faute de mutateur public.
- Toute heure affichée doit être convertie vers le fuseau du navigateur.

## Sources

- [Choisir entre DateTime, DateTimeOffset, TimeSpan et TimeZoneInfo (Microsoft)](https://learn.microsoft.com/en-us/dotnet/standard/datetime/choosing-between-datetime)
- [Dates et heures avec Npgsql](https://www.npgsql.org/doc/types/datetime.html)
- [RFC 9562 : UUID, dont la version 7](https://www.rfc-editor.org/rfc/rfc9562)

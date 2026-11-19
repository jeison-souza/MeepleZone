---
title: "Tester EF Core : le modèle sans base, le schéma avec une vraie base"
date: 2026-11-15
lastmod: 2026-11-19
draft: false
tags: ["Tests", "EF Core", "PostgreSQL", "Jalon 3"]
categories: ["Documentation"]
---

Une base de données relationnelle porte des règles : types de colonnes, valeurs par défaut, contraintes `CHECK`, unicité, clés étrangères et leur comportement à la suppression. Avec EF Core, ces règles sont écrites en C# dans des configurations, puis traduites en SQL par les migrations. La question est donc de savoir **où et comment les tester**, sans alourdir un projet maintenu par une seule personne. La réponse retenue tient en trois niveaux, qui ne prouvent pas la même chose.

## 1. Le point de départ : pas de base dans les tests

L'[ADR 0008](https://github.com/jeison-souza/MeepleZone/blob/main/docs/adr/0008-logique-d-affaires-testable.md) a posé la règle : les tests automatisés portent sur la logique d'affaires, avec des dépôts factices, sans base réelle ni conteneur. Les motifs sont l'outillage (aucun Docker requis), la vitesse d'exécution et la taille du projet.

Cette règle a un coût, noté dans le même ADR : les contraintes de la base et les règles du code peuvent diverger, et les dépôts, qui contiennent les requêtes EF Core, ne sont pas testés. D'où les deux autres niveaux.

## 2. Niveau 1 : tester le modèle d'EF Core, sans connexion

Le modèle d'EF Core est un objet que l'on peut interroger **sans ouvrir de connexion** : il décrit les tables, les colonnes, les contraintes et les index tels que la migration les produira. Un test peut donc vérifier que la configuration dit bien ce que le schéma publié annonce.

Une *fixture* (un objet de préparation partagé entre les tests) construit le contexte une seule fois par classe de tests, avec un hôte qui n'existe pas :

```csharp
var options = new DbContextOptionsBuilder<MeepleZoneContexte>()
    .UtiliserPostgreSql("Host=hote-inexistant")
    .Options;

Contexte = new MeepleZoneContexte(options);
Modele = Contexte.GetService<IDesignTimeModel>().Model;
```

Le détail qui compte est la dernière ligne. Le modèle d'exécution (`Contexte.Model`) ne contient ni la collation ni les contraintes `CHECK` : seul le **modèle de conception** les a, et c'est lui qu'utilise la migration. Tester le mauvais modèle donne des tests qui passent sans rien vérifier.

Un test typique vérifie une contrainte et son SQL :

```csharp
[Theory]
[InlineData("ck_possession_note", "note BETWEEN 1 AND 10")]
[InlineData("ck_possession_parties_jouees", "parties_jouees >= 0")]
public void Possession_ContrainteCheck_EstDeclareeAvecSonSql(string nom, string sqlAttendu)
{
    var type = _modele.TypeEntite<Possession>();
    var contrainte = type.GetCheckConstraints().SingleOrDefault(c => c.Name == nom);

    contrainte.Should().NotBeNull();
    contrainte!.Sql.Should().Be(sqlAttendu);
}
```

Les mêmes tests couvrent, pour chaque entité : le nom de la table, la clé primaire et son nom, la nullabilité de chaque colonne, le type de colonne, les valeurs par défaut, le comportement des clés étrangères (`CASCADE`, `RESTRICT`, `SET NULL`) avec leur nom, l'unicité de chaque index et l'absence de collation là où elle n'est pas voulue.

### La règle d'or : l'attendu ne vient jamais du code testé

Un test dont la valeur attendue est copiée de la configuration ne prouve rien : si la configuration est fausse, le test l'est aussi, et il passe. Les valeurs attendues viennent donc du [schéma de données publié](https://github.com/jeison-souza/MeepleZone/blob/main/docs/schema/schema-donnees.md), jamais du code.

Deux cas réels l'ont montré :

- Une clé étrangère (`photo_id`) avait été configurée en `CASCADE`, par copie de la configuration d'une autre entité. Le test, écrit à l'identique, passait. Le schéma disait `SET NULL`.
- Un test de type de colonne comparait le nom du type. `GetColumnType()` renvoie `timestamp with time zone`, et non `timestamptz`. Un avertissement de l'analyseur (paramètre de `[Theory]` inutilisé) a révélé que le test ne comparait pas ce qu'il croyait comparer.

## 3. Niveau 2 : la logique d'affaires, avec des dépôts factices

Les règles du domaine (limites, validations qui reflètent les `CHECK`, résultats énumérés des dépôts) se testent avec des dépôts factices, sans EF Core. C'est le niveau le plus rapide, et il couvre la plus grande partie des tests de la solution. Il ne dit rien sur la base : un dépôt factice ne viole jamais une contrainte.

## 4. Niveau 3 : valider le schéma contre une vraie base

Ce que les niveaux 1 et 2 ne peuvent pas prouver : que PostgreSQL **applique** réellement les contraintes, que les migrations donnent bien le schéma publié, et que les requêtes des dépôts fonctionnent. Pour combler l'écart, sans introduire de conteneur dans les tests automatisés, des scripts SQL sont exécutés à la main contre la base de développement, une fois les migrations appliquées.

Le principe tient en quelques lignes. Chaque script s'exécute dans une transaction qui se termine par un `ROLLBACK` : rien ne reste dans la base. Une fonction temporaire exécute chaque cas et note soit `ok`, soit le **nom de la contrainte violée** :

```sql
CREATE FUNCTION pg_temp.essayer(p_test text, p_sql text, p_attendu text) RETURNS void
LANGUAGE plpgsql AS $f$
DECLARE v_obtenu text;
BEGIN
    BEGIN
        EXECUTE p_sql;
        v_obtenu := 'ok';
    EXCEPTION WHEN OTHERS THEN
        GET STACKED DIAGNOSTICS v_obtenu = CONSTRAINT_NAME;
        v_obtenu := COALESCE(NULLIF(v_obtenu, ''), SQLSTATE);
    END;
    INSERT INTO resultat VALUES (p_test, p_attendu, v_obtenu);
END
$f$;
```

Un cas se lit alors comme une phrase : « insérer une note de 11 doit échouer sur `ck_possession_note` ».

```sql
SELECT pg_temp.essayer('note = 11',
    $q$ INSERT INTO possession (id, utilisateur_id, jeu_id, note)
        VALUES (gen_random_uuid(), '…', '…', 11) $q$,
    'ck_possession_note');
```

La dernière requête du script compare l'attendu à l'obtenu, et toutes les lignes doivent afficher `ok = true`. Comme au niveau 1, **le résultat attendu est le nom que le schéma publié annonce**, jamais une valeur copiée du code testé.

Sept scripts couvrent, au total, 145 cas : contraintes `CHECK`, unicité, clés étrangères, suppressions (`RESTRICT`, `CASCADE`, `SET NULL`), valeurs par défaut, lecture de la collection avec pagination, recherche sans accents, filtres par utilisateur, atomicité de la création d'un jeu avec ses mécaniques. Le détail est dans le [README du dossier de validation](https://github.com/jeison-souza/MeepleZone/blob/main/docs/validation/README.md).

### Ce que la première exécution a révélé

Les 145 cas ont été exécutés les 11 et 12 novembre contre la base de développement, et tous sont conformes. La première exécution avait toutefois échoué, sur un problème que ni le niveau 1 ni le niveau 2 ne pouvaient voir : les tables du catalogue avaient été créées avec un compte d'administration du serveur. Ce compte en était donc devenu le propriétaire, et le compte que l'application utilise n'y avait aucun accès (`permission denied`). Le site aurait échoué partout. La correction (propriétaire et privilèges) a été faite avant que le site s'en serve, et la règle est maintenant écrite : les migrations ne s'appliquent jamais avec un compte d'administration ([ADR 0007](https://github.com/jeison-souza/MeepleZone/blob/main/docs/adr/0007-strategie-de-migrations.md)).

Un autre `false` de la première exécution venait du script lui-même (un couple utilisateur-jeu répété par erreur), et c'est l'unicité qui a répondu correctement. Un résultat négatif ne veut donc pas toujours dire que le schéma est fautif : le cas de test lui-même peut contenir l'erreur, et il faut le relire avant de blâmer le schéma.

## 5. Ce que chaque niveau prouve

| Niveau | Prouve | Ne prouve pas |
|---|---|---|
| 1 : modèle d'EF Core, sans base | La configuration dit ce que le schéma publié annonce (types, noms, `CHECK`, clés, index) | Que PostgreSQL applique ces règles; que les migrations produisent bien ce modèle |
| 2 : logique d'affaires, dépôts factices | Les règles du domaine, sans base | Les requêtes, les contraintes réelles |
| 3 : scripts contre une vraie base | Les contraintes, les suppressions, les droits et les requêtes en SQL se comportent comme annoncé | Le SQL qu'EF Core génère pour chaque requête (les scripts réécrivent la logique en SQL à la main, sans passer par EF Core) |

Ces limites sont écrites dans le README des scripts. Elles relèvent d'un test d'intégration contre un PostgreSQL réel, prévu avec l'exécution des migrations dans la chaîne d'intégration continue.

## 6. Quelques pièges rencontrés

- **Tester le bon modèle.** `Contexte.Model` n'a ni collation ni `CHECK`; le modèle de conception, oui.
- **La clé primaire n'est pas un index** dans `GetIndexes()` : elle se lit avec `FindPrimaryKey()`.
- **`index.Name` est `null`** quand le nom n'a pas été donné à la main : le nom calculé se lit avec `GetDatabaseName()`.
- **Les noms de clés étrangères se donnent à la main.** Le nom calculé utilise le nom par défaut de l'autre table, qui dépend de l'ordre alphabétique dans lequel les configurations sont appliquées : il peut donc changer quand une entité s'ajoute.
- **`[InlineData]` n'accepte que des constantes**, et pas de `decimal`. Contournement : passer un `int?` en paramètre, puis le convertir en `decimal` dans une méthode d'aide du test.
- **Le tri des noms dépend de la collation de la base.** Dans les scripts, grouper par identifiant plutôt que par nom, sinon un accent ou une espace peut inverser l'ordre.
- **Avant d'appliquer ou de valider : `SELECT current_user;`.** Les scripts récents le font à la première ligne.

## 7. La suite : automatiser le niveau 3

Rien n'empêche le niveau 3 de devenir un test automatique contre un vrai PostgreSQL, dès que le projet aura la maturité pour le soutenir. C'est d'ailleurs ce que recommande la documentation de Microsoft : dans la vue d'ensemble « [Overview of testing applications that use EF Core](https://learn.microsoft.com/en-us/ef/core/testing/) » (en anglais), il est conseillé de tester au moins une partie du code contre le même système de base de données qu'en production, parce que rien d'autre ne montre que l'application fonctionne avec lui. Le fournisseur « en mémoire », en particulier, y est déconseillé, car il ne se comporte pas comme une vraie base. La page « [Testing against your Production Database System](https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database) » décrit les techniques d'isolation, dont les transactions annulées à la fin du test.

La règle posée par l'[ADR 0008](https://github.com/jeison-souza/MeepleZone/blob/main/docs/adr/0008-logique-d-affaires-testable.md), pas de base dans les tests automatisés, est donc un choix de départ, pas un principe définitif : elle répond à des contraintes d'outillage (aucun Docker requis), de vitesse et de taille du projet. Les scripts de validation en tirent déjà parti : ils emploient la même technique de transaction annulée, et leurs cas, dont l'attendu vient du schéma publié, se prêtent bien à une transposition en tests automatisés. Reste à décider quand et comment : un conteneur éphémère, ou une base dédiée à la chaîne d'intégration continue.

## 8. À retenir

- Un test dont l'attendu est copié du code ne prouve rien : l'attendu vient du schéma publié.
- Un modèle testé sans base est rapide et utile, mais il ne remplace pas une exécution contre un vrai PostgreSQL : le problème de droits n'était visible qu'à ce niveau.
- Un script de validation peut lui aussi contenir une erreur : devant un échec, vérifier d'abord le cas avant de conclure que le schéma est fautif.
- Les limites de chaque niveau se disent noir sur blanc, pour ne pas confondre « testé » et « prouvé ».

## Mise à jour du 19 novembre

Cet article a été rédigé le 15 novembre, et les chiffres ci-dessus sont ceux de ce jour-là. Depuis, six scripts se sont ajoutés (comptes techniques, liaison des fournisseurs de connexion, suppression d'un compte, filtre de la collection, colonne de recherche et recherche par trigrammes). Le dossier de validation compte maintenant 13 scripts et 236 cas, exécutés du 11 au 17 novembre contre la base de développement, et tous sont conformes. Les trois niveaux et leurs limites n'ont pas changé.

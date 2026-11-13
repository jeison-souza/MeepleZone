-- Validation de la requête de DepotCollection.ListerPossessionsAsync sur la base de développement.
-- Reproduit en SQL la requête du dépôt : jointure possession/jeu filtrée par utilisateur, total, tri par nom puis par id, saut et prise.
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Exécuter en entier (Run all). Résultat attendu : toutes les lignes de la dernière requête avec ok = true.

BEGIN;

CREATE TEMP TABLE resultat (test text, attendu text, obtenu text);

-- Même logique que le dépôt : le nom du jeu, puis l'id de la possession, départagent les ex aequo.
CREATE FUNCTION pg_temp.page(p_utilisateur uuid, p_saut int, p_prendre int) RETURNS text
LANGUAGE sql AS $f$
    SELECT COALESCE(string_agg(t.nom || ':' || right(t.id::text, 2), ',' ORDER BY t.nom, t.id), '')
    FROM (
        SELECT j.nom, p.id
        FROM possession p
        JOIN jeu j ON j.id = p.jeu_id
        WHERE p.utilisateur_id = p_utilisateur
        ORDER BY j.nom, p.id
        OFFSET p_saut LIMIT p_prendre
    ) t
$f$;

CREATE FUNCTION pg_temp.total(p_utilisateur uuid) RETURNS text
LANGUAGE sql AS $f$
    SELECT count(*)::text
    FROM possession p
    JOIN jeu j ON j.id = p.jeu_id
    WHERE p.utilisateur_id = p_utilisateur
$f$;

-- Données : deux utilisateurs, quatre jeux dont deux portent le même nom ("Catan").
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH'),
    ('00000000-0000-0000-0000-0000000000a2', 'Essai 2', 'JKLMNPQR');
INSERT INTO jeu (id, nom, cree_par_id) VALUES
    ('00000000-0000-0000-0000-0000000000c1', 'Catan', '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000c2', 'Azul',  '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000c3', 'Catan', '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000c4', 'Brass', '00000000-0000-0000-0000-0000000000a1');
-- Utilisateur 1 : Catan (d1), Azul (d2), Catan bis (d3). Utilisateur 2 : Brass (d4).
INSERT INTO possession (id, utilisateur_id, jeu_id) VALUES
    ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1'),
    ('00000000-0000-0000-0000-0000000000d2', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c2'),
    ('00000000-0000-0000-0000-0000000000d3', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c3'),
    ('00000000-0000-0000-0000-0000000000d4', '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c4');

-- Total : seulement les possessions de l'utilisateur demandé.
INSERT INTO resultat VALUES ('total utilisateur 1', '3', pg_temp.total('00000000-0000-0000-0000-0000000000a1'));
INSERT INTO resultat VALUES ('total utilisateur 2', '1', pg_temp.total('00000000-0000-0000-0000-0000000000a2'));
INSERT INTO resultat VALUES ('total utilisateur inconnu', '0', pg_temp.total(gen_random_uuid()));

-- Pages de 2 (page 1 : saut 0, page 2 : saut 2, page 3 : saut 4).
INSERT INTO resultat VALUES ('page 1 de 2 (tri par nom, ex aequo par id)', 'Azul:d2,Catan:d1', pg_temp.page('00000000-0000-0000-0000-0000000000a1', 0, 2));
INSERT INTO resultat VALUES ('page 2 de 2', 'Catan:d3', pg_temp.page('00000000-0000-0000-0000-0000000000a1', 2, 2));
INSERT INTO resultat VALUES ('page au-dela de la fin', '', pg_temp.page('00000000-0000-0000-0000-0000000000a1', 4, 2));

-- Une seule page contient tout, sans doublon ni oubli entre les pages.
INSERT INTO resultat VALUES ('taille 24, une seule page', 'Azul:d2,Catan:d1,Catan:d3', pg_temp.page('00000000-0000-0000-0000-0000000000a1', 0, 24));
INSERT INTO resultat VALUES ('page 1 de 1 puis page 2 de 1 puis page 3 de 1 couvrent tout',
    'Azul:d2|Catan:d1|Catan:d3',
    pg_temp.page('00000000-0000-0000-0000-0000000000a1', 0, 1) || '|' ||
    pg_temp.page('00000000-0000-0000-0000-0000000000a1', 1, 1) || '|' ||
    pg_temp.page('00000000-0000-0000-0000-0000000000a1', 2, 1));

-- Isolation : la collection de l'utilisateur 2 ne contient pas les jeux de l'utilisateur 1.
INSERT INTO resultat VALUES ('collection de l''utilisateur 2', 'Brass:d4', pg_temp.page('00000000-0000-0000-0000-0000000000a2', 0, 24));
INSERT INTO resultat VALUES ('utilisateur sans possession', '', pg_temp.page(gen_random_uuid(), 0, 24));

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

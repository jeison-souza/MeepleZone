-- Validation de la création d'un jeu sur la base de développement (DepotCollection.CreerJeuAsync et ValidationJeu).
-- Vérifie que chaque règle du domaine correspond bien à une contrainte CHECK de la table jeu, que les violations de clé étrangère
-- portent les noms que le dépôt traduit en « référence invalide » (fk_jeu_mode_jeu_mode_jeu_id et fk_jeu_mecanique_mecanique_mecanique_id),
-- et que la création du jeu, de ses mécaniques et de la possession du créateur est atomique.
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Exécuter en entier (Run all) avec le compte de déploiement (la première requête affiche le compte utilisé).
-- Résultat attendu : toutes les lignes de la dernière requête avec ok = true.

SELECT current_user;

BEGIN;

CREATE TEMP TABLE resultat (test text, attendu text, obtenu text);

CREATE FUNCTION pg_temp.essayer(p_test text, p_sql text, p_attendu text) RETURNS void
LANGUAGE plpgsql AS $f$
DECLARE
    v_obtenu text;
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

INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH');

-- Les règles du domaine reprennent les CHECK de la table : chaque cas refusé par ValidationJeu l'est aussi par la base.
SELECT pg_temp.essayer('nom vide', $q$ INSERT INTO jeu (id, nom, cree_par_id) VALUES (gen_random_uuid(), '', '00000000-0000-0000-0000-0000000000a1') $q$, 'ck_jeu_nom');
SELECT pg_temp.essayer('nom de 255 caracteres', format($q$ INSERT INTO jeu (id, nom, cree_par_id) VALUES (gen_random_uuid(), %L, '00000000-0000-0000-0000-0000000000a1') $q$, repeat('a', 255)), 'ok');
SELECT pg_temp.essayer('nom de 256 caracteres', format($q$ INSERT INTO jeu (id, nom, cree_par_id) VALUES (gen_random_uuid(), %L, '00000000-0000-0000-0000-0000000000a1') $q$, repeat('a', 256)), 'ck_jeu_nom');

SELECT pg_temp.essayer('joueurs_min = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_min) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ck_jeu_joueurs_min');
SELECT pg_temp.essayer('joueurs_max = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_max) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ck_jeu_joueurs_max');
SELECT pg_temp.essayer('joueurs_meilleur = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_meilleur) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ck_jeu_joueurs_meilleur');
SELECT pg_temp.essayer('duree_min = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, duree_min_minutes) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ck_jeu_duree_min_minutes');
SELECT pg_temp.essayer('duree_max = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, duree_max_minutes) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ck_jeu_duree_max_minutes');
SELECT pg_temp.essayer('age_min = -1', $q$ INSERT INTO jeu (id, nom, cree_par_id, age_min) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', -1) $q$, 'ck_jeu_age_min');
SELECT pg_temp.essayer('age_min = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, age_min) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ok');
SELECT pg_temp.essayer('complexite = 0.9', $q$ INSERT INTO jeu (id, nom, cree_par_id, complexite) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 0.9) $q$, 'ck_jeu_complexite');
SELECT pg_temp.essayer('complexite = 5.1', $q$ INSERT INTO jeu (id, nom, cree_par_id, complexite) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 5.1) $q$, 'ck_jeu_complexite');
SELECT pg_temp.essayer('complexite = 1.0', $q$ INSERT INTO jeu (id, nom, cree_par_id, complexite) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 1.0) $q$, 'ok');
SELECT pg_temp.essayer('complexite = 5.0', $q$ INSERT INTO jeu (id, nom, cree_par_id, complexite) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 5.0) $q$, 'ok');
-- Une complexité de 4.96 passe la règle du domaine (entre 1 et 5) et la base l'arrondit à 5.0 (numeric(2,1)) sans refus.
SELECT pg_temp.essayer('complexite = 4.96 (arrondie a 5.0)', $q$ INSERT INTO jeu (id, nom, cree_par_id, complexite) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 4.96) $q$, 'ok');

SELECT pg_temp.essayer('joueurs_min > joueurs_max', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_min, joueurs_max) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 4, 2) $q$, 'ck_jeu_joueurs_min_max');
SELECT pg_temp.essayer('joueurs_min = joueurs_max', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_min, joueurs_max) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 2, 2) $q$, 'ok');
SELECT pg_temp.essayer('meilleur < min', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_min, joueurs_max, joueurs_meilleur) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 3, 4, 2) $q$, 'ck_jeu_joueurs_meilleur_min');
SELECT pg_temp.essayer('meilleur > max', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_min, joueurs_max, joueurs_meilleur) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 3, 4, 5) $q$, 'ck_jeu_joueurs_meilleur_max');
SELECT pg_temp.essayer('meilleur entre min et max', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_min, joueurs_max, joueurs_meilleur) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 3, 4, 3) $q$, 'ok');
SELECT pg_temp.essayer('duree_min > duree_max', $q$ INSERT INTO jeu (id, nom, cree_par_id, duree_min_minutes, duree_max_minutes) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 60, 30) $q$, 'ck_jeu_duree_min_max');
SELECT pg_temp.essayer('duree_min = duree_max', $q$ INSERT INTO jeu (id, nom, cree_par_id, duree_min_minutes, duree_max_minutes) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', 60, 60) $q$, 'ok');

-- Clés étrangères que le dépôt traduit en « référence invalide ».
SELECT pg_temp.essayer('mode de jeu inexistant', $q$ INSERT INTO jeu (id, nom, cree_par_id, mode_jeu_id) VALUES (gen_random_uuid(), 'X', '00000000-0000-0000-0000-0000000000a1', gen_random_uuid()) $q$, 'fk_jeu_mode_jeu_mode_jeu_id');
SELECT pg_temp.essayer('mecanique inexistante', $q$
    DO $x$ DECLARE v uuid := gen_random_uuid(); BEGIN
        INSERT INTO jeu (id, nom, cree_par_id) VALUES (v, 'X', '00000000-0000-0000-0000-0000000000a1');
        INSERT INTO jeu_mecanique (jeu_id, mecanique_id) VALUES (v, gen_random_uuid());
    END $x$ $q$, 'fk_jeu_mecanique_mecanique_mecanique_id');

-- Atomicité : si une mécanique est refusée, le jeu et sa possession ne restent pas (une seule transaction dans le dépôt).
SELECT pg_temp.essayer('creation avec mecanique inexistante : rien ne reste', $q$
    DO $x$ BEGIN
        INSERT INTO jeu (id, nom, cree_par_id) VALUES ('00000000-0000-0000-0000-0000000000c9', 'Jeu atomique', '00000000-0000-0000-0000-0000000000a1');
        INSERT INTO possession (id, utilisateur_id, jeu_id) VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c9');
        INSERT INTO jeu_mecanique (jeu_id, mecanique_id) VALUES ('00000000-0000-0000-0000-0000000000c9', gen_random_uuid());
    END $x$ $q$, 'fk_jeu_mecanique_mecanique_mecanique_id');
INSERT INTO resultat SELECT 'le jeu de la creation refusee n''existe pas', '0', (SELECT count(*)::text FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c9');
INSERT INTO resultat SELECT 'la possession de la creation refusee n''existe pas', '0', (SELECT count(*)::text FROM possession WHERE jeu_id = '00000000-0000-0000-0000-0000000000c9');

-- Chemin heureux : un jeu en attente, deux mécaniques existantes et la possession du créateur.
INSERT INTO jeu (id, nom, cree_par_id, mode_jeu_id, statut) VALUES
    ('00000000-0000-0000-0000-0000000000c1', 'Jeu cree', '00000000-0000-0000-0000-0000000000a1', (SELECT id FROM mode_jeu ORDER BY nom LIMIT 1), 'en_attente');
INSERT INTO jeu_mecanique (jeu_id, mecanique_id) SELECT '00000000-0000-0000-0000-0000000000c1', id FROM mecanique ORDER BY nom LIMIT 2;
INSERT INTO possession (id, utilisateur_id, jeu_id) VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1');
INSERT INTO resultat SELECT 'jeu en attente cree', 'en_attente', (SELECT statut FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c1');
INSERT INTO resultat SELECT 'deux mecaniques liees', '2', (SELECT count(*)::text FROM jeu_mecanique WHERE jeu_id = '00000000-0000-0000-0000-0000000000c1');
INSERT INTO resultat SELECT 'possession du createur', '1', (SELECT count(*)::text FROM possession WHERE jeu_id = '00000000-0000-0000-0000-0000000000c1' AND utilisateur_id = '00000000-0000-0000-0000-0000000000a1');

-- La même mécanique deux fois pour un jeu est refusée par la clé primaire : le gestionnaire retire les doublons avant d'écrire.
SELECT pg_temp.essayer('meme mecanique deux fois', $q$
    INSERT INTO jeu_mecanique (jeu_id, mecanique_id)
    SELECT '00000000-0000-0000-0000-0000000000c1', mecanique_id FROM jeu_mecanique WHERE jeu_id = '00000000-0000-0000-0000-0000000000c1' LIMIT 1 $q$, 'pk_jeu_mecanique');

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

-- Validation du catalogue sur la base de développement (migration CreationTablesCatalogue).
-- Ce script ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Étape 1 : appliquer docs/schema/schema-idempotent.sql (deux fois : la seconde ne doit rien changer).
-- Étape 2 : exécuter ce script en entier (Run all). Résultat attendu : toutes les lignes de
--           la dernière requête avec ok = true.

-- ---------------------------------------------------------------------------
-- A. Constats (lecture seule). Attendu : 4 modes, 23 mécaniques, 0 jeu, 0 lien.
-- ---------------------------------------------------------------------------
SELECT
    (SELECT count(*) FROM mode_jeu)      AS modes_jeu,
    (SELECT count(*) FROM mecanique)     AS mecaniques,
    (SELECT count(*) FROM jeu)           AS jeux,
    (SELECT count(*) FROM jeu_mecanique) AS liens;

-- Attendu : la migration 20261111190649_CreationTablesCatalogue figure en tête.
SELECT migration_id FROM historique_migration ORDER BY migration_id DESC LIMIT 3;

-- ---------------------------------------------------------------------------
-- B. Essais de contraintes, dans une transaction annulée à la fin.
-- ---------------------------------------------------------------------------
BEGIN;

CREATE TEMP TABLE resultat (test text, attendu text, obtenu text);

-- Tente le SQL; note « ok » s'il passe, sinon le nom de la contrainte violée (ou le SQLSTATE).
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

-- Données de départ : deux utilisateurs et une annexe.
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH'),
    ('00000000-0000-0000-0000-0000000000a2', 'Essai 2', 'JKLMNPQR');
INSERT INTO annexe (id, nom_fichier, contenu, type_mime, taille_octets, cree_par_id) VALUES
    ('00000000-0000-0000-0000-0000000000b1', 'essai.webp', '\x00', 'image/webp', 1,
     '00000000-0000-0000-0000-0000000000a1');

-- Jeu de base : seuls nom et cree_par_id sont fournis (valeurs par défaut du schéma).
SELECT pg_temp.essayer('jeu minimal accepte', $q$
    INSERT INTO jeu (id, nom, cree_par_id)
    VALUES ('00000000-0000-0000-0000-0000000000c1', 'Jeu essai', '00000000-0000-0000-0000-0000000000a1') $q$, 'ok');
SELECT pg_temp.essayer('valeurs par defaut de jeu', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c1'
                       AND statut = 'en_attente' AND cree_le IS NOT NULL AND modifie_le IS NOT NULL) THEN
            RAISE EXCEPTION 'valeurs par defaut absentes';
        END IF;
    END $x$ $q$, 'ok');

-- CHECK de colonne.
SELECT pg_temp.essayer('nom vide', $q$ INSERT INTO jeu (id, nom, cree_par_id) VALUES (gen_random_uuid(), '', '00000000-0000-0000-0000-0000000000a1') $q$, 'ck_jeu_nom');
SELECT pg_temp.essayer('nom de 256 caracteres', $q$ INSERT INTO jeu (id, nom, cree_par_id) VALUES (gen_random_uuid(), repeat('a', 256), '00000000-0000-0000-0000-0000000000a1') $q$, 'ck_jeu_nom');
SELECT pg_temp.essayer('nom de 255 caracteres', $q$ INSERT INTO jeu (id, nom, cree_par_id) VALUES (gen_random_uuid(), repeat('a', 255), '00000000-0000-0000-0000-0000000000a1') $q$, 'ok');
SELECT pg_temp.essayer('joueurs_min = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_min) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ck_jeu_joueurs_min');
SELECT pg_temp.essayer('joueurs_max = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_max) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ck_jeu_joueurs_max');
SELECT pg_temp.essayer('joueurs_meilleur = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_meilleur) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ck_jeu_joueurs_meilleur');
SELECT pg_temp.essayer('duree_min = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, duree_min_minutes) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ck_jeu_duree_min_minutes');
SELECT pg_temp.essayer('duree_max = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, duree_max_minutes) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ck_jeu_duree_max_minutes');
SELECT pg_temp.essayer('age_min = -1', $q$ INSERT INTO jeu (id, nom, cree_par_id, age_min) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', -1) $q$, 'ck_jeu_age_min');
SELECT pg_temp.essayer('age_min = 0', $q$ INSERT INTO jeu (id, nom, cree_par_id, age_min) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 0) $q$, 'ok');
SELECT pg_temp.essayer('complexite = 0.9', $q$ INSERT INTO jeu (id, nom, cree_par_id, complexite) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 0.9) $q$, 'ck_jeu_complexite');
SELECT pg_temp.essayer('complexite = 5.1', $q$ INSERT INTO jeu (id, nom, cree_par_id, complexite) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 5.1) $q$, 'ck_jeu_complexite');
SELECT pg_temp.essayer('complexite = 1.0', $q$ INSERT INTO jeu (id, nom, cree_par_id, complexite) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 1.0) $q$, 'ok');
SELECT pg_temp.essayer('complexite = 5.0', $q$ INSERT INTO jeu (id, nom, cree_par_id, complexite) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 5.0) $q$, 'ok');
SELECT pg_temp.essayer('complexite = 10.0 (numeric(2,1) : debordement)', $q$ INSERT INTO jeu (id, nom, cree_par_id, complexite) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 10.0) $q$, '22003');
SELECT pg_temp.essayer('statut inconnu', $q$ INSERT INTO jeu (id, nom, cree_par_id, statut) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 'xyz') $q$, 'ck_jeu_statut');
SELECT pg_temp.essayer('statut valide', $q$ INSERT INTO jeu (id, nom, cree_par_id, statut) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 'valide') $q$, 'ok');

-- CHECK de table.
SELECT pg_temp.essayer('joueurs_min > joueurs_max', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_min, joueurs_max) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 5, 3) $q$, 'ck_jeu_joueurs_min_max');
SELECT pg_temp.essayer('joueurs_meilleur < joueurs_min', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_min, joueurs_meilleur) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 3, 2) $q$, 'ck_jeu_joueurs_meilleur_min');
SELECT pg_temp.essayer('joueurs_meilleur > joueurs_max', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_max, joueurs_meilleur) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 4, 5) $q$, 'ck_jeu_joueurs_meilleur_max');
SELECT pg_temp.essayer('joueurs 2 a 4, meilleur 3', $q$ INSERT INTO jeu (id, nom, cree_par_id, joueurs_min, joueurs_max, joueurs_meilleur) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 2, 4, 3) $q$, 'ok');
SELECT pg_temp.essayer('duree_min > duree_max', $q$ INSERT INTO jeu (id, nom, cree_par_id, duree_min_minutes, duree_max_minutes) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 60, 30) $q$, 'ck_jeu_duree_min_max');
SELECT pg_temp.essayer('couverture fichier et url', $q$ INSERT INTO jeu (id, nom, cree_par_id, couverture_id, couverture_url) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1', 'https://exemple.invalid/c.jpg') $q$, 'ck_jeu_couverture');
SELECT pg_temp.essayer('couverture url seule', $q$ INSERT INTO jeu (id, nom, cree_par_id, couverture_url) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 'https://exemple.invalid/c.jpg') $q$, 'ok');

-- Unicité.
SELECT pg_temp.essayer('bgg_id premier', $q$ INSERT INTO jeu (id, nom, cree_par_id, bgg_id) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', 1234) $q$, 'ok');
SELECT pg_temp.essayer('bgg_id en double', $q$ INSERT INTO jeu (id, nom, cree_par_id, bgg_id) VALUES (gen_random_uuid(), 'y', '00000000-0000-0000-0000-0000000000a1', 1234) $q$, 'ix_jeu_bgg_id');
SELECT pg_temp.essayer('plusieurs jeux sans bgg_id', $q$ INSERT INTO jeu (id, nom, cree_par_id) VALUES (gen_random_uuid(), 'z', '00000000-0000-0000-0000-0000000000a1') $q$, 'ok');
SELECT pg_temp.essayer('mode_jeu nom en double', $q$ INSERT INTO mode_jeu (id, nom) VALUES (gen_random_uuid(), 'Coopératif') $q$, 'ix_mode_jeu_nom');
SELECT pg_temp.essayer('mecanique nom en double', $q$ INSERT INTO mecanique (id, nom) VALUES (gen_random_uuid(), 'Bluff') $q$, 'ix_mecanique_nom');
SELECT pg_temp.essayer('mecanique bgg_id premier', $q$ INSERT INTO mecanique (id, nom, bgg_id) VALUES (gen_random_uuid(), 'Essai A', 9999) $q$, 'ok');
SELECT pg_temp.essayer('mecanique bgg_id en double', $q$ INSERT INTO mecanique (id, nom, bgg_id) VALUES (gen_random_uuid(), 'Essai B', 9999) $q$, 'ix_mecanique_bgg_id');

-- Clés étrangères de jeu.
SELECT pg_temp.essayer('mode_jeu inexistant', $q$ INSERT INTO jeu (id, nom, cree_par_id, mode_jeu_id) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', gen_random_uuid()) $q$, 'fk_jeu_mode_jeu_mode_jeu_id');
SELECT pg_temp.essayer('createur inexistant', $q$ INSERT INTO jeu (id, nom, cree_par_id) VALUES (gen_random_uuid(), 'x', gen_random_uuid()) $q$, 'fk_jeu_utilisateur_cree_par_id');
SELECT pg_temp.essayer('couverture inexistante', $q$ INSERT INTO jeu (id, nom, cree_par_id, couverture_id) VALUES (gen_random_uuid(), 'x', '00000000-0000-0000-0000-0000000000a1', gen_random_uuid()) $q$, 'fk_jeu_annexe_couverture_id');

-- jeu_mecanique. Mécanique « Bluff » : identifiant du jeu de données initiales.
SELECT pg_temp.essayer('lien jeu-mecanique', $q$ INSERT INTO jeu_mecanique (jeu_id, mecanique_id) VALUES ('00000000-0000-0000-0000-0000000000c1', '01a1c7e7-8000-7012-9023-6a29bfaf8326') $q$, 'ok');
SELECT pg_temp.essayer('lien en double', $q$ INSERT INTO jeu_mecanique (jeu_id, mecanique_id) VALUES ('00000000-0000-0000-0000-0000000000c1', '01a1c7e7-8000-7012-9023-6a29bfaf8326') $q$, 'pk_jeu_mecanique');
SELECT pg_temp.essayer('lien avec jeu inexistant', $q$ INSERT INTO jeu_mecanique (jeu_id, mecanique_id) VALUES (gen_random_uuid(), '01a1c7e7-8000-7012-9023-6a29bfaf8326') $q$, 'fk_jeu_mecanique_jeu_jeu_id');
SELECT pg_temp.essayer('lien avec mecanique inexistante', $q$ INSERT INTO jeu_mecanique (jeu_id, mecanique_id) VALUES ('00000000-0000-0000-0000-0000000000c1', gen_random_uuid()) $q$, 'fk_jeu_mecanique_mecanique_mecanique_id');

-- Suppressions (RESTRICT, CASCADE, SET NULL).
SELECT pg_temp.essayer('supprimer une mecanique utilisee', $q$ DELETE FROM mecanique WHERE id = '01a1c7e7-8000-7012-9023-6a29bfaf8326' $q$, 'fk_jeu_mecanique_mecanique_mecanique_id');
SELECT pg_temp.essayer('classer un jeu dans un mode', $q$ UPDATE jeu SET mode_jeu_id = '01a1c7e7-8000-7001-adcc-71b334e95c80' WHERE id = '00000000-0000-0000-0000-0000000000c1' $q$, 'ok');
SELECT pg_temp.essayer('supprimer un mode utilise', $q$ DELETE FROM mode_jeu WHERE id = '01a1c7e7-8000-7001-adcc-71b334e95c80' $q$, 'fk_jeu_mode_jeu_mode_jeu_id');
SELECT pg_temp.essayer('supprimer un utilisateur createur de jeu', $q$
    DO $x$ BEGIN
        INSERT INTO jeu (id, nom, cree_par_id) VALUES ('00000000-0000-0000-0000-0000000000c2', 'Jeu de 2', '00000000-0000-0000-0000-0000000000a2');
        DELETE FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a2';
    END $x$ $q$, 'fk_jeu_utilisateur_cree_par_id');
SELECT pg_temp.essayer('supprimer un jeu supprime ses liens', $q$
    DO $x$ BEGIN
        DELETE FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c1';
        IF EXISTS (SELECT 1 FROM jeu_mecanique WHERE jeu_id = '00000000-0000-0000-0000-0000000000c1') THEN
            RAISE EXCEPTION 'liens restants';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('supprimer une annexe vide la couverture', $q$
    DO $x$ BEGIN
        INSERT INTO jeu (id, nom, cree_par_id, couverture_id) VALUES ('00000000-0000-0000-0000-0000000000c3', 'Jeu couvert', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000b1');
        DELETE FROM annexe WHERE id = '00000000-0000-0000-0000-0000000000b1';
        IF EXISTS (SELECT 1 FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c3' AND couverture_id IS NOT NULL) THEN
            RAISE EXCEPTION 'couverture non videe';
        END IF;
    END $x$ $q$, 'ok');

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

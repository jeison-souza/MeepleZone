-- Validation de la table possession sur la base de développement (migration CreationTablePossession).
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Prérequis : schema-idempotent.sql appliqué (deux fois : la seconde ne doit rien changer).
-- Exécuter en entier (Run all). Résultat attendu : toutes les lignes de la dernière requête avec ok = true.

-- A. Constats (lecture seule). Attendu : la table existe et est vide, la migration figure en tête.
SELECT count(*) AS possessions FROM possession;
SELECT migration_id FROM historique_migration ORDER BY migration_id DESC LIMIT 3;

-- B. Essais, dans une transaction annulée à la fin.
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

-- Données de départ : deux utilisateurs et deux jeux.
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH'),
    ('00000000-0000-0000-0000-0000000000a2', 'Essai 2', 'JKLMNPQR');
INSERT INTO jeu (id, nom, cree_par_id) VALUES
    ('00000000-0000-0000-0000-0000000000c1', 'Jeu 1', '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000c2', 'Jeu 2', '00000000-0000-0000-0000-0000000000a1');

-- Insertion minimale et valeurs par défaut.
SELECT pg_temp.essayer('possession minimale acceptee', $q$
    INSERT INTO possession (id, utilisateur_id, jeu_id)
    VALUES ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1') $q$, 'ok');
SELECT pg_temp.essayer('valeurs par defaut de possession', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM possession WHERE id = '00000000-0000-0000-0000-0000000000d1'
                       AND parties_jouees = 0 AND note IS NULL AND commentaire IS NULL
                       AND cree_le IS NOT NULL AND modifie_le IS NOT NULL) THEN
            RAISE EXCEPTION 'valeurs par defaut absentes';
        END IF;
    END $x$ $q$, 'ok');

-- CHECK.
SELECT pg_temp.essayer('note = 0', $q$ INSERT INTO possession (id, utilisateur_id, jeu_id, note) VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c1', 0) $q$, 'ck_possession_note');
SELECT pg_temp.essayer('note = 11', $q$ INSERT INTO possession (id, utilisateur_id, jeu_id, note) VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c1', 11) $q$, 'ck_possession_note');
SELECT pg_temp.essayer('note = 1', $q$ INSERT INTO possession (id, utilisateur_id, jeu_id, note) VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c1', 1) $q$, 'ok');
SELECT pg_temp.essayer('note = 10', $q$ INSERT INTO possession (id, utilisateur_id, jeu_id, note) VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c2', 10) $q$, 'ok');
SELECT pg_temp.essayer('parties_jouees negatif', $q$ INSERT INTO possession (id, utilisateur_id, jeu_id, parties_jouees) VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c2', -1) $q$, 'ck_possession_parties_jouees');

-- Unicité (utilisateur, jeu).
SELECT pg_temp.essayer('meme jeu deux fois pour un utilisateur', $q$ INSERT INTO possession (id, utilisateur_id, jeu_id) VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1') $q$, 'ix_possession_utilisateur_id_jeu_id');
SELECT pg_temp.essayer('meme jeu possede par deux utilisateurs', $q$ INSERT INTO possession (id, utilisateur_id, jeu_id) VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c2') $q$, 'ok');

-- Clés étrangères.
SELECT pg_temp.essayer('utilisateur inexistant', $q$ INSERT INTO possession (id, utilisateur_id, jeu_id) VALUES (gen_random_uuid(), gen_random_uuid(), '00000000-0000-0000-0000-0000000000c1') $q$, 'fk_possession_utilisateur_utilisateur_id');
SELECT pg_temp.essayer('jeu inexistant', $q$ INSERT INTO possession (id, utilisateur_id, jeu_id) VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', gen_random_uuid()) $q$, 'fk_possession_jeu_jeu_id');

-- Suppressions.
SELECT pg_temp.essayer('supprimer un jeu possede (RESTRICT)', $q$ DELETE FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c1' $q$, 'fk_possession_jeu_jeu_id');
SELECT pg_temp.essayer('supprimer un utilisateur supprime ses possessions (CASCADE)', $q$
    DO $x$ BEGIN
        DELETE FROM possession WHERE utilisateur_id = '00000000-0000-0000-0000-0000000000a2';
        INSERT INTO possession (id, utilisateur_id, jeu_id) VALUES ('00000000-0000-0000-0000-0000000000d2', '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c1');
        DELETE FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a2';
        IF EXISTS (SELECT 1 FROM possession WHERE id = '00000000-0000-0000-0000-0000000000d2') THEN
            RAISE EXCEPTION 'possession restante';
        END IF;
    END $x$ $q$, 'ok');

-- Retirer un jeu de la collection (supprimer la ligne) libère le jeu.
SELECT pg_temp.essayer('retirer un jeu libere sa suppression', $q$
    DO $x$ BEGIN
        DELETE FROM possession WHERE id = '00000000-0000-0000-0000-0000000000d1';
        DELETE FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c1';
    END $x$ $q$, 'ok');

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

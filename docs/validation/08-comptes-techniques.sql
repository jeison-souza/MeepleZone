-- Validation des comptes techniques sur la base de développement (migration AjoutComptesTechniques).
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Prérequis : schema-idempotent.sql appliqué (deux fois : la seconde ne doit rien changer).
-- Exécuter en entier (Run all). Résultat attendu : toutes les lignes de la dernière requête avec ok = true.

-- A. Constats (lecture seule). Attendu : les deux comptes techniques, rôle systeme, sans courriel vérifié ni photo.
SELECT nom_affichage, courriel, role, code_amitie, visibilite_collection, courriel_verifie_le, photo_id, cree_le, modifie_le
FROM utilisateur WHERE role = 'systeme' ORDER BY code_amitie;
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

-- Contenu des données initiales (valeurs du schéma publié : deux comptes, rôle systeme, codes réservés).
SELECT pg_temp.essayer('deux comptes techniques presents', $q$
    DO $x$ BEGIN
        IF (SELECT count(*) FROM utilisateur WHERE role = 'systeme' AND code_amitie IN ('SYSTEME2', 'EFFACE22')) <> 2 THEN
            RAISE EXCEPTION 'comptes techniques manquants';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('noms d affichage des comptes techniques', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM utilisateur WHERE code_amitie = 'SYSTEME2' AND nom_affichage = 'Système')
           OR NOT EXISTS (SELECT 1 FROM utilisateur WHERE code_amitie = 'EFFACE22' AND nom_affichage = 'Utilisateur supprimé') THEN
            RAISE EXCEPTION 'noms differents';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('collection privee, sans courriel verifie ni photo', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM utilisateur WHERE role = 'systeme'
                   AND (visibilite_collection <> 'privee' OR courriel_verifie_le IS NOT NULL OR photo_id IS NOT NULL)) THEN
            RAISE EXCEPTION 'valeurs inattendues';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('dates de creation et de modification renseignees et egales', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM utilisateur WHERE role = 'systeme'
                   AND (cree_le IS NULL OR modifie_le IS NULL OR cree_le <> modifie_le)) THEN
            RAISE EXCEPTION 'dates inattendues';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('aucune identite externe pour les comptes techniques', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM identite_externe i JOIN utilisateur u ON u.id = i.utilisateur_id WHERE u.role = 'systeme') THEN
            RAISE EXCEPTION 'identite externe presente';
        END IF;
    END $x$ $q$, 'ok');

-- Les codes réservés ne peuvent pas être pris par un autre compte. L'unicité du courriel est vérifiée par 15-courriel-verifie.sql.
SELECT pg_temp.essayer('code d amitie des comptes techniques deja pris', $q$
    INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES (gen_random_uuid(), 'Essai', 'SYSTEME2') $q$, 'ix_utilisateur_code_amitie');

-- Format du code d'amitié : aucune lettre I ni O, aucun 0 ni 1.
SELECT pg_temp.essayer('code avec la lettre I', $q$ INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES (gen_random_uuid(), 'Essai', 'ABCDEFGI') $q$, 'ck_utilisateur_code_amitie');
SELECT pg_temp.essayer('code avec la lettre O', $q$ INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES (gen_random_uuid(), 'Essai', 'ABCDEFGO') $q$, 'ck_utilisateur_code_amitie');
SELECT pg_temp.essayer('code avec le chiffre 0', $q$ INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES (gen_random_uuid(), 'Essai', 'ABCDEFG0') $q$, 'ck_utilisateur_code_amitie');
SELECT pg_temp.essayer('code avec le chiffre 1', $q$ INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES (gen_random_uuid(), 'Essai', 'ABCDEFG1') $q$, 'ck_utilisateur_code_amitie');
SELECT pg_temp.essayer('code de 7 caracteres', $q$ INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES (gen_random_uuid(), 'Essai', 'ABCDEFG') $q$, 'ck_utilisateur_code_amitie');

-- Rôles.
SELECT pg_temp.essayer('role systeme accepte', $q$ INSERT INTO utilisateur (id, nom_affichage, code_amitie, role) VALUES (gen_random_uuid(), 'Essai', 'JKLMNPQR', 'systeme') $q$, 'ok');
SELECT pg_temp.essayer('role inconnu refuse', $q$ INSERT INTO utilisateur (id, nom_affichage, code_amitie, role) VALUES (gen_random_uuid(), 'Essai', 'STUVWXYZ', 'robot') $q$, 'ck_utilisateur_role');

-- Suppression de compte (ADR 0006) : le contenu partagé passe au compte « Utilisateur supprimé », qui ne peut pas être supprimé tant qu'il en porte.
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES ('00000000-0000-0000-0000-0000000000a1', 'Essai membre', '23456789');
INSERT INTO jeu (id, nom, cree_par_id) VALUES ('00000000-0000-0000-0000-0000000000c1', 'Jeu du membre', '00000000-0000-0000-0000-0000000000a1');

SELECT pg_temp.essayer('supprimer un membre qui a cree un jeu (RESTRICT)', $q$ DELETE FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a1' $q$, 'fk_jeu_utilisateur_cree_par_id');
SELECT pg_temp.essayer('rattacher le jeu au compte Utilisateur supprime puis supprimer le membre', $q$
    DO $x$ BEGIN
        UPDATE jeu SET cree_par_id = (SELECT id FROM utilisateur WHERE code_amitie = 'EFFACE22')
        WHERE id = '00000000-0000-0000-0000-0000000000c1';
        DELETE FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a1';
        IF NOT EXISTS (SELECT 1 FROM jeu j JOIN utilisateur u ON u.id = j.cree_par_id
                       WHERE j.id = '00000000-0000-0000-0000-0000000000c1' AND u.code_amitie = 'EFFACE22') THEN
            RAISE EXCEPTION 'jeu non rattache';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('supprimer le compte Utilisateur supprime tant qu il porte un jeu (RESTRICT)', $q$
    DELETE FROM utilisateur WHERE code_amitie = 'EFFACE22' $q$, 'fk_jeu_utilisateur_cree_par_id');

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

-- Validation de la table demande_verification sur la base de développement (migration CreationTableDemandeVerification).
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Prérequis : schema-idempotent.sql appliqué (deux fois : la seconde ne doit rien changer).
-- Exécuter en entier (Run all). Résultat attendu : toutes les lignes de la dernière requête avec ok = true.

-- A. Constats (lecture seule). Attendu : la dernière migration est présente.
SELECT current_user AS compte;
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

INSERT INTO utilisateur (id, nom_affichage, code_amitie)
VALUES ('00000000-0000-0000-0000-0000000000b1', 'Essai 1', 'ABCDEFGH'),
       ('00000000-0000-0000-0000-0000000000b2', 'Essai 2', 'JKLMNPQR');

SELECT pg_temp.essayer('deux index non uniques, sur utilisateur_id et sur courriel', $q$
    DO $x$ BEGIN
        IF (SELECT count(*) FROM pg_indexes
            WHERE tablename = 'demande_verification'
              AND indexname IN ('ix_demande_verification_utilisateur_id', 'ix_demande_verification_courriel')
              AND indexdef LIKE 'CREATE INDEX%') <> 2 THEN
            RAISE EXCEPTION 'index inattendus';
        END IF;
    END $x$ $q$, 'ok');

-- Demande valide : les valeurs par défaut sont celles du schéma.
SELECT pg_temp.essayer('demande valide', $q$
    INSERT INTO demande_verification (id, utilisateur_id, courriel, duree_validite)
    VALUES ('00000000-0000-0000-0000-0000000000c1', '00000000-0000-0000-0000-0000000000b1', 'nom@exemple.ca', interval '24 hours') $q$, 'ok');
SELECT pg_temp.essayer('date de creation renseignee, envoi vide', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM demande_verification
                       WHERE id = '00000000-0000-0000-0000-0000000000c1' AND cree_le = now() AND envoye_le IS NULL) THEN
            RAISE EXCEPTION 'valeurs par defaut inattendues';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('echeance calculee avec la duree de la demande', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM demande_verification
                       WHERE id = '00000000-0000-0000-0000-0000000000c1' AND cree_le + duree_validite = now() + interval '1 day') THEN
            RAISE EXCEPTION 'echeance inattendue';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('envoi date apres coup', $q$
    UPDATE demande_verification SET envoye_le = now() WHERE id = '00000000-0000-0000-0000-0000000000c1' $q$, 'ok');

-- Aucune unicité : les limites sont appliquées par le code (ADR 0017).
SELECT pg_temp.essayer('seconde demande du meme compte pour le meme courriel', $q$
    INSERT INTO demande_verification (id, utilisateur_id, courriel, duree_validite)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000b1', 'nom@exemple.ca', interval '24 hours') $q$, 'ok');
SELECT pg_temp.essayer('demande d un autre compte pour le meme courriel', $q$
    INSERT INTO demande_verification (id, utilisateur_id, courriel, duree_validite)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000b2', 'NOM@Exemple.CA', interval '24 hours') $q$, 'ok');
SELECT pg_temp.essayer('demandes d une adresse comptees sans egard a la casse', $q$
    DO $x$ BEGIN
        IF (SELECT count(*) FROM demande_verification WHERE courriel = 'Nom@Exemple.ca') <> 3 THEN
            RAISE EXCEPTION 'compte inattendu';
        END IF;
    END $x$ $q$, 'ok');

-- Contraintes.
SELECT pg_temp.essayer('courriel vide', $q$
    INSERT INTO demande_verification (id, utilisateur_id, courriel, duree_validite)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000b1', '', interval '24 hours') $q$, 'ck_demande_verification_courriel');
SELECT pg_temp.essayer('courriel fait d espaces', $q$
    INSERT INTO demande_verification (id, utilisateur_id, courriel, duree_validite)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000b1', '   ', interval '24 hours') $q$, 'ck_demande_verification_courriel');
SELECT pg_temp.essayer('courriel avec une espace au debut', $q$
    INSERT INTO demande_verification (id, utilisateur_id, courriel, duree_validite)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000b1', ' nom@exemple.ca', interval '24 hours') $q$, 'ck_demande_verification_courriel');
SELECT pg_temp.essayer('courriel absent', $q$
    INSERT INTO demande_verification (id, utilisateur_id, duree_validite)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000b1', interval '24 hours') $q$, '23502');
SELECT pg_temp.essayer('duree nulle', $q$
    INSERT INTO demande_verification (id, utilisateur_id, courriel, duree_validite)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000b1', 'nom@exemple.ca', interval '0') $q$, 'ck_demande_verification_duree_validite');
SELECT pg_temp.essayer('duree negative', $q$
    INSERT INTO demande_verification (id, utilisateur_id, courriel, duree_validite)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000b1', 'nom@exemple.ca', interval '-1 minute') $q$, 'ck_demande_verification_duree_validite');
SELECT pg_temp.essayer('duree absente', $q$
    INSERT INTO demande_verification (id, utilisateur_id, courriel)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000b1', 'nom@exemple.ca') $q$, '23502');
SELECT pg_temp.essayer('compte absent', $q$
    INSERT INTO demande_verification (id, courriel, duree_validite)
    VALUES (gen_random_uuid(), 'nom@exemple.ca', interval '24 hours') $q$, '23502');
SELECT pg_temp.essayer('compte inexistant', $q$
    INSERT INTO demande_verification (id, utilisateur_id, courriel, duree_validite)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000b9', 'nom@exemple.ca', interval '24 hours') $q$, 'fk_demande_verification_utilisateur_utilisateur_id');

-- Suppression du compte : ses demandes disparaissent, pas celles des autres.
SELECT pg_temp.essayer('suppression en cascade des demandes du compte', $q$
    DO $x$ BEGIN
        DELETE FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000b1';
        IF EXISTS (SELECT 1 FROM demande_verification WHERE utilisateur_id = '00000000-0000-0000-0000-0000000000b1')
           OR (SELECT count(*) FROM demande_verification WHERE utilisateur_id = '00000000-0000-0000-0000-0000000000b2') <> 1 THEN
            RAISE EXCEPTION 'cascade inattendue';
        END IF;
    END $x$ $q$, 'ok');

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

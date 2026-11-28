-- Validation de l'unicité du courriel vérifié sur la base de développement (migration UniciteCourrielVerifie).
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

SELECT pg_temp.essayer('index unique limite aux courriels verifies', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_utilisateur_courriel'
                       AND indexdef LIKE 'CREATE UNIQUE INDEX%WHERE (courriel_verifie_le IS NOT NULL)') THEN
            RAISE EXCEPTION 'index inattendu';
        END IF;
    END $x$ $q$, 'ok');

-- Un courriel non vérifié ne réserve pas l'adresse (ADR 0004, règle 1).
SELECT pg_temp.essayer('compte avec un courriel non verifie', $q$
    INSERT INTO utilisateur (id, nom_affichage, code_amitie, courriel)
    VALUES ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH', 'nom@exemple.ca') $q$, 'ok');
SELECT pg_temp.essayer('meme courriel non verifie sur un autre compte', $q$
    INSERT INTO utilisateur (id, nom_affichage, code_amitie, courriel)
    VALUES ('00000000-0000-0000-0000-0000000000a2', 'Essai 2', 'JKLMNPQR', 'nom@exemple.ca') $q$, 'ok');
SELECT pg_temp.essayer('meme courriel non verifie, casse differente', $q$
    INSERT INTO utilisateur (id, nom_affichage, code_amitie, courriel)
    VALUES ('00000000-0000-0000-0000-0000000000a3', 'Essai 3', 'STUVWXYZ', 'NOM@Exemple.CA') $q$, 'ok');

-- Un courriel vérifié n'appartient qu'à un seul compte, sans égard à la casse.
SELECT pg_temp.essayer('premier compte qui verifie le courriel', $q$
    UPDATE utilisateur SET courriel_verifie_le = now() WHERE id = '00000000-0000-0000-0000-0000000000a1' $q$, 'ok');
SELECT pg_temp.essayer('second compte qui verifie le meme courriel', $q$
    UPDATE utilisateur SET courriel_verifie_le = now() WHERE id = '00000000-0000-0000-0000-0000000000a2' $q$, 'ix_utilisateur_courriel');
SELECT pg_temp.essayer('verification avec une casse differente', $q$
    UPDATE utilisateur SET courriel_verifie_le = now() WHERE id = '00000000-0000-0000-0000-0000000000a3' $q$, 'ix_utilisateur_courriel');
SELECT pg_temp.essayer('nouveau compte avec le courriel deja verifie, verifie lui aussi', $q$
    INSERT INTO utilisateur (id, nom_affichage, code_amitie, courriel, courriel_verifie_le)
    VALUES (gen_random_uuid(), 'Essai 4', '23456789', 'nom@exemple.ca', now()) $q$, 'ix_utilisateur_courriel');

-- La base accepte encore une copie non vérifiée : le refus est une règle du code (ADR 0004, règle 1).
SELECT pg_temp.essayer('copie non verifiee acceptee par la base apres la verification', $q$
    INSERT INTO utilisateur (id, nom_affichage, code_amitie, courriel)
    VALUES (gen_random_uuid(), 'Essai 5', '98765432', 'nom@exemple.ca') $q$, 'ok');
SELECT pg_temp.essayer('copie non verifiee du courriel d un compte technique', $q$
    INSERT INTO utilisateur (id, nom_affichage, code_amitie, courriel)
    VALUES (gen_random_uuid(), 'Essai 6', 'HGFEDCBA', 'systeme@meeplezone.ca') $q$, 'ok');

SELECT pg_temp.essayer('courriel libere quand le compte en change', $q$
    DO $x$ BEGIN
        UPDATE utilisateur SET courriel = 'autre@exemple.ca', courriel_verifie_le = NULL
        WHERE id = '00000000-0000-0000-0000-0000000000a1';
        UPDATE utilisateur SET courriel_verifie_le = now() WHERE id = '00000000-0000-0000-0000-0000000000a2';
    END $x$ $q$, 'ok');

-- Recherche d'un courriel à la création d'un compte : comptes vérifiés, puis connexions externes.
SELECT pg_temp.essayer('presence : courriel verifie par un compte', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM utilisateur WHERE courriel = 'Nom@Exemple.ca' AND courriel_verifie_le IS NOT NULL) THEN
            RAISE EXCEPTION 'courriel verifie introuvable';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('presence : courriel non verifie d un compte seulement, aucune', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM utilisateur WHERE courriel = 'autre@exemple.ca' AND courriel_verifie_le IS NOT NULL)
           OR EXISTS (SELECT 1 FROM identite_externe WHERE courriel = 'autre@exemple.ca') THEN
            RAISE EXCEPTION 'courriel compte a tort';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('presence : courriel d une connexion externe seulement, non verifiee', $q$
    DO $x$ BEGIN
        INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe, courriel)
        VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a3', 'Google', 'g-essai-15', 'externe@exemple.ca');
        IF EXISTS (SELECT 1 FROM utilisateur WHERE courriel = 'externe@exemple.ca' AND courriel_verifie_le IS NOT NULL)
           OR NOT EXISTS (SELECT 1 FROM identite_externe WHERE courriel = 'Externe@Exemple.ca') THEN
            RAISE EXCEPTION 'presence inattendue';
        END IF;
    END $x$ $q$, 'ok');

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

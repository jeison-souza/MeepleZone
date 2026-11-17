-- Validation de la liaison des fournisseurs sur la base de développement (table identite_externe).
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Prérequis : schema-idempotent.sql appliqué (deux fois : la seconde ne doit rien changer).
-- Exécuter en entier (Run all). Résultat attendu : toutes les lignes de la dernière requête avec ok = true.

-- A. Constats (lecture seule). Attendu : la table existe; le compte utilisé est celui de déploiement.
SELECT current_user AS compte;
SELECT count(*) AS identites FROM identite_externe;

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

-- Données de départ : deux utilisateurs.
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH'),
    ('00000000-0000-0000-0000-0000000000a2', 'Essai 2', 'JKLMNPQR');

-- Liaison d'un fournisseur : insertion minimale (courriel absent) et valeurs par défaut.
SELECT pg_temp.essayer('connexion sans courriel acceptee', $q$
    INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe)
    VALUES ('00000000-0000-0000-0000-0000000000b1', '00000000-0000-0000-0000-0000000000a1', 'Google', 'g-1') $q$, 'ok');
SELECT pg_temp.essayer('dates par defaut renseignees', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM identite_externe WHERE id = '00000000-0000-0000-0000-0000000000b1'
                       AND cree_le IS NOT NULL AND modifie_le IS NOT NULL AND derniere_connexion_le IS NOT NULL) THEN
            RAISE EXCEPTION 'dates absentes';
        END IF;
    END $x$ $q$, 'ok');

-- Règle 2 de l'ADR 0004 : une identité (fournisseur + identifiant) n'appartient qu'à un seul compte.
SELECT pg_temp.essayer('meme identite pour le meme compte', $q$
    INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', 'Google', 'g-1') $q$, 'ix_identite_externe_fournisseur_identifiant_compte_externe');
SELECT pg_temp.essayer('meme identite pour un autre compte', $q$
    INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a2', 'Google', 'g-1') $q$, 'ix_identite_externe_fournisseur_identifiant_compte_externe');

-- Plusieurs connexions par fournisseur sont permises (décision du 16/11/2026) : aucun index unique sur (utilisateur_id, fournisseur).
SELECT pg_temp.essayer('deuxieme connexion Google du meme compte', $q$
    INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe)
    VALUES ('00000000-0000-0000-0000-0000000000b2', '00000000-0000-0000-0000-0000000000a1', 'Google', 'g-2') $q$, 'ok');
SELECT pg_temp.essayer('meme identifiant chez un autre fournisseur', $q$
    INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe)
    VALUES ('00000000-0000-0000-0000-0000000000b3', '00000000-0000-0000-0000-0000000000a2', 'Microsoft', 'g-1') $q$, 'ok');

-- Courriel de la connexion (information) : facultatif, jamais vide ni avec des espaces en bordure.
SELECT pg_temp.essayer('courriel valide', $q$
    INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe, courriel)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a2', 'Microsoft', 'm-2', 'nom@exemple.ca') $q$, 'ok');
SELECT pg_temp.essayer('courriel vide refuse', $q$
    INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe, courriel)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a2', 'Microsoft', 'm-3', '') $q$, 'ck_identite_externe_courriel');
SELECT pg_temp.essayer('courriel avec espaces refuse', $q$
    INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe, courriel)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a2', 'Microsoft', 'm-4', ' nom@exemple.ca ') $q$, 'ck_identite_externe_courriel');
SELECT pg_temp.essayer('meme courriel pour deux comptes accepte (information)', $q$
    INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe, courriel)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', 'Microsoft', 'm-5', 'nom@exemple.ca') $q$, 'ok');

-- Clé étrangère : l'utilisateur doit exister (cas d'un témoin de session d'un compte supprimé).
SELECT pg_temp.essayer('utilisateur inexistant', $q$
    INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe)
    VALUES (gen_random_uuid(), gen_random_uuid(), 'Google', 'g-9') $q$, 'fk_identite_externe_utilisateur_utilisateur_id');

-- Lecture pour la liste des connexions : seulement celles du compte, triées par création puis identifiant.
SELECT pg_temp.essayer('liste limitee au compte', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM identite_externe WHERE utilisateur_id = '00000000-0000-0000-0000-0000000000a1'
                   AND id = '00000000-0000-0000-0000-0000000000b3') THEN
            RAISE EXCEPTION 'connexion d un autre compte';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('tri par creation puis identifiant', $q$
    DO $x$
    DECLARE v_ordre uuid[];
    BEGIN
        UPDATE identite_externe SET cree_le = '2026-01-01T00:00:00Z' WHERE utilisateur_id = '00000000-0000-0000-0000-0000000000a1';
        UPDATE identite_externe SET cree_le = '2026-02-01T00:00:00Z' WHERE id = '00000000-0000-0000-0000-0000000000b1';
        SELECT array_agg(id ORDER BY cree_le, id) INTO v_ordre FROM identite_externe
        WHERE utilisateur_id = '00000000-0000-0000-0000-0000000000a1' AND id IN
            ('00000000-0000-0000-0000-0000000000b1', '00000000-0000-0000-0000-0000000000b2');
        IF v_ordre <> ARRAY['00000000-0000-0000-0000-0000000000b2', '00000000-0000-0000-0000-0000000000b1']::uuid[] THEN
            RAISE EXCEPTION 'ordre inattendu';
        END IF;
    END $x$ $q$, 'ok');

-- Suppression du compte : ses connexions disparaissent avec lui (ADR 0006), celles des autres restent.
SELECT pg_temp.essayer('supprimer le compte supprime ses connexions (CASCADE)', $q$
    DO $x$ BEGIN
        DELETE FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a1';
        IF EXISTS (SELECT 1 FROM identite_externe WHERE utilisateur_id = '00000000-0000-0000-0000-0000000000a1') THEN
            RAISE EXCEPTION 'connexion restante';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM identite_externe WHERE id = '00000000-0000-0000-0000-0000000000b3') THEN
            RAISE EXCEPTION 'connexion d un autre compte supprimee';
        END IF;
    END $x$ $q$, 'ok');

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

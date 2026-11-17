-- Validation de la suppression de compte sur la base de développement (ADR 0006, opération SupprimerCompte).
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Prérequis : schema-idempotent.sql appliqué (deux fois : la seconde ne doit rien changer).
-- Exécuter en entier (Run all) avec le compte de déploiement. Résultat attendu : toutes les lignes de la dernière requête avec ok = true.
-- Le DO de la section « Séquence » reproduit, en SQL, les étapes que le dépôt exécute dans une seule transaction.

-- A. Constats (lecture seule). Attendu : le compte utilisé est celui de déploiement; les deux comptes techniques existent.
SELECT current_user AS compte;
SELECT code_amitie, role FROM utilisateur WHERE role = 'systeme' ORDER BY code_amitie;

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

-- Données de départ.
--   a1 : compte à supprimer (photo b1, connexion), a2 : autre membre, a3 : ne possède qu'un jeu, a4 : ne possède qu'une annexe.
--   c1 validé de a1, possédé par a1 et a2     -> rattaché au compte technique, la possession de a2 reste
--   c2 en attente de a1, couverture b2, sans autre possession -> supprimé avec sa couverture
--   c3 en attente de a1, possédé par a2       -> rattaché au compte technique
--   c4 de a2, possédé par a1                  -> reste à a2, seule la possession de a1 disparaît
--   c5 validé de a1, couverture b3, sans possession -> rattaché, annexe b3 rattachée
--   c6 refusé de a1, sans possession          -> supprimé
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH'),
    ('00000000-0000-0000-0000-0000000000a2', 'Essai 2', 'JKLMNPQR'),
    ('00000000-0000-0000-0000-0000000000a3', 'Essai 3', 'STUVWXYZ'),
    ('00000000-0000-0000-0000-0000000000a4', 'Essai 4', 'ABCDEFGJ');

INSERT INTO annexe (id, nom_fichier, contenu, type_mime, taille_octets, cree_par_id) VALUES
    ('00000000-0000-0000-0000-0000000000b1', 'photo.webp', '\x00', 'image/webp', 1, '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000b2', 'couv2.webp', '\x00', 'image/webp', 1, '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000b3', 'couv5.webp', '\x00', 'image/webp', 1, '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000b4', 'autre.webp', '\x00', 'image/webp', 1, '00000000-0000-0000-0000-0000000000a2'),
    ('00000000-0000-0000-0000-0000000000b5', 'seule.webp', '\x00', 'image/webp', 1, '00000000-0000-0000-0000-0000000000a4');

UPDATE utilisateur SET photo_id = '00000000-0000-0000-0000-0000000000b1' WHERE id = '00000000-0000-0000-0000-0000000000a1';

INSERT INTO jeu (id, nom, cree_par_id, statut, couverture_id) VALUES
    ('00000000-0000-0000-0000-0000000000c1', 'Jeu 1', '00000000-0000-0000-0000-0000000000a1', 'valide', NULL),
    ('00000000-0000-0000-0000-0000000000c2', 'Jeu 2', '00000000-0000-0000-0000-0000000000a1', 'en_attente', '00000000-0000-0000-0000-0000000000b2'),
    ('00000000-0000-0000-0000-0000000000c3', 'Jeu 3', '00000000-0000-0000-0000-0000000000a1', 'en_attente', NULL),
    ('00000000-0000-0000-0000-0000000000c4', 'Jeu 4', '00000000-0000-0000-0000-0000000000a2', 'en_attente', NULL),
    ('00000000-0000-0000-0000-0000000000c5', 'Jeu 5', '00000000-0000-0000-0000-0000000000a1', 'valide', '00000000-0000-0000-0000-0000000000b3'),
    ('00000000-0000-0000-0000-0000000000c6', 'Jeu 6', '00000000-0000-0000-0000-0000000000a1', 'refuse', NULL),
    ('00000000-0000-0000-0000-0000000000c7', 'Jeu 7', '00000000-0000-0000-0000-0000000000a3', 'en_attente', NULL);

INSERT INTO possession (id, utilisateur_id, jeu_id) VALUES
    ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1'),
    ('00000000-0000-0000-0000-0000000000d2', '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c1'),
    ('00000000-0000-0000-0000-0000000000d3', '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c3'),
    ('00000000-0000-0000-0000-0000000000d4', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c4');

INSERT INTO identite_externe (id, utilisateur_id, fournisseur, identifiant_compte_externe) VALUES
    ('00000000-0000-0000-0000-0000000000e1', '00000000-0000-0000-0000-0000000000a1', 'Google', 'g-a1'),
    ('00000000-0000-0000-0000-0000000000e2', '00000000-0000-0000-0000-0000000000a2', 'Google', 'g-a2');

-- Rôles : le rôle administrateur existe (le dépôt et le gestionnaire refusent de le supprimer).
SELECT pg_temp.essayer('role administrateur accepte', $q$
    INSERT INTO utilisateur (id, nom_affichage, code_amitie, role)
    VALUES ('00000000-0000-0000-0000-0000000000a5', 'Admin', 'ABCDEFGK', 'administrateur') $q$, 'ok');

-- Raisons de l'ordre des étapes : sans nettoyage préalable, la base refuse (RESTRICT).
SELECT pg_temp.essayer('utilisateur avec un jeu : suppression refusee', $q$
    DELETE FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a3' $q$, 'fk_jeu_utilisateur_cree_par_id');
SELECT pg_temp.essayer('utilisateur avec une annexe : suppression refusee', $q$
    DELETE FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a4' $q$, 'fk_annexe_utilisateur_cree_par_id');
-- Cas de conflit : un jeu qui a (ou vient de recevoir) une possession ne peut pas être supprimé; le dépôt traduit ce nom en « Conflit ».
SELECT pg_temp.essayer('jeu encore possede : suppression refusee', $q$
    DELETE FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c3' $q$, 'fk_possession_jeu_jeu_id');

-- Séquence (une seule transaction dans le dépôt).
SELECT pg_temp.essayer('sequence de suppression executee', $q$
    DO $x$
    DECLARE
        v_jeux uuid[];
        v_couvertures uuid[];
        v_photo uuid;
    BEGIN
        SELECT photo_id INTO v_photo FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a1';

        -- a. possessions de la personne
        DELETE FROM possession WHERE utilisateur_id = '00000000-0000-0000-0000-0000000000a1';

        -- b. jeux non validés de la personne, sans autre possession, puis leurs couvertures
        SELECT array_agg(j.id), array_agg(j.couverture_id) FILTER (WHERE j.couverture_id IS NOT NULL)
          INTO v_jeux, v_couvertures
          FROM jeu j
         WHERE j.cree_par_id = '00000000-0000-0000-0000-0000000000a1'
           AND j.statut <> 'valide'
           AND NOT EXISTS (SELECT 1 FROM possession p WHERE p.jeu_id = j.id);
        DELETE FROM jeu WHERE id = ANY(v_jeux);
        DELETE FROM annexe WHERE id = ANY(v_couvertures);

        -- c. photo de profil
        DELETE FROM annexe WHERE id = v_photo;

        -- d. contenu partagé restant : rattaché au compte technique « Utilisateur supprimé »
        UPDATE jeu SET cree_par_id = '01a1c7e7-8000-7102-8000-000000000002' WHERE cree_par_id = '00000000-0000-0000-0000-0000000000a1';
        UPDATE annexe SET cree_par_id = '01a1c7e7-8000-7102-8000-000000000002' WHERE cree_par_id = '00000000-0000-0000-0000-0000000000a1';

        -- e. le compte (ses connexions disparaissent en cascade)
        DELETE FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a1';
    END $x$ $q$, 'ok');

-- Résultats de la séquence.
SELECT pg_temp.essayer('le compte n existe plus', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a1') THEN RAISE EXCEPTION 'compte restant'; END IF;
        IF NOT EXISTS (SELECT 1 FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a2') THEN RAISE EXCEPTION 'autre compte supprime'; END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('connexions supprimees, celles des autres conservees', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM identite_externe WHERE id = '00000000-0000-0000-0000-0000000000e1') THEN RAISE EXCEPTION 'connexion restante'; END IF;
        IF NOT EXISTS (SELECT 1 FROM identite_externe WHERE id = '00000000-0000-0000-0000-0000000000e2') THEN RAISE EXCEPTION 'connexion d un autre compte supprimee'; END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('possessions de la personne supprimees, celles des autres conservees', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM possession WHERE id IN ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000d4')) THEN RAISE EXCEPTION 'possession restante'; END IF;
        IF (SELECT count(*) FROM possession WHERE id IN ('00000000-0000-0000-0000-0000000000d2', '00000000-0000-0000-0000-0000000000d3')) <> 2 THEN RAISE EXCEPTION 'possession d un autre compte supprimee'; END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('jeux non valides sans possession supprimes (en attente et refuse)', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM jeu WHERE id IN ('00000000-0000-0000-0000-0000000000c2', '00000000-0000-0000-0000-0000000000c6')) THEN RAISE EXCEPTION 'jeu restant'; END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('couverture du jeu supprime, photo de profil supprimees', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM annexe WHERE id IN ('00000000-0000-0000-0000-0000000000b1', '00000000-0000-0000-0000-0000000000b2')) THEN RAISE EXCEPTION 'annexe restante'; END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('jeux valides ou possedes rattaches au compte technique', $q$
    DO $x$ BEGIN
        IF (SELECT count(*) FROM jeu WHERE id IN ('00000000-0000-0000-0000-0000000000c1', '00000000-0000-0000-0000-0000000000c3', '00000000-0000-0000-0000-0000000000c5')
                AND cree_par_id = '01a1c7e7-8000-7102-8000-000000000002') <> 3 THEN RAISE EXCEPTION 'jeu non rattache'; END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('jeu d un autre createur inchange', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c4' AND cree_par_id = '00000000-0000-0000-0000-0000000000a2') THEN RAISE EXCEPTION 'jeu modifie'; END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('couverture conservee et rattachee, annexes des autres inchangees', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM annexe WHERE id = '00000000-0000-0000-0000-0000000000b3' AND cree_par_id = '01a1c7e7-8000-7102-8000-000000000002') THEN RAISE EXCEPTION 'couverture non rattachee'; END IF;
        IF NOT EXISTS (SELECT 1 FROM annexe WHERE id = '00000000-0000-0000-0000-0000000000b4' AND cree_par_id = '00000000-0000-0000-0000-0000000000a2') THEN RAISE EXCEPTION 'annexe d un autre compte modifiee'; END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('le jeu valide garde son lien de couverture', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c5' AND couverture_id = '00000000-0000-0000-0000-0000000000b3') THEN RAISE EXCEPTION 'couverture detachee'; END IF;
    END $x$ $q$, 'ok');

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

-- Validation des amitiés sur la base de développement (tables demande_amitie et amitie).
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Prérequis : migration CreationTablesAmities appliquée (au démarrage de l'application, ou avec schema-idempotent.sql).
-- Exécuter en entier (Run all) avec le compte de déploiement. Résultat attendu : toutes les lignes de la dernière requête avec ok = true.

-- A. Constats (lecture seule). Attendu : le compte utilisé est celui de déploiement; la migration figure dans l'historique.
SELECT current_user AS compte;
SELECT migration_id FROM historique_migration WHERE migration_id LIKE '%_CreationTablesAmities';

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

-- Données de départ : quatre utilisateurs.
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH'),
    ('00000000-0000-0000-0000-0000000000a2', 'Essai 2', 'JKLMNPQR'),
    ('00000000-0000-0000-0000-0000000000a3', 'Essai 3', 'STUVWXYZ'),
    ('00000000-0000-0000-0000-0000000000a4', 'Essai 4', 'ABCDEFGJ');

-- Demande à une personne qui a un compte : insertion minimale et valeurs par défaut.
SELECT pg_temp.essayer('demande a un compte acceptee', $q$
    INSERT INTO demande_amitie (id, demandeur_id, destinataire_id)
    VALUES ('00000000-0000-0000-0000-0000000000f1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a2') $q$, 'ok');
SELECT pg_temp.essayer('statut et dates par defaut', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM demande_amitie WHERE id = '00000000-0000-0000-0000-0000000000f1'
                       AND statut = 'en_attente' AND cree_le IS NOT NULL AND modifie_le IS NOT NULL) THEN
            RAISE EXCEPTION 'valeurs par defaut absentes';
        END IF;
    END $x$ $q$, 'ok');

-- Demande par courriel à une personne sans compte.
SELECT pg_temp.essayer('demande par courriel acceptee', $q$
    INSERT INTO demande_amitie (id, demandeur_id, courriel_destinataire)
    VALUES ('00000000-0000-0000-0000-0000000000f2', '00000000-0000-0000-0000-0000000000a1', 'nom@exemple.ca') $q$, 'ok');

-- Exactement un des deux destinataires, et jamais soi-même.
SELECT pg_temp.essayer('destinataire et courriel ensemble refuses', $q$
    INSERT INTO demande_amitie (id, demandeur_id, destinataire_id, courriel_destinataire)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a3', 'autre@exemple.ca') $q$, 'ck_demande_amitie_destinataire');
SELECT pg_temp.essayer('ni destinataire ni courriel refuse', $q$
    INSERT INTO demande_amitie (id, demandeur_id)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1') $q$, 'ck_demande_amitie_destinataire');
SELECT pg_temp.essayer('demande a soi-meme refusee', $q$
    INSERT INTO demande_amitie (id, demandeur_id, destinataire_id)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a1') $q$, 'ck_demande_amitie_demandeur_distinct');

-- Statuts permis.
SELECT pg_temp.essayer('statut ignoree accepte', $q$
    UPDATE demande_amitie SET statut = 'ignoree' WHERE id = '00000000-0000-0000-0000-0000000000f1' $q$, 'ok');
SELECT pg_temp.essayer('statut rejetee accepte', $q$
    UPDATE demande_amitie SET statut = 'rejetee' WHERE id = '00000000-0000-0000-0000-0000000000f1' $q$, 'ok');
SELECT pg_temp.essayer('statut inconnu refuse', $q$
    UPDATE demande_amitie SET statut = 'acceptee' WHERE id = '00000000-0000-0000-0000-0000000000f1' $q$, 'ck_demande_amitie_statut');
SELECT pg_temp.essayer('statut absent refuse', $q$
    UPDATE demande_amitie SET statut = NULL WHERE id = '00000000-0000-0000-0000-0000000000f1' $q$, '23502');

-- Une seule demande à la même personne, quel que soit son état.
SELECT pg_temp.essayer('deuxieme demande a la meme personne refusee', $q$
    INSERT INTO demande_amitie (id, demandeur_id, destinataire_id)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a2') $q$, 'ix_demande_amitie_demandeur_id_destinataire_id');
SELECT pg_temp.essayer('meme destinataire pour un autre demandeur', $q$
    INSERT INTO demande_amitie (id, demandeur_id, destinataire_id)
    VALUES ('00000000-0000-0000-0000-0000000000f3', '00000000-0000-0000-0000-0000000000a3', '00000000-0000-0000-0000-0000000000a2') $q$, 'ok');
SELECT pg_temp.essayer('meme demandeur pour un autre destinataire', $q$
    INSERT INTO demande_amitie (id, demandeur_id, destinataire_id)
    VALUES ('00000000-0000-0000-0000-0000000000f4', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a3') $q$, 'ok');
SELECT pg_temp.essayer('demande en sens inverse acceptee par la base (regle du code)', $q$
    INSERT INTO demande_amitie (id, demandeur_id, destinataire_id)
    VALUES ('00000000-0000-0000-0000-0000000000f5', '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000a1') $q$, 'ok');

-- Courriel du destinataire : unicité et recherche sans égard à la casse, accents distingués.
SELECT pg_temp.essayer('meme courriel pour le meme demandeur refuse', $q$
    INSERT INTO demande_amitie (id, demandeur_id, courriel_destinataire)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', 'nom@exemple.ca') $q$, 'ix_demande_amitie_demandeur_id_courriel_destinataire');
SELECT pg_temp.essayer('meme courriel en majuscules refuse', $q$
    INSERT INTO demande_amitie (id, demandeur_id, courriel_destinataire)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', 'NOM@Exemple.CA') $q$, 'ix_demande_amitie_demandeur_id_courriel_destinataire');
SELECT pg_temp.essayer('courriel avec accent distinct', $q$
    INSERT INTO demande_amitie (id, demandeur_id, courriel_destinataire)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', 'nôm@exemple.ca') $q$, 'ok');
SELECT pg_temp.essayer('meme courriel pour un autre demandeur', $q$
    INSERT INTO demande_amitie (id, demandeur_id, courriel_destinataire)
    VALUES ('00000000-0000-0000-0000-0000000000f6', '00000000-0000-0000-0000-0000000000a3', 'Nom@exemple.ca') $q$, 'ok');
SELECT pg_temp.essayer('recherche par courriel sans egard a la casse', $q$
    DO $x$ BEGIN
        IF (SELECT count(*) FROM demande_amitie WHERE courriel_destinataire = 'NOM@EXEMPLE.CA'
                AND demandeur_id IN ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a3')) <> 2 THEN
            RAISE EXCEPTION 'demandes non retrouvees';
        END IF;
    END $x$ $q$, 'ok');

-- Rattachement à un compte quand la personne s'inscrit avec ce courriel.
SELECT pg_temp.essayer('courriel remplace par le destinataire', $q$
    UPDATE demande_amitie SET destinataire_id = '00000000-0000-0000-0000-0000000000a4', courriel_destinataire = NULL
    WHERE id = '00000000-0000-0000-0000-0000000000f2' $q$, 'ok');
SELECT pg_temp.essayer('rattachement qui doublerait une demande refuse', $q$
    UPDATE demande_amitie SET destinataire_id = '00000000-0000-0000-0000-0000000000a2', courriel_destinataire = NULL
    WHERE id = '00000000-0000-0000-0000-0000000000f6' $q$, 'ix_demande_amitie_demandeur_id_destinataire_id');

-- Clés étrangères des demandes.
SELECT pg_temp.essayer('demandeur inexistant', $q$
    INSERT INTO demande_amitie (id, demandeur_id, destinataire_id)
    VALUES (gen_random_uuid(), gen_random_uuid(), '00000000-0000-0000-0000-0000000000a2') $q$, 'fk_demande_amitie_utilisateur_demandeur_id');
SELECT pg_temp.essayer('destinataire inexistant', $q$
    INSERT INTO demande_amitie (id, demandeur_id, destinataire_id)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', gen_random_uuid()) $q$, 'fk_demande_amitie_utilisateur_destinataire_id');

-- Amitiés : paire ordonnée, présente une seule fois.
SELECT pg_temp.essayer('amitie ordonnee acceptee', $q$
    INSERT INTO amitie (utilisateur_a_id, utilisateur_b_id)
    VALUES ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a2') $q$, 'ok');
SELECT pg_temp.essayer('date de l amitie par defaut', $q$
    DO $x$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM amitie WHERE utilisateur_a_id = '00000000-0000-0000-0000-0000000000a1'
                       AND utilisateur_b_id = '00000000-0000-0000-0000-0000000000a2' AND cree_le IS NOT NULL) THEN
            RAISE EXCEPTION 'date absente';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('paire inversee refusee', $q$
    INSERT INTO amitie (utilisateur_a_id, utilisateur_b_id)
    VALUES ('00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000a1') $q$, 'ck_amitie_utilisateurs_ordre');
SELECT pg_temp.essayer('amitie avec soi-meme refusee', $q$
    INSERT INTO amitie (utilisateur_a_id, utilisateur_b_id)
    VALUES ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a1') $q$, 'ck_amitie_utilisateurs_ordre');
SELECT pg_temp.essayer('meme amitie deux fois refusee', $q$
    INSERT INTO amitie (utilisateur_a_id, utilisateur_b_id)
    VALUES ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a2') $q$, 'pk_amitie');
SELECT pg_temp.essayer('plusieurs amis, des deux cotes', $q$
    INSERT INTO amitie (utilisateur_a_id, utilisateur_b_id) VALUES
        ('00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000a3'),
        ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a3') $q$, 'ok');

-- Ordre des identifiants : comparaison octet par octet, sans signe (l'application doit ordonner la paire de la même façon).
SELECT pg_temp.essayer('ordre des uuid sans signe', $q$
    DO $x$ BEGIN
        IF NOT ('7fffffff-0000-0000-0000-000000000000'::uuid < '80000000-0000-0000-0000-000000000000'::uuid) THEN
            RAISE EXCEPTION 'ordre inattendu';
        END IF;
    END $x$ $q$, 'ok');

-- Clés étrangères des amitiés.
SELECT pg_temp.essayer('premier ami inexistant', $q$
    INSERT INTO amitie (utilisateur_a_id, utilisateur_b_id)
    VALUES ('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-0000000000a2') $q$, 'fk_amitie_utilisateur_utilisateur_a_id');
SELECT pg_temp.essayer('second ami inexistant', $q$
    INSERT INTO amitie (utilisateur_a_id, utilisateur_b_id)
    VALUES ('00000000-0000-0000-0000-0000000000a2', 'ffffffff-ffff-ffff-ffff-ffffffffffff') $q$, 'fk_amitie_utilisateur_utilisateur_b_id');

-- Amis d'une personne : les lignes où elle apparaît de l'un ou l'autre côté.
SELECT pg_temp.essayer('amis retrouves des deux cotes', $q$
    DO $x$
    DECLARE v_amis uuid[];
    BEGIN
        SELECT array_agg(autre ORDER BY autre) INTO v_amis FROM (
            SELECT CASE WHEN utilisateur_a_id = '00000000-0000-0000-0000-0000000000a2' THEN utilisateur_b_id ELSE utilisateur_a_id END AS autre
              FROM amitie
             WHERE '00000000-0000-0000-0000-0000000000a2' IN (utilisateur_a_id, utilisateur_b_id)) amis;
        IF v_amis IS DISTINCT FROM ARRAY['00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000a3']::uuid[] THEN
            RAISE EXCEPTION 'amis inattendus';
        END IF;
    END $x$ $q$, 'ok');

-- Suppression du compte a2 : ses demandes et ses amitiés disparaissent avec lui (ADR 0006), celles des autres restent.
SELECT pg_temp.essayer('suppression du compte permise', $q$
    DELETE FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a2' $q$, 'ok');
SELECT pg_temp.essayer('demandes envoyees et recues supprimees', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM demande_amitie WHERE id IN ('00000000-0000-0000-0000-0000000000f1',
                   '00000000-0000-0000-0000-0000000000f3', '00000000-0000-0000-0000-0000000000f5')) THEN
            RAISE EXCEPTION 'demande restante';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('demandes des autres conservees', $q$
    DO $x$ BEGIN
        IF (SELECT count(*) FROM demande_amitie WHERE id IN ('00000000-0000-0000-0000-0000000000f2',
                '00000000-0000-0000-0000-0000000000f4', '00000000-0000-0000-0000-0000000000f6')) <> 3 THEN
            RAISE EXCEPTION 'demande d un autre compte supprimee';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('amities supprimees des deux cotes, les autres conservees', $q$
    DO $x$ BEGIN
        IF EXISTS (SELECT 1 FROM amitie WHERE '00000000-0000-0000-0000-0000000000a2' IN (utilisateur_a_id, utilisateur_b_id)) THEN
            RAISE EXCEPTION 'amitie restante';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM amitie WHERE utilisateur_a_id = '00000000-0000-0000-0000-0000000000a1'
                       AND utilisateur_b_id = '00000000-0000-0000-0000-0000000000a3') THEN
            RAISE EXCEPTION 'amitie d autres comptes supprimee';
        END IF;
    END $x$ $q$, 'ok');
SELECT pg_temp.essayer('suppression du demandeur efface le courriel saisi', $q$
    DO $x$ BEGIN
        DELETE FROM utilisateur WHERE id = '00000000-0000-0000-0000-0000000000a3';
        IF EXISTS (SELECT 1 FROM demande_amitie WHERE id = '00000000-0000-0000-0000-0000000000f6') THEN
            RAISE EXCEPTION 'courriel conserve';
        END IF;
    END $x$ $q$, 'ok');

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

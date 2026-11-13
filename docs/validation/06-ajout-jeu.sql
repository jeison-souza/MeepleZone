-- Validation de l'ajout d'un jeu à la collection sur la base de développement (DepotCollection.TrouverJeuVisibleAsync et AjouterPossessionAsync).
-- Vérifie la visibilité des jeux et, surtout, que les violations de contrainte portent bien les noms que le dépôt reconnaît
-- (ix_possession_utilisateur_id_jeu_id et fk_possession_jeu_jeu_id) : si un nom change, le dépôt relance l'exception au lieu de la traduire.
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
    v_etat text;
BEGIN
    BEGIN
        EXECUTE p_sql;
        v_obtenu := 'ok';
    EXCEPTION WHEN OTHERS THEN
        GET STACKED DIAGNOSTICS v_obtenu = CONSTRAINT_NAME;
        v_etat := SQLSTATE;
        v_obtenu := COALESCE(NULLIF(v_obtenu, ''), v_etat) || ' (' || v_etat || ')';
    END;
    INSERT INTO resultat VALUES (p_test, p_attendu, v_obtenu);
END
$f$;

-- Même filtre que le dépôt : jeu validé, ou jeu en attente créé par l'utilisateur.
CREATE FUNCTION pg_temp.visible(p_utilisateur uuid, p_jeu uuid) RETURNS text
LANGUAGE sql AS $f$
    SELECT count(*)::text FROM jeu j
    WHERE j.id = p_jeu AND (j.statut = 'valide' OR (j.statut = 'en_attente' AND j.cree_par_id = p_utilisateur))
$f$;

-- Données : deux utilisateurs et quatre jeux (validé, en attente de 1, en attente de 2, refusé).
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH'),
    ('00000000-0000-0000-0000-0000000000a2', 'Essai 2', 'JKLMNPQR');
INSERT INTO jeu (id, nom, statut, cree_par_id) VALUES
    ('00000000-0000-0000-0000-0000000000c1', 'Valide',           'valide',     '00000000-0000-0000-0000-0000000000a2'),
    ('00000000-0000-0000-0000-0000000000c2', 'Attente de 1',     'en_attente', '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000c3', 'Attente de 2',     'en_attente', '00000000-0000-0000-0000-0000000000a2'),
    ('00000000-0000-0000-0000-0000000000c4', 'Refuse',           'refuse',     '00000000-0000-0000-0000-0000000000a1');

-- Visibilité.
INSERT INTO resultat VALUES ('jeu valide visible pour 1', '1', pg_temp.visible('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1'));
INSERT INTO resultat VALUES ('jeu valide visible pour 2', '1', pg_temp.visible('00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c1'));
INSERT INTO resultat VALUES ('jeu en attente visible pour son createur', '1', pg_temp.visible('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c2'));
INSERT INTO resultat VALUES ('jeu en attente invisible pour un autre', '0', pg_temp.visible('00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c2'));
INSERT INTO resultat VALUES ('jeu refuse invisible pour son createur', '0', pg_temp.visible('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c4'));
INSERT INTO resultat VALUES ('jeu inexistant invisible', '0', pg_temp.visible('00000000-0000-0000-0000-0000000000a1', gen_random_uuid()));

-- Ajout : un premier ajout passe, un second du même couple viole l'index unique (traduit par DejaPossedee).
SELECT pg_temp.essayer('premier ajout', $q$
    INSERT INTO possession (id, utilisateur_id, jeu_id)
    VALUES ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1') $q$, 'ok');
SELECT pg_temp.essayer('second ajout du meme jeu', $q$
    INSERT INTO possession (id, utilisateur_id, jeu_id)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1') $q$, 'ix_possession_utilisateur_id_jeu_id (23505)');

-- Un autre utilisateur peut ajouter le même jeu ; un utilisateur peut ajouter son jeu en attente.
SELECT pg_temp.essayer('meme jeu par un autre utilisateur', $q$
    INSERT INTO possession (id, utilisateur_id, jeu_id)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c1') $q$, 'ok');
SELECT pg_temp.essayer('ajouter son jeu en attente', $q$
    INSERT INTO possession (id, utilisateur_id, jeu_id)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c2') $q$, 'ok');

-- Jeu supprimé entre la lecture et l'écriture : violation de clé étrangère (traduite par JeuInexistant).
SELECT pg_temp.essayer('ajout d''un jeu inexistant', $q$
    INSERT INTO possession (id, utilisateur_id, jeu_id)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', gen_random_uuid()) $q$, 'fk_possession_jeu_jeu_id (23503)');

-- Valeurs par défaut d'une possession créée par le gestionnaire (aucune note, aucun commentaire, 0 partie).
INSERT INTO resultat SELECT 'valeurs par defaut de la possession ajoutee', 'null|null|0',
    (SELECT COALESCE(note::text, 'null') || '|' || COALESCE(commentaire, 'null') || '|' || parties_jouees
     FROM possession WHERE id = '00000000-0000-0000-0000-0000000000d1');

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

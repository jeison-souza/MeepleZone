-- Validation des écritures de DepotCollection sur la base de développement (ModifierPossessionAsync et RetirerPossessionAsync).
-- Reproduit en SQL les filtres du dépôt (id + utilisateur) et vérifie les contraintes que la couche métier doit respecter.
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Limite : le champ modifie_le est rempli par l'intercepteur de l'application, pas par la base ; ce script ne peut donc pas le vérifier.
-- Exécuter en entier (Run all). Résultat attendu : toutes les lignes de la dernière requête avec ok = true.

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

-- Données : deux utilisateurs, deux jeux ; chaque utilisateur possède le jeu 1.
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH'),
    ('00000000-0000-0000-0000-0000000000a2', 'Essai 2', 'JKLMNPQR');
INSERT INTO jeu (id, nom, cree_par_id) VALUES
    ('00000000-0000-0000-0000-0000000000c1', 'Jeu 1', '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000c2', 'Jeu 2', '00000000-0000-0000-0000-0000000000a1');
INSERT INTO possession (id, utilisateur_id, jeu_id, note, commentaire, parties_jouees) VALUES
    ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1', 5, 'avant', 3),
    ('00000000-0000-0000-0000-0000000000d2', '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c1', 8, 'autre', 1);

-- Modifier : le filtre (id + utilisateur) ne touche rien chez un autre utilisateur.
WITH u AS (
    UPDATE possession SET note = 9, commentaire = 'pirate', parties_jouees = 99
    WHERE id = '00000000-0000-0000-0000-0000000000d1' AND utilisateur_id = '00000000-0000-0000-0000-0000000000a2'
    RETURNING 1)
INSERT INTO resultat SELECT 'modifier la possession d''un autre : 0 ligne', '0', (SELECT count(*)::text FROM u);
INSERT INTO resultat SELECT 'la possession de l''autre reste intacte', '5|avant|3',
    (SELECT note || '|' || commentaire || '|' || parties_jouees FROM possession WHERE id = '00000000-0000-0000-0000-0000000000d1');

-- Modifier : le propriétaire modifie les trois champs.
WITH u AS (
    UPDATE possession SET note = 7, commentaire = 'après', parties_jouees = 12
    WHERE id = '00000000-0000-0000-0000-0000000000d1' AND utilisateur_id = '00000000-0000-0000-0000-0000000000a1'
    RETURNING 1)
INSERT INTO resultat SELECT 'modifier sa propre possession : 1 ligne', '1', (SELECT count(*)::text FROM u);
INSERT INTO resultat SELECT 'les trois champs sont modifiés', '7|après|12',
    (SELECT note || '|' || commentaire || '|' || parties_jouees FROM possession WHERE id = '00000000-0000-0000-0000-0000000000d1');

-- Modifier : valeurs limites (les mêmes que celles validées par le gestionnaire).
SELECT pg_temp.essayer('note nulle', $q$ UPDATE possession SET note = NULL WHERE id = '00000000-0000-0000-0000-0000000000d1' $q$, 'ok');
SELECT pg_temp.essayer('note = 1', $q$ UPDATE possession SET note = 1 WHERE id = '00000000-0000-0000-0000-0000000000d1' $q$, 'ok');
SELECT pg_temp.essayer('note = 10', $q$ UPDATE possession SET note = 10 WHERE id = '00000000-0000-0000-0000-0000000000d1' $q$, 'ok');
SELECT pg_temp.essayer('note = 0 refusee', $q$ UPDATE possession SET note = 0 WHERE id = '00000000-0000-0000-0000-0000000000d1' $q$, 'ck_possession_note');
SELECT pg_temp.essayer('note = 11 refusee', $q$ UPDATE possession SET note = 11 WHERE id = '00000000-0000-0000-0000-0000000000d1' $q$, 'ck_possession_note');
SELECT pg_temp.essayer('parties = 0 acceptee', $q$ UPDATE possession SET parties_jouees = 0 WHERE id = '00000000-0000-0000-0000-0000000000d1' $q$, 'ok');
SELECT pg_temp.essayer('parties = -1 refusee', $q$ UPDATE possession SET parties_jouees = -1 WHERE id = '00000000-0000-0000-0000-0000000000d1' $q$, 'ck_possession_parties_jouees');
SELECT pg_temp.essayer('commentaire de 2000 caracteres', format($q$ UPDATE possession SET commentaire = %L WHERE id = '00000000-0000-0000-0000-0000000000d1' $q$, repeat('a', 2000)), 'ok');

-- Retirer : le filtre (id + utilisateur) ne supprime rien chez un autre utilisateur.
WITH d AS (
    DELETE FROM possession
    WHERE id = '00000000-0000-0000-0000-0000000000d1' AND utilisateur_id = '00000000-0000-0000-0000-0000000000a2'
    RETURNING 1)
INSERT INTO resultat SELECT 'retirer la possession d''un autre : 0 ligne', '0', (SELECT count(*)::text FROM d);
INSERT INTO resultat SELECT 'la possession existe toujours', '1', (SELECT count(*)::text FROM possession WHERE id = '00000000-0000-0000-0000-0000000000d1');

-- Retirer : le propriétaire retire sa possession ; le jeu reste dans le catalogue.
WITH d AS (
    DELETE FROM possession
    WHERE id = '00000000-0000-0000-0000-0000000000d1' AND utilisateur_id = '00000000-0000-0000-0000-0000000000a1'
    RETURNING 1)
INSERT INTO resultat SELECT 'retirer sa propre possession : 1 ligne', '1', (SELECT count(*)::text FROM d);
INSERT INTO resultat SELECT 'le jeu reste dans le catalogue', '1', (SELECT count(*)::text FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c1');
INSERT INTO resultat SELECT 'l''autre utilisateur garde sa possession', '1', (SELECT count(*)::text FROM possession WHERE id = '00000000-0000-0000-0000-0000000000d2');

-- Retirer deux fois : la seconde ne trouve plus rien.
WITH d AS (
    DELETE FROM possession
    WHERE id = '00000000-0000-0000-0000-0000000000d1' AND utilisateur_id = '00000000-0000-0000-0000-0000000000a1'
    RETURNING 1)
INSERT INTO resultat SELECT 'retirer une possession deja retiree : 0 ligne', '0', (SELECT count(*)::text FROM d);

-- Après le retrait, le même jeu peut être ajouté de nouveau (l'unicité utilisateur + jeu est libérée).
SELECT pg_temp.essayer('reajouter le jeu apres retrait', $q$
    INSERT INTO possession (id, utilisateur_id, jeu_id)
    VALUES (gen_random_uuid(), '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1') $q$, 'ok');

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

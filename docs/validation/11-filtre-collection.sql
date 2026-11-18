-- Validation du filtre de la collection sur la base de développement (issue #13, opération ListerCollection).
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Exécuter en entier (Run all) avec le compte de déploiement. Résultat attendu : toutes les lignes de la dernière requête avec ok = true.
-- Les requêtes reproduisent, en SQL, la sémantique des critères du dépôt (nombre de joueurs dans l'intervalle, durée maximale,
-- note minimale, parties jouées, mode, mécaniques au choix). Le SQL réellement produit par EF Core n'est confirmé que par l'usage.
-- Les valeurs attendues viennent des données de départ ci-dessous, pas du code testé.

SELECT current_user AS compte;

BEGIN;

CREATE TEMP TABLE resultat (test text, attendu text, obtenu text);

-- Retourne les noms des jeux de la requête, triés et séparés par des virgules ('' si aucun).
CREATE FUNCTION pg_temp.noms(p_sql text) RETURNS text
LANGUAGE plpgsql AS $f$
DECLARE
    v_obtenu text;
BEGIN
    EXECUTE 'SELECT COALESCE(string_agg(nom, '','' ORDER BY nom), '''') FROM (' || p_sql || ') t' INTO v_obtenu;
    RETURN v_obtenu;
END
$f$;

CREATE FUNCTION pg_temp.verifier(p_test text, p_sql text, p_attendu text) RETURNS void
LANGUAGE plpgsql AS $f$
BEGIN
    INSERT INTO resultat VALUES (p_test, p_attendu, pg_temp.noms(p_sql));
END
$f$;

-- Données de départ. La personne a1 possède quatre jeux, la personne a2 en possède un (isolement entre personnes).
--   Alpha   : 2 à 4 joueurs, meilleur à 3, 60 min max, Compétitif, Bluff           ; note 8,  5 parties
--   Bravo   : 1 à 2 joueurs, meilleur inconnu, 30 min max, Coopératif, Bluff + Déduction ; note 6,  0 partie
--   Charlie : tout inconnu (joueurs, durée, mode), aucune mécanique              ; note inconnue, 0 partie
--   Delta   : 3 à 6 joueurs, meilleur à 4, 120 min max, Compétitif, Déduction     ; note 10, 20 parties
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH'),
    ('00000000-0000-0000-0000-0000000000a2', 'Essai 2', 'JKLMNPQR');

INSERT INTO jeu (id, nom, joueurs_min, joueurs_max, joueurs_meilleur, duree_max_minutes, mode_jeu_id, cree_par_id, statut) VALUES
    ('00000000-0000-0000-0000-0000000000c1', 'Alpha', 2, 4, 3, 60,
        (SELECT id FROM mode_jeu WHERE nom = 'Compétitif'), '00000000-0000-0000-0000-0000000000a1', 'valide'),
    ('00000000-0000-0000-0000-0000000000c2', 'Bravo', 1, 2, NULL, 30,
        (SELECT id FROM mode_jeu WHERE nom = 'Coopératif'), '00000000-0000-0000-0000-0000000000a1', 'valide'),
    ('00000000-0000-0000-0000-0000000000c3', 'Charlie', NULL, NULL, NULL, NULL,
        NULL, '00000000-0000-0000-0000-0000000000a1', 'en_attente'),
    ('00000000-0000-0000-0000-0000000000c4', 'Delta', 3, 6, 4, 120,
        (SELECT id FROM mode_jeu WHERE nom = 'Compétitif'), '00000000-0000-0000-0000-0000000000a1', 'valide');

INSERT INTO jeu_mecanique (jeu_id, mecanique_id) VALUES
    ('00000000-0000-0000-0000-0000000000c1', (SELECT id FROM mecanique WHERE nom = 'Bluff')),
    ('00000000-0000-0000-0000-0000000000c2', (SELECT id FROM mecanique WHERE nom = 'Bluff')),
    ('00000000-0000-0000-0000-0000000000c2', (SELECT id FROM mecanique WHERE nom = 'Déduction')),
    ('00000000-0000-0000-0000-0000000000c4', (SELECT id FROM mecanique WHERE nom = 'Déduction'));

INSERT INTO possession (id, utilisateur_id, jeu_id, note, parties_jouees) VALUES
    ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1', 8, 5),
    ('00000000-0000-0000-0000-0000000000d2', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c2', 6, 0),
    ('00000000-0000-0000-0000-0000000000d3', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c3', NULL, 0),
    ('00000000-0000-0000-0000-0000000000d4', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c4', 10, 20),
    ('00000000-0000-0000-0000-0000000000d5', '00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-0000000000c1', 9, 1);

-- Possessions de a1 avec leur jeu (la base de toutes les requêtes ci-dessous).
CREATE TEMP VIEW pv AS
SELECT j.nom, j.joueurs_min, j.joueurs_max, j.joueurs_meilleur, j.duree_max_minutes, j.mode_jeu_id,
       p.jeu_id, p.note, p.parties_jouees
FROM possession p
JOIN jeu j ON j.id = p.jeu_id
WHERE p.utilisateur_id = '00000000-0000-0000-0000-0000000000a1';

SELECT pg_temp.verifier('Sans filtre : seules les possessions de la personne', 'SELECT nom FROM pv', 'Alpha,Bravo,Charlie,Delta');
SELECT pg_temp.verifier('3 joueurs : dans l''intervalle, inconnus exclus', 'SELECT nom FROM pv WHERE joueurs_min <= 3 AND joueurs_max >= 3', 'Alpha,Delta');
SELECT pg_temp.verifier('2 joueurs : bornes incluses', 'SELECT nom FROM pv WHERE joueurs_min <= 2 AND joueurs_max >= 2', 'Alpha,Bravo');
SELECT pg_temp.verifier('Meilleur à 3', 'SELECT nom FROM pv WHERE joueurs_meilleur = 3', 'Alpha');
SELECT pg_temp.verifier('Durée maximale 60 : borne incluse, inconnue exclue', 'SELECT nom FROM pv WHERE duree_max_minutes <= 60', 'Alpha,Bravo');
SELECT pg_temp.verifier('Note minimale 8 : borne incluse, note inconnue exclue', 'SELECT nom FROM pv WHERE note >= 8', 'Alpha,Delta');
SELECT pg_temp.verifier('Parties jouées au moins 1', 'SELECT nom FROM pv WHERE parties_jouees >= 1', 'Alpha,Delta');
SELECT pg_temp.verifier('Jamais joué (au plus 0 partie)', 'SELECT nom FROM pv WHERE parties_jouees <= 0', 'Bravo,Charlie');
SELECT pg_temp.verifier('Parties jouées entre 5 et 5', 'SELECT nom FROM pv WHERE parties_jouees >= 5 AND parties_jouees <= 5', 'Alpha');
SELECT pg_temp.verifier('Mode Compétitif',
    'SELECT nom FROM pv WHERE mode_jeu_id = (SELECT id FROM mode_jeu WHERE nom = ''Compétitif'')', 'Alpha,Delta');
SELECT pg_temp.verifier('Une mécanique (Bluff)',
    'SELECT nom FROM pv WHERE EXISTS (SELECT 1 FROM jeu_mecanique jm WHERE jm.jeu_id = pv.jeu_id AND jm.mecanique_id IN (SELECT id FROM mecanique WHERE nom = ''Bluff''))',
    'Alpha,Bravo');
SELECT pg_temp.verifier('Deux mécaniques au choix : ET non exigé, sans doublon',
    'SELECT nom FROM pv WHERE EXISTS (SELECT 1 FROM jeu_mecanique jm WHERE jm.jeu_id = pv.jeu_id AND jm.mecanique_id IN (SELECT id FROM mecanique WHERE nom IN (''Bluff'', ''Déduction'')))',
    'Alpha,Bravo,Delta');
SELECT pg_temp.verifier('Critères combinés : 3 joueurs, Compétitif, Déduction',
    'SELECT nom FROM pv WHERE joueurs_min <= 3 AND joueurs_max >= 3 AND mode_jeu_id = (SELECT id FROM mode_jeu WHERE nom = ''Compétitif'') AND EXISTS (SELECT 1 FROM jeu_mecanique jm WHERE jm.jeu_id = pv.jeu_id AND jm.mecanique_id IN (SELECT id FROM mecanique WHERE nom = ''Déduction''))',
    'Delta');
SELECT pg_temp.verifier('Critères sans correspondance : liste vide', 'SELECT nom FROM pv WHERE note >= 9 AND parties_jouees <= 0', '');

-- Noms des mécaniques d'un jeu, triés (colonne MecaniqueNoms du résultat).
INSERT INTO resultat
SELECT 'Mécaniques de Bravo, triées', 'Bluff,Déduction',
       (SELECT string_agg(m.nom, ',' ORDER BY m.nom)
        FROM jeu_mecanique jm JOIN mecanique m ON m.id = jm.mecanique_id
        WHERE jm.jeu_id = '00000000-0000-0000-0000-0000000000c2');

INSERT INTO resultat
SELECT 'Nom du mode de Charlie : inconnu', '<aucun>',
       COALESCE((SELECT m.nom FROM mode_jeu m WHERE m.id = (SELECT mode_jeu_id FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c3')), '<aucun>');

SELECT test, attendu, obtenu, attendu IS NOT DISTINCT FROM obtenu AS ok FROM resultat;

ROLLBACK;

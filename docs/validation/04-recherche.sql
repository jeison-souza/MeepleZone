-- Validation de la recherche sans accents (extension unaccent) sur la base de développement.
-- Reproduit en SQL les filtres de DepotCollection : visibilité des jeux, recherche de nom sans accents ni casse, indicateur de possession.
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Prérequis : schema-idempotent.sql appliqué (la migration CreationExtensionUnaccent crée l'extension).
-- Exécuter en entier (Run all). Résultat attendu : toutes les lignes de la dernière requête avec ok = true.

-- A. Constats (lecture seule). Attendu : l'extension est installée et la migration figure en tête.
SELECT extname, extversion FROM pg_extension WHERE extname = 'unaccent';
SELECT migration_id FROM historique_migration ORDER BY migration_id DESC LIMIT 3;

-- B. Essais, dans une transaction annulée à la fin.
BEGIN;

CREATE TEMP TABLE resultat (test text, attendu text, obtenu text);

-- Même logique que le dépôt : jeux validés, plus les jeux en attente créés par l'utilisateur, filtrés par le nom.
CREATE FUNCTION pg_temp.chercher(p_utilisateur uuid, p_texte text, p_saut int, p_prendre int) RETURNS text
LANGUAGE sql AS $f$
    -- L'agrégat est trié par id (et non par nom) : l'ordre des noms dépend de la collation de la base, pas ce que le test vérifie.
    SELECT COALESCE(string_agg(t.nom || ':' || t.deja, ',' ORDER BY t.id), '')
    FROM (
        SELECT j.nom, j.id,
               CASE WHEN EXISTS (SELECT 1 FROM possession p WHERE p.utilisateur_id = p_utilisateur AND p.jeu_id = j.id) THEN 'oui' ELSE 'non' END AS deja
        FROM jeu j
        WHERE (j.statut = 'valide' OR (j.statut = 'en_attente' AND j.cree_par_id = p_utilisateur))
          AND strpos(lower(unaccent(j.nom)), lower(unaccent(p_texte))) > 0
        ORDER BY j.nom, j.id
        OFFSET p_saut LIMIT p_prendre
    ) t
$f$;

CREATE FUNCTION pg_temp.total(p_utilisateur uuid, p_texte text) RETURNS text
LANGUAGE sql AS $f$
    SELECT count(*)::text
    FROM jeu j
    WHERE (j.statut = 'valide' OR (j.statut = 'en_attente' AND j.cree_par_id = p_utilisateur))
      AND strpos(lower(unaccent(j.nom)), lower(unaccent(p_texte))) > 0
$f$;

-- Filtre de la collection : possessions de l'utilisateur dont le nom du jeu correspond.
CREATE FUNCTION pg_temp.collection(p_utilisateur uuid, p_texte text) RETURNS text
LANGUAGE sql AS $f$
    SELECT COALESCE(string_agg(j.nom, ',' ORDER BY j.nom, p.id), '')
    FROM possession p
    JOIN jeu j ON j.id = p.jeu_id
    WHERE p.utilisateur_id = p_utilisateur
      AND strpos(lower(unaccent(j.nom)), lower(unaccent(p_texte))) > 0
$f$;

-- Données : deux utilisateurs et des jeux de statuts et de créateurs variés.
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH'),
    ('00000000-0000-0000-0000-0000000000a2', 'Essai 2', 'JKLMNPQR');
INSERT INTO jeu (id, nom, statut, cree_par_id) VALUES
    ('00000000-0000-0000-0000-0000000000c1', 'Éléphant',        'valide',     '00000000-0000-0000-0000-0000000000a2'),
    ('00000000-0000-0000-0000-0000000000c2', 'Café Crème',      'valide',     '00000000-0000-0000-0000-0000000000a2'),
    ('00000000-0000-0000-0000-0000000000c3', 'Cafe Noir',       'valide',     '00000000-0000-0000-0000-0000000000a2'),
    ('00000000-0000-0000-0000-0000000000c4', 'Elephant bis',    'en_attente', '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000c5', 'Elephant cache',  'en_attente', '00000000-0000-0000-0000-0000000000a2'),
    ('00000000-0000-0000-0000-0000000000c6', 'Elephant refuse', 'refuse',     '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000c7', '100% Coton',      'valide',     '00000000-0000-0000-0000-0000000000a2'),
    ('00000000-0000-0000-0000-0000000000c8', '100 Coton',       'valide',     '00000000-0000-0000-0000-0000000000a2');
-- Utilisateur 1 possède Éléphant (c1) et Café Crème (c2).
INSERT INTO possession (id, utilisateur_id, jeu_id) VALUES
    ('00000000-0000-0000-0000-0000000000d1', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c1'),
    ('00000000-0000-0000-0000-0000000000d2', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000c2');

-- Sans accents ni casse : « elephant » trouve « Éléphant », « ÉLÉPHANT » aussi.
INSERT INTO resultat VALUES ('elephant trouve Éléphant (sans accents)', 'Éléphant:oui,Elephant bis:non', pg_temp.chercher('00000000-0000-0000-0000-0000000000a1', 'elephant', 0, 24));
INSERT INTO resultat VALUES ('ELEPHANT en majuscules', 'Éléphant:oui,Elephant bis:non', pg_temp.chercher('00000000-0000-0000-0000-0000000000a1', 'ELEPHANT', 0, 24));
INSERT INTO resultat VALUES ('Éléphant avec accents trouve aussi Elephant', 'Éléphant:oui,Elephant bis:non', pg_temp.chercher('00000000-0000-0000-0000-0000000000a1', 'Éléphant', 0, 24));
INSERT INTO resultat VALUES ('cafe trouve Café et Cafe', 'Café Crème:oui,Cafe Noir:non', pg_temp.chercher('00000000-0000-0000-0000-0000000000a1', 'cafe', 0, 24));
INSERT INTO resultat VALUES ('contient (milieu du nom)', 'Café Crème:oui', pg_temp.chercher('00000000-0000-0000-0000-0000000000a1', 'creme', 0, 24));

-- Visibilité : un jeu en attente n'est visible que pour son créateur ; un jeu refusé jamais.
INSERT INTO resultat VALUES ('utilisateur 2 voit son jeu en attente, pas celui de 1', 'Éléphant:non,Elephant cache:non', pg_temp.chercher('00000000-0000-0000-0000-0000000000a2', 'elephant', 0, 24));
INSERT INTO resultat VALUES ('jeu refuse jamais visible', '0', (SELECT count(*)::text FROM (SELECT 1 WHERE pg_temp.chercher('00000000-0000-0000-0000-0000000000a1', 'refuse', 0, 24) <> '') x));

-- Les jokers du texte saisi sont littéraux (« % » ne veut pas dire « n'importe quoi »).
INSERT INTO resultat VALUES ('pourcent litteral', '100% Coton:non', pg_temp.chercher('00000000-0000-0000-0000-0000000000a1', '100%', 0, 24));

-- Total et pagination.
INSERT INTO resultat VALUES ('total elephant (utilisateur 1)', '2', pg_temp.total('00000000-0000-0000-0000-0000000000a1', 'elephant'));
-- L'ordre des noms (collation) est déjà validé par 03-collection-lecture.sql : ici, seulement la taille des pages.
INSERT INTO resultat VALUES ('une page de taille 1 contient un jeu', '1', array_length(string_to_array(pg_temp.chercher('00000000-0000-0000-0000-0000000000a1', 'elephant', 0, 1), ','), 1)::text);
INSERT INTO resultat VALUES ('page au-dela de la fin', '', pg_temp.chercher('00000000-0000-0000-0000-0000000000a1', 'elephant', 2, 1));
INSERT INTO resultat VALUES ('aucun resultat', '', pg_temp.chercher('00000000-0000-0000-0000-0000000000a1', 'zzz', 0, 24));

-- Filtre de la collection : seulement les jeux possédés par l'utilisateur.
INSERT INTO resultat VALUES ('collection filtree par elephant', 'Éléphant', pg_temp.collection('00000000-0000-0000-0000-0000000000a1', 'elephant'));
INSERT INTO resultat VALUES ('collection filtree par cafe', 'Café Crème', pg_temp.collection('00000000-0000-0000-0000-0000000000a1', 'cafe'));
INSERT INTO resultat VALUES ('collection filtree sans correspondance', '', pg_temp.collection('00000000-0000-0000-0000-0000000000a1', 'coton'));

-- Résultat : ok doit valoir true partout.
SELECT test, attendu, obtenu, attendu = obtenu AS ok FROM resultat ORDER BY ok, test;

ROLLBACK;

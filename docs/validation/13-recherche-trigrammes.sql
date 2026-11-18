-- Validation de la recherche de jeux et de la détection des doublons par trigrammes (ADR 0015, issue #134).
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Prérequis : migration AjoutRechercheTrigrammes appliquée.
-- Exécuter en entier (Run all) avec le compte de déploiement. Résultat attendu : toutes les lignes de la dernière requête avec ok = true.
-- Les requêtes reproduisent, en SQL, la règle du dépôt : recherche = « contient » puis similarité par mots (seuil 0,6),
-- classée par pertinence; doublons = similarité du nom entier (seuil 0,5). Les seuils sont les constantes SeuilSimilariteMots
-- et SeuilSimilariteDoublons du dépôt. Le SQL réellement produit par EF Core est confirmé par l'exécution du dépôt contre la base.
-- Les valeurs attendues viennent de la définition de la similarité de pg_trgm (calculée à la main), pas du code testé.

SELECT current_user AS compte;

BEGIN;

CREATE TEMP TABLE resultat (test text, attendu text, obtenu text);

INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH');

INSERT INTO jeu (id, nom, cree_par_id, statut) VALUES
    ('00000000-0000-0000-0000-0000000000c1', 'Catan', '00000000-0000-0000-0000-0000000000a1', 'valide'),
    ('00000000-0000-0000-0000-0000000000c2', 'Les Colons de Catan', '00000000-0000-0000-0000-0000000000a1', 'valide'),
    ('00000000-0000-0000-0000-0000000000c3', 'Azul', '00000000-0000-0000-0000-0000000000a1', 'valide'),
    ('00000000-0000-0000-0000-0000000000c4', 'Azur', '00000000-0000-0000-0000-0000000000a1', 'valide'),
    ('00000000-0000-0000-0000-0000000000c5', 'Café Crème', '00000000-0000-0000-0000-0000000000a1', 'valide'),
    ('00000000-0000-0000-0000-0000000000c6', 'Éclair de Lune', '00000000-0000-0000-0000-0000000000a1', 'valide'),
    ('00000000-0000-0000-0000-0000000000c7', 'Pandemic', '00000000-0000-0000-0000-0000000000a1', 'valide'),
    ('00000000-0000-0000-0000-0000000000c8', 'Go', '00000000-0000-0000-0000-0000000000a1', 'valide');

-- Les jeux de l'essai seulement : la base de développement contient d'autres jeux.
CREATE TEMP VIEW essai AS
SELECT nom, nom_recherche FROM jeu WHERE cree_par_id = '00000000-0000-0000-0000-0000000000a1';

-- Recherche (texte de 3 caractères ou plus) : contient d'abord, puis similarité par mots, puis nom.
CREATE FUNCTION pg_temp.rechercher(p_texte text) RETURNS text
LANGUAGE sql AS $f$
    SELECT COALESCE(string_agg(nom, ',' ORDER BY (strpos(nom_recherche, lower(unaccent_immuable(p_texte))) > 0) DESC,
                                                 word_similarity(lower(unaccent_immuable(p_texte)), nom_recherche) DESC, nom), '')
    FROM essai
    WHERE strpos(nom_recherche, lower(unaccent_immuable(p_texte))) > 0
       OR word_similarity(lower(unaccent_immuable(p_texte)), nom_recherche) >= 0.6
$f$;

-- Recherche d'un texte trop court : « contient » seulement.
CREATE FUNCTION pg_temp.rechercher_court(p_texte text) RETURNS text
LANGUAGE sql AS $f$
    SELECT COALESCE(string_agg(nom, ',' ORDER BY nom), '')
    FROM essai
    WHERE strpos(nom_recherche, lower(unaccent_immuable(p_texte))) > 0
$f$;

-- Doublons (nom de 3 caractères ou plus) : similarité du nom entier, du plus semblable au moins semblable.
CREATE FUNCTION pg_temp.doublons(p_nom text) RETURNS text
LANGUAGE sql AS $f$
    SELECT COALESCE(string_agg(nom, ',' ORDER BY similarity(lower(unaccent_immuable(p_nom)), nom_recherche) DESC, nom), '')
    FROM essai
    WHERE similarity(lower(unaccent_immuable(p_nom)), nom_recherche) >= 0.5
$f$;

-- Doublons d'un nom trop court : nom identique sans accents ni casse.
CREATE FUNCTION pg_temp.doublons_court(p_nom text) RETURNS text
LANGUAGE sql AS $f$
    SELECT COALESCE(string_agg(nom, ',' ORDER BY nom), '')
    FROM essai
    WHERE nom_recherche = lower(unaccent_immuable(p_nom))
$f$;

-- A. Recherche.
INSERT INTO resultat VALUES ('Recherche catann : la faute de frappe retrouve Catan', 'Catan,Les Colons de Catan', pg_temp.rechercher('catann'));
INSERT INTO resultat VALUES ('Recherche creme : sans accents ni casse', 'Café Crème', pg_temp.rechercher('CREME'));
INSERT INTO resultat VALUES ('Recherche catan : les noms qui contiennent le texte', 'Catan,Les Colons de Catan', pg_temp.rechercher('catan'));
INSERT INTO resultat VALUES ('Recherche cat : mot proche seulement, pas Café', 'Catan,Les Colons de Catan', pg_temp.rechercher('cat'));
INSERT INTO resultat VALUES ('Recherche ca (trop court) : « contient » seulement', 'Café Crème,Catan,Les Colons de Catan', pg_temp.rechercher_court('ca'));
INSERT INTO resultat VALUES ('Recherche zz (trop court) : aucun résultat', '', pg_temp.rechercher_court('zz'));
INSERT INTO resultat VALUES ('Recherche : les jokers sont des caractères', '', pg_temp.rechercher_court('%'));

-- B. Doublons.
INSERT INTO resultat VALUES ('Doublon Catann : Catan', 'Catan', pg_temp.doublons('Catann'));
INSERT INTO resultat VALUES ('Doublon Azul : Azul seul (Azur sous le seuil)', 'Azul', pg_temp.doublons('Azul'));
INSERT INTO resultat VALUES ('Doublon Pandémie : Pandemic', 'Pandemic', pg_temp.doublons('Pandémie'));
INSERT INTO resultat VALUES ('Doublon Cafe Creme : sans accents', 'Café Crème', pg_temp.doublons('Cafe Creme'));
INSERT INTO resultat VALUES ('Doublon Zzzz Game : aucun', '', pg_temp.doublons('Zzzz Game'));
INSERT INTO resultat VALUES ('Doublon GO (trop court) : nom identique', 'Go', pg_temp.doublons_court('GO'));
INSERT INTO resultat VALUES ('Doublon Gu (trop court) : aucun', '', pg_temp.doublons_court('Gu'));

SELECT test, attendu, obtenu, attendu IS NOT DISTINCT FROM obtenu AS ok FROM resultat;

ROLLBACK;

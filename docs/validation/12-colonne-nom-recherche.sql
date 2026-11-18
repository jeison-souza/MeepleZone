-- Validation de la colonne jeu.nom_recherche sur la base de développement (ADR 0015, issue #134).
-- Ne laisse rien dans la base : tout est annulé par le ROLLBACK final.
-- Prérequis : migration AjoutRechercheTrigrammes appliquée (extensions unaccent et pg_trgm, fonction unaccent_immuable).
-- Exécuter en entier (Run all) avec le compte de déploiement. Résultat attendu : toutes les lignes de la dernière requête avec ok = true.
-- Les valeurs attendues viennent de l'ADR 0015 et de la définition de la similarité de pg_trgm, pas du code testé.

SELECT current_user AS compte;

BEGIN;

CREATE TEMP TABLE resultat (test text, attendu text, obtenu text);

-- Données de départ : une personne et quatre jeux.
INSERT INTO utilisateur (id, nom_affichage, code_amitie) VALUES
    ('00000000-0000-0000-0000-0000000000a1', 'Essai 1', 'ABCDEFGH');

INSERT INTO jeu (id, nom, cree_par_id) VALUES
    ('00000000-0000-0000-0000-0000000000c1', 'Éléphant', '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000c2', 'Café Crème', '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000c3', 'ÉCLAIR DE LUNE', '00000000-0000-0000-0000-0000000000a1'),
    ('00000000-0000-0000-0000-0000000000c4', 'Catan', '00000000-0000-0000-0000-0000000000a1');

-- A. Objets créés par la migration.
INSERT INTO resultat
SELECT 'Fonction unaccent_immuable déclarée immuable', 'i', provolatile::text
FROM pg_proc WHERE proname = 'unaccent_immuable';

INSERT INTO resultat
SELECT 'Colonne nom_recherche générée et stockée', 's', attgenerated::text
FROM pg_attribute WHERE attrelid = 'jeu'::regclass AND attname = 'nom_recherche';

INSERT INTO resultat
SELECT 'Index de trigrammes (gin, gin_trgm_ops)', 'true',
       (indexdef LIKE '%USING gin%' AND indexdef LIKE '%gin_trgm_ops%')::text
FROM pg_indexes WHERE tablename = 'jeu' AND indexname = 'ix_jeu_nom_recherche';

INSERT INTO resultat
SELECT 'Extensions unaccent et pg_trgm installées', '2', count(*)::text
FROM pg_extension WHERE extname IN ('unaccent', 'pg_trgm');

-- B. Valeurs calculées : minuscules et sans accents.
INSERT INTO resultat SELECT 'Éléphant', 'elephant', nom_recherche FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c1';
INSERT INTO resultat SELECT 'Café Crème', 'cafe creme', nom_recherche FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c2';
INSERT INTO resultat SELECT 'ÉCLAIR DE LUNE', 'eclair de lune', nom_recherche FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c3';
INSERT INTO resultat SELECT 'Catan', 'catan', nom_recherche FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c4';

-- C. Cohérence : la base recalcule la colonne quand le nom change, sans code d'écriture.
UPDATE jeu SET nom = 'Pêcheurs d''Étoiles' WHERE id = '00000000-0000-0000-0000-0000000000c4';
INSERT INTO resultat SELECT 'Nom modifié : colonne recalculée', 'pecheurs d''etoiles', nom_recherche FROM jeu WHERE id = '00000000-0000-0000-0000-0000000000c4';

-- D. Écriture directe refusée (colonne générée).
CREATE FUNCTION pg_temp.ecrire_directement() RETURNS text
LANGUAGE plpgsql AS $f$
BEGIN
    UPDATE jeu SET nom_recherche = 'x' WHERE id = '00000000-0000-0000-0000-0000000000c1';
    RETURN 'ok';
EXCEPTION WHEN OTHERS THEN
    RETURN SQLSTATE;
END
$f$;
INSERT INTO resultat SELECT 'Écriture directe de nom_recherche refusée', '428C9', pg_temp.ecrire_directement();

-- E. Recherche : « contient » sans accents ni casse (le texte tapé reçoit le même traitement).
-- Limitée aux jeux de l'essai : la base de développement contient d'autres jeux (données de test).
INSERT INTO resultat
SELECT 'creme retrouve Café Crème', 'Café Crème', string_agg(nom, ',')
FROM jeu
WHERE cree_par_id = '00000000-0000-0000-0000-0000000000a1'
  AND nom_recherche LIKE '%' || lower(unaccent_immuable('CREME')) || '%';

INSERT INTO resultat
SELECT 'ELEPHANT retrouve Éléphant', 'Éléphant', string_agg(nom, ',')
FROM jeu
WHERE cree_par_id = '00000000-0000-0000-0000-0000000000a1'
  AND nom_recherche LIKE '%' || lower(unaccent_immuable('ELEPHANT')) || '%';

-- F. Tolérance aux fautes : similarité de trigrammes (seuil par défaut de 0,3).
INSERT INTO resultat SELECT 'Catann ressemble à Catan', 'true', ('catann' % 'catan')::text;
INSERT INTO resultat SELECT 'Catan et Zzzz ne se ressemblent pas', 'false', ('catan' % 'zzzz')::text;
-- Faux positif connu sur les noms courts (ADR 0015) : Azul et Azur dépassent le seuil de 0,3 (3 trigrammes sur 7).
INSERT INTO resultat SELECT 'Azul ressemble à Azur (faux positif connu)', 'true', ('azul' % 'azur')::text;

SELECT test, attendu, obtenu, attendu IS NOT DISTINCT FROM obtenu AS ok FROM resultat;

ROLLBACK;

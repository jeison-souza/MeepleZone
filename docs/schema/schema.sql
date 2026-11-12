CREATE TABLE IF NOT EXISTS historique_migration (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk_historique_migration PRIMARY KEY (migration_id)
);

START TRANSACTION;
CREATE COLLATION courriel_ci (LOCALE = 'und-u-ks-level2',
    PROVIDER = icu,
    DETERMINISTIC = False
);

CREATE TABLE utilisateur (
    id uuid NOT NULL,
    courriel text COLLATE courriel_ci NOT NULL,
    courriel_verifie_le timestamp with time zone,
    nom_affichage text NOT NULL,
    role text NOT NULL DEFAULT 'membre',
    photo_id uuid,
    code_amitie text NOT NULL,
    visibilite_collection text NOT NULL DEFAULT 'privee',
    cree_le timestamp with time zone NOT NULL DEFAULT (now()),
    modifie_le timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT pk_utilisateur PRIMARY KEY (id),
    CONSTRAINT ck_utilisateur_code_amitie CHECK (code_amitie ~ '^[A-HJ-NP-Z2-9]{8}$'),
    CONSTRAINT ck_utilisateur_role CHECK (role IN ('membre', 'administrateur', 'systeme')),
    CONSTRAINT ck_utilisateur_visibilite_collection CHECK (visibilite_collection IN ('privee', 'amis'))
);

CREATE UNIQUE INDEX ix_utilisateur_code_amitie ON utilisateur (code_amitie);

CREATE UNIQUE INDEX ix_utilisateur_courriel ON utilisateur (courriel);

INSERT INTO historique_migration (migration_id, product_version)
VALUES ('20261019181533_CreationTableUtilisateur', '10.0.14');

COMMIT;

START TRANSACTION;
CREATE TABLE identite_externe (
    id uuid NOT NULL,
    utilisateur_id uuid NOT NULL,
    fournisseur text NOT NULL,
    identifiant_compte_externe text NOT NULL,
    courriel text COLLATE courriel_ci NOT NULL,
    derniere_connexion_le timestamp with time zone NOT NULL DEFAULT (now()),
    cree_le timestamp with time zone NOT NULL DEFAULT (now()),
    modifie_le timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT pk_identite_externe PRIMARY KEY (id),
    CONSTRAINT fk_identite_externe_utilisateur_utilisateur_id FOREIGN KEY (utilisateur_id) REFERENCES utilisateur (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ix_identite_externe_fournisseur_identifiant_compte_externe ON identite_externe (fournisseur, identifiant_compte_externe);

CREATE INDEX ix_identite_externe_utilisateur_id ON identite_externe (utilisateur_id);

INSERT INTO historique_migration (migration_id, product_version)
VALUES ('20261020043254_CreationTableIdentiteExterne', '10.0.14');

COMMIT;

START TRANSACTION;
CREATE TABLE annexe (
    id uuid NOT NULL,
    nom_fichier text NOT NULL,
    contenu bytea NOT NULL,
    type_mime text NOT NULL,
    taille_octets integer NOT NULL,
    cree_par_id uuid NOT NULL,
    cree_le timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT pk_annexe PRIMARY KEY (id),
    CONSTRAINT ck_annexe_nom_fichier CHECK (char_length(nom_fichier) BETWEEN 1 AND 255),
    CONSTRAINT ck_annexe_taille_octets CHECK (taille_octets > 0),
    CONSTRAINT fk_annexe_utilisateur_cree_par_id FOREIGN KEY (cree_par_id) REFERENCES utilisateur (id) ON DELETE RESTRICT
);

CREATE INDEX ix_utilisateur_photo_id ON utilisateur (photo_id);

CREATE INDEX ix_annexe_cree_par_id ON annexe (cree_par_id);

ALTER TABLE utilisateur ADD CONSTRAINT fk_utilisateur_annexe_photo_id FOREIGN KEY (photo_id) REFERENCES annexe (id) ON DELETE SET NULL;

INSERT INTO historique_migration (migration_id, product_version)
VALUES ('20261020142005_CreationTableAnnexe', '10.0.14');

COMMIT;

START TRANSACTION;
ALTER TABLE utilisateur ALTER COLUMN courriel DROP NOT NULL;

ALTER TABLE identite_externe ALTER COLUMN courriel DROP NOT NULL;

ALTER TABLE utilisateur ADD CONSTRAINT ck_utilisateur_courriel CHECK (courriel = btrim(courriel) AND courriel <> '');

ALTER TABLE identite_externe ADD CONSTRAINT ck_identite_externe_courriel CHECK (courriel = btrim(courriel) AND courriel <> '');

INSERT INTO historique_migration (migration_id, product_version)
VALUES ('20261021010837_ModificationCourrielFacultatif', '10.0.14');

COMMIT;

START TRANSACTION;
CREATE INDEX ix_identite_externe_courriel ON identite_externe (courriel);

INSERT INTO historique_migration (migration_id, product_version)
VALUES ('20261022134627_AjoutIndexCourrielIdentiteExterne', '10.0.14');

COMMIT;

START TRANSACTION;
CREATE TABLE mecanique (
    id uuid NOT NULL,
    nom text NOT NULL,
    description text,
    bgg_id integer,
    cree_le timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT pk_mecanique PRIMARY KEY (id)
);

CREATE TABLE mode_jeu (
    id uuid NOT NULL,
    nom text NOT NULL,
    description text,
    cree_le timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT pk_mode_jeu PRIMARY KEY (id)
);

CREATE TABLE jeu (
    id uuid NOT NULL,
    nom text NOT NULL,
    annee_publication smallint,
    joueurs_min smallint,
    joueurs_max smallint,
    joueurs_meilleur smallint,
    duree_min_minutes smallint,
    duree_max_minutes smallint,
    age_min smallint,
    complexite numeric(2,1),
    mode_jeu_id uuid,
    editeur text,
    bgg_id integer,
    couverture_id uuid,
    couverture_url text,
    statut text NOT NULL DEFAULT 'en_attente',
    cree_par_id uuid NOT NULL,
    cree_le timestamp with time zone NOT NULL DEFAULT (now()),
    modifie_le timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT pk_jeu PRIMARY KEY (id),
    CONSTRAINT ck_jeu_age_min CHECK (age_min >= 0),
    CONSTRAINT ck_jeu_complexite CHECK (complexite BETWEEN 1 AND 5),
    CONSTRAINT ck_jeu_couverture CHECK (couverture_id IS NULL OR couverture_url IS NULL),
    CONSTRAINT ck_jeu_duree_max_minutes CHECK (duree_max_minutes > 0),
    CONSTRAINT ck_jeu_duree_min_max CHECK (duree_min_minutes IS NULL OR duree_max_minutes IS NULL OR duree_min_minutes <= duree_max_minutes),
    CONSTRAINT ck_jeu_duree_min_minutes CHECK (duree_min_minutes > 0),
    CONSTRAINT ck_jeu_joueurs_max CHECK (joueurs_max >= 1),
    CONSTRAINT ck_jeu_joueurs_meilleur CHECK (joueurs_meilleur >= 1),
    CONSTRAINT ck_jeu_joueurs_meilleur_max CHECK (joueurs_meilleur IS NULL OR joueurs_max IS NULL OR joueurs_meilleur <= joueurs_max),
    CONSTRAINT ck_jeu_joueurs_meilleur_min CHECK (joueurs_meilleur IS NULL OR joueurs_min IS NULL OR joueurs_meilleur >= joueurs_min),
    CONSTRAINT ck_jeu_joueurs_min CHECK (joueurs_min >= 1),
    CONSTRAINT ck_jeu_joueurs_min_max CHECK (joueurs_min IS NULL OR joueurs_max IS NULL OR joueurs_min <= joueurs_max),
    CONSTRAINT ck_jeu_nom CHECK (char_length(nom) BETWEEN 1 AND 255),
    CONSTRAINT ck_jeu_statut CHECK (statut IN ('en_attente', 'valide', 'refuse')),
    CONSTRAINT fk_jeu_annexe_couverture_id FOREIGN KEY (couverture_id) REFERENCES annexe (id) ON DELETE SET NULL,
    CONSTRAINT fk_jeu_mode_jeu_mode_jeu_id FOREIGN KEY (mode_jeu_id) REFERENCES mode_jeu (id) ON DELETE RESTRICT,
    CONSTRAINT fk_jeu_utilisateur_cree_par_id FOREIGN KEY (cree_par_id) REFERENCES utilisateur (id) ON DELETE RESTRICT
);

CREATE TABLE jeu_mecanique (
    jeu_id uuid NOT NULL,
    mecanique_id uuid NOT NULL,
    CONSTRAINT pk_jeu_mecanique PRIMARY KEY (jeu_id, mecanique_id),
    CONSTRAINT fk_jeu_mecanique_jeu_jeu_id FOREIGN KEY (jeu_id) REFERENCES jeu (id) ON DELETE CASCADE,
    CONSTRAINT fk_jeu_mecanique_mecanique_mecanique_id FOREIGN KEY (mecanique_id) REFERENCES mecanique (id) ON DELETE RESTRICT
);

INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7005-bec5-8cc0f7c0f4cf', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Chaque joueur place des pions sur des cases d''action, qui ne peuvent souvent servir qu''une fois par tour.', 'Pose d''ouvriers');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7006-98b6-f2b0c15611b6', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Chaque joueur part d''un petit paquet de cartes et en achète d''autres pour l''améliorer au fil de la partie.', 'Construction de pioche');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7007-99bf-876d55d7363c', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les joueurs assemblent des éléments qui se renforcent entre eux et produisent de plus en plus de ressources.', 'Construction de moteur');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7008-a934-b970b2133b80', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Le défi consiste à choisir quelles cartes jouer, garder ou défausser parmi celles qu''on tient en main.', 'Gestion de main');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7009-ba51-046f870ca39c', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Chaque joueur choisit une carte parmi plusieurs, puis passe le reste à son voisin.', 'Draft de cartes');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-700a-8b25-b3cebeb27ba4', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Le résultat des dés détermine les actions possibles ou le déroulement des événements.', 'Lancer de dés');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-700b-b27f-b08614a1dfa7', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les joueurs posent des tuiles pour construire un paysage ou une structure, avec des règles d''adjacence.', 'Placement de tuiles');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-700c-8f50-56c207831c05', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Il faut avoir plus de présence que les adversaires dans une zone pour la contrôler et en tirer des points.', 'Majorité de zones');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-700d-bb72-356f51a22f8c', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les joueurs réunissent des ensembles d''objets ou de cartes qui rapportent plus quand ils sont complets.', 'Collection de séries');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-700e-9beb-e12f23687eb7', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les joueurs misent pour obtenir des objets, des actions ou l''ordre du tour.', 'Enchères');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-700f-b915-be3362b46bd9', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les joueurs discutent, échangent des ressources ou concluent des ententes entre eux.', 'Échanges et négociation');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7010-b25d-e037b6e19408', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Chaque joueur reçoit une identité secrète, et les camps s''affrontent sans se connaître.', 'Rôles cachés');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7011-89ad-524b7c0b520f', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Il faut recouper des indices pour deviner une information cachée.', 'Déduction');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7012-9023-6a29bfaf8326', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les joueurs dissimulent ou inventent de l''information pour tromper les autres.', 'Bluff');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7013-929f-09f2c4e9a2c6', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'On peut continuer à tenter sa chance pour gagner plus, au risque de tout perdre au tour.', 'Pousser sa chance');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7014-865c-fdf4c2f7d63a', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Chaque joueur dispose de capacités ou de règles qui lui sont propres.', 'Pouvoirs variables');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7015-96d9-267515323aad', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Le plateau est assemblé à partir d''éléments qui changent d''une partie à l''autre.', 'Plateau modulaire');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7016-82ae-e33f4a4219c2', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les pions se déplacent sur des cases, des zones ou des chemins selon des règles de mouvement.', 'Déplacement sur le plateau');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7017-99b9-6516d7a24420', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les joueurs relient des lieux par des routes, des voies ou des liens pour former un réseau.', 'Construction de réseaux');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7018-8b8c-451f0e5e4d58', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Chaque tour, le joueur dispose d''un nombre limité de points à répartir entre plusieurs actions.', 'Points d''action');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7019-8c30-d2e6890d11a0', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Il faut produire, échanger et dépenser des ressources de façon à atteindre ses objectifs.', 'Gestion de ressources');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-701a-a45b-2f1506627cc3', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les joueurs choisissent leurs actions à l''avance, parfois en secret, puis les dévoilent et les exécutent.', 'Programmation d''actions');
INSERT INTO mecanique (id, bgg_id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-701b-833a-1a0892bf63b4', NULL, TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les parties s''enchaînent, et les choix de l''une modifient le jeu pour les suivantes.', 'Campagne et évolution');

INSERT INTO mode_jeu (id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7001-adcc-71b334e95c80', TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Chaque joueur joue pour lui-même et cherche à gagner contre les autres.', 'Compétitif');
INSERT INTO mode_jeu (id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7002-9c8a-87833414cc9b', TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Tous les joueurs gagnent ou perdent ensemble contre le jeu.', 'Coopératif');
INSERT INTO mode_jeu (id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7003-9c80-f4cb84b0633c', TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les joueurs coopèrent, mais chacun peut avoir un objectif personnel, ou l''un d''eux peut trahir les autres.', 'Semi-coopératif');
INSERT INTO mode_jeu (id, cree_le, description, nom)
VALUES ('01a1c7e7-8000-7004-bac3-3649459c0f35', TIMESTAMPTZ '2026-11-11T00:00:00+00:00', 'Les joueurs sont répartis en équipes, et chaque équipe joue contre les autres.', 'Par équipes');

CREATE UNIQUE INDEX ix_jeu_bgg_id ON jeu (bgg_id);

CREATE INDEX ix_jeu_couverture_id ON jeu (couverture_id);

CREATE INDEX ix_jeu_cree_par_id ON jeu (cree_par_id);

CREATE INDEX ix_jeu_mode_jeu_id ON jeu (mode_jeu_id);

CREATE INDEX ix_jeu_mecanique_mecanique_id ON jeu_mecanique (mecanique_id);

CREATE UNIQUE INDEX ix_mecanique_bgg_id ON mecanique (bgg_id);

CREATE UNIQUE INDEX ix_mecanique_nom ON mecanique (nom);

CREATE UNIQUE INDEX ix_mode_jeu_nom ON mode_jeu (nom);

INSERT INTO historique_migration (migration_id, product_version)
VALUES ('20261111190649_CreationTablesCatalogue', '10.0.14');

COMMIT;

START TRANSACTION;
CREATE TABLE possession (
    id uuid NOT NULL,
    utilisateur_id uuid NOT NULL,
    jeu_id uuid NOT NULL,
    note smallint,
    commentaire text,
    parties_jouees integer NOT NULL DEFAULT 0,
    cree_le timestamp with time zone NOT NULL DEFAULT (now()),
    modifie_le timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT pk_possession PRIMARY KEY (id),
    CONSTRAINT ck_possession_note CHECK (note BETWEEN 1 AND 10),
    CONSTRAINT ck_possession_parties_jouees CHECK (parties_jouees >= 0),
    CONSTRAINT fk_possession_jeu_jeu_id FOREIGN KEY (jeu_id) REFERENCES jeu (id) ON DELETE RESTRICT,
    CONSTRAINT fk_possession_utilisateur_utilisateur_id FOREIGN KEY (utilisateur_id) REFERENCES utilisateur (id) ON DELETE CASCADE
);

CREATE INDEX ix_possession_jeu_id ON possession (jeu_id);

CREATE UNIQUE INDEX ix_possession_utilisateur_id_jeu_id ON possession (utilisateur_id, jeu_id);

INSERT INTO historique_migration (migration_id, product_version)
VALUES ('20261111201700_CreationTablePossession', '10.0.14');

COMMIT;

START TRANSACTION;
CREATE EXTENSION IF NOT EXISTS unaccent;

INSERT INTO historique_migration (migration_id, product_version)
VALUES ('20261112052729_CreationExtensionUnaccent', '10.0.14');

COMMIT;


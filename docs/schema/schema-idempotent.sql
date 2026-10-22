CREATE TABLE IF NOT EXISTS historique_migration (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk_historique_migration PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261019181533_CreationTableUtilisateur') THEN
    CREATE COLLATION courriel_ci (LOCALE = 'und-u-ks-level2',
        PROVIDER = icu,
        DETERMINISTIC = False
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261019181533_CreationTableUtilisateur') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261019181533_CreationTableUtilisateur') THEN
    CREATE UNIQUE INDEX ix_utilisateur_code_amitie ON utilisateur (code_amitie);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261019181533_CreationTableUtilisateur') THEN
    CREATE UNIQUE INDEX ix_utilisateur_courriel ON utilisateur (courriel);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261019181533_CreationTableUtilisateur') THEN
    INSERT INTO historique_migration (migration_id, product_version)
    VALUES ('20261019181533_CreationTableUtilisateur', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261020043254_CreationTableIdentiteExterne') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261020043254_CreationTableIdentiteExterne') THEN
    CREATE UNIQUE INDEX ix_identite_externe_fournisseur_identifiant_compte_externe ON identite_externe (fournisseur, identifiant_compte_externe);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261020043254_CreationTableIdentiteExterne') THEN
    CREATE INDEX ix_identite_externe_utilisateur_id ON identite_externe (utilisateur_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261020043254_CreationTableIdentiteExterne') THEN
    INSERT INTO historique_migration (migration_id, product_version)
    VALUES ('20261020043254_CreationTableIdentiteExterne', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261020142005_CreationTableAnnexe') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261020142005_CreationTableAnnexe') THEN
    CREATE INDEX ix_utilisateur_photo_id ON utilisateur (photo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261020142005_CreationTableAnnexe') THEN
    CREATE INDEX ix_annexe_cree_par_id ON annexe (cree_par_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261020142005_CreationTableAnnexe') THEN
    ALTER TABLE utilisateur ADD CONSTRAINT fk_utilisateur_annexe_photo_id FOREIGN KEY (photo_id) REFERENCES annexe (id) ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261020142005_CreationTableAnnexe') THEN
    INSERT INTO historique_migration (migration_id, product_version)
    VALUES ('20261020142005_CreationTableAnnexe', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261021010837_ModificationCourrielFacultatif') THEN
    ALTER TABLE utilisateur ALTER COLUMN courriel DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261021010837_ModificationCourrielFacultatif') THEN
    ALTER TABLE identite_externe ALTER COLUMN courriel DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261021010837_ModificationCourrielFacultatif') THEN
    ALTER TABLE utilisateur ADD CONSTRAINT ck_utilisateur_courriel CHECK (courriel = btrim(courriel) AND courriel <> '');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261021010837_ModificationCourrielFacultatif') THEN
    ALTER TABLE identite_externe ADD CONSTRAINT ck_identite_externe_courriel CHECK (courriel = btrim(courriel) AND courriel <> '');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261021010837_ModificationCourrielFacultatif') THEN
    INSERT INTO historique_migration (migration_id, product_version)
    VALUES ('20261021010837_ModificationCourrielFacultatif', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261022134627_AjoutIndexCourrielIdentiteExterne') THEN
    CREATE INDEX ix_identite_externe_courriel ON identite_externe (courriel);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM historique_migration WHERE "migration_id" = '20261022134627_AjoutIndexCourrielIdentiteExterne') THEN
    INSERT INTO historique_migration (migration_id, product_version)
    VALUES ('20261022134627_AjoutIndexCourrielIdentiteExterne', '10.0.13');
    END IF;
END $EF$;
COMMIT;


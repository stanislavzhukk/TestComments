CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002205310_Initial') THEN
    CREATE TABLE "Comments" (
        "Id" uuid NOT NULL,
        "RootId" uuid,
        "ParentId" uuid,
        "UserName" text NOT NULL,
        "UserEmail" text NOT NULL,
        "Content" text NOT NULL,
        "HomePageUrl" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Comments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002205310_Initial') THEN
    CREATE TABLE "Attachments" (
        "Id" uuid NOT NULL,
        "CommentId" uuid NOT NULL,
        "OriginalFileName" text NOT NULL,
        "InternalFileName" uuid NOT NULL,
        "FilePath" text NOT NULL,
        "Type" integer NOT NULL,
        CONSTRAINT "PK_Attachments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Attachments_Comments_CommentId" FOREIGN KEY ("CommentId") REFERENCES "Comments" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002205310_Initial') THEN
    CREATE INDEX "IX_Attachments_CommentId" ON "Attachments" ("CommentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002205310_Initial') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261002205310_Initial', '10.0.12');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003192503_AddedBasicIndexes') THEN
    CREATE INDEX "IX_Comments_CreatedAt" ON "Comments" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003192503_AddedBasicIndexes') THEN
    CREATE INDEX "IX_Comments_ParentId" ON "Comments" ("ParentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003192503_AddedBasicIndexes') THEN
    CREATE INDEX "IX_Comments_RootId" ON "Comments" ("RootId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003192503_AddedBasicIndexes') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261003192503_AddedBasicIndexes', '10.0.12');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004114216_LengthRestrictionsForComments') THEN
    ALTER TABLE "Comments" ALTER COLUMN "UserName" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004114216_LengthRestrictionsForComments') THEN
    ALTER TABLE "Comments" ALTER COLUMN "UserEmail" TYPE character varying(254);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004114216_LengthRestrictionsForComments') THEN
    ALTER TABLE "Comments" ALTER COLUMN "HomePageUrl" TYPE character varying(2048);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004114216_LengthRestrictionsForComments') THEN
    ALTER TABLE "Comments" ALTER COLUMN "Content" TYPE character varying(5000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004114216_LengthRestrictionsForComments') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261004114216_LengthRestrictionsForComments', '10.0.12');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004120802_UserDataCollection') THEN
    ALTER TABLE "Comments" ADD "UserAgent" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004120802_UserDataCollection') THEN
    ALTER TABLE "Comments" ADD "UserIp" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004120802_UserDataCollection') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261004120802_UserDataCollection', '10.0.12');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004130245_AttachmentPathRemove') THEN
    ALTER TABLE "Attachments" DROP COLUMN "InternalFileName";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004130245_AttachmentPathRemove') THEN
    ALTER TABLE "Attachments" RENAME COLUMN "FilePath" TO "StoredFileName";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261004130245_AttachmentPathRemove') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261004130245_AttachmentPathRemove', '10.0.12');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007193835_AdditionalLengthRestrictionsForEntities') THEN
    ALTER TABLE "Comments" ALTER COLUMN "UserIp" TYPE character varying(45);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007193835_AdditionalLengthRestrictionsForEntities') THEN
    ALTER TABLE "Comments" ALTER COLUMN "UserAgent" TYPE character varying(512);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007193835_AdditionalLengthRestrictionsForEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261007193835_AdditionalLengthRestrictionsForEntities', '10.0.12');
    END IF;
END $EF$;
COMMIT;


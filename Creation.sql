/********** INVENTORY **********/
/********** SOCIAL **********/
CREATE TABLE social_user (
    id UUID NOT NULL DEFAULT (gen_random_uuid()),
	first_name VARCHAR(25) NOT NULL,
	last_name VARCHAR(25) NOT NULL,
	display_name VARCHAR(50) NOT NULL,
    user_name VARCHAR(50) NOT NULL,
    password VARCHAR(32) NOT NULL,
    salt VARCHAR(8) NOT NULL DEFAULT (SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)),
    email VARCHAR(320) NOT NULL,
    sex VARCHAR(10),
	phone VARCHAR(20),
	country VARCHAR(20),
	city VARCHAR(20),
	province VARCHAR(20),
	verified_email BOOLEAN NOT NULL DEFAULT FALSE,
	avatar TEXT,
	status VARCHAR(20) NOT NULL DEFAULT 'Activated',
    roles JSON NOT NULL DEFAULT '[]',
    settings JSON NOT NULL DEFAULT '{}',
	ranks JSON NOT NULL DEFAULT '{}',
    last_access_timestamp TIMESTAMPTZ NULL,
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_social_user" PRIMARY KEY (id),
    CONSTRAINT "CK_Status_Valid_Value" CHECK (status = 'Deleted' OR status = 'Blocked' OR status = 'Activated')
);

CREATE TABLE social_post (
    id UUID NOT NULL DEFAULT (gen_random_uuid()),
	owner UUID NOT NULL,
	last_name VARCHAR(25) NOT NULL,
	display_name VARCHAR(50) NOT NULL,
    user_name VARCHAR(50) NOT NULL,
    password VARCHAR(32) NOT NULL,
    salt VARCHAR(8) NOT NULL DEFAULT (SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)),
    email VARCHAR(320) NOT NULL,
    sex VARCHAR(10),
	phone VARCHAR(20), 
	country VARCHAR(20),
	city VARCHAR(20),
	province VARCHAR(20),
	verified BOOLEAN NOT NULL DEFAULT FALSE,
	avatar TEXT,
	status VARCHAR(20) NOT NULL DEFAULT 'Not Activated',
    roles JSON NOT NULL DEFAULT '[]',
    settings JSON NOT NULL DEFAULT '{}',
	ranks JSON NOT NULL DEFAULT '{}',
    last_access_timestamp TIMESTAMPTZ NULL,
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_social_user" PRIMARY KEY (id),
    CONSTRAINT "CK_Status_Valid_Value" CHECK (status = 'Deleted' OR status = 'Not Activated' OR status = 'Activated' OR status = 'Readonly'),
    CONSTRAINT "CK_LastAccessTimestamp_Valid_Value" CHECK ((last_access_timestamp IS NULL AND status = 'Not Activated') OR (status <> 'Not Activated'))
);

CREATE TABLE social_type_of_post (
	id INTEGER GENERATED ALWAYS AS IDENTITY,
	title VARCHAR(20) NOT NULL,
	display_title VARCHAR(50) NOT NULL,
	describe VARCHAR(100) NOT NULL,
	thumbnail TEXT,
	status VARCHAR(20) NOT NULL,
	followers INTEGER NOT NULL DEFAULT '0',
	likes INTEGER NOT NULL DEFAULT '0',
	created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
	last_modified_timestamp TIMESTAMPTZ,
	CONSTRAINT "PK_social_type_of_post" PRIMARY KEY (id),
    CONSTRAINT "CK_Status_Valid_Value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE social_category (
	id INTEGER GENERATED ALWAYS AS IDENTITY,
	title VARCHAR(20) NOT NULL,
	display_title VARCHAR(50) NOT NULL,
	describe VARCHAR(100) NOT NULL,
	slug TEXT NOT NULL,
	thumbnail TEXT,
	status VARCHAR(20) NOT NULL,
	followers INTEGER NOT NULL DEFAULT '0',
	likes INTEGER NOT NULL DEFAULT '0',
	created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
	last_modified_timestamp TIMESTAMPTZ,
	CONSTRAINT "PK_social_type_of_post" PRIMARY KEY (id),
    CONSTRAINT "CK_Status_Valid_Value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE social_topic (
	id INTEGER GENERATED ALWAYS AS IDENTITY,
	category_id INTEGER NOT NULL,
	title VARCHAR(20) NOT NULL,
	display_title VARCHAR(50) NOT NULL,
	slug TEXT NOT NULL,
	describe VARCHAR(100) NOT NULL,
	thumbnail TEXT,
	status VARCHAR(20) NOT NULL,
	followers INTEGER NOT NULL DEFAULT '0',
	likes INTEGER NOT NULL DEFAULT '0',
	created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
	last_modified_timestamp TIMESTAMPTZ,
	CONSTRAINT "PK_social_type_of_post" PRIMARY KEY (id),
	CONSTRAINT "FK_category_id" FOREIGN KEY social_category(id),
    CONSTRAINT "CK_Status_Valid_Value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE UNIQUE INDEX "IX_social_type_of_post_title" ON social_type_of_post (title) WHERE status != 'Disabled';

CREATE UNIQUE INDEX "IX_social_user_user_name_email" ON social_user (user_name, email) WHERE status != 'Deleted';
/********** CACHED **********/
/********** CONFIG **********/
CREATE TABLE admin_user (
    id UUID NOT NULL DEFAULT (gen_random_uuid()),
    user_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    password VARCHAR(32) NOT NULL,
    salt VARCHAR(8) NOT NULL DEFAULT (SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)),
    email VARCHAR(320) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Activated',
    roles JSON NOT NULL DEFAULT '[]',
    settings JSON NOT NULL DEFAULT '{}',
    last_access_timestamp TIMESTAMPTZ NULL,
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_admin_user" PRIMARY KEY (id),
    CONSTRAINT "CK_Status_Valid_Value" CHECK (status = 'Deleted' OR status = 'Blocked' OR status = 'Activated' OR status = 'Readonly')
);

CREATE TABLE admin_user_right (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    right_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    describe VARCHAR(150) NOT NULL,
    status VARCHAR(20) NOT NULL,
    CONSTRAINT "PK_admin_user_right" PRIMARY KEY (id),
    CONSTRAINT "CK_Status_Valid_Value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE admin_user_role (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    role_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    describe VARCHAR(150) NOT NULL,
    rights JSON NOT NULL DEFAULT '[]',
    status VARCHAR(20) NOT NULL,
    CONSTRAINT "PK_admin_user_role" PRIMARY KEY (id),
    CONSTRAINT "CK_Status_Valid_Value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE base_config (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    config_key VARCHAR(50) NOT NULL,
    value JSON NOT NULL DEFAULT '{}',
    status VARCHAR(20) NOT NULL,
    CONSTRAINT "PK_base_config" PRIMARY KEY (id),
    CONSTRAINT "CK_Status_Valid_Value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE admin_audit_log (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    "table" VARCHAR(50) NOT NULL,
    table_key VARCHAR(100) NOT NULL,
    action VARCHAR(50) NOT NULL,
    old_value TEXT NOT NULL,
    new_value TEXT NOT NULL,
    "user" VARCHAR(50) NOT NULL,
    timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', "table" || ' ' || table_key || ' ' || old_value || ' ' || new_value || ' ' || "user")) STORED,
    CONSTRAINT "PK_admin_audit_log" PRIMARY KEY (id)
);

CREATE TABLE social_audit_log (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    "table" VARCHAR(50) NOT NULL,
    table_key VARCHAR(100) NOT NULL,
    action VARCHAR(50) NOT NULL,
    old_value TEXT NOT NULL,
    new_value TEXT NOT NULL,
    "user" VARCHAR(50) NOT NULL,
    timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', "table" || ' ' || table_key || ' ' || old_value || ' ' || new_value || ' ' || "user")) STORED,
    CONSTRAINT "PK_social_audit_log" PRIMARY KEY (id)
);

CREATE TABLE social_user_right (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    right_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    describe VARCHAR(150) NOT NULL,
    status VARCHAR(20) NOT NULL,
    CONSTRAINT "PK_social_user_right" PRIMARY KEY (id),
    CONSTRAINT "CK_Status_Valid_Value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE social_user_role (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    role_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    describe VARCHAR(150) NOT NULL,
    rights JSON NOT NULL DEFAULT '[]',
    status VARCHAR(20) NOT NULL,
    CONSTRAINT "PK_social_user_role" PRIMARY KEY (id),
    CONSTRAINT "CK_Status_Valid_Value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE UNIQUE INDEX "IX_admin_user_user_name_email" ON admin_user (user_name, email) WHERE status != 'Deleted';

CREATE UNIQUE INDEX "IX_admin_user_right_right_name" ON admin_user_right (right_name) WHERE status != 'Disabled';

CREATE UNIQUE INDEX "IX_admin_user_role_role_name" ON admin_user_role (role_name) WHERE status != 'Disabled';

CREATE UNIQUE INDEX "IX_base_config_config_key" ON base_config (config_key) WHERE status != 'Disabled';

CREATE INDEX "IX_admin_audit_log_search_vector" ON admin_audit_log USING GIN (search_vector);

CREATE INDEX "IX_social_audit_log_search_vector" ON social_audit_log USING GIN (search_vector);

CREATE UNIQUE INDEX "IX_social_user_right_right_name" ON social_user_right (right_name) WHERE status != 'Disabled';

CREATE UNIQUE INDEX "IX_social_user_role_role_name" ON social_user_role (role_name) WHERE status != 'Disabled';

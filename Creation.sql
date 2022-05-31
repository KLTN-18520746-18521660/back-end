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
    status VARCHAR(15) NOT NULL DEFAULT 'Activated',
    roles JSON NOT NULL DEFAULT '[]',
    settings JSON NOT NULL DEFAULT '{}',
    ranks JSON NOT NULL DEFAULT '{}',
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', display_name || ' ' || user_name)) STORED,
    last_access_timestamp TIMESTAMPTZ NULL,
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_social_user" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Deleted' OR status = 'Blocked' OR status = 'Activated')
);

CREATE TABLE social_post (
    id BIGINT GENERATED ALWAYS AS IDENTITY,
    owner UUID NOT NULL,
    title TEXT NOT NULL,
    slug TEXT NOT NULL,
    thumbnail TEXT NOT NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'Pending',
    content_search TEXT NOT NULL,
    content TEXT NOT NULL,
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', title || ' ' || content_search)) STORED,
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp TIMESTAMPTZ NULL DEFAULT NULL,
    CONSTRAINT "PK_social_post" PRIMARY KEY (id),
    CONSTRAINT "FK_social_post_user_id" FOREIGN KEY (owner) REFERENCES social_user(id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Deleted' OR status = 'Pending' OR status = 'Private' OR status = 'Approved')
);

CREATE TABLE social_category (
    id BIGINT GENERATED ALWAYS AS IDENTITY,
    parent_id BIGINT NULL DEFAULT NULL,
    name VARCHAR(20) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    describe VARCHAR(100) NOT NULL,
    slug TEXT NOT NULL,
    thumbnail TEXT,
    status VARCHAR(15) NOT NULL DEFAULT 'Enabled',
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', name || ' ' || display_name || ' ' || describe)) STORED,
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp TIMESTAMPTZ NULL DEFAULT NULL,
    CONSTRAINT "PK_social_category" PRIMARY KEY (id),
    CONSTRAINT "FK_social_category_parent" FOREIGN KEY (parent_id) REFERENCES social_category(id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE social_tag (
    id BIGINT GENERATED ALWAYS AS IDENTITY,
    tag VARCHAR(20) NOT NULL,
    describe VARCHAR(100) NOT NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'Enabled',
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp TIMESTAMPTZ NULL DEFAULT NULL,
    CONSTRAINT "PK_social_tag" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE social_post_tag (
    post_id BIGINT NOT NULL,
    tag_id BIGINT NOT NULL,
    CONSTRAINT "PK_social_post_tag" PRIMARY KEY (post_id, tag_id),
    CONSTRAINT "FK_social_post_tag_post" FOREIGN KEY (post_id) REFERENCES social_post(id),
    CONSTRAINT "FK_social_post_tag_tag" FOREIGN KEY (tag_id) REFERENCES social_tag(id)
);

CREATE TABLE social_post_category (
    post_id BIGINT NOT NULL,
    category_id BIGINT NOT NULL,
    CONSTRAINT "PK_social_post_category" PRIMARY KEY (post_id, category_id),
    CONSTRAINT "social_post_category_post" FOREIGN KEY (post_id) REFERENCES social_post(id),
    CONSTRAINT "social_post_category_category" FOREIGN KEY (category_id) REFERENCES social_category(id)
);

CREATE TABLE social_comment (
    id BIGINT GENERATED ALWAYS AS IDENTITY,
    parent_id BIGINT NULL DEFAULT NULL,
    post_id BIGINT NOT NULL,
    owner UUID NOT NULL,
    content TEXT NOT NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'Created',
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', content)) STORED,
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp TIMESTAMPTZ NULL DEFAULT NULL,
    CONSTRAINT "PK_social_comment" PRIMARY KEY (id),
    CONSTRAINT "FK_social_comment_user_id" FOREIGN KEY (owner) REFERENCES social_user(id),
    CONSTRAINT "FK_social_comment_post" FOREIGN KEY (post_id) REFERENCES social_post(id),
    CONSTRAINT "FK_social_comment_parent" FOREIGN KEY (parent_id) REFERENCES social_comment(id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Deleted' OR status = 'Created' OR status = 'Edited')
);

CREATE TABLE social_report (
    id BIGINT GENERATED ALWAYS AS IDENTITY,
    user_id UUID NOT NULL,
    post_id BIGINT NULL,
    comment_id BIGINT NULL,
    content TEXT NOT NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'Pending',
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', content)) STORED,
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp TIMESTAMPTZ NULL DEFAULT NULL,
    CONSTRAINT "PK_social_report" PRIMARY KEY (id),
    CONSTRAINT "FK_social_report_user_id" FOREIGN KEY (user_id) REFERENCES social_user(id),
	CONSTRAINT "FK_social_report_post" FOREIGN KEY (post_id) REFERENCES social_post(id),
    CONSTRAINT "FK_social_report_comment" FOREIGN KEY (comment_id) REFERENCES social_comment(id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Pending' OR status = 'Handled' OR status = 'Ignored')
);

-- follow, report, friend, block
CREATE TABLE social_user_action_with_user (
    user_id UUID NOT NULL,
    user_id_des UUID NOT NULL,
    actions JSON NOT NULL DEFAULT '[]',
    CONSTRAINT "PK_social_user_action_with_user" PRIMARY KEY (user_id, user_id_des),
    CONSTRAINT "FK_social_user_action_with_user_user_id" FOREIGN KEY (user_id) REFERENCES social_user(id),
    CONSTRAINT "FK_social_user_action_with_user_user_id_des" FOREIGN KEY (user_id_des) REFERENCES social_user(id),
    CONSTRAINT "CK_pair_user_id_valid_value" CHECK (user_id != user_id_des)
);

-- follow, comment, like, dislike, report
CREATE TABLE social_user_action_with_post (
    user_id UUID NOT NULL,
    post_id BIGINT NOT NULL,
    actions JSON NOT NULL DEFAULT '[]',
    CONSTRAINT "PK_social_user_action_with_post" PRIMARY KEY (user_id, post_id),
    CONSTRAINT "FK_social_user_action_with_post_user_id" FOREIGN KEY (user_id) REFERENCES social_user(id),
    CONSTRAINT "FK_social_user_action_with_post_post_id" FOREIGN KEY (post_id) REFERENCES social_post(id)
);

-- like, dislike, report, reply
CREATE TABLE social_user_action_with_comment (
    user_id UUID NOT NULL,
    comment_id BIGINT NOT NULL,
    actions JSON NOT NULL DEFAULT '[]',
    CONSTRAINT "PK_social_user_action_with_comment" PRIMARY KEY (user_id, comment_id),
    CONSTRAINT "FK_social_user_action_with_comment_user_id" FOREIGN KEY (user_id) REFERENCES social_user(id),
    CONSTRAINT "FK_social_user_action_with_comment_comment_id" FOREIGN KEY (comment_id) REFERENCES social_comment(id)
);

-- follow
CREATE TABLE social_user_action_with_tag (
    user_id UUID NOT NULL,
    tag_id BIGINT NOT NULL,
    actions JSON NOT NULL DEFAULT '[]',
    CONSTRAINT "PK_social_user_action_with_tag" PRIMARY KEY (user_id, tag_id),
    CONSTRAINT "FK_social_user_action_with_tag_user_id" FOREIGN KEY (user_id) REFERENCES social_user(id),
    CONSTRAINT "FK_social_user_action_with_tag_tag_id" FOREIGN KEY (tag_id) REFERENCES social_tag(id)
);

-- follow
CREATE TABLE social_user_action_with_category (
    user_id UUID NOT NULL,
    category_id BIGINT NOT NULL,
    actions JSON NOT NULL DEFAULT '[]',
    CONSTRAINT "PK_social_user_action_with_category" PRIMARY KEY (user_id, category_id),
    CONSTRAINT "FK_social_user_action_with_category_user_id" FOREIGN KEY (user_id) REFERENCES social_user(id),
    CONSTRAINT "FK_social_user_action_with_category_category_id" FOREIGN KEY (category_id) REFERENCES social_category(id)
);

CREATE TABLE social_notification (
    id BIGINT GENERATED ALWAYS AS IDENTITY,
    user_id UUID NOT NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'Sent',
    content JSON NOT NULL DEFAULT '{}',
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp TIMESTAMPTZ NULL DEFAULT NULL,
    CONSTRAINT "PK_social_notification" PRIMARY KEY (id),
    CONSTRAINT "FK_social_notification_user_id" FOREIGN KEY (user_id) REFERENCES social_user(id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Sent' OR status = 'Read' OR status = 'Deleted')
);

CREATE UNIQUE INDEX "IX_social_post_slug" ON social_post(slug) WHERE status != 'Deleted';
CREATE UNIQUE INDEX "IX_social_category_slug" ON social_category(slug) WHERE status != 'Disabled';
CREATE UNIQUE INDEX "IX_social_tag_tag" ON social_tag(tag) WHERE status != 'Disabled';
CREATE UNIQUE INDEX "IX_social_user_user_name_email" ON social_user (user_name, email) WHERE status != 'Deleted';
CREATE INDEX "IX_social_user_search_vector" ON social_user USING GIST (search_vector);
CREATE INDEX "IX_social_post_search_vector" ON social_post USING GIN (search_vector);
CREATE INDEX "IX_social_category_search_vector" ON social_category USING GIN (search_vector);
CREATE INDEX "IX_social_comment_search_vector" ON social_comment USING GIN (search_vector);
CREATE INDEX "IX_social_report_search_vector" ON social_report USING GIN (search_vector);

/********** CONFIG **********/
CREATE TABLE admin_user (
    id UUID NOT NULL DEFAULT (gen_random_uuid()),
    user_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    password VARCHAR(32) NOT NULL,
    salt VARCHAR(8) NOT NULL DEFAULT (SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)),
    email VARCHAR(320) NOT NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'Activated',
    roles JSON NOT NULL DEFAULT '[]',
    settings JSON NOT NULL DEFAULT '{}',
    last_access_timestamp TIMESTAMPTZ NULL,
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_admin_user" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Deleted' OR status = 'Blocked' OR status = 'Activated' OR status = 'Readonly')
);

CREATE TABLE admin_user_right (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    right_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    describe VARCHAR(150) NOT NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'Enabled',
    CONSTRAINT "PK_admin_user_right" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE admin_user_role (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    role_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    describe VARCHAR(150) NOT NULL,
    rights JSON NOT NULL DEFAULT '[]',
    status VARCHAR(15) NOT NULL DEFAULT 'Enabled',
    CONSTRAINT "PK_admin_user_role" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE admin_base_config (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    config_key VARCHAR(50) NOT NULL,
    value JSON NOT NULL DEFAULT '{}',
    status VARCHAR(15) NOT NULL DEFAULT 'Enabled',
    CONSTRAINT "PK_admin_base_config" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
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
    status VARCHAR(15) NOT NULL DEFAULT 'Enabled',
    CONSTRAINT "PK_social_user_right" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE social_user_role (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    role_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    describe VARCHAR(150) NOT NULL,
    rights JSON NOT NULL DEFAULT '[]',
    status VARCHAR(15) NOT NULL DEFAULT 'Enabled',
    CONSTRAINT "PK_social_user_role" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE UNIQUE INDEX "IX_admin_user_user_name_email" ON admin_user (user_name, email) WHERE status != 'Deleted';
CREATE UNIQUE INDEX "IX_admin_user_right_right_name" ON admin_user_right (right_name) WHERE status != 'Disabled';
CREATE UNIQUE INDEX "IX_admin_user_role_role_name" ON admin_user_role (role_name) WHERE status != 'Disabled';
CREATE UNIQUE INDEX "IX_admin_base_config_config_key" ON admin_base_config (config_key) WHERE status != 'Disabled';
CREATE INDEX "IX_admin_audit_log_search_vector" ON admin_audit_log USING GIN (search_vector);
CREATE INDEX "IX_social_audit_log_search_vector" ON social_audit_log USING GIN (search_vector);
CREATE UNIQUE INDEX "IX_social_user_right_right_name" ON social_user_right (right_name) WHERE status != 'Disabled';
CREATE UNIQUE INDEX "IX_social_user_role_role_name" ON social_user_role (role_name) WHERE status != 'Disabled';

/********** CACHED **********/
CREATE TABLE session_social_user (
    session_token VARCHAR(30) NOT NULL,
    user_id UUID NOT NULL,
    saved BOOLEAN NOT NULL DEFAULT 'false',
    data JSON NOT NULL DEFAULT '{}',
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_interaction_time TIMESTAMPTZ,
    CONSTRAINT "PK_session_social_user" PRIMARY KEY (session_token),
    CONSTRAINT "FK_session_social_user_user_id" FOREIGN KEY (user_id) REFERENCES social_user(id)
);

CREATE TABLE session_admin_user (
    session_token VARCHAR(30) NOT NULL,
    user_id UUID NOT NULL,
    saved BOOLEAN NOT NULL DEFAULT 'false',
    data JSON NOT NULL DEFAULT '{}',
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_interaction_time TIMESTAMPTZ,
    CONSTRAINT "PK_session_admin_user" PRIMARY KEY (session_token),
    CONSTRAINT "FK_session_admin_user_user_id" FOREIGN KEY (user_id) REFERENCES admin_user(id)
);

---------------------------------------------
START TRANSACTION;

CREATE TABLE admin_base_config (
    id integer GENERATED ALWAYS AS IDENTITY,  
    config_key character varying(50) NOT NULL,
    value JSON NULL DEFAULT ('{}'),
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    CONSTRAINT "PK_admin_base_config" PRIMARY KEY (id),
    CONSTRAINT "CK_admin_base_config_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE admin_user (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    user_name character varying(50) NOT NULL,
    display_name character varying(50) NOT NULL,
    password character varying(32) NOT NULL,
    salt character varying(8) NOT NULL DEFAULT (SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)),
    email character varying(320) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Activated'),
    settings jsonb NOT NULL DEFAULT ('{}'),
    last_access_timestamp timestamp with time zone NULL,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_admin_user" PRIMARY KEY (id),
    CONSTRAINT "CK_admin_user_status_valid_value" CHECK (status = 'Activated' OR status = 'Blocked' OR status = 'Deleted' OR status = 'Readonly'),
    CONSTRAINT "CK_admin_user_last_access_timestamp_valid_value" CHECK ((last_access_timestamp IS NULL) OR (last_access_timestamp > created_timestamp))
);

CREATE TABLE admin_user_right (
    id integer GENERATED ALWAYS AS IDENTITY,
    right_name character varying(50) NOT NULL,
    display_name character varying(50) NOT NULL,
    describe character varying(150) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    CONSTRAINT "PK_admin_user_right" PRIMARY KEY (id),
    CONSTRAINT "CK_admin_user_right_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly')
);

CREATE TABLE admin_user_role (
    id integer GENERATED ALWAYS AS IDENTITY,
    role_name character varying(50) NOT NULL,
    display_name character varying(50) NOT NULL,
    describe character varying(150) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    CONSTRAINT "PK_admin_user_role" PRIMARY KEY (id),
    CONSTRAINT "CK_admin_user_role_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly')
);

CREATE TABLE social_category (
    id bigint GENERATED ALWAYS AS IDENTITY,
    parent_id bigint NULL,
    name character varying(20) NOT NULL,
    display_name character varying(50) NOT NULL,
    describe character varying(100) NOT NULL,
    slug text NOT NULL,
    thumbnail text NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', name || ' ' || display_name || ' ' || describe)) STORED,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_category" PRIMARY KEY (id),
    CONSTRAINT "CK_social_category_last_modified_timestamp_valid_value" CHECK ((last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)),
    CONSTRAINT "CK_social_category_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly'),
    CONSTRAINT "FK_social_category_parent" FOREIGN KEY (parent_id) REFERENCES social_category (id) ON DELETE RESTRICT
);

CREATE TABLE social_tag (
    id bigint GENERATED ALWAYS AS IDENTITY,
    tag character varying(25) NOT NULL,
    describe character varying(100) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_tag" PRIMARY KEY (id),
    CONSTRAINT "CK_social_tag_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly')
);

CREATE TABLE social_user (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    first_name character varying(25) NOT NULL,
    last_name character varying(25) NOT NULL,
    display_name character varying(50) NOT NULL,
    user_name character varying(50) NOT NULL,
    password character varying(32) NOT NULL,
    salt character varying(8) NOT NULL DEFAULT (SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)),
    email character varying(320) NOT NULL,
    sex character varying(10) NULL,
    phone character varying(20) NULL,
    country character varying(20) NULL,
    city character varying(20) NULL,
    province character varying(20) NULL,
    verified_email boolean NOT NULL,
    avatar text NULL,
    status character varying(15) NOT NULL DEFAULT ('Activated'),
    settings jsonb NOT NULL DEFAULT ('{}'),
    ranks jsonb NOT NULL DEFAULT ('{}'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', display_name || ' ' || user_name)) STORED,
    last_access_timestamp timestamp with time zone NULL,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_social_user" PRIMARY KEY (id),
    CONSTRAINT "CK_social_report_status_valid_value" CHECK (status = 'Activated' OR status = 'Deleted' OR status = 'Blocked'),
    CONSTRAINT "CK_social_user_last_access_timestamp_valid_value" CHECK ((last_access_timestamp IS NULL) OR (last_access_timestamp > created_timestamp))        
);

CREATE TABLE social_user_right (
    id integer GENERATED ALWAYS AS IDENTITY,
    right_name character varying(50) NOT NULL,
    display_name character varying(50) NOT NULL,
    describe character varying(150) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    CONSTRAINT "PK_social_user_right" PRIMARY KEY (id),
    CONSTRAINT "CK_social_user_right_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly')
);

CREATE TABLE social_user_role (
    id integer GENERATED ALWAYS AS IDENTITY,
    role_name character varying(50) NOT NULL,
    display_name character varying(50) NOT NULL,
    describe character varying(150) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    CONSTRAINT "PK_social_user_role" PRIMARY KEY (id),
    CONSTRAINT "CK_social_user_role_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly')
);

CREATE TABLE admin_audit_log (
    id integer GENERATED ALWAYS AS IDENTITY,
    "table" character varying(50) NOT NULL,
    table_key character varying(100) NOT NULL,
    action character varying(50) NOT NULL,
    old_value TEXT NOT NULL,
    new_value TEXT NOT NULL,
    user_id uuid NOT NULL,
    timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', "table" || ' ' || table_key || ' ' || old_value || ' ' || new_value)) STORED,
    CONSTRAINT "PK_admin_audit_log" PRIMARY KEY (id),
    CONSTRAINT "FK_admin_audit_log_user_id" FOREIGN KEY (user_id) REFERENCES admin_user (id) ON DELETE RESTRICT
);

CREATE TABLE session_admin_user (
    session_token character varying(30) NOT NULL,
    user_id uuid NOT NULL,
    saved boolean NOT NULL,
    data jsonb NOT NULL DEFAULT ('{}'),
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_interaction_time timestamp with time zone NOT NULL,
    CONSTRAINT "PK_session_admin_user" PRIMARY KEY (session_token),
    CONSTRAINT "CK_session_admin_user_last_interaction_time_valid_value" CHECK ((last_interaction_time >= created_timestamp)),
    CONSTRAINT "FK_session_admin_user_user_id" FOREIGN KEY (user_id) REFERENCES admin_user (id) ON DELETE RESTRICT
);

CREATE TABLE admin_user_role_detail (
    role_id integer NOT NULL,
    right_id integer NOT NULL,
    actions jsonb NOT NULL DEFAULT ('{
  "read": false,
  "write": false
}'),
    CONSTRAINT "PK_admin_user_role_detail" PRIMARY KEY (role_id, right_id),
    CONSTRAINT "FK_admin_user_role_detail_right" FOREIGN KEY (right_id) REFERENCES admin_user_right (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_admin_user_role_detail_role" FOREIGN KEY (role_id) REFERENCES admin_user_role (id) ON DELETE RESTRICT
);

CREATE TABLE admin_user_role_of_user (
    user_id uuid NOT NULL,
    role_id integer NOT NULL,
    CONSTRAINT "PK_admin_user_role_of_user" PRIMARY KEY (user_id, role_id),
    CONSTRAINT "FK_admin_user_role_of_user_role" FOREIGN KEY (role_id) REFERENCES admin_user_role (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_admin_user_role_of_user_user" FOREIGN KEY (user_id) REFERENCES admin_user (id) ON DELETE RESTRICT
);

CREATE TABLE session_social_user (
    session_token character varying(30) NOT NULL,
    user_id uuid NOT NULL,
    saved boolean NOT NULL,
    data jsonb NOT NULL DEFAULT ('{}'),
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_interaction_time timestamp with time zone NOT NULL,
    CONSTRAINT "PK_session_social_user" PRIMARY KEY (session_token),
    CONSTRAINT "CK_session_social_user_last_interaction_time_valid_value" CHECK ((last_interaction_time >= created_timestamp)),
    CONSTRAINT "FK_session_social_user_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_audit_log (
    id integer GENERATED ALWAYS AS IDENTITY,
    "table" character varying(50) NOT NULL,
    table_key character varying(100) NOT NULL,
    action character varying(50) NOT NULL,
    old_value TEXT NOT NULL,
    new_value TEXT NOT NULL,
    user_id uuid NOT NULL,
    timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', "table" || ' ' || table_key || ' ' || old_value || ' ' || new_value)) STORED,
    CONSTRAINT "PK_social_audit_log" PRIMARY KEY (id),
    CONSTRAINT "FK_social_audit_log_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_notification (
    id bigint GENERATED ALWAYS AS IDENTITY,
    user_id uuid NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Sent'),
    content jsonb NOT NULL DEFAULT ('{}'),
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_notification" PRIMARY KEY (id),
    CONSTRAINT "CK_social_notification_status_valid_value" CHECK (status = 'Sent' OR status = 'Read' OR status = 'Deleted'),
    CONSTRAINT "CK_social_notification_last_modified_timestamp_valid_value" CHECK ((last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)),
    CONSTRAINT "FK_social_notification_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_post (
    id bigint GENERATED ALWAYS AS IDENTITY,
    owner uuid NOT NULL,
    title text NOT NULL,
    slug text NOT NULL,
    thumbnail text NOT NULL,
    views integer NOT NULL DEFAULT (0),
    time_read integer NOT NULL DEFAULT (2),
    status character varying(15) NOT NULL DEFAULT ('Pending'),
    content_search text NOT NULL,
    content text NOT NULL,
    short_content text NOT NULL,
    content_type character varying(15) NOT NULL,
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', content_search || ' ' || title)) STORED,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_post" PRIMARY KEY (id),
    CONSTRAINT "CK_social_post_content_type_valid_value" CHECK (content_type = 'HTML' OR content_type = 'MARKDOWN'),
    CONSTRAINT "CK_social_post_status_valid_value" CHECK (status = 'Pending' OR status = 'Approved' OR status = 'Private' OR status = 'Deleted'),
    CONSTRAINT "CK_social_post_time_read_valid_value" CHECK (time_read >= 2),
    CONSTRAINT "CK_social_post_last_modified_timestamp_valid_value" CHECK ((last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)), 
    CONSTRAINT "FK_social_post_user_id" FOREIGN KEY (owner) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_action_with_category (
    user_id uuid NOT NULL,
    category_id bigint NOT NULL,
    actions jsonb NOT NULL DEFAULT ('[]'),
    CONSTRAINT "PK_social_user_action_with_category" PRIMARY KEY (user_id, category_id),
    CONSTRAINT "FK_social_user_action_with_category_category_id" FOREIGN KEY (category_id) REFERENCES social_category (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_action_with_category_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_action_with_tag (
    user_id uuid NOT NULL,
    tag_id bigint NOT NULL,
    actions jsonb NOT NULL DEFAULT ('[]'),
    CONSTRAINT "PK_social_user_action_with_tag" PRIMARY KEY (user_id, tag_id),
    CONSTRAINT "FK_social_user_action_with_tag_tag_id" FOREIGN KEY (tag_id) REFERENCES social_tag (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_action_with_tag_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_action_with_user (
    user_id uuid NOT NULL,
    user_id_des uuid NOT NULL,
    actions jsonb NOT NULL DEFAULT ('[]'),
    CONSTRAINT "PK_social_user_action_with_user" PRIMARY KEY (user_id, user_id_des),
    CONSTRAINT "FK_social_user_action_with_user_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_action_with_user_user_id_des" FOREIGN KEY (user_id_des) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_role_detail (
    role_id integer NOT NULL,
    right_id integer NOT NULL,
    actions jsonb NOT NULL DEFAULT ('{
  "read": false,
  "write": false
}'),
    CONSTRAINT "PK_social_user_role_detail" PRIMARY KEY (role_id, right_id),
    CONSTRAINT "FK_social_user_role_detail_right" FOREIGN KEY (right_id) REFERENCES social_user_right (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_role_detail_role" FOREIGN KEY (role_id) REFERENCES social_user_role (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_role_of_user (
    user_id uuid NOT NULL,
    role_id integer NOT NULL,
    CONSTRAINT "PK_social_user_role_of_user" PRIMARY KEY (user_id, role_id),
    CONSTRAINT "FK_social_user_role_of_user_role" FOREIGN KEY (role_id) REFERENCES social_user_role (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_role_of_user_user" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_comment (
    id bigint GENERATED ALWAYS AS IDENTITY,
    parent_id bigint NULL,
    post_id bigint NOT NULL,
    owner uuid NOT NULL,
    content text NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Created'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', content)) STORED,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_comment" PRIMARY KEY (id),
    CONSTRAINT "CK_social_comment_status_valid_value" CHECK (status = 'Created' OR status = 'Edited' OR status = 'Deleted'),
    CONSTRAINT "CK_social_comment_last_modified_timestamp_valid_value" CHECK ((last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)),
    CONSTRAINT "FK_social_comment_parent" FOREIGN KEY (parent_id) REFERENCES social_comment (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_comment_post" FOREIGN KEY (post_id) REFERENCES social_post (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_comment_user_id" FOREIGN KEY (owner) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_post_category (
    post_id bigint NOT NULL,
    category_id bigint NOT NULL,
    CONSTRAINT "PK_social_post_category" PRIMARY KEY (post_id, category_id),
    CONSTRAINT "FK_social_post_category_category" FOREIGN KEY (category_id) REFERENCES social_category (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_post_category_post" FOREIGN KEY (post_id) REFERENCES social_post (id) ON DELETE RESTRICT
);

CREATE TABLE social_post_tag (
    post_id bigint NOT NULL,
    tag_id bigint NOT NULL,
    CONSTRAINT "PK_social_post_tag" PRIMARY KEY (post_id, tag_id),
    CONSTRAINT "FK_social_post_tag_post" FOREIGN KEY (post_id) REFERENCES social_post (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_post_tag_tag" FOREIGN KEY (tag_id) REFERENCES social_tag (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_action_with_post (
    user_id uuid NOT NULL,
    post_id bigint NOT NULL,
    actions jsonb NOT NULL DEFAULT ('[]'),
    CONSTRAINT "PK_social_user_action_with_post" PRIMARY KEY (user_id, post_id),
    CONSTRAINT "FK_social_user_action_with_post_post_id" FOREIGN KEY (post_id) REFERENCES social_post (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_action_with_post_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_report (
    id bigint GENERATED ALWAYS AS IDENTITY,
    user_id uuid NOT NULL,
    post_id bigint NULL,
    comment_id bigint NULL,
    content text NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Pending'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', content)) STORED,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_report" PRIMARY KEY (id),
    CONSTRAINT "CK_social_report_status_valid_value" CHECK (status = 'Pending' OR status = 'Ignored' OR status = 'Handled'),
    CONSTRAINT "FK_social_report_comment" FOREIGN KEY (comment_id) REFERENCES social_comment (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_report_post" FOREIGN KEY (post_id) REFERENCES social_post (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_report_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_action_with_comment (
    user_id uuid NOT NULL,
    comment_id bigint NOT NULL,
    actions jsonb NOT NULL DEFAULT ('[]'),
    CONSTRAINT "PK_social_user_action_with_comment" PRIMARY KEY (user_id, comment_id),
    CONSTRAINT "FK_social_user_action_with_comment_comment_id" FOREIGN KEY (comment_id) REFERENCES social_comment (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_action_with_comment_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (1, 'AdminUserLoginConfig', 'Enabled', '{
  "number_of_times_allow_login_failure": 5,
  "lock_time": 360
}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (2, 'SocialUserLoginConfig', 'Enabled', '{
  "number_of_times_allow_login_failure": 5,
  "lock_time": 360
}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (3, 'SessionAdminUserConfig', 'Enabled', '{
  "expiry_time": 5,
  "extension_time": 5
}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (4, 'SessionSocialUserConfig', 'Enabled', '{
  "expiry_time": 5,
  "extension_time": 5
}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (5, 'EmailClientConfig', 'Enabled', '{
  "limit_sender": 5,
  "template_user_signup": "<p>Dear @Model.UserName,</p>\r\n                                        <p>Confirm link here: <a href=''@UserName.ConfirmLink''>@Model.ConfirmLink</a><br>\r\n                                        Send datetime: @Model.DateTimeSend</p>\r\n                                        <p>Thanks for your register.</p>"
}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (6, 'SocialUserConfirmConfig', 'Enabled', '{
  "expiry_time": 2880,
  "number_of_times_allow_confirm_failure": 3
}');

INSERT INTO admin_user (id, created_timestamp, display_name, email, last_access_timestamp, salt, settings, status, password, user_name)
VALUES ('7de2a755-f800-4c71-b63d-ae4e9a1c8c99', TIMESTAMPTZ '2022-03-30 16:45:05.999724 UTC', 'Administrator', 'admin@admin', NULL, '712bcd55', '{}', 'Readonly', 'AC53AF2C41772CAC9642B4FB8DECBA5C', 'admin');

INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (11, 'Modify, get config of server.', 'Config', 'config', 'Readonly');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (9, 'Add, block, unblock, delete AdminUser.', 'Admin User', 'admin_user', 'Readonly');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (8, 'Block, unblock SocialUser', 'Social User', 'social_user', 'Readonly');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (7, 'Configure security of Server.', 'Security', 'security', 'Readonly');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (6, 'Delete comment. See report about comment.', 'Comment', 'comment', 'Readonly');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (10, 'See and tracking log file.', 'Log', 'log', 'Readonly');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (4, 'Add, create, disable tag.', 'Tag', 'tag', 'Readonly');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (3, 'Add, create, disable topics', 'Topic', 'topic', 'Readonly');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (2, 'Add, create, disable category.', 'Category', 'category', 'Readonly');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (1, 'Can access Homepage and see statistic.', 'Dashboard', 'dashboard', 'Readonly');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (5, 'Review, accept, reject post. See report about post.', 'Post', 'post', 'Readonly');

INSERT INTO admin_user_role (id, describe, display_name, role_name, status)
OVERRIDING SYSTEM VALUE
VALUES (1, 'Administrator', 'Administrator', 'admin', 'Readonly');

INSERT INTO social_category (id, created_timestamp, describe, display_name, last_modified_timestamp, name, parent_id, slug, status, thumbnail)
OVERRIDING SYSTEM VALUE
VALUES (5, TIMESTAMPTZ '2022-03-30 16:45:06.027515 UTC', 'Life die have number', 'Left', NULL, 'left', NULL, 'left', 'Readonly', NULL);
INSERT INTO social_category (id, created_timestamp, describe, display_name, last_modified_timestamp, name, parent_id, slug, status, thumbnail)
OVERRIDING SYSTEM VALUE
VALUES (3, TIMESTAMPTZ '2022-03-30 16:45:06.027514 UTC', 'Search google to have better solution', 'Dicussion', NULL, 'dicussion', NULL, 'dicussion', 'Readonly', NULL);
INSERT INTO social_category (id, created_timestamp, describe, display_name, last_modified_timestamp, name, parent_id, slug, status, thumbnail)
OVERRIDING SYSTEM VALUE
VALUES (4, TIMESTAMPTZ '2022-03-30 16:45:06.027514 UTC', 'Nothing in here', 'Blog', NULL, 'blog', NULL, 'blog', 'Readonly', NULL);
INSERT INTO social_category (id, created_timestamp, describe, display_name, last_modified_timestamp, name, parent_id, slug, status, thumbnail)
OVERRIDING SYSTEM VALUE
VALUES (1, TIMESTAMPTZ '2022-03-30 16:45:06.027504 UTC', 'This not a bug this a feature', 'Technology', NULL, 'technology', NULL, 'technology', 'Readonly', NULL);
INSERT INTO social_category (id, created_timestamp, describe, display_name, last_modified_timestamp, name, parent_id, slug, status, thumbnail)
OVERRIDING SYSTEM VALUE
VALUES (2, TIMESTAMPTZ '2022-03-30 16:45:06.027512 UTC', 'Do not click to this', 'Developer', NULL, 'developer', NULL, 'developer', 'Readonly', NULL);

INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (1, TIMESTAMPTZ '2022-03-30 16:45:06.047753 UTC', 'Angular', NULL, 'Readonly', '#angular');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (2, TIMESTAMPTZ '2022-03-30 16:45:06.047757 UTC', 'Something is not thing', NULL, 'Readonly', '#life-die-have-number');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (3, TIMESTAMPTZ '2022-03-30 16:45:06.047758 UTC', 'Dot not choose this tag', NULL, 'Readonly', '#develop');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (4, TIMESTAMPTZ '2022-03-30 16:45:06.047758 UTC', 'Nothing in here', NULL, 'Readonly', '#nothing');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (5, TIMESTAMPTZ '2022-03-30 16:45:06.047759 UTC', 'hi hi', NULL, 'Readonly', '#hihi');

INSERT INTO social_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (3, 'Can create, interactive report.', 'Report', 'report', 'Readonly');
INSERT INTO social_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (1, 'Can create, interactive posts.', 'Post', 'post', 'Readonly');
INSERT INTO social_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (2, 'Can create, interactive comment.', 'Comment', 'comment', 'Readonly');

INSERT INTO social_user_role (id, describe, display_name, role_name, status)
OVERRIDING SYSTEM VALUE
VALUES (1, 'Normal user', 'User', 'user', 'Readonly');

INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (1, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (2, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (3, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (4, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (5, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (6, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (7, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (8, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (9, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (10, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (11, 1, '{
  "read": true,
  "write": true
}');

INSERT INTO admin_user_role_of_user (role_id, user_id)
VALUES (1, '7de2a755-f800-4c71-b63d-ae4e9a1c8c99');

INSERT INTO social_user_role_detail (right_id, role_id, actions)
VALUES (1, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO social_user_role_detail (right_id, role_id, actions)
VALUES (2, 1, '{
  "read": true,
  "write": true
}');
INSERT INTO social_user_role_detail (right_id, role_id, actions)
VALUES (3, 1, '{
  "read": true,
  "write": true
}');

CREATE INDEX "IX_admin_audit_log_search_vector" ON admin_audit_log USING GIN (search_vector);

CREATE INDEX "IX_admin_audit_log_user_id" ON admin_audit_log (user_id);

CREATE UNIQUE INDEX "IX_admin_base_config_config_key" ON admin_base_config (config_key) WHERE (status) <> 'Disabled';

CREATE UNIQUE INDEX "IX_admin_user_user_name_email" ON admin_user (user_name, email) WHERE (status) <> 'Deleted';

CREATE UNIQUE INDEX "IX_admin_user_right_right_name" ON admin_user_right (right_name) WHERE (status) <> 'Disabled';

CREATE UNIQUE INDEX "IX_admin_user_role_role_name" ON admin_user_role (role_name) WHERE (status) <> 'Disabled';

CREATE INDEX "IX_admin_user_role_detail_right_id" ON admin_user_role_detail (right_id);

CREATE INDEX "IX_admin_user_role_of_user_role_id" ON admin_user_role_of_user (role_id);

CREATE UNIQUE INDEX "IX_session_admin_user_token_user_id" ON session_admin_user (session_token, user_id);

CREATE INDEX "IX_session_admin_user_user_id" ON session_admin_user (user_id);

CREATE UNIQUE INDEX "IX_session_social_user_token_user_id" ON session_social_user (session_token, user_id);

CREATE INDEX "IX_session_social_user_user_id" ON session_social_user (user_id);

CREATE INDEX "IX_social_audit_log_search_vector" ON social_audit_log USING GIN (search_vector);

CREATE INDEX "IX_social_audit_log_user_id" ON social_audit_log (user_id);

CREATE INDEX "IX_social_category_parent_id" ON social_category (parent_id);

CREATE INDEX "IX_social_category_search_vector" ON social_category USING GIN (search_vector);

CREATE UNIQUE INDEX "IX_social_category_slug" ON social_category (slug) WHERE (status) <> 'Disabled';

CREATE INDEX "IX_social_comment_owner" ON social_comment (owner);

CREATE INDEX "IX_social_comment_parent_id" ON social_comment (parent_id);

CREATE INDEX "IX_social_comment_post_id" ON social_comment (post_id);

CREATE INDEX "IX_social_comment_search_vector" ON social_comment USING GIST (search_vector);

CREATE INDEX "IX_social_notification_user_id" ON social_notification (user_id);

CREATE INDEX "IX_social_post_owner" ON social_post (owner);

CREATE INDEX "IX_social_post_search_vector" ON social_post USING GIST (search_vector);

CREATE UNIQUE INDEX "IX_social_post_slug" ON social_post (slug) WHERE (status) <> 'Deleted';

CREATE INDEX "IX_social_post_category_category_id" ON social_post_category (category_id);

CREATE INDEX "IX_social_post_tag_tag_id" ON social_post_tag (tag_id);

CREATE INDEX "IX_social_report_comment_id" ON social_report (comment_id);

CREATE INDEX "IX_social_report_post_id" ON social_report (post_id);

CREATE INDEX "IX_social_report_search_vector" ON social_report USING GIN (search_vector);

CREATE INDEX "IX_social_report_user_id" ON social_report (user_id);

CREATE UNIQUE INDEX "IX_social_tag_tag" ON social_tag (tag) WHERE (status) <> 'Disabled';

CREATE INDEX "IX_social_user_search_vector" ON social_user USING GIST (search_vector);

CREATE UNIQUE INDEX "IX_social_user_user_name_email" ON social_user (user_name, email) WHERE (status) <> 'Deleted';

CREATE INDEX "IX_social_user_action_with_category_category_id" ON social_user_action_with_category (category_id);

CREATE INDEX "IX_social_user_action_with_comment_comment_id" ON social_user_action_with_comment (comment_id);

CREATE INDEX "IX_social_user_action_with_post_post_id" ON social_user_action_with_post (post_id);

CREATE INDEX "IX_social_user_action_with_tag_tag_id" ON social_user_action_with_tag (tag_id);

CREATE INDEX "IX_social_user_action_with_user_user_id_des" ON social_user_action_with_user (user_id_des);

CREATE UNIQUE INDEX "IX_social_user_right_right_name" ON social_user_right (right_name) WHERE (status) <> 'Disabled';

CREATE UNIQUE INDEX "IX_social_user_role_role_name" ON social_user_role (role_name) WHERE (status) <> 'Disabled';

CREATE INDEX "IX_social_user_role_detail_right_id" ON social_user_role_detail (right_id);

CREATE INDEX "IX_social_user_role_of_user_role_id" ON social_user_role_of_user (role_id);

SELECT setval(
    pg_get_serial_sequence('admin_base_config', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM admin_base_config) + 1,
        nextval(pg_get_serial_sequence('admin_base_config', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('admin_user_right', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM admin_user_right) + 1,
        nextval(pg_get_serial_sequence('admin_user_right', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('admin_user_role', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM admin_user_role) + 1,
        nextval(pg_get_serial_sequence('admin_user_role', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('social_category', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM social_category) + 1,
        nextval(pg_get_serial_sequence('social_category', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('social_tag', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM social_tag) + 1,
        nextval(pg_get_serial_sequence('social_tag', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('social_user_right', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM social_user_right) + 1,
        nextval(pg_get_serial_sequence('social_user_right', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('social_user_role', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM social_user_role) + 1,
        nextval(pg_get_serial_sequence('social_user_role', 'id'))),
    false);

COMMIT;
-------------------------
-------------------------
-------------------------
-------------------------
-------------------------
-------------------------
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE admin_base_config (
    id integer GENERATED ALWAYS AS IDENTITY,
    config_key character varying(50) NOT NULL,
    value JSON NULL DEFAULT ('{}'),
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    CONSTRAINT "PK_admin_base_config" PRIMARY KEY (id),
    CONSTRAINT "CK_admin_base_config_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly')
);

CREATE TABLE admin_user (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    user_name character varying(50) NOT NULL,
    display_name character varying(50) NOT NULL,
    password character varying(32) NOT NULL,
    salt character varying(8) NOT NULL DEFAULT (SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)),
    email character varying(320) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Activated'),
    settings jsonb NOT NULL DEFAULT ('{}'),
    last_access_timestamp timestamp with time zone NULL,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_admin_user" PRIMARY KEY (id),
    CONSTRAINT "CK_admin_user_status_valid_value" CHECK (status = 'Activated' OR status = 'Blocked' OR status = 'Deleted' OR status = 'Readonly'),
    CONSTRAINT "CK_admin_user_last_access_timestamp_valid_value" CHECK ((last_access_timestamp IS NULL) OR (last_access_timestamp > created_timestamp))
);

CREATE TABLE admin_user_right (
    id integer GENERATED ALWAYS AS IDENTITY,
    right_name character varying(50) NOT NULL,
    display_name character varying(50) NOT NULL,
    describe character varying(150) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    CONSTRAINT "PK_admin_user_right" PRIMARY KEY (id),
    CONSTRAINT "CK_admin_user_right_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly')
);

CREATE TABLE admin_user_role (
    id integer GENERATED ALWAYS AS IDENTITY,
    role_name character varying(50) NOT NULL,
    display_name character varying(50) NOT NULL,
    describe character varying(150) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    priority boolean NOT NULL,
    CONSTRAINT "PK_admin_user_role" PRIMARY KEY (id),
    CONSTRAINT "CK_admin_user_role_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly')
);

CREATE TABLE social_category (
    id bigint GENERATED ALWAYS AS IDENTITY,
    parent_id bigint NULL,
    name character varying(20) NOT NULL,
    display_name character varying(50) NOT NULL,
    describe character varying(300) NOT NULL,
    slug text NOT NULL,
    thumbnail text NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', name || ' ' || display_name || ' ' || describe)) STORED,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_category" PRIMARY KEY (id),
    CONSTRAINT "CK_social_category_last_modified_timestamp_valid_value" CHECK ((last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)),
    CONSTRAINT "CK_social_category_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly'),
    CONSTRAINT "FK_social_category_parent" FOREIGN KEY (parent_id) REFERENCES social_category (id) ON DELETE RESTRICT
);

CREATE TABLE social_tag (
    id bigint GENERATED ALWAYS AS IDENTITY,
    tag character varying(25) NOT NULL,
    name character varying(50) NOT NULL,
    describe character varying(300) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_tag" PRIMARY KEY (id),
    CONSTRAINT "CK_social_tag_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly')
);

CREATE TABLE social_user (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    first_name character varying(25) NOT NULL,
    last_name character varying(25) NOT NULL,
    display_name character varying(50) NOT NULL,
    user_name character varying(50) NOT NULL,
    password character varying(32) NOT NULL,
    salt character varying(8) NOT NULL DEFAULT (SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)),
    email character varying(320) NOT NULL,
    description character varying(2048) NULL,
    sex character varying(10) NULL,
    phone character varying(20) NULL,
    country character varying(20) NULL,
    city character varying(20) NULL,
    province character varying(20) NULL,
    verified_email boolean NOT NULL,
    avatar text NULL,
    status character varying(15) NOT NULL DEFAULT ('Activated'),
    settings jsonb NOT NULL DEFAULT ('{}'),
    ranks jsonb NOT NULL DEFAULT ('{}'),
    publics jsonb NOT NULL DEFAULT ('[]'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', display_name || ' ' || user_name)) STORED,
    last_access_timestamp timestamp with time zone NULL,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_social_user" PRIMARY KEY (id),
    CONSTRAINT "CK_social_report_status_valid_value" CHECK (status = 'Activated' OR status = 'Deleted' OR status = 'Blocked'),
    CONSTRAINT "CK_social_user_last_access_timestamp_valid_value" CHECK ((last_access_timestamp IS NULL) OR (last_access_timestamp > created_timestamp))
);

CREATE TABLE social_user_right (
    id integer GENERATED ALWAYS AS IDENTITY,
    right_name character varying(50) NOT NULL,
    display_name character varying(50) NOT NULL,
    describe character varying(150) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    CONSTRAINT "PK_social_user_right" PRIMARY KEY (id),
    CONSTRAINT "CK_social_user_right_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly')
);

CREATE TABLE social_user_role (
    id integer GENERATED ALWAYS AS IDENTITY,
    role_name character varying(50) NOT NULL,
    display_name character varying(50) NOT NULL,
    describe character varying(150) NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Enabled'),
    priority boolean NOT NULL,
    CONSTRAINT "PK_social_user_role" PRIMARY KEY (id),
    CONSTRAINT "CK_social_user_role_status_valid_value" CHECK (status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly')
);

CREATE TABLE admin_audit_log (
    id integer GENERATED ALWAYS AS IDENTITY,
    "table" character varying(50) NOT NULL,
    table_key character varying(100) NOT NULL,
    action character varying(50) NOT NULL,
    old_value TEXT NOT NULL,
    new_value TEXT NOT NULL,
    user_id uuid NOT NULL,
    timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', "table" || ' ' || table_key || ' ' || old_value || ' ' || new_value)) STORED,
    CONSTRAINT "PK_admin_audit_log" PRIMARY KEY (id),
    CONSTRAINT "FK_admin_audit_log_user_id" FOREIGN KEY (user_id) REFERENCES admin_user (id) ON DELETE RESTRICT
);

CREATE TABLE session_admin_user (
    session_token character varying(30) NOT NULL,
    user_id uuid NOT NULL,
    saved boolean NOT NULL,
    data jsonb NOT NULL DEFAULT ('{}'),
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_interaction_time timestamp with time zone NOT NULL,
    CONSTRAINT "PK_session_admin_user" PRIMARY KEY (session_token),
    CONSTRAINT "CK_session_admin_user_last_interaction_time_valid_value" CHECK ((last_interaction_time >= created_timestamp)),
    CONSTRAINT "FK_session_admin_user_user_id" FOREIGN KEY (user_id) REFERENCES admin_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_audit_log (
    id integer GENERATED ALWAYS AS IDENTITY,
    "table" character varying(50) NOT NULL,
    table_key character varying(100) NOT NULL,
    action character varying(50) NOT NULL,
    old_value TEXT NOT NULL,
    new_value TEXT NOT NULL,
    user_id uuid NOT NULL,
    timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', "table" || ' ' || table_key || ' ' || old_value || ' ' || new_value)) STORED,
    CONSTRAINT "PK_social_audit_log" PRIMARY KEY (id),
    CONSTRAINT "FK_social_audit_log_user_id" FOREIGN KEY (user_id) REFERENCES admin_user (id) ON DELETE RESTRICT
);

CREATE TABLE admin_user_role_detail (
    role_id integer NOT NULL,
    right_id integer NOT NULL,
    actions jsonb NOT NULL DEFAULT ('{"read":false,"write":false}'),
    CONSTRAINT "PK_admin_user_role_detail" PRIMARY KEY (role_id, right_id),
    CONSTRAINT "FK_admin_user_role_detail_right" FOREIGN KEY (right_id) REFERENCES admin_user_right (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_admin_user_role_detail_role" FOREIGN KEY (role_id) REFERENCES admin_user_role (id) ON DELETE RESTRICT
);

CREATE TABLE admin_user_role_of_user (
    user_id uuid NOT NULL,
    role_id integer NOT NULL,
    CONSTRAINT "PK_admin_user_role_of_user" PRIMARY KEY (user_id, role_id),
    CONSTRAINT "FK_admin_user_role_of_user_role" FOREIGN KEY (role_id) REFERENCES admin_user_role (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_admin_user_role_of_user_user" FOREIGN KEY (user_id) REFERENCES admin_user (id) ON DELETE RESTRICT
);

CREATE TABLE session_social_user (
    session_token character varying(30) NOT NULL,
    user_id uuid NOT NULL,
    saved boolean NOT NULL,
    data jsonb NOT NULL DEFAULT ('{}'),
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_interaction_time timestamp with time zone NOT NULL,
    CONSTRAINT "PK_session_social_user" PRIMARY KEY (session_token),
    CONSTRAINT "CK_session_social_user_last_interaction_time_valid_value" CHECK ((last_interaction_time >= created_timestamp)),
    CONSTRAINT "FK_session_social_user_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_post (
    id bigint GENERATED ALWAYS AS IDENTITY,
    owner uuid NOT NULL,
    title text NOT NULL,
    slug text NOT NULL,
    thumbnail text NULL,
    views integer NOT NULL DEFAULT (0),
    time_read integer NOT NULL DEFAULT (2),
    status character varying(15) NULL DEFAULT ('Pending'),
    content_search text NOT NULL,
    content text NOT NULL,
    pending_content jsonb NULL,
    short_content text NOT NULL,
    content_type character varying(15) NOT NULL,
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', content_search || ' ' || title || ' ' || short_content)) STORED,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    approved_timestamp timestamp with time zone NOT NULL,
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_post" PRIMARY KEY (id),
    CONSTRAINT "CK_social_post_content_type_valid_value" CHECK (content_type = 'HTML' OR content_type = 'MARKDOWN'),
    CONSTRAINT "CK_social_post_status_valid_value" CHECK (status = 'Pending' OR status = 'Approved' OR status = 'Private' OR status = 'Deleted'),
    CONSTRAINT "CK_social_post_time_read_valid_value" CHECK (time_read >= 2),
    CONSTRAINT "CK_social_post_last_modified_timestamp_valid_value" CHECK ((last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)),
    CONSTRAINT "FK_social_post_user_id" FOREIGN KEY (owner) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_action_with_category (
    user_id uuid NOT NULL,
    category_id bigint NOT NULL,
    actions jsonb NOT NULL DEFAULT ('[]'),
    CONSTRAINT "PK_social_user_action_with_category" PRIMARY KEY (user_id, category_id),
    CONSTRAINT "FK_social_user_action_with_category_category_id" FOREIGN KEY (category_id) REFERENCES social_category (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_action_with_category_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_action_with_tag (
    user_id uuid NOT NULL,
    tag_id bigint NOT NULL,
    actions jsonb NOT NULL DEFAULT ('[]'),
    CONSTRAINT "PK_social_user_action_with_tag" PRIMARY KEY (user_id, tag_id),
    CONSTRAINT "FK_social_user_action_with_tag_tag_id" FOREIGN KEY (tag_id) REFERENCES social_tag (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_action_with_tag_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_action_with_user (
    user_id uuid NOT NULL,
    user_id_des uuid NOT NULL,
    actions jsonb NOT NULL DEFAULT ('[]'),
    CONSTRAINT "PK_social_user_action_with_user" PRIMARY KEY (user_id, user_id_des),
    CONSTRAINT "FK_social_user_action_with_user_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_action_with_user_user_id_des" FOREIGN KEY (user_id_des) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_audit_log (
    id integer GENERATED ALWAYS AS IDENTITY,
    "table" character varying(50) NOT NULL,
    table_key character varying(100) NOT NULL,
    action character varying(50) NOT NULL,
    old_value TEXT NOT NULL,
    new_value TEXT NOT NULL,
    user_id uuid NOT NULL,
    amin_user_id uuid NULL,
    timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', "table" || ' ' || table_key || ' ' || old_value || ' ' || new_value)) STORED,
    CONSTRAINT "PK_social_user_audit_log" PRIMARY KEY (id),
    CONSTRAINT "FK_social_user_audit_log_admin_user_id" FOREIGN KEY (amin_user_id) REFERENCES admin_user (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_audit_log_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_role_detail (
    role_id integer NOT NULL,
    right_id integer NOT NULL,
    actions jsonb NOT NULL DEFAULT ('{"read":false,"write":false}'),
    CONSTRAINT "PK_social_user_role_detail" PRIMARY KEY (role_id, right_id),
    CONSTRAINT "FK_social_user_role_detail_right" FOREIGN KEY (right_id) REFERENCES social_user_right (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_role_detail_role" FOREIGN KEY (role_id) REFERENCES social_user_role (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_role_of_user (
    user_id uuid NOT NULL,
    role_id integer NOT NULL,
    CONSTRAINT "PK_social_user_role_of_user" PRIMARY KEY (user_id, role_id),
    CONSTRAINT "FK_social_user_role_of_user_role" FOREIGN KEY (role_id) REFERENCES social_user_role (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_role_of_user_user" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_comment (
    id bigint GENERATED ALWAYS AS IDENTITY,
    parent_id bigint NULL,
    post_id bigint NOT NULL,
    owner uuid NOT NULL,
    content text NOT NULL,
    status character varying(15) NOT NULL DEFAULT ('Created'),
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', content)) STORED,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_comment" PRIMARY KEY (id),
    CONSTRAINT "CK_social_comment_status_valid_value" CHECK (status = 'Created' OR status = 'Edited' OR status = 'Deleted'),
    CONSTRAINT "CK_social_comment_last_modified_timestamp_valid_value" CHECK ((last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)),
    CONSTRAINT "FK_social_comment_parent" FOREIGN KEY (parent_id) REFERENCES social_comment (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_comment_post" FOREIGN KEY (post_id) REFERENCES social_post (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_comment_user_id" FOREIGN KEY (owner) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_post_category (
    post_id bigint NOT NULL,
    category_id bigint NOT NULL,
    CONSTRAINT "PK_social_post_category" PRIMARY KEY (post_id, category_id),
    CONSTRAINT "FK_social_post_category_category" FOREIGN KEY (category_id) REFERENCES social_category (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_post_category_post" FOREIGN KEY (post_id) REFERENCES social_post (id) ON DELETE RESTRICT
);

CREATE TABLE social_post_tag (
    post_id bigint NOT NULL,
    tag_id bigint NOT NULL,
    CONSTRAINT "PK_social_post_tag" PRIMARY KEY (post_id, tag_id),
    CONSTRAINT "FK_social_post_tag_post" FOREIGN KEY (post_id) REFERENCES social_post (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_post_tag_tag" FOREIGN KEY (tag_id) REFERENCES social_tag (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_action_with_post (
    user_id uuid NOT NULL,
    post_id bigint NOT NULL,
    actions jsonb NOT NULL DEFAULT ('[]'),
    CONSTRAINT "PK_social_user_action_with_post" PRIMARY KEY (user_id, post_id),
    CONSTRAINT "FK_social_user_action_with_post_post_id" FOREIGN KEY (post_id) REFERENCES social_post (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_action_with_post_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_notification (
    id bigint GENERATED ALWAYS AS IDENTITY,
    owner uuid NOT NULL,
    action_of_user_id uuid NULL,
    action_of_admin_user_id uuid NULL,
    post_id bigint NULL,
    comment_id bigint NULL,
    user_id uuid NULL,
    status character varying(15) NOT NULL DEFAULT ('Sent'),
    type character varying(25) NOT NULL,
    content jsonb NOT NULL DEFAULT ('{}'),
    last_update_content timestamp with time zone NULL,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_notification" PRIMARY KEY (id),
    CONSTRAINT "CK_social_notification_status_valid_value" CHECK (status = 'Sent' OR status = 'Read' OR status = 'Deleted'),
    CONSTRAINT "CK_social_notification_last_modified_timestamp_valid_value" CHECK ((last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)),
    CONSTRAINT "FK_social_notification_action_of_amdin_user_id" FOREIGN KEY (action_of_admin_user_id) REFERENCES admin_user (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_notification_action_of_user_id" FOREIGN KEY (action_of_user_id) REFERENCES social_user (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_notification_comment_id" FOREIGN KEY (comment_id) REFERENCES social_comment (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_notification_post_id" FOREIGN KEY (post_id) REFERENCES social_post (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_notification_user_id" FOREIGN KEY (owner) REFERENCES social_user (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_notification_user_id_des" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_report (
    id bigint GENERATED ALWAYS AS IDENTITY,
    user_id uuid NULL,
    post_id bigint NULL,
    comment_id bigint NULL,
    type text NOT NULL,
    report_type text NULL,
    content text NULL,
    status character varying(15) NOT NULL DEFAULT ('Pending'),
    reporter_id uuid NOT NULL,
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', coalesce(content, ''))) STORED,
    created_timestamp timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    last_modified_timestamp timestamp with time zone NULL,
    CONSTRAINT "PK_social_report" PRIMARY KEY (id),
    CONSTRAINT "CK_social_report_status_valid_value" CHECK (status = 'Pending' OR status = 'Ignored' OR status = 'Handled'),
    CONSTRAINT "FK_social_report_comment" FOREIGN KEY (comment_id) REFERENCES social_comment (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_report_post" FOREIGN KEY (post_id) REFERENCES social_post (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_report_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

CREATE TABLE social_user_action_with_comment (
    user_id uuid NOT NULL,
    comment_id bigint NOT NULL,
    actions jsonb NOT NULL DEFAULT ('[]'),
    CONSTRAINT "PK_social_user_action_with_comment" PRIMARY KEY (user_id, comment_id),
    CONSTRAINT "FK_social_user_action_with_comment_comment_id" FOREIGN KEY (comment_id) REFERENCES social_comment (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_social_user_action_with_comment_user_id" FOREIGN KEY (user_id) REFERENCES social_user (id) ON DELETE RESTRICT
);

INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (1, 'AdminUserLoginConfig', 'Enabled', '{"number_of_times_allow_failure":5,"lock_time":360}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (15, 'APIGetCommentConfig', 'Enabled', '{"limit_size_get_reply_comment":2}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (14, 'AdminPasswordPolicy', 'Enabled', '{"min_len":5,"max_len":25,"min_upper_char":0,"min_lower_char":0,"min_number_char":0,"min_special_char":0,"expiry_time":30,"required_change_expired_password":true}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (13, 'SocialPasswordPolicy', 'Enabled', '{"min_len":5,"max_len":25,"min_upper_char":0,"min_lower_char":0,"min_number_char":0,"min_special_char":0,"expiry_time":30,"required_change_expired_password":true}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (12, 'AdminUserIdle', 'Enabled', '{"idle":300,"timeout":10,"ping":10}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (11, 'SocialUserIdle', 'Enabled', '{"idle":300,"timeout":10,"ping":10}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (10, 'Notification', 'Enabled', '{"interval_time":120}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (9, 'UploadFileConfig', 'Enabled', '{"max_length_of_single_file":5242880}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (16, 'ForgotPasswordConfig', 'Enabled', '{"expiry_time":2880,"timeout":720,"number_of_times_allow_failure":1,"prefix_url":"/auth/new-password","host_name":"http://localhost:7005","subject":"[oOwlet Blog] Forgot password."}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (7, 'UIConfig', 'Enabled', '{}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (6, 'SocialUserConfirmConfig', 'Enabled', '{"expiry_time":2880,"timeout":720,"number_of_times_allow_failure":3,"prefix_url":"/auth/confirm-account","host_name":"http://localhost:7005","subject":"[oOwlet Blog] Confirm signup."}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (5, 'EmailClientConfig', 'Enabled', '{"limit_sender":5,"template_user_signup":"<p>Dear @Model.DisplayName,</p><p>Confirm link <a href=''@Model.ConfirmLink''>here</a><br>Send datetime: @Model.DateTimeSend</p><p>Thanks for your register.</p>","template_forgot_password":"<p>Dear @Model.DisplayName,</p><p>Confirm link <a href=''@Model.ResetPasswordLink''>here</a><br>Send datetime: @Model.DateTimeSend.</p>"}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (4, 'SessionSocialUserConfig', 'Enabled', '{"expiry_time":5,"extension_time":5}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (3, 'SessionAdminUserConfig', 'Enabled', '{"expiry_time":5,"extension_time":5}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (2, 'SocialUserLoginConfig', 'Enabled', '{"number_of_times_allow_failure":5,"lock_time":360}');
INSERT INTO admin_base_config (id, config_key, status, value)
OVERRIDING SYSTEM VALUE
VALUES (8, 'PublicConfig', 'Enabled', '{"UIConfig":"all","SessionAdminUserConfig":"all","SessionSocialUserConfig":"all","UploadFileConfig":"all","SocialUserIdle":"all","AdminUserIdle":"all","SocialPasswordPolicy":"all","AdminPasswordPolicy":"all"}');

INSERT INTO admin_user (id, created_timestamp, display_name, email, last_access_timestamp, salt, settings, status, password, user_name)
VALUES ('1afc27e9-85c3-4e48-89ab-dd997621ab32', TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Administrator', 'admin@admin', NULL, '82b82727', '{}', 'Readonly', '730B79CA0F3C34D5FF7ABEB11A8F3B28', 'admin');

INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (12, 'Upload files.', 'Upload', 'upload', 'Enabled');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (11, 'Modify, get config of server.', 'Config', 'config', 'Enabled');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (10, 'See and tracking log file.', 'Log', 'log', 'Enabled');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (9, 'Add, block, unblock, delete AdminUser.', 'Admin User', 'admin_user', 'Enabled');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (8, 'Block, unblock SocialUser', 'Social User', 'social_user', 'Enabled');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (7, 'Configure security of Server.', 'Security', 'security', 'Enabled');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (4, 'Add, create, disable tag.', 'Tag', 'tag', 'Enabled');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (5, 'Review, accept, reject post. See report about post.', 'Post', 'post', 'Enabled');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (3, 'Add, create, disable topics', 'Topic', 'topic', 'Enabled');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (2, 'Add, create, disable category.', 'Category', 'category', 'Enabled');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (1, 'Can access Homepage and see statistic.', 'Dashboard', 'dashboard', 'Enabled');
INSERT INTO admin_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (6, 'Delete comment. See report about comment.', 'Comment', 'comment', 'Enabled');

INSERT INTO admin_user_role (id, describe, display_name, priority, role_name, status)
OVERRIDING SYSTEM VALUE
VALUES (1, 'Administrator', 'Administrator', FALSE, 'admin', 'Readonly');

INSERT INTO social_category (id, created_timestamp, describe, display_name, last_modified_timestamp, name, parent_id, slug, status, thumbnail)
OVERRIDING SYSTEM VALUE
VALUES (5, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Life die have number', 'Left', NULL, 'left', NULL, 'left', 'Readonly', NULL);
INSERT INTO social_category (id, created_timestamp, describe, display_name, last_modified_timestamp, name, parent_id, slug, status, thumbnail)
OVERRIDING SYSTEM VALUE
VALUES (3, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Search google to have better solution', 'Dicussion', NULL, 'dicussion', NULL, 'dicussion', 'Readonly', NULL);
INSERT INTO social_category (id, created_timestamp, describe, display_name, last_modified_timestamp, name, parent_id, slug, status, thumbnail)
OVERRIDING SYSTEM VALUE
VALUES (4, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Nothing in here', 'Blog', NULL, 'blog', NULL, 'blog', 'Readonly', NULL);
INSERT INTO social_category (id, created_timestamp, describe, display_name, last_modified_timestamp, name, parent_id, slug, status, thumbnail)
OVERRIDING SYSTEM VALUE
VALUES (1, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'This not a bug this a feature', 'Technology', NULL, 'technology', NULL, 'technology', 'Readonly', NULL);
INSERT INTO social_category (id, created_timestamp, describe, display_name, last_modified_timestamp, name, parent_id, slug, status, thumbnail)
OVERRIDING SYSTEM VALUE
VALUES (2, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Do not click to this', 'Developer', NULL, 'developer', NULL, 'developer', 'Readonly', NULL);

INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (15, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Bootstrap Vue is a Vue.js wrapper for Bootstrap. It is maintained by a community of individual developers and companies.', NULL, 'Bootstrap Vue', 'Readonly', 'bootstrap-vue');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (26, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'React Router is a routing library for React. It is maintained by a community of individual developers and companies.', NULL, 'React Router', 'Readonly', 'react-router');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (25, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Next.js is a JavaScript framework for building web applications. It is maintained by a community of individual developers and companies.', NULL, 'Next.js', 'Readonly', 'nextjs');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (24, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'ASP.NET Core is a web application framework developed by Microsoft. It is maintained by a community of individual developers and companies.', NULL, 'ASP.NET Core', 'Readonly', 'aspnet-core');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (23, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'ASP.NET is a web application framework developed by Microsoft. It is maintained by a community of individual developers and companies.', NULL, 'ASP.NET', 'Readonly', 'aspnet');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (22, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'C# is a programming language and runtime environment developed by Microsoft. It is maintained by a community of individual developers and companies.', NULL, 'CSharp', 'Readonly', 'csharp');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (21, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', '.NET is a programming language and runtime environment developed by Microsoft. It is maintained by a community of individual developers and companies.', NULL, '.NET', 'Readonly', 'dotnet');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (20, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Vue Router I18n is a localization library for Vue Router. It is maintained by a community of individual developers and companies.', NULL, 'Vue Router I18n', 'Readonly', 'vue-router-i18n');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (19, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Vue Resource is a REST client for Vue.js. It is maintained by a community of individual developers and companies.', NULL, 'Vue Resource', 'Readonly', 'vue-resource');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (18, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Vue I18n is a localization library for Vue.js. It is maintained by a community of individual developers and companies.', NULL, 'Vue I18n', 'Readonly', 'vue-i18n');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (17, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Vuex is a state management pattern and library for Vue.js applications. It is maintained by a community of individual developers and companies.', NULL, 'Vuex', 'Readonly', 'vuex');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (16, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Vue Router is a routing library for Vue.js. It is maintained by a community of individual developers and companies.', NULL, 'Vue Router', 'Readonly', 'vue-router');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (14, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Material Icons is a set of open source icons for use in web and mobile applications. It is maintained by a community of individual developers and companies.', NULL, 'Material Icons', 'Readonly', 'material-icons');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (13, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Material Design is a design language developed by Google. It is used to create a consistent and beautiful user experience across all products on Android, iOS, and the web.', NULL, 'Material Design', 'Readonly', 'material-design');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (12, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Material-UI is a React component library that enables you to create beautiful, high-fidelity, mobile-first experiences. It is maintained by a community of individual developers and companies.', NULL, 'Material-UI', 'Readonly', 'material-ui');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (11, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Bootstrap is a free and open-source front-end web framework for designing websites and web applications. It is maintained by a community of individual developers and companies.', NULL, 'Bootstrap', 'Readonly', 'bootstrap');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (10, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Less is a stylesheet language that is interpreted into Cascading Style Sheets (CSS). It is maintained by a community of individual developers and companies.', NULL, 'Less', 'Readonly', 'less');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (9, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Sass is a stylesheet language that is interpreted into Cascading Style Sheets (CSS). It is maintained by a community of individual developers and companies.', NULL, 'Sass', 'Readonly', 'sass');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (8, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Gulp is a streaming build system. It is maintained by a community of individual developers and companies.', NULL, 'Gulp', 'Readonly', 'gulp');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (7, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Webpack is a module bundler that packs multiple modules with dependencies into a single module. It is maintained by a community of individual developers and companies.', NULL, 'Webpack', 'Readonly', 'webpack');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (6, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Vue CLI is a command-line interface for the Vue.js development platform. It is used to create and manage projects for the Vue framework.', NULL, 'Vue CLI', 'Readonly', 'vue-cli');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (5, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'React Native is a framework for building native apps using React. It is maintained by Facebook and a community of individual developers and companies.', NULL, 'React Native', 'Readonly', 'react-native');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (4, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Angular CLI is a command-line interface for the Angular development platform. It is used to create and manage projects for the Angular framework.', NULL, 'Angular CLI', 'Readonly', 'angular-cli');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (3, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Vue.js is an open-source JavaScript framework for building user interfaces. It is maintained by a community of individual developers and companies. Vue can be used as a base in the development of single-page or mobile applications.', NULL, 'Vue', 'Readonly', 'vue');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (2, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'React is a JavaScript library for building user interfaces. It is maintained by Facebook and a community of individual developers and companies. React can be used as a base in the development of single-page or mobile applications.', NULL, 'React', 'Readonly', 'react');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (1, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'Angular is a TypeScript-based open-source web application platform led by the Angular Team at Google and by a community of individuals and corporations. Angular is a complete rewrite from the same team that built AngularJS.', NULL, 'Angular', 'Readonly', 'angular');
INSERT INTO social_tag (id, created_timestamp, describe, last_modified_timestamp, name, status, tag)
OVERRIDING SYSTEM VALUE
VALUES (27, TIMESTAMPTZ '2022-02-20 06:13:13 UTC', 'React Router DOM is a routing library for React. It is maintained by a community of individual developers and companies.', NULL, 'React Router DOM', 'Readonly', 'react-router-dom');

INSERT INTO social_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (3, 'Can create, interactive report.', 'Report', 'report', 'Readonly');
INSERT INTO social_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (4, 'Can create, interactive report.', 'Upload', 'upload', 'Readonly');
INSERT INTO social_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (1, 'Can create, interactive posts.', 'Post', 'post', 'Readonly');
INSERT INTO social_user_right (id, describe, display_name, right_name, status)
OVERRIDING SYSTEM VALUE
VALUES (2, 'Can create, interactive comment.', 'Comment', 'comment', 'Readonly');

INSERT INTO social_user_role (id, describe, display_name, priority, role_name, status)
OVERRIDING SYSTEM VALUE
VALUES (1, 'Normal user', 'User', FALSE, 'user', 'Readonly');

INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (1, 1, '{"read":true,"write":true}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (12, 1, '{"read":true,"write":true}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (11, 1, '{"read":true,"write":true}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (10, 1, '{"read":true,"write":true}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (8, 1, '{"read":true,"write":true}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (7, 1, '{"read":true,"write":true}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (9, 1, '{"read":true,"write":true}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (5, 1, '{"read":true,"write":true}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (4, 1, '{"read":true,"write":true}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (3, 1, '{"read":true,"write":true}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (2, 1, '{"read":true,"write":true}');
INSERT INTO admin_user_role_detail (right_id, role_id, actions)
VALUES (6, 1, '{"read":true,"write":true}');

INSERT INTO admin_user_role_of_user (role_id, user_id)
VALUES (1, '1afc27e9-85c3-4e48-89ab-dd997621ab32');

INSERT INTO social_user_role_detail (right_id, role_id, actions)
VALUES (3, 1, '{"read":true,"write":true}');
INSERT INTO social_user_role_detail (right_id, role_id, actions)
VALUES (1, 1, '{"read":true,"write":true}');
INSERT INTO social_user_role_detail (right_id, role_id, actions)
VALUES (2, 1, '{"read":true,"write":true}');
INSERT INTO social_user_role_detail (right_id, role_id, actions)
VALUES (4, 1, '{"read":true,"write":true}');

CREATE INDEX "IX_admin_audit_log_search_vector" ON admin_audit_log USING GIN (search_vector);

CREATE INDEX "IX_admin_audit_log_table" ON admin_audit_log ("table");

CREATE INDEX "IX_admin_audit_log_user_id" ON admin_audit_log (user_id);

CREATE UNIQUE INDEX "IX_admin_base_config_config_key" ON admin_base_config (config_key) WHERE (status) <> 'Disabled';

CREATE UNIQUE INDEX "IX_admin_user_user_name_email" ON admin_user (user_name, email) WHERE (status) <> 'Deleted';

CREATE UNIQUE INDEX "IX_admin_user_right_right_name" ON admin_user_right (right_name) WHERE (status) <> 'Disabled';

CREATE UNIQUE INDEX "IX_admin_user_role_role_name" ON admin_user_role (role_name) WHERE (status) <> 'Disabled';

CREATE INDEX "IX_admin_user_role_detail_right_id" ON admin_user_role_detail (right_id);

CREATE INDEX "IX_admin_user_role_of_user_role_id" ON admin_user_role_of_user (role_id);

CREATE UNIQUE INDEX "IX_session_admin_user_token_user_id" ON session_admin_user (session_token, user_id);

CREATE INDEX "IX_session_admin_user_user_id" ON session_admin_user (user_id);

CREATE UNIQUE INDEX "IX_session_social_user_token_user_id" ON session_social_user (session_token, user_id);

CREATE INDEX "IX_session_social_user_user_id" ON session_social_user (user_id);

CREATE INDEX "IX_social_audit_log_search_vector" ON social_audit_log USING GIN (search_vector);

CREATE INDEX "IX_social_audit_log_table" ON social_audit_log ("table");

CREATE INDEX "IX_social_audit_log_user_id" ON social_audit_log (user_id);

CREATE INDEX "IX_social_category_parent_id" ON social_category (parent_id);

CREATE INDEX "IX_social_category_search_vector" ON social_category USING GIN (search_vector);

CREATE UNIQUE INDEX "IX_social_category_slug" ON social_category (slug) WHERE (status) <> 'Disabled';

CREATE INDEX "IX_social_comment_owner" ON social_comment (owner);

CREATE INDEX "IX_social_comment_parent_id" ON social_comment (parent_id);

CREATE INDEX "IX_social_comment_post_id" ON social_comment (post_id);

CREATE INDEX "IX_social_comment_search_vector" ON social_comment USING GIST (search_vector);

CREATE INDEX "IX_social_notification_action_of_admin_user_id" ON social_notification (action_of_admin_user_id);

CREATE INDEX "IX_social_notification_action_of_user_id" ON social_notification (action_of_user_id);

CREATE INDEX "IX_social_notification_comment_id" ON social_notification (comment_id);

CREATE INDEX "IX_social_notification_owner" ON social_notification (owner);

CREATE INDEX "IX_social_notification_post_id" ON social_notification (post_id);

CREATE INDEX "IX_social_notification_user_id" ON social_notification (user_id);

CREATE INDEX "IX_social_post_owner" ON social_post (owner);

CREATE INDEX "IX_social_post_search_vector" ON social_post USING GIST (search_vector);

CREATE UNIQUE INDEX "IX_social_post_slug" ON social_post (slug) WHERE (slug <> '');

CREATE INDEX "IX_social_post_category_category_id" ON social_post_category (category_id);

CREATE INDEX "IX_social_post_tag_tag_id" ON social_post_tag (tag_id);

CREATE INDEX "IX_social_report_comment_id" ON social_report (comment_id);

CREATE INDEX "IX_social_report_post_id" ON social_report (post_id);

CREATE INDEX "IX_social_report_search_vector" ON social_report USING GIN (search_vector);

CREATE INDEX "IX_social_report_user_id" ON social_report (user_id);

CREATE UNIQUE INDEX "IX_social_tag_tag" ON social_tag (tag);

CREATE INDEX "IX_social_user_search_vector" ON social_user USING GIST (search_vector);

CREATE UNIQUE INDEX "IX_social_user_user_name_email" ON social_user (user_name, email) WHERE (status) <> 'Deleted';

CREATE INDEX "IX_social_user_action_with_category_category_id" ON social_user_action_with_category (category_id);

CREATE INDEX "IX_social_user_action_with_comment_comment_id" ON social_user_action_with_comment (comment_id);

CREATE INDEX "IX_social_user_action_with_post_post_id" ON social_user_action_with_post (post_id);

CREATE INDEX "IX_social_user_action_with_tag_tag_id" ON social_user_action_with_tag (tag_id);

CREATE INDEX "IX_social_user_action_with_user_user_id_des" ON social_user_action_with_user (user_id_des);

CREATE INDEX "IX_social_user_audit_log_amin_user_id" ON social_user_audit_log (amin_user_id);

CREATE INDEX "IX_social_user_audit_log_search_vector" ON social_user_audit_log USING GIN (search_vector);

CREATE INDEX "IX_social_user_audit_log_table" ON social_user_audit_log ("table");

CREATE INDEX "IX_social_user_audit_log_user_id" ON social_user_audit_log (user_id);

CREATE UNIQUE INDEX "IX_social_user_right_right_name" ON social_user_right (right_name) WHERE (status) <> 'Disabled';

CREATE UNIQUE INDEX "IX_social_user_role_role_name" ON social_user_role (role_name) WHERE (status) <> 'Disabled';

CREATE INDEX "IX_social_user_role_detail_right_id" ON social_user_role_detail (right_id);

CREATE INDEX "IX_social_user_role_of_user_role_id" ON social_user_role_of_user (role_id);

SELECT setval(
    pg_get_serial_sequence('admin_base_config', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM admin_base_config) + 1,
        nextval(pg_get_serial_sequence('admin_base_config', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('admin_user_right', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM admin_user_right) + 1,
        nextval(pg_get_serial_sequence('admin_user_right', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('admin_user_role', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM admin_user_role) + 1,
        nextval(pg_get_serial_sequence('admin_user_role', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('social_category', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM social_category) + 1,
        nextval(pg_get_serial_sequence('social_category', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('social_tag', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM social_tag) + 1,
        nextval(pg_get_serial_sequence('social_tag', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('social_user_right', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM social_user_right) + 1,
        nextval(pg_get_serial_sequence('social_user_right', 'id'))),
    false);
SELECT setval(
    pg_get_serial_sequence('social_user_role', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM social_user_role) + 1,
        nextval(pg_get_serial_sequence('social_user_role', 'id'))),
    false);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('DBCreation', '5.0.10');

COMMIT;

START TRANSACTION;

ALTER TABLE social_post ADD settings jsonb NOT NULL DEFAULT '{}';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('UpdateDBCreation', '5.0.10');

COMMIT;

START TRANSACTION;

CREATE TABLE redirect_url (
    url integer NOT NULL,
    times bigint NOT NULL,
    CONSTRAINT "PK_redirect_url" PRIMARY KEY (url),
    CONSTRAINT "CK_redirect_url_times_valid_value" CHECK ((times >= 0))
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('UpdateDBCreation_01', '5.0.10');

COMMIT;



-------------------------
-------------------------
-------------------------
-------------------------
-------------------------

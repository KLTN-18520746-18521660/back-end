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
    created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_social_post" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Deleted' OR status = 'Pending' OR status = 'Private' OR status = 'Approved')
);

CREATE TABLE social_post_content (
    post_id BIGINT GENERATED ALWAYS AS IDENTITY,
    content_search TEXT NOT NULL,
    content TEXT NOT NULL,
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('english', content_search)) STORED,
    CONSTRAINT "PK_social_post_content" PRIMARY KEY (post_id),
    CONSTRAINT "FK_social_post" FOREIGN KEY (post_id) REFERENCES social_post(id)
);

CREATE TABLE social_category (
	id BIGINT GENERATED ALWAYS AS IDENTITY,
    parent_id INTEGER NULL DEFAULT NULL,
	name VARCHAR(20) NOT NULL,
	display_name VARCHAR(50) NOT NULL,
	describe VARCHAR(100) NOT NULL,
	slug TEXT NOT NULL,
	thumbnail TEXT,
	status VARCHAR(15) NOT NULL DEFAULT 'Enabled',
	created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
	last_modified_timestamp TIMESTAMPTZ,
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
	last_modified_timestamp TIMESTAMPTZ,
	CONSTRAINT "PK_social_tag" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE social_post_tag (
    post_id BIGINT NOT NULL,
    tag_id BIGINT NOT NULL,
    CONSTRAINT "PK_social_post_tag" PRIMARY KEY (post_id, tag_id)
);

CREATE TABLE social_post_category (
    post_id BIGINT NOT NULL,
    category_id BIGINT NOT NULL,
    CONSTRAINT "PK_social_post_category" PRIMARY KEY (post_id, category_id)
);

CREATE TABLE social_comment (
    id BIGINT GENERATED ALWAYS AS IDENTITY,
    parent_id BIGINT NULL DEFAULT NULL,
    post_id BIGINT NOT NULL,
    owner UUID NOT NULL,
    content TEXT NOT NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'Created',
	created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
	last_modified_timestamp TIMESTAMPTZ,
    CONSTRAINT "PK_social_comment" PRIMARY KEY (id),
    CONSTRAINT "FK_social_comment_post" FOREIGN KEY (post_id) REFERENCES social_post(id),
    CONSTRAINT "FK_social_comment_parent" FOREIGN KEY (id) REFERENCES social_comment(id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Deleted' OR status = 'Created' OR status = 'Edited')
);

CREATE TABLE social_report (
    id BIGINT GENERATED ALWAYS AS IDENTITY,
    user_id UUID NOT NULL,
    post_id BIGINT NULL,
    comment_id BIGINT NULL,
    content TEXT NOT NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'Pending',
	created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
	last_modified_timestamp TIMESTAMPTZ,
    CONSTRAINT "PK_social_report" PRIMARY KEY (id),
    CONSTRAINT "FK_social_report_post" FOREIGN KEY (post_id) REFERENCES social_post(id),
    CONSTRAINT "FK_social_report_comment" FOREIGN KEY (comment_id) REFERENCES social_comment(id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Pending' OR status = 'Handled' OR status = 'Ignored')
);

-- follow, report, friend
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
    status VARCHAR(15) NOT NULL,
    content JSON NOT NULL DEFAULT '{}',
	created_timestamp TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
	last_modified_timestamp TIMESTAMPTZ,
    CONSTRAINT "PK_social_notification" PRIMARY KEY (id),
    CONSTRAINT "FK_social_notification_user_id" FOREIGN KEY (user_id) REFERENCES social_user(id)
);

CREATE UNIQUE INDEX "IX_social_post_slug" ON social_post(slug) WHERE status != 'Deleted';
CREATE UNIQUE INDEX "IX_social_category_slug" ON social_category(slug) WHERE status != 'Disabled';
CREATE UNIQUE INDEX "IX_social_user_user_name_email" ON social_user (user_name, email) WHERE status != 'Deleted';

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
    status VARCHAR(15) NOT NULL,
    CONSTRAINT "PK_admin_user_right" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE admin_user_role (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    role_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    describe VARCHAR(150) NOT NULL,
    rights JSON NOT NULL DEFAULT '[]',
    status VARCHAR(15) NOT NULL,
    CONSTRAINT "PK_admin_user_role" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE base_config (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    config_key VARCHAR(50) NOT NULL,
    value JSON NOT NULL DEFAULT '{}',
    status VARCHAR(15) NOT NULL,
    CONSTRAINT "PK_base_config" PRIMARY KEY (id),
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
    status VARCHAR(15) NOT NULL,
    CONSTRAINT "PK_social_user_right" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE TABLE social_user_role (
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    role_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    describe VARCHAR(150) NOT NULL,
    rights JSON NOT NULL DEFAULT '[]',
    status VARCHAR(15) NOT NULL,
    CONSTRAINT "PK_social_user_role" PRIMARY KEY (id),
    CONSTRAINT "CK_status_valid_value" CHECK (status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly')
);

CREATE UNIQUE INDEX "IX_admin_user_user_name_email" ON admin_user (user_name, email) WHERE status != 'Deleted';
CREATE UNIQUE INDEX "IX_admin_user_right_right_name" ON admin_user_right (right_name) WHERE status != 'Disabled';
CREATE UNIQUE INDEX "IX_admin_user_role_role_name" ON admin_user_role (role_name) WHERE status != 'Disabled';
CREATE UNIQUE INDEX "IX_base_config_config_key" ON base_config (config_key) WHERE status != 'Disabled';
CREATE INDEX "IX_admin_audit_log_search_vector" ON admin_audit_log USING GIN (search_vector);
CREATE INDEX "IX_social_audit_log_search_vector" ON social_audit_log USING GIN (search_vector);
CREATE UNIQUE INDEX "IX_social_user_right_right_name" ON social_user_right (right_name) WHERE status != 'Disabled';
CREATE UNIQUE INDEX "IX_social_user_role_role_name" ON social_user_role (role_name) WHERE status != 'Disabled';

/********** CACHED **********/
CREATE TABLE session_social_user (
    session_id VARCHAR(30) NOT NULL,
    user_id UUID NOT NULL,
    saved BOOLEAN NOT NULL DEFAULT 'false',
    data JSON NOT NULL DEFAULT '{}',
	login_time TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
	last_interaction_time TIMESTAMPTZ,
    CONSTRAINT "PK_session_social_user" PRIMARY KEY (session_id),
    CONSTRAINT "FK_session_social_user_user_id" FOREIGN KEY (user_id) REFERENCES social_user(id)
);

CREATE TABLE session_admin_user (
    session_id VARCHAR(30) NOT NULL,
    user_id UUID NOT NULL,
    saved BOOLEAN NOT NULL DEFAULT 'false',
    data JSON NOT NULL DEFAULT '{}',
	login_time TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
	last_interaction_time TIMESTAMPTZ,
    CONSTRAINT "PK_session_admin_user" PRIMARY KEY (session_id),
    CONSTRAINT "FK_session_admin_user_user_id" FOREIGN KEY (user_id) REFERENCES admin_user(id)
);
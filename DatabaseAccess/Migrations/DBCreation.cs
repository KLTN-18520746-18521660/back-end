using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

namespace DatabaseAccess.Migrations
{
    public partial class DBCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_base_config",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    config_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    value = table.Column<string>(type: "JSON", nullable: true, defaultValueSql: "'{}'"),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Enabled'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_base_config", x => x.id);
                    table.CheckConstraint("CK_admin_base_config_status_valid_value", "status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly'");
                });

            migrationBuilder.CreateTable(
                name: "admin_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    salt = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false, defaultValueSql: "SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)"),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Activated'"),
                    settings = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    last_access_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_user", x => x.id);
                    table.CheckConstraint("CK_admin_user_status_valid_value", "status = 'Activated' OR status = 'Blocked' OR status = 'Deleted' OR status = 'Readonly'");
                    table.CheckConstraint("CK_admin_user_last_access_timestamp_valid_value", "(last_access_timestamp IS NULL) OR (last_access_timestamp > created_timestamp)");
                });

            migrationBuilder.CreateTable(
                name: "admin_user_right",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    right_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    describe = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Enabled'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_user_right", x => x.id);
                    table.CheckConstraint("CK_admin_user_right_status_valid_value", "status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly'");
                });

            migrationBuilder.CreateTable(
                name: "admin_user_role",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    role_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    describe = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Enabled'"),
                    priority = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_user_role", x => x.id);
                    table.CheckConstraint("CK_admin_user_role_status_valid_value", "status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly'");
                });

            migrationBuilder.CreateTable(
                name: "social_category",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    display_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    describe = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    thumbnail = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Enabled'"),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "name", "display_name", "describe" }),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    last_modified_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_category", x => x.id);
                    table.CheckConstraint("CK_social_category_last_modified_timestamp_valid_value", "(last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)");
                    table.CheckConstraint("CK_social_category_status_valid_value", "status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly'");
                    table.ForeignKey(
                        name: "FK_social_category_parent",
                        column: x => x.parent_id,
                        principalTable: "social_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_tag",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    tag = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    describe = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Enabled'"),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    last_modified_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_tag", x => x.id);
                    table.CheckConstraint("CK_social_tag_status_valid_value", "status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly'");
                });

            migrationBuilder.CreateTable(
                name: "social_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    first_name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    last_name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    display_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    salt = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false, defaultValueSql: "SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)"),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    sex = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    country = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    city = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    province = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    verified_email = table.Column<bool>(type: "boolean", nullable: false),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Activated'"),
                    settings = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    ranks = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    publics = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'"),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "display_name", "user_name" }),
                    last_access_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user", x => x.id);
                    table.CheckConstraint("CK_social_report_status_valid_value", "status = 'Activated' OR status = 'Deleted' OR status = 'Blocked'");
                    table.CheckConstraint("CK_social_user_last_access_timestamp_valid_value", "(last_access_timestamp IS NULL) OR (last_access_timestamp > created_timestamp)");
                });

            migrationBuilder.CreateTable(
                name: "social_user_right",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    right_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    describe = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Enabled'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_right", x => x.id);
                    table.CheckConstraint("CK_social_user_right_status_valid_value", "status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly'");
                });

            migrationBuilder.CreateTable(
                name: "social_user_role",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    role_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    describe = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Enabled'"),
                    priority = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_role", x => x.id);
                    table.CheckConstraint("CK_social_user_role_status_valid_value", "status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly'");
                });

            migrationBuilder.CreateTable(
                name: "admin_audit_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    table = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    table_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    old_value = table.Column<string>(type: "TEXT", nullable: false),
                    new_value = table.Column<string>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "table", "table_key", "old_value", "new_value" })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_audit_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_admin_audit_log_user_id",
                        column: x => x.user_id,
                        principalTable: "admin_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "session_admin_user",
                columns: table => new
                {
                    session_token = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    saved = table.Column<bool>(type: "boolean", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    last_interaction_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_admin_user", x => x.session_token);
                    table.CheckConstraint("CK_session_admin_user_last_interaction_time_valid_value", "(last_interaction_time >= created_timestamp)");
                    table.ForeignKey(
                        name: "FK_session_admin_user_user_id",
                        column: x => x.user_id,
                        principalTable: "admin_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_audit_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    table = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    table_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    old_value = table.Column<string>(type: "TEXT", nullable: false),
                    new_value = table.Column<string>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "table", "table_key", "old_value", "new_value" })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_audit_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_social_audit_log_user_id",
                        column: x => x.user_id,
                        principalTable: "admin_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "admin_user_role_detail",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    right_id = table.Column<int>(type: "integer", nullable: false),
                    actions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{\r\n  \"read\": false,\r\n  \"write\": false\r\n}'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_user_role_detail", x => new { x.role_id, x.right_id });
                    table.ForeignKey(
                        name: "FK_admin_user_role_detail_right",
                        column: x => x.right_id,
                        principalTable: "admin_user_right",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_admin_user_role_detail_role",
                        column: x => x.role_id,
                        principalTable: "admin_user_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "admin_user_role_of_user",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_user_role_of_user", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_admin_user_role_of_user_role",
                        column: x => x.role_id,
                        principalTable: "admin_user_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_admin_user_role_of_user_user",
                        column: x => x.user_id,
                        principalTable: "admin_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "session_social_user",
                columns: table => new
                {
                    session_token = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    saved = table.Column<bool>(type: "boolean", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    last_interaction_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_social_user", x => x.session_token);
                    table.CheckConstraint("CK_session_social_user_last_interaction_time_valid_value", "(last_interaction_time >= created_timestamp)");
                    table.ForeignKey(
                        name: "FK_session_social_user_user_id",
                        column: x => x.user_id,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_post",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    owner = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    thumbnail = table.Column<string>(type: "text", nullable: false),
                    views = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "0"),
                    time_read = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "2"),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, defaultValueSql: "'Pending'"),
                    content_search = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    pending_content = table.Column<string>(type: "jsonb", nullable: true),
                    short_content = table.Column<string>(type: "text", nullable: false),
                    content_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "content_search", "title", "short_content" }),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    approved_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_post", x => x.id);
                    table.CheckConstraint("CK_social_post_content_type_valid_value", "content_type = 'HTML' OR content_type = 'MARKDOWN'");
                    table.CheckConstraint("CK_social_post_status_valid_value", "status = 'Pending' OR status = 'Approved' OR status = 'Private' OR status = 'Deleted'");
                    table.CheckConstraint("CK_social_post_time_read_valid_value", "time_read >= 2");
                    table.CheckConstraint("CK_social_post_last_modified_timestamp_valid_value", "(last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)");
                    table.ForeignKey(
                        name: "FK_social_post_user_id",
                        column: x => x.owner,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_user_action_with_category",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    actions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_action_with_category", x => new { x.user_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_social_user_action_with_category_category_id",
                        column: x => x.category_id,
                        principalTable: "social_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_user_action_with_category_user_id",
                        column: x => x.user_id,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_user_action_with_tag",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<long>(type: "bigint", nullable: false),
                    actions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_action_with_tag", x => new { x.user_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_social_user_action_with_tag_tag_id",
                        column: x => x.tag_id,
                        principalTable: "social_tag",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_user_action_with_tag_user_id",
                        column: x => x.user_id,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_user_action_with_user",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id_des = table.Column<Guid>(type: "uuid", nullable: false),
                    actions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_action_with_user", x => new { x.user_id, x.user_id_des });
                    table.ForeignKey(
                        name: "FK_social_user_action_with_user_user_id",
                        column: x => x.user_id,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_user_action_with_user_user_id_des",
                        column: x => x.user_id_des,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_user_audit_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    table = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    table_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    old_value = table.Column<string>(type: "TEXT", nullable: false),
                    new_value = table.Column<string>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amin_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "table", "table_key", "old_value", "new_value" })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_audit_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_social_user_audit_log_admin_user_id",
                        column: x => x.amin_user_id,
                        principalTable: "admin_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_user_audit_log_user_id",
                        column: x => x.user_id,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_user_role_detail",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    right_id = table.Column<int>(type: "integer", nullable: false),
                    actions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{\r\n  \"read\": false,\r\n  \"write\": false\r\n}'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_role_detail", x => new { x.role_id, x.right_id });
                    table.ForeignKey(
                        name: "FK_social_user_role_detail_right",
                        column: x => x.right_id,
                        principalTable: "social_user_right",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_user_role_detail_role",
                        column: x => x.role_id,
                        principalTable: "social_user_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_user_role_of_user",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_role_of_user", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_social_user_role_of_user_role",
                        column: x => x.role_id,
                        principalTable: "social_user_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_user_role_of_user_user",
                        column: x => x.user_id,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_comment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    owner = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Created'"),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "content" }),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    last_modified_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_comment", x => x.id);
                    table.CheckConstraint("CK_social_comment_status_valid_value", "status = 'Created' OR status = 'Edited' OR status = 'Deleted'");
                    table.CheckConstraint("CK_social_comment_last_modified_timestamp_valid_value", "(last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)");
                    table.ForeignKey(
                        name: "FK_social_comment_parent",
                        column: x => x.parent_id,
                        principalTable: "social_comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_comment_post",
                        column: x => x.post_id,
                        principalTable: "social_post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_comment_user_id",
                        column: x => x.owner,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_post_category",
                columns: table => new
                {
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_post_category", x => new { x.post_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_social_post_category_category",
                        column: x => x.category_id,
                        principalTable: "social_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_post_category_post",
                        column: x => x.post_id,
                        principalTable: "social_post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_post_tag",
                columns: table => new
                {
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    tag_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_post_tag", x => new { x.post_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_social_post_tag_post",
                        column: x => x.post_id,
                        principalTable: "social_post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_post_tag_tag",
                        column: x => x.tag_id,
                        principalTable: "social_tag",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_user_action_with_post",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    actions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_action_with_post", x => new { x.user_id, x.post_id });
                    table.ForeignKey(
                        name: "FK_social_user_action_with_post_post_id",
                        column: x => x.post_id,
                        principalTable: "social_post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_user_action_with_post_user_id",
                        column: x => x.user_id,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_notification",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<long>(type: "bigint", nullable: true),
                    comment_id = table.Column<long>(type: "bigint", nullable: true),
                    user_id_des = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Sent'"),
                    type = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    last_modified_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_notification", x => x.id);
                    table.CheckConstraint("CK_social_notification_status_valid_value", "status = 'Sent' OR status = 'Read' OR status = 'Deleted'");
                    table.CheckConstraint("CK_social_notification_last_modified_timestamp_valid_value", "(last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)");
                    table.ForeignKey(
                        name: "FK_social_notification_comment_id",
                        column: x => x.comment_id,
                        principalTable: "social_comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_notification_post_id",
                        column: x => x.post_id,
                        principalTable: "social_post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_notification_user_id",
                        column: x => x.user_id,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_notification_user_id_des",
                        column: x => x.user_id_des,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_report",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    post_id = table.Column<long>(type: "bigint", nullable: true),
                    comment_id = table.Column<long>(type: "bigint", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    report_type = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Pending'"),
                    reporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "content" }),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    last_modified_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_report", x => x.id);
                    table.CheckConstraint("CK_social_report_status_valid_value", "status = 'Pending' OR status = 'Ignored' OR status = 'Handled'");
                    table.ForeignKey(
                        name: "FK_social_report_comment",
                        column: x => x.comment_id,
                        principalTable: "social_comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_report_post",
                        column: x => x.post_id,
                        principalTable: "social_post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_report_user_id",
                        column: x => x.user_id,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_user_action_with_comment",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_id = table.Column<long>(type: "bigint", nullable: false),
                    actions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_action_with_comment", x => new { x.user_id, x.comment_id });
                    table.ForeignKey(
                        name: "FK_social_user_action_with_comment_comment_id",
                        column: x => x.comment_id,
                        principalTable: "social_comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_user_action_with_comment_user_id",
                        column: x => x.user_id,
                        principalTable: "social_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "admin_base_config",
                columns: new[] { "id", "config_key", "status", "value" },
                values: new object[,]
                {
                    { 1, "AdminUserLoginConfig", "Enabled", "{\r\n  \"number_of_times_allow_login_failure\": 5,\r\n  \"lock_time\": 360\r\n}" },
                    { 2, "SocialUserLoginConfig", "Enabled", "{\r\n  \"number_of_times_allow_login_failure\": 5,\r\n  \"lock_time\": 360\r\n}" },
                    { 3, "SessionAdminUserConfig", "Enabled", "{\r\n  \"expiry_time\": 5,\r\n  \"extension_time\": 5\r\n}" },
                    { 4, "SessionSocialUserConfig", "Enabled", "{\r\n  \"expiry_time\": 5,\r\n  \"extension_time\": 5\r\n}" },
                    { 5, "EmailClientConfig", "Enabled", "{\r\n  \"limit_sender\": 5,\r\n  \"template_user_signup\": \"<p>Dear @Model.UserName,</p>\\r\\n                                        <p>Confirm link here: <a href='@Model.ConfirmLink'>@Model.ConfirmLink</a><br>\\r\\n                                        Send datetime: @Model.DateTimeSend</p>\\r\\n                                        <p>Thanks for your register.</p>\"\r\n}" },
                    { 6, "SocialUserConfirmConfig", "Enabled", "{\r\n  \"expiry_time\": 2880,\r\n  \"number_of_times_allow_confirm_failure\": 3,\r\n  \"prefix_url\": \"/auth/confirm-account\",\r\n  \"host_name\": \"http://localhost:4200\"\r\n}" },
                    { 7, "UIConfig", "Enabled", "{}" },
                    { 8, "PublicConfig", "Enabled", "{\r\n  \"UIConfig\": \"all\",\r\n  \"SessionAdminUserConfig\": \"all\",\r\n  \"SessionSocialUserConfig\": \"all\",\r\n  \"UploadFileConfig\": \"all\"\r\n}" }
                });

            migrationBuilder.InsertData(
                table: "admin_user",
                columns: new[] { "id", "created_timestamp", "display_name", "email", "last_access_timestamp", "salt", "settings", "status", "password", "user_name" },
                values: new object[] { new Guid("1afc27e9-85c3-4e48-89ab-dd997621ab32"), new DateTime(2022, 4, 26, 19, 20, 54, 359, DateTimeKind.Utc).AddTicks(2302), "Administrator", "admin@admin", null, "f0925c2b", "{}", "Readonly", "9F1E9DA16B5E9E11CE426F4843F22742", "admin" });

            migrationBuilder.InsertData(
                table: "admin_user_right",
                columns: new[] { "id", "describe", "display_name", "right_name", "status" },
                values: new object[,]
                {
                    { 12, "Upload files.", "Upload", "upload", "Enabled" },
                    { 10, "See and tracking log file.", "Log", "log", "Enabled" },
                    { 9, "Add, block, unblock, delete AdminUser.", "Admin User", "admin_user", "Enabled" },
                    { 8, "Block, unblock SocialUser", "Social User", "social_user", "Enabled" },
                    { 7, "Configure security of Server.", "Security", "security", "Enabled" },
                    { 11, "Modify, get config of server.", "Config", "config", "Enabled" },
                    { 5, "Review, accept, reject post. See report about post.", "Post", "post", "Enabled" },
                    { 4, "Add, create, disable tag.", "Tag", "tag", "Enabled" },
                    { 3, "Add, create, disable topics", "Topic", "topic", "Enabled" },
                    { 2, "Add, create, disable category.", "Category", "category", "Enabled" },
                    { 6, "Delete comment. See report about comment.", "Comment", "comment", "Enabled" },
                    { 1, "Can access Homepage and see statistic.", "Dashboard", "dashboard", "Enabled" }
                });

            migrationBuilder.InsertData(
                table: "admin_user_role",
                columns: new[] { "id", "describe", "display_name", "priority", "role_name", "status" },
                values: new object[] { 1, "Administrator", "Administrator", false, "admin", "Readonly" });

            migrationBuilder.InsertData(
                table: "social_category",
                columns: new[] { "id", "created_timestamp", "describe", "display_name", "last_modified_timestamp", "name", "parent_id", "slug", "status", "thumbnail" },
                values: new object[,]
                {
                    { 5L, new DateTime(2022, 4, 26, 19, 20, 54, 483, DateTimeKind.Utc).AddTicks(4714), "Life die have number", "Left", null, "left", null, "left", "Readonly", null },
                    { 4L, new DateTime(2022, 4, 26, 19, 20, 54, 483, DateTimeKind.Utc).AddTicks(4651), "Nothing in here", "Blog", null, "blog", null, "blog", "Readonly", null },
                    { 2L, new DateTime(2022, 4, 26, 19, 20, 54, 483, DateTimeKind.Utc).AddTicks(4341), "Do not click to this", "Developer", null, "developer", null, "developer", "Readonly", null },
                    { 1L, new DateTime(2022, 4, 26, 19, 20, 54, 483, DateTimeKind.Utc).AddTicks(1482), "This not a bug this a feature", "Technology", null, "technology", null, "technology", "Readonly", null },
                    { 3L, new DateTime(2022, 4, 26, 19, 20, 54, 483, DateTimeKind.Utc).AddTicks(4563), "Search google to have better solution", "Dicussion", null, "dicussion", null, "dicussion", "Readonly", null }
                });

            migrationBuilder.InsertData(
                table: "social_tag",
                columns: new[] { "id", "created_timestamp", "describe", "last_modified_timestamp", "name", "status", "tag" },
                values: new object[,]
                {
                    { 20L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5201), "Vue Router I18n is a localization library for Vue Router. It is maintained by a community of individual developers and companies.", null, "Vue Router I18n", "Readonly", "vue-router-i18n" },
                    { 17L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5168), "Vuex is a state management pattern and library for Vue.js applications. It is maintained by a community of individual developers and companies.", null, "Vuex", "Readonly", "vuex" },
                    { 18L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5181), "Vue I18n is a localization library for Vue.js. It is maintained by a community of individual developers and companies.", null, "Vue I18n", "Readonly", "vue-i18n" },
                    { 19L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5191), "Vue Resource is a REST client for Vue.js. It is maintained by a community of individual developers and companies.", null, "Vue Resource", "Readonly", "vue-resource" },
                    { 21L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5210), ".NET is a programming language and runtime environment developed by Microsoft. It is maintained by a community of individual developers and companies.", null, ".NET", "Readonly", "dotnet" },
                    { 27L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5270), "React Router DOM is a routing library for React. It is maintained by a community of individual developers and companies.", null, "React Router DOM", "Readonly", "react-router-dom" },
                    { 23L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5231), "ASP.NET is a web application framework developed by Microsoft. It is maintained by a community of individual developers and companies.", null, "ASP.NET", "Readonly", "aspnet" },
                    { 24L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5241), "ASP.NET Core is a web application framework developed by Microsoft. It is maintained by a community of individual developers and companies.", null, "ASP.NET Core", "Readonly", "aspnet-core" },
                    { 25L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5250), "Next.js is a JavaScript framework for building web applications. It is maintained by a community of individual developers and companies.", null, "Next.js", "Readonly", "nextjs" },
                    { 26L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5260), "React Router is a routing library for React. It is maintained by a community of individual developers and companies.", null, "React Router", "Readonly", "react-router" },
                    { 16L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5157), "Vue Router is a routing library for Vue.js. It is maintained by a community of individual developers and companies.", null, "Vue Router", "Readonly", "vue-router" },
                    { 22L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5221), "C# is a programming language and runtime environment developed by Microsoft. It is maintained by a community of individual developers and companies.", null, "CSharp", "Readonly", "csharp" },
                    { 15L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5147), "Bootstrap Vue is a Vue.js wrapper for Bootstrap. It is maintained by a community of individual developers and companies.", null, "Bootstrap Vue", "Readonly", "bootstrap-vue" },
                    { 3L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(4988), "Vue.js is an open-source JavaScript framework for building user interfaces. It is maintained by a community of individual developers and companies. Vue can be used as a base in the development of single-page or mobile applications.", null, "Vue", "Readonly", "vue" },
                    { 13L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5103), "Material Design is a design language developed by Google. It is used to create a consistent and beautiful user experience across all products on Android, iOS, and the web.", null, "Material Design", "Readonly", "material-design" },
                    { 1L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(4804), "Angular is a TypeScript-based open-source web application platform led by the Angular Team at Google and by a community of individuals and corporations. Angular is a complete rewrite from the same team that built AngularJS.", null, "Angular", "Readonly", "angular" },
                    { 2L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(4975), "React is a JavaScript library for building user interfaces. It is maintained by Facebook and a community of individual developers and companies. React can be used as a base in the development of single-page or mobile applications.", null, "React", "Readonly", "react" },
                    { 4L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(4998), "Angular CLI is a command-line interface for the Angular development platform. It is used to create and manage projects for the Angular framework.", null, "Angular CLI", "Readonly", "angular-cli" },
                    { 14L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5114), "Material Icons is a set of open source icons for use in web and mobile applications. It is maintained by a community of individual developers and companies.", null, "Material Icons", "Readonly", "material-icons" },
                    { 6L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5027), "Vue CLI is a command-line interface for the Vue.js development platform. It is used to create and manage projects for the Vue framework.", null, "Vue CLI", "Readonly", "vue-cli" },
                    { 5L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5008), "React Native is a framework for building native apps using React. It is maintained by Facebook and a community of individual developers and companies.", null, "React Native", "Readonly", "react-native" },
                    { 8L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5049), "Gulp is a streaming build system. It is maintained by a community of individual developers and companies.", null, "Gulp", "Readonly", "gulp" },
                    { 9L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5061), "Sass is a stylesheet language that is interpreted into Cascading Style Sheets (CSS). It is maintained by a community of individual developers and companies.", null, "Sass", "Readonly", "sass" },
                    { 10L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5074), "Less is a stylesheet language that is interpreted into Cascading Style Sheets (CSS). It is maintained by a community of individual developers and companies.", null, "Less", "Readonly", "less" },
                    { 11L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5083), "Bootstrap is a free and open-source front-end web framework for designing websites and web applications. It is maintained by a community of individual developers and companies.", null, "Bootstrap", "Readonly", "bootstrap" },
                    { 12L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5092), "Material-UI is a React component library that enables you to create beautiful, high-fidelity, mobile-first experiences. It is maintained by a community of individual developers and companies.", null, "Material-UI", "Readonly", "material-ui" },
                    { 7L, new DateTime(2022, 4, 26, 19, 20, 54, 665, DateTimeKind.Utc).AddTicks(5037), "Webpack is a module bundler that packs multiple modules with dependencies into a single module. It is maintained by a community of individual developers and companies.", null, "Webpack", "Readonly", "webpack" }
                });

            migrationBuilder.InsertData(
                table: "social_user_right",
                columns: new[] { "id", "describe", "display_name", "right_name", "status" },
                values: new object[,]
                {
                    { 4, "Can create, interactive report.", "Upload", "upload", "Readonly" },
                    { 1, "Can create, interactive posts.", "Post", "post", "Readonly" },
                    { 2, "Can create, interactive comment.", "Comment", "comment", "Readonly" },
                    { 3, "Can create, interactive report.", "Report", "report", "Readonly" }
                });

            migrationBuilder.InsertData(
                table: "social_user_role",
                columns: new[] { "id", "describe", "display_name", "priority", "role_name", "status" },
                values: new object[] { 1, "Normal user", "User", false, "user", "Readonly" });

            migrationBuilder.InsertData(
                table: "admin_user_role_detail",
                columns: new[] { "right_id", "role_id", "actions" },
                values: new object[,]
                {
                    { 1, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 12, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 11, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 10, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 8, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 7, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 9, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 5, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 4, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 3, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 2, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 6, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" }
                });

            migrationBuilder.InsertData(
                table: "admin_user_role_of_user",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { 1, new Guid("1afc27e9-85c3-4e48-89ab-dd997621ab32") });

            migrationBuilder.InsertData(
                table: "social_user_role_detail",
                columns: new[] { "right_id", "role_id", "actions" },
                values: new object[,]
                {
                    { 3, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 1, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 2, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 4, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_audit_log_search_vector",
                table: "admin_audit_log",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_admin_audit_log_table",
                table: "admin_audit_log",
                column: "table");

            migrationBuilder.CreateIndex(
                name: "IX_admin_audit_log_user_id",
                table: "admin_audit_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_admin_base_config_config_key",
                table: "admin_base_config",
                column: "config_key",
                unique: true,
                filter: "(status) <> 'Disabled'");

            migrationBuilder.CreateIndex(
                name: "IX_admin_user_user_name_email",
                table: "admin_user",
                columns: new[] { "user_name", "email" },
                unique: true,
                filter: "(status) <> 'Deleted'");

            migrationBuilder.CreateIndex(
                name: "IX_admin_user_right_right_name",
                table: "admin_user_right",
                column: "right_name",
                unique: true,
                filter: "(status) <> 'Disabled'");

            migrationBuilder.CreateIndex(
                name: "IX_admin_user_role_role_name",
                table: "admin_user_role",
                column: "role_name",
                unique: true,
                filter: "(status) <> 'Disabled'");

            migrationBuilder.CreateIndex(
                name: "IX_admin_user_role_detail_right_id",
                table: "admin_user_role_detail",
                column: "right_id");

            migrationBuilder.CreateIndex(
                name: "IX_admin_user_role_of_user_role_id",
                table: "admin_user_role_of_user",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_admin_user_token_user_id",
                table: "session_admin_user",
                columns: new[] { "session_token", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_admin_user_user_id",
                table: "session_admin_user",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_social_user_token_user_id",
                table: "session_social_user",
                columns: new[] { "session_token", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_social_user_user_id",
                table: "session_social_user",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_audit_log_search_vector",
                table: "social_audit_log",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_social_audit_log_table",
                table: "social_audit_log",
                column: "table");

            migrationBuilder.CreateIndex(
                name: "IX_social_audit_log_user_id",
                table: "social_audit_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_category_parent_id",
                table: "social_category",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_category_search_vector",
                table: "social_category",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_social_category_slug",
                table: "social_category",
                column: "slug",
                unique: true,
                filter: "(status) <> 'Disabled'");

            migrationBuilder.CreateIndex(
                name: "IX_social_comment_owner",
                table: "social_comment",
                column: "owner");

            migrationBuilder.CreateIndex(
                name: "IX_social_comment_parent_id",
                table: "social_comment",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_comment_post_id",
                table: "social_comment",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_comment_search_vector",
                table: "social_comment",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_social_notification_comment_id",
                table: "social_notification",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_notification_post_id",
                table: "social_notification",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_notification_user_id",
                table: "social_notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_notification_user_id_des",
                table: "social_notification",
                column: "user_id_des");

            migrationBuilder.CreateIndex(
                name: "IX_social_post_owner",
                table: "social_post",
                column: "owner");

            migrationBuilder.CreateIndex(
                name: "IX_social_post_search_vector",
                table: "social_post",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_social_post_slug",
                table: "social_post",
                column: "slug",
                unique: true,
                filter: "((status = 'Approved' OR status = 'Private') AND (slug <> ''))");

            migrationBuilder.CreateIndex(
                name: "IX_social_post_category_category_id",
                table: "social_post_category",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_post_tag_tag_id",
                table: "social_post_tag",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_report_comment_id",
                table: "social_report",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_report_post_id",
                table: "social_report",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_report_search_vector",
                table: "social_report",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_social_report_user_id",
                table: "social_report",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_tag_tag",
                table: "social_tag",
                column: "tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_social_user_search_vector",
                table: "social_user",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_user_name_email",
                table: "social_user",
                columns: new[] { "user_name", "email" },
                unique: true,
                filter: "(status) <> 'Deleted'");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_action_with_category_category_id",
                table: "social_user_action_with_category",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_action_with_comment_comment_id",
                table: "social_user_action_with_comment",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_action_with_post_post_id",
                table: "social_user_action_with_post",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_action_with_tag_tag_id",
                table: "social_user_action_with_tag",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_action_with_user_user_id_des",
                table: "social_user_action_with_user",
                column: "user_id_des");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_audit_log_amin_user_id",
                table: "social_user_audit_log",
                column: "amin_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_audit_log_search_vector",
                table: "social_user_audit_log",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_audit_log_table",
                table: "social_user_audit_log",
                column: "table");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_audit_log_user_id",
                table: "social_user_audit_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_right_right_name",
                table: "social_user_right",
                column: "right_name",
                unique: true,
                filter: "(status) <> 'Disabled'");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_role_role_name",
                table: "social_user_role",
                column: "role_name",
                unique: true,
                filter: "(status) <> 'Disabled'");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_role_detail_right_id",
                table: "social_user_role_detail",
                column: "right_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_role_of_user_role_id",
                table: "social_user_role_of_user",
                column: "role_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_audit_log");

            migrationBuilder.DropTable(
                name: "admin_base_config");

            migrationBuilder.DropTable(
                name: "admin_user_role_detail");

            migrationBuilder.DropTable(
                name: "admin_user_role_of_user");

            migrationBuilder.DropTable(
                name: "session_admin_user");

            migrationBuilder.DropTable(
                name: "session_social_user");

            migrationBuilder.DropTable(
                name: "social_audit_log");

            migrationBuilder.DropTable(
                name: "social_notification");

            migrationBuilder.DropTable(
                name: "social_post_category");

            migrationBuilder.DropTable(
                name: "social_post_tag");

            migrationBuilder.DropTable(
                name: "social_report");

            migrationBuilder.DropTable(
                name: "social_user_action_with_category");

            migrationBuilder.DropTable(
                name: "social_user_action_with_comment");

            migrationBuilder.DropTable(
                name: "social_user_action_with_post");

            migrationBuilder.DropTable(
                name: "social_user_action_with_tag");

            migrationBuilder.DropTable(
                name: "social_user_action_with_user");

            migrationBuilder.DropTable(
                name: "social_user_audit_log");

            migrationBuilder.DropTable(
                name: "social_user_role_detail");

            migrationBuilder.DropTable(
                name: "social_user_role_of_user");

            migrationBuilder.DropTable(
                name: "admin_user_right");

            migrationBuilder.DropTable(
                name: "admin_user_role");

            migrationBuilder.DropTable(
                name: "social_category");

            migrationBuilder.DropTable(
                name: "social_comment");

            migrationBuilder.DropTable(
                name: "social_tag");

            migrationBuilder.DropTable(
                name: "admin_user");

            migrationBuilder.DropTable(
                name: "social_user_right");

            migrationBuilder.DropTable(
                name: "social_user_role");

            migrationBuilder.DropTable(
                name: "social_post");

            migrationBuilder.DropTable(
                name: "social_user");
        }
    }
}

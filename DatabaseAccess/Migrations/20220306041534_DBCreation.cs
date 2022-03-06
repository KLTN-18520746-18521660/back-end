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
                    user = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "table", "table_key", "old_value", "new_value", "user" })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_audit_log", x => x.id);
                });

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
                    table.CheckConstraint("CK_admin_base_config_status_valid_value", "status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly'");
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
                    settings = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'"),
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
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Enabled'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_user_role", x => x.id);
                    table.CheckConstraint("CK_admin_user_role_status_valid_value", "status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly'");
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
                    user = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "table", "table_key", "old_value", "new_value", "user" })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_audit_log", x => x.id);
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
                    describe = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    sex = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    country = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    city = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    province = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    verified_email = table.Column<bool>(type: "boolean", nullable: false),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Activated'"),
                    settings = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'"),
                    ranks = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'"),
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
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Enabled'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_role", x => x.id);
                    table.CheckConstraint("CK_social_user_role_status_valid_value", "status = 'Enabled' OR status = 'Disabled' OR status = 'Readonly'");
                });

            migrationBuilder.CreateTable(
                name: "session_admin_user",
                columns: table => new
                {
                    session_token = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    saved = table.Column<bool>(type: "boolean", nullable: false),
                    data = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'"),
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
                name: "admin_user_role_detail",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    right_id = table.Column<int>(type: "integer", nullable: false),
                    actions = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'")
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
                    data = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'"),
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
                name: "social_notification",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Sent'"),
                    content = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'"),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    last_modified_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_notification", x => x.id);
                    table.CheckConstraint("CK_social_notification_status_valid_value", "status = 'Sent' OR status = 'Read' OR status = 'Deleted'");
                    table.CheckConstraint("CK_social_notification_last_modified_timestamp_valid_value", "(last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)");
                    table.ForeignKey(
                        name: "FK_social_notification_user_id",
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
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Pending'"),
                    content_search = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "content_search", "title" }),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    last_modified_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_post", x => x.id);
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
                    actions = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'")
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
                    actions = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'")
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
                    actions = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'")
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
                name: "social_user_role_detail",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    right_id = table.Column<int>(type: "integer", nullable: false),
                    actions = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'")
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
                    actions = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'")
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
                name: "social_report",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<long>(type: "bigint", nullable: true),
                    comment_id = table.Column<long>(type: "bigint", nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValueSql: "'Pending'"),
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
                    actions = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'")
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
                    { 1, "AdminUserLoginConfig", "Enabled", "{\r\n  \"number\": 5,\r\n  \"time\": 5,\r\n  \"lock\": 360\r\n}" },
                    { 2, "SocialUserLoginConfig", "Enabled", "{\r\n  \"number\": 5,\r\n  \"time\": 5,\r\n  \"lock\": 360\r\n}" },
                    { 3, "SessionAdminUserConfig", "Enabled", "{\r\n  \"expiry_time\": 5,\r\n  \"extension_time\": 5\r\n}" },
                    { 4, "SessionSocialUserConfig", "Enabled", "{\r\n  \"expiry_time\": 5,\r\n  \"extension_time\": 5\r\n}" }
                });

            migrationBuilder.InsertData(
                table: "admin_user",
                columns: new[] { "id", "created_timestamp", "display_name", "email", "last_access_timestamp", "salt", "settings", "status", "password", "user_name" },
                values: new object[] { new Guid("aef445ea-10f2-469a-bf08-0425f403030c"), new DateTime(2022, 3, 6, 4, 15, 33, 32, DateTimeKind.Utc).AddTicks(4268), "Administrator", "admin@admin", null, "3c6efe21", "{}", "Readonly", "0AFC343755FA7D0BA5B9D31A5062E165", "admin" });

            migrationBuilder.InsertData(
                table: "admin_user_right",
                columns: new[] { "id", "describe", "display_name", "right_name", "status" },
                values: new object[,]
                {
                    { 9, "Add, block, unblock, delete AdminUser.", "Admin User", "admin_user", "Readonly" },
                    { 8, "Block, unblock SocialUser", "Social User", "social_user", "Readonly" },
                    { 7, "Configure security of Server.", "Security", "security", "Readonly" },
                    { 6, "Delete comment. See report about comment.", "Comment", "comment", "Readonly" },
                    { 10, "See and tracking log file.", "Log", "log", "Readonly" },
                    { 4, "Add, create, disable tag.", "Tag", "tag", "Readonly" },
                    { 3, "Add, create, disable topics", "Topic", "topic", "Readonly" },
                    { 2, "Add, create, disable category.", "Category", "category", "Readonly" },
                    { 1, "Can access Homepage and see statistic.", "Dashboard", "dashboard", "Readonly" },
                    { 5, "Review, accept, reject post. See report about post.", "Post", "post", "Readonly" }
                });

            migrationBuilder.InsertData(
                table: "admin_user_role",
                columns: new[] { "id", "describe", "display_name", "role_name", "status" },
                values: new object[] { 1, "Administrator", "Administrator", "admin", "Readonly" });

            migrationBuilder.InsertData(
                table: "social_category",
                columns: new[] { "id", "created_timestamp", "describe", "display_name", "last_modified_timestamp", "name", "parent_id", "slug", "status", "thumbnail" },
                values: new object[,]
                {
                    { 5L, new DateTime(2022, 3, 6, 4, 15, 33, 67, DateTimeKind.Utc).AddTicks(8314), "Life die have number", "Left", null, "left", null, "left", "Readonly", null },
                    { 3L, new DateTime(2022, 3, 6, 4, 15, 33, 67, DateTimeKind.Utc).AddTicks(8300), "Search google to have better solution", "Dicussion", null, "dicussion", null, "dicussion", "Readonly", null },
                    { 4L, new DateTime(2022, 3, 6, 4, 15, 33, 67, DateTimeKind.Utc).AddTicks(8305), "Nothing in here", "Blog", null, "blog", null, "blog", "Readonly", null },
                    { 1L, new DateTime(2022, 3, 6, 4, 15, 33, 67, DateTimeKind.Utc).AddTicks(8249), "This not a bug this a feature", "Technology", null, "technology", null, "technology", "Readonly", null },
                    { 2L, new DateTime(2022, 3, 6, 4, 15, 33, 67, DateTimeKind.Utc).AddTicks(8294), "Do not click to this", "Developer", null, "developer", null, "developer", "Readonly", null }
                });

            migrationBuilder.InsertData(
                table: "social_tag",
                columns: new[] { "id", "created_timestamp", "describe", "last_modified_timestamp", "status", "tag" },
                values: new object[,]
                {
                    { 1L, new DateTime(2022, 3, 6, 4, 15, 33, 90, DateTimeKind.Utc).AddTicks(8159), "Angular", null, "Readonly", "#angular" },
                    { 2L, new DateTime(2022, 3, 6, 4, 15, 33, 90, DateTimeKind.Utc).AddTicks(8210), "Something is not thing", null, "Readonly", "#life-die-have-number" },
                    { 3L, new DateTime(2022, 3, 6, 4, 15, 33, 90, DateTimeKind.Utc).AddTicks(8216), "Dot not choose this tag", null, "Readonly", "#develop" },
                    { 4L, new DateTime(2022, 3, 6, 4, 15, 33, 90, DateTimeKind.Utc).AddTicks(8220), "Nothing in here", null, "Readonly", "#nothing" },
                    { 5L, new DateTime(2022, 3, 6, 4, 15, 33, 90, DateTimeKind.Utc).AddTicks(8225), "hi hi", null, "Readonly", "#hihi" }
                });

            migrationBuilder.InsertData(
                table: "social_user_right",
                columns: new[] { "id", "describe", "display_name", "right_name", "status" },
                values: new object[,]
                {
                    { 3, "Can create, interactive report.", "Report", "report", "Readonly" },
                    { 1, "Can create, interactive posts.", "Post", "post", "Readonly" },
                    { 2, "Can create, interactive comment.", "Comment", "comment", "Readonly" }
                });

            migrationBuilder.InsertData(
                table: "social_user_role",
                columns: new[] { "id", "describe", "display_name", "role_name", "status" },
                values: new object[] { 1, "Normal user", "User", "user", "Readonly" });

            migrationBuilder.InsertData(
                table: "admin_user_role_detail",
                columns: new[] { "right_id", "role_id", "actions" },
                values: new object[,]
                {
                    { 1, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 2, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 3, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 4, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 5, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 6, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 7, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 8, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 9, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 10, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" }
                });

            migrationBuilder.InsertData(
                table: "admin_user_role_of_user",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { 1, new Guid("aef445ea-10f2-469a-bf08-0425f403030c") });

            migrationBuilder.InsertData(
                table: "social_user_role_detail",
                columns: new[] { "right_id", "role_id", "actions" },
                values: new object[,]
                {
                    { 1, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 2, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" },
                    { 3, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_audit_log_search_vector",
                table: "admin_audit_log",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

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
                name: "IX_social_notification_user_id",
                table: "social_notification",
                column: "user_id");

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
                filter: "(status) <> 'Deleted'");

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
                unique: true,
                filter: "(status) <> 'Disabled'");

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
                name: "social_user_role_detail");

            migrationBuilder.DropTable(
                name: "social_user_role_of_user");

            migrationBuilder.DropTable(
                name: "admin_user_right");

            migrationBuilder.DropTable(
                name: "admin_user_role");

            migrationBuilder.DropTable(
                name: "admin_user");

            migrationBuilder.DropTable(
                name: "social_category");

            migrationBuilder.DropTable(
                name: "social_comment");

            migrationBuilder.DropTable(
                name: "social_tag");

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

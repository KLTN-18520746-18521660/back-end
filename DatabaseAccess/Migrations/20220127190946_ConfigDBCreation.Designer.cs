﻿// <auto-generated />
using System;
using DatabaseAccess.Contexts.ConfigDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

namespace DatabaseAccess.Migrations
{
    [DbContext(typeof(DBContext))]
    [Migration("20220127190946_ConfigDBCreation")]
    partial class ConfigDBCreation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("DatabaseAccess.Contexts.ConfigDB.Models.AdminUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("UUID")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("CreatedTimestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMPTZ")
                        .HasColumnName("created_timestamp")
                        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("display_name");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("VARCHAR(320)")
                        .HasColumnName("email");

                    b.Property<DateTime?>("LastAccessTimestamp")
                        .HasColumnType("TIMESTAMPTZ")
                        .HasColumnName("last_access_timestamp");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("VARCHAR(32)")
                        .HasColumnName("password");

                    b.Property<string>("RolesStr")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("JSON")
                        .HasDefaultValue("[]")
                        .HasColumnName("roles");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("VARCHAR(8)")
                        .HasColumnName("salt")
                        .HasDefaultValueSql("SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)");

                    b.Property<string>("SettingsStr")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("JSON")
                        .HasDefaultValue("{}")
                        .HasColumnName("settings");

                    b.Property<string>("StatusStr")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("VARCHAR(20)")
                        .HasDefaultValue("Not Activated")
                        .HasColumnName("status");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("user_name");

                    b.HasKey("Id");

                    b.HasIndex("UserName", "Email")
                        .IsUnique()
                        .HasFilter("status != 'Deleted'");

                    b.ToTable("admin_user");

                    b.HasCheckConstraint("CK_Status_Valid_Value", "status = 'Deleted' OR status = 'Not Activated' OR status = 'Activated' OR status = 'Readonly'");

                    b.HasCheckConstraint("CK_LastAccessTimestamp_Valid_Value", "(last_access_timestamp IS NULL AND status = 'Not Activated') OR (status <> 'Not Activated')");

                    b.HasData(
                        new
                        {
                            Id = new Guid("58229419-2000-4923-96e9-1cdc856e9d67"),
                            CreatedTimestamp = new DateTime(2022, 1, 27, 19, 9, 44, 912, DateTimeKind.Utc).AddTicks(9042),
                            DisplayName = "Administrator",
                            Email = "admin@admin",
                            Password = "06AFA2F8E79FFE9D57EFBD3EF4D9C7FF",
                            RolesStr = "[]",
                            Salt = "2638ead0",
                            SettingsStr = "{}",
                            StatusStr = "Readonly",
                            UserName = "admin"
                        });
                });

            modelBuilder.Entity("DatabaseAccess.Contexts.ConfigDB.Models.AdminUserRight", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<string>("Describe")
                        .IsRequired()
                        .HasColumnType("VARCHAR(150)")
                        .HasColumnName("describe");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("display_name");

                    b.Property<string>("RightName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("right_name");

                    b.Property<string>("StatusStr")
                        .IsRequired()
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.HasIndex("RightName")
                        .IsUnique()
                        .HasFilter("status != 'Disabled'");

                    b.ToTable("admin_user_right");

                    b.HasCheckConstraint("CK_Status_Valid_Value", "status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly'");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Describe = "Can access Homepage and see statistic",
                            DisplayName = "Dashboard",
                            RightName = "dashboard",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 2,
                            Describe = "Add, create, disable category",
                            DisplayName = "Category",
                            RightName = "category",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 3,
                            Describe = "Add, create, disable topics",
                            DisplayName = "Topic",
                            RightName = "topic",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 4,
                            Describe = "Add, create, disable type of post.",
                            DisplayName = "Type of post",
                            RightName = "type_of_post",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 5,
                            Describe = "Review, accept, deny post. See report about post.",
                            DisplayName = "Post",
                            RightName = "post",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 6,
                            Describe = "Delete comment. See report about comment.",
                            DisplayName = "Comment",
                            RightName = "comment",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 7,
                            Describe = "Configure security of Server.",
                            DisplayName = "Security",
                            RightName = "security",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 8,
                            Describe = "Deactivate, activate SocialUser",
                            DisplayName = "Social User",
                            RightName = "social_user",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 9,
                            Describe = "Add, deactivate, activate, delete AdminUser.",
                            DisplayName = "Admin User",
                            RightName = "admin_user",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 10,
                            Describe = "See and tracking log file.",
                            DisplayName = "Log",
                            RightName = "log",
                            StatusStr = "Readonly"
                        });
                });

            modelBuilder.Entity("DatabaseAccess.Contexts.ConfigDB.Models.AdminUserRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<string>("Describe")
                        .IsRequired()
                        .HasColumnType("VARCHAR(150)")
                        .HasColumnName("describe");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("display_name");

                    b.Property<string>("RightsStr")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("JSON")
                        .HasDefaultValue("[]")
                        .HasColumnName("rights");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("role_name");

                    b.Property<string>("StatusStr")
                        .IsRequired()
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.HasIndex("RoleName")
                        .IsUnique()
                        .HasFilter("status != 'Disabled'");

                    b.ToTable("admin_user_role");

                    b.HasCheckConstraint("CK_Status_Valid_Value", "status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly'");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Describe = "Administrator",
                            DisplayName = "Administrator",
                            RightsStr = "{\"dashboard\":[\"write\",\"read\"],\"category\":[\"write\",\"read\"],\"topic\":[\"write\",\"read\"],\"type_of_post\":[\"write\",\"read\"],\"post\":[\"write\",\"read\"],\"comment\":[\"write\",\"read\"],\"security\":[\"write\",\"read\"],\"social_user\":[\"write\",\"read\"],\"admin_user\":[\"write\",\"read\"],\"log\":[\"write\",\"read\"]}",
                            RoleName = "admin",
                            StatusStr = "Readonly"
                        });
                });

            modelBuilder.Entity("DatabaseAccess.Contexts.ConfigDB.Models.BaseConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<string>("ConfigKey")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("config_key");

                    b.Property<string>("StatusStr")
                        .IsRequired()
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");

                    b.Property<string>("ValueStr")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("JSON")
                        .HasDefaultValue("{}")
                        .HasColumnName("value");

                    b.HasKey("Id");

                    b.HasIndex("ConfigKey")
                        .IsUnique()
                        .HasFilter("status != 'Disabled'");

                    b.ToTable("base_config");

                    b.HasCheckConstraint("CK_Status_Valid_Value", "status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly'");
                });

            modelBuilder.Entity("DatabaseAccess.Contexts.ConfigDB.Models.ConfigAuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("action");

                    b.Property<string>("NewValueStr")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("new_value");

                    b.Property<string>("OldValueStr")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("old_value");

                    b.Property<NpgsqlTsVector>("SearchVector")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("tsvector")
                        .HasColumnName("search_vector")
                        .HasAnnotation("Npgsql:TsVectorConfig", "english")
                        .HasAnnotation("Npgsql:TsVectorProperties", new[] { "Table", "TableKey", "OldValueStr", "NewValueStr", "User" });

                    b.Property<string>("Table")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("table");

                    b.Property<string>("TableKey")
                        .IsRequired()
                        .HasColumnType("VARCHAR(100)")
                        .HasColumnName("table_key");

                    b.Property<DateTime>("Timestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMPTZ")
                        .HasColumnName("timestamp")
                        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                    b.Property<string>("User")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("user");

                    b.HasKey("Id");

                    b.HasIndex("SearchVector")
                        .HasMethod("GIN");

                    b.ToTable("config_audit_log");
                });

            modelBuilder.Entity("DatabaseAccess.Contexts.ConfigDB.Models.SocialAuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("action");

                    b.Property<string>("NewValueStr")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("new_value");

                    b.Property<string>("OldValueStr")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("old_value");

                    b.Property<NpgsqlTsVector>("SearchVector")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("tsvector")
                        .HasColumnName("search_vector")
                        .HasAnnotation("Npgsql:TsVectorConfig", "english")
                        .HasAnnotation("Npgsql:TsVectorProperties", new[] { "Table", "TableKey", "OldValueStr", "NewValueStr", "User" });

                    b.Property<string>("Table")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("table");

                    b.Property<string>("TableKey")
                        .IsRequired()
                        .HasColumnType("VARCHAR(100)")
                        .HasColumnName("table_key");

                    b.Property<DateTime>("Timestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMPTZ")
                        .HasColumnName("timestamp")
                        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                    b.Property<string>("User")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("user");

                    b.HasKey("Id");

                    b.HasIndex("SearchVector")
                        .HasMethod("GIN");

                    b.ToTable("social_audit_log");
                });

            modelBuilder.Entity("DatabaseAccess.Contexts.ConfigDB.Models.SocialUserRight", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<string>("Describe")
                        .IsRequired()
                        .HasColumnType("VARCHAR(150)")
                        .HasColumnName("describe");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("display_name");

                    b.Property<string>("RightName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("right_name");

                    b.Property<string>("StatusStr")
                        .IsRequired()
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.HasIndex("RightName")
                        .IsUnique()
                        .HasFilter("status != 'Disabled'");

                    b.ToTable("social_user_right");

                    b.HasCheckConstraint("CK_Status_Valid_Value", "status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly'");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Describe = "Can read, write post.",
                            DisplayName = "Post",
                            RightName = "post",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 2,
                            Describe = "Can read, write comment.",
                            DisplayName = "Comment",
                            RightName = "comment",
                            StatusStr = "Readonly"
                        });
                });

            modelBuilder.Entity("DatabaseAccess.Contexts.ConfigDB.Models.SocialUserRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<string>("Describe")
                        .IsRequired()
                        .HasColumnType("VARCHAR(150)")
                        .HasColumnName("describe");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("display_name");

                    b.Property<string>("RightsStr")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("JSON")
                        .HasDefaultValue("[]")
                        .HasColumnName("rights");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)")
                        .HasColumnName("role_name");

                    b.Property<string>("StatusStr")
                        .IsRequired()
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.HasIndex("RoleName")
                        .IsUnique()
                        .HasFilter("status != 'Disabled'");

                    b.ToTable("social_user_role");

                    b.HasCheckConstraint("CK_Status_Valid_Value", "status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly'");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Describe = "Post - Read",
                            DisplayName = "Post - Read",
                            RightsStr = "{\"post\":[\"read\"]}",
                            RoleName = "post_read",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 2,
                            Describe = "Post - Write",
                            DisplayName = "Post - Write",
                            RightsStr = "{\"post\":[\"write\"]}",
                            RoleName = "post_write",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 3,
                            Describe = "Comment - Read",
                            DisplayName = "Comment - Read",
                            RightsStr = "{\"comment\":[\"read\"]}",
                            RoleName = "comment_read",
                            StatusStr = "Readonly"
                        },
                        new
                        {
                            Id = 4,
                            Describe = "Comment - Write",
                            DisplayName = "Comment - Write",
                            RightsStr = "{\"comment\":[\"write\"]}",
                            RoleName = "comment_write",
                            StatusStr = "Readonly"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}

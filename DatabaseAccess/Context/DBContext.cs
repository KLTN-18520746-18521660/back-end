using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using DatabaseAccess.Common;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// using System.Text.Json;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Actions;
using DatabaseAccess.Common.Status;
using System.Collections;


#nullable disable

namespace DatabaseAccess.Context
{
    public partial class DBContext : DbContext
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
        public virtual DbSet<AdminBaseConfig> AdminBaseConfigs { get; set; }
        public virtual DbSet<AdminUser> AdminUsers { get; set; }
        public virtual DbSet<AdminUserRight> AdminUserRights { get; set; }
        public virtual DbSet<AdminUserRole> AdminUserRoles { get; set; }
        public virtual DbSet<SessionAdminUser> SessionAdminUsers { get; set; }
        public virtual DbSet<SessionSocialUser> SessionSocialUsers { get; set; }
        public virtual DbSet<SocialAuditLog> SocialAuditLogs { get; set; }
        public virtual DbSet<SocialCategory> SocialCategories { get; set; }
        public virtual DbSet<SocialComment> SocialComments { get; set; }
        public virtual DbSet<SocialNotification> SocialNotifications { get; set; }
        public virtual DbSet<SocialPost> SocialPosts { get; set; }
        public virtual DbSet<SocialPostCategory> SocialPostCategories { get; set; }
        public virtual DbSet<SocialPostTag> SocialPostTags { get; set; }
        public virtual DbSet<SocialReport> SocialReports { get; set; }
        public virtual DbSet<SocialTag> SocialTags { get; set; }
        public virtual DbSet<SocialUser> SocialUsers { get; set; }
        public virtual DbSet<SocialUserActionWithCategory> SocialUserActionWithCategories { get; set; }
        public virtual DbSet<SocialUserActionWithComment> SocialUserActionWithComments { get; set; }
        public virtual DbSet<SocialUserActionWithPost> SocialUserActionWithPosts { get; set; }
        public virtual DbSet<SocialUserActionWithTag> SocialUserActionWithTags { get; set; }
        public virtual DbSet<SocialUserActionWithUser> SocialUserActionWithUsers { get; set; }
        public virtual DbSet<SocialUserRight> SocialUserRights { get; set; }
        public virtual DbSet<SocialUserRole> SocialUserRoles { get; set; }

        #region Configure Context
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                .UseLazyLoadingProxies()
                .UseNpgsql(
                    BaseConfigurationDB.GetConnectStringToDB(),
                    npgsqlOptionsAction: o => {
                        o.SetPostgresVersion(14, 1);
                    }
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");
            ConfigureEnityAdminAuditLog(modelBuilder);
            ConfigureEnityAdminBaseConfig(modelBuilder);
            ConfigureEnityAdminUser(modelBuilder);
            ConfigureEnityAdminUserRight(modelBuilder);
            ConfigureEnityAdminUserRole(modelBuilder);
            ConfigureEnityAdminUserRoleDetail(modelBuilder);
            ConfigureEnityAdminUserRoleOfUser(modelBuilder);
            ConfigureEnitySessionAdminUser(modelBuilder);
            ConfigureEnitySessionSocialUser(modelBuilder);
            ConfigureEnitySocialAuditLog(modelBuilder);
            ConfigureEnitySocialCategory(modelBuilder);
            ConfigureEnitySocialComment(modelBuilder);
            ConfigureEnitySocialNotification(modelBuilder);
            ConfigureEnitySocialPost(modelBuilder);
            ConfigureEnitySocialPostCategory(modelBuilder);
            ConfigureEnitySocialPostTag(modelBuilder);
            ConfigureEnitySocialReport(modelBuilder);
            ConfigureEnitySocialTag(modelBuilder);
            ConfigureEnitySocialUser(modelBuilder);
            ConfigureEnitySocialUserActionWithCategory(modelBuilder);
            ConfigureEnitySocialUserActionWithComment(modelBuilder);
            ConfigureEnitySocialUserActionWithPost(modelBuilder);
            ConfigureEnitySocialUserActionWithTag(modelBuilder);
            ConfigureEnitySocialUserActionWithUser(modelBuilder);
            ConfigureEnitySocialUserRight(modelBuilder);
            ConfigureEnitySocialUserRole(modelBuilder);
            ConfigureEnitySocialUserRoleDetail(modelBuilder);
            ConfigureEnitySocialUserRoleOfUser(modelBuilder);

            OnModelCreatingPartial(modelBuilder);
        }
        #region Table AdminAuditLog
        private static void ConfigureEnityAdminAuditLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminAuditLog>(entity =>
            {
                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                //entity.HasIndex(e => e.SearchVector, "IX_admin_audit_log_search_vector")
                //    .HasMethod("gin");
                //entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, (((((((((\"table\")::text || ' '::text) || (table_key)::text) || ' '::text) || old_value) || ' '::text) || new_value) || ' '::text) || (\"user\")::text))", true);
                entity
                    .HasGeneratedTsVectorColumn(
                        e => e.SearchVector,
                        "english",
                        e => new { e.Table, e.TableKey, e.OldValueStr, e.NewValueStr, e.User }
                    )
                    .HasIndex(e => e.SearchVector)
                    .HasMethod("GIN");
            });
        }
        #endregion
        #region Table AdminBaseConfig
        private static void ConfigureEnityAdminBaseConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminBaseConfig>(entity =>
            {
                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.StatusStr)
                    .HasDefaultValueSql($"'{ BaseStatus.StatusToString(AdminBaseConfigStatus.Enabled, EntityStatus.AdminBaseConfigStatus) }'");
                entity.Property(e => e.ValueStr).HasDefaultValueSql("'{}'");

                entity.HasIndex(e => e.ConfigKey, "IX_admin_base_config_config_key")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ BaseStatus.StatusToString(AdminBaseConfigStatus.Disabled, EntityStatus.AdminBaseConfigStatus) }'");
                entity
                    .HasCheckConstraint(
                        "CK_admin_base_config_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            BaseStatus.StatusToString(AdminBaseConfigStatus.Disabled, EntityStatus.AdminBaseConfigStatus),
                            BaseStatus.StatusToString(AdminBaseConfigStatus.Enabled, EntityStatus.AdminBaseConfigStatus),
                            BaseStatus.StatusToString(AdminBaseConfigStatus.Readonly, EntityStatus.AdminBaseConfigStatus)
                        )
                    );
                entity.HasData(AdminBaseConfig.GetDefaultData());
            });
        }
        #endregion
        #region Table AdminUser
        private static void ConfigureEnityAdminUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminUser>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedTimestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.Salt)
                    .HasDefaultValueSql("SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)");
                entity.Property(e => e.SettingsStr)
                    .HasDefaultValueSql("'{}'");
                entity.Property(e => e.StatusStr)
                    .HasDefaultValueSql($"'{ BaseStatus.StatusToString(AdminUserStatus.Activated, EntityStatus.AdminUserStatus) }'");

                entity.HasIndex(e => new { e.UserName, e.Email }, "IX_admin_user_user_name_email")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus) }'");
                entity
                    .HasCheckConstraint(
                        "CK_admin_user_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}' OR status = '{3}'",
                            BaseStatus.StatusToString(AdminUserStatus.Activated, EntityStatus.AdminUserStatus),
                            BaseStatus.StatusToString(AdminUserStatus.Blocked, EntityStatus.AdminUserStatus),
                            BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus),
                            BaseStatus.StatusToString(AdminUserStatus.Readonly, EntityStatus.AdminUserStatus)
                        )
                    );
                entity
                    .HasCheckConstraint(
                        "CK_admin_user_last_access_timestamp_valid_value",
                        "(last_access_timestamp IS NULL) OR (last_access_timestamp > created_timestamp)"
                    );
                entity.HasData(AdminUser.GetDefaultData());
            });
        }
        #endregion
        #region Table AdminUserRight
        private static void ConfigureEnityAdminUserRight(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminUserRight>(entity =>
            {
                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.StatusStr)
                    .HasDefaultValueSql($"'{ BaseStatus.StatusToString(AdminUserRightStatus.Enabled, EntityStatus.AdminUserRightStatus) }'");

                entity.HasIndex(e => e.RightName, "IX_admin_user_right_right_name")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ BaseStatus.StatusToString(AdminUserRightStatus.Disabled, EntityStatus.AdminUserRightStatus) }'");
                entity
                    .HasCheckConstraint(
                        "CK_admin_user_right_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            BaseStatus.StatusToString(AdminUserRightStatus.Enabled, EntityStatus.AdminUserRightStatus),
                            BaseStatus.StatusToString(AdminUserRightStatus.Disabled, EntityStatus.AdminUserRightStatus),
                            BaseStatus.StatusToString(AdminUserRightStatus.Readonly, EntityStatus.AdminUserRightStatus)
                        )
                    );
                entity.HasData(AdminUserRight.GetDefaultData());
            });
        }
        #endregion
        #region Table AdminUserRole
        private static void ConfigureEnityAdminUserRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminUserRole>(entity =>
            {
                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.StatusStr).HasDefaultValueSql($"'{ BaseStatus.StatusToString(AdminUserRoleStatus.Enabled, EntityStatus.AdminUserRoleStatus) }'");

                entity.HasIndex(e => e.RoleName, "IX_admin_user_role_role_name")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ BaseStatus.StatusToString(AdminUserRoleStatus.Disabled, EntityStatus.AdminUserRoleStatus) }'");
                entity
                    .HasCheckConstraint(
                        "CK_admin_user_role_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            BaseStatus.StatusToString(AdminUserRoleStatus.Enabled, EntityStatus.AdminUserRoleStatus),
                            BaseStatus.StatusToString(AdminUserRoleStatus.Disabled, EntityStatus.AdminUserRoleStatus),
                            BaseStatus.StatusToString(AdminUserRoleStatus.Readonly, EntityStatus.AdminUserRoleStatus)
                        )
                    );
                entity.HasData(AdminUserRole.GetDefaultData());
            });
        }
        #endregion
        #region Table AdminUserRoleDetail
        private static void ConfigureEnityAdminUserRoleDetail(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminUserRoleDetail>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.RightId });
                entity.Property(e => e.ActionsStr)
                    .HasDefaultValueSql("'{}'");

                entity.HasOne(d => d.Right)
                    .WithMany(p => p.AdminUserRoleDetails)
                    .HasForeignKey(d => d.RightId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_admin_user_role_detail_right");
                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AdminUserRoleDetails)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_admin_user_role_detail_role");

                entity.HasData(AdminUserRoleDetail.GetDefaultData());
            });
        }
        #endregion
        #region Table AdminUserRoleOfUser
        private static void ConfigureEnityAdminUserRoleOfUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminUserRoleOfUser>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AdminUserRoleOfUsers)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_admin_user_role_of_user_role");
                entity.HasOne(d => d.User)
                    .WithMany(p => p.AdminUserRoleOfUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_admin_user_role_of_user_user");

                entity.HasData(AdminUserRoleOfUser.GetDefaultData());
            });
        }
        #endregion
        #region Table SessionAdminUser
        private static void ConfigureEnitySessionAdminUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SessionAdminUser>(entity =>
            {
                entity.Property(e => e.DataStr).HasDefaultValueSql("'{}'");
                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SessionAdminUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_session_admin_user_user_id");
                entity.HasIndex(e => new { e.SessionToken, e.UserId }, "IX_session_admin_user_token_user_id")
                    .IsUnique();
                entity
                    .HasCheckConstraint(
                        "CK_session_admin_user_last_interaction_time_valid_value",
                        "(last_interaction_time >= created_timestamp)"
                    );
            });
        }
        #endregion
        #region Table SessionSocialUser
        private static void ConfigureEnitySessionSocialUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SessionSocialUser>(entity =>
            {
                entity.Property(e => e.DataStr).HasDefaultValueSql("'{}'");
                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SessionSocialUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_session_social_user_user_id");
                entity.HasIndex(e => new { e.SessionToken, e.UserId }, "IX_session_social_user_token_user_id")
                    .IsUnique();
                entity
                    .HasCheckConstraint(
                        "CK_session_social_user_last_interaction_time_valid_value",
                        "(last_interaction_time >= created_timestamp)"
                    );
            });
        }
        #endregion
        #region Table SocialAuditLog
        private static void ConfigureEnitySocialAuditLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialAuditLog>(entity =>
            {
                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                //entity.HasIndex(e => e.SearchVector, "IX_admin_audit_log_search_vector")
                //    .HasMethod("gin");
                //entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, (((((((((\"table\")::text || ' '::text) || (table_key)::text) || ' '::text) || old_value) || ' '::text) || new_value) || ' '::text) || (\"user\")::text))", true);
                entity
                    .HasGeneratedTsVectorColumn(
                        e => e.SearchVector,
                        "english",
                        e => new { e.Table, e.TableKey, e.OldValueStr, e.NewValueStr, e.User }
                    )
                    .HasIndex(e => e.SearchVector)
                    .HasMethod("GIN");
            });

        }
        #endregion
        #region Table SocialCategory
        private static void ConfigureEnitySocialCategory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialCategory>(entity =>
            {
                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.CreatedTimestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.StatusStr)
                    .HasDefaultValueSql($"'{ BaseStatus.StatusToString(SocialCategoryStatus.Enabled, EntityStatus.SocialCategoryStatus) }'");
                //entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, (((((name)::text || ' '::text) || (display_name)::text) || ' '::text) || (describe)::text))", true);
                //entity.HasIndex(e => e.SearchVector, "IX_social_category_search_vector")
                //    .HasMethod("gin");

                entity
                    .HasGeneratedTsVectorColumn(
                        e => e.SearchVector,
                        "english",
                        e => new { e.Name, e.DisplayName, e.Describe}
                    )
                    .HasIndex(e => e.SearchVector)
                    .HasMethod("GIN");
                entity.HasIndex(e => e.Slug, "IX_social_category_slug")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ BaseStatus.StatusToString(SocialCategoryStatus.Disabled, EntityStatus.SocialCategoryStatus) }'");
                entity
                    .HasCheckConstraint(
                        "CK_social_category_last_modified_timestamp_valid_value",
                        "(last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)"
                    );
                entity
                    .HasCheckConstraint(
                        "CK_social_category_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            BaseStatus.StatusToString(SocialCategoryStatus.Enabled, EntityStatus.SocialCategoryStatus),
                            BaseStatus.StatusToString(SocialCategoryStatus.Disabled, EntityStatus.SocialCategoryStatus),
                            BaseStatus.StatusToString(SocialCategoryStatus.Readonly, EntityStatus.SocialCategoryStatus)
                        )
                    );
                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_social_category_parent");

                entity.HasData(SocialCategory.GetDefaultData());
            });

        }
        #endregion
        #region Table SocialComment
        private static void ConfigureEnitySocialComment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialComment>(entity =>
            {
                //entity.HasIndex(e => e.SearchVector, "IX_social_comment_search_vector")
                //    .HasMethod("gin");
                //entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, content)", true);

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.CreatedTimestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.StatusStr)
                    .HasDefaultValueSql($"'{ BaseStatus.StatusToString(SocialCommentStatus.Created, EntityStatus.SocialCommentStatus) }'");

                entity
                    .HasGeneratedTsVectorColumn(
                        e => e.SearchVector,
                        "english",
                        e => new { e.Content }
                    )
                    .HasIndex(e => e.SearchVector)
                    .HasMethod("GIST");
                entity
                    .HasCheckConstraint(
                        "CK_social_comment_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            BaseStatus.StatusToString(SocialCommentStatus.Created, EntityStatus.SocialCommentStatus),
                            BaseStatus.StatusToString(SocialCommentStatus.Edited, EntityStatus.SocialCommentStatus),
                            BaseStatus.StatusToString(SocialCommentStatus.Deleted, EntityStatus.SocialCommentStatus)
                        )
                    );
                entity
                    .HasCheckConstraint(
                        "CK_social_comment_last_modified_timestamp_valid_value",
                        "(last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)"
                    );
                entity.HasOne(d => d.OwnerNavigation)
                    .WithMany(p => p.SocialComments)
                    .HasForeignKey(d => d.Owner)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_comment_user_id");
                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_social_comment_parent");
                entity.HasOne(d => d.Post)
                    .WithMany(p => p.SocialComments)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_comment_post");
            });

        }
        #endregion
        #region Table SocialNotification
        private static void ConfigureEnitySocialNotification(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialNotification>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.ContentStr).HasDefaultValueSql("'{}'");
                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.StatusStr)
                    .HasDefaultValueSql($"'{ BaseStatus.StatusToString(SocialNotificationStatus.Sent, EntityStatus.SocialNotificationStatus) }'");
                
                entity
                    .HasCheckConstraint(
                        "CK_social_notification_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            BaseStatus.StatusToString(SocialNotificationStatus.Sent, EntityStatus.SocialNotificationStatus),
                            BaseStatus.StatusToString(SocialNotificationStatus.Read, EntityStatus.SocialNotificationStatus),
                            BaseStatus.StatusToString(SocialNotificationStatus.Deleted, EntityStatus.SocialNotificationStatus)
                        )
                    );
                entity
                    .HasCheckConstraint(
                        "CK_social_notification_last_modified_timestamp_valid_value",
                        "(last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)"
                    );
                entity.HasOne(d => d.User)
                    .WithMany(p => p.SocialNotifications)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_notification_user_id");
            });
        }
        #endregion
        #region Table SocialPost
        private static void ConfigureEnitySocialPost(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialPost>(entity =>
            {
                entity.HasIndex(e => e.Slug, "IX_social_post_slug")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ BaseStatus.StatusToString(SocialPostStatus.Deleted, EntityStatus.SocialPostStatus) }'");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.Views)
                    .HasDefaultValueSql("0");
                entity.Property(e => e.TimeRead)
                    .HasDefaultValueSql("2");
                entity.Property(e => e.CreatedTimestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.StatusStr).HasDefaultValueSql($"'{ BaseStatus.StatusToString(SocialPostStatus.Pending, EntityStatus.SocialPostStatus) }'");


                entity
                    .HasGeneratedTsVectorColumn(
                        e => e.SearchVector,
                        "english",
                        e => new { e.ContentSearch, e.Title }
                    )
                    .HasIndex(e => e.SearchVector)
                    .HasMethod("GIST");
                entity
                    .HasCheckConstraint(
                        "CK_social_post_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}' OR status = '{3}'",
                            BaseStatus.StatusToString(SocialPostStatus.Pending, EntityStatus.SocialPostStatus),
                            BaseStatus.StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus),
                            BaseStatus.StatusToString(SocialPostStatus.Private, EntityStatus.SocialPostStatus),
                            BaseStatus.StatusToString(SocialPostStatus.Deleted, EntityStatus.SocialPostStatus)
                        )
                    );
                entity
                    .HasCheckConstraint(
                        "CK_social_post_time_read_valid_value",
                        "time_read >= 2"
                    );
                entity
                    .HasCheckConstraint(
                        "CK_social_post_last_modified_timestamp_valid_value",
                        "(last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)"
                    );
                entity.HasOne(d => d.OwnerNavigation)
                    .WithMany(p => p.SocialPosts)
                    .HasForeignKey(d => d.Owner)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_post_user_id");
            });
        }
        #endregion
        #region Table SocialPostCategory
        private static void ConfigureEnitySocialPostCategory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialPostCategory>(entity =>
            {
                entity.HasKey(e => new { e.PostId, e.CategoryId });

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.SocialPostCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_post_category_category");
                entity.HasOne(d => d.Post)
                    .WithMany(p => p.SocialPostCategories)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_post_category_post");
            });
        }
        #endregion
        #region Table SocialPostTag
        private static void ConfigureEnitySocialPostTag(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialPostTag>(entity =>
            {
                entity.HasKey(e => new { e.PostId, e.TagId });

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.SocialPostTags)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_post_tag_post");
                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.SocialPostTags)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_post_tag_tag");
            });
        }
        #endregion
        #region Table SocialReport
        private static void ConfigureEnitySocialReport(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialReport>(entity =>
            {
                // entity.HasIndex(e => e.SearchVector, "IX_social_report_search_vector")
                //     .HasMethod("gin");
                // entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, content)", true);
                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.CreatedTimestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.StatusStr)
                    .HasDefaultValueSql($"'{ BaseStatus.StatusToString(SocialReportStatus.Pending, EntityStatus.SocialReportStatus) }'");
                entity
                    .HasGeneratedTsVectorColumn(
                        e => e.SearchVector,
                        "english",
                        e => e.Content
                    )
                    .HasIndex(e => e.SearchVector)
                    .HasMethod("GIN");
                entity
                    .HasCheckConstraint(
                        "CK_social_report_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            BaseStatus.StatusToString(SocialReportStatus.Pending, EntityStatus.SocialReportStatus),
                            BaseStatus.StatusToString(SocialReportStatus.Ignored, EntityStatus.SocialReportStatus),
                            BaseStatus.StatusToString(SocialReportStatus.Handled, EntityStatus.SocialReportStatus)
                        )
                    );

                entity.HasOne(d => d.Comment)
                    .WithMany(p => p.SocialReports)
                    .HasForeignKey(d => d.CommentId)
                    .HasConstraintName("FK_social_report_comment");
                entity.HasOne(d => d.Post)
                    .WithMany(p => p.SocialReports)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK_social_report_post");
                entity.HasOne(d => d.User)
                    .WithMany(p => p.SocialReports)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_report_user_id");
            });
        }
        #endregion
        #region Table SocialTag
        private static void ConfigureEnitySocialTag(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialTag>(entity =>
            {
                entity.HasIndex(e => e.Tag, "IX_social_tag_tag")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ BaseStatus.StatusToString(SocialTagStatus.Disabled, EntityStatus.SocialTagStatus) }'");
                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.CreatedTimestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.StatusStr)
                    .HasDefaultValueSql($"'{ BaseStatus.StatusToString(SocialTagStatus.Enabled, EntityStatus.SocialTagStatus) }'");
                entity
                    .HasCheckConstraint(
                        "CK_social_tag_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            BaseStatus.StatusToString(SocialTagStatus.Enabled, EntityStatus.SocialTagStatus),
                            BaseStatus.StatusToString(SocialTagStatus.Disabled, EntityStatus.SocialTagStatus),
                            BaseStatus.StatusToString(SocialTagStatus.Readonly, EntityStatus.SocialTagStatus)
                        )
                    );
                entity.HasData(SocialTag.GetDefaultData());
            });
        }
        #endregion
        #region Table SocialUser
        private static void ConfigureEnitySocialUser(ModelBuilder modelBuilder)
        {
             modelBuilder.Entity<SocialUser>(entity =>
            {
                // entity.HasIndex(e => e.SearchVector, "IX_social_user_search_vector")
                //     .HasMethod("gist");
                // entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, (((display_name)::text || ' '::text) || (user_name)::text))", true);


                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedTimestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.RanksStr)
                    .HasDefaultValueSql("'{}'");
                entity.Property(e => e.SettingsStr)
                    .HasDefaultValueSql("'{}'");
                entity.Property(e => e.Salt)
                    .HasDefaultValueSql("SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)");
                entity.Property(e => e.StatusStr)
                    .HasDefaultValueSql($"'{ BaseStatus.StatusToString(SocialUserStatus.Activated, EntityStatus.SocialUserStatus) }'");
                entity
                    .HasGeneratedTsVectorColumn(
                        e => e.SearchVector,
                        "english",
                        e => new { e.DisplayName, e.UserName }
                    )
                    .HasIndex(e => e.SearchVector)
                    .HasMethod("GIST");
                entity.HasIndex(e => new { e.UserName, e.Email }, "IX_social_user_user_name_email")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus) }'");
                entity
                    .HasCheckConstraint(
                        "CK_social_report_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            BaseStatus.StatusToString(SocialUserStatus.Activated, EntityStatus.SocialUserStatus),
                            BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus),
                            BaseStatus.StatusToString(SocialUserStatus.Blocked, EntityStatus.SocialUserStatus)
                        )
                    );
                entity
                    .HasCheckConstraint(
                        "CK_social_user_last_access_timestamp_valid_value",
                        "(last_access_timestamp IS NULL) OR (last_access_timestamp > created_timestamp)"
                    );
            });
        }
        #endregion
        #region Table SocialUserActionWithCategory
        private static void ConfigureEnitySocialUserActionWithCategory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialUserActionWithCategory>(entity =>
            {
                entity
                    .HasKey(e => new { e.UserId, e.CategoryId });
                entity.Property(e => e.ActionsStr)
                    .HasDefaultValueSql("'[]'");
                entity.HasOne(d => d.Category)
                    .WithMany(p => p.SocialUserActionWithCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_action_with_category_category_id");
                entity.HasOne(d => d.User)
                    .WithMany(p => p.SocialUserActionWithCategories)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_action_with_category_user_id");
            });
        }
        #endregion
        #region Table SocialUserActionWithComment
        private static void ConfigureEnitySocialUserActionWithComment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialUserActionWithComment>(entity =>
            {
                entity
                    .HasKey(e => new { e.UserId, e.CommentId });
                entity.Property(e => e.ActionsStr)
                    .HasDefaultValueSql("'[]'");
                entity.HasOne(d => d.Comment)
                    .WithMany(p => p.SocialUserActionWithComments)
                    .HasForeignKey(d => d.CommentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_action_with_comment_comment_id");
                entity.HasOne(d => d.User)
                    .WithMany(p => p.SocialUserActionWithComments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_action_with_comment_user_id");
            });
        }
        #endregion
        #region Table SocialUserActionWithPost
        private static void ConfigureEnitySocialUserActionWithPost(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialUserActionWithPost>(entity =>
            {
                entity
                    .HasKey(e => new { e.UserId, e.PostId });
                entity.Property(e => e.ActionsStr)
                    .HasDefaultValueSql("'[]'");
                entity.HasOne(d => d.Post)
                    .WithMany(p => p.SocialUserActionWithPosts)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_action_with_post_post_id");
                entity.HasOne(d => d.User)
                    .WithMany(p => p.SocialUserActionWithPosts)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_action_with_post_user_id");
            });
        }
        #endregion
        #region Table SocialUserActionWithTag
        private static void ConfigureEnitySocialUserActionWithTag(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialUserActionWithTag>(entity =>
            {
                entity
                    .HasKey(e => new { e.UserId, e.TagId });
                entity.Property(e => e.ActionsStr)
                    .HasDefaultValueSql("'[]'");
                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.SocialUserActionWithTags)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_action_with_tag_tag_id");
                entity.HasOne(d => d.User)
                    .WithMany(p => p.SocialUserActionWithTags)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_action_with_tag_user_id");
            });
        }
        #endregion
        #region Table SocialUserActionWithUser
        private static void ConfigureEnitySocialUserActionWithUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialUserActionWithUser>(entity =>
            {
                entity
                    .HasKey(e => new { e.UserId, e.UserIdDes });
                entity.Property(e => e.ActionsStr)
                    .HasDefaultValueSql("'[]'");
                entity.HasOne(d => d.User)
                    .WithMany(p => p.SocialUserActionWithUserUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_action_with_user_user_id");
                entity.HasOne(d => d.UserIdDesNavigation)
                    .WithMany(p => p.SocialUserActionWithUserUserIdDesNavigations)
                    .HasForeignKey(d => d.UserIdDes)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_action_with_user_user_id_des");
            });
        }
        #endregion
        #region Table SocialUserRight
        private static void ConfigureEnitySocialUserRight(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialUserRight>(entity =>
            {
                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.StatusStr).HasDefaultValueSql($"'{ BaseStatus.StatusToString(SocialUserRightStatus.Enabled, EntityStatus.SocialUserRightStatus) }'");
                entity.HasIndex(e => e.RightName, "IX_social_user_right_right_name")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ BaseStatus.StatusToString(SocialUserRightStatus.Disabled, EntityStatus.SocialUserRightStatus) }'");
                entity
                    .HasCheckConstraint(
                        "CK_social_user_right_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            BaseStatus.StatusToString(SocialUserRightStatus.Enabled, EntityStatus.SocialUserRightStatus),
                            BaseStatus.StatusToString(SocialUserRightStatus.Disabled, EntityStatus.SocialUserRightStatus),
                            BaseStatus.StatusToString(SocialUserRightStatus.Readonly, EntityStatus.SocialUserRightStatus)
                        )
                    );
                entity.HasData(SocialUserRight.GetDefaultData());
            });
        }
        #endregion
        #region Table SocialUserRole
        private static void ConfigureEnitySocialUserRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialUserRole>(entity =>
            {
                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.StatusStr).HasDefaultValueSql($"'{ BaseStatus.StatusToString(SocialUserRoleStatus.Enabled, EntityStatus.SocialUserRoleStatus) }'");

                entity.HasIndex(e => e.RoleName, "IX_social_user_role_role_name")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ BaseStatus.StatusToString(SocialUserRoleStatus.Disabled, EntityStatus.SocialUserRoleStatus) }'");
                entity
                    .HasCheckConstraint(
                        "CK_social_user_role_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            BaseStatus.StatusToString(SocialUserRoleStatus.Enabled, EntityStatus.SocialUserRoleStatus),
                            BaseStatus.StatusToString(SocialUserRoleStatus.Disabled, EntityStatus.SocialUserRoleStatus),
                            BaseStatus.StatusToString(SocialUserRoleStatus.Readonly, EntityStatus.SocialUserRoleStatus)
                        )
                    );
                entity.HasData(SocialUserRole.GetDefaultData());
            });
        }
        #endregion
        #region Table SocialUserRoleDetail
        private static void ConfigureEnitySocialUserRoleDetail(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialUserRoleDetail>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.RightId });
                entity.Property(e => e.ActionsStr)
                    .HasDefaultValueSql("'{}'");

                entity.HasOne(d => d.Right)
                    .WithMany(p => p.SocialUserRoleDetails)
                    .HasForeignKey(d => d.RightId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_role_detail_right");
                entity.HasOne(d => d.Role)
                    .WithMany(p => p.SocialUserRoleDetails)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_role_detail_role");
                entity.HasData(SocialUserRoleDetail.GetDefaultData());
            });
        }
        #endregion
        #region Table SocialUserRoleOfUser
        private static void ConfigureEnitySocialUserRoleOfUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialUserRoleOfUser>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.SocialUserRoleOfUsers)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_role_of_user_role");
                entity.HasOne(d => d.User)
                    .WithMany(p => p.SocialUserRoleOfUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_user_role_of_user_user");
            });
        }
        #endregion
        #endregion
        #region Function Handle
        #region Session
        public virtual void ClearExpriedSessionAdminUser(List<string> sessionTokens)
        {
            var sessions = SessionAdminUsers.Where(e => sessionTokens.Contains(e.SessionToken));
            SessionAdminUsers.RemoveRange(sessions);
        }
        public virtual void ClearExpriedSessionSocialUser(List<string> sessionTokens)
        {
            var sessions = SessionSocialUsers.Where(e => sessionTokens.Contains(e.SessionToken));
            SessionSocialUsers.RemoveRange(sessions);
        }
        #endregion
        #endregion
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

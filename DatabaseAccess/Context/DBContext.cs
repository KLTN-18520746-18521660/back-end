using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using DatabaseAccess.Common;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Common.Models;

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
                optionsBuilder.UseNpgsql(
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



            












            modelBuilder.Entity<SocialReport>(entity =>
            {
                entity.HasIndex(e => e.SearchVector, "IX_social_report_search_vector")
                    .HasMethod("gin");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");

                entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, content)", true);

                entity.Property(e => e.Status).HasDefaultValueSql("'Pending'::character varying");

                entity.HasOne(d => d.Comment)
                    .WithMany(p => p.SocialReports)
                    .HasForeignKey(d => d.CommentId)
                    .HasConstraintName("FK_social_report_comment");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.SocialReports)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK_social_report_post");
            });

            modelBuilder.Entity<SocialTag>(entity =>
            {
                entity.HasIndex(e => e.Tag, "IX_social_tag_tag")
                    .IsUnique()
                    .HasFilter("((status)::text <> 'Disabled'::text)");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");

                entity.Property(e => e.Status).HasDefaultValueSql("'Enabled'::character varying");
            });

            modelBuilder.Entity<SocialUser>(entity =>
            {
                entity.HasIndex(e => e.SearchVector, "IX_social_user_search_vector")
                    .HasMethod("gist");

                entity.HasIndex(e => new { e.UserName, e.Email }, "IX_social_user_user_name_email")
                    .IsUnique()
                    .HasFilter("((status)::text <> 'Deleted'::text)");

                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");

                entity.Property(e => e.Ranks).HasDefaultValueSql("'{}'::json");

                entity.Property(e => e.Roles).HasDefaultValueSql("'[]'::json");

                entity.Property(e => e.Salt).HasDefaultValueSql("\"substring\"(replace(((gen_random_uuid())::character varying)::text, '-'::text, ''::text), 1, 8)");

                entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, (((display_name)::text || ' '::text) || (user_name)::text))", true);

                entity.Property(e => e.Settings).HasDefaultValueSql("'{}'::json");

                entity.Property(e => e.Status).HasDefaultValueSql("'Activated'::character varying");
            });

            modelBuilder.Entity<SocialUserActionWithCategory>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.CategoryId });

                entity.Property(e => e.Actions).HasDefaultValueSql("'[]'::json");

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

            modelBuilder.Entity<SocialUserActionWithComment>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.CommentId });

                entity.Property(e => e.Actions).HasDefaultValueSql("'[]'::json");

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

            modelBuilder.Entity<SocialUserActionWithPost>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.PostId });

                entity.Property(e => e.Actions).HasDefaultValueSql("'[]'::json");

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

            modelBuilder.Entity<SocialUserActionWithTag>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.TagId });

                entity.Property(e => e.Actions).HasDefaultValueSql("'[]'::json");

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

            modelBuilder.Entity<SocialUserActionWithUser>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.UserIdDes });

                entity.Property(e => e.Actions).HasDefaultValueSql("'[]'::json");

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

            modelBuilder.Entity<SocialUserRight>(entity =>
            {
                entity.HasIndex(e => e.RightName, "IX_social_user_right_right_name")
                    .IsUnique()
                    .HasFilter("((status)::text <> 'Disabled'::text)");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Status).HasDefaultValueSql("'Enabled'::character varying");
            });

            modelBuilder.Entity<SocialUserRole>(entity =>
            {
                entity.HasIndex(e => e.RoleName, "IX_social_user_role_role_name")
                    .IsUnique()
                    .HasFilter("((status)::text <> 'Disabled'::text)");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Rights).HasDefaultValueSql("'[]'::json");

                entity.Property(e => e.Status).HasDefaultValueSql("'Enabled'::character varying");
            });

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
                entity.Property(e => e.Status)
                    .HasDefaultValueSql($"'{ AdminBaseConfigStatus.StatusToString(AdminBaseConfigStatus.Enabled) }'");
                entity.Property(e => e.ValueStr).HasDefaultValueSql("{}");

                entity.HasIndex(e => e.ConfigKey, "IX_admin_base_config_config_key")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ AdminBaseConfigStatus.StatusToString(AdminBaseConfigStatus.Disabled) }'");
                entity
                    .HasCheckConstraint(
                        "CK_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            AdminBaseConfigStatus.StatusToString(AdminBaseConfigStatus.Disabled),
                            AdminBaseConfigStatus.StatusToString(AdminBaseConfigStatus.Enabled),
                            AdminBaseConfigStatus.StatusToString(AdminBaseConfigStatus.Readonly)
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
                entity.Property(e => e.Roles)
                    .HasDefaultValueSql("[]");
                entity.Property(e => e.Salt)
                    .HasDefaultValueSql("SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)");
                entity.Property(e => e.Settings)
                    .HasDefaultValueSql("{}");
                entity.Property(e => e.Status)
                    .HasDefaultValueSql($"{ AdminUserStatus.StatusToString(AdminUserStatus.Activated) }");

                entity.HasIndex(e => new { e.UserName, e.Email }, "IX_admin_user_user_name_email")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ AdminUserStatus.StatusToString(AdminUserStatus.Deleted) }'");
                entity
                    .HasCheckConstraint(
                        "CK_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}' OR status = '{3}'",
                            AdminUserStatus.StatusToString(AdminUserStatus.Activated),
                            AdminUserStatus.StatusToString(AdminUserStatus.Blocked),
                            AdminUserStatus.StatusToString(AdminUserStatus.Deleted),
                            AdminUserStatus.StatusToString(AdminUserStatus.Readonly)
                        )
                    );
                entity
                    .HasCheckConstraint(
                        "CK_last_access_timestamp_valid_value",
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
                    .HasDefaultValueSql($"{ AdminUserRightStatus.StatusToString(AdminUserRightStatus.Enabled) }");

                entity.HasIndex(e => e.RightName, "IX_admin_user_right_right_name")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ AdminUserRightStatus.StatusToString(AdminUserRightStatus.Disabled) }'");
                entity
                    .HasCheckConstraint(
                        "CK_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            AdminUserRightStatus.StatusToString(AdminUserRightStatus.Enabled),
                            AdminUserRightStatus.StatusToString(AdminUserRightStatus.Disabled),
                            AdminUserRightStatus.StatusToString(AdminUserRightStatus.Readonly)
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
                entity.Property(e => e.RightsStr)
                    .HasDefaultValueSql("[]");
                entity.Property(e => e.StatusStr).HasDefaultValueSql($"{ AdminUserRoleStatus.StatusToString(AdminUserRoleStatus.Enabled) }");

                entity.HasIndex(e => e.RoleName, "IX_admin_user_role_role_name")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ AdminUserRoleStatus.StatusToString(AdminUserRoleStatus.Disabled) }')");
                entity
                    .HasCheckConstraint(
                        "CK_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            AdminUserRoleStatus.StatusToString(AdminUserRoleStatus.Enabled),
                            AdminUserRoleStatus.StatusToString(AdminUserRoleStatus.Disabled),
                            AdminUserRoleStatus.StatusToString(AdminUserRoleStatus.Readonly)
                        )
                    );
                entity.HasData(AdminUserRole.GetDefaultData());
            });
        }
        #endregion
        #region Table SessionAdminUser
        private static void ConfigureEnitySessionAdminUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SessionAdminUser>(entity =>
            {
                entity.Property(e => e.DataStr).HasDefaultValueSql("{}");
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
                        "CK_last_interaction_time_valid_value",
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
                entity.Property(e => e.Data).HasDefaultValueSql("{}");
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
                        "CK_last_interaction_time_valid_value",
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
                entity.Property(e => e.Status)
                    .HasDefaultValueSql($"{ SocialCategoryStatus.StatusToString(SocialCategoryStatus.Enabled) }");
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
                    .HasFilter($"(status) <> '{ SocialCategoryStatus.StatusToString(SocialCategoryStatus.Disabled) }'");
                entity
                    .HasCheckConstraint(
                        "CK_last_modified_timestamp_valid_value",
                        "(last_modified_timestamp IS NULL) OR (last_modified_timestamp > created_timestamp)"
                    );
                entity
                    .HasCheckConstraint(
                        "CK_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            SocialCategoryStatus.StatusToString(SocialCategoryStatus.Enabled),
                            SocialCategoryStatus.StatusToString(SocialCategoryStatus.Disabled),
                            SocialCategoryStatus.StatusToString(SocialCategoryStatus.Readonly)
                        )
                    );
                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_social_category_parent");
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
                    .HasDefaultValueSql($"{ SocialCommentStatus.StatusToString(SocialCommentStatus.Created) }");

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
                        "CK_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            SocialCommentStatus.StatusToString(SocialCommentStatus.Created),
                            SocialCommentStatus.StatusToString(SocialCommentStatus.Edited),
                            SocialCommentStatus.StatusToString(SocialCommentStatus.Deleted)
                        )
                    );
                entity
                    .HasCheckConstraint(
                        "CK_last_modified_timestamp_valid_value",
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
                entity.Property(e => e.Content).HasDefaultValueSql("{}");
                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.StatusStr)
                    .HasDefaultValueSql($"{ SocialNotificationStatus.StatusToString(SocialNotificationStatus.Sent) }");
                
                entity
                    .HasCheckConstraint(
                        "CK_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                            SocialNotificationStatus.StatusToString(SocialNotificationStatus.Sent),
                            SocialNotificationStatus.StatusToString(SocialNotificationStatus.Read),
                            SocialNotificationStatus.StatusToString(SocialNotificationStatus.Deleted)
                        )
                    );
                entity
                    .HasCheckConstraint(
                        "CK_last_modified_timestamp_valid_value",
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
                //entity.HasIndex(e => e.SearchVector, "IX_social_post_search_vector")
                //    .HasMethod("gin");

                entity.HasIndex(e => e.Slug, "IX_social_post_slug")
                    .IsUnique()
                    .HasFilter($"(status) <> '{ SocialPostStatus.StatusToString(SocialPostStatus.Deleted) }'");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();
                entity.Property(e => e.CreatedTimestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.StatusStr).HasDefaultValueSql($"{ SocialPostStatus.StatusToString(SocialPostStatus.Pending) }");

                //entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, ((title || ' '::text) || content_search))", true);

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
                        "CK_status_valid_value",
                        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}' OR status = '{2}'",
                            SocialPostStatus.StatusToString(SocialPostStatus.Pending),
                            SocialPostStatus.StatusToString(SocialPostStatus.Approved),
                            SocialPostStatus.StatusToString(SocialPostStatus.Private),
                            SocialPostStatus.StatusToString(SocialPostStatus.Deleted)
                        )
                    );
                entity
                    .HasCheckConstraint(
                        "CK_last_modified_timestamp_valid_value",
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
            });
        }
        #endregion
        #region Table SocialPostTag
        private static void ConfigureEnitySocialPostTag(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialPostTag>(entity =>
            {
                entity.HasKey(e => new { e.PostId, e.TagId });
            });
        }
        #endregion
        #region Table SocialReport
        private static void ConfigureEnitySocialReport(ModelBuilder modelBuilder)
        {

        }
        #endregion
        #region Table SocialTag
        private static void ConfigureEnitySocialTag(ModelBuilder modelBuilder)
        {

        }
        #endregion
        #region Table SocialUser
        private static void ConfigureEnitySocialUser(ModelBuilder modelBuilder)
        {

        }
        #endregion
        #region Table SocialUserActionWithCategory
        private static void ConfigureEnitySocialUserActionWithCategory(ModelBuilder modelBuilder)
        {

        }
        #endregion
        #region Table SocialUserActionWithComment
        private static void ConfigureEnitySocialUserActionWithComment(ModelBuilder modelBuilder)
        {

        }
        #endregion
        #region Table SocialUserActionWithPost
        private static void ConfigureEnitySocialUserActionWithPost(ModelBuilder modelBuilder)
        {

        }
        #endregion
        #region Table SocialUserActionWithTag
        private static void ConfigureEnitySocialUserActionWithTag(ModelBuilder modelBuilder)
        {

        }
        #endregion
        #region Table SocialUserActionWithUser
        private static void ConfigureEnitySocialUserActionWithUser(ModelBuilder modelBuilder)
        {

        }
        #endregion
        #region Table SocialUserRight
        private static void ConfigureEnitySocialUserRight(ModelBuilder modelBuilder)
        {

        }
        #endregion
        #region Table SocialUserRole
        private static void ConfigureEnitySocialUserRole(ModelBuilder modelBuilder)
        {

        }
        #endregion
        #endregion
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace DatabaseAccess.tst
{
    public partial class postgresContext : DbContext
    {
        public postgresContext()
        {
        }

        public postgresContext(DbContextOptions<postgresContext> options)
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Database=postgres;Password=a;Port=5432");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");

            modelBuilder.Entity<AdminAuditLog>(entity =>
            {
                entity.HasIndex(e => e.SearchVector, "IX_admin_audit_log_search_vector")
                    .HasMethod("gin");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, (((((((((\"table\")::text || ' '::text) || (table_key)::text) || ' '::text) || old_value) || ' '::text) || new_value) || ' '::text) || (\"user\")::text))", true);

                entity.Property(e => e.Timestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");
            });

            modelBuilder.Entity<AdminBaseConfig>(entity =>
            {
                entity.HasIndex(e => e.ConfigKey, "IX_admin_base_config_config_key")
                    .IsUnique()
                    .HasFilter("((status)::text <> 'Disabled'::text)");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Status).HasDefaultValueSql("'Enabled'::character varying");

                entity.Property(e => e.Value).HasDefaultValueSql("'{}'::json");
            });

            modelBuilder.Entity<AdminUser>(entity =>
            {
                entity.HasIndex(e => new { e.UserName, e.Email }, "IX_admin_user_user_name_email")
                    .IsUnique()
                    .HasFilter("((status)::text <> 'Deleted'::text)");

                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");

                entity.Property(e => e.Roles).HasDefaultValueSql("'[]'::json");

                entity.Property(e => e.Salt).HasDefaultValueSql("\"substring\"(replace(((gen_random_uuid())::character varying)::text, '-'::text, ''::text), 1, 8)");

                entity.Property(e => e.Settings).HasDefaultValueSql("'{}'::json");

                entity.Property(e => e.Status).HasDefaultValueSql("'Activated'::character varying");
            });

            modelBuilder.Entity<AdminUserRight>(entity =>
            {
                entity.HasIndex(e => e.RightName, "IX_admin_user_right_right_name")
                    .IsUnique()
                    .HasFilter("((status)::text <> 'Disabled'::text)");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Status).HasDefaultValueSql("'Enabled'::character varying");
            });

            modelBuilder.Entity<AdminUserRole>(entity =>
            {
                entity.HasIndex(e => e.RoleName, "IX_admin_user_role_role_name")
                    .IsUnique()
                    .HasFilter("((status)::text <> 'Disabled'::text)");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Rights).HasDefaultValueSql("'[]'::json");

                entity.Property(e => e.Status).HasDefaultValueSql("'Enabled'::character varying");
            });

            modelBuilder.Entity<SessionAdminUser>(entity =>
            {
                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");

                entity.Property(e => e.Data).HasDefaultValueSql("'{}'::json");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SessionAdminUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_session_admin_user_user_id");
            });

            modelBuilder.Entity<SessionSocialUser>(entity =>
            {
                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");

                entity.Property(e => e.Data).HasDefaultValueSql("'{}'::json");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SessionSocialUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_session_social_user_user_id");
            });

            modelBuilder.Entity<SocialAuditLog>(entity =>
            {
                entity.HasIndex(e => e.SearchVector, "IX_social_audit_log_search_vector")
                    .HasMethod("gin");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, (((((((((\"table\")::text || ' '::text) || (table_key)::text) || ' '::text) || old_value) || ' '::text) || new_value) || ' '::text) || (\"user\")::text))", true);

                entity.Property(e => e.Timestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");
            });

            modelBuilder.Entity<SocialCategory>(entity =>
            {
                entity.HasIndex(e => e.SearchVector, "IX_social_category_search_vector")
                    .HasMethod("gin");

                entity.HasIndex(e => e.Slug, "IX_social_category_slug")
                    .IsUnique()
                    .HasFilter("((status)::text <> 'Disabled'::text)");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");

                entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, (((((name)::text || ' '::text) || (display_name)::text) || ' '::text) || (describe)::text))", true);

                entity.Property(e => e.Status).HasDefaultValueSql("'Enabled'::character varying");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_social_category_parent");
            });

            modelBuilder.Entity<SocialComment>(entity =>
            {
                entity.HasIndex(e => e.SearchVector, "IX_social_comment_search_vector")
                    .HasMethod("gin");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");

                entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, content)", true);

                entity.Property(e => e.Status).HasDefaultValueSql("'Created'::character varying");

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

            modelBuilder.Entity<SocialNotification>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Content).HasDefaultValueSql("'{}'::json");

                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");

                entity.Property(e => e.Status).HasDefaultValueSql("'Sent'::character varying");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SocialNotifications)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_notification_user_id");
            });

            modelBuilder.Entity<SocialPost>(entity =>
            {
                entity.HasIndex(e => e.SearchVector, "IX_social_post_search_vector")
                    .HasMethod("gin");

                entity.HasIndex(e => e.Slug, "IX_social_post_slug")
                    .IsUnique()
                    .HasFilter("((status)::text <> 'Deleted'::text)");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");

                entity.Property(e => e.SearchVector).HasComputedColumnSql("to_tsvector('english'::regconfig, ((title || ' '::text) || content_search))", true);

                entity.Property(e => e.Status).HasDefaultValueSql("'Pending'::character varying");

                entity.HasOne(d => d.OwnerNavigation)
                    .WithMany(p => p.SocialPosts)
                    .HasForeignKey(d => d.Owner)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_post_user_id");
            });

            modelBuilder.Entity<SocialPostCategory>(entity =>
            {
                entity.HasKey(e => new { e.PostId, e.CategoryId });

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.SocialPostCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("social_post_category_category");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.SocialPostCategories)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("social_post_category_post");
            });

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

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SocialReports)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_social_report_user_id");
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

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

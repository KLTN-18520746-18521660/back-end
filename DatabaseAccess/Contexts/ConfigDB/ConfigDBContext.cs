using Microsoft.EntityFrameworkCore;
using Common.Password;

namespace DatabaseAccess.Contexts.ConfigDB
{
    public class ConfigDBContext : DbContext
    {
        public DbSet<Models.AdminUser> AdminUsers { get; set; }
        public DbSet<Models.AdminUserRole> AdminUserRoles { get; set; }
        public DbSet<Models.BaseConfig> BaseConfigs { get; set; }
        public DbSet<Models.ConfigAuditLog> ConfigAuditLogs { get; set; }
        public DbSet<Models.SocialAuditLog> SocialAuditLogs { get; set; }
        public DbSet<Models.SocialUserRole> SocialUserRoles { get; set; }
        public ConfigDBContext()
        {
        }

        public ConfigDBContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseNpgsql(
                    BaseConfigurationDB.GetConnectStringToConfigDB(),
                    npgsqlOptionsAction: o => {
                        o.SetPostgresVersion(14, 1);
                    }
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureEnityAdminUser(modelBuilder);
            ConfigureEnityAdminUserRole(modelBuilder);
            ConfigureEnityBaseConfig(modelBuilder);
            ConfigureEnitySocialAuditLog(modelBuilder);
            ConfigureEnitySocialUserRole(modelBuilder);
        }
        private static void ConfigureEnityAdminUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.AdminUser>(entityBuilder =>
            {
                /// Configure for Primary Key and column
                entityBuilder.HasKey(e => e.Id);
                entityBuilder.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()")
                    .IsRequired();
                entityBuilder.Property(e => e.UserName)
                    .IsRequired();
                entityBuilder.Property(e => e.DisplayName)
                    .IsRequired();
                entityBuilder.Property(e => e.Salt)
                    .HasDefaultValueSql("SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)")
                    .IsRequired();
                entityBuilder.Property(e => e.Password)
                    .IsRequired();
                entityBuilder.Property(e => e.Email)
                    .IsRequired();
                entityBuilder.Property(e => e.Status)
                    .HasDefaultValue(1)
                    .IsRequired();
                entityBuilder.Property(e => e.RightsStr)
                    .HasDefaultValue("[]")
                    .IsRequired();
                entityBuilder.Property(e => e.SettingsStr)
                    .HasDefaultValue("{}")
                    .IsRequired();
                entityBuilder.Property(e => e.CreatedTimestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
                    .IsRequired();
                entityBuilder.Property(e => e.LastAccessTimestamp)
                    .IsRequired(false);

                entityBuilder
                    .HasIndex(e => e.UserName)
                    .IsUnique()
                    .HasFilter("status != 0");
                entityBuilder
                    .HasIndex(e => e.Id);
                entityBuilder
                    .HasCheckConstraint(
                        "CK_Status_Valid_Value",
                        $"status >= { Common.UserStatus.Deleted } AND status <= { Common.UserStatus.Readonly }"
                    );
                entityBuilder
                    .HasCheckConstraint(
                        "CK_LastAccessTimestamp_Valid_Value",
                        "last_access_timestamp IS NULL"
                    )
                    .HasQueryFilter(e => e.Status == Common.UserStatus.NotActivated);
                entityBuilder.HasData(Models.AdminUser.GetUserDefault());
            });
        }
        private void ConfigureEnityAdminUserRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.AdminUserRole>(entityBuilder =>
            {
                //entityBuilder.
            });
        }
        private void ConfigureEnityBaseConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.BaseConfig>(entityBuilder =>
            {
                //entityBuilder.
            });
        }
        private void ConfigureEnityConfigAuditLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.ConfigAuditLog>(entityBuilder =>
            {
                //entityBuilder.
            });
        }
        private void ConfigureEnitySocialAuditLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.SocialAuditLog>(entityBuilder =>
            {
                //entityBuilder.
            });
        }
        private void ConfigureEnitySocialUserRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.SocialUserRole>(entityBuilder =>
            {
                //entityBuilder.
            });
        }
    }
}

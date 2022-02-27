using Microsoft.EntityFrameworkCore;
using DatabaseAccess.Contexts.CachedDB.Models;

namespace DatabaseAccess.Contexts.CachedDB
{
    public class CachedDBContext : DbContext
    {
        DbSet<RecommendResultOfSocialUser> RecommendResultOfSocialUsers { get; set; }
        DbSet<SessionSocialUser> SessionSocialUsers { get; set; }
        DbSet<SessionAdminUser> SessionAdminUsers { get; set; }
        public CachedDBContext()
        {
        }

        public CachedDBContext(DbContextOptions<CachedDBContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseNpgsql(
                    BaseConfigurationDB.GetConnectStringToCachedDB(),
                    npgsqlOptionsAction: o => {
                        o.SetPostgresVersion(14, 1);
                    }
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}

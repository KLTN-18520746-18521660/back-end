using Microsoft.EntityFrameworkCore;

namespace DatabaseAccess.Contexts.CachedDB
{
    public class CachedDBContext : DbContext
    {
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
        }
    }
}

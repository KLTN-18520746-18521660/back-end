using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Common
{
#pragma warning disable EF1001 // Internal EF Core API usage.
    public class FixedMigrationsIdGenerator : MigrationsIdGenerator
#pragma warning restore EF1001 // Internal EF Core API usage.
    {
        public override string GetName(string id) => id;
        public override bool IsValidId(string value) => value != default && value != string.Empty;
        public override string GenerateId(string name)
        {
            return name;
        }
    }
}
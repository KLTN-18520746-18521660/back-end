using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Common
{
    public class FixedMigrationsIdGenerator : MigrationsIdGenerator
    {
        public override string GetName(string id) => id;
        public override bool IsValidId(string value) => value != default && value != string.Empty;
        public override string GenerateId(string name)
        {
            return name;
        }
    }
}
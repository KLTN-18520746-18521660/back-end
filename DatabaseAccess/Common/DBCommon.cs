using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using DatabaseAccess.Common;
using Microsoft.EntityFrameworkCore;

namespace Common
{
    public static class DBCommon
    {
        public static readonly int MAX_LENGTH_OF_TEXT = 65535;
        public static readonly DateTime DEFAULT_DATETIME_FOR_DATA_SEED = new DateTime(2022, 02, 20, 13, 13, 13).ToUniversalTime();
        public static readonly string FIRST_ADMIN_USER_NAME = "admin";
#if DEBUG
        public static readonly Guid FIRST_ADMIN_USER_ID = new Guid("1afc27e9-85c3-4e48-89ab-dd997621ab32");
        public static readonly string FIRST_ADMIN_USER_SALT = "82b82727";
        public static readonly string FIRST_ADMIN_USER_EMAIL = "admin@admin";
#else
        public static readonly Guid FIRST_ADMIN_USER_ID                 = Guid.NewGuid();
        public static readonly string FIRST_ADMIN_USER_SALT             = PasswordEncryptor.GenerateSalt();
        public static readonly string FIRST_ADMIN_USER_EMAIL            = "admin@gmail.com";
#endif
    }
    public static class DBHelper
    {
        public static async Task<List<T>> RawSqlQuery<T>(string query, Func<DbDataReader, T> map)
        {
            using (var context = new DatabaseAccess.Context.DBContext())
            {
                using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;

                    context.Database.OpenConnection();

                    using (var result = await command.ExecuteReaderAsync())
                    {
                        var entities = new List<T>();

                        while (result.Read()) {
                            entities.Add(map(result));
                        }

                        return entities;
                    }
                }
            }
        }
    }
}
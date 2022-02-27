using Microsoft.EntityFrameworkCore;
using DatabaseAccess.Common;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System;

namespace DatabaseAccess.Contexts.ConfigDB
{
    public class DBContext : DbContext
    {
        public DbSet<Models.AdminUser> AdminUsers { get; set; }
        public DbSet<Models.AdminUserRole> AdminUserRoles { get; set; }
        public DbSet<Models.AdminUserRight> AdminUserRights { get; set; }
        public DbSet<Models.BaseConfig> BaseConfigs { get; set; }
        public DbSet<Models.ConfigAuditLog> ConfigAuditLogs { get; set; }
        public DbSet<Models.SocialAuditLog> SocialAuditLogs { get; set; }
        public DbSet<Models.SocialUserRole> SocialUserRoles { get; set; }
        public DbSet<Models.SocialUserRight> SocialUserRights { get; set; }
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }
        #region Configure Context
        // Configure connection string
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseNpgsql(
                    BaseConfigurationDB.GetConnectStringToDB(),
                    npgsqlOptionsAction: o => {
                        o.SetPostgresVersion(14, 1);
                    }
                );
            }
        }
        // Call function for configure table
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureEnityAdminUser(modelBuilder);
            ConfigureEnityAdminUserRight(modelBuilder);
            ConfigureEnityAdminUserRole(modelBuilder);
            ConfigureEnityBaseConfig(modelBuilder);
            ConfigureEnityConfigAuditLog(modelBuilder);
            ConfigureEnitySocialAuditLog(modelBuilder);
            ConfigureEnitySocialUserRight(modelBuilder);
            ConfigureEnitySocialUserRole(modelBuilder);
        }
        #region Table AdminUser
        private static void ConfigureEnityAdminUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.AdminUser>(entityBuilder =>
            {
                //entityBuilder.HasKey(e => e.Id);
                //entityBuilder.Property(e => e.Id)
                //    .HasDefaultValueSql("gen_random_uuid()")
                //    .IsRequired();
                //entityBuilder.Property(e => e.UserName)
                //    .IsRequired();
                //entityBuilder.Property(e => e.DisplayName)
                //    .IsRequired();
                //entityBuilder.Property(e => e.Salt)
                //    .HasDefaultValueSql("SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)")
                //    .IsRequired();
                //entityBuilder.Property(e => e.Password)
                //    .IsRequired();
                //entityBuilder.Property(e => e.Email)
                //    .IsRequired();
                //entityBuilder.Property(e => e.StatusStr)
                //    .HasDefaultValue(Common.SocialUserStatus.StatusToString(Common.SocialUserStatus.NotActivated))
                //    .IsRequired();
                //entityBuilder.Property(e => e.RolesStr)
                //    .HasDefaultValue("[]")
                //    .IsRequired();
                //entityBuilder.Property(e => e.SettingsStr)
                //    .HasDefaultValue("{}")
                //    .IsRequired();
                //entityBuilder.Property(e => e.CreatedTimestamp)
                //    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
                //    .IsRequired();
                //entityBuilder.Property(e => e.LastAccessTimestamp)
                //    .IsRequired(false);

                //entityBuilder
                //    .HasIndex(e => new { e.UserName, e.Email })
                //    .IsUnique()
                //    .HasFilter($"status != '{ Common.SocialUserStatus.StatusToString(Common.SocialUserStatus.Deleted) }'");
                //entityBuilder
                //    .HasCheckConstraint(
                //        "CK_Status_Valid_Value",
                //        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}' OR status = '{3}'",
                //            Common.SocialUserStatus.StatusToString(Common.SocialUserStatus.Deleted),
                //            Common.SocialUserStatus.StatusToString(Common.SocialUserStatus.NotActivated),
                //            Common.SocialUserStatus.StatusToString(Common.SocialUserStatus.Activated),
                //            Common.SocialUserStatus.StatusToString(Common.SocialUserStatus.Readonly)
                //        )
                //    );
                //entityBuilder
                //    .HasCheckConstraint(
                //        "CK_LastAccessTimestamp_Valid_Value",
                //        string.Format("(last_access_timestamp IS NULL AND status = '{0}') OR (status <> '{0}')",
                //            Common.SocialUserStatus.StatusToString(Common.SocialUserStatus.NotActivated)
                //        )
                //    );
                //entityBuilder.HasData(Models.AdminUser.GetDefaultData());
            });
        }
        #endregion
        #region Table AdminUserRight
        private static void ConfigureEnityAdminUserRight(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Models.AdminUserRight>(entityBuilder =>
            //{
            //    entityBuilder.HasKey(e => e.Id);
            //    entityBuilder.Property(e => e.Id)
            //        .UseIdentityAlwaysColumn()
            //        .IsRequired();
            //    entityBuilder.Property(e => e.RightName)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.DisplayName)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.Describe)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.StatusStr)
            //        .IsRequired();

            //    entityBuilder.HasIndex(e => e.RightName)
            //        .IsUnique()
            //        .HasFilter($"status != '{ Common.EntityStatus.StatusToString(Common.EntityStatus.Disabled) }'");
            //    entityBuilder.HasData(Models.AdminUserRight.GetDefaultData());
            //    entityBuilder
            //        .HasCheckConstraint(
            //            "CK_Status_Valid_Value",
            //            string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
            //                Common.EntityStatus.StatusToString(Common.EntityStatus.Disabled),
            //                Common.EntityStatus.StatusToString(Common.EntityStatus.Enabled),
            //                Common.EntityStatus.StatusToString(Common.EntityStatus.Readonly)
            //            )
            //        );
            //});
        }
        #endregion
        #region Table AdminUserRole
        private static void ConfigureEnityAdminUserRole(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Models.AdminUserRole>(entityBuilder =>
            //{
            //    entityBuilder.HasKey (e => e.Id);
            //    entityBuilder.Property(e => e.Id)
            //        .UseIdentityAlwaysColumn()
            //        .IsRequired();
            //    entityBuilder.Property(e => e.RoleName)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.DisplayName)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.Describe)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.RightsStr)
            //        .HasDefaultValue("[]")
            //        .IsRequired();
            //    entityBuilder.Property(e => e.StatusStr)
            //        .IsRequired();

            //    entityBuilder.HasIndex(e => e.RoleName)
            //        .IsUnique()
            //        .HasFilter($"status != '{ Common.EntityStatus.StatusToString(Common.EntityStatus.Disabled) }'");
            //    entityBuilder.HasData(Models.AdminUserRole.GetDefaultData());
            //    entityBuilder
            //        .HasCheckConstraint(
            //            "CK_Status_Valid_Value",
            //            string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
            //                Common.EntityStatus.StatusToString(Common.EntityStatus.Disabled),
            //                Common.EntityStatus.StatusToString(Common.EntityStatus.Enabled),
            //                Common.EntityStatus.StatusToString(Common.EntityStatus.Readonly)
            //            )
            //        );
            //});
        }
        #endregion
        #region Table BaseConfig
        private static void ConfigureEnityBaseConfig(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Models.BaseConfig>(entityBuilder =>
            //{
            //    entityBuilder.HasKey (e => e.Id);
            //    entityBuilder.Property(e => e.Id)
            //        .UseIdentityAlwaysColumn()
            //        .IsRequired();
            //    entityBuilder.Property(e => e.ConfigKey)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.ValueStr)
            //        .HasDefaultValue("{}")
            //        .IsRequired();
            //    entityBuilder.Property(e => e.StatusStr)
            //        .IsRequired();

            //    entityBuilder.HasIndex(e => e.ConfigKey)
            //        .IsUnique()
            //        .HasFilter($"status != '{ Common.EntityStatus.StatusToString(Common.EntityStatus.Disabled) }'");
            //    entityBuilder.HasData(Models.BaseConfig.GetDefaultData());
            //    entityBuilder
            //        .HasCheckConstraint(
            //            "CK_Status_Valid_Value",
            //            string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
            //                Common.EntityStatus.StatusToString(Common.EntityStatus.Disabled),
            //                Common.EntityStatus.StatusToString(Common.EntityStatus.Enabled),
            //                Common.EntityStatus.StatusToString(Common.EntityStatus.Readonly)
            //            )
            //        );
            //});
        }
        #endregion
        #region Table ConfigAuditLog
        private static void ConfigureEnityConfigAuditLog(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Models.ConfigAuditLog>(entityBuilder =>
            //{
            //    entityBuilder.HasKey (e => e.Id);
            //    entityBuilder.Property(e => e.Id)
            //        .UseIdentityAlwaysColumn()
            //        .IsRequired();
            //    entityBuilder.Property(e => e.Table)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.TableKey)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.Action)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.OldValueStr)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.NewValueStr)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.User)
            //        .IsRequired();
            //    entityBuilder.Property(e => e.Timestamp)
            //        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
            //        .IsRequired();

            //    entityBuilder
            //        .HasGeneratedTsVectorColumn(
            //            e => e.SearchVector,
            //            "english",
            //            e => new { e.Table, e.TableKey, e.OldValueStr, e.NewValueStr, e.User }
            //        )
            //        .HasIndex(e => e.SearchVector)
            //        .HasMethod("GIN");
            //});
        }
        #endregion
        #region Table SocialAuditLog
        private static void ConfigureEnitySocialAuditLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.SocialAuditLog>(entityBuilder =>
            {
                entityBuilder.HasKey(e => e.Id);
                entityBuilder.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .IsRequired();
                entityBuilder.Property(e => e.Table)
                    .IsRequired();
                entityBuilder.Property(e => e.TableKey)
                    .IsRequired();
                entityBuilder.Property(e => e.Action)
                    .IsRequired();
                entityBuilder.Property(e => e.OldValueStr)
                    .IsRequired();
                entityBuilder.Property(e => e.NewValueStr)
                    .IsRequired();
                entityBuilder.Property(e => e.User)
                    .IsRequired();
                entityBuilder.Property(e => e.Timestamp)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
                    .IsRequired();

                entityBuilder
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
        #region Table SocialUserRight
        private static void ConfigureEnitySocialUserRight(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.SocialUserRight>(entityBuilder =>
            {
                //entityBuilder.HasKey(e => e.Id);
                //entityBuilder.Property(e => e.Id)
                //    .UseIdentityAlwaysColumn()
                //    .IsRequired();
                //entityBuilder.Property(e => e.RightName)
                //    .IsRequired();
                //entityBuilder.Property(e => e.DisplayName)
                //    .IsRequired();
                //entityBuilder.Property(e => e.Describe)
                //    .IsRequired();
                //entityBuilder.Property(e => e.StatusStr)
                //    .IsRequired();

                //entityBuilder.HasIndex(e => e.RightName)
                //    .IsUnique()
                //    .HasFilter($"status != '{ Common.EntityStatus.StatusToString(Common.EntityStatus.Disabled) }'");
                //entityBuilder.HasData(Models.SocialUserRight.GetDefaultData());
                //entityBuilder
                //    .HasCheckConstraint(
                //        "CK_Status_Valid_Value",
                //        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                //            Common.EntityStatus.StatusToString(Common.EntityStatus.Disabled),
                //            Common.EntityStatus.StatusToString(Common.EntityStatus.Enabled),
                //            Common.EntityStatus.StatusToString(Common.EntityStatus.Readonly)
                //        )
                //    );
            });
        }
        #endregion
        #region Table SocialUserRole
        private static void ConfigureEnitySocialUserRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.SocialUserRole>(entityBuilder =>
            {
                //entityBuilder.HasKey (e => e.Id);
                //entityBuilder.Property(e => e.Id)
                //    .UseIdentityAlwaysColumn()
                //    .IsRequired();
                //entityBuilder.Property(e => e.RoleName)
                //    .IsRequired();
                //entityBuilder.Property(e => e.DisplayName)
                //    .IsRequired();
                //entityBuilder.Property(e => e.Describe)
                //    .IsRequired();
                //entityBuilder.Property(e => e.RightsStr)
                //    .HasDefaultValue("[]")
                //    .IsRequired();
                //entityBuilder.Property(e => e.StatusStr)
                //    .IsRequired();

                //entityBuilder.HasIndex(e => e.RoleName)
                //    .IsUnique()
                //    .HasFilter($"status != '{ Common.EntityStatus.StatusToString(Common.EntityStatus.Disabled) }'");
                //entityBuilder.HasData(Models.SocialUserRole.GetDefaultData());
                //entityBuilder
                //    .HasCheckConstraint(
                //        "CK_Status_Valid_Value",
                //        string.Format("status = '{0}' OR status = '{1}' OR status = '{2}'",
                //            Common.EntityStatus.StatusToString(Common.EntityStatus.Disabled),
                //            Common.EntityStatus.StatusToString(Common.EntityStatus.Enabled),
                //            Common.EntityStatus.StatusToString(Common.EntityStatus.Readonly)
                //        )
                //    );
            });
        }
        #endregion
        #endregion
        #region Functions of context
        #region Table AdminUser
        #region GET
        public Models.AdminUser GetAdminUserById(string Uuid, out string Error)
        {
            Error = null;
            return null;
            //Guid Id = new Guid(Uuid);
            //var Users = AdminUsers.Where(e => e.Id == Id &&
            //        e.Status != Common.SocialUserStatus.Deleted &&
            //        e.Status != Common.SocialUserStatus.InvalidStatus).ToList();

            //if (Users.Count == 1) {
            //    Error = "";
            //    Users[0].Rights = GetAdminRightsByRoles(Users[0].Roles);
            //    return Users[0];
            //} else if (Users.Count < 1)
            //{
            //    Error = $"AdminUser not found. Id: {Uuid}";
            //    return null;
            //} else {
            //    Error = $"Not expected result. Multi user was found with id: {Uuid}. Total users found: {Users.Count}.";
            //    return null;
            //}
        }
        public Models.AdminUser GetAdminUserByIdIgnoreStatus(string Uuid, out string Error)
        {
            Guid Id = new Guid(Uuid);
            var Users = AdminUsers.Where(e => e.Id == Id).ToList();

            if (Users.Count == 1) {
                Error = "";
                Users[0].Rights = GetAdminRightsByRoles(Users[0].Roles);
                return Users[0];
            }
            else if (Users.Count < 1) {
                Error = $"AdminUser not found. Id: {Uuid}";
                return null;
            } else {
                Error = $"Not expected result. Multi user was found with id: {Uuid}. Total users found: {Users.Count}.";
                return null;
            }
        }
        public Models.AdminUser GetAdminUserByUserName(string UserName, out string Error)
        {
            Error = null;
            return null;
            //var Users = AdminUsers
            //    .Where(e => e.UserName == UserName && 
            //        e.Status != Common.SocialUserStatus.Deleted && 
            //        e.Status != Common.SocialUserStatus.InvalidStatus).ToList();

            //if (Users.Count == 1) {
            //    Error = "";
            //    Users[0].Rights = GetAdminRightsByRoles(Users[0].Roles);
            //    return Users[0];
            //} else if (Users.Count < 1) {
            //    Error = $"AdminUser not found. UserName: {UserName}";
            //    return null;
            //} else {
            //    Error = $"Not expected result. Multi user was found with user name: {UserName}. Total users found: {Users.Count}.";
            //    return null;
            //}
        }
        public List<Models.AdminUser> GetAdminUserByUserNameIgnoreStatus(string UserName, out string Error)
        {
            var Users = AdminUsers
                .Where(e => e.UserName == UserName).ToList();

            if (Users.Count < 1) {
                Error = $"AdminUser not found. UserName: {UserName}";
                return null;
            }

            Error = "";
            foreach (var User in Users) {
                User.Rights = GetAdminRightsByRoles(User.Roles);
            }
            return Users;
        }
        public List<Models.AdminUser> GetAllAdminUser()
        {
            return new List<Models.AdminUser>();
            //var Users = AdminUsers
            //    .Where(e => e.Status != Common.SocialUserStatus.Deleted && 
            //        e.Status != Common.SocialUserStatus.InvalidStatus).ToList();

            //foreach (var User in Users) {
            //    User.Rights = GetAdminRightsByRoles(User.Roles);
            //}
            //return Users;
        }
        #endregion
        #region CREATE
        public bool InsertAdminUser(Models.AdminUser User, out string Error)
        {
            AdminUsers.Add(User);
            Error = "";
            try {
                if (SaveChanges() > 0) {
                    return true;
                }
            } catch (Exception ex) {
                if (ex is DbUpdateException || ex is DbUpdateConcurrencyException) {
                    // DbUpdateException: error while save changes
                    // DbUpdateConcurrencyException: rows affected not equals with expected rows
                    Error = $"Error when save new AdminUser. Message: {ex}";
                } else {
                    throw;
                }
            }
            return false;
        }
        #endregion
        #region UPDATE
        #endregion
        #region DELETE
        #endregion
        #endregion
        #region Table AdminUserRight
        #region GET
        public Models.AdminUserRight GetAdminUserRightById(int Id, out string Error)
        {
            Error = "";
            return null;
            //var Rights = AdminUserRights.Where(e => e.Id == Id &&
            //                e.Status != Common.EntityStatus.InvalidStatus &&
            //                e.Status != Common.EntityStatus.Disabled).ToList();

            //if (Rights.Count == 1) {
            //    Error = "";
            //    return Rights[0];
            //} else if (Rights.Count < 1) {
            //    Error = $"AdminUserRight not found. Id: {Id}";
            //    return null;
            //} else {
            //    Error = $"Not expected result. Multi user right was found with id: {Id}. Total rights found: {Rights.Count}.";
            //    return null;
            //}
        }
        public List<Models.AdminUserRight> GetAdminUserRightByIdIgnoreStatus(int Id, out string Error)
        {
            var Rights = AdminUserRights.Where(e => e.Id == Id).ToList();

            if (Rights.Count < 1) {
                Error = $"AdminUserRight not found. Id: {Id}";
                return null;
            }
            
            Error = "";
            return Rights;
        }
        public List<Models.AdminUserRight> GetAllAdminUserRight()
        {
            //var Rights = AdminUserRights.Where(e => e.Status != Common.EntityStatus.InvalidStatus &&
            //                e.Status != Common.EntityStatus.Disabled).ToList();
            //return Rights;
            return null;
        }
        #endregion
        #region CREATE
        #endregion
        #region UPDATE
        #endregion
        #region DELETE
        #endregion
        #endregion
        #region Table AdminUserRole
        #region GET
        public Models.AdminUserRole GetAdminUserRoleById(int Id, out string Error)
        {
            Error = "";
            return null;
            //var Roles = AdminUserRoles.Where(e => e.Id == Id &&
            //                e.Status != Common.EntityStatus.InvalidStatus &&
            //                e.Status != Common.EntityStatus.Disabled).ToList();

            //if (Roles.Count == 1)
            //{
            //    Error = "";
            //    return Roles[0];
            //}
            //else if (Roles.Count < 1)
            //{
            //    Error = $"AdminUserRole not found. Id: {Id}";
            //    return null;
            //}
            //else
            //{
            //    Error = $"Not expected result. Multi user right was found with id: {Id}. Total roles found: {Roles.Count}.";
            //    return null;
            //}
        }
        public List<Models.AdminUserRole> GetAdminUserRoleByIdIgnoreStatus(int Id, out string Error)
        {
            var Roles = AdminUserRoles.Where(e => e.Id == Id).ToList();

            if (Roles.Count < 1) {
                Error = $"AdminUserRole not found. Id: {Id}";
                return null;
            }
            
            Error = "";
            return Roles;
        }
        public Models.AdminUserRole GetAdminUserRoleByRoleName(string RoleName, out string Error)
        {
            Error = "";
            return null;
            //var Roles = AdminUserRoles.Where(e => e.RoleName == RoleName &&
            //                e.Status != Common.EntityStatus.InvalidStatus &&
            //                e.Status != Common.EntityStatus.Disabled).ToList();

            //if (Roles.Count == 1) {
            //    Error = "";
            //    return Roles[0];
            //} else if (Roles.Count < 1) {
            //    Error = $"AdminUserRole not found. RoleName: {RoleName}";
            //    return null;
            //} else {
            //    Error = $"Not expected result. Multi user right was found with RoleName: {RoleName}. Total roles found: {Roles.Count}.";
            //    return null;
            //}
        }
        public List<Models.AdminUserRole> GetAdminUserRoleByRoleNameIgnoreStatus(string RoleName, out string Error)
        {
            var Roles = AdminUserRoles.Where(e => e.RoleName == RoleName).ToList();

            if (Roles.Count < 1) {
                Error = $"AdminUserRole not found. RoleName: {RoleName}";
                return null;
            }
            
            Error = "";
            return Roles;
        }
        public List<Models.AdminUserRole> GetAllAdminUserRole()
        {
            return null;
            //var Roles = AdminUserRoles.Where(e => e.Status != Common.EntityStatus.InvalidStatus &&
            //                e.Status != Common.EntityStatus.Disabled).ToList();
            //return Roles;
        }
        #endregion
        #region CREATE
        #endregion
        #region UPDATE
        #endregion
        #region DELETE
        #endregion
        #endregion
        #region Table BaseConfig
        #region GET
        #endregion
        #region CREATE
        #endregion
        #region UPDATE
        #endregion
        #region DELETE
        #endregion
        #endregion
        #region Table ConfigAuditLog
        #region GET
        #endregion
        #region CREATE
        #endregion
        #region UPDATE
        #endregion
        #region DELETE
        #endregion
        #endregion
        #region Table SocialAuditLog
        #region GET
        #endregion
        #region CREATE
        #endregion
        #region UPDATE
        #endregion
        #region DELETE
        #endregion
        #endregion
        #region Table SocialUserRight
        #region GET
        #endregion
        #region CREATE
        #endregion
        #region UPDATE
        #endregion
        #region DELETE
        #endregion
        #endregion
        #region Table SocialUserRole
        #region GET
        #endregion
        #region CREATE
        #endregion
        #region UPDATE
        #endregion
        #region DELETE
        #endregion
        #endregion
        #endregion
        #region Function: GetAdminRightsByRoles        
        public Dictionary<string, List<string>> GetAdminRightsByRoles(List<string> Roles)
        {
            Dictionary<string, List<string>> Rights = new();
            foreach (var RoleName in Roles) {
                string Error;
                var SubRole = GetAdminUserRoleByRoleName(RoleName, out Error);
                if (Error.Length == 0) {
                    var SubRights = SubRole.Rights;
                    foreach (var Right in SubRights) {
                        if (!Rights.Keys.Contains(Right.Key)) {
                            Rights.Add(Right.Key, Right.Value);
                        } else {
                            var Abilities = Rights.GetValueOrDefault(Right.Key, new List<string>());
                            Abilities.AddRange(Right.Value);
                            Rights.Remove(Right.Key);
                            Rights.Add(Right.Key, Abilities.Distinct().ToList());
                        }
                    }
                }
            }
            return Rights;
        }
        #endregion
        #region Event Hanlde | override base function
        //public override int SaveChanges()
        //{
        //    var changedEntities = ChangeTracker
        //        .Entries()
        //        .Where(_ => _.State == EntityState.Added || 
        //                    _.State == EntityState.Modified);

        //    var errors = new List<ValidationResult>(); // all errors are here
        //    foreach (var e in changedEntities)
        //    {
        //        var vc = new ValidationContext(e.Entity);
        //        Validator.TryValidateObject(
        //            e.Entity, vc, errors, validateAllProperties: true);
        //    }

        //    return base.SaveChanges();
        //}
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using NpgsqlTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Common;
using Common;
using DatabaseAccess.Common.Actions;


#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_user")]
    public class SocialUser : BaseModel
    {
        [Key]
        [Column("id")]
        public Guid Id { get; private set; }
        [Required]
        [Column("first_name")]
        [StringLength(25)]
        public string FirstName { get; set; }
        [Required]
        [Column("last_name")]
        [StringLength(25)]
        public string LastName { get; set; }
        [Required]
        [Column("display_name")]
        [StringLength(50)]
        public string DisplayName { get; set; }
        [Required]
        [Column("user_name")]
        [StringLength(50)]
        public string UserName { get; set; }
        [Required]
        [Column("password")]
        [StringLength(32)]
        public string StorePassword { get; private set; }
        [NotMapped]
        public string Password {
            get => StorePassword;
            set => StorePassword = PasswordEncryptor.EncryptPassword(value, Salt); 
        }
        [Required]
        [Column("salt")]
        [StringLength(8)]
        public string Salt { get; private set; }
        [Required]
        [Column("email")]
        [StringLength(320)]
        public string Email { get; set; }
        [Column("description")]
        [StringLength(2048)]
        public string Description { get; set; }
        [Column("sex")]
        [StringLength(10)]
        public string Sex { get; set; }
        [Column("phone")]
        [StringLength(20)]
        public string Phone { get; set; }
        [Column("country")]
        [StringLength(20)]
        public string Country { get; set; }
        [Column("city")]
        [StringLength(20)]
        public string City { get; set; }
        [Column("province")]
        [StringLength(20)]
        public string Province { get; set; }
        [Column("verified_email")]
        public bool VerifiedEmail { get; set; }
        [Column("avatar")]
        public string Avatar { get; set; }
        [NotMapped]
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => BaseStatus.StatusToString(Status, EntityStatus.AdminUserStatus);
            set => Status = BaseStatus.StatusFromString(value, EntityStatus.AdminUserStatus);
        }
        [NotMapped]
        public Dictionary<string, JObject> Rights { get => GetRights(); }
        [NotMapped]
        public List<string> Roles { get => GetRoles(); }
        [NotMapped]
        public JObject Settings { get; set; }
        [Required]
        [Column("settings", TypeName = "jsonb")]
        public string SettingsStr {
            get { return Settings.ToString(); }
            set { Settings = JsonConvert.DeserializeObject<JObject>(value); }
        }
        [NotMapped]
        public JObject Ranks { get; set; }
        [Required]
        [Column("ranks", TypeName = "jsonb")]
        public string RanksStr {
            get { return Ranks.ToString(); }
            set { Ranks = JsonConvert.DeserializeObject<JObject>(value); }
        }
        [NotMapped]
        public JArray Publics { get; set; }
        [Required]
        [Column("publics", TypeName = "jsonb")]
        public string PublicsStr {
            get { return Publics.ToString(); }
            set { Publics = JsonConvert.DeserializeObject<JArray>(value); }
        }
        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; set; }
        [Column("last_access_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastAccessTimestamp { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; private set; }

        [InverseProperty(nameof(SessionSocialUser.User))]
        public virtual ICollection<SessionSocialUser> SessionSocialUsers { get; set; }
        [InverseProperty(nameof(SocialUserAuditLog.User))]
        public virtual ICollection<SocialUserAuditLog> SocialUserAuditLogs { get; set; }
        [InverseProperty(nameof(SocialComment.OwnerNavigation))]
        public virtual ICollection<SocialComment> SocialComments { get; set; }
        [InverseProperty(nameof(SocialNotification.User))]
        public virtual ICollection<SocialNotification> SocialNotifications { get; set; }
        [InverseProperty(nameof(SocialPost.OwnerNavigation))]
        public virtual ICollection<SocialPost> SocialPosts { get; set; }
        [InverseProperty(nameof(SocialReport.User))]
        public virtual ICollection<SocialReport> SocialReports { get; set; }
        [InverseProperty(nameof(SocialUserActionWithCategory.User))]
        public virtual ICollection<SocialUserActionWithCategory> SocialUserActionWithCategories { get; set; }
        [InverseProperty(nameof(SocialUserActionWithComment.User))]
        public virtual ICollection<SocialUserActionWithComment> SocialUserActionWithComments { get; set; }
        [InverseProperty(nameof(SocialUserActionWithPost.User))]
        public virtual ICollection<SocialUserActionWithPost> SocialUserActionWithPosts { get; set; }
        [InverseProperty(nameof(SocialUserActionWithTag.User))]
        public virtual ICollection<SocialUserActionWithTag> SocialUserActionWithTags { get; set; }
        [InverseProperty(nameof(SocialUserActionWithUser.UserIdDesNavigation))]
        public virtual ICollection<SocialUserActionWithUser> SocialUserActionWithUserUserIdDesNavigations { get; set; }
        [InverseProperty(nameof(SocialUserActionWithUser.User))]
        public virtual ICollection<SocialUserActionWithUser> SocialUserActionWithUserUsers { get; set; }
        [InverseProperty(nameof(SocialUserRoleOfUser.User))]
        public virtual ICollection<SocialUserRoleOfUser> SocialUserRoleOfUsers { get; set; }

        public SocialUser()
        {
            SessionSocialUsers = new HashSet<SessionSocialUser>();
            SocialUserAuditLogs = new HashSet<SocialUserAuditLog>();
            SocialComments = new HashSet<SocialComment>();
            SocialNotifications = new HashSet<SocialNotification>();
            SocialPosts = new HashSet<SocialPost>();
            SocialReports = new HashSet<SocialReport>();
            SocialUserActionWithCategories = new HashSet<SocialUserActionWithCategory>();
            SocialUserActionWithComments = new HashSet<SocialUserActionWithComment>();
            SocialUserActionWithPosts = new HashSet<SocialUserActionWithPost>();
            SocialUserActionWithTags = new HashSet<SocialUserActionWithTag>();
            SocialUserActionWithUserUserIdDesNavigations = new HashSet<SocialUserActionWithUser>();
            SocialUserActionWithUserUsers = new HashSet<SocialUserActionWithUser>();
            SocialUserRoleOfUsers = new HashSet<SocialUserRoleOfUser>();

            __ModelName = "SocialUser";
            Id = Guid.NewGuid();
            CreatedTimestamp = DateTime.UtcNow;
            Status = SocialUserStatus.Activated;
            Salt = PasswordEncryptor.GenerateSalt();
            Publics = GetDefaultPublicFields();
            SettingsStr = "{}";
            RanksStr = "{}";
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
            try {
                var parser = (ParserModels.ParserSocialUser)Parser;
                FirstName = parser.first_name;
                LastName = parser.last_name;
                DisplayName = parser.display_name;
                Password = parser.password;
                Email = parser.email;
                Sex = parser.sex;
                Phone = parser.phone;
                Country = parser.country;
                City = parser.city;
                Province = parser.province;
                Avatar = parser.avatar;
                UserName = Utils.GenerateUserName();

                if (parser.settings != default) {
                    Settings = parser.settings;
                } else {
                    SettingsStr = "{}";
                }
                if (DisplayName == default) {
                    DisplayName = $"{ FirstName } { LastName }";
                }

                return true;
            } catch (Exception ex) {
                Error = ex.ToString();
                return false;
            }
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>()
            {
                { "id", Id },
                { "first_name", FirstName },
                { "last_name", LastName },
                { "display_name", DisplayName },
                { "user_name", UserName },
                { "email", Email },
                { "description", Description },
                { "sex", Sex },
                { "phone", Phone },
                { "country", Country },
                { "city", City },
                { "province", Province },
                { "verified_email", VerifiedEmail },
                { "avatar", Avatar },
                { "status", StatusStr },
                { "roles", Roles },
                { "rights", Rights },
                { "settings", Settings },
                { "publics", Publics },
                { "ranks", Ranks },
                { "followers", CountFollowers() },
                { "posts", CountPosts() },
                { "views", CountViews() },
                { "likes", CountLikes() },
                { "last_access_timestamp", LastAccessTimestamp },
                { "created_timestamp", CreatedTimestamp },
#if DEBUG
                { "password", Password },
                { "salt", Salt },
                { "__ModelName", __ModelName }
#endif
            };
            return true;
        }

        public override JObject GetPublicJsonObject(List<string> publicFields = default)
        {
            if (publicFields == default) {
                publicFields = new List<string>();
                foreach(var item in Publics.ToArray()) {
                    publicFields.Add(item.ToString());
                }
            }
            var ret = GetJsonObject();
            foreach (var x in __ObjectJson) {
                if (!publicFields.Contains(x.Key)) {
                    ret.Remove(x.Key);
                }
            }
            return ret;
        }

        public JArray GetDefaultPublicFields()
        {
            return new JArray(){
                "display_name",
                "user_name",
                "email",
                "description",
                "sex",
                "country",
                "avatar",
                "status",
                "ranks",
                "publics",
                "followers",
                "posts",
                "views",
                "likes",
            };
        }

        #region Handle default data
        public int CountPosts()
        {
            return SocialPosts.Count();
        }

        public int CountLikes()
        {
            return SocialPosts
                .Sum(
                    e =>
                        e.Owner == this.Id ?
                        e.SocialUserActionWithPosts
                        .Count(ac => ac.ActionsStr.Contains(
                                BaseAction.ActionToString(UserActionWithPost.Like, EntityAction.UserActionWithPost))
                        ) : 0
                );
        }

        public int CountViews()
        {
            return SocialPosts.Sum(e => e.Views);
        }

        public int CountFollowing()
        {
            return SocialUserActionWithUserUsers
                .Count(e => e.ActionsStr.Contains(
                                BaseAction.ActionToString(UserActionWithUser.Follow, EntityAction.UserActionWithUser)
                    )
                );
        }

        public int CountFollowers()
        {
            return SocialUserActionWithUserUserIdDesNavigations
                .Count(e => e.ActionsStr.Contains(
                                BaseAction.ActionToString(UserActionWithUser.Follow, EntityAction.UserActionWithUser)
                    )
                );
        }

        public List<string> GetRoles()
        {
            return SocialUserRoleOfUsers.Select(e => e.Role.RoleName).ToList();
        }

        public Dictionary<string, JObject> GetRights()
        {
            Dictionary<string, JObject> rights = new();
            var allRoleDetails = SocialUserRoleOfUsers
                .Select(e => e.Role.SocialUserRoleDetails)
                .ToList();

            foreach(var roleDetails in allRoleDetails) {
                foreach(var detail in roleDetails) {
                    var _obj = rights.GetValueOrDefault(detail.Right.RightName, new JObject());
                    var obj = detail.Actions;
                    JObject action;
                    if (_obj.Count != 0) {
                        try {
                            var _read = _obj.Value<bool>("read");
                            var _write = _obj.Value<bool>("write");
                            var read = obj.Value<bool>("read") ? true : _read;
                            var write = obj.Value<bool>("write") ? true : _write;
                            rights.Remove(detail.Right.RightName);
                            action = new JObject {
                                { "read", read },
                                { "write", write }
                            };
                        } catch (Exception) {
                            action = _obj;
                        }
                        rights.Add(detail.Right.RightName, action);
                    } else {
                        rights.Add(detail.Right.RightName, obj);
                    }
                }
            }
            return rights;
        }

        private static Guid AdminUserId = Guid.NewGuid();
        private static string AdminUserName = "admin";

        public static Guid GetAdminUserId()
        {
            return AdminUserId;
        }

        public static string GetAdminUserName()
        {
            return AdminUserName;
        }
        #endregion
        #region Handle session user
        public List<string> GetExpiredSessions(int ExpiryTime) // minute
        {
            var now = DateTime.UtcNow;
            return SessionSocialUsers
                    .Where(e => (now - e.LastInteractionTime.ToUniversalTime()).TotalMinutes >= ExpiryTime && e.Saved == false)
                    .Select(e => e.SessionToken)
                    .ToList();
        }
        public void SessionExtension(string SessionToken, int ExtensionTime) // minute
        {
            var now = DateTime.UtcNow.AddMinutes(ExtensionTime);
            var session = SessionSocialUsers.Where<SessionSocialUser>(e => e.SessionToken == SessionToken).ToList().First();
            session.LastInteractionTime = now;
        }
        #endregion
    
    }
}

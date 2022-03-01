using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Common;


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
        [NotMapped]
        private string StorePassword;
        [Required]
        [Column("password")]
        [StringLength(32)]
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
        public List<string> Roles { get; set; }
        [Required]
        [Column("roles", TypeName = "json")]
        public string RolesStr {
            get { return JsonConvert.SerializeObject(Roles).ToString(); }
            set { Roles = JsonConvert.DeserializeObject<List<string>>(value); }
        }
        [NotMapped]
        public Dictionary<string, List<string>> Rights { get; set; }
        [NotMapped]
        public JObject Settings { get; set; }
        [Required]
        [Column("settings", TypeName = "json")]
        public string SettingsStr {
            get { return Settings.ToString(); }
            set { Settings = JsonConvert.DeserializeObject<JObject>(value); }
        }
        [NotMapped]
        public JObject Ranks { get; set; }
        [Required]
        [Column("ranks", TypeName = "json")]
        public string RanksStr {
            get { return Ranks.ToString(); }
            set { Ranks = JsonConvert.DeserializeObject<JObject>(value); }
        }
        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; set; }
        [Column("last_access_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastAccessTimestamp { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; private set; }

        [InverseProperty(nameof(SessionSocialUser.User))]
        public virtual ICollection<SessionSocialUser> SessionSocialUsers { get; set; }
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

        public SocialUser()
        {
            SessionSocialUsers = new HashSet<SessionSocialUser>();
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

            __ModelName = "SocialUser";
            Id = Guid.NewGuid();
            CreatedTimestamp = DateTime.UtcNow;
            Status = SocialUserStatus.Activated;
            Salt = PasswordEncryptor.GenerateSalt();
            RolesStr = "[]";
            SettingsStr = "{}";
            RanksStr = "{}";
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserSocialUser)Parser;
                FirstName = parser.first_name;
                LastName = parser.last_name;
                DisplayName = parser.display_name;
                UserName = parser.user_name;
                Password = parser.password;
                Email = parser.email;
                Sex = parser.sex;
                Phone = parser.phone;
                Country = parser.country;
                City = parser.city;
                Province = parser.province;
                Avatar = parser.avatar;
                Settings = parser.settings;

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
                { "display_name", DisplayName },
                { "email", Email },
                { "sex", Sex },
                { "phone", Phone },
                { "country", Country },
                { "city", City },
                { "province", Province },
                { "verified_email", VerifiedEmail },
                { "avatar", Avatar },
                { "status", StatusStr },
                { "roles", Roles},
                { "rights", Rights},
                { "settings", Settings},
                { "ranks", Ranks },
                { "last_access_timestamp", LastAccessTimestamp},
                { "created_timestamp", CreatedTimestamp },
#if DEBUG
                { "password", Password },
                { "salt", Salt },
                { "__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}

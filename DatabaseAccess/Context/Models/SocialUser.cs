using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_user")]
    public partial class SocialUser
    {
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
        }

        [Key]
        [Column("id")]
        public Guid Id { get; set; }
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
        public string Password { get; set; }
        [Required]
        [Column("salt")]
        [StringLength(8)]
        public string Salt { get; set; }
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
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string Status { get; set; }
        [Required]
        [Column("roles", TypeName = "json")]
        public string Roles { get; set; }
        [Required]
        [Column("settings", TypeName = "json")]
        public string Settings { get; set; }
        [Required]
        [Column("ranks", TypeName = "json")]
        public string Ranks { get; set; }
        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; set; }
        [Column("last_access_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastAccessTimestamp { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; set; }

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
    }
}

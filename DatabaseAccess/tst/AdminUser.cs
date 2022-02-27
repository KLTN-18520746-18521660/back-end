using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.tst
{
    [Table("admin_user")]
    public partial class AdminUser
    {
        public AdminUser()
        {
            SessionAdminUsers = new HashSet<SessionAdminUser>();
        }

        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        [Required]
        [Column("user_name")]
        [StringLength(50)]
        public string UserName { get; set; }
        [Required]
        [Column("display_name")]
        [StringLength(50)]
        public string DisplayName { get; set; }
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
        [Column("last_access_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastAccessTimestamp { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; set; }

        [InverseProperty(nameof(SessionAdminUser.User))]
        public virtual ICollection<SessionAdminUser> SessionAdminUsers { get; set; }
    }
}

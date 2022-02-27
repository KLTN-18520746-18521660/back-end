using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.tst
{
    [Table("session_admin_user")]
    public partial class SessionAdminUser
    {
        [Key]
        [Column("session_token")]
        [StringLength(30)]
        public string SessionToken { get; set; }
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Column("saved")]
        public bool Saved { get; set; }
        [Required]
        [Column("data", TypeName = "json")]
        public string Data { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; set; }
        [Column("last_interaction_time", TypeName = "timestamp with time zone")]
        public DateTime? LastInteractionTime { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(AdminUser.SessionAdminUsers))]
        public virtual AdminUser User { get; set; }
    }
}

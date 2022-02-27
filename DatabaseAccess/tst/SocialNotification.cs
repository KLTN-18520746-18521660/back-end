using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.tst
{
    [Table("social_notification")]
    public partial class SocialNotification
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string Status { get; set; }
        [Required]
        [Column("content", TypeName = "json")]
        public string Content { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; set; }
        [Column("last_modified_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastModifiedTimestamp { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialNotifications))]
        public virtual SocialUser User { get; set; }
    }
}

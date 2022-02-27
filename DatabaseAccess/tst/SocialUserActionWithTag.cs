using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.tst
{
    [Table("social_user_action_with_tag")]
    public partial class SocialUserActionWithTag
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Key]
        [Column("tag_id")]
        public long TagId { get; set; }
        [Required]
        [Column("actions", TypeName = "json")]
        public string Actions { get; set; }

        [ForeignKey(nameof(TagId))]
        [InverseProperty(nameof(SocialTag.SocialUserActionWithTags))]
        public virtual SocialTag Tag { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialUserActionWithTags))]
        public virtual SocialUser User { get; set; }
    }
}

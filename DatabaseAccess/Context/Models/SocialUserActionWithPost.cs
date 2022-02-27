using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_user_action_with_post")]
    public partial class SocialUserActionWithPost
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Key]
        [Column("post_id")]
        public long PostId { get; set; }
        [Required]
        [Column("actions", TypeName = "json")]
        public string Actions { get; set; }

        [ForeignKey(nameof(PostId))]
        [InverseProperty(nameof(SocialPost.SocialUserActionWithPosts))]
        public virtual SocialPost Post { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialUserActionWithPosts))]
        public virtual SocialUser User { get; set; }
    }
}

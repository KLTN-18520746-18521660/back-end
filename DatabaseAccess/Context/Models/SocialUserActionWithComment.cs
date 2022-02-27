using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_user_action_with_comment")]
    public partial class SocialUserActionWithComment
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Key]
        [Column("comment_id")]
        public long CommentId { get; set; }
        [Required]
        [Column("actions", TypeName = "json")]
        public string Actions { get; set; }

        [ForeignKey(nameof(CommentId))]
        [InverseProperty(nameof(SocialComment.SocialUserActionWithComments))]
        public virtual SocialComment Comment { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialUserActionWithComments))]
        public virtual SocialUser User { get; set; }
    }
}

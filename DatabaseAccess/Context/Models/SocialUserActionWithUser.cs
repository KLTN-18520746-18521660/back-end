using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_user_action_with_user")]
    public partial class SocialUserActionWithUser
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Key]
        [Column("user_id_des")]
        public Guid UserIdDes { get; set; }
        [Required]
        [Column("actions", TypeName = "json")]
        public string Actions { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialUserActionWithUserUsers))]
        public virtual SocialUser User { get; set; }
        [ForeignKey(nameof(UserIdDes))]
        [InverseProperty(nameof(SocialUser.SocialUserActionWithUserUserIdDesNavigations))]
        public virtual SocialUser UserIdDesNavigation { get; set; }
    }
}

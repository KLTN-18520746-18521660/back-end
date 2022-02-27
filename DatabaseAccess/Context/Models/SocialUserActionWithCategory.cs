using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_user_action_with_category")]
    public partial class SocialUserActionWithCategory
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Key]
        [Column("category_id")]
        public long CategoryId { get; set; }
        [Required]
        [Column("actions", TypeName = "json")]
        public string Actions { get; set; }

        [ForeignKey(nameof(CategoryId))]
        [InverseProperty(nameof(SocialCategory.SocialUserActionWithCategories))]
        public virtual SocialCategory Category { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialUserActionWithCategories))]
        public virtual SocialUser User { get; set; }
    }
}

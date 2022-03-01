using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

#nullable disable

namespace DatabaseAccess.tst
{
    [Table("social_category")]
    public partial class SocialCategory
    {
        public SocialCategory()
        {
            InverseParent = new HashSet<SocialCategory>();
            SocialPostCategories = new HashSet<SocialPostCategory>();
            SocialUserActionWithCategories = new HashSet<SocialUserActionWithCategory>();
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("parent_id")]
        public long? ParentId { get; set; }
        [Required]
        [Column("name")]
        [StringLength(20)]
        public string Name { get; set; }
        [Required]
        [Column("display_name")]
        [StringLength(50)]
        public string DisplayName { get; set; }
        [Required]
        [Column("describe")]
        [StringLength(100)]
        public string Describe { get; set; }
        [Required]
        [Column("slug")]
        public string Slug { get; set; }
        [Column("thumbnail")]
        public string Thumbnail { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string Status { get; set; }
        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; set; }
        [Column("last_modified_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastModifiedTimestamp { get; set; }

        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(SocialCategory.InverseParent))]
        public virtual SocialCategory Parent { get; set; }
        [InverseProperty(nameof(SocialCategory.Parent))]
        public virtual ICollection<SocialCategory> InverseParent { get; set; }
        [InverseProperty(nameof(SocialPostCategory.Category))]
        public virtual ICollection<SocialPostCategory> SocialPostCategories { get; set; }
        [InverseProperty(nameof(SocialUserActionWithCategory.Category))]
        public virtual ICollection<SocialUserActionWithCategory> SocialUserActionWithCategories { get; set; }
    }
}

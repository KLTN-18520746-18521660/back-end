using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_post_category")]
    public class SocialPostCategory : BaseModel
    {
        [Key]
        [Column("post_id")]
        public long PostId { get; set; }
        [Key]
        [Column("category_id")]
        public long CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        [InverseProperty(nameof(SocialCategory.SocialPostCategories))]
        public virtual SocialCategory Category { get; set; }
        [ForeignKey(nameof(PostId))]
        [InverseProperty(nameof(SocialPost.SocialPostCategories))]
        public virtual SocialPost Post { get; set; }

        public SocialPostCategory()
        {
            __ModelName = "SocialPostCategory";
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "Not Implemented Error";
            return false;
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "post_id", PostId },
                { "category_id", CategoryId },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}

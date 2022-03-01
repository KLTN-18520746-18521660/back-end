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
    [Table("social_post_tag")]
    public class SocialPostTag : BaseModel
    {
        [Key]
        [Column("post_id")]
        public long PostId { get; set; }
        [Key]
        [Column("tag_id")]
        public long TagId { get; set; }

        [ForeignKey(nameof(PostId))]
        [InverseProperty(nameof(SocialPost.SocialPostTags))]
        public virtual SocialPost Post { get; set; }
        [ForeignKey(nameof(TagId))]
        [InverseProperty(nameof(SocialTag.SocialPostTags))]
        public virtual SocialTag Tag { get; set; }
        
        public SocialPostTag()
        {
            __ModelName = "SocialPostTag";
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
                { "tag_id", TagId },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}

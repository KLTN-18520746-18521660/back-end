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
using DatabaseAccess.Common.Status;


#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_user_action_with_category")]
    public class SocialUserActionWithCategory : BaseModel
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Key]
        [Column("category_id")]
        public long CategoryId { get; set; }
        [NotMapped]
        public JArray Actions { get; set; }
        [Required]
        [Column("actions", TypeName = "json")]
        public string ActionsStr {
            get { return Actions.ToString(); }
            set { Actions = JsonConvert.DeserializeObject<JArray>(value); }
        }

        [ForeignKey(nameof(CategoryId))]
        [InverseProperty(nameof(SocialCategory.SocialUserActionWithCategories))]
        public virtual SocialCategory Category { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialUserActionWithCategories))]
        public virtual SocialUser User { get; set; }

        public SocialUserActionWithCategory()
        {
            __ModelName = "SocialUserActionWithCategory";
            ActionsStr = "[]";
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
                { "user_id", UserId },
                { "category_id", CategoryId },
                { "actions", Actions },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}

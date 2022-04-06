﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Newtonsoft.Json;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_user_audit_log")]
    public class SocialUserAuditLog : BaseModel
    {
        [Key]
        [Column("id")]
        public int Id { get; private set; }
        [Required]
        [Column("table")]
        [StringLength(50)]
        public string Table { get; set; }
        [Required]
        [Column("table_key")]
        [StringLength(100)]
        public string TableKey { get; set; }
        [Required]
        [Column("action")]
        [StringLength(50)]
        public string Action { get; set; }
        [NotMapped]
        public LogValue OldValue { get; set; }
        [Required]
        [Column("old_value", TypeName = "TEXT")]
        public string OldValueStr
        {
            get { return OldValue.ToString(); }
            set { OldValue = new LogValue(value); }
        }
        [NotMapped]
        public LogValue NewValue { get; set; }
        [Required]
        [Column("new_value", TypeName = "TEXT")]
        public string NewValueStr
        {
            get { return NewValue.ToString(); }
            set { NewValue = new LogValue(value); }
        }
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Column("timestamp", TypeName = "timestamp with time zone")]
        public DateTime Timestamp { get; private set; }
        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialUserAuditLogs))]
        public virtual SocialUser User { get; set; }

        public SocialUserAuditLog()
        {
            __ModelName = "SocialUserAuditLog";
            NewValueStr = "[]";
            OldValueStr = "[]";
            Timestamp = DateTime.UtcNow;
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
                { "id", Id },
                { "table", Table },
                { "table_key", TableKey },
                { "action", Action },
                { "old_value", OldValue.Data },
                { "new_value", NewValue.Data },
                { "user_id", UserId },
                { "timestamp", Timestamp },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}
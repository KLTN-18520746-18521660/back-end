
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DatabaseAccess.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using DatabaseAccess.Common.Interface;

namespace DatabaseAccess.Contexts.ConfigDB.Models
{
    [Table("social_audit_log")]
    public class SocialAuditLog : BaseModel
    {
        public SocialAuditLog()
        {
            __ModelName = "ConfigAuditLog";
            NewValueStr = "{Data: []}";
            OldValueStr = "{Data: []}";
            Timestamp = DateTime.UtcNow;
        }
        
        public override bool Parse(IBaseParserModel Parser, string Error = null)
        {
            throw new NotImplementedException();
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "id", Id },
                { "table", Table },
                { "table_key", TableKey },
                { "action", Action },
                { "old_value", OldValue },
                { "new_value", NewValue },
                { "user", User },
                { "timestamp", Timestamp },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }

        [Column("id", TypeName = "INTEGER")]
        public int Id { get; private set; }
        
        [Column("table", TypeName = "VARCHAR(50)")]
        public string Table { get; set; }
        
        [Column("table_key", TypeName = "VARCHAR(100)")]
        public string TableKey { get; set; }
        
        [Column("action", TypeName = "VARCHAR(50)")]
        public string Action { get; set; }

        [NotMapped]
        public LogValue OldValue { get; set; }
        [Column("old_value", TypeName = "TEXT")]
        public string OldValueStr
        {
            get { return OldValue.ToString(); }
            set { OldValue = JsonConvert.DeserializeObject<LogValue>(value); }
        }

        [NotMapped]
        public LogValue NewValue { get; set; }
        [Column("new_value", TypeName = "TEXT")]
        public string NewValueStr
        {
            get { return NewValue.ToString(); }
            set { NewValue = JsonConvert.DeserializeObject<LogValue>(value); }
        }
        
        [Column("user", TypeName = "VARCHAR(50)")]
        public string User { get; set; }
        
        [Column("timestamp", TypeName = "TIMESTAMPTZ")]
        public DateTime Timestamp { get; private set; }

        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Common.Actions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("admin_base_config")]
    public class AdminBaseConfig : BaseModel
    {
        [Key]
        [Column("id")]
        public int Id { get; private set; }
        [Required]
        [Column("config_key")]
        [StringLength(50)]
        public string ConfigKey { get; set; }
        [NotMapped]
        public JObject Value { get; set; }
        [Column("value", TypeName = "JSON")]
        public string ValueStr  {
            get { return Value.ToString(); }
            set { Value = JsonConvert.DeserializeObject<JObject>(value); }
        }
        [NotMapped]
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr
        {
            get => BaseStatus.StatusToString(Status, EntityStatus.AdminBaseConfigStatus);
            set => Status = BaseStatus.StatusFromString(value, EntityStatus.AdminBaseConfigStatus);
        }
        public AdminBaseConfig()
        {
            __ModelName = "BaseConfig";
            ConfigKey = "";
            ValueStr = "{}";
            Status = AdminBaseConfigStatus.Enabled;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserAdminBaseConfig)Parser;
                ConfigKey = parser.config_key;
                Value = parser.value;
                return true;
            } catch (Exception ex) {
                Error = ex.ToString();
                return false;
            }
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "id", Id },
                { "config_key", ConfigKey },
                { "value", Value },
                { "status", StatusStr },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }

        public static List<AdminBaseConfig> GetDefaultData()
        {
            List<AdminBaseConfig> ListData = new()
            {

            };
            return ListData;
        }
    }
}

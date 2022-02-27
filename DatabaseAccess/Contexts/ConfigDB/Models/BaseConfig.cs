
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common.Interface;
namespace DatabaseAccess.Contexts.ConfigDB.Models
{
    [Table("base_config")]
    public class BaseConfig : BaseModel
    {
        public BaseConfig()
        {
            __ModelName = "BaseConfig";
            ConfigKey = "";
            ValueStr = "{}";
            //Status = EntityStatus.Enabled;
        }

        public static List<BaseConfig> GetDefaultData()
        {
            List<BaseConfig> ListData = new()
            {

            };
            return ListData;
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "id", Id },
                { "config_key", ConfigKey },
                { "value", Value },
                { "status", Status },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserBaseConfig)Parser;
                ConfigKey = parser.config_key;
                Value = parser.value;
                return true;
            } catch (Exception ex) {
                Error ??= ex.ToString();
                return false;
            }
        }

        [Column("id", TypeName = "INTEGER")]
        public int Id { get; private set; }

        [Column("config_key", TypeName = "VARCHAR(50)")]
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
        [Column("status", TypeName = "VARCHAR(20)")]
        public string StatusStr
        {
            get => "";
            set => Status = 1;
        }
    }
}
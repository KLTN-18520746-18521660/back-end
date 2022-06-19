using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccess.Common.Interface;
using System.ComponentModel;
using CoreApi.Common.Interface;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Context.Models;
using CoreApi.Common;
using Newtonsoft.Json;

namespace CoreApi.Models.ModifyModels
{
    public class SocialPostModifyModel
    {
        [DefaultValue("Modify title")]
        public string title { get; set; }
        [DefaultValue("Modify thumbnail")]
        public string thumbnail { get; set; }
        [DefaultValue("Modify short post")]
        public string short_content { get; set; }
        [DefaultValue("Modify post body. Dummy value.")]
        public string content { get; set; }
        [DefaultValue(25)]
        public int? time_read { get; set; }
        [DefaultValue("MARKDOWN")]
        public string content_type { get; set; }
        [DefaultValue("[\"develop\"]")]
        public string[] categories { get; set; }
        [DefaultValue("[\"new_tag\"]")]
        public string[] tags { get; set; }

        public static SocialPostModifyModel FromJson(JObject obj)
        {
            return JsonConvert.DeserializeObject<SocialPostModifyModel>(obj.ToString());
        }

        public JObject ToJsonObject()
        {
            return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(this));
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CoreApi.Models
{
    public class LoginModel
    {
        [DefaultValue("admin")]
        public string user_name { get; set; }
        [DefaultValue("admin")]
        public string password { get; set; }
        [DefaultValue(false)]
        public bool remember { get; set; }
        [DefaultValue(null)]
        public JObject data { get; set; }
    }
}
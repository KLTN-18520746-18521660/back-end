using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CoreApi.Models
{
    public class LoginModel
    {
        [DefaultValue("user_name")]
        public string user_name { get; set; }
        [DefaultValue("password")]
        public string password { get; set; }
        [DefaultValue(true)]
        public bool remember { get; set; }
        [DefaultValue("{}")]
        public JObject data { get; set; }
    }
}
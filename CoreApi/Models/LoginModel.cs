using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace CoreApi.Models
{
    public class LoginModel
    {
        public string user_name { get; set; }
        public string password { get; set; }
        public bool remember { get; set; }
        public JObject data { get; set; }
    }
}
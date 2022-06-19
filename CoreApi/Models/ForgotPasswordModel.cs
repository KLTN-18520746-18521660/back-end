using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CoreApi.Models
{
    public class ForgotPasswordModel
    {
        [DefaultValue("email@email.com")]
        public string user_name { get; set; }
    }
}
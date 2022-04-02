using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CoreApi.Models
{
    public class ConfirmUserModel
    {
        [DefaultValue("password")]
        public string password { get; set; }
    }
}
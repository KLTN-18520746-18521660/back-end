using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CoreApi.Models
{
    public class ChangePasswordModel
    {
        
        [DefaultValue("password")]
        public string old_password { get; set; }
        [DefaultValue("Password123$")]
        public string new_password { get; set; }
    }
}
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
        [DefaultValue("tmp")]
        public string i { get; set; }
        [DefaultValue("tmp")]
        public string d { get; set; }
        [DefaultValue("tmp")]
        public string s { get; set; }
    }
}
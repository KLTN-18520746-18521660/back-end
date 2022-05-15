using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CoreApi.Models
{
    public class NewPasswordModel
    {
        [DefaultValue("Password123$")]
        public string new_password  { get; set; }
        [DefaultValue("tmp")]
        public string i             { get; set; } // Id
        [DefaultValue("tmp")]
        public string d             { get; set; } // Date
        [DefaultValue("tmp")]
        public string s             { get; set; } // Sate
    }
}
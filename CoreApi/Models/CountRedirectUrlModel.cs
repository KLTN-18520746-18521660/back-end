using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CoreApi.Models
{
    public class CountRedirectUrlModel
    {
        [DefaultValue("http://localhost/api/swagger")]
        public string url { get; set; }
    }
}
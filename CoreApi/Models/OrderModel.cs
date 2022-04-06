using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CoreApi.Models
{
    public class OrderModel
    {
        [DefaultValue("created_timestamp")]
        public string order_field { get; set; }
        [DefaultValue(false)]
        public bool is_desc { get; set; }
    }
}
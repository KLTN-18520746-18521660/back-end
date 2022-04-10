using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;


namespace CoreApi.Models
{
    public class OrderModel
    {
        [DefaultValue("views")]
        [FromQuery(Name = "order")]
        public string[] orders { get; set; }
        [DefaultValue("true")]
        [FromQuery(Name = "desc")]
        public bool[] descs { get; set; }

        public (string, bool)[] GetOrders() {
            var ret = new List<(string, bool)>();
            for (int i = 0; i < orders.Length; i++) {
                ret.Add(new (
                    orders[i],
                    descs.Length > i ? descs[i] : false
                ));
            }
            return ret.ToArray();
        }
    }
}
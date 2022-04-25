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
        [DefaultValue("views,created_timestamp")]
        public string sort_by { get; set; }
        [DefaultValue("desc,asc")]
        public string order { get; set; }

        public bool IsValid()
        {
            if (order != default) {
                foreach (var o in order.Split(',')) {
                    if (o != "asc" && o != "desc") {
                        return false;
                    }
                }
            }
            return true;
        }

        public (string, bool)[] GetOrders()
        {
            var ret = new List<(string, bool)>();
            var _orders = sort_by != default ? sort_by.Split(',') : new string[]{};
            var _descs = order != default ? order.Split(',') : new string[]{};
            if (_orders != default) {
                for (int i = 0; i < _orders.Length; i++) {
                    ret.Add(new (
                        _orders[i],
                        _descs != default && _descs.Length > i
                            ? (_descs[i] == "desc" ? true : false)
                            : false
                    ));
                }
            }
            return ret.ToArray();
        }
    }
}
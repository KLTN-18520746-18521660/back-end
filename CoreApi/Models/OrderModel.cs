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
        [FromQuery(Name = "sort_by")]
        public string SortBy { get; set; }
        [FromQuery(Name = "order")]
        public string Order { get; set; }

        public bool IsValid()
        {
            if (Order != default) {
                foreach (var _Order in Order.Split(',')) {
                    if (_Order != "asc" && _Order != "desc") {
                        return false;
                    }
                }
            }
            return true;
        }

        public (string, bool)[] GetOrders()
        {
            if (!IsValid()) {
                return default;
            }
            var Ret     = new List<(string, bool)>();
            var _Orders = SortBy != default ? SortBy.Split(',') : new string[]{};
            var _Descs  = Order != default ? Order.Split(',') : new string[]{};
            if (_Orders != default) {
                for (int i = 0; i < _Orders.Length; i++) {
                    Ret.Add(new (
                        _Orders[i],
                        _Descs != default && _Descs.Length > i
                            ? (_Descs[i] == "desc" ? true : false)
                            : false
                    ));
                }
            }
            return Ret.ToArray();
        }
    }
}
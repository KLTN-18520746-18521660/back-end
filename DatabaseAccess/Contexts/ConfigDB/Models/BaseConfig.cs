
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

using DatabaseAccess.CommonModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace DatabaseAccess.Contexts.ConfigDB.Models
{
    public class BaseConfig
    {
        [Key]
        public string id { get; set; }
    }
}
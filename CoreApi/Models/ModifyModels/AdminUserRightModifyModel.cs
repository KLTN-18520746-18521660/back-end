using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccess.Common.Interface;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CoreApi.Models.ModifyModels
{
    public class AdminUserRightModifyModel
    {
        public string display_name { get; set; }
        public string describe { get; set; }
    }
}

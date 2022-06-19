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
using CoreApi.Common.Interface;
using DatabaseAccess.Common.Models;
using CoreApi.Common;
using DatabaseAccess.Context.Models;

namespace CoreApi.Models.ModifyModels
{
    public class SocialCommentModifyModel
    {
        [DefaultValue("Modify comment")]
        public string content { get; set; }
    }
}

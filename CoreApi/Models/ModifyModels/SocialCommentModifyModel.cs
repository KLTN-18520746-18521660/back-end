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

namespace CoreApi.Models.ModifyModels
{
    public class SocialCommentModifyModel
    {
        [DefaultValue("Modify comment")]
        public string content { get; set; }
    }
}

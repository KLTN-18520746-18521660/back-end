using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using DatabaseAccess.Context;
using DatabaseAccess;
using CoreApi.Common;
using System.Collections.Generic;
using System.Collections;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace CoreApi.Common
{
    public class HeadersFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) {
                operation.Parameters = new List<OpenApiParameter>();
            }
    
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = HEADER_KEYS.API_KEY,
                In = ParameterLocation.Header,
                Required = false
            });
        }
    }
}
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

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
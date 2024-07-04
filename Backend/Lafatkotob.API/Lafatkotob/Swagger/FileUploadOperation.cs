using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lafatkotob.Swagger
{
    public class FileUploadOperation : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUploadMime = "multipart/form-data";
            if (operation.RequestBody != null && operation.RequestBody.Content.ContainsKey(fileUploadMime))
            {
                var formParams = context.ApiDescription.ParameterDescriptions
                    .Where(p => p.Source.Id.Equals("Form", System.StringComparison.InvariantCultureIgnoreCase));

                foreach (var param in formParams)
                {
                    var description = operation.RequestBody.Content[fileUploadMime];
                    if (description.Schema.Properties.ContainsKey(param.Name))
                    {
                        continue;
                    }

                    description.Schema.Properties.Add(param.Name, new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    });
                }
            }
        }
    }
}

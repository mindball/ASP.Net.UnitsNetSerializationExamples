using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ASP.Net.UnitsNetSerializationExamples.Filters;

/// <summary>
/// A schema filter implementation for Swashbuckle that modifies the OpenAPI schema
/// to represent enumeration types as strings in the generated Swagger documentation.
/// </summary>
/// <remarks>
/// This filter clears the default enumeration values in the schema and replaces them
/// with string representations of the enumeration names. It also sets the schema type
/// to "string" and clears the format.
/// </remarks>
public class QuantitiesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum) return;

        schema.Enum.Clear();
        foreach (var name in Enum.GetNames(context.Type))
        {
            schema.Enum.Add(new OpenApiString(name));
        }
        schema.Type = "string";
        schema.Format = string.Empty;
    }
}
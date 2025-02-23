using System.Reflection;
using ASP.Net.UnitsNetSerializationExamples;
using ASP.Net.UnitsNetSerializationExamples.Filters;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnitsNet;
using UnitsNet.Serialization.JsonNet;
using JsonConverter = Newtonsoft.Json.JsonConverter;

var builder = WebApplication.CreateBuilder(args);

var globalJsonConverter = InitializeGlobalJsonConverter(builder);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Formatting = Formatting.Indented;
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
    if (globalJsonConverter != null) options.SerializerSettings.Converters.Add(globalJsonConverter);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    
    options.EnableAnnotations();
    
    options.SchemaFilter<QuantitiesSchemaFilter>();
    
    

    if (globalJsonConverter is AbbreviatedUnitsConverter)
    {
        options.SchemaGeneratorOptions.CustomTypeMappings =
            new Dictionary<Type, Func<OpenApiSchema>>()
                .WithUnitsNet(CustomTypeMappingSwaggerExtension.ToOpenApiSchemaWithAbbreviations, true, true)
                .WithAdditionalInfo();
    }
    else if (globalJsonConverter is UnitsNetIQuantityJsonConverter)
    {
        options.SchemaGeneratorOptions.CustomTypeMappings =
            new Dictionary<Type, Func<OpenApiSchema>>()
                .WithUnitsNet(CustomTypeMappingSwaggerExtension.ToOpenApiSchemaWithUnits, true, false)
                .WithAdditionalInfo();
    }

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "UnitsNet serialization examples, with additional custom mapping",
        Description = $"Used converter: {globalJsonConverter?.GetType().Name}"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();

static JsonConverter? InitializeGlobalJsonConverter(WebApplicationBuilder builder)
{
    var converterTypeName = builder.Configuration.GetSection("JsonConverter:Type").Value;
    if (string.IsNullOrEmpty(converterTypeName))
    {
        return null; // Configuration is missing
    }
    var converterType = Type.GetType(converterTypeName);
    if (converterType == null)
    {
        return null; // Type could not be loaded
    }
    return converterType switch
    {
        _ when converterType == typeof(UnitsNetIQuantityJsonConverter) => new UnitsNetIQuantityJsonConverter(),
        _ when converterType == typeof(AbbreviatedUnitsConverter) => new AbbreviatedUnitsConverter(),
        _ => null
    };
}

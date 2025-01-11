using System.Reflection;
using ASP.Net.UnitsNetSerializationExamples;
using ASP.Net.UnitsNetSerializationExamples.Filters;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnitsNet.Serialization.JsonNet;
using JsonConverter = Newtonsoft.Json.JsonConverter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Formatting = Formatting.Indented;
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
    options.SerializerSettings.Converters.Add(CustomJsonConverterSetup(builder));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    
    options.EnableAnnotations();
    
    options.SchemaFilter<QuantitiesSchemaFilter>();
   
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "UnitsNet serialization examples, with additional custom mapping",
    });
    
    options.SchemaGeneratorOptions.CustomTypeMappings = new Dictionary<Type, Func<OpenApiSchema>>().WithUnitsNet().WithAdditionalInfo();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();

static JsonConverter CustomJsonConverterSetup(WebApplicationBuilder builder)
{
    var converterTypeName = builder.Configuration.GetSection("JsonConverter:Type").Value;
    if (string.IsNullOrEmpty(converterTypeName))
    {
        throw new InvalidOperationException("JsonConverter type is not configured.");
    }

    var converterType = Type.GetType(converterTypeName)
                        ?? throw new InvalidOperationException($"Cannot load type for converter: {converterTypeName}");

    return converterType switch
    {
        not null when converterType == typeof(UnitsNetIQuantityJsonConverter) => new UnitsNetIQuantityJsonConverter(),
        not null when converterType == typeof(AbbreviatedUnitsConverter) => new AbbreviatedUnitsConverter(),
        _ => throw new InvalidOperationException($"Unsupported converter type: {converterTypeName}")
    };
}
using ASP.Net.UnitsNetSerializationExamples.Filters;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using System.Reflection;
using Newtonsoft.Json;
using UnitsNet.Serialization.JsonNet;
using UnitsNet;

namespace ASP.Net.UnitsNetSerializationExamples.Extensions;

/// <summary>
/// Provides extension methods for configuring services in an ASP.NET Core application.
/// </summary>
/// <remarks>
/// This class includes methods for adding custom JSON serialization settings and configuring Swagger
/// with custom schema mappings for UnitsNet serialization.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures the application to use custom JSON serialization settings with Newtonsoft.Json.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the custom JSON settings will be added.</param>
    /// <param name="jsonConverter">The custom <see cref="JsonConverter"/> to be added to the JSON serializer settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the custom JSON settings applied.</returns>
    public static IServiceCollection AddCustomJson(this IServiceCollection services, JsonConverter jsonConverter)
    {
        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.Formatting = Formatting.Indented;
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
            options.SerializerSettings.Converters.Add(jsonConverter);
        });
        return services;
    }

    /// <summary>
    /// Configures the application to use Swagger with custom schema mappings and additional settings
    /// for UnitsNet serialization.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which Swagger services will be added.</param>
    /// <param name="jsonConverter">
    /// The custom <see cref="JsonConverter"/> used to determine the schema mappings for UnitsNet types.
    /// Supported converters include <see cref="AbbreviatedUnitsConverter"/> and <see cref="UnitsNetIQuantityJsonConverter"/>.
    /// </param>
    /// <param name="schema"></param>
    /// <returns>The updated <see cref="IServiceCollection"/> with Swagger services configured.</returns>
    /// <remarks>
    /// This method configures Swagger to include XML comments, enable annotations, and apply custom schema filters.
    /// Depending on the provided <paramref name="jsonConverter"/>, it applies specific schema mappings for UnitsNet types:
    /// - <see cref="AbbreviatedUnitsConverter"/>: Maps UnitsNet types with abbreviations.
    /// - <see cref="UnitsNetIQuantityJsonConverter"/>: Maps UnitsNet types with unit information.
    /// </remarks>
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services, SerializationSchema schema)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.EnableAnnotations();
            
            options.SchemaFilter<EnumStringSchemaFilter>();
            
            options.MapType<QuantityInfo>(() => new OpenApiSchema() { Description = "Should not be mapped" });
            options.MapType<UnitInfo>(() => new OpenApiSchema() { Description = "Should not be mapped" });
            // options.MapType<UnitKey>(() => new OpenApiSchema() { Description = "Should not be mapped" });
            options.MapType<BaseDimensions>(() => new OpenApiSchema() { Description = "Should not be mapped" });

            if (schema is SerializationSchema.Abbreviated)
            {
                // options.SchemaFilter<AbbreviatedQuantitySchemaFilter>();
                // options.SchemaFilter<AbbreviatedInterfaceQuantitySchemaFilter>();
                // options.MapType<Density>(() => new OpenApiSchema(){Description = "Test", });
                // options.MapType<IQuantity>(() => new OpenApiSchema(){Description = "Test IQuantity", });
                options.SchemaGeneratorOptions.CustomTypeMappings =
                    new Dictionary<Type, Func<OpenApiSchema>>()
                        .AddIQuantityWithAbbreviationMapping(CustomTypeMappingSwaggerExtension.ToOpenApiSchemaWithAbbreviations)
                        .WithAdditionalResourcesInfo();
            }
            else if (schema is SerializationSchema.UnitTypeAndName)
            {
                options.SchemaGeneratorOptions.CustomTypeMappings =
                    new Dictionary<Type, Func<OpenApiSchema>>()
                        .AddIQuantityMapping(CustomTypeMappingSwaggerExtension.ToOpenApiSchemaWithUnits)
                        .WithAdditionalResourcesInfo();
            }

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "UnitsNet serialization examples, with additional custom mapping",
                Description = $"Using converter schema: {schema}"
            });
        });
        return services;
    }

    /// <summary>
    ///     Configures the application to use custom JSON serialization settings with Newtonsoft.Json.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to which the custom JSON settings will be added.</param>
    /// <param name="schema">
    ///     The <see cref="SerializationSchema" /> that determines the JSON serialization strategy to be used:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="SerializationSchema.Abbreviated" />: Uses the
    ///                 <see cref="UnitsNet.Serialization.JsonNet.AbbreviatedUnitsConverter" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="SerializationSchema.UnitTypeAndName" />: Uses the
    ///                 <see cref="UnitsNet.Serialization.JsonNet.UnitsNetIQuantityJsonConverter" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="SerializationSchema.Default" />: Throws a <see cref="NotSupportedException" /> as
    ///                 no default implementation is available.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection" /> with the custom JSON settings applied.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided <paramref name="schema" /> is not recognized.</exception>
    /// <exception cref="NotSupportedException">
    ///     Thrown if <paramref name="schema" /> is set to
    ///     <see cref="SerializationSchema.Default" />.
    /// </exception>
    public static IServiceCollection AddControllersWithNewtonsoftConverter(this IServiceCollection services, SerializationSchema schema)
    {
        JsonConverter jsonConverter = schema switch
        {
            SerializationSchema.Abbreviated => new AbbreviatedUnitsConverter(),
            SerializationSchema.UnitTypeAndName => new UnitsNetIQuantityJsonConverter(),
            SerializationSchema.Default => throw new NotSupportedException("No default implementation available with Newtonsoft"),
            _ => throw new ArgumentOutOfRangeException()
        };
        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.Formatting = Formatting.Indented;
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
            options.SerializerSettings.Converters.Add(jsonConverter);
        });
        return services;
    }
}
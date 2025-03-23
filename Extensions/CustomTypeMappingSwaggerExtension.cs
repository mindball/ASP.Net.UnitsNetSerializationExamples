using System.Globalization;
using System.Xml;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using UnitsNet;
using UnitsNet.Units;


namespace ASP.Net.UnitsNetSerializationExamples.Extensions;

/// <summary>
/// Provides extension methods for customizing Swagger/OpenAPI schema generation
/// with mappings for <see cref="IQuantity"/> and its derived types.
/// </summary>
/// <remarks>
/// This class includes methods to add mappings for UnitsNet quantities to Swagger/OpenAPI schemas,
/// enabling enhanced documentation and support for UnitsNet types in API specifications.
/// It also provides functionality to include additional resources and metadata, such as external documentation links.
/// </remarks>
public static class CustomTypeMappingSwaggerExtension
{
    private const string DocumentationHost = "https://github.com/angularsen/UnitsNet";

    /// <summary>
    /// Extends the provided mappings with additional information.
    /// </summary>
    /// <param name="mappings">The mappings to be extended.</param>
    /// <returns>A new dictionary of mappings with additional information.</returns>
    /// <remarks>
    /// This method is used to extend the provided mappings with additional information. 
    /// It adds an external documentation link to each mapping, pointing to the UnitsNet Documentation portal.
    /// </remarks>
    public static Dictionary<Type, Func<OpenApiSchema>> WithAdditionalResourcesInfo(
        this Dictionary<Type, Func<OpenApiSchema>> mappings)
    {
        // TODO
        var customMappings = new Dictionary<Type, Func<OpenApiSchema>>();
        foreach (var mapping in mappings)
        {
            var type = mapping.Key;
            var defaultMappingFunction = mapping.Value;
            customMappings[type] = () =>
            {
                var defaultSchema = defaultMappingFunction();
                defaultSchema.ExternalDocs = new OpenApiExternalDocs()
                {
                    Description = "UnitsNet Documentation portal",
                    Url = new Uri($"{DocumentationHost}")
                };
                return defaultSchema;
            };
        }

        return customMappings;
    }

    private static string ExtractXmlSummary(object instance, string xmlPath = null)
    {
        var type = instance.GetType();
        var xmlFile = $"{type.Assembly.GetName().Name}.xml";
        xmlPath = xmlPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            var memberNodes = xmlDoc.SelectNodes($"/doc/members/member[starts-with(@name, 'T:{type.FullName}')]");
            if (memberNodes != null)
                foreach (XmlNode memberNode in memberNodes)
                {
                    var summaryNode = memberNode.SelectSingleNode("summary");
                    if (summaryNode != null)
                    {
                        return summaryNode.InnerText.Trim();
                    }
                }
        }

        return null;
    }


    /// <summary>
    /// Adds a mapping for <see cref="IQuantity"/> and its derived types to the provided dictionary.
    /// </summary>
    /// <typeparam name="TDictionary">The type of the dictionary to which the mappings will be added.</typeparam>
    /// <param name="mappings">The dictionary to which the mappings will be added.</param>
    /// <param name="toOpenApiSchemaMethod">
    /// A method that generates an <see cref="OpenApiSchema"/> for a given 
    /// <see cref="QuantityInfo"/>, type, and optional XML documentation path.
    /// </param>
    /// <param name="includingIQuantity"></param>
    /// <param name="xmlPath">
    /// An optional path to the XML documentation file that provides additional metadata for the schema.
    /// </param>
    /// <returns>The dictionary with the added mappings for <see cref="IQuantity"/> and its derived types.</returns>
    /// <remarks>
    /// This method iterates through all quantities defined in the UnitsNet library and adds mappings for each quantity type.
    /// Additionally, it adds a generic mapping for <see cref="IQuantity"/> with a predefined schema.
    /// </remarks>
    public static TDictionary AddIQuantityMapping<TDictionary>(
        this TDictionary mappings,
        Func<QuantityInfo, Type, string, Func<OpenApiSchema>> toOpenApiSchemaMethod,
        bool includingIQuantity = true,
        string? xmlPath = null)
        where TDictionary : IDictionary<Type, Func<OpenApiSchema>>
    {
        var assembly = typeof(Quantity).Assembly;
        foreach (var quantityInfo in Quantity.Infos)
        {
            var quantityStruct = assembly.GetType("UnitsNet." + quantityInfo.Name);
            var openApiSchema = toOpenApiSchemaMethod(quantityInfo, quantityStruct, xmlPath);
            mappings[quantityStruct!] = openApiSchema;
        }

        if (!includingIQuantity)
            return mappings;

        mappings[typeof(IQuantity)] = () =>
        {
            var example = Density.FromGramsPerLiter(1);
            var unitTypeName = example.Unit.GetType().Name;
            var unitExample = $"{unitTypeName}.{DensityUnit.GramPerLiter}";
            return new OpenApiSchema()
            {
                Description = $"A generic quantity such as [{unitTypeName}]",
                ExternalDocs = new OpenApiExternalDocs()
                {
                    Url = new Uri("https://github.com/angularsen/UnitsNet/blob/master/UnitsNet/IQuantity.cs"),
                    Description = "github"
                },
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "unit", new OpenApiSchema
                        {
                            Type = "enum",
                            Example = new OpenApiString(unitExample)
                        }
                    },
                    {
                        "value", new OpenApiSchema
                        {
                            Type = "number",
                            Example = new OpenApiDouble(example.Value)
                        }
                    }
                }
            };
        };
        return mappings;
    }


    /// <summary>
    /// Adds mappings for <see cref="IQuantity"/> types, including abbreviations, to the provided dictionary.
    /// </summary>
    /// <typeparam name="TDictionary">The type of the dictionary to which the mappings will be added.</typeparam>
    /// <param name="mappings">The dictionary to which the mappings will be added.</param>
    /// <param name="toOpenApiSchemaMethod">
    /// A method that generates an <see cref="OpenApiSchema"/> for a given <see cref="QuantityInfo"/>, type, and optional XML documentation path.
    /// </param>
    /// <param name="includingIQuantity"></param>
    /// <param name="xmlPath">The optional path to the XML documentation file.</param>
    /// <returns>The updated dictionary with the added mappings.</returns>
    /// <remarks>
    /// This method extends the provided dictionary with mappings for <see cref="IQuantity"/> types, including their abbreviations.
    /// It also includes a generic mapping for <see cref="IQuantity"/> with an example schema and external documentation link.
    /// </remarks>
    public static TDictionary AddIQuantityWithAbbreviationMapping<TDictionary>(
        this TDictionary mappings,
        Func<QuantityInfo, Type, string, Func<OpenApiSchema>> toOpenApiSchemaMethod,
        bool includingIQuantity = true,
        string? xmlPath = null)
        where TDictionary : IDictionary<Type, Func<OpenApiSchema>>
    {
        var assembly = typeof(Quantity).Assembly;
        foreach (var quantityInfo in Quantity.Infos)
        {
            var quantityStruct = assembly.GetType("UnitsNet." + quantityInfo.Name);
            var openApiSchema = toOpenApiSchemaMethod(quantityInfo, quantityStruct, xmlPath);
            mappings[quantityStruct!] = openApiSchema;
        }

        if (!includingIQuantity)
            return mappings;

        mappings[typeof(IQuantity)] = () =>
        {
            var example = Mass.FromKilograms(1);
            var exampleQuantityInfo = example.QuantityInfo;
            return new OpenApiSchema()
            {
                Description = $"A generic quantity such as [{exampleQuantityInfo.Name}]",
                ExternalDocs = new OpenApiExternalDocs()
                {
                    Url = new Uri("https://github.com/angularsen/UnitsNet/blob/master/UnitsNet/IQuantity.cs"),
                    Description = "github"
                },
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "Value", new OpenApiSchema
                        {
                            Type = "number",
                            Example = new OpenApiDouble(example.Value)
                        }
                    },
                    {
                        "Unit", new OpenApiSchema
                        {
                            Type = "enum",
                            Example = new OpenApiString(example.ToString("a", CultureInfo.InvariantCulture)),
                        }
                    },
                    {
                        "Type", new OpenApiSchema
                        {
                            Type = "string",
                            Default = new OpenApiString(exampleQuantityInfo.Name)
                        }
                    }
                }
            };
        };
        return mappings;
    }

    /// <summary>
    /// Converts a <see cref="QuantityInfo"/> instance into an OpenAPI schema representation, 
    /// including unit abbreviations and additional metadata.
    /// </summary>
    /// <param name="quantityInfo">The <see cref="QuantityInfo"/> instance to be converted.</param>
    /// <param name="unitsNetType">The type associated with the UnitsNet quantity.</param>
    /// <param name="xmlPath">
    /// Optional path to the XML documentation file. If provided, it is used to extract 
    /// the XML summary for the quantity.
    /// </param>
    /// <returns>
    /// A function that generates an <see cref="OpenApiSchema"/> object representing the 
    /// quantity, including its value, unit, and type.
    /// </returns>
    /// <remarks>
    /// This method generates an OpenAPI schema for a UnitsNet quantity, including its value, 
    /// unit (with all possible abbreviations), and type. It also includes a link to the 
    /// UnitsNet documentation and attempts to extract a description from the XML documentation 
    /// if available.
    /// </remarks>
    public static Func<OpenApiSchema> ToOpenApiSchemaWithAbbreviations(this QuantityInfo quantityInfo, Type unitsNetType, string xmlPath = null)
    {
        var example = Quantity.FromQuantityInfo(quantityInfo, 1);
        var unitAbbreviationsCache = UnitsNetSetup.Default.UnitAbbreviations;
        var abbreviations = unitAbbreviationsCache.GetAllUnitAbbreviationsForQuantity(quantityInfo.UnitType, CultureInfo.InvariantCulture);
        List<IOpenApiAny> enumValues = abbreviations.Select(a => (IOpenApiAny)new OpenApiString(a)).ToList();

        return () => new OpenApiSchema
        {
            Type = "object",
            Description = ExtractXmlSummary(example, xmlPath) ?? null,
            ExternalDocs = new OpenApiExternalDocs
            {
                Description = "UnitsNet documentation",
                Url = new Uri("https://github.com/angularsen/UnitsNet")
            },
            Example = new OpenApiObject
            {
                ["Value"] = new OpenApiDouble(example.Value),
                ["Unit"] = new OpenApiString(example.ToString("a", CultureInfo.InvariantCulture)),
                ["Type"] = new OpenApiString(quantityInfo.Name)
            },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                {"Value", new OpenApiSchema {Type = "number", Example = new OpenApiDouble(example.Value)}},
                {
                    "Unit",
                    new OpenApiSchema
                    {
                        Type = "string",
                        Format = "enum",
                        Enum = enumValues,
                        Example = new OpenApiString(example.ToString())
                    }
                },
                {"Type", new OpenApiSchema {Type = "string", Default = new OpenApiString(quantityInfo.Name)}}
            }
        };
    }

    /// <summary>
    /// Converts a <see cref="QuantityInfo"/> instance into an OpenAPI schema representation with units.
    /// </summary>
    /// <param name="quantityInfo">The <see cref="QuantityInfo"/> instance to be converted.</param>
    /// <param name="unitsNetType">The type of the UnitsNet quantity.</param>
    /// <param name="xmlPath">
    /// The optional path to the XML documentation file. If provided, it is used to extract additional
    /// documentation details for the schema.
    /// </param>
    /// <returns>
    /// A function that generates an <see cref="OpenApiSchema"/> representing the quantity with its value and unit.
    /// </returns>
    /// <remarks>
    /// This method creates an OpenAPI schema for a UnitsNet quantity, including its value and unit.
    /// It also adds a link to the UnitsNet documentation as external documentation.
    /// </remarks>
    public static Func<OpenApiSchema> ToOpenApiSchemaWithUnits(this QuantityInfo quantityInfo, Type unitsNetType, string xmlPath = null)
    {
        var example = Quantity.FromQuantityInfo(quantityInfo, 1);
        var unitTypeName = quantityInfo.UnitType.Name;
        var unitExample = $"{unitTypeName}.{DensityUnit.GramPerLiter}";
        return () => new OpenApiSchema
        {
            Type = "object",
            Description = ExtractXmlSummary(example, xmlPath) ?? null,
            ExternalDocs = new OpenApiExternalDocs
            {
                Description = "UnitsNet documentation",
                Url = new Uri("https://github.com/angularsen/UnitsNet")
            },
            Example = new OpenApiObject
            {
                ["unit"] = new OpenApiString(unitExample),
                ["value"] = new OpenApiDouble(example.Value),
            },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                {"unit", new OpenApiSchema {Type = "string", Default = new OpenApiString(unitExample)}},
                {"value", new OpenApiSchema {Type = "number", Example = new OpenApiDouble(example.Value)}}
            }
        };
    }
}
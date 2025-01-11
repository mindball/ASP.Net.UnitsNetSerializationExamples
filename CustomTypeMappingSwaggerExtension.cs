using System.Globalization;
using System.Xml;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using UnitsNet;


namespace ASP.Net.UnitsNetSerializationExamples;

public static class CustomTypeMappingSwaggerExtension
{
    // TODO move to the swagger extensions
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
    public static Dictionary<Type, Func<OpenApiSchema>> WithAdditionalInfo(
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
    /// Extends the provided mappings with UnitsNet quantities.
    /// </summary>
    /// <typeparam name="TDictionary">The type of the dictionary.</typeparam>
    /// <param name="mappings">The mappings to be extended.</param>
    /// <param name="includingIQuantity">If set to <c>true</c>, includes IQuantity in the mappings.</param>
    /// <param name="xmlPath">The XML path for documentation.</param>
    /// <returns>The extended mappings.</returns>
    /// <remarks>
    /// This method is used to extend the provided mappings with UnitsNet quantities. 
    /// If the 'includingIQuantity' parameter is set to true, it also includes IQuantity in the mappings.
    /// The 'xmlPath' parameter can be used to specify the XML path for documentation.
    /// </remarks>
    public static TDictionary WithUnitsNet<TDictionary>(this TDictionary mappings, bool includingIQuantity = false, string xmlPath = null)
        where TDictionary : IDictionary<Type, Func<OpenApiSchema>>
    {
        var assembly = typeof(Quantity).Assembly;
        foreach (var quantityInfo in Quantity.Infos)
        {
            var quantityStruct = assembly.GetType("UnitsNet." + quantityInfo.Name);
            var openApiSchema = quantityInfo.ToOpenApiSchema(quantityStruct, xmlPath);
            mappings[quantityStruct!] = openApiSchema;
        }

        if (!includingIQuantity)
            return mappings;
        // var custom = $"A generic quantity such as [{exampleQuantityInfo.Name}]({DocumentationHost}/api/units/UnitsNet.IQuantity.html)",

        //TODO ask them  if it's worth adding when pulling this feature
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
    
    private static Func<OpenApiSchema> ToOpenApiSchema(this QuantityInfo quantityInfo, Type unitsNetType, string xmlPath = null)
    {
        var example = Quantity.FromQuantityInfo(quantityInfo, 1);
        var unitAbbreviationsCache = UnitsNetSetup.Default.UnitAbbreviations;
        var abbreviations =
            unitAbbreviationsCache.GetAllUnitAbbreviationsForQuantity(quantityInfo.UnitType,
                CultureInfo.InvariantCulture);
        List<IOpenApiAny> enumValues = abbreviations.Select(a => (IOpenApiAny) new OpenApiString(a)).ToList();

        return () => new OpenApiSchema
        {
            Type = "object",
            Description = ExtractXmlSummary(example, xmlPath) ?? null, //TODO provide default Description, 

            ExternalDocs = new OpenApiExternalDocs
            {
                Description = "UnitsNet documentation",
                Url = new Uri("https://github.com/angularsen/UnitsNet")
            },
            Example = new OpenApiObject
            {
                ["Value"] = new OpenApiDouble((double) example.Value),
                ["Unit"] = new OpenApiString(example.ToString("a", CultureInfo.InvariantCulture)),
                ["Type"] = new OpenApiString(quantityInfo.Name)
            },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                {"Value", new OpenApiSchema {Type = "number", Example = new OpenApiDouble((double) example.Value)}},
                {
                    "Unit",
                    new OpenApiSchema
                    {
                        Type = "string",
                        Format = "enum",
                        Enum = enumValues,
                        Example = new OpenApiString(example.ToString("a", CultureInfo.InvariantCulture))
                    }
                },
                {"Type", new OpenApiSchema {Type = "string", Default = new OpenApiString(quantityInfo.Name)}}
            }
        };
    }
}
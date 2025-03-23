# UnitsNet Serialization Examples

This project demonstrates how to use UnitsNet with ASP.NET Core for serializing and deserializing units of measurement using custom converters like `AbbreviatedUnitsConverter` and `UnitsNetIQuantityJsonConverter`.

## Project Overview

The project provides a set of API endpoints to handle operations related to UnitsNet quantities and conversions. It includes functionalities such as retrieving default values, constructing quantities, and performing conversions between different units of measurement.

### Key Features

- **Serialization and Deserialization**: Demonstrates how to serialize and deserialize UnitsNet types using custom JSON converters.
- **API Endpoints**: Provides RESTful API endpoints for various operations on UnitsNet quantities.
- **Units Conversion**: Supports conversion between different units of measurement using UnitsNet.

## Project Structure

- **ASP.Net.UnitsNetSerializationExamples.csproj**: Project file containing package references and project settings.
- **Program.cs**: Configures the ASP.NET Core application, including JSON serialization settings and Swagger configuration.
- **Controllers/UnitsNetConverterController.cs**: API controller providing endpoints for working with UnitsNet types.
- **Extensions/ServiceCollectionExtensions.cs**: Provides extension methods for configuring services in the ASP.NET Core application.

## Configuration

### Program.cs

The `Program.cs` file configures the ASP.NET Core application, including JSON serialization settings and Swagger configuration.

### JSON Configuration Files

#### appsettings.IQuantityJSONConverter.json or appsettings.AbbreviatedUnitsConverter.json

This configuration file specifies the use of the `UnitsNetIQuantityJsonConverter`/`AbbreviatedUnitsConverter` for JSON serialization.

```csharp
var converterType = builder.Configuration.GetSection("JsonConverter:Type").Value; 
var converterTypeName = Type.GetType(converterType)?.FullName; 
JsonConverter globalJsonConverter = converterTypeName == typeof(AbbreviatedUnitsConverter).FullName ? new AbbreviatedUnitsConverter() : new UnitsNetIQuantityJsonConverter();
builder.Services.AddCustomJson(globalJsonConverter); 
builder.Services.AddCustomSwagger(globalJsonConverter);
```

## Extensions/ServiceCollectionExtensions.cs

This file provides extension methods for configuring services in the ASP.NET Core application. It includes methods for adding custom JSON serialization settings and configuring Swagger with custom schema mappings for UnitsNet serialization.

The method configures Swagger to include XML comments, enable annotations, and apply custom schema filters. Depending on the provided `jsonConverter`, it applies specific schema mappings for UnitsNet types:

- `AbbreviatedUnitsConverter`: Maps UnitsNet types with abbreviations.
- `UnitsNetIQuantityJsonConverter`: Maps UnitsNet types with unit information.

 ```csharp
if (jsonConverter is AbbreviatedUnitsConverter)
    {
        options.SchemaGeneratorOptions.CustomTypeMappings =
            new Dictionary<Type, Func<OpenApiSchema>>()
                .AddIQuantityWithAbbreviationMapping(CustomTypeMappingSwaggerExtension.ToOpenApiSchemaWithAbbreviations)
                .WithAdditionalResourcesInfo();
    }
    else if (jsonConverter is UnitsNetIQuantityJsonConverter)
    {
        options.SchemaGeneratorOptions.CustomTypeMappings =
            new Dictionary<Type, Func<OpenApiSchema>>()
                .AddIQuantityMapping(CustomTypeMappingSwaggerExtension.ToOpenApiSchemaWithUnits)
                .WithAdditionalResourcesInfo();
    }
```

## CustomTypeMappingSwaggerExtension.cs

The `CustomTypeMappingSwaggerExtension.cs` file provides extension methods for customizing Swagger/OpenAPI schema generation with mappings for `IQuantity` and its derived types. It enables enhanced documentation and support for UnitsNet types in API specifications.

### WithAdditionalResourcesInfo Method

The `WithAdditionalResourcesInfo` method extends the provided mappings with additional information, such as external documentation links.

### AddIQuantityMapping Method

The `AddIQuantityMapping` method adds a mapping for `IQuantity` and its derived types to the provided dictionary.

#### Parameters

- `mappings`: The dictionary to which the mappings will be added.
- `toOpenApiSchemaMethod`: A method that generates an `OpenApiSchema` for a given `QuantityInfo`, type, and optional XML documentation path.
- `includingIQuantity`: A boolean indicating whether to include a generic mapping for `IQuantity`.
- `xmlPath`: An optional path to the XML documentation file that provides additional metadata for the schema.

#### Returns

- The dictionary with the added mappings for `IQuantity` and its derived types.

#### Implementation

The method iterates through all quantities defined in the UnitsNet library and adds mappings for each quantity type. Additionally, it adds a generic mapping for `IQuantity` with a predefined schema.

```csharp
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
```


### AddIQuantityWithAbbreviationMapping Method

The `AddIQuantityWithAbbreviationMapping` method adds mappings for `IQuantity` types, including abbreviations, to the provided dictionary.

#### Parameters

- `mappings`: The dictionary to which the mappings will be added.
- `toOpenApiSchemaMethod`: A method that generates an `OpenApiSchema` for a given `QuantityInfo`, type, and optional XML documentation path.
- `includingIQuantity`: A boolean indicating whether to include a generic mapping for `IQuantity`.
- `xmlPath`: The optional path to the XML documentation file.

#### Returns

- The updated dictionary with the added mappings.

#### Implementation

The method extends the provided dictionary with mappings for `IQuantity` types, including their abbreviations. It also includes a generic mapping for `IQuantity` with an example schema and external documentation link.

```csharp
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
```


### ToOpenApiSchemaWithAbbreviations Method

The `ToOpenApiSchemaWithAbbreviations` method converts a `QuantityInfo` instance into an OpenAPI schema representation, including unit abbreviations and additional metadata.

#### Parameters

- `quantityInfo`: The `QuantityInfo` instance to be converted.
- `unitsNetType`: The type associated with the UnitsNet quantity.
- `xmlPath`: Optional path to the XML documentation file. If provided, it is used to extract the XML summary for the quantity.

#### Returns

- A function that generates an `OpenApiSchema` object representing the quantity, including its value, unit, and type.

#### Implementation

The method generates an OpenAPI schema for a UnitsNet quantity, including its value, unit (with all possible abbreviations), and type. It also includes a link to the UnitsNet documentation and attempts to extract a description from the XML documentation if available.

```csharp
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
```


### ToOpenApiSchemaWithUnits Method

The `ToOpenApiSchemaWithUnits` method converts a `QuantityInfo` instance into an OpenAPI schema representation with units.

#### Parameters

- `quantityInfo`: The `QuantityInfo` instance to be converted.
- `unitsNetType`: The type of the UnitsNet quantity.
- `xmlPath`: The optional path to the XML documentation file. If provided, it is used to extract additional documentation details for the schema.

#### Returns

- A function that generates an `OpenApiSchema` representing the quantity with its value and unit.

#### Implementation

The method creates an OpenAPI schema for a UnitsNet quantity, including its value and unit. It also adds a link to the UnitsNet documentation as external documentation.



## Conclusion

The `ServiceCollectionExtensions.cs` and `CustomTypeMappingSwaggerExtension.cs` files provide powerful functionality for configuring custom JSON serialization settings and enhancing Swagger/OpenAPI documentation with UnitsNet types. By using these extensions, developers can create comprehensive and well-documented APIs that support UnitsNet quantities and conversions.

# UnitsNetConverterController

The `UnitsNetConverterController` provides API endpoints for handling operations related to UnitsNet quantities and conversions. This controller includes methods for retrieving default values, constructing quantities, and performing conversions between different units of measurement.

## Endpoints

### 1. Retrieve Default Mass Value

#### Endpoint

- **GET** `/mass-unitZero`

### 2. Construct a Quantity by Name

#### Endpoint

- **GET** `/construct-quantity-by-name`

#### Parameters

- `quantityName`: The name of the quantity to construct (e.g., "Volume", "Mass").
- `unitName`: The name of the unit for the quantity (e.g., "Liter", "Kilogram").
- `value`: The numeric value of the quantity.

#### Description

Creates a quantity based on the provided quantity name, unit name, and value. Returns a `BadRequest` result if the quantity name or unit name is invalid.

### 3. Construct a Density Quantity

#### Endpoint

- **GET** `/construct-density-from-unit`

#### Parameters

- `unit`: The unit of density to use for constructing the quantity (e.g., "KilogramPerCubicMeter").
- `value`: The numeric value of the density.

#### Description

Creates a density quantity based on the provided density unit and value. Returns a `BadRequest` result if the density unit is invalid.

### 4. Convert Mass and Volume to Density

#### Endpoint

- **GET** `/create-density-from-massUnit-and-volumeUnit`

#### Parameters

- `massUnit`: The unit of the mass value (e.g., `MassUnit.Kilogram`).
- `massValue`: The numeric value of the mass to be converted.
- `volumeUnit`: The unit of the volume value (e.g., `VolumeUnit.Liter`).
- `volumeValue`: The numeric value of the volume to be converted. Must be greater than zero.

#### Description

Converts mass and volume values to density. Returns a `BadRequest` result if the volume value is zero or negative.

### 5. Convert Quantity Using Unit Abbreviation

#### Endpoint

- **POST** `/convert-to-unit-with-abbreviation`

#### Parameters

- `quantity`: The quantity to be converted, represented as an `IQuantity`.
- `targetUnitAbbreviation`: The abbreviation of the target unit (e.g., "kg" for Kilogram, "g" for Gram).

#### Description

Converts a quantity to a specified unit using the unit abbreviation. Returns a `BadRequest` result if the unit abbreviation is invalid.

### 6. Convert Density Unit

#### Endpoint

- **POST** `/convert-density-unit`

#### Parameters

- `quantity`: The density quantity to be converted. This is provided in the request body.
- `toDensityUnit`: The target density unit to which the quantity will be converted. This is provided as a query parameter.

#### Description

Converts a given density quantity to a specified density unit. Returns the converted density value in the specified unit.

### 6. Convert Density Unit

#### Endpoint

- **POST** `/convert-density-unit`

#### Parameters

- `quantity`: The density quantity to be converted. This is provided in the request body.
- `toDensityUnit`: The target density unit to which the quantity will be converted. This is provided as a query parameter.

#### Description

Converts a given density quantity to a specified density unit. Returns the converted density value in the specified unit.


## Conclusion

The `UnitsNetConverterController` provides a comprehensive set of API endpoints for working with UnitsNet quantities and conversions. It includes methods for retrieving default values, constructing quantities, and performing conversions between different units of measurement. The controller is well-documented with Swagger annotations, making it easy to understand and use the provided endpoints.



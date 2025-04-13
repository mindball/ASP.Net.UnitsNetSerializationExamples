namespace ASP.Net.UnitsNetSerializationExamples.Extensions;

/// <summary>
/// Represents the options for serializing and deserializing UnitsNet quantities.
/// </summary>
/// <remarks>
/// This record encapsulates the serializer type and serialization schema to be used
/// for handling UnitsNet quantities in JSON.
/// </remarks>
public record struct SerializationOptions(SerializerType Serializer, SerializationSchema Schema);

/// <summary>
/// Defines the serialization schema options for UnitsNet quantities.
/// </summary>
/// <remarks>
/// This enumeration is used to specify how UnitsNet quantities should be serialized in JSON.
/// </remarks>
/// <summary>
/// The default serialization schema.
/// </summary>
/// <summary>
/// Serializes UnitsNet quantities using abbreviated unit names (e.g., "m" for meters).
/// </summary>
/// <summary>
/// Serializes UnitsNet quantities by including both the unit type and the unit name (e.g., "Length.Meters").
/// </summary>
public enum SerializationSchema
{
    /// <summary>
    /// Represents the default serialization schema for UnitsNet quantities.
    /// </summary>
    /// <remarks>
    /// This schema uses the default serialization settings, which may vary depending on the implementation.
    /// It is intended to provide a standard and consistent way to serialize UnitsNet quantities.
    /// </remarks>
    Default,
    /// <summary>
    /// Serializes UnitsNet quantities using abbreviated unit names (e.g., "m" for meters).
    /// </summary>
    /// <remarks>
    /// This option is useful for compact representations of UnitsNet quantities in JSON,
    /// where only the abbreviated unit name is included.
    /// </remarks>
    Abbreviated,
    /// <summary>
    /// Serializes UnitsNet quantities by including both the unit type and the unit name (e.g., "LengthUnit.Meter").
    /// </summary>
    /// <remarks>
    /// This serialization schema provides detailed information about the quantity by combining the type of the unit
    /// and its specific name, which can be useful for scenarios requiring precise unit identification.
    /// </remarks>
    UnitTypeAndName
}

/// <summary>
/// Specifies the serializer type to be used for serializing and deserializing data.
/// </summary>
/// <remarks>
/// This enumeration defines the supported serializers for handling JSON serialization.
/// </remarks>
/// <summary>
/// Uses the System.Text.Json serializer for JSON serialization and deserialization.
/// </summary>
/// <summary>
/// Uses the Newtonsoft.Json serializer (also known as Json.NET) for JSON serialization and deserialization.
/// </summary>
public enum SerializerType
{
    /// <summary>
    /// Uses the System.Text.Json serializer for JSON serialization and deserialization.
    /// </summary>
    /// <remarks>
    /// This option leverages the System.Text.Json library, which is a high-performance, 
    /// lightweight, and built-in JSON serializer in .NET.
    /// </remarks>
    SystemTextJson,
    /// <summary>
    /// Uses the Newtonsoft.Json serializer (also known as Json.NET) for JSON serialization and deserialization.
    /// </summary>
    /// <remarks>
    /// This option leverages the Newtonsoft.Json library to handle JSON serialization and deserialization, 
    /// providing extensive customization options and compatibility with a wide range of .NET applications.
    /// </remarks>
    NewtonsoftJson
}
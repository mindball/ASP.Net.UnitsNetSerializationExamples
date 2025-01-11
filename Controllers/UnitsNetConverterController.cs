using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;
using UnitsNet;
using UnitsNet.Units;
using System.Runtime.Serialization;
using System.Diagnostics.Metrics;
using System;
using System.Text.RegularExpressions;

namespace ASP.Net.UnitsNetSerializationExamples.Controllers;

/// <summary>
///     Provides API endpoints for handling operations related to UnitsNet quantities and conversions.
/// </summary>
/// <remarks>
///     This controller includes methods for retrieving default values, constructing quantities, 
///     and performing conversions between different units of measurement.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class UnitsNetConverterController : ControllerBase
{
    /// <summary>
    ///     Retrieves the default length value, which is represented as <c>Length.Zero</c>.
    /// </summary>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the default length value (<c>Length.Zero</c>).
    /// </returns>
    [HttpGet("length-unitZero")]
    [SwaggerOperation(
        Summary = "Retrieves the length.zero value.",
        Description = "Returns the default length value, represented as Length.Zero."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLength()
    {
        
        return Ok(Length.Zero);
    }

    
    /// <summary>
    ///     Constructs a quantity based on the provided quantity name, unit name, and value.
    /// </summary>
    /// <param name="quantityName">
    ///     The name of the quantity to construct (e.g., "Length", "Mass").
    /// </param>
    /// <param name="unitName">
    ///     The name of the unit for the quantity (e.g., "Meter", "Kilogram").
    /// </param>
    /// <param name="value">
    ///     The numeric value of the quantity.
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the constructed quantity if the input is valid.
    ///     Returns a <c>BadRequest</c> result if the quantity name or unit name is invalid.
    /// </returns>
    [HttpGet("construct-quantity")]
    [SwaggerOperation(
        Summary = "Constructs a quantity.",
        Description = "Creates a quantity based on the provided quantity name, unit name, and value.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetConstructQuantity(string quantityName, string unitName, double value)
    {
        if (Quantity.TryFrom(value, quantityName, unitName, out var quantity)) return Ok(quantity);
        
        return BadRequest("Invalid quantity or unit name.");
    }

    /// <summary>
    ///     Constructs a quantity based on the provided unit abbreviation and value.
    /// </summary>
    /// <param name="unitName">The abbreviation of the unit (e.g., "cm" for Centimeters).</param>
    /// <param name="value">The numeric value of the quantity (e.g., 3).</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the constructed quantity if the input is valid.
    ///     Returns a <c>BadRequest</c> result if the unit abbreviation is invalid.
    /// </returns>
    [HttpGet("construct-quantity-from abbreviation")]
    [SwaggerOperation(
        Summary = "Constructs a quantity using unit abbreviation.",
        Description = "Creates a quantity based on the provided unit abbreviation and value."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetConstructQuantityFromAbbreviation(string unitName, double value)
    {
        if (Quantity.TryFromUnitAbbreviation(value, unitName, out var quantity)) return Ok(quantity);
        return BadRequest("Invalid quantity or unit name.");
    }


    /// <summary>
    ///     Converts a given constructed length to a specified target length unit.
    /// </summary>
    /// <param name="length">
    ///     The constructed length to be converted, provided as a <see cref="Length" /> object.
    /// </param>
    /// <param name="toLengthUnit">
    ///     The target length unit to which the value should be converted, specified as a <see cref="LengthUnit" />.
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the converted length value if the input is valid.
    ///     Returns a <c>BadRequest</c> result if the length value is negative.
    /// </returns>
    /// <remarks>
    ///     This method performs a unit conversion for a given length. If the source and target units are identical, 
    ///     the method returns a <c>BadRequest</c> response.
    /// </remarks>
    [HttpPost("convert-length-unit-value")]
    public IActionResult ConvertLengthValue([FromBody] Length length, LengthUnit toLengthUnit)
    {
        if (length.Unit == toLengthUnit) return BadRequest("Length units are the same.");
        double convertedLengthValue = length.As(toLengthUnit);
        return Ok(convertedLengthValue);
    }

    /// <summary>
    ///     Converts a given constructed length to a specified target length unit.
    /// </summary>
    /// <param name="length">The length value to be converted, represented as a <see cref="Length" /> object.</param>
    /// <param name="toLengthUnit">The target unit to which the length will be converted, represented as a <see cref="LengthUnit" />.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the converted length in the target unit if the conversion is successful.
    ///     Returns a <c>BadRequest</c> result if the source and target units are the same.
    /// </returns>
    /// <remarks>
    ///     This method performs a unit conversion for a given length. If the source and target units are identical, 
    ///     the method returns a <c>BadRequest</c> response.
    /// </remarks>
    [HttpPost("convert-length-unit")]
    public IActionResult ConvertLengthUnit([FromBody] Length length, LengthUnit toLengthUnit)
    {
        if(length.Unit == toLengthUnit) return BadRequest("Length units are the same.");

        Length convertedLengthUnit = length.ToUnit(toLengthUnit);
        return Ok(convertedLengthUnit);
    }


    /// <summary>
    ///     Converts mass and volume values to a density value.
    /// </summary>
    /// <param name="densityUnits">
    ///     The target density unit for the conversion (e.g., <see cref="DensityUnit.GramPerMilliliter" />).
    /// </param>
    /// <param name="massUnit">
    ///     The unit of the mass value (e.g., <see cref="MassUnit.Kilogram" />).
    /// </param>
    /// <param name="massValue">
    ///     The numeric value of the mass to be converted.
    /// </param>
    /// <param name="volumeUnit">
    ///     The unit of the volume value (e.g., <see cref="VolumeUnit.Liter" />).
    /// </param>
    /// <param name="volumeValue">
    ///     The numeric value of the volume to be converted. Must be greater than zero.
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the converted density value in the specified unit.
    ///     Returns a <c>BadRequest</c> result if the volume value is zero or negative.
    /// </returns>
    [HttpGet("conversion-from-massUnit-volumeUnit-to-density")]
    public IActionResult ConvertFromMassAndVolumeToDensityGramPerMilliliter(DensityUnit densityUnits, MassUnit massUnit, double massValue, VolumeUnit volumeUnit, double volumeValue)
    {
        if (volumeValue == 0) return BadRequest("Volume value must be positive quantity value");
        
        var mass = Mass.From(massValue, massUnit);
        var volume = Volume.From(volumeValue, volumeUnit);
        
        Density density = mass / volume;

        return Ok(density.ToUnit(densityUnits));
    }


    /// <summary>
    ///     Creates a <see cref="Length" /> instance from the provided string representation of its value.
    /// </summary>
    /// <param name="value">
    ///     The string representation of the length value to be converted (e.g., 1km />).
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the created <see cref="Length" /> instance if the conversion is successful.
    ///     Returns a <c>BadRequest</c> result if the conversion fails.
    /// </returns>
    /// <remarks>
    ///     This method utilizes a <see cref="QuantityTypeConverter{T}" /> to parse the string value into a <see cref="Length" /> object.
    /// </remarks>
    [HttpGet("create-lengh")]
    public IActionResult CreateLengthFromStringValue(string value)
    {
        

        var converter = new QuantityTypeConverter<Length>();
        var convertedValue = (Length)converter.ConvertFromString(value)!;

        return Ok(convertedValue);
    }


    // [HttpGet("TestDefaultEnumConfig")]
    // public IActionResult TestEnum(MyEnum toLengthUnit)
    // {
    //     return new JsonResult(toLengthUnit, new JsonSerializerSettings { Converters = new List<JsonConverter> { new StringEnumConverter() } });
    // }
    //
    // [HttpGet("TestDefaultEnumConfig2")]
    // public IActionResult TestEnum2(MyEnum toLengthUnit)
    // {
    //     var jsonResult = JsonConvert.SerializeObject(toLengthUnit,
    //         new JsonSerializerSettings { Converters = new List<JsonConverter> { new StringEnumConverter() } });
    //     return new ContentResult { Content = jsonResult, ContentType = "application/json" };
    // }
    //
    // [JsonConverter(typeof(StringEnumConverter))]
    // public enum MyEnum
    // {
    //     Test1 = 1,
    //     [EnumMember(Value = "Test2")]
    //     Test2 = 2
    // } 
}
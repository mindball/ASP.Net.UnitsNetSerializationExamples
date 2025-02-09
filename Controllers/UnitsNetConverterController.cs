using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Annotations;
using UnitsNet;
using UnitsNet.Units;

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
public class UnitsNetConverterController: ControllerBase
{
    /// <summary>
    ///     Retrieves the default mass value.
    /// </summary>
    /// <remarks>
    ///     This method returns the default mass value, represented as <see cref="Mass.Zero" />.
    /// </remarks>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the default mass value.
    /// </returns>
    [HttpGet("mass-unitZero")]
    [SwaggerOperation(
        Summary = "Retrieves the length.zero value.",
        Description = "Returns the default length value, represented as Length.Zero."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetMass()
        => Ok(Mass.Zero);
    
    /// <summary>
    ///     Constructs a quantity based on the provided quantity name, unit name, and value.
    /// </summary>
    /// <param name="quantityName">
    ///     The name of the quantity to construct (e.g., "Volume", "Mass").
    /// </param>
    /// <param name="unitName">
    ///     The name of the unit for the quantity (e.g., "Liter", "Kilogram").
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
        if (!Quantity.TryFrom(value, quantityName, unitName, out var quantity)) 
            return BadRequest("Invalid quantity or unit name."); ;
        
        return Ok(quantity);
    }

    /// <summary>
    ///     Converts mass and volume values to a density value.
    /// </summary>
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
    public IActionResult ConvertFromMassAndVolumeToDensity(MassUnit massUnit, double massValue, VolumeUnit volumeUnit, double volumeValue)
    {
        if (volumeValue == 0) return BadRequest("Volume value must be positive quantity value");
        
        var mass = Mass.From(massValue, massUnit);
        var volume = Volume.From(volumeValue, volumeUnit);
        
        Density density = mass / volume;

        return Ok(density);
    }

    /// <summary>
    ///     Converts a quantity to a specified unit using its abbreviation.
    /// </summary>
    /// <param name="quantity">
    ///     The quantity to be converted, represented as an <see cref="IQuantity" />.
    /// </param>
    /// <param name="targetUnitAbbreviation">
    ///     The abbreviation of the target unit (e.g., "kg" for Kilogram, "g" for Gram).
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the converted quantity if the input is valid.
    ///     Returns a <c>BadRequest</c> result if the unit abbreviation is invalid.
    /// </returns>
    /// <remarks>
    ///     This method attempts to parse the provided unit abbreviation and convert the given quantity to the specified unit.
    ///     If the abbreviation is invalid or incompatible with the quantity, a <c>BadRequest</c> response is returned.
    /// </remarks>
    [HttpPost("convert-to-unit-with-abbreviation")]
    [SwaggerOperation(
        Summary = "Constructs a quantity using unit abbreviation.",
        Description = "Creates a quantity based on the provided unit abbreviation and value."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ConvertToUnitByAbbreviation([FromBody] IQuantity quantity, string targetUnitAbbreviation)
    {
        if (!UnitParser.Default.TryParse(targetUnitAbbreviation, quantity.Unit.GetType(), out var targetUnit))
            return BadRequest("Invalid quantity or unit abbreviation.");

        //Feature request: if(UnitConverter.TryConvert(quantity, targetUnit, out convertedQuantity))
        return Ok(quantity.ToUnit(targetUnit));
    }

    
    /// <summary>
    ///     Converts a given quantity to the specified density unit.
    /// </summary>
    /// <param name="quantity">
    ///     The quantity to be converted. This must implement the <see cref="IQuantity" /> interface.
    /// </param>
    /// <param name="toDensityUnit">
    ///     The target density unit to which the quantity will be converted. 
    ///     This is specified as a <see cref="DensityUnit" />.
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the converted quantity in the specified density unit.
    ///     Returns a <c>BadRequest</c> result if the conversion fails.
    /// </returns>
    /// <remarks>
    ///     This method allows for the conversion of a quantity to a specific density unit, 
    ///     leveraging the UnitsNet library for unit conversions.
    /// </remarks>
    [HttpPost("convert-quantity-unit")]
    public IActionResult ConvertDensityUnit([FromBody] IQuantity quantity, [FromQuery] DensityUnit toDensityUnit)
    {
        var convertedLengthUnit = quantity.ToUnit(toDensityUnit);
        return Ok(convertedLengthUnit);
    }
}
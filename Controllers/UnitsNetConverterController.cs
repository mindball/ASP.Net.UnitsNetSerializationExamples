using System.Globalization;
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
    public ActionResult<Mass> GetMass()
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
    [HttpGet("construct-quantity-by-name")]
    [SwaggerOperation(
        Summary = "Constructs a quantity.",
        Description = "Creates a quantity based on the provided quantity name, unit name, and value.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IQuantity> GetConstructQuantityByName(string quantityName, string unitName, double value)
    {
        if (!Quantity.TryFrom(value, quantityName, unitName, out var quantity)) 
            return BadRequest("Invalid quantity or unit name."); ;
        
        return Ok(quantity);
    }

    /// <summary>
    ///     Constructs a density quantity based on the provided density unit and value.
    /// </summary>
    /// <param name="unit">
    ///     The unit of density to use for constructing the quantity (e.g., "KilogramPerCubicMeter").
    /// </param>
    /// <param name="value">
    ///     The numeric value of the density.
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the constructed density quantity if the input is valid.
    ///     Returns a <c>BadRequest</c> result if the density unit is invalid.
    /// </returns>
    [HttpGet("construct-density-from-unit")]
    [SwaggerOperation(
        Summary = "Construct a density.",
        Description = "Create a density based on the provided density unit.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Density> GetConstructDensityFromUnit(DensityUnit unit, double value)
    {
        if (!Quantity.TryFrom(value, unit, out var quantity))
            return BadRequest("Invalid quantity unit."); ;
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
    [HttpGet("create-density-from-massUnit-and-volumeUnit")]
    public ActionResult<Density> CreateDensityFromMassAndVolume(MassUnit massUnit, double massValue, VolumeUnit volumeUnit, double volumeValue)
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
    ///     It is important to have an abbreviation for the selected unit of measurement. Keep in mind that when using a custom mapping, the JSON pattern example applies to the unit of measurement,
    ///     such as Density(g/ml, kg/cm³). If you need an abbreviation for Mass, use 'kg', 'mg', etc.; for Volume, use 'l', 'ml', and so on. In other words, the 'Unit' field in
    ///     the JSON body must correspond to the abbreviation.
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the converted quantity if the input is valid.
    ///     Returns a <c>BadRequest</c> result if the unit abbreviation is invalid.
    /// </returns>
    /// <remarks>
    ///     This method attempts to parse the provided unit abbreviation and convert the given quantity to the specified unit.
    ///     If the abbreviation is invalid or incompatible with the quantity, a <c>BadRequest</c> response is returned.
    ///     
    /// </remarks>
    [HttpPost("convert-to-unit-with-abbreviation")]
    [SwaggerOperation(
        Summary = "Constructs a quantity using unit abbreviation.",
        Description = "Creates a quantity based on the provided unit abbreviation and value."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IQuantity> ConvertToUnitByAbbreviation([FromBody] IQuantity quantity, string targetUnitAbbreviation)
    {
        if (!UnitParser.Default.TryParse(targetUnitAbbreviation, quantity.Unit.GetType(), out var targetUnit))
            return BadRequest("Invalid quantity or unit abbreviation.");

        return Ok(quantity.ToUnit(targetUnit));
    }
    
    /// <summary>
    ///     Converts a given density quantity to a specified density unit.
    /// </summary>
    /// <param name="quantity">
    ///     The density quantity to be converted. This is provided in the request body.
    /// </param>
    /// <param name="toDensityUnit">
    ///     The target density unit to which the quantity will be converted. This is provided as a query parameter.
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the converted density value in the specified unit.
    /// </returns>
    /// <remarks>
    ///     This method takes a <see cref="Density" /> object and a target <see cref="DensityUnit" /> as input, 
    ///     performs the conversion, and returns the result.
    /// </remarks>
    /// <response code="200">
    ///     Returns the converted density value in the specified unit.
    /// </response>
    [HttpPost("convert-density-unit")]
    // public IActionResult ConvertDensityUnitV1([FromBody] IQuantity quantity, [FromQuery] DensityUnit toDensityUnit)  worked
    public ActionResult<Density> ConvertDensityUnit([FromBody] Density quantity, [FromQuery] DensityUnit toDensityUnit)
    {
        var convertedDensityUnit = quantity.ToUnit(toDensityUnit);
        return Ok(convertedDensityUnit);
    }

    /// <summary>
    ///     Constructs a quantity using the specified unit abbreviation and value.
    /// </summary>
    /// <param name="unitAbbreviation">
    ///     The abbreviation of the unit to be used for constructing the quantity (e.g., "kg", "mg", 'cm', or 'mg/l').
    /// </param>
    /// <param name="value">
    ///     The numerical value to be associated with the constructed quantity.
    /// </param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the constructed quantity if successful, 
    ///     or a <see cref="BadRequestResult" /> if the unit abbreviation or value is invalid.
    /// </returns>
    /// <remarks>
    ///     This method attempts to create a quantity by interpreting the provided unit abbreviation.
    ///     If the abbreviation or value is invalid, an error response is returned.
    /// </remarks>
    /// <response code="200">
    ///     Returns the constructed quantity.
    /// </response>
    /// <response code="400">
    ///     Returns an error message if the unit abbreviation or value is invalid.
    /// </response>
    [HttpGet("construct-quantity-by-abbreviation")]
    [Obsolete("This route is deprecated and will be removed in future versions. Please use the new route for constructing quantities.")]
    [SwaggerOperation(
        Summary = "Constructs a quantity by abbreviation (Deprecated).",
        Description = "This route is deprecated and will be removed in future versions. Creates a quantity based on the provided unit abbreviation name and value.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IQuantity> GetConstructQuantityByAbbreviation(string unitAbbreviation, double value)
    {
        IQuantity quantity;
        try
        {
            quantity = Quantity.FromUnitAbbreviation(value, unitAbbreviation);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        return Ok(quantity);
    }
}
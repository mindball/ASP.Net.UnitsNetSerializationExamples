using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UnitsNet;
using UnitsNet.Units;


namespace UnitsNetSerializationExamples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UnitsNetConverterController : ControllerBase
    {

        /// <summary>
        /// Retrieves the default length value, which is represented as <c>Length.Zero</c>.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the default length value (<c>Length.Zero</c>).
        /// </returns>
        [HttpGet("GetLengthUnitZero")]
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
        /// Constructs a length quantity based on the provided quantity name, unit name, and value.
        /// </summary>
        /// <param name="quantityName">The name of the quantity (e.g., "Length").</param>
        /// <param name="unitName">The name of the unit (e.g., "Centimeter").</param>
        /// <param name="value">The numeric value of the quantity (e.g., 3).</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the constructed quantity if the input is valid.
        /// Returns a <c>BadRequest</c> result if the quantity name or unit name is invalid.
        /// </returns>
        /// <remarks>UnitName does not work in the plural?!?!</remarks>
        [HttpGet("ConstructLengthQuantity")]
        [SwaggerOperation(
            Summary = "Constructs a length quantity.",
            Description = "Creates a length quantity based on the provided quantity name, unit name, and value.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetConstructLengthQuantity(string quantityName, string unitName, double value)
        {
            if (Quantity.TryFrom(value, quantityName, unitName, out IQuantity? quantity))
            {
                return Ok(quantity);
            }
            return BadRequest("Invalid quantity or unit name.");
        }

        /// <summary>
        /// Constructs a length quantity based on the provided unit abbreviation and value.
        /// </summary>
        /// <param name="unitName">The abbreviation of the unit (e.g., "cm" for Centimeters).</param>
        /// <param name="value">The numeric value of the quantity (e.g., 3).</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the constructed quantity if the input is valid.
        /// Returns a <c>BadRequest</c> result if the unit abbreviation is invalid.
        /// </returns>
        [HttpGet("ConstructLengthQuantityFromAbbreviation")]
        [SwaggerOperation(
            Summary = "Constructs a length quantity using unit abbreviation.",
            Description = "Creates a length quantity based on the provided unit abbreviation and value."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetConstructLengthQuantityFromAbbreviation(string unitName, double value)
        {
            if (Quantity.TryFromUnitAbbreviation(value, unitName, out IQuantity? quantity))
            {
                return Ok(quantity);
            }
            return BadRequest("Invalid quantity or unit name.");
        }

        /// <summary>
        /// Converts a length value from one unit to another.
        /// </summary>
        /// <param name="fromValue">The value to be converted .</param>
        /// <param name="fromLengthUnit">The source unit of the length value (e.g., from Meters)</param>
        /// <param name="toLengthUnit">The target unit to which the length value will be converted (e.g., to Centimeters)</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the converted length value if the conversion is successful (e.g., 1 meters = 100 centimeters)
        /// Returns a <c>BadRequest</c> result if the source and target units are the same.
        /// </returns>
        [HttpGet("ConvertLengthCalculator")]
        public IActionResult ConvertLengthCalculator(decimal fromValue,
             LengthUnit fromLengthUnit, LengthUnit toLengthUnit)
        {
            if (fromLengthUnit == toLengthUnit) return BadRequest("Choose different length units of measurement");

            QuantityValue quantityValue = fromValue;

            var result = UnitConverter.Convert(fromValue,
                fromLengthUnit,
                toLengthUnit);
            return Ok(result);
        }



        /// <summary>
        /// Calculates the density based on the provided mass and volume values.
        /// </summary>
        /// <param name="request">
        /// A dynamic object containing the following properties:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <c>mass.value</c>: The numeric value of the mass.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>mass.unit</c>: The unit of the mass as a string (e.g., "Kilogram").
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>volume.value</c>: The numeric value of the volume.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>volume.unit</c>: The unit of the volume as a string (e.g., "CubicMeter").
        /// </description>
        /// </item>
        /// </list>
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the calculated density if the input is valid.
        /// Returns a <c>BadRequest</c> result if the input data is invalid or if the volume is less than or equal to zero.
        /// </returns>
        [HttpPost("calculate-density")]
        public IActionResult CalculateDensity([FromBody] dynamic request)
        {
            try
            {
                double massValue = request.mass.value;
                string massUnit = request.mass.unit;
                double volumeValue = request.volume.value;
                string volumeUnit = request.volume.unit;

                if (volumeValue <= 0)
                {
                    return BadRequest("Volume must be greater than zero.");
                }

                var mass = Mass.From(massValue, Enum.Parse<MassUnit>(massUnit));
                var volume = Volume.From(volumeValue, Enum.Parse<VolumeUnit>(volumeUnit));

                var density = Density.FromKilogramsPerCubicMeter(mass.Kilograms / volume.CubicMeters);

                return Ok(density);
            }
            catch (Exception)
            {
                return BadRequest("Invalid input data.");
            }
        }
    }
}

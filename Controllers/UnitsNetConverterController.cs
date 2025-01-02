using Microsoft.AspNetCore.Mvc;
// using System.Text.Json;
using UnitsNet;
using UnitsNet.Units;


namespace UnitsNetSerializationExamples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UnitsNetConverterController : ControllerBase
    {
        /// <summary>
        /// Gets the default mass value (Mass.Zero).
        /// </summary>
        [HttpGet("default-mass")]
        public IActionResult GetDefaultMass()
        {
            return Ok(Mass.Zero);
        }

        /// <summary>
        /// Processes a given mass object and returns it.
        /// </summary>
        /// <param name="mass">The mass object to process.</param>
        [HttpPost("process-mass")]
        public IActionResult ProcessMass([FromBody] Mass mass)
        {
            return Ok(mass);
        }

        /// <summary>
        /// Gets the default volume value (Volume.Zero).
        /// </summary>
        [HttpGet("default-volume")]
        public IActionResult GetDefaultVolume()
        {
            return Ok(Volume.Zero);
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

using Microsoft.AspNetCore.Mvc;
using fujitsuDeliveryFee.Domain.Interfaces;
using System.Threading.Tasks;

namespace fujitsuDeliveryFee.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryFeeController : ControllerBase
    {
        private readonly IDeliveryFeeCalculationService _deliveryFeeCalculationService;
        private readonly ILogger<DeliveryFeeController> _logger;

        public DeliveryFeeController(
            IDeliveryFeeCalculationService deliveryFeeCalculationService,
            ILogger<DeliveryFeeController> logger)
        {
            _deliveryFeeCalculationService = deliveryFeeCalculationService;
            _logger = logger;
        }

        /// <summary>
        /// Calculates the delivery fee based on city and vehicle type
        /// </summary>
        /// <param name="city">The city for delivery (Tallinn, Tartu, Pärnu)</param>
        /// <param name="vehicleType">The vehicle type (Car, Scooter, Bike)</param>
        /// <returns>The calculated delivery fee or an error message</returns>
        /// <response code="200">Returns the calculated delivery fee</response>
        /// <response code="400">If the city or vehicle type is invalid</response>
        /// <response code="403">If the vehicle usage is forbidden due to weather conditions</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CalculateDeliveryFee([FromQuery] string city, [FromQuery] string vehicleType)
        {
            _logger.LogInformation("Received delivery fee calculation request for city: {City}, vehicle type: {VehicleType}", city, vehicleType);

            try
            {
                var fee = await _deliveryFeeCalculationService.CalculateDeliveryFeeAsync(city, vehicleType);
                return Ok(fee);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input parameters: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("forbidden"))
            {
                _logger.LogWarning(ex, "Vehicle usage forbidden: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation error: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calculating delivery fee");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }
    }
}
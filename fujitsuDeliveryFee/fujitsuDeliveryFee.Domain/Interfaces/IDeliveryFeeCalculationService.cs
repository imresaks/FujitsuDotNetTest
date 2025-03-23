using System;
using System.Threading.Tasks;

namespace fujitsuDeliveryFee.Domain.Interfaces
{
    /// <summary>
    /// Interface for delivery fee calculation service operations
    /// </summary>
    public interface IDeliveryFeeCalculationService
    {
        /// <summary>
        /// Calculates the delivery fee based on city and vehicle type
        /// </summary>
        /// <param name="city">The city for delivery (Tallinn, Tartu, Pärnu)</param>
        /// <param name="vehicleType">The vehicle type (Car, Scooter, Bike)</param>
        /// <returns>The calculated delivery fee or throws an exception if vehicle usage is forbidden</returns>
        Task<decimal> CalculateDeliveryFeeAsync(string city, string vehicleType);
    }
}
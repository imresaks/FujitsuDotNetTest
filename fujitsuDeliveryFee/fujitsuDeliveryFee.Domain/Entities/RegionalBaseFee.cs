using System;

namespace fujitsuDeliveryFee.Domain.Entities
{
    /// <summary>
    /// Represents the base fee for a specific city and vehicle type combination
    /// </summary>
    public class RegionalBaseFee
    {
        /// <summary>
        /// Unique identifier for the regional base fee
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// City ID this fee applies to
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// Vehicle type ID this fee applies to
        /// </summary>
        public int VehicleTypeId { get; set; }

        /// <summary>
        /// Base fee amount in euros
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Navigation property for the City
        /// </summary>
        public City? City { get; set; }

        /// <summary>
        /// Navigation property for the VehicleType
        /// </summary>
        public VehicleType? VehicleType { get; set; }
    }
}
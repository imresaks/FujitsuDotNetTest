using System;

namespace fujitsuDeliveryFee.Domain.Entities
{
    /// <summary>
    /// Represents a type of vehicle used for delivery
    /// </summary>
    public class VehicleType
    {
        /// <summary>
        /// Unique identifier for the vehicle type
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the vehicle type (Car, Scooter, Bike)
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
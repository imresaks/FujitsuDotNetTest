using System;

namespace fujitsuDeliveryFee.Domain.Entities
{
    /// <summary>
    /// Represents a city where delivery service is available
    /// </summary>
    public class City
    {
        /// <summary>
        /// Unique identifier for the city
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the city
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Weather station name associated with this city
        /// </summary>
        public string StationName { get; set; } = string.Empty;
    }
}
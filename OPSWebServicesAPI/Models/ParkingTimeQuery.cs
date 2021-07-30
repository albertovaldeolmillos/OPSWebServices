using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request parking operation time
    /// </summary>
    public class ParkingTimeQuery
    {
        /// <summary>
        /// (*) Contract ID
        /// </summary>
        [DisplayName("contid")]
        public string contractId { get; set; }
        /// <summary>
        /// (*) Plate
        /// </summary>
        [DisplayName("p")]
        public string plate { get; set; }
        /// <summary>
        /// (*) Sector
        /// </summary>
        [DisplayName("g")]
        public string sector { get; set; }
        /// <summary>
        /// (*) Time in minutes
        /// </summary>
        [DisplayName("t")]
        public string time { get; set; }
    }
}
using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request parking operation with time steps
    /// </summary>
    public class ParkingStepsQuery
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
        /// Date in format hh24missddMMYY
        /// </summary>
        [DisplayName("d")]
        public string datetime { get; set; }
    }
}
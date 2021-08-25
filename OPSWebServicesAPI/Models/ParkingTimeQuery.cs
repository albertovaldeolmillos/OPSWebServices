using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        [Required]
        public string contractId { get; set; }
        /// <summary>
        /// (*) Plate
        /// </summary>
        [DisplayName("p")]
        [Required]
        public string plate { get; set; }
        /// <summary>
        /// (*) Sector
        /// </summary>
        [DisplayName("g")]
        [Required]
        public string sector { get; set; }
        /// <summary>
        /// (*) Time in minutes
        /// </summary>
        [DisplayName("t")]
        [Required]
        public string time { get; set; }
    }
}
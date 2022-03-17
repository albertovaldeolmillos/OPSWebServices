using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request sectors
    /// </summary>
    public class SectorsQuery
    {
        /// <summary>
        /// (*) Contract ID
        /// </summary>
        [DisplayName("contid")]
        [Required]
        public int contractId { get; set; }
        /// <summary>
        /// Street ID
        /// </summary>
        [DisplayName("streetId")]
        [Required]
        public int streetId { get; set; }
        /// <summary>
        /// Longitude
        /// </summary>
        [DisplayName("lg")]
        public double longitude { get; set; }
        /// <summary>
        /// Latitude
        /// </summary>
        [DisplayName("lt")]
        public double latitude { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request zone
    /// </summary>
    public class ZoneQuery
    {
        /// <summary>
        /// (*) Contract ID
        /// </summary>
        [DisplayName("contid")]
        [Required]
        public int contractId { get; set; }
        /// <summary>
        /// (*) Longitude
        /// </summary>
        [DisplayName("lg")]
        [Required]
        public double longitude { get; set; }
        /// <summary>
        /// (*) Latitude
        /// </summary>
        [DisplayName("lt")]
        [Required]
        public double latitude { get; set; }
        /// <summary>
        /// (*) Street name
        /// </summary>
        [DisplayName("streetname")]
        [Required]
        public string streetname { get; set; }
        /// <summary>
        /// (*) Street number
        /// </summary>
        [DisplayName("streetno")]
        [Required]
        public string streetno { get; set; }
    }
}
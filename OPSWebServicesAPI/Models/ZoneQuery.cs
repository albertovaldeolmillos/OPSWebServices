using System.ComponentModel;

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
        public string contractId { get; set; }
        /// <summary>
        /// (*) Longitude
        /// </summary>
        [DisplayName("lg")]
        public string longitude { get; set; }
        /// <summary>
        /// (*) Latitude
        /// </summary>
        [DisplayName("lt")]
        public string latitude { get; set; }
        /// <summary>
        /// (*) Street name
        /// </summary>
        [DisplayName("streetname")]
        public string streetname { get; set; }
        /// <summary>
        /// (*) Street number
        /// </summary>
        [DisplayName("streetno")]
        public string streetno { get; set; }
    }
}
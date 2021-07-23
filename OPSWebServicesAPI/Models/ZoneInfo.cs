using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Zone information
    /// </summary>
    public class ZoneInfo
    {
        /// <summary>
        /// Longitude
        /// </summary>
        [DisplayName("lg")]
        public string longitude { get; set; }
        /// <summary>
        /// Latitude
        /// </summary>
        [DisplayName("lt")]
        public string latitude { get; set; }
        /// <summary>
        /// Street name
        /// </summary>
        [DisplayName("streetname")]
        public string streetname { get; set; }
        /// <summary>
        /// Street number
        /// </summary>
        [DisplayName("streetno")]
        public string streetno { get; set; }
        /// <summary>
        /// Zone
        /// </summary>
        [DisplayName("zone")]
        public string zone { get; set; }
        /// <summary>
        /// Zone color
        /// </summary>
        [DisplayName("zonecolor")]
        public string zonecolor { get; set; }
        /// <summary>
        /// Zone name
        /// </summary>
        [DisplayName("zonename")]
        public string zonename { get; set; }
        /// <summary>
        /// Sector
        /// </summary>
        [DisplayName("sector")]
        public string sector { get; set; }
        /// <summary>
        /// Sector color
        /// </summary>
        [DisplayName("sectorcolor")]
        public string sectorcolor { get; set; }
        /// <summary>
        /// Sector name
        /// </summary>
        [DisplayName("sectorname")]
        public string sectorname { get; set; }
    }
}
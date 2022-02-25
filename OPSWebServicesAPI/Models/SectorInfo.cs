using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Sector information
    /// </summary>
    public class SectorInfo
    {
        /// <summary>
        /// zone identification number
        /// </summary>
        [DisplayName("zoneId")]
        public int zoneId { get; set; }
        /// <summary>
        /// zone name
        /// </summary>
        [DisplayName("zone")]
        public string zone { get; set; }
        /// <summary>
        /// zone color
        /// </summary>
        [DisplayName("zoneColor")]
        public string zoneColor { get; set; }
        /// <summary>
        /// sector identification number
        /// </summary>
        [DisplayName("sectorId")]
        public int sectorId { get; set; }
        /// <summary>
        /// sector name
        /// </summary>
        [DisplayName("sector")]
        public string sector { get; set; }
        /// <summary>
        /// sector color
        /// </summary>
        [DisplayName("sectorColor")]
        public string sectorColor { get; set; }
    }
}
using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Sectors information
    /// </summary>
    public class SectorsInfo
    {
        /// <summary>
        /// Number of sectors
        /// </summary>
        [DisplayName("sectorsNumber")]
        public int sectorsNumber { get; set; }
        /// <summary>
        /// Sector list
        /// </summary>
        [DisplayName("sectorlist")]
        public SectorInfo[] sectorlist { get; set; }
    }
}

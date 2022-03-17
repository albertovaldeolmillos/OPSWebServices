using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Streets information
    /// </summary>
    public class StreetsFullInfo
    {
        /// <summary>
        /// Number of streets with sector active
        /// </summary>
        [DisplayName("streetsFullNumber")]
        public int streetsFullNumber { get; set; }
        /// <summary>
        /// Street list
        /// </summary>
        [DisplayName("streetsFulllist")]
        public StreetFullInfo[] streetsFulllist { get; set; }
    }
}

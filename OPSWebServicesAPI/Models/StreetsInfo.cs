using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Streets information
    /// </summary>
    public class StreetsInfo
    {
        /// <summary>
        /// Number of streets
        /// </summary>
        [DisplayName("st_no")]
        public string streetsNumber { get; set; }
        /// <summary>
        /// Current Date in format hh24missddMMYY
        /// </summary>
        [DisplayName("t")]
        public string datetime { get; set; }
        /// <summary>
        /// Result of the method
        /// </summary>
        [DisplayName("r")]
        public string result { get; set; }
        /// <summary>
        /// Streets name list
        /// </summary>
        [DisplayName("streetlist")]
        public string[] streetlist { get; set; }
    }
}
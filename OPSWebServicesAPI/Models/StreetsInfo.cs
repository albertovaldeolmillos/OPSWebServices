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
        /// Result of the method:
        ///1: Parking of extension is possible and the restrictions come after this tag.
        ///-1: Invalid authentication hash
        ///-9: Generic Error (for example database or execution error)
        ///-10: Invalid input parameter
        ///-11: Missing input parameter
        ///-12: OPS System error
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
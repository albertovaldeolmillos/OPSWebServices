using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Place information
    /// </summary>
    public class PlaceInfo
    {
        /// <summary>
        /// Result of the method:
        ///1: Parking of extension is possible and the restrictions come after this tag.
        ///-1: Invalid authentication hash
        ///-9: Generic Error (for example database or execution error)
        ///-10: Invalid input parameter
        ///-11: Missing input parameter
        ///-12: OPS System error
        ///-30: Location not found
        /// </summary>
        [DisplayName("r")]
        public string result { get; set; }
        /// <summary>
        /// json response from google link: https://maps.googleapis.com/maps/api/place/autocomplete/json
        /// </summary>
        [DisplayName("response")]
        public string response { get; set; }
    }
}
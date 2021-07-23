using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Place information
    /// </summary>
    public class PlaceInfo
    {
        /// <summary>
        /// Result of the method
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
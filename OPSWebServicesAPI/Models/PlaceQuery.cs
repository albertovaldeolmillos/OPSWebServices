using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request place
    /// </summary>
    public class PlaceQuery
    {
        /// <summary>
        /// (*) Contract ID
        /// </summary>
        [DisplayName("contid")]
        public string contractId { get; set; }
        /// <summary>
        /// (*) Street name
        /// </summary>
        [DisplayName("streetname")]
        public string streetname { get; set; }
        /// <summary>
        /// Street number
        /// </summary>
        [DisplayName("streetno")]
        public string streetno { get; set; }
    }
}
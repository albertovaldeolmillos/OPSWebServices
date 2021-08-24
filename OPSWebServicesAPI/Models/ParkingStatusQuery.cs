using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request parking status
    /// </summary>
    public class ParkingStatusQuery
    {
        /// <summary>
        /// (*) Contract ID
        /// </summary>
        [DisplayName("contid")]
        public string contractId { get; set; }
        /// <summary>
        /// (*) Plate
        /// </summary>
        [DisplayName("p")]
        public string plate { get; set; }
        /// <summary>
        /// (*) Mobile user id (authorization token)
        /// </summary>
        [DisplayName("mui")]
        public string authorizationToken { get; set; }
        /// <summary>
        /// date in format hh24missddMMYY
        /// </summary>
        [DisplayName("d")]
        public string date { get; set; }
    }
}
using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request unparking confirm
    /// </summary>
    public class UnParkingConfirmQuery
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
        /// (*) Amount refunded in Euro Cents
        /// </summary>
        [DisplayName("q")]
        public string quantity { get; set; }
        /// <summary>
        /// (*) Mobile user id (authorization token)
        /// </summary>
        [DisplayName("mui")]
        public string authorizationToken { get; set; }
        /// <summary>
        /// (*) Cloud token
        /// </summary>
        [DisplayName("cid")]
        public string cloudToken { get; set; }
        /// <summary>
        /// (*) Operating system: 1 (Android), 2 (iOS)
        /// </summary>
        [DisplayName("os")]
        public string operatingSystem { get; set; }
        /// <summary>
        /// date in format hh24missddMMYY
        /// </summary>
        [DisplayName("d")]
        public string date { get; set; }
    }
}
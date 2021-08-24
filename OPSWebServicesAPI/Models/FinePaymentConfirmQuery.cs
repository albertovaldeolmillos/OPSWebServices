using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request fine payment confirm
    /// </summary>
    public class FinePaymentConfirmQuery
    {
        /// <summary>
        /// (*) Contract ID
        /// </summary>
        [DisplayName("contid")]
        public string contractId { get; set; }
        /// <summary>
        /// (*) fine number
        /// </summary>
        [DisplayName("f")]
        public string fine { get; set; }
        /// <summary>
        /// (*) quantity paid in Euro Cents for the fine
        /// </summary>
        [DisplayName("q")]
        public string quantity { get; set; }
        /// <summary>
        /// date in format hh24missddMMYY
        /// </summary>
        [DisplayName("d")]
        public string date { get; set; }
        /// <summary>
        /// (*) Cloud token
        /// </summary>
        [DisplayName("cid")]
        public string cloudToken { get; set; }
        /// <summary>
        /// (*) Mobile user id (authorization token)
        /// </summary>
        [DisplayName("mui")]
        public string authorizationToken { get; set; }
        /// <summary>
        /// (*) Operating system: 1 (Android), 2 (iOS)
        /// </summary>
        [DisplayName("os")]
        public string operatingSystem { get; set; }
    }
}
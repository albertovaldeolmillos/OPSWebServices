using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        [Required]
        public int contractId { get; set; }
        /// <summary>
        /// (*) fine number
        /// </summary>
        [DisplayName("f")]
        [Required]
        public string fine { get; set; }
        /// <summary>
        /// (*) quantity paid in Euro Cents for the fine
        /// </summary>
        [DisplayName("q")]
        [Required]
        public int quantity { get; set; }
        /// <summary>
        /// date in format hh24missddMMYY
        /// </summary>
        [DisplayName("d")]
        public string date { get; set; }
        /// <summary>
        /// (*) Cloud token
        /// </summary>
        [DisplayName("cid")]
        [Required]
        public string cloudToken { get; set; }
        /// <summary>
        /// (*) Mobile user id (authorization token)
        /// </summary>
        [DisplayName("mui")]
        [Required]
        public string authorizationToken { get; set; }
        /// <summary>
        /// (*) Operating system: 1 (Android), 2 (iOS)
        /// </summary>
        [DisplayName("os")]
        [Required]
        public int operatingSystem { get; set; }
    }
}
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        [Required]
        public string contractId { get; set; }
        /// <summary>
        /// (*) Plate
        /// </summary>
        [DisplayName("p")]
        [Required]
        public string plate { get; set; }
        /// <summary>
        /// (*) Amount refunded in Euro Cents
        /// </summary>
        [DisplayName("q")]
        [Required]
        public string quantity { get; set; }
        /// <summary>
        /// (*) Mobile user id (authorization token)
        /// </summary>
        [DisplayName("mui")]
        [Required]
        public string authorizationToken { get; set; }
        /// <summary>
        /// (*) Cloud token
        /// </summary>
        [DisplayName("cid")]
        [Required]
        public string cloudToken { get; set; }
        /// <summary>
        /// (*) Operating system: 1 (Android), 2 (iOS)
        /// </summary>
        [DisplayName("os")]
        [Required]
        public string operatingSystem { get; set; }
        /// <summary>
        /// date in format hh24missddMMYY
        /// </summary>
        [DisplayName("d")]
        [Required]
        public string date { get; set; }
    }
}
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request parking confirm
    /// </summary>
    public class ParkingConfirmQuery
    {
        /// <summary>
        /// (*) Contract ID
        /// </summary>
        [DisplayName("contid")]
        [Required]
        public int contractId { get; set; }
        /// <summary>
        /// (*) Plate
        /// </summary>
        [DisplayName("p")]
        [Required]
        public string plate { get; set; }
        /// <summary>
        /// (*) Sector
        /// </summary>
        [DisplayName("g")]
        [Required]
        public string sector { get; set; }
        /// <summary>
        /// (*) Amount of money paid in Euro cents
        /// </summary>
        [DisplayName("q")]
        [Required]
        public int quantity { get; set; }
        /// <summary>
        /// (*) tariff type to apply: For example: 4 (ROTATION), 5 (RESIDENTS), 6 VIPS
        /// </summary>
        [DisplayName("ad")]
        [Required]
        public int tariffType { get; set; }
        /// <summary>
        /// (*) Mobile user id (authorization token)
        /// </summary>
        //[DisplayName("mui")]
        //[Required]
        //public string authorizationToken { get; set; }
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
        public int operatingSystem { get; set; }
        /// <summary>
        /// date in format hh24missddMMYY
        /// </summary>
        [DisplayName("d")]
        public string date { get; set; }
        /// <summary>
        /// Time in minutes obtained paying q cents
        /// </summary>
        [DisplayName("t")]
        public int time { get; set; }
        /// <summary>
        /// Longitude
        /// </summary>
        [DisplayName("lg")]
        public double longitude { get; set; }
        /// <summary>
        /// Latitude
        /// </summary>
        [DisplayName("lt")]
        public double latitude { get; set; }
        /// <summary>
        /// Reference of current operation
        /// </summary>
        [DisplayName("re")]
        public string reference { get; set; }
        /// <summary>
        /// Space id
        /// </summary>
        [DisplayName("spcid")]
        public string spaceId { get; set; }
        /// <summary>
        /// (*) Street name
        /// </summary>
        [DisplayName("streetname")]
        public string streetname { get; set; }
        /// <summary>
        /// Street address number
        /// </summary>
        [DisplayName("streetno")]
        public string streetno { get; set; }
    }
}
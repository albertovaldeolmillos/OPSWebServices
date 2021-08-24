using System.ComponentModel;

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
        public string contractId { get; set; }
        /// <summary>
        /// (*) Plate
        /// </summary>
        [DisplayName("p")]
        public string plate { get; set; }
        /// <summary>
        /// (*) Sector
        /// </summary>
        [DisplayName("g")]
        public string sector { get; set; }
        /// <summary>
        /// (*) Amount of money paid in Euro cents
        /// </summary>
        [DisplayName("q")]
        public string quantity { get; set; }
        /// <summary>
        /// (*) tariff type to apply: For example: 4 (ROTATION), 5 (RESIDENTS), 6 VIPS
        /// </summary>
        [DisplayName("ad")]
        public string tariffType { get; set; }
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
        /// <summary>
        /// Time in minutes obtained paying q cents
        /// </summary>
        [DisplayName("t")]
        public string time { get; set; }
        /// <summary>
        /// Longitude
        /// </summary>
        [DisplayName("lg")]
        public string longitude { get; set; }
        /// <summary>
        /// Latitude
        /// </summary>
        [DisplayName("lt")]
        public string latitude { get; set; }
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
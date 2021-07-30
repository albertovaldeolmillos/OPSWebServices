using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request parking operation money quantity
    /// </summary>
    public class ParkingMoneyQuery
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
        /// (*) Quantity in cents
        /// </summary>
        [DisplayName("q")]
        public string quantity { get; set; }
    }
}
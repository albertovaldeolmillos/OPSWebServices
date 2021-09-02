using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        /// (*) Quantity in cents
        /// </summary>
        [DisplayName("q")]
        [Required]
        public int quantity { get; set; }
    }
}
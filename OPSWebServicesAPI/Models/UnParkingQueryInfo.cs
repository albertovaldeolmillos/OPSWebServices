using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// UnParking operation information
    /// </summary>
    public class UnParkingQueryInfo
    {
        /// <summary>
        /// Result of the method:
        ///1: UnParking is possible and the restrictions come after this tag.
        ///-1: Invalid authentication hash
        ///-4: Plate has no rights for doing an unparking operation
        ///-9: Generic Error (for example database or execution error.)
        ///-10: Invalid input parameter
        ///-11: Missing input parameter
        ///-12: OPS System error
        /// </summary>
        [DisplayName("r")]
        public int result { get; set; }
        /// <summary>
        /// tariff type to apply: For example: 4 (ROTATION), 5 (RESIDENTS), 6 VIPS
        /// </summary>
        [DisplayName("ad")]
        public int tariffType { get; set; }
        /// <summary>
        /// Tariff time in minutes for the parking operation chain after unparking (d2-d1)
        /// </summary>
        [DisplayName("t")]
        public int tariffTime { get; set; }
        /// <summary>
        /// quantity in Euro Cents to be refunded
        /// </summary>
        [DisplayName("q")]
        public int payAmount { get; set; }
        /// <summary>
        /// Initial date (in format hh24missddMMYY) for the parking operation chain (first parking, extensions and unparking operation) after unparking
        /// </summary>
        [DisplayName("d1")]
        public string dateInitial { get; set; }
        /// <summary>
        /// End date (in format hh24missddMMYY) for the parking operation chain (first parking, extensions and unparking operation) after unparking
        /// </summary>
        [DisplayName("d2")]
        public string dateEnd { get; set; }
    }
}
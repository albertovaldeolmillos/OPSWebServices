using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Parking operation time information
    /// </summary>
    public class ParkingTimeInfo
    {
        /// <summary>
        /// Result of the method:
        ///1: Parking of extension is possible and the restrictions come after this tag.
        ///-1: Invalid authentication hash
        ///-2: The plate has used the maximum amount of time/money in the sector, so the extension is not possible. In Bilbao this depends on the colour of the zone and the tariff type.
        ///-3: The plate has not waited enough to return to the current sector.
        ///-9: Generic Error (for example database or execution error.)
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
        /// Operation Type: 1: First parking: 2: extension
        /// </summary>
        [DisplayName("o")]
        public int operationType { get; set; }
        /// <summary>
        /// amount needed to arrive to the input parameter t 
        /// </summary>
        [DisplayName("q")]
        public int payAmount { get; set; }
        /// <summary>
        /// Final date of the parking
        /// </summary>
        [DisplayName("d")]
        public string date { get; set; }
        /// <summary>
        /// Initial date (in format hh24missddMMYY) of the parking: the same as the input date if the operation is a first parking, or the date of the end of parking operations chain if the operation is an extension</di>
        /// </summary>
        [DisplayName("di")]
        public string dateInitial { get; set; }
        /// <summary>
        /// Amount of Euro Cents accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation
        /// </summary>
        [DisplayName("aq")]
        public int accumulatedQuantity { get; set; }
        /// <summary>
        /// Amount of minutes accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation
        /// </summary>
        [DisplayName("at")]
        public int accumulatedTime { get; set; }

    }
}
using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    public class ParkingMoneyInfo
    {
        /// <summary>
        /// Result of the method
        /// </summary>
        [DisplayName("r")]
        public string result { get; set; }
        /// <summary>
        /// tariff type to apply: For example: 4 (ROTATION), 5 (RESIDENTS), 6 VIPS
        /// </summary>
        [DisplayName("ad")]
        public string tariffType { get; set; }
        /// <summary>
        /// Operation Type: 1: First parking: 2: extension
        /// </summary>
        [DisplayName("o")]
        public string operationType { get; set; }
        /// <summary>
        /// time in minutes given by the amount for money q 
        /// </summary>
        [DisplayName("t")]
        public string time { get; set; }
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
        public string accumulatedQuantity { get; set; }
        /// <summary>
        /// Amount of minutes accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation
        /// </summary>
        [DisplayName("at")]
        public string accumulatedTime { get; set; }
    }
}
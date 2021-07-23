using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Parking operation with time steps information
    /// </summary>
    public class ParkingStepsInfo
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
        /// minimum amount to pay in Euro cents
        /// </summary>
        [DisplayName("q1")]
        public string payAmountMin { get; set; }
        /// <summary>
        /// maximum amount to pay in Euro cents
        /// </summary>
        [DisplayName("q2")]
        public string payAmountMax { get; set; }
        /// <summary>
        /// minimum amount of time to park in minutes
        /// </summary>
        [DisplayName("t1")]
        public string timeAmountMin { get; set; }
        /// <summary>
        /// maximum amount of time to park in minutes
        /// </summary>
        [DisplayName("t2")]
        public string timeAmountMax { get; set; }
        /// <summary>
        /// minimum date
        /// </summary>
        [DisplayName("d1")]
        public string dateMin { get; set; }
        /// <summary>
        /// maximum date
        /// </summary>
        [DisplayName("d2")]
        public string dateMax { get; set; }
        /// <summary>
        /// Initial date (in format hh24missddMMYY) of the parking: the same as the input date if the operation is a first parking, or the date of the end of parking operations chain if the operation is an extension
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
        /// <summary>
        /// Steps
        /// </summary>
        [DisplayName("lst")]
        public Step[] steps { get; set; }
    }
}
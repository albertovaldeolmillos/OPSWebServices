using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Parking status rotation information
    /// </summary>
    public class ParkingStatusRotationInfo
    {
        /// <summary>
        /// Result of the method:
        /// </summary>
        [DisplayName("r")]
        public int result { get; set; }
        /// <summary>
        /// status: 1 (UNPARKED), 2 (PARKED)
        /// </summary>
        [DisplayName("sta")]
        public int status { get; set; }
        /// <summary>
        /// Extension is permitted in the current sector?: 1 (YES), -2 (NO YET), -3 (The plate has used the maximun amount of time/money in the sector)
        /// </summary>
        [DisplayName("ex")]
        public int extension { get; set; }
        /// <summary>
        /// tariff ID: 101 (ESRO non resident), 103 (ESRE non resident)
        /// </summary>
        [DisplayName("id")]
        public int tariffId { get; set; }
        /// <summary>
        /// Tariff type of current operation: 4 (ROTATION), 6 (VIPS)
        /// </summary>
        [DisplayName("ad")]
        public int tariffType { get; set; }
        /// <summary>
        /// Operation Type: 1: First parking: 2: extension
        /// </summary>
        [DisplayName("o")]
        public int operationType { get; set; }
        /// <summary>
        /// Initial date of current operation (in format hh24missddMMYY) of the parking: the same as the input date if the operation is a first parking, or the date of the end of parking operations chain if the operation is an extension
        /// </summary>
        [DisplayName("di")]
        public string dateInitial { get; set; }
        /// <summary>
        /// End date of current operation (in format hh24missddMMYY) of the parking. In order to show the user the end of the current parking operation.
        /// </summary>
        [DisplayName("df")]
        public string dateEnd { get; set; }
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
        /// <summary>
        /// Sector
        /// </summary>
        [DisplayName("g")]
        public string sector { get; set; }
        /// <summary>
        /// Sector color
        /// </summary>
        [DisplayName("sectorcolor")]
        public string sectorcolor { get; set; }
        /// <summary>
        /// Sector name
        /// </summary>
        [DisplayName("sectorname")]
        public string sectorname { get; set; }
        /// <summary>
        /// Zone
        /// </summary>
        [DisplayName("zone")]
        public string zone { get; set; }
        /// <summary>
        /// Zone color
        /// </summary>
        [DisplayName("zonecolor")]
        public string zonecolor { get; set; }
        /// <summary>
        /// Zone name
        /// </summary>
        [DisplayName("zonename")]
        public string zonename { get; set; }
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
        /// Reference of current operation. 128 characters maximum
        /// </summary>
        [DisplayName("re")]
        public string reference { get; set; }
        /// <summary>
        /// Refundable tariff: 0 (NO), 1 (YES)
        /// </summary>
        [DisplayName("rfd")]
        public int refundable { get; set; }
        /// <summary>
        /// Operation date
        /// </summary>
        [DisplayName("od")]
        public string operationDate { get; set; }
        /// <summary>
        /// Street name
        /// </summary>
        [DisplayName("streetname")]
        public string streetname { get; set; }
        /// <summary>
        /// Street number
        /// </summary>
        [DisplayName("streetno")]
        public string streetno { get; set; }
    }
}
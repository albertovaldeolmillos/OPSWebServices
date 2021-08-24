using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Parking status information
    /// </summary>
    public class ParkingStatusInfo
    {
        /// <summary>
        /// Parking status rotation information
        /// </summary>
        [DisplayName("parkingStatusRotationInfo")]
        public ParkingStatusRotationInfo parkingStatusRotationInfo { get; set; }
        /// <summary>
        /// Parking status resident information
        /// </summary>
        [DisplayName("parkingStatusResidentInfo")]
        public ParkingStatusResidentInfo parkingStatusResidentInfo { get; set; }
        /// <summary>
        /// Parking status tariffs information
        /// </summary>
        [DisplayName("parkingStatusTariffsInfo")]
        public ParkingStatusTariffInfo[] parkingStatusTariffsInfo { get; set; }
    }
}
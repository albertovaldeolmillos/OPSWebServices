using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Parking status tariffs information
    /// </summary>
    public class ParkingStatusTariffInfo
    {
        /// <summary>
        /// tariff id
        /// </summary>
        [DisplayName("tarid")]
        public int tariffId { get; set; }
        /// <summary>
        /// tariff description
        /// </summary>
        [DisplayName("tardesc")]
        public string tariffDescription { get; set; }
        /// <summary>
        /// Tariff type
        /// </summary>
        [DisplayName("tarad")]
        public int tariffType { get; set; }
        /// <summary>
        /// If Tariff 1 is refundable: 0 (NO), 1 (YES)
        /// </summary>
        [DisplayName("tarrfd")]
        public int tariffRefundable { get; set; }
    }
}
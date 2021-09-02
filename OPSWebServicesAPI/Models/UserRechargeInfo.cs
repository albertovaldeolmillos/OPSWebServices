using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// User recharge credit information
    /// </summary>
    public class UserRechargeInfo
    {
        /// <summary>
        /// Result of the method:
        ///1: Data come after this tag
        ///-1: Invalid authentication hash
        ///-9: Generic Error (for example database or execution error)
        ///-10: Invalid input parameter
        ///-11: Missing input parameter
        ///-12: OPS System error
        ///-20: User not found.
        /// </summary>
        [DisplayName("r")]
        public int result { get; set; }
        /// <summary>
        /// Notification URL for payment gateway response
        /// </summary>
        [DisplayName("mu")]
        public string urlResponse { get; set; }
        /// <summary>
        /// Order ID
        /// </summary>
        [DisplayName("or")]
        public string orderId { get; set; }
    }
}
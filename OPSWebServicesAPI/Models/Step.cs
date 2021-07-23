using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Step information
    /// </summary>
    public class Step
    {
        /// <summary>
        /// Time in minutes
        /// </summary>
        [DisplayName("t")]
        public string time { get; set; }
        /// <summary>
        /// Cost in Euro Cents for t minutes
        /// </summary>
        [DisplayName("q")]
        public string quantity { get; set; }
        /// <summary>
        /// datetime given by q cents
        /// </summary>
        [DisplayName("d")]
        public string datetime { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Contracts information
    /// </summary>
    public class ContractsInfo
    {
        /// <summary>
        /// Number of contracts
        /// </summary>
        [DisplayName("cont_no")]
        public string contractsNumber { get; set; }
        /// <summary>
        /// Contract list
        /// </summary>
        [DisplayName("contractlist")]
        public Contract[] contractlist { get; set; }
        /// <summary>
        /// Result of the method
        /// </summary>
        [DisplayName("r")]
        public string result { get; set; }
        /// <summary>
        /// Current Date in format hh24missddMMYY
        /// </summary>
        [DisplayName("t")]
        public string time { get; set; }

    }
}
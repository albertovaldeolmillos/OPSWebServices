using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request user operations
    /// </summary>
    public class UserOperation
    {
        /// <summary>
        /// Last x days from now
        /// </summary>
        public int d { get; set; }
        /// <summary>
        /// Filter. List of Operation types (1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing, 104: Unpaid fines)
        /// </summary>
        public int[] ots { get; set; }
        /// <summary>
        /// Contract ID
        /// </summary>
        public int contid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string msgId { get; set; }
        /// <summary>
        /// Mobile user id (authorization token)
        /// </summary>
        public string mui { get; set; }

    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        /// (*) Last x days from now
        /// </summary>
        [DisplayName("d")]
        [Required]
        public int daysFromNow { get; set; }
        /// <summary>
        /// (*) Filter. List of Operation types (1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing, 104: Unpaid fines)
        /// </summary>
        [DisplayName("ots")]
        [Required]
        public int[] operationTypeList { get; set; }
        /// <summary>
        /// (*) Contract ID
        /// </summary>
        [DisplayName("contid")]
        [Required]
        public int contractId { get; set; }
        /// <summary>
        /// (*) Mobile user id (authorization token)
        /// </summary>
        [DisplayName("mui")]
        [Required]
        public string authorizationToken { get; set; }

    }
}
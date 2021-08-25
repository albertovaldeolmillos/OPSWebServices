using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    public class Notification
    {
        /// <summary>
        /// (*) low balance notification
        /// </summary>
        [DisplayName("ba")]
        [Required]
        public string balance { get; set; }
        /// <summary>
        /// (*) Fine notifications? (1:true, 0:false)
        /// </summary>
        [DisplayName("fn")]
        [Required]
        public string fineNotifications { get; set; }
        /// <summary>
        /// (*) low balance amount
        /// </summary>
        [DisplayName("q_ba")]
        [Required]
        public string quantityBalance { get; set; }
        /// <summary>
        /// (*) recharge notifications? (1:true, 0:false)
        /// </summary>
        [DisplayName("re")]
        [Required]
        public string rechargeNotifications { get; set; }
        /// <summary>
        /// (*) minutes before the limit (unparking notifications)
        /// </summary>
        [DisplayName("t_unp")]
        [Required]
        public string minutesBeforeUnparking { get; set; }
        /// <summary>
        /// (*) UnParking notifications? (1:true, 0:false)
        /// </summary>
        [DisplayName("unp")]
        [Required]
        public string unparkingNotifications { get; set; }
    }
}
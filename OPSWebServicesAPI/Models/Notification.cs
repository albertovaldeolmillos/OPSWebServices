using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public string balance { get; set; }
        /// <summary>
        /// (*) Fine notifications? (1:true, 0:false)
        /// </summary>
        [DisplayName("fn")]
        public string fineNotifications { get; set; }
        /// <summary>
        /// (*) low balance amount
        /// </summary>
        [DisplayName("q_ba")]
        public string quantityBalance { get; set; }
        /// <summary>
        /// (*) recharge notifications? (1:true, 0:false)
        /// </summary>
        [DisplayName("re")]
        public string rechargeNotifications { get; set; }
        /// <summary>
        /// (*) minutes before the limit (unparking notifications)
        /// </summary>
        [DisplayName("t_unp")]
        public string minutesBeforeUnparking { get; set; }
        /// <summary>
        /// (*) UnParking notifications? (1:true, 0:false)
        /// </summary>
        [DisplayName("unp")]
        public string unparkingNotifications { get; set; }
    }
}
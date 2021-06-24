using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    public class Plate
    {
        /// <summary>
        /// Plate
        /// </summary>
        [DisplayName("p")]
        public string plate { get; set; }
        /// <summary>
        /// status (1:Rotative, 2:Resident, 3:VIP
        /// </summary>
        [DisplayName("stp")]
        public string status { get; set; }
    }
}
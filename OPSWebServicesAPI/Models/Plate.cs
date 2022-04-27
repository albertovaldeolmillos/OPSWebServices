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
        /// status 4:Rotative, 5:Resident, 6:VIP
        /// </summary>
        [DisplayName("stp")]
        public int status { get; set; }
    }
}
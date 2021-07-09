using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Contract object
    /// </summary>
    public class Contract
    {
        /// <summary>
        /// Address
        /// </summary>
        [DisplayName("addr")]
        public string address { get; set; }
        /// <summary>
        /// Contract ID
        /// </summary>
        [DisplayName("cont_id")]
        public string contractId { get; set; }
        /// <summary>
        /// Description 1 (name in Spanish)
        /// </summary>
        [DisplayName("desc1")]
        public string description1 { get; set; }
        /// <summary>
        /// Description 2 (name in Basque)
        /// </summary>
        [DisplayName("desc2")]
        public string description2 { get; set; }
        /// <summary>
        /// E-Mail
        /// </summary>
        [DisplayName("email")]
        public string email { get; set; }
        /// <summary>
        /// Image path
        /// </summary>
        [DisplayName("image")]
        public string imagePath { get; set; }
        /// <summary>
        /// Longitude
        /// </summary>
        [DisplayName("lg")]
        public string longitude { get; set; }
        /// <summary>
        /// Latitude
        /// </summary>
        [DisplayName("lt")]
        public string latitude { get; set; }
        /// <summary>
        /// Telephone
        /// </summary>
        [DisplayName("phone")]
        public string phone { get; set; }
        /// <summary>
        /// Radius (in meters)
        /// </summary>
        [DisplayName("rad")]
        public string radius { get; set; }
        /// <summary>
        /// WSMobilePayment.asmx url
        /// </summary>
        [DisplayName("wsoper")]
        public string wsoper { get; set; }
        /// <summary>
        /// WSUserManagement url
        /// </summary>
        [DisplayName("wsuser")]
        public string wsuser { get; set; }
    }
}
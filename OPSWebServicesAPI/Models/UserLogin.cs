using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request Login
    /// </summary>
    public class UserLogin
    {
        /// <summary>
        /// (*) Cloud token
        /// </summary>
        [DisplayName("cid")]
        public string cloudToken { get; set; }
        /// <summary>
        /// (*) id del municipio
        /// </summary>
        [DisplayName("contid")]
        public string contractId { get; set; }
        /// <summary>
        /// (*) OS --> 1- Android 2- iOS 3- web
        /// </summary>
        [DisplayName("os")]
        public string operatingSystem { get; set; }
        /// <summary>
        /// (*) login
        /// </summary>
        [DisplayName("un")]
        public string userName { get; set; }
        /// <summary>
        /// (*) password
        /// </summary>
        [DisplayName("pw")]
        public string password { get; set; }
        /// <summary>
        /// (*) Version OS
        /// </summary>
        [DisplayName("v")]
        public string versionOS { get; set; }
        /// <summary>
        /// (*) Authentication token
        /// </summary>
        [DisplayName("mui")]
        public string authorizationToken { get; set; }
        /// <summary>
        /// hash
        /// </summary>
        [DisplayName("ah")]
        public string authenticationHash { get; set; }
    }
}
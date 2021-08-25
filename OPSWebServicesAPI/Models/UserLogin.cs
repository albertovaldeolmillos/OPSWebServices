using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        public string cloudToken { get; set; }
        /// <summary>
        /// (*) id del municipio
        /// </summary>
        [DisplayName("contid")]
        [Required]
        public string contractId { get; set; }
        /// <summary>
        /// (*) OS --> 1- Android 2- iOS 3- web
        /// </summary>
        [DisplayName("os")]
        [Required]
        public string operatingSystem { get; set; }
        /// <summary>
        /// (*) login
        /// </summary>
        [DisplayName("un")]
        [Required]
        public string userName { get; set; }
        /// <summary>
        /// (*) password
        /// </summary>
        [DisplayName("pw")]
        [Required]
        public string password { get; set; }
        /// <summary>
        /// (*) Version OS
        /// </summary>
        [DisplayName("v")]
        [Required]
        public string appVersion { get; set; }
        /// <summary>
        /// (*) Authentication token
        /// </summary>
        [DisplayName("mui")]
        [Required]
        public string authorizationToken { get; set; }
        /// <summary>
        /// hash
        /// </summary>
        [DisplayName("ah")]
        public string authenticationHash { get; set; }
    }
}
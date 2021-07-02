using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request user change password
    /// </summary>
    public class UserChangePassword
    {
        /// <summary>
        /// (*) Contract ID
        /// </summary>
        [DisplayName("contid")]
        public string contractId { get; set; }
        /// <summary>
        /// (*) User name
        /// </summary>
        [DisplayName("un")]
        public string userName { get; set; }
        /// <summary>
        /// (*) email
        /// </summary>
        [DisplayName("email")]
        public string email { get; set; }
        /// <summary>
        /// (*) Password to change
        /// </summary>
        [DisplayName("pw")]
        public string password { get; set; }
        /// <summary>
        /// (*) recode. Code verification
        /// </summary>
        [DisplayName("recode")]
        public string recode { get; set; }
    }
}
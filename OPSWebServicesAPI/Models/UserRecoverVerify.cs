using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request user recover verify
    /// </summary>
    public class UserRecoverVerify
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
        /// (*) recode. Code verification
        /// </summary>
        [DisplayName("recode")]
        public string recode { get; set; }
    }
}
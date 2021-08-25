using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        public string contractId { get; set; }
        /// <summary>
        /// (*) User name
        /// </summary>
        [DisplayName("un")]
        [Required]
        public string userName { get; set; }
        /// <summary>
        /// (*) email
        /// </summary>
        [DisplayName("email")]
        [Required]
        public string email { get; set; }
        /// <summary>
        /// (*) recode. Code verification
        /// </summary>
        [DisplayName("recode")]
        [Required]
        public string recode { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        /// Contract ID
        /// </summary>
        [DisplayName("contid")]
        [Required]
        public int contractId { get; set; }
        /// <summary>
        /// User name
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
        /// (*) Password to change
        /// </summary>
        [DisplayName("pw")]
        [Required]
        public string password { get; set; }
        /// <summary>
        /// (*) recode. Code verification
        /// </summary>
        [DisplayName("recode")]
        [Required]
        public string recode { get; set; }
    }
}
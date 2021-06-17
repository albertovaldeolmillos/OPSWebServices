using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// generic response
    /// </summary>
    public class PostResponse
    {
        /// <summary>
        /// Result
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// Is Success?
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// Error Message
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request Login
    /// </summary>
    public class LoginUser
    {
        /// <summary>
        /// Cloud token
        /// </summary>
        public string cid { get; set; }
        /// <summary>
        /// id del municipio
        /// </summary>
        public string contid { get; set; }
        /// <summary>
        /// OS --> 1- Android 2- iOS 3- web
        /// </summary>
        public string os { get; set; }
        /// <summary>
        /// login
        /// </summary>
        public string un { get; set; }
        /// <summary>
        /// password
        /// </summary>
        public string pw { get; set; }
        /// <summary>
        /// Version OS
        /// </summary>
        public string v { get; set; }
        /// <summary>
        /// Authentication token
        /// </summary>
        public string mui { get; set; }
        /// <summary>
        /// hash
        /// </summary>
        public string ah { get; set; }
    }
}
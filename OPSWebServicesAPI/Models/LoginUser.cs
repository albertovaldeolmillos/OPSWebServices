using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    public class LoginUser
    {
        //Cloud token
        public string cid { get; set; }
        //id del municipio
        public string contid { get; set; }
        //OS --> 1- Android 2- iOS 3- web
        public string os { get; set; }
        //login
        public string un { get; set; }
        //password
        public string pw { get; set; }
        //Version OS
        public string v { get; set; }
        //Authentication token
        public string mui { get; set; }
        //hash
        public string ah { get; set; }
    }
}
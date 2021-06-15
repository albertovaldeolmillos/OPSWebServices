using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    public class UpdateUser
    {
        public string abn { get; set; }
        public string aci { get; set; }
        public string add { get; set; }
        public string adf { get; set; }
        public string adl { get; set; }
        public object[] ads { get; set; }
        public object[] amp { get; set; }
        public string apc { get; set; }
        public string apr { get; set; }
        public string asn { get; set; }
        public string cid { get; set; }
        public string contid { get; set; }
        public string em { get; set; }
        public string fs { get; set; }
        public string mmp { get; set; }
        public string mui { get; set; }
        public string na { get; set; }
        public string nif { get; set; }
        public Notifications notifications { get; set; }
        public string os { get; set; }
        public Plate[] plates { get; set; }
        public string ss { get; set; }
        public string un { get; set; }
        public string v { get; set; }
        public string val { get; set; }
    }

    public class Notifications
    {
        public string ba { get; set; }
        public string fn { get; set; }
        public string q_ba { get; set; }
        public string re { get; set; }
        public string t_unp { get; set; }
        public string unp { get; set; }
    }

    public class Plate
    {
        public string p { get; set; }
        public string stp { get; set; }
    }
}
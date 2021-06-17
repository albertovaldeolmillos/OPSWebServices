using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Class to request update user
    /// </summary>
    public class UpdateUser
    {
        /// <summary>
        /// Address: Building Number
        /// </summary>
        public string abn { get; set; }
        /// <summary>
        /// Address: City
        /// </summary>
        public string aci { get; set; }
        /// <summary>
        /// Address: Department Door
        /// </summary>
        public string add { get; set; }
        /// <summary>
        /// Address: Department Floor
        /// </summary>
        public string adf { get; set; }
        /// <summary>
        /// Address: Department Letter or Number
        /// </summary>
        public string adl { get; set; }
        /// <summary>
        /// Address: Department Stair
        /// </summary>
        public object[] ads { get; set; }
        /// <summary>
        /// Alternative Mobile Phone
        /// </summary>
        public object[] amp { get; set; }
        /// <summary>
        /// Address: Postal Code
        /// </summary>
        public string apc { get; set; }
        /// <summary>
        /// Address: Province
        /// </summary>
        public string apr { get; set; }
        /// <summary>
        /// Address: Street Name
        /// </summary>
        public string asn { get; set; }
        /// <summary>
        /// ??? - Cloud token
        /// </summary>
        public string cid { get; set; }
        /// <summary>
        /// Contract ID
        /// </summary>
        public string contid { get; set; }
        /// <summary>
        /// E-Mail
        /// </summary>
        public string em { get; set; }
        /// <summary>
        /// First Surname
        /// </summary>
        public string fs { get; set; }
        /// <summary>
        /// Main Mobile Phone
        /// </summary>
        public string mmp { get; set; }
        /// <summary>
        /// Mobile user Id (authorization token)
        /// </summary>
        public string mui { get; set; }
        /// <summary>
        /// Names
        /// </summary>
        public string na { get; set; }
        /// <summary>
        /// NIF, NIE or CIF
        /// </summary>
        public string nif { get; set; }
        /// <summary>
        /// Notifications
        /// </summary>
        public Notifications notifications { get; set; }
        /// <summary>
        /// Operative System (1: Android, 2: iOS)
        /// </summary>
        public string os { get; set; }
        /// <summary>
        /// Plates
        /// </summary>
        public Plate[] plates { get; set; }
        /// <summary>
        /// Second Surname
        /// </summary>
        public string ss { get; set; }
        /// <summary>
        /// Username
        /// </summary>
        public string un { get; set; }
        /// <summary>
        /// App version
        /// </summary>
        public string v { get; set; }
        /// <summary>
        /// Validate new conditions is necessary
        /// </summary>
        public string val { get; set; }
    }

    public class Notifications
    {
        /// <summary>
        /// low balance notification
        /// </summary>
        public string ba { get; set; }
        /// <summary>
        /// Fine notifications? (1:true, 0:false)
        /// </summary>
        public string fn { get; set; }
        /// <summary>
        /// low balance amount
        /// </summary>
        public string q_ba { get; set; }
        /// <summary>
        /// recharge notifications? (1:true, 0:false)
        /// </summary>
        public string re { get; set; }
        /// <summary>
        /// minutes before the limit (unparking notifications)
        /// </summary>
        public string t_unp { get; set; }
        /// <summary>
        /// UnParking notifications? (1:true, 0:false)
        /// </summary>
        public string unp { get; set; }
    }

    public class Plate
    {
        /// <summary>
        /// Plate
        /// </summary>
        public string p { get; set; }
        /// <summary>
        /// status (1:Rotative, 2:Resident, 3:VIP
        /// </summary>
        public string stp { get; set; }
    }
}
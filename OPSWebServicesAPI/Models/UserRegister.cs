using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    public class UserRegister
    {
        /// <summary>
        /// Address: Building Number
        /// </summary>
        [DisplayName("abn")]
        public string addressBuildingNumber { get; set; }
        /// <summary>
        /// Address: City
        /// </summary>
        [DisplayName("aci")]
        public string addressCity { get; set; }
        /// <summary>
        /// Address: Department Door
        /// </summary>
        [DisplayName("add")]
        public string addressDepartmentDoor { get; set; }
        /// <summary>
        /// Address: Department Floor
        /// </summary>
        [DisplayName("adf")]
        public string addressDepartmentFloor { get; set; }
        /// <summary>
        /// Address: Department Letter or Number
        /// </summary>
        [DisplayName("adl")]
        public string addressLetterNumber { get; set; }
        /// <summary>
        /// Address: Department Stair
        /// </summary>
        [DisplayName("ads")]
        public object[] addressDepartmentStair { get; set; }
        /// <summary>
        /// Alternative Mobile Phone
        /// </summary>
        [DisplayName("amp")]
        public object[] addressMobilePhone { get; set; }
        /// <summary>
        /// Address: Postal Code
        /// </summary>
        [DisplayName("apc")]
        public string addressPostalCode { get; set; }
        /// <summary>
        /// Address: Province
        /// </summary>
        [DisplayName("apr")]
        public string addressProvince { get; set; }
        /// <summary>
        /// Address: Street Name
        /// </summary>
        [DisplayName("asn")]
        public string addressStreetName { get; set; }
        /// <summary>
        /// (*) Username
        /// </summary>
        [DisplayName("un")]
        public string userName { get; set; }
        /// <summary>
        /// (*) password
        /// </summary>
        [DisplayName("pw")]
        public string password { get; set; }
        /// <summary>
        /// (*) First Surname
        /// </summary>
        [DisplayName("fs")]
        public string firstSurname { get; set; }
        /// <summary>
        /// Second Surname
        /// </summary>
        [DisplayName("ss")]
        public string secondSurname { get; set; }
        /// <summary>
        /// (*) Contract ID
        /// </summary>
        [DisplayName("contid")]
        public string contractId { get; set; }
        /// <summary>
        /// (*) E-Mail
        /// </summary>
        [DisplayName("em")]
        public string email { get; set; }
        /// <summary>
        /// (*) Main Mobile Phone
        /// </summary>
        [DisplayName("mmp")]
        public string mainMobilePhone { get; set; }
        /// <summary>
        /// (*) Names
        /// </summary>
        [DisplayName("na")]
        public string names { get; set; }
        /// <summary>
        /// NIF, NIE or CIF
        /// </summary>
        [DisplayName("nif")]
        public string nif { get; set; }
        /// <summary>
        /// (*) Notifications
        /// </summary>
        [DisplayName("notifications")]
        public Notification notifications { get; set; }
        /// <summary>
        /// (*) Plates
        /// </summary>
        [DisplayName("plates")]
        public Plate[] plates { get; set; }
        /// <summary>
        /// Mobile user founds
        /// </summary>
        [DisplayName("userCredit")]
        public string userCredit { get; set; }
    }
}
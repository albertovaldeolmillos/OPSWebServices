using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// Class to request update user
    /// </summary>
    public class User
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
        public string addressDepartmentStair { get; set; }
        /// <summary>
        /// Alternative Mobile Phone
        /// </summary>
        [DisplayName("amp")]
        public string alternativeMobilePhone { get; set; }
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
        /// Cloud token
        /// </summary>
        [DisplayName("cid")]
        public string cloudToken { get; set; }
        /// <summary>
        /// Contract ID
        /// </summary>
        [DisplayName("contid")]
        public string contractId { get; set; }
        /// <summary>
        /// (*) E-Mail
        /// </summary>
        [DisplayName("em")]
        [Required]
        public string email { get; set; }
        /// <summary>
        /// (*) First Surname
        /// </summary>
        [DisplayName("fs")]
        [Required]
        public string firstSurname { get; set; }
        /// <summary>
        /// (*) Main Mobile Phone
        /// </summary>
        [DisplayName("mmp")]
        [Required]
        public string mainMobilePhone { get; set; }
        /// <summary>
        /// (*) Mobile user Id (authorization token)
        /// </summary>
        [DisplayName("mui")]
        [Required]
        public string authorizationToken { get; set; }
        /// <summary>
        /// (*) Names
        /// </summary>
        [DisplayName("na")]
        [Required]
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
        [Required]
        public Notification notifications { get; set; }
        /// <summary>
        /// Operative System (1: Android, 2: iOS)
        /// </summary>
        [DisplayName("os")]
        public string operatingSystem { get; set; }
        /// <summary>
        /// (*) Plates
        /// </summary>
        [DisplayName("plates")]
        [Required]
        public Plate[] plates { get; set; }
        /// <summary>
        /// Second Surname
        /// </summary>
        [DisplayName("ss")]
        public string secondSurname { get; set; }
        /// <summary>
        /// Username
        /// </summary>
        [DisplayName("un")]
        public string userName { get; set; }
        /// <summary>
        /// App version
        /// </summary>
        [DisplayName("v")]
        public string version { get; set; }
        /// <summary>
        /// (*) Validate new conditions is necessary
        /// </summary>
        [DisplayName("val")]
        public string validateConditions { get; set; }
    }

}
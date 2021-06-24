using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    public class Operation
    {
        /// <summary>
        /// Score needed to apply the new bonus
        /// </summary>
        [DisplayName("bns")]
        public string bonus { get; set; }
        /// <summary>
        /// Contract id
        /// </summary>
        [DisplayName("contid")]
        public string contractId { get; set; }
        /// <summary>
        /// Contract name
        /// </summary>
        [DisplayName("contname")]
        public string contractName { get; set; }
        /// <summary>
        /// Operation number
        /// </summary>
        [DisplayName("on")]
        public string operationNumber { get; set; }
        /// <summary>
        /// Operation type (1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing, 104: Unpaid fines)
        /// </summary>
        [DisplayName("ot")]
        public string operationType { get; set; }
        /// <summary>
        /// Payment amount (Expressed in Euro cents)
        /// </summary>
        [DisplayName("pa")]
        public string paymentAmount { get; set; }
        /// <summary>
        /// Payment method (1: Chip-Card, 2: Credit Card, 3: Cash, 4: Web, 5: Phone)
        /// </summary>
        [DisplayName("pm")]
        public string paymentMethod { get; set; }
        /// <summary>
        /// Post-Paid (0: False, 1: True)
        /// </summary>
        [DisplayName("pp")]
        public string postPaid { get; set; }
        /// <summary>
        /// Recharge date (Format: hh24missddMMYY)
        /// </summary>
        [DisplayName("rd")]
        public string rechargeDate { get; set; }
        /// <summary>
        /// Zone
        /// </summary>
        [DisplayName("zo")]
        public string zone { get; set; }
        /// <summary>
        /// Zone color
        /// </summary>
        [DisplayName("zonecolor")]
        public string zonecolor { get; set; }
        /// <summary>
        /// Zone name
        /// </summary>
        [DisplayName("zonename")]
        public string zonename { get; set; }
        /// <summary>
        /// Fine article
        /// </summary>
        [DisplayName("farticle")]
        public string farticle { get; set; }
        /// <summary>
        /// Car color
        /// </summary>
        [DisplayName("fcolor")]
        public string fcolor { get; set; }
        /// <summary>
        /// Car make
        /// </summary>
        [DisplayName("fmake")]
        public string fmake { get; set; }
        /// <summary>
        /// Fine number
        /// </summary>
        [DisplayName("fn")]
        public string fineNumber { get; set; }
        /// <summary>
        /// Fine processing date (Format: hh24missddMMYY)
        /// </summary>
        [DisplayName("fpd")]
        public string fineProcessingDate { get; set; }
        /// <summary>
        /// Fine status (1: Payable, 2:Expired, 3:Not payable)
        /// </summary>
        [DisplayName("fs")]
        public string fineStatus { get; set; }
        /// <summary>
        /// Fine street
        /// </summary>
        [DisplayName("fstreet")]
        public string fstreet { get; set; }
        /// <summary>
        /// Fine street number
        /// </summary>
        [DisplayName("fstrnum")]
        public string fstrnum { get; set; }
        /// <summary>
        /// Plate
        /// </summary>
        [DisplayName("pl")]
        public string plate { get; set; }
        /// <summary>
        /// Parking end date (Format: hh24missddMMYY)
        /// </summary>
        [DisplayName("ed")]
        public string parkingEndDate { get; set; }
        /// <summary>
        /// Parking start date (Format: hh24missddMMYY)
        /// </summary>
        [DisplayName("sd")]
        public string parkingStartDate { get; set; }
        /// <summary>
        /// status: 1 (UNPARKED), 2 (PARKED)
        /// </summary>
        [DisplayName("sta")]
        public string status { get; set; }

    }
 
}
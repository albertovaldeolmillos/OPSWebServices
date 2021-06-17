using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    public class UserOperationResponse
    {
        /// <summary>
        /// operation list
        /// </summary>
        public O[] o { get; set; }
        /// <summary>
        /// IsSuccess
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// ErrorMessage
        /// </summary>
        public string ErrorMessage { get; set; }
    }
    /// <summary>
    /// Operation
    /// </summary>
    public class O
    {
        /// <summary>
        /// ???
        /// </summary>
        public string bns { get; set; }
        /// <summary>
        /// Contract id
        /// </summary>
        public string contid { get; set; }
        /// <summary>
        /// Contract name
        /// </summary>
        public string contname { get; set; }
        /// <summary>
        /// Operation number
        /// </summary>
        public string on { get; set; }
        /// <summary>
        /// Operation type (1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing, 104: Unpaid fines)
        /// </summary>
        public string ot { get; set; }
        /// <summary>
        /// Payment amount (Expressed in Euro cents)
        /// </summary>
        public string pa { get; set; }
        /// <summary>
        /// Payment method (1: Chip-Card, 2: Credit Card, 3: Cash, 4: Web, 5: Phone)
        /// </summary>
        public string pm { get; set; }
        /// <summary>
        /// Post-Paid (0: False, 1: True)
        /// </summary>
        public string pp { get; set; }
        /// <summary>
        /// Recharge date (Format: hh24missddMMYY)
        /// </summary>
        public string rd { get; set; }
        /// <summary>
        /// Zone
        /// </summary>
        public string zo { get; set; }
        /// <summary>
        /// Zone color
        /// </summary>
        public string zonecolor { get; set; }
        /// <summary>
        /// Zone name
        /// </summary>
        public string zonename { get; set; }
        /// <summary>
        /// Fine article
        /// </summary>
        public string farticle { get; set; }
        /// <summary>
        /// Car color
        /// </summary>
        public string fcolor { get; set; }
        /// <summary>
        /// Car make
        /// </summary>
        public string fmake { get; set; }
        /// <summary>
        /// Fine number
        /// </summary>
        public string fn { get; set; }
        /// <summary>
        /// Fine processing date (Format: hh24missddMMYY)
        /// </summary>
        public string fpd { get; set; }
        /// <summary>
        /// Fine status (1: Payable, 2:Expired, 3:Not payable)
        /// </summary>
        public string fs { get; set; }
        /// <summary>
        /// Fine street
        /// </summary>
        public string fstreet { get; set; }
        /// <summary>
        /// Fine street number
        /// </summary>
        public string fstrnum { get; set; }
        /// <summary>
        /// Plate
        /// </summary>
        public string pl { get; set; }
        /// <summary>
        /// Parking end date (Format: hh24missddMMYY)
        /// </summary>
        public string ed { get; set; }
        /// <summary>
        /// Parking start date (Format: hh24missddMMYY)
        /// </summary>
        public string sd { get; set; }
        /// <summary>
        /// status: 1 (UNPARKED), 2 (PARKED)
        /// </summary>
        public string sta { get; set; }
    }
}
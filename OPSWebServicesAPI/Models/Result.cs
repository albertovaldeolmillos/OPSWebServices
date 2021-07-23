using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// generic response
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Result
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// Is Success?
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// Error Message
        /// </summary>
        public Error Error { get; set; }
    }

    public class ResultParkingStepsInfo : Result
    {
        public ParkingStepsInfo Value { get; set; }
    }
}
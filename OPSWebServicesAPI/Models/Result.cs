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
        /// Is Success?
        /// </summary>
        public bool isSuccess { get; set; }
        /// <summary>
        /// Error Message
        /// </summary>
        public Error error { get; set; }
    }

    public class ResultParkingStepsInfo : Result
    {
        public ParkingStepsInfo value { get; set; }
    }

    public class ResultParkingTimeInfo : Result
    {
        public ParkingTimeInfo value { get; set; }
    }

    public class ResultParkingMoneyInfo : Result
    {
        public ParkingMoneyInfo value { get; set; }
    }

    public class ResultPlaceInfo : Result
    {
        public PlaceInfo value { get; set; }
    }

    public class ResultStreetsInfo : Result
    {
        public StreetsInfo value { get; set; }
    }

    public class ResultZoneInfo : Result
    {
        public ZoneInfo value { get; set; }
    }

    public class ResultContractsInfo : Result
    {
        public ContractsInfo value { get; set; }
    }

    public class ResultLoginInfo : Result
    {
        public string value { get; set; }
    }

    public class ResultUpdateUserInfo : Result
    {
        public string value { get; set; }
    }

    public class ResultListOperationInfo : Result
    {
        public Operation[] value { get; set; }
    }

    public class ResultUserInfo : Result
    {
        public User value { get; set; }
    }

    public class ResultRecoverPasswordInfo : Result
    {
        public string value { get; set; }
    }

    public class ResultVerifyRecoverPasswordInfo : Result
    {
        public string value { get; set; }
    }

    public class ResultChangePasswordInfo : Result
    {
        public string value { get; set; }
    }

    public class ResultRegisterUserInfo : Result
    {
        public string value { get; set; }
    }
}
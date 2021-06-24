using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{


    public class ErrorText
    {
        public enum ResultType
        {
            Result_OK = 1,
            Result_Error = 0,
            Result_Error_InvalidAuthenticationHash = -1,
            Result_Error_MaxTimeAlreadyUsedInPark = -2,
            Result_Error_ReentryTimeError = -3,
            Result_Error_Plate_Has_No_Return = -4,
            Result_Error_FineNumberNotFound = -5,
            Result_Error_FineNumberFoundButNotPayable = -6,
            Result_Error_FineNumberFoundButTimeExpired = -7,
            Result_Error_FineNumberAlreadyPayed = -8,
            Result_Error_Generic = -9,
            Result_Error_Invalid_Input_Parameter = -10,
            Result_Error_Missing_Input_Parameter = -11,
            Result_Error_OPS_Error = -12,
            Result_Error_Operation_Already_Inserted = -13,
            Result_Error_Quantity_To_Pay_Different_As_Calculated = -14,
            Result_Error_Mobile_User_Not_Found = -20,
            Result_Error_Mobile_User_Already_Registered = -21,
            Result_Error_Mobile_User_Email_Already_Registered = -22,
            Result_Error_Invalid_Login = -23,
            Result_Error_ParkingStartedByDifferentUser = -24,
            Result_Error_Not_Enough_Credit = -25,
            Result_Error_Cloud_Id_Not_Found = -26,
            Result_Error_App_Update_Required = -27,
            Result_Error_No_Return_For_Minimum = -28,
            Result_Error_User_Not_Validated = -29,
            Result_Error_Service_Expired = -30,
            Result_Error_Recovery_Code_Not_Found = -31,
            Result_Error_Recovery_Code_Invalid = -32,
            Result_Error_Recovery_Code_Expired = -33
        }

        public enum SeverityError
        {
            Warning = 1, //aviso a usuario
            Exception = 2, //error no controlado
            Critical = 3, //error de lógica
            Low = 4 //informativo (para logs)
        }

        public Dictionary<int, string> ErrorTextEN = new Dictionary<int, string>()
            {
                { (int)ResultType.Result_OK, "OK" },
                { (int)ResultType.Result_Error, "Undefined" },
                { (int)ResultType.Result_Error_InvalidAuthenticationHash, "InvalidAuthenticationHash" },
                { (int)ResultType.Result_Error_MaxTimeAlreadyUsedInPark, "MaxTimeAlreadyUsedInPark" },
                { (int)ResultType.Result_Error_ReentryTimeError, "ReentryTimeError" },
                { (int)ResultType.Result_Error_Plate_Has_No_Return, "Plate_Has_No_Return" },
                { (int)ResultType.Result_Error_FineNumberNotFound, "FineNumberNotFound" },
                { (int)ResultType.Result_Error_FineNumberFoundButNotPayable, "FineNumberFoundButNotPayable" },
                { (int)ResultType.Result_Error_FineNumberFoundButTimeExpired, "FineNumberFoundButTimeExpired" },
                { (int)ResultType.Result_Error_FineNumberAlreadyPayed, "FineNumberAlreadyPayed" },
                { (int)ResultType.Result_Error_Generic, "Generic" },
                { (int)ResultType.Result_Error_Invalid_Input_Parameter, "Invalid_Input_Parameter" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter, "Missing_Input_Parameter" },
                { (int)ResultType.Result_Error_OPS_Error, "OPS_Error" },
                { (int)ResultType.Result_Error_Operation_Already_Inserted, "Operation_Already_Inserted" },
                { (int)ResultType.Result_Error_Quantity_To_Pay_Different_As_Calculated, "Quantity_To_Pay_Different_As_Calculated" },
                { (int)ResultType.Result_Error_Mobile_User_Not_Found, "Mobile_User_Not_Found" },
                { (int)ResultType.Result_Error_Mobile_User_Already_Registered, "Mobile_User_Already_Registered" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Already_Registered, "Mobile_User_Email_Already_Registered" },
                { (int)ResultType.Result_Error_Invalid_Login, "Invalid_Login" },
                { (int)ResultType.Result_Error_ParkingStartedByDifferentUser, "ParkingStartedByDifferentUser" },
                { (int)ResultType.Result_Error_Not_Enough_Credit, "Not_Enough_Credit" },
                { (int)ResultType.Result_Error_Cloud_Id_Not_Found, "Cloud_Id_Not_Found" },
                { (int)ResultType.Result_Error_App_Update_Required, "App_Update_Required" },
                { (int)ResultType.Result_Error_No_Return_For_Minimum, "No_Return_For_Minimum" },
                { (int)ResultType.Result_Error_User_Not_Validated, "User_Not_Validated" },
                { (int)ResultType.Result_Error_Service_Expired, "Service_Expired" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Recovery_Code_Not_Found" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Recovery_Code_Invalid" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Recovery_Code_Expired" }
            };

        public Dictionary<int, string> ErrorTextES = new Dictionary<int, string>()
            {
                { (int)ResultType.Result_OK, "OK" },
                { (int)ResultType.Result_Error, "Undefined" },
                { (int)ResultType.Result_Error_InvalidAuthenticationHash, "InvalidAuthenticationHash" },
                { (int)ResultType.Result_Error_MaxTimeAlreadyUsedInPark, "MaxTimeAlreadyUsedInPark" },
                { (int)ResultType.Result_Error_ReentryTimeError, "ReentryTimeError" },
                { (int)ResultType.Result_Error_Plate_Has_No_Return, "Plate_Has_No_Return" },
                { (int)ResultType.Result_Error_FineNumberNotFound, "FineNumberNotFound" },
                { (int)ResultType.Result_Error_FineNumberFoundButNotPayable, "FineNumberFoundButNotPayable" },
                { (int)ResultType.Result_Error_FineNumberFoundButTimeExpired, "FineNumberFoundButTimeExpired" },
                { (int)ResultType.Result_Error_FineNumberAlreadyPayed, "FineNumberAlreadyPayed" },
                { (int)ResultType.Result_Error_Generic, "Generic" },
                { (int)ResultType.Result_Error_Invalid_Input_Parameter, "Invalid_Input_Parameter" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter, "Missing_Input_Parameter" },
                { (int)ResultType.Result_Error_OPS_Error, "OPS_Error" },
                { (int)ResultType.Result_Error_Operation_Already_Inserted, "Operation_Already_Inserted" },
                { (int)ResultType.Result_Error_Quantity_To_Pay_Different_As_Calculated, "Quantity_To_Pay_Different_As_Calculated" },
                { (int)ResultType.Result_Error_Mobile_User_Not_Found, "Mobile_User_Not_Found" },
                { (int)ResultType.Result_Error_Mobile_User_Already_Registered, "Mobile_User_Already_Registered" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Already_Registered, "Mobile_User_Email_Already_Registered" },
                { (int)ResultType.Result_Error_Invalid_Login, "Invalid_Login" },
                { (int)ResultType.Result_Error_ParkingStartedByDifferentUser, "ParkingStartedByDifferentUser" },
                { (int)ResultType.Result_Error_Not_Enough_Credit, "Not_Enough_Credit" },
                { (int)ResultType.Result_Error_Cloud_Id_Not_Found, "Cloud_Id_Not_Found" },
                { (int)ResultType.Result_Error_App_Update_Required, "App_Update_Required" },
                { (int)ResultType.Result_Error_No_Return_For_Minimum, "No_Return_For_Minimum" },
                { (int)ResultType.Result_Error_User_Not_Validated, "User_Not_Validated" },
                { (int)ResultType.Result_Error_Service_Expired, "Service_Expired" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Recovery_Code_Not_Found" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Recovery_Code_Invalid" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Recovery_Code_Expired" }
            };

        public Dictionary<int, string> ErrorTextEU = new Dictionary<int, string>()
            {
                { (int)ResultType.Result_OK, "OK" },
                { (int)ResultType.Result_Error, "Undefined" },
                { (int)ResultType.Result_Error_InvalidAuthenticationHash, "InvalidAuthenticationHash" },
                { (int)ResultType.Result_Error_MaxTimeAlreadyUsedInPark, "MaxTimeAlreadyUsedInPark" },
                { (int)ResultType.Result_Error_ReentryTimeError, "ReentryTimeError" },
                { (int)ResultType.Result_Error_Plate_Has_No_Return, "Plate_Has_No_Return" },
                { (int)ResultType.Result_Error_FineNumberNotFound, "FineNumberNotFound" },
                { (int)ResultType.Result_Error_FineNumberFoundButNotPayable, "FineNumberFoundButNotPayable" },
                { (int)ResultType.Result_Error_FineNumberFoundButTimeExpired, "FineNumberFoundButTimeExpired" },
                { (int)ResultType.Result_Error_FineNumberAlreadyPayed, "FineNumberAlreadyPayed" },
                { (int)ResultType.Result_Error_Generic, "Generic" },
                { (int)ResultType.Result_Error_Invalid_Input_Parameter, "Invalid_Input_Parameter" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter, "Missing_Input_Parameter" },
                { (int)ResultType.Result_Error_OPS_Error, "OPS_Error" },
                { (int)ResultType.Result_Error_Operation_Already_Inserted, "Operation_Already_Inserted" },
                { (int)ResultType.Result_Error_Quantity_To_Pay_Different_As_Calculated, "Quantity_To_Pay_Different_As_Calculated" },
                { (int)ResultType.Result_Error_Mobile_User_Not_Found, "Mobile_User_Not_Found" },
                { (int)ResultType.Result_Error_Mobile_User_Already_Registered, "Mobile_User_Already_Registered" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Already_Registered, "Mobile_User_Email_Already_Registered" },
                { (int)ResultType.Result_Error_Invalid_Login, "Invalid_Login" },
                { (int)ResultType.Result_Error_ParkingStartedByDifferentUser, "ParkingStartedByDifferentUser" },
                { (int)ResultType.Result_Error_Not_Enough_Credit, "Not_Enough_Credit" },
                { (int)ResultType.Result_Error_Cloud_Id_Not_Found, "Cloud_Id_Not_Found" },
                { (int)ResultType.Result_Error_App_Update_Required, "App_Update_Required" },
                { (int)ResultType.Result_Error_No_Return_For_Minimum, "No_Return_For_Minimum" },
                { (int)ResultType.Result_Error_User_Not_Validated, "User_Not_Validated" },
                { (int)ResultType.Result_Error_Service_Expired, "Service_Expired" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Recovery_Code_Not_Found" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Recovery_Code_Invalid" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Recovery_Code_Expired" }
            };

        public Dictionary<int, string> ErrorTextFR = new Dictionary<int, string>()
            {
                { (int)ResultType.Result_OK, "OK" },
                { (int)ResultType.Result_Error, "Undefined" },
                { (int)ResultType.Result_Error_InvalidAuthenticationHash, "InvalidAuthenticationHash" },
                { (int)ResultType.Result_Error_MaxTimeAlreadyUsedInPark, "MaxTimeAlreadyUsedInPark" },
                { (int)ResultType.Result_Error_ReentryTimeError, "ReentryTimeError" },
                { (int)ResultType.Result_Error_Plate_Has_No_Return, "Plate_Has_No_Return" },
                { (int)ResultType.Result_Error_FineNumberNotFound, "FineNumberNotFound" },
                { (int)ResultType.Result_Error_FineNumberFoundButNotPayable, "FineNumberFoundButNotPayable" },
                { (int)ResultType.Result_Error_FineNumberFoundButTimeExpired, "FineNumberFoundButTimeExpired" },
                { (int)ResultType.Result_Error_FineNumberAlreadyPayed, "FineNumberAlreadyPayed" },
                { (int)ResultType.Result_Error_Generic, "Generic" },
                { (int)ResultType.Result_Error_Invalid_Input_Parameter, "Invalid_Input_Parameter" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter, "Missing_Input_Parameter" },
                { (int)ResultType.Result_Error_OPS_Error, "OPS_Error" },
                { (int)ResultType.Result_Error_Operation_Already_Inserted, "Operation_Already_Inserted" },
                { (int)ResultType.Result_Error_Quantity_To_Pay_Different_As_Calculated, "Quantity_To_Pay_Different_As_Calculated" },
                { (int)ResultType.Result_Error_Mobile_User_Not_Found, "Mobile_User_Not_Found" },
                { (int)ResultType.Result_Error_Mobile_User_Already_Registered, "Mobile_User_Already_Registered" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Already_Registered, "Mobile_User_Email_Already_Registered" },
                { (int)ResultType.Result_Error_Invalid_Login, "Invalid_Login" },
                { (int)ResultType.Result_Error_ParkingStartedByDifferentUser, "ParkingStartedByDifferentUser" },
                { (int)ResultType.Result_Error_Not_Enough_Credit, "Not_Enough_Credit" },
                { (int)ResultType.Result_Error_Cloud_Id_Not_Found, "Cloud_Id_Not_Found" },
                { (int)ResultType.Result_Error_App_Update_Required, "App_Update_Required" },
                { (int)ResultType.Result_Error_No_Return_For_Minimum, "No_Return_For_Minimum" },
                { (int)ResultType.Result_Error_User_Not_Validated, "User_Not_Validated" },
                { (int)ResultType.Result_Error_Service_Expired, "Service_Expired" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Recovery_Code_Not_Found" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Recovery_Code_Invalid" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Recovery_Code_Expired" }
            };
    }

    public class Error
    {


        public Error(int code, int type)
        {
            Code = code;
            Type = type;
            ErrorText errortext = new ErrorText();
            Message_EN = errortext.ErrorTextEN[code];
            Message_ES = errortext.ErrorTextES[code];
            Message_EU = errortext.ErrorTextEU[code];
            Message_FR = errortext.ErrorTextFR[code];
        }
        /// <summary>
        /// codigo de error
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// tipo de error
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// Mensaje de error en inglés
        /// </summary>
        public string Message_EN { get; set; }
        /// <summary>
        /// Mensaje de error en castellano
        /// </summary>
        public string Message_ES { get; set; }
        /// <summary>
        /// Mensaje de error en euskera
        /// </summary>
        public string Message_EU { get; set; }
        /// <summary>
        /// Mensaje de error en francés
        /// </summary>
        public string Message_FR { get; set; }
    }

}
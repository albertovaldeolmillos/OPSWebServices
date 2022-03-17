using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OPSWebServicesAPI.Models
{

    /// <summary>
    /// Clase auxiliar para la definición de los errores en 4 idiomas
    /// </summary>
    public class ErrorText
    {
        /// <summary>
        /// Tipos de errores
        /// </summary>
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
            Result_Error_Location_Not_Found = -30,
            Result_Error_Recovery_Code_Not_Found = -31,
            Result_Error_Recovery_Code_Invalid = -32,
            Result_Error_Recovery_Code_Expired = -33,
            //NUEVOS
            Result_Error_Invalid_Login_NotBeforeFailed = -230,
            Result_Error_Invalid_Login_TokenExpired = -231,
            Result_Error_Invalid_Login_TokenNotCorrectlyFormed = -232,
            Result_Error_Invalid_Login_SignatureNotValid = -233,
            Result_Error_Invalid_Login_OnTokenValidateFailed = -234,
            Result_Error_Invalid_Login_OnJtiValidateFailed = -235,
            Result_Error_Invalid_Login_CustomCheckFailed = -236,
            Result_Error_Invalid_Login_CreatedTimeCheckFailed = -237,
            Result_Error_Street_Or_Sector_Not_Found = -250,
            Result_Error_No_Bearer_Token = -290,
            Result_Error_Missing_Input_Parameter_AuthorizationToken = -100,
            Result_Error_Missing_Input_Parameter_CloudToken = -101,
            Result_Error_Missing_Input_Parameter_ContractId = -102,
            Result_Error_Missing_Input_Parameter_UserName = -103,
            Result_Error_Missing_Input_Parameter_Password = -104,
            Result_Error_Missing_Input_Parameter_OperativeSystem = -105,
            Result_Error_Missing_Input_Parameter_Version = -106,
            Result_Error_Missing_Input_Parameter_ReportFormat = -107,
            Result_Error_Missing_Input_Parameter_Email = -108,
            Result_Error_Missing_Input_Parameter_Recode = -109,
            Result_Error_Missing_Input_Parameter_AmountInCents = -110,
            Result_Error_Missing_Input_Parameter_Date = -111,
            Result_Error_Missing_Input_Parameter_DateStart = -112,
            Result_Error_Missing_Input_Parameter_DateEnd = -113,
            Result_Error_Missing_Input_Parameter_Latitude = -114,
            Result_Error_Missing_Input_Parameter_Longitude = -115,
            Result_Error_Missing_Input_Parameter_StreetName = -116,
            Result_Error_Missing_Input_Parameter_StreetNumber = -117,
            Result_Error_Missing_Input_Parameter_Plate = -118,
            Result_Error_Missing_Input_Parameter_Group = -119,
            Result_Error_Missing_Input_Parameter_TimeInMinutes = -120,
            Result_Error_Missing_Input_Parameter_QuantityInCents = -121,
            Result_Error_Missing_Input_Parameter_TariffType = -122,
            Result_Error_Missing_Input_Parameter_Fine = -123,
            Result_Error_Missing_Input_Parameter_Name = -124,
            Result_Error_Missing_Input_Parameter_FirstSurname = -125,
            Result_Error_Missing_Input_Parameter_MainMobilePhone = -126,
            Result_Error_Missing_Input_Parameter_ValidateConditions = -127,
            Result_Error_Missing_Input_Parameter_No_Plates = -128,
            Result_Error_Missing_Input_Parameter_UserName_Email = -129,
            Result_Error_Missing_Input_Parameter_AuthorizationToken_UserNme_Password = -130,
            Result_Error_Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude = -131,
            Result_Error_Parking_Not_Allowed = -300,
            Result_Error_Parking_Not_Allowed_Resident_Zone_24h = -301,
            Result_Error_Parking_Not_Allowed_1_June_30_September = -302,
            Result_Error_Parking_Not_Allowed_1_June_30_September_And_May_Weekends = -303,
            Result_Error_Parking_Not_Allowed_1_June_15_September = -304,
            Result_Error_Parking_Not_Allowed_Outside_Working_Hours = -305
        }

        /// <summary>
        /// Severidad del error
        /// </summary>
        public enum SeverityError
        {
            Warning = 1, //aviso a usuario
            Exception = 2, //error no controlado
            Critical = 3, //error de lógica
            Low = 4 //informativo (para logs)
        }

        /// <summary>
        /// Asociación del identificador del error con su mensaje en inglés
        /// </summary>
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
                { (int)ResultType.Result_Error_Location_Not_Found, "Location_Not_Found" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Recovery_Code_Not_Found" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Recovery_Code_Invalid" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Recovery_Code_Expired" },
                { (int)ResultType.Result_Error_Invalid_Login_NotBeforeFailed, "Invalid_Login_NotBeforeFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenExpired, "Invalid_Login_TokenExpired" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenNotCorrectlyFormed, "Invalid_Login_TokenNotCorrectlyFormed" },
                { (int)ResultType.Result_Error_Invalid_Login_SignatureNotValid, "Invalid_Login_SignatureNotValid" },
                { (int)ResultType.Result_Error_Invalid_Login_OnTokenValidateFailed, "Invalid_Login_OnTokenValidateFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_OnJtiValidateFailed, "Invalid_Login_OnJtiValidateFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_CustomCheckFailed, "Invalid_Login_CustomCheckFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_CreatedTimeCheckFailed, "Invalid_Login_CreatedTimeCheckFailed" },
                { (int)ResultType.Result_Error_Street_Or_Sector_Not_Found, "Street_Or_Sector_Not_Found" },
                { (int)ResultType.Result_Error_No_Bearer_Token, "No_Bearer_Token" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, "Missing__Input_Parameter_AuthorizationToken"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_CloudToken, "Missing__Input_Parameter_CloudToken"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, "Missing__Input_Parameter_ContractId"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName, "Missing__Input_Parameter_UserName"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Password, "Missing__Input_Parameter_Password"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_OperativeSystem, "Missing__Input_Parameter_OperativeSystem"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Version, "Missing__Input_Parameter_Version"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ReportFormat, "Missing_Input_Parameter_ReportFormat"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Email, "Missing_Input_Parameter_Email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Recode, "Missing_Input_Parameter_Recode"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AmountInCents, "Missing_Input_Parameter_AmountInCents"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Date, "Missing_Input_Parameter_Date"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateStart, "Missing_Input_Parameter_DateStart"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateEnd, "Missing_Input_Parameter_DateEnd"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Latitude, "Missing_Input_Parameter_Latitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Longitude, "Missing_Input_Parameter_Longitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName, "Missing_Input_Parameter_StreetName"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetNumber, "Missing_Input_Parameter_StreetNumber"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Plate, "Missing_Input_Parameter_Plate"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Group, "Missing_Input_Parameter_Group"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TimeInMinutes, "Missing_Input_Parameter_TimeInMinutes"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_QuantityInCents, "Missing_Input_Parameter_QuantityInCents"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TariffType, "Missing_Input_Parameter_TariffType"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Fine, "Missing_Input_Parameter_Fine"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Name, "Missing_Input_Parameter_Name"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_FirstSurname, "Missing_Input_Parameter_FirstSurname"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_MainMobilePhone, "Missing_Input_Parameter_MainMobilePhone"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ValidateConditions, "Missing_Input_Parameter_ValidateConditions"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_No_Plates, "Missing_Input_Parameter_No_Plates"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName_Email, "Missing_Input_Parameter_UserName_Email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken_UserNme_Password, "Missing_Input_Parameter_AuthorizationToken_UserNme_Password"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude, "Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed, "Parking_Not_Allowed"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Resident_Zone_24h, "Parking_Not_Allowed_Resident_Zone_24h"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September, "Parking_Not_Allowed_1_June_30_September"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September_And_May_Weekends, "Parking_Not_Allowed_1_June_30_September_And_May_Weekends"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_15_September, "Parking_Not_Allowed_1_June_15_September"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Outside_Working_Hours, "Parking_Not_Allowed_Outside_Working_Hours"}
            };

        /// <summary>
        /// Asociación del identificador del error con su mensaje en castellano
        /// </summary>
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
                { (int)ResultType.Result_Error_Location_Not_Found, "Location_Not_Found" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Recovery_Code_Not_Found" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Recovery_Code_Invalid" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Recovery_Code_Expired" },
                { (int)ResultType.Result_Error_Invalid_Login_NotBeforeFailed, "Invalid_Login_NotBeforeFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenExpired, "Invalid_Login_TokenExpired" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenNotCorrectlyFormed, "Invalid_Login_TokenNotCorrectlyFormed" },
                { (int)ResultType.Result_Error_Invalid_Login_SignatureNotValid, "Invalid_Login_SignatureNotValid" },
                { (int)ResultType.Result_Error_Invalid_Login_OnTokenValidateFailed, "Invalid_Login_OnTokenValidateFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_OnJtiValidateFailed, "Invalid_Login_OnJtiValidateFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_CustomCheckFailed, "Invalid_Login_CustomCheckFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_CreatedTimeCheckFailed, "Invalid_Login_CreatedTimeCheckFailed" },
                { (int)ResultType.Result_Error_Street_Or_Sector_Not_Found, "Street_Or_Sector_Not_Found" },
                { (int)ResultType.Result_Error_No_Bearer_Token, "No_Bearer_Token" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, "Missing__Input_Parameter_AuthorizationToken"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_CloudToken, "Missing__Input_Parameter_CloudToken"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, "Missing__Input_Parameter_ContractId"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName, "Missing__Input_Parameter_UserName"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Password, "Missing__Input_Parameter_Password"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_OperativeSystem, "Missing__Input_Parameter_OperativeSystem"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Version, "Missing__Input_Parameter_Version"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ReportFormat, "Missing_Input_Parameter_ReportFormat"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Email, "Missing_Input_Parameter_Email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Recode, "Missing_Input_Parameter_Recode"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AmountInCents, "Missing_Input_Parameter_AmountInCents"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Date, "Missing_Input_Parameter_Date"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateStart, "Missing_Input_Parameter_DateStart"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateEnd, "Missing_Input_Parameter_DateEnd"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Latitude, "Missing_Input_Parameter_Latitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Longitude, "Missing_Input_Parameter_Longitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName, "Missing_Input_Parameter_StreetName"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetNumber, "Missing_Input_Parameter_StreetNumber"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Plate, "Missing_Input_Parameter_Plate"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Group, "Missing_Input_Parameter_Group"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TimeInMinutes, "Missing_Input_Parameter_TimeInMinutes"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_QuantityInCents, "Missing_Input_Parameter_QuantityInCents"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TariffType, "Missing_Input_Parameter_TariffType"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Fine, "Missing_Input_Parameter_Fine"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Name, "Missing_Input_Parameter_Name"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_FirstSurname, "Missing_Input_Parameter_FirstSurname"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_MainMobilePhone, "Missing_Input_Parameter_MainMobilePhone"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ValidateConditions, "Missing_Input_Parameter_ValidateConditions"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_No_Plates, "Missing_Input_Parameter_No_Plates"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName_Email, "Missing_Input_Parameter_UserName_Email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken_UserNme_Password, "Missing_Input_Parameter_AuthorizationToken_UserNme_Password"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude, "Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed, "Parking_Not_Allowed"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Resident_Zone_24h, "Parking_Not_Allowed_Resident_Zone_24h"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September, "Parking_Not_Allowed_1_June_30_September"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September_And_May_Weekends, "Parking_Not_Allowed_1_June_30_September_And_May_Weekends"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_15_September, "Parking_Not_Allowed_1_June_15_September"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Outside_Working_Hours, "Parking_Not_Allowed_Outside_Working_Hours"}
            };

        /// <summary>
        /// Asociación del identificador del error con su mensaje en euskera
        /// </summary>
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
                { (int)ResultType.Result_Error_Location_Not_Found, "Service_Expired" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Recovery_Code_Not_Found" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Recovery_Code_Invalid" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Recovery_Code_Expired" },
                { (int)ResultType.Result_Error_Invalid_Login_NotBeforeFailed, "Invalid_Login_NotBeforeFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenExpired, "Invalid_Login_TokenExpired" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenNotCorrectlyFormed, "Invalid_Login_TokenNotCorrectlyFormed" },
                { (int)ResultType.Result_Error_Invalid_Login_SignatureNotValid, "Invalid_Login_SignatureNotValid" },
                { (int)ResultType.Result_Error_Invalid_Login_OnTokenValidateFailed, "Invalid_Login_OnTokenValidateFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_OnJtiValidateFailed, "Invalid_Login_OnJtiValidateFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_CustomCheckFailed, "Invalid_Login_CustomCheckFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_CreatedTimeCheckFailed, "Invalid_Login_CreatedTimeCheckFailed" },
                { (int)ResultType.Result_Error_Street_Or_Sector_Not_Found, "Street_Or_Sector_Not_Found" },
                { (int)ResultType.Result_Error_No_Bearer_Token, "No_Bearer_Token" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, "Missing__Input_Parameter_AuthorizationToken"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_CloudToken, "Missing__Input_Parameter_CloudToken"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, "Missing__Input_Parameter_ContractId"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName, "Missing__Input_Parameter_UserName"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Password, "Missing__Input_Parameter_Password"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_OperativeSystem, "Missing__Input_Parameter_OperativeSystem"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Version, "Missing__Input_Parameter_Version"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ReportFormat, "Missing_Input_Parameter_ReportFormat"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Email, "Missing_Input_Parameter_Email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Recode, "Missing_Input_Parameter_Recode"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AmountInCents, "Missing_Input_Parameter_AmountInCents"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Date, "Missing_Input_Parameter_Date"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateStart, "Missing_Input_Parameter_DateStart"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateEnd, "Missing_Input_Parameter_DateEnd"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Latitude, "Missing_Input_Parameter_Latitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Longitude, "Missing_Input_Parameter_Longitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName, "Missing_Input_Parameter_StreetName"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetNumber, "Missing_Input_Parameter_StreetNumber"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Plate, "Missing_Input_Parameter_Plate"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Group, "Missing_Input_Parameter_Group"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TimeInMinutes, "Missing_Input_Parameter_TimeInMinutes"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_QuantityInCents, "Missing_Input_Parameter_QuantityInCents"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TariffType, "Missing_Input_Parameter_TariffType"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Fine, "Missing_Input_Parameter_Fine"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Name, "Missing_Input_Parameter_Name"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_FirstSurname, "Missing_Input_Parameter_FirstSurname"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_MainMobilePhone, "Missing_Input_Parameter_MainMobilePhone"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ValidateConditions, "Missing_Input_Parameter_ValidateConditions"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_No_Plates, "Missing_Input_Parameter_No_Plates"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName_Email, "Missing_Input_Parameter_UserName_Email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken_UserNme_Password, "Missing_Input_Parameter_AuthorizationToken_UserNme_Password"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude, "Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed, "Parking_Not_Allowed"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Resident_Zone_24h, "Parking_Not_Allowed_Resident_Zone_24h"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September, "Parking_Not_Allowed_1_June_30_September"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September_And_May_Weekends, "Parking_Not_Allowed_1_June_30_September_And_May_Weekends"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_15_September, "Parking_Not_Allowed_1_June_15_September"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Outside_Working_Hours, "Parking_Not_Allowed_Outside_Working_Hours"}
            };

        /// <summary>
        /// Asociación del identificador del error con su mensaje en francés
        /// </summary>
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
                { (int)ResultType.Result_Error_Location_Not_Found, "Service_Expired" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Recovery_Code_Not_Found" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Recovery_Code_Invalid" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Recovery_Code_Expired" },
                { (int)ResultType.Result_Error_Invalid_Login_NotBeforeFailed, "Invalid_Login_NotBeforeFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenExpired, "Invalid_Login_TokenExpired" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenNotCorrectlyFormed, "Invalid_Login_TokenNotCorrectlyFormed" },
                { (int)ResultType.Result_Error_Invalid_Login_SignatureNotValid, "Invalid_Login_SignatureNotValid" },
                { (int)ResultType.Result_Error_Invalid_Login_OnTokenValidateFailed, "Invalid_Login_OnTokenValidateFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_OnJtiValidateFailed, "Invalid_Login_OnJtiValidateFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_CustomCheckFailed, "Invalid_Login_CustomCheckFailed" },
                { (int)ResultType.Result_Error_Invalid_Login_CreatedTimeCheckFailed, "Invalid_Login_CreatedTimeCheckFailed" },
                { (int)ResultType.Result_Error_Street_Or_Sector_Not_Found, "Street_Or_Sector_Not_Found" },
                { (int)ResultType.Result_Error_No_Bearer_Token, "No_Bearer_Token" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, "Missing__Input_Parameter_AuthorizationToken"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_CloudToken, "Missing__Input_Parameter_CloudToken"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, "Missing__Input_Parameter_ContractId"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName, "Missing__Input_Parameter_UserName"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Password, "Missing__Input_Parameter_Password"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_OperativeSystem, "Missing__Input_Parameter_OperativeSystem"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Version, "Missing__Input_Parameter_Version"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ReportFormat, "Missing_Input_Parameter_ReportFormat"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Email, "Missing_Input_Parameter_Email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Recode, "Missing_Input_Parameter_Recode"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AmountInCents, "Missing_Input_Parameter_AmountInCents"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Date, "Missing_Input_Parameter_Date"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateStart, "Missing_Input_Parameter_DateStart"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateEnd, "Missing_Input_Parameter_DateEnd"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Latitude, "Missing_Input_Parameter_Latitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Longitude, "Missing_Input_Parameter_Longitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName, "Missing_Input_Parameter_StreetName"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetNumber, "Missing_Input_Parameter_StreetNumber"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Plate, "Missing_Input_Parameter_Plate"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Group, "Missing_Input_Parameter_Group"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TimeInMinutes, "Missing_Input_Parameter_TimeInMinutes"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_QuantityInCents, "Missing_Input_Parameter_QuantityInCents"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TariffType, "Missing_Input_Parameter_TariffType"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Fine, "Missing_Input_Parameter_Fine"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Name, "Missing_Input_Parameter_Name"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_FirstSurname, "Missing_Input_Parameter_FirstSurname"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_MainMobilePhone, "Missing_Input_Parameter_MainMobilePhone"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ValidateConditions, "Missing_Input_Parameter_ValidateConditions"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_No_Plates, "Missing_Input_Parameter_No_Plates"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName_Email, "Missing_Input_Parameter_UserName_Email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken_UserNme_Password, "Missing_Input_Parameter_AuthorizationToken_UserNme_Password"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude, "Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed, "Parking_Not_Allowed"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Resident_Zone_24h, "Parking_Not_Allowed_Resident_Zone_24h"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September, "Parking_Not_Allowed_1_June_30_September"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September_And_May_Weekends, "Parking_Not_Allowed_1_June_30_September_And_May_Weekends"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_15_September, "Parking_Not_Allowed_1_June_15_September"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Outside_Working_Hours, "Parking_Not_Allowed_Outside_Working_Hours"}
            };
    }

    /// <summary>
    /// Clase de error
    /// </summary>
    [DataContractAttribute]
    public class Error
    {

        /// <summary>
        /// constructor del error
        /// </summary>
        /// <param name="code"></param>
        /// <param name="type"></param>
        public Error(int code, int type)
        {
            this.code = code;
            this.type = type;
            ErrorText errortext = new ErrorText();
            message_EN = errortext.ErrorTextEN[code];
            message_ES = errortext.ErrorTextES[code];
            message_EU = errortext.ErrorTextEU[code];
            message_FR = errortext.ErrorTextFR[code];
        }
        /// <summary>
        /// codigo de error
        /// </summary>
        [DataMemberAttribute]
        public int code { get; set; }
        /// <summary>
        /// tipo de error: 1. (Warning) --> aviso a usuario  2. (Exception) --> error no controlado  3. (Critical) --> error de lógica  4. (Low) --> informativo (para logs)
        /// </summary>
        [DataMemberAttribute]
        public int type { get; set; }
        /// <summary>
        /// Mensaje de error en inglés
        /// </summary>
        [DataMemberAttribute]
        public string message_EN { get; set; }
        /// <summary>
        /// Mensaje de error en castellano
        /// </summary>
        [DataMemberAttribute]
        public string message_ES { get; set; }
        /// <summary>
        /// Mensaje de error en euskera
        /// </summary>
        [DataMemberAttribute]
        public string message_EU { get; set; }
        /// <summary>
        /// Mensaje de error en francés
        /// </summary>
        [DataMemberAttribute]
        public string message_FR { get; set; }
    }

}
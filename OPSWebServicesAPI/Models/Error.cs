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
            Result_Error_Mobile_User_Email_Failed = -34,
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
                { (int)ResultType.Result_Error_InvalidAuthenticationHash, "Invalid authentication hash" },
                { (int)ResultType.Result_Error_MaxTimeAlreadyUsedInPark, "The plate has used the maximum amount of time/money in the sector, so the extension is not possible. This depends on the colour of the zone and the tariff type" },
                { (int)ResultType.Result_Error_ReentryTimeError, "The plate has not waited enough to return to the current sector" },
                { (int)ResultType.Result_Error_Plate_Has_No_Return, "Plate has no rights for doing an unparking operation" },
                { (int)ResultType.Result_Error_FineNumberNotFound, "Fine number not found" },
                { (int)ResultType.Result_Error_FineNumberFoundButNotPayable, "Fine number found but fine type is not payable" },
                { (int)ResultType.Result_Error_FineNumberFoundButTimeExpired, "Fine number not found but payment period has expired" },
                { (int)ResultType.Result_Error_FineNumberAlreadyPayed, "Fine number already paid" },
                { (int)ResultType.Result_Error_Generic, "Generic error" },
                { (int)ResultType.Result_Error_Invalid_Input_Parameter, "Invalid input parameter" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter, "Missing input parameter" },
                { (int)ResultType.Result_Error_OPS_Error, "System Error" },
                { (int)ResultType.Result_Error_Operation_Already_Inserted, "Operation already inserted" },
                { (int)ResultType.Result_Error_Quantity_To_Pay_Different_As_Calculated, "Quantity received different as the quantity calculated previously" },
                { (int)ResultType.Result_Error_Mobile_User_Not_Found, "Mobile user not found" },
                { (int)ResultType.Result_Error_Mobile_User_Already_Registered, "User name already registered" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Already_Registered, "e-mail already registered" },
                { (int)ResultType.Result_Error_Invalid_Login, "Invalid login" },
                { (int)ResultType.Result_Error_ParkingStartedByDifferentUser, "User has no rights. Operation begun by another user" },
                { (int)ResultType.Result_Error_Not_Enough_Credit, "User does not have enough credit" },
                { (int)ResultType.Result_Error_Cloud_Id_Not_Found, "Cloud identificator not found" },
                { (int)ResultType.Result_Error_App_Update_Required, "Update required" },
                { (int)ResultType.Result_Error_No_Return_For_Minimum, "Funds are not returned for exceeded minimum" },
                { (int)ResultType.Result_Error_User_Not_Validated, "Validation required" },
                { (int)ResultType.Result_Error_Location_Not_Found, "Location not found" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Recovery code not found" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Invalid recovery code" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Recovery code expired" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Failed, "User email failed" },
                { (int)ResultType.Result_Error_Invalid_Login_NotBeforeFailed, "Invalid login: not before failed" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenExpired, "Invalid login: token expired" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenNotCorrectlyFormed, "Invalid login: token not correctly formed" },
                { (int)ResultType.Result_Error_Invalid_Login_SignatureNotValid, "Invalid login: signature not valid" },
                { (int)ResultType.Result_Error_Invalid_Login_OnTokenValidateFailed, "Invalid login: on token validate failed" },
                { (int)ResultType.Result_Error_Invalid_Login_OnJtiValidateFailed, "Invalid login: on jti validate failed" },
                { (int)ResultType.Result_Error_Invalid_Login_CustomCheckFailed, "Invalid login: custom check failed" },
                { (int)ResultType.Result_Error_Invalid_Login_CreatedTimeCheckFailed, "Invalid login: created time check failed" },
                { (int)ResultType.Result_Error_Street_Or_Sector_Not_Found, "Street or sector not found" },
                { (int)ResultType.Result_Error_No_Bearer_Token, "No bearer token" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, "Missing input parameter: authorization token"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_CloudToken, "Missing input parameter: cloud token"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, "Missing input parameter: contract id"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName, "Missing input parameter: username"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Password, "Missing input parameter: password"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_OperativeSystem, "Missing input parameter: operative system"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Version, "Missing input parameter: version"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ReportFormat, "Missing input parameter: report format"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Email, "Missing input parameter: email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Recode, "Missing input parameter: recode"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AmountInCents, "Missing input parameter: amount in cents"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Date, "Missing input parameter: date"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateStart, "Missing input parameter: date start"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateEnd, "Missing input parameter: date end"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Latitude, " Missing input parameter: latitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Longitude, "Missing input parameter: longitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName, "Missing input parameter: street name"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetNumber, "Missing input parameter: street number"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Plate, "Missing input parameter: plate"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Group, "Missing input parameter: zone"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TimeInMinutes, "Missing input parameter: time in minutes"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_QuantityInCents, "Missing input parameter: quantity in cents"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TariffType, "Missing input parameter: tariff type"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Fine, "Missing input parameter: fine"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Name, "Missing input parameter: name"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_FirstSurname, "Missing input parameter: first surname"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_MainMobilePhone, "Missing input parameter: main mobile phone"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ValidateConditions, "Missing input parameter: validate conditions"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_No_Plates, "Missing input parameter: no plates"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName_Email, "Missing input parameter: user name or email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken_UserNme_Password, "Missing input parameter: authorization token or user name or password"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude, "Missing input parameter: street name and street number or latitude and longitude"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed, "Parking not allowed"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Resident_Zone_24h, "Parking not allowed resident zone 24h"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September, "Regulated parking in this zone only works during holiday season, from 1 June to 30 September"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September_And_May_Weekends, "Regulated parking in this zone only works during holiday season, from 1 June to 30 September, and the weekends and festive in May"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_15_September, "Regulated parking in this zone only works during holiday season, from 1 June to 15 September"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Outside_Working_Hours, "Out of hours. Parking is not permitted"}
            };

        /// <summary>
        /// Asociación del identificador del error con su mensaje en castellano
        /// </summary>
        public Dictionary<int, string> ErrorTextES = new Dictionary<int, string>()
            {
                { (int)ResultType.Result_OK, "OK" },
                { (int)ResultType.Result_Error, "Undefined" },
                { (int)ResultType.Result_Error_InvalidAuthenticationHash, "Autenticación de mensajes Hash no válida" },
                { (int)ResultType.Result_Error_MaxTimeAlreadyUsedInPark, "La matrícula ha consumido la cantidad máxima de tiempo, de forma que la extensión no es posible. Esto depende del color de la zona y el tipo de tarifa" },
                { (int)ResultType.Result_Error_ReentryTimeError, "La matrícula no ha esperado lo suficiente antes de volver al sector actual" },
                { (int)ResultType.Result_Error_Plate_Has_No_Return, "La matrícula no tiene derechos al desaparcar" },
                { (int)ResultType.Result_Error_FineNumberNotFound, "Número de referencia de la multa no encontrado" },
                { (int)ResultType.Result_Error_FineNumberFoundButNotPayable, "Número de referencia de la multa encontrado pero el tipo de multa no es pagable" },
                { (int)ResultType.Result_Error_FineNumberFoundButTimeExpired, "Número de referencia de la multa no encontrado pero el plazo de pago ha vencido" },
                { (int)ResultType.Result_Error_FineNumberAlreadyPayed, "El número de referencia de la multa ya se ha abonado" },
                { (int)ResultType.Result_Error_Generic, "Error genérico" },
                { (int)ResultType.Result_Error_Invalid_Input_Parameter, "Parámetro de entrada inválido" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter, "Falta uno de los parámetros de entrada" },
                { (int)ResultType.Result_Error_OPS_Error, "Error en el sistema" },
                { (int)ResultType.Result_Error_Operation_Already_Inserted, "La operación ya se ha insertado" },
                { (int)ResultType.Result_Error_Quantity_To_Pay_Different_As_Calculated, "La cantidad recibida no corresponde a la cantidad calculada previamente" },
                { (int)ResultType.Result_Error_Mobile_User_Not_Found, "El usuario del móvil no se encuentra" },
                { (int)ResultType.Result_Error_Mobile_User_Already_Registered, "El nombre del usuario ya se ha registrado" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Already_Registered, "El e-mail ya se ha registrado" },
                { (int)ResultType.Result_Error_Invalid_Login, "Login inválido" },
                { (int)ResultType.Result_Error_ParkingStartedByDifferentUser, "La operación ha sido iniciada por otro usuario" },
                { (int)ResultType.Result_Error_Not_Enough_Credit, "El usuario no dispone de crédito suficiente" },
                { (int)ResultType.Result_Error_Cloud_Id_Not_Found, "Identificador de nube no encontrado" },
                { (int)ResultType.Result_Error_App_Update_Required, "Actualización requerida" },
                { (int)ResultType.Result_Error_No_Return_For_Minimum, "No hay devolución de dinero cuando el mínimo se ha excedido" },
                { (int)ResultType.Result_Error_User_Not_Validated, "Validación requerida" },
                { (int)ResultType.Result_Error_Location_Not_Found, "Ubicación no encontrada" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Código de recuperación no encontrado" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Código de recuperación no válido" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Código de recuperación caducado" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Failed, "El email es inválido" },
                { (int)ResultType.Result_Error_Invalid_Login_NotBeforeFailed, "Login inválido no antes de error" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenExpired, "Login inválido, token caducado" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenNotCorrectlyFormed, "Login inválido por no formar correctamente el token" },
                { (int)ResultType.Result_Error_Invalid_Login_SignatureNotValid, "Login inválido por firma inválida" },
                { (int)ResultType.Result_Error_Invalid_Login_OnTokenValidateFailed, "Login inválido por fallo al validar el token" },
                { (int)ResultType.Result_Error_Invalid_Login_OnJtiValidateFailed, "Login inválido error al validar el jti" },
                { (int)ResultType.Result_Error_Invalid_Login_CustomCheckFailed, "Login inválido error en la comprobación personalizada" },
                { (int)ResultType.Result_Error_Invalid_Login_CreatedTimeCheckFailed, "Login inválido error en la comprobación del tiempo creada" },
                { (int)ResultType.Result_Error_Street_Or_Sector_Not_Found, "Calle o sector no encontrados" },
                { (int)ResultType.Result_Error_No_Bearer_Token, "No hay ficha del portador" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, "Falta un parámetro de entrada: token de autorización"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_CloudToken, "Falta un parámetro de entrada: cloud token (token de nube)"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, "Falta un parámetro de entrada: identificación de municipio"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName, "Falta un parámetro de entrada: nombre de usuario"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Password, "Falta un parámetro de entrada: contraseña"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_OperativeSystem, "Falta un parámetro de entrada: sistema operativo"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Version, "Falta un parámetro de entrada: versión"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ReportFormat, "Falta un parámetro de entrada: formato del informe"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Email, "Falta un parámetro de entrada: email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Recode, "Falta un parámetro de entrada: código de recuperación"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AmountInCents, "Falta un parámetro de entrada: cantidad en céntimos"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Date, "Falta un parámetro de entrada: fecha"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateStart, "Falta un parámetro de entrada: fecha de comienzo"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateEnd, "Falta un parámetro de entrada: fecha de finalización"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Latitude, "Falta un parámetro de entrada: latitud"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Longitude, "Falta un parámetro de entrada: longitud"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName, "Falta un parámetro de entrada: nombre de la calle"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetNumber, "Falta un parámetro de entrada: número de la calle"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Plate, "Falta un parámetro de entrada: matrícula"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Group, "Falta un parámetro de entrada: sector"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TimeInMinutes, "Falta un parámetro de entrada: el tiempo en minutos"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_QuantityInCents, "Falta un parámetro de entrada: cantidad en céntimos"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TariffType, "Falta un parámetro de entrada: tipo de tarifa"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Fine, "Falta un parámetro de entrada: multa"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Name, "Falta un parámetro de entrada: nombre"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_FirstSurname, "Falta un parámetro de entrada: primer apellido"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_MainMobilePhone, "Falta un parámetro de entrada: teléfono móvil principal"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ValidateConditions, "Falta un parámetro de entrada: validar condiciones"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_No_Plates, "Falta un parámetro de entrada: no hay matrículas"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName_Email, "Falta un parámetro de entrada: nombre de usuario o email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken_UserNme_Password, "Falta un parámetro de entrada: token de autorización o nombre de usuario o contraseña"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude, "Falta un parámetro de entrada: nombre de calle y número de calle o latitud y longitud"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed, "No se permite aparcar"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Resident_Zone_24h, "No se permite aparcar zona residencial 24h"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September, "El estacionamiento regulado en esta zona sólo está operativo durante el periodo vacacional, del 1 de junio al 30 de septiembre."},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September_And_May_Weekends, "El estacionamiento regulado en esta zona sólo está operativo durante el periodo vacacional, del 1 de junio al 30 de septiembre y los fines de semana y festivos de mayo"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_15_September, "El estacionamiento regulado en esta zona sólo está operativo durante el periodo vacacional, del 1 de junio al 15 de septiembre."},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Outside_Working_Hours, "Fuera de horario. Estacionamiento no permitido."}
            };

        /// <summary>
        /// Asociación del identificador del error con su mensaje en euskera
        /// </summary>
        public Dictionary<int, string> ErrorTextEU = new Dictionary<int, string>()
            {
                { (int)ResultType.Result_OK, "OK" },
                { (int)ResultType.Result_Error, "Undefined" },
                { (int)ResultType.Result_Error_InvalidAuthenticationHash, "Hash mezuen baliogabeko autentifikazioa" },
                { (int)ResultType.Result_Error_MaxTimeAlreadyUsedInPark, "Matrikulak gehienezko denbora /diru kopurua erabili du sektorean , beraz luzapena ezinezkoa da. Hau zonaren kolorearen eta tarifa motaren araberakoa da" },
                { (int)ResultType.Result_Error_ReentryTimeError, "Matrikulak ez du nahikoa itxaron sektore honetara itzultzeko" },
                { (int)ResultType.Result_Error_Plate_Has_No_Return, "Matrikulak ez dauka desaparkatzeko eskubiderik" },
                { (int)ResultType.Result_Error_FineNumberNotFound, "Isun-zenbakia ez da aurkitu" },
                { (int)ResultType.Result_Error_FineNumberFoundButNotPayable, "Isun-zenbakia aurkitu da baina isun-mota ez da ordaingarria" },
                { (int)ResultType.Result_Error_FineNumberFoundButTimeExpired, "Isun-zenbakia aurkitu da baina ordaintzeko epea iraungi da" },
                { (int)ResultType.Result_Error_FineNumberAlreadyPayed, "Isun-zenbakia dagoeneko ordaindu da" },
                { (int)ResultType.Result_Error_Generic, "Errore generikoa" },
                { (int)ResultType.Result_Error_Invalid_Input_Parameter, "Baliogabeko sarrera-parametroa" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter, "Sarrera-parametroa falta da" },
                { (int)ResultType.Result_Error_OPS_Error, "Sistemaren errorea" },
                { (int)ResultType.Result_Error_Operation_Already_Inserted, "Eragiketa dagoeneko sartu da" },
                { (int)ResultType.Result_Error_Quantity_To_Pay_Different_As_Calculated, "Jasotako kopurua aurretik kalkulatutakoaren desberdina da" },
                { (int)ResultType.Result_Error_Mobile_User_Not_Found, "Mugikor erabiltzailea ez da aurkitu" },
                { (int)ResultType.Result_Error_Mobile_User_Already_Registered, "Erabiltzaile izena dagoeneko erregistratu da" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Already_Registered, "E-maila dagoeneko erregistratu da" },
                { (int)ResultType.Result_Error_Invalid_Login, "Baliogabeko saio-hasiera" },
                { (int)ResultType.Result_Error_ParkingStartedByDifferentUser, "Erabiltzaileak ez du eskubiderik. Beste erabiltzaile batek hasitako eragiketa" },
                { (int)ResultType.Result_Error_Not_Enough_Credit, "Erabiltzaileak ez dauka nahikoa kreditu" },
                { (int)ResultType.Result_Error_Cloud_Id_Not_Found, "Hodeieko identifikadorea ez da aurkitu" },
                { (int)ResultType.Result_Error_App_Update_Required, "Eguneratzea beharrezkoa" },
                { (int)ResultType.Result_Error_No_Return_For_Minimum, "Dirurik ez da itzultzen gutxienekoa gainditzerakoan" },
                { (int)ResultType.Result_Error_User_Not_Validated, "Balioztatzea beharrezkoa" },
                { (int)ResultType.Result_Error_Location_Not_Found, "Kokalekua ez da aurkitu" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Berreskuratze-kodea ez da aurkitu" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Baliogabeko berreskuratze-kodea" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Berreskuratze-kodea iraungi egin da" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Failed, "E-maila ez da aurkitu" },
                { (int)ResultType.Result_Error_Invalid_Login_NotBeforeFailed, "Baliogabeko saio-hasiera ez huts egin aurretik" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenExpired, "Baliogabeko saio-hasiera tokena iraungi egin da" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenNotCorrectlyFormed, "Baliogabeko saio-hasiera tokena ez da behar bezala sortu" },
                { (int)ResultType.Result_Error_Invalid_Login_SignatureNotValid, "Baliogabeko saio-hasiera izenpea ez da baliozkoa" },
                { (int)ResultType.Result_Error_Invalid_Login_OnTokenValidateFailed, "Baliogabeko saio-hasiera tokena balidatzerakoan huts egin du" },
                { (int)ResultType.Result_Error_Invalid_Login_OnJtiValidateFailed, "Baliogabeko saio-hasiera jti balidatzerakoan huts egin du" },
                { (int)ResultType.Result_Error_Invalid_Login_CustomCheckFailed, "Baliogabeko saio-hasiera pertsonalizatutako egiaztapenak huts egin du" },
                { (int)ResultType.Result_Error_Invalid_Login_CreatedTimeCheckFailed, "Baliogabeko saio-hasiera sortutako denboraren egiaztapenak huts egin du" },
                { (int)ResultType.Result_Error_Street_Or_Sector_Not_Found, "Kalea edo sektorea ez da aurkitu" },
                { (int)ResultType.Result_Error_No_Bearer_Token, "Eramailearen tokenik ez" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, "Sarrera-parametroa falta da: baimentzeko tokena"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_CloudToken, "Sarrera-parametroa falta da: hodeieko tokena"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, "Sarrera-parametroa falta da: kontratuaren identifikazioa"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName, "Sarrera-parametroa falta da: erabiltzaile izena"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Password, "Sarrera-parametroa falta da: pasahitza"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_OperativeSystem, "Sarrera-parametroa falta da: sistema eragilea"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Version, "Sarrera-parametroa falta da: bertsioa"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ReportFormat, "Sarrera-parametroa falta da: txostenaren formatua"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Email, "Sarrera-parametroa falta da: e-maila"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Recode, "Sarrera-parametroa falta da: berkodetu"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AmountInCents, "Sarrera-parametroa falta da: kopurua zentaboetan"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Date, "Sarrera-parametroa falta da: data"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateStart, "Sarrera-parametroa falta da: hasiera data"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateEnd, "Sarrera-parametroa falta da: amaiera data"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Latitude, "Sarrera-parametroa falta da: latitudea"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Longitude, "Sarrera-parametroa falta da: longitudea"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName, "Sarrera-parametroa falta da: kalearen izena"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetNumber, "Sarrera-parametroa falta da: kalearen zenbakia"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Plate, "Sarrera-parametroa falta da: matrikula zenbakia"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Group, "Sarrera-parametroa falta da: taldea"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TimeInMinutes, "Sarrera-parametroa falta da: denbora minutuetan"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_QuantityInCents, "Sarrera-parametroa falta da: kantitatea zentaboetan"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TariffType, "Sarrera-parametroa falta da: tarifa mota"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Fine, "Sarrera-parametroa falta da: isuna"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Name, "Sarrera-parametroa falta da: izena"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_FirstSurname, "Sarrera-parametroa falta da: lehen abizena"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_MainMobilePhone, "Sarrera-parametroa falta da: telefono mugikor nagusia"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ValidateConditions, "Sarrera-parametroa falta da: baldintzak balioztatu"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_No_Plates, "Sarrera-parametroa falta da: matrikula-zenbakirik ez"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName_Email, "Sarrera-parametroa falta da: erabiltzaile izena edo e-maila"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken_UserNme_Password, "Sarrera-parametroa falta da: baimentze tokena edo erabiltzaile izena edo pasahitza"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude, "Sarrera-parametroa falta da: kalearen izena eta zenbakia edo latitude eta longitudea"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed, "Aparkatzea debekatuta dago"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Resident_Zone_24h, "Aparkatzea debekatuta dago bertakoentzako gunea 24h"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September, "Gune honetan aparkalekua uda-garian bakarrik funtzionatzen du, ekainaren 1etik irailaren 30ra"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September_And_May_Weekends, "Gune honetan aparkalekua uda-garian bakarrik funtzionatzen du, ekainaren 1etik irailaren 30ra, eta maiatzako asteburu eta jai egunetan"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_15_September, "Gune honetan aparkalekua uda-garian bakarrik funtzionatzen du, ekainaren 1etik irailaren 15ra"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Outside_Working_Hours, "Ordutik kampo. Ezin duzu aparkatu"}
            };

        /// <summary>
        /// Asociación del identificador del error con su mensaje en francés
        /// </summary>
        public Dictionary<int, string> ErrorTextFR = new Dictionary<int, string>()
            {
                { (int)ResultType.Result_OK, "OK" },
                { (int)ResultType.Result_Error, "Undefined" },
                { (int)ResultType.Result_Error_InvalidAuthenticationHash, "Authentification par hachage invalide" },
                { (int)ResultType.Result_Error_MaxTimeAlreadyUsedInPark, "La plaque d´immatriculation a utilisé la quantité maximale de temps/argent dans le secteur donc l´extension n´est pas possible. Cela dépend de la couleur de la zone et du type de tarif" },
                { (int)ResultType.Result_Error_ReentryTimeError, "La plaque d´inmatriculation n´a pas assez attendu pour retourner dans le secteur actuel" },
                { (int)ResultType.Result_Error_Plate_Has_No_Return, "La plaque d´immatriculation  n´a pas le droit de faire l´opération d´annulation de stationnement" },
                { (int)ResultType.Result_Error_FineNumberNotFound, "Numéro de contravention non trouvé" },
                { (int)ResultType.Result_Error_FineNumberFoundButNotPayable, "Numéro de contravention trouvé mais type de contravention non payable" },
                { (int)ResultType.Result_Error_FineNumberFoundButTimeExpired, "Numéro de contravention non trouvé mais la période de paiement a expiré" },
                { (int)ResultType.Result_Error_FineNumberAlreadyPayed, "Numéro de contravention déjà payé" },
                { (int)ResultType.Result_Error_Generic, "Erreur générique" },
                { (int)ResultType.Result_Error_Invalid_Input_Parameter, "Paramètre d´entrée invalide" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter, "Paramètre d´entrée manquant" },
                { (int)ResultType.Result_Error_OPS_Error, "Erreur du système" },
                { (int)ResultType.Result_Error_Operation_Already_Inserted, "Opération déjà insérée" },
                { (int)ResultType.Result_Error_Quantity_To_Pay_Different_As_Calculated, "Quantité reçue différente à la quantité calculée préalablement" },
                { (int)ResultType.Result_Error_Mobile_User_Not_Found, "Utilisateur de mobile non trouvé" },
                { (int)ResultType.Result_Error_Mobile_User_Already_Registered, "Nom d´utilisateur déjà enregistré" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Already_Registered, "e-mail déjà enregistré" },
                { (int)ResultType.Result_Error_Invalid_Login, "Connexion invalide" },
                { (int)ResultType.Result_Error_ParkingStartedByDifferentUser, "L´utilisateur n´a pas de droits. Opération commencée par un autre utilisateur" },
                { (int)ResultType.Result_Error_Not_Enough_Credit, "L´utilisateur n´a pas assez de crédit" },
                { (int)ResultType.Result_Error_Cloud_Id_Not_Found, "Identification cloud non trouvée" },
                { (int)ResultType.Result_Error_App_Update_Required, "Mise à jour requise" },
                { (int)ResultType.Result_Error_No_Return_For_Minimum, "Fonds non remboursés pour le minimum dépassé" },
                { (int)ResultType.Result_Error_User_Not_Validated, "Validation requise" },
                { (int)ResultType.Result_Error_Location_Not_Found, "Emplacement non trouvé" },
                { (int)ResultType.Result_Error_Recovery_Code_Not_Found, "Code de récupération non trouvé" },
                { (int)ResultType.Result_Error_Recovery_Code_Invalid, "Code de récupération invalide" },
                { (int)ResultType.Result_Error_Recovery_Code_Expired, "Le code de récupération a expiré" },
                { (int)ResultType.Result_Error_Mobile_User_Email_Failed, "e-mail invalide" },
                { (int)ResultType.Result_Error_Invalid_Login_NotBeforeFailed, "Connexion invalide non avant échec préalable" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenExpired, "Connexion invalide jeton expiré" },
                { (int)ResultType.Result_Error_Invalid_Login_TokenNotCorrectlyFormed, "Connexion invalide jeton non correctement formé" },
                { (int)ResultType.Result_Error_Invalid_Login_SignatureNotValid, "Connexion invalide signature non valide" },
                { (int)ResultType.Result_Error_Invalid_Login_OnTokenValidateFailed, "Connexion invalide échec de validation jeton" },
                { (int)ResultType.Result_Error_Invalid_Login_OnJtiValidateFailed, "Connexion invalide échec validation jti" },
                { (int)ResultType.Result_Error_Invalid_Login_CustomCheckFailed, "Connexion invalide échec vérification personalisée" },
                { (int)ResultType.Result_Error_Invalid_Login_CreatedTimeCheckFailed, "Connexion invalide échec validation temps créée" },
                { (int)ResultType.Result_Error_Street_Or_Sector_Not_Found, "Rue ou secteur non trouvés" },
                { (int)ResultType.Result_Error_No_Bearer_Token, "Pas de jeton détenu" },
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, "Entrée manquante paramètre: autorisation jeton "},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_CloudToken, "Entrée manquante paramètre: jeton cloud"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, "Entrée manquante paramètre: contrat id"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName, "Entrée manquante paramètre: nom d´utilisateur"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Password, "Entrée manquante paramètre: mot de passe"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_OperativeSystem, "Entrée manquante paramètre: système de fonctionnement"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Version, "Entrée manquante paramètre: version"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ReportFormat, "Entrée manquante paramètre: rapport format"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Email, "Entrée manquante paramètre: email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Recode, "Entrée manquante paramètre: recode"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AmountInCents, "Entrée manquante paramètre: quantité en centimes"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Date, "Entrée manquante paramètre: date"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateStart, "Entrée manquante paramètre:  date de début "},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_DateEnd, "Entrée manquante paramètre:  date de fin"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Latitude, " Entrée manquante paramètre: latitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Longitude, "Entrée manquante paramètre: longitude"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName, "Entrée manquante paramètre: nom de la rue"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetNumber, "Entrée manquante paramètre: numéro de la rue"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Plate, "Entrée manquante paramètre: plaque d´immatriculation"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Group, "Entrée manquante paramètre: secteur"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TimeInMinutes, "Entrée manquante paramètre: temps en minutes"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_QuantityInCents, "Entrée manquante paramètre: quantité en centimes"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_TariffType, "Entrée manquante paramètre: type de tarif"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Fine, "Entrée manquante paramètre: contravention"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_Name, "Entrée manquante paramètre: prénom"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_FirstSurname, "Entrée manquante paramètre: premier nom de famille"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_MainMobilePhone, "Entrée manquante paramètre: téléphone mobile principal"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_ValidateConditions, "Entrée manquante paramètre: valider conditions"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_No_Plates, "Entrée manquante paramètre: pas de plaque d´immatriculation"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_UserName_Email, "Entrée manquante paramètre: nom d´utilisateur ou email"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken_UserNme_Password, "Entrée manquante paramètre: autorisation de jeton ou nom d´utilisateur ou mot de passe"},
                { (int)ResultType.Result_Error_Missing_Input_Parameter_StreetName_StreetNumber_Latitude_Longitude, "Entrée manquante paramètre: nom et numéro  de la rue ou latitude et longitude"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed, "Parking non autorisé"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Resident_Zone_24h, "Parking non autorisé zone résident 24h"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September, "Le stationnement réglementé dans cette zone ne fonctionne que pendant la saison des vacances, du 1er juin au 30 septembre"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_30_September_And_May_Weekends, "Le stationnement réglementé dans cette zone ne fonctionne que pendant la saison des vacances, du 1er juin au 30 septembre, et les week-ends et jours fériés en mai"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_1_June_15_September, "Le stationnement réglementé dans cette zone ne fonctionne que pendant la saison des vacances, du 1er juin au 15 septembre"},
                { (int)ResultType.Result_Error_Parking_Not_Allowed_Outside_Working_Hours, "En dehors de ces horaires, le stationnement est interdit."}
            };
    }

    /// <summary>
    /// Clase de error
    /// </summary>
    [DataContractAttribute]
    public class Error
    {
        public Error() { }
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
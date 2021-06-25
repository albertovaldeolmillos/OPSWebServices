using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Xml;
using AutoMapper;
using Jot;
using Newtonsoft.Json;
using OPS.Comm;
using OPS.Comm.Becs.Messages;
using OPS.Comm.Cryptography.TripleDes;
using OPS.Components.Data;
using OPS.FineLib;
using OPSWebServicesAPI.Helpers;
using OPSWebServicesAPI.Models;
using Oracle.ManagedDataAccess.Client;

namespace OPSWebServicesAPI.Controllers
{
    public class WSUserManagementController : ApiController
    {

        static ILogger _logger = null;
        static string _MacTripleDesKey = null;
        static byte[] _normTripleDesKey = null;
        static MACTripleDES _mac3des = null;
        static string _partnerMacTripleDesKey = null;
        static byte[] _partnerNormTripleDesKey = null;
        static MACTripleDES _partnerMac3des = null;
        static string _xmlTagName = null;
        static string _useHash = null;
        private const long BIG_PRIME_NUMBER = 2147483647;
        private const string IN_SUFIX = "_in";
        private const string OUT_SUFIX = "_out";
        internal const string KEY_MESSAGE_TCP_0 = "75o73K3%0=53?73*h>7*32<5";
        internal const string KEY_MESSAGE_TCP_1 = "35s03!*3!8H3j33*53)73*lf";
        internal const string KEY_MESSAGE_TCP_2 = "7*32z5$8j07!3*35f5%73(30";
        internal const string KEY_MESSAGE_TCP_3 = "*5%57*3j3!*50,73*3(65k3%";
        internal const string KEY_MESSAGE_TCP_4 = "3!*50g73*5=57*3j$8j07!3*";
        internal const string KEY_MESSAGE_TCP_5 = "j07!(*h>7*32<5y8n%=!g5/&";
        internal const string KEY_MESSAGE_TCP_6 = "!8H37t3*5*3(65k3%57*3j3!";
        internal const string KEY_MESSAGE_TCP_7 = "253)73*lf5%73(30*32z5$8j";
        private const int DATE_FORMAT_DAYS = 1;
        private const int DATE_FORMAT_RANGE = 2;
        internal const string MERCHANT_NAME = "Zaragoza Z+M";
        internal const string MERCHANT_CODE = "322319344";
        internal const string MERCHANT_KEY = "3926LO490N18T012";
        internal const string MERCHANT_KEY_TEST = "qwertyasdf0123456789";
        internal const string MERCHANT_KEY_LONG = "JMd7/eiuzsEEF+lKxxDq41rADVNt4X5v";
        internal const string MERCHANT_KEY_LONG_TEST = "Mk9m98IfEblmPfrpsawt7BmxObt98Jev";
        internal const string PARM_NUM_SPACES_BONUS = "P_NUM_SPACES_BONUS";
        internal const string PARM_FREE_SPACE_TIME = "P_FREE_SPACE_TIME";
        internal const string PARM_MAX_FREE_SPACES = "P_MAX_FREE_SPACES";
        internal const string PARM_MAX_DIST_SPACES = "P_MAX_DIST_SPACES";

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


        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            InitializeStatic();
        }

        /*
             * 
             a.	LoginUser: object containing input parameters of the method:

                 - mui: Mobile user ID (authorization token) - ** Login is performed with token or combination of username/password
                 - un: Username
                 - pw: Password
                 - v: App version
                 - cid: Cloud ID. Used in ‘balance’ notifications because this notification is not associated with specific operation.
                 - os: Operative System (1: Android, 2: iOS)
                 - contid: Contract ID - *This parameter is optional
                 - ah: authentication hash - *This parameter is optional


                 The authentication hash will be a string generated using the input parameters. Using this value we will detect the method call has been made by a well known client.

                 If the login or email is sent along with the password, then an authorization token is generated and saved with the user information. This token is then returned as
                 the user ID.
                 If the token is used for the login, then it is verified with the current token value saved with the user information. If the token is no longer valid, but corresponds
                 with the previous token, then a new token is generated, saved and returned. However, if the token does not correspond to the previous one, then an error is returned.

             b.	Result: is a string with the possible values:
                 a.	>0: Mobile user id - the user ID is now the authorization token
                 b.	-1: Invalid authentication hash
                 c.	-9: Generic Error (for example database or execution error.)
                 d.	-10: Invalid input parameter
                 e.	-11: Missing input parameter
                 f.	-12: OPS System error
                 g.	-23: Login invalid
                 h.	-27: update required
                 i. -29: validation required
             * 
             * 
         */

        /// <summary>
        /// Método que devuelve el token de autorización de inicio de sesión o error en caso contrario
        /// </summary>
        /// <param name="userLogin">Objeto UserLogin con la información necesaria para el Login</param>
        /// <returns>Devuelve un objeto Result indicando si la respuesta ha sido correcta, el resultado (mui - authorization token) y si ha habido error, dicho error  </returns>
        [HttpPost]
        [Route("LoginUserAPI")]
        public Result LoginUserAPI([FromBody] UserLogin userLogin)
        {
            string strToken = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
            Result response = new Result();

            /*SortedList parametersIn = new SortedList();
            parametersIn.Add("un", loginUser.un);
            parametersIn.Add("pw", loginUser.pw);
            parametersIn.Add("mui", loginUser.mui);
            parametersIn.Add("cid", loginUser.cid);
            parametersIn.Add("os", loginUser.os);
            parametersIn.Add("v", loginUser.v);
            parametersIn.Add("ah", loginUser.ah);
            parametersIn.Add("contid", loginUser.contid);*/

            SortedList parametersIn = new SortedList();
            PropertyInfo[] properties = typeof(UserLogin).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                //string NombreAtributo = property.Name;
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                var Valor = property.GetValue(userLogin);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("LoginUserAPI: parametersIn= {0}", parametersIn), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if (((parametersIn["un"] == null || (parametersIn["un"] != null && parametersIn["un"].ToString().Length == 0)) && 
                        (parametersIn["pw"] == null || (parametersIn["pw"] != null && parametersIn["pw"].ToString().Length == 0)) && 
                        (parametersIn["mui"] == null) || (parametersIn["mui"] != null && parametersIn["mui"].ToString().Length == 0)) ||
                        (parametersIn["cid"] == null) || (parametersIn["cid"].ToString().Length == 0) ||
                        (parametersIn["os"] == null) || (parametersIn["os"].ToString().Length == 0) ||
                        (parametersIn["v"] == null) || (parametersIn["v"].ToString().Length == 0) ||
                        (parametersIn["contid"] == null) || (parametersIn["contid"].ToString().Length == 0))
                    {
                        Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Missing parameter: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                        response.IsSuccess = false;
                        response.Error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
                        response.Value = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;//Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    }
                    else
                    {
                        bool bHashOk = false;

                        if (_useHash.Equals("true"))
                        {
                            string strCalculatedHash = CalculateHash(strHashString);
                            string strCalculatedHashJavaBouncyCastle = CalculateHashJavaBouncyCastle(strHashString);

                            if ((strCalculatedHash == strHash) && (strCalculatedHashJavaBouncyCastle == strHash))
                                bHashOk = true;
                        }
                        else
                            bHashOk = true;

                        if (!bHashOk)
                        {
                            Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Bad hash: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                            response.IsSuccess = false;
                            response.Error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.Value = Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                            return response;// Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            if (parametersIn["contid"] != null)
                            {
                                if (parametersIn["contid"].ToString().Trim().Length > 0)
                                    nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            }
                            // Set Contract Id to 0 to force all user queries to use the global users connection
                            nContractId = 0;

                            // Check application version
                            int nVersionResult = CheckApplicationVersion(parametersIn["v"].ToString(), nContractId);
                            if (nVersionResult == 0)
                            {
                                Logger_AddLogMessage(string.Format("LoginUserAPI::Incorrect app version - update needed: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_App_Update_Required, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_App_Update_Required).ToString();
                                return response;//Convert.ToInt32(ResultType.Result_Error_App_Update_Required).ToString();
                            }
                            else if (nVersionResult < 0)
                            {
                                Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Could not verify version: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;//Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                            }

                            // Check to see if login is valid
                            bool bUseLogin = false;
                            bool bUseToken = false;
                            if (parametersIn["un"] != null)
                            {
                                if (parametersIn["un"].ToString().Length > 0)
                                    bUseLogin = true;
                            }
                            if (parametersIn["mui"] != null)
                            {
                                if (parametersIn["mui"].ToString().Length > 0)
                                    bUseToken = true;
                            }

                            // Use login for verification
                            if (bUseLogin)
                            {
                                int nMobileUserId = IsLoginValid(parametersIn["un"].ToString(), parametersIn["pw"].ToString(), nContractId);

                                if (nMobileUserId <= 0)
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Could not validate user: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                    response.IsSuccess = false;
                                    response.Error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.Value = Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                    return response;//Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                }
                                else
                                    Logger_AddLogMessage(string.Format("LoginUserAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                // Check user validation
                                if (!IsUserValidated(nMobileUserId, nContractId))
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::User not validated - needs to activate account: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                    response.IsSuccess = false;
                                    response.Error = new Error((int)ResultType.Result_Error_User_Not_Validated, (int)SeverityError.Critical);
                                    response.Value = Convert.ToInt32(ResultType.Result_Error_User_Not_Validated).ToString();
                                    return response;//Convert.ToInt32(ResultType.Result_Error_User_Not_Validated).ToString();
                                }

                                // Generate authorization token
                                strToken = GetNewToken();

                                if (!UpdateWebCredentials(nMobileUserId, parametersIn["cid"].ToString(), Convert.ToInt32(parametersIn["os"]), strToken, parametersIn["v"].ToString(), nContractId))
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Could not update web credentials: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                    response.IsSuccess = false;
                                    response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                    response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    return response;//Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                }
                            }
                            else if (bUseToken)
                            {
                                // Use token for verification
                                strToken = parametersIn["mui"].ToString();

                                // Try to obtain user from token
                                int nMobileUserId = GetUserFromToken(strToken, nContractId);

                                if (nMobileUserId <= 0)
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Could not obtain user from token: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                    response.IsSuccess = false;
                                    response.Error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.Value = Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                    return response;//Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                }
                                else
                                    Logger_AddLogMessage(string.Format("LoginUserAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                // Determine if token is valid
                                TokenValidationResult tokenResult = DefaultVerification(strToken);

                                if (tokenResult != TokenValidationResult.Passed && tokenResult != TokenValidationResult.TokenExpired)
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Token not valid: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                    response.IsSuccess = false;
                                    response.Error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.Value = Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                    return response;//Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                }

                                // If token is valid, but has expired, issue a new token
                                if (tokenResult == TokenValidationResult.TokenExpired)
                                {
                                    strToken = GetNewToken();
                                    Logger_AddLogMessage(string.Format("LoginUserAPI: Token expired, issued new one = {0}", strToken), LoggerSeverities.Info);
                                }

                                if (!UpdateWebCredentials(nMobileUserId, parametersIn["cid"].ToString(), Convert.ToInt32(parametersIn["os"]), strToken, parametersIn["v"].ToString(), nContractId))
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Could not update web credentials: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                    response.IsSuccess = false;
                                    response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                    response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    return response;//Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                }
                            }
                        }
                    }
                }
                else
                {
                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Incorrect input format: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                    response.IsSuccess = false;
                    response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;//Convert.ToInt32(rt).ToString();
                }
            }
            catch (Exception e)
            {
                strToken = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                Logger_AddLogMessage(string.Format("LoginUserAPI::Error - {0}: parametersIn= {1}", e.Message, parametersIn), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.IsSuccess = false;
                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;//Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
            }

            response.IsSuccess = true;
            response.Error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            response.Value = strToken;
            return response;//strToken;
        }

        /*
        * 
        * The parameters of method UpdateUserXML are:
        a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <contid>Installation ID</contid>
                <mui>Mobile user Id (authorization token)</mui>
                <un>Username</un> - *This parameter is optional
                <pw>Password</pw> - *This parameter is optional
                <em>E-Mail</em>
                <fs>First Surname</fs>
                <ss>Second Surname</ss> - *This parameter is optional
                <na>Names</na>
                <nif>NIF, NIE or CIF</nif> - *This parameter is optional
                <mmp>Main Mobile Phone</mmp>
                <amp>Alternative Mobile Phone</amp> - *This parameter is optional
                <asn>Address: Street Name</asn> - *This parameter is optional
                <abn>Address: Building Number</abn> - *This parameter is optional
                <adf>Address: Department Floor</adf> - *This parameter is optional
                <add>Address: Department Door</add> - *This parameter is optional
                <ads>Address: Department Stair</ads> - *This parameter is optional
                <adl>Address: Department Letter or Number</adl> - *This parameter is optional
                <apc>Address: Postal Code</apc> - *This parameter is optional
                <aci>Address: City</aci> - *This parameter is optional
                <apr>Address: Province</apr> - *This parameter is optional
                <val>Validate new conditions is necessary</val> 
                <plates>
                    <p>Plate</p>
                    <p>Plate</p>
                    ...
                    <p>Plate</p>
                </plates>
                <notifications>
	                <fn>Fine notifications? (1:true, 0:false)</fn>
	                <unp>UnParking notifications? (1:true, 0:false)</unp>
	                <t_unp>minutes before the limit (unparking notifications)</t_unp>
	                <re>recharge notifications? (1:true, 0:false)</re>
	                <ba>low balance notification</ba>
                    <q_ba>low balance amount</q_ba>
                </notifications>
                <ah>authentication hash</ah> - *This parameter is optional
	        </arinpark_in>

	        The authentication hash will be a string generated using the input parameters. Using this value we will detect the method call has been made by a well known client.

        
        b.	Result: is also a string containing an xml with the result of the method:
                    <arinpark_out>
                        <r>Result of the method</r>
                        <un>Username</un>
                        <em>E-Mail</em>
                        <fs>First Surname</fs>
                        <ss>Second Surname</ss>
                        <na>Names</na>
                        <nif>NIF, NIE or CIF</nif>
                        <mmp>Main Mobile Phone</mmp>
                        <amp>Alternative Mobile Phone</amp>
                        <asn>Address: Street Name</asn>
                        <abn>Address: Building Number</abn>
                        <adf>Address: Department Floor</adf>
                        <add>Address: Department Door</add>
                        <ads>Address: Department Stair</ads>
                        <adl>Address: Department Letter or Number</adl>
                        <apc>Address: Postal Code</apc>
                        <aci>Address: City</aci>
                        <apr>Address: Province</apr>
                        <val>Validate new conditions is necessary</val>
                        <notifications>
	                        <fn>Fine notifications? (1:true, 0:false)</fn>
	                        <unp>UnParking notifications? (1:true, 0:false)</unp>
	                        <t_unp>minutes before the limit (unparking notifications)</t_unp>
	                        <re>recharge notifications? (1:true, 0:false)</re>
	                        <ba>low balance notification</ba>
                            <q_ba>low balance amount</q_ba>
                        </notifications>
                        <plates>
                            <plate>
                                <p>Plate</p>
                                <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                                <sp>sector</sp> ***
                            </plate>
                            <plate>
                                <p>Plate</p>
                                <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                                <sp>sector</sp> ***
                            </plate>
                            ...
                            <plate>
                                <p>Plate</p>
                                <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                                <sp>sector</sp> ***
                            </plate>
                        </plates>
	                </arinpark_out>

            *** Not required when plate status is rotative
         
            The tag <r> of the method will have these possible values:
            a.	>0: Mobile user id
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error
            g.	-20: Mobile user id not found
            h.	-21: User name already registered
            i.	-22: e-mail already registered
        * 
        * 
    */

        /// <summary>
        /// Método que actualiza la información asociada al usuario y que devuelve el id del mismo en caso de actualización correcta o -1 en caso contrario
        /// </summary>
        /// <param name="user">Objeto User con la información necesaria para el Update</param>
        /// <returns>Devuelve un objeto Result indicando si la respuesta ha sido correcta, el resultado (id de usuario) y si ha habido error</returns>
        [HttpPost]
        [Route("UpdateUserAPI")]
        public Result UpdateUserAPI([FromBody] User user)
        {
            //string xmlOut = "";
            int nMobileUserId = -1;
            Result response = new Result();

            SortedList notificationList = new SortedList();
            SortedList plateList = new SortedList();
            int numPlates = 0;
            SortedList parametersIn = new SortedList();
            PropertyInfo[] properties = typeof(User).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                if (NombreAtributo == "plates")
                {        
                    foreach (Plate plate in user.plates)
                    {
                        plateList.Add("p" + numPlates, plate.plate);
                        numPlates++;
                    }
                    parametersIn.Add("plates", plateList);
                }
                else if (NombreAtributo == "notifications")
                {
                    PropertyInfo[] propertiesNot = typeof(Notification).GetProperties();
                    foreach (PropertyInfo propertyNot in propertiesNot)
                    {
                        var attributeNot = propertyNot.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                        string NombreAtributoNot = (attribute == null) ? propertyNot.Name : attributeNot.DisplayName;
                        parametersIn.Add(NombreAtributoNot, propertyNot.GetValue(user.notifications));
                    }
                }
                else 
                {
                    var Valor = property.GetValue(user);
                    parametersIn.Add(NombreAtributo, Valor);
                }
            }

            try
            {
                //SortedList parametersIn = null;
                SortedList parametersOut = null;
                SortedList plateDataList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("UpdateUserAPI: parametersIn= {0}", parametersIn), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["em"] == null) || (parametersIn["em"].ToString().Length == 0) ||
                        (parametersIn["fs"] == null) || (parametersIn["fs"].ToString().Length == 0) ||
                        (parametersIn["na"] == null) || (parametersIn["na"].ToString().Length == 0) ||
                        (parametersIn["mmp"] == null) || (parametersIn["mmp"].ToString().Length == 0) ||
                        (parametersIn["val"] == null) || (parametersIn["val"].ToString().Length == 0) ||
                        (parametersIn["plates"] == null) || 
                        (parametersIn["fn"] == null) || (parametersIn["fn"].ToString().Length == 0) ||
                        (parametersIn["unp"] == null) || (parametersIn["unp"].ToString().Length == 0) ||
                        (parametersIn["t_unp"] == null) || (parametersIn["t_unp"].ToString().Length == 0) ||
                        (parametersIn["re"] == null) || (parametersIn["re"].ToString().Length == 0) ||
                        (parametersIn["ba"] == null) || (parametersIn["ba"].ToString().Length == 0) ||
                        (parametersIn["q_ba"] == null) || (parametersIn["q_ba"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Missing parameter: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                        response.IsSuccess = false;
                        response.Error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
                        response.Value = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else
                    {
                        bool bHashOk = false;

                        if (_useHash.Equals("true"))
                        {
                            string strCalculatedHash = CalculateHash(strHashString);
                            string strCalculatedHashJavaBouncyCastle = CalculateHashJavaBouncyCastle(strHashString);

                            if ((strCalculatedHash == strHash) && (strCalculatedHashJavaBouncyCastle == strHash))
                                bHashOk = true;
                        }
                        else
                            bHashOk = true;

                        if (!bHashOk)
                        {
                            //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_InvalidAuthenticationHash);
                            Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Bad hash: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                            response.IsSuccess = false;
                            response.Error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.Value = Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                            return response;
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            if (parametersIn["contid"] != null)
                            {
                                if (parametersIn["contid"].ToString().Trim().Length > 0)
                                    nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            }
                            // Set Contract Id to 0 to force all user queries to use the global users connection
                            nContractId = 0;

                            // Use token for verification
                            string strToken = parametersIn["mui"].ToString();

                            // Try to obtain user from token
                            nMobileUserId = GetUserFromToken(strToken, nContractId);

                            if (nMobileUserId <= 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Could not obtain user from token: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }
                            else
                                Logger_AddLogMessage(string.Format("UpdateUserXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Token not valid: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            if (CheckMobileUserName(parametersIn["mui"].ToString(), parametersIn["un"].ToString(), nContractId) != 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Mobile_User_Already_Registered);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - User name already registered: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Mobile_User_Already_Registered, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Mobile_User_Already_Registered).ToString();
                                return response;
                            }

                            if (CheckMobileUserEmail(parametersIn["mui"].ToString(), parametersIn["em"].ToString(), nContractId) != 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Mobile_User_Email_Already_Registered);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Email already registered: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Mobile_User_Email_Already_Registered, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Mobile_User_Email_Already_Registered).ToString();
                                return response;
                            }

                            nMobileUserId = ModifyMobileUser(parametersIn, nContractId);

                            if (nMobileUserId <= 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Failed to modify user: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }

                            Logger_AddLogMessage(string.Format("UpdateUserAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Get user data
                            if (!GetUserData(Convert.ToInt32(parametersIn["mui"]), out parametersOut, nContractId))
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Could not obtain user data: parametersIn= {0}, error={1}", parametersIn, "Result_Error_Generic"), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }

                            // Get parking data for assigned plates
                            if (!GetPlateData(Convert.ToInt32(parametersIn["mui"]), out plateDataList, nContractId))
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Could not obtain plate data: parametersIn= {0}, error={1}", parametersIn, "Result_Error_Generic"), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }

                            if (plateDataList.Count > 0)
                                parametersOut["plates"] = plateDataList;

                            parametersOut["r"] = nMobileUserId.ToString();
                            //xmlOut = GenerateXMLOuput(parametersOut);
                        }
                    }
                }
                else
                {
                    //xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Incorrect input format: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                    response.IsSuccess = false;
                    response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }
            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error: parametersIn= {0}, error={1}", parametersIn, "Result_Error_Generic"), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.IsSuccess = false;
                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            response.IsSuccess = true;
            response.Error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            response.Value = nMobileUserId.ToString();
            return response;//nMobileUserId;
            //return xmlOut;
        }

        /*
              * The parameters of method QueryUserOperationsXML are:

                 a.	xmlIn: xml containing input parameters of the method:
                     <arinpark_in>
                         <mui>Mobile user id (authorization token)</mui>
                         <d>Last x days from now</d>
                         <ots>Filter. List of Operation types 
                             <ot>(1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing)</ot>
                             …
                             <ot>(1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing)</ot>
                         </ots>   
                         <ah>authentication hash</ah> - *This parameter is optional
                     </arinpark_in>

                 b.	Result: is also a string containing an xml with the result of the method:
                     <arinpark_out>
                         <r>Result of the method</r>
                         <lst>
                             <o>
                                 <contname>Contract name</contname>
                                 <on>Operation number</on>
                                 <ot>Operation type (1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing, 104: Unpaid fines)</ot>
                                 <pl>Plate</pl>
                                 <zo>Zone</zo>
                                 <zonecolor>Zone color</zonecolor> **
                                 <sd>Parking start date (Format: hh24missddMMYY)</sd>
                                 <ed>Parking end date (Format: hh24missddMMYY)</ed>
                                 <pm>Payment method (1: Chip-Card, 2: Credit Card, 3: Cash, 4: Web, 5: Phone)</pm>
                                 <pp>Post-Paid (0: False, 1: True)</pp>
                                 <pa>Payment amount (Expressed in Euro cents)</pa>
                                 <fd>Fine payment date (Format: hh24missddMMYY)</fd> ***
                                 <fn>Fine number</fn>
                                 <fpd>Fine processing date (Format: hh24missddMMYY)</fpd>
                                 <fs>Fine status (1: Payable, 2:Expired, 3:Not payable)</fs> ****
                                 <farticle>Fine article</farticle>
                                 <fmake>Car make</fmake>
                                 <fcolor>Car color</fcolor>
                                 <fstreet>Fine street</fstreet>
                                 <fstrnum>Fine street number</fstrnum>
                                 <rd>Recharge date (Format: hh24missddMMYY)</rd> *****
                                 sta>status: 1 (UNPARKED), 2 (PARKED)</sta> ** 
                             </o>
                             <o>...</o>
                             ...
                             <o>...</o>
                         </lst>
                     </arinpark_out>

                     ** Only for parking operations.
                     *** Doubt: Only if the fine has been paid?
                     **** Status conditionated by server datetime.
                     ***** Only included if it the operation type is a recharge

                     The tag <r> of the method will have these possible values:
                     a.	1: User operations come after this tag
                     b.	-1: Invalid authentication hash
                     c.	-9: Generic Error (for example database or execution error.)
                     d.	-10: Invalid input parameter
                     e.	-11: Missing input parameter
                     f.	-12: OPS System error
                     g.	-20: Mobile user id not found

              */

        /// <summary>
        /// Método que obtiene las operaciones realizadas por un usuario
        /// </summary>
        /// <param name="userOperation">Objeto UserOperation con la información necesaria para obtener estas operaciones</param>
        /// <returns>Devuelve un objeto Result indicando si la respuesta ha sido correcta, el resultado (el listado de operaciones) y si ha habido error, con un listado vacio </returns>        
        [HttpPost]
        [Route("QueryUserOperationsAPI")]
        public Result QueryUserOperationsAPI([FromBody] UserOperation userOperation)
        {
            //string xmlOut = "";
            Result response = new Result();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();
            
            PropertyInfo[] properties = typeof(UserOperation).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                if (NombreAtributo == "ots")
                {
                    SortedList ops = new SortedList();
                    int numOps = 0;
                    foreach (int op in userOperation.operationTypeList)
                    {
                        ops.Add("op" + numOps, op);
                        numOps++;
                    }
                    parametersIn.Add(NombreAtributo, ops);
                }    
                else
                {
                    var Valor = property.GetValue(userOperation);
                    parametersIn.Add(NombreAtributo, Valor);
                }
            }

            try
            {
                //SortedList parametersIn = null;
                
                SortedList operationList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUserOperationsXML: parametersIn= {0}", parametersIn), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["d"] == null) || (parametersIn["d"].ToString() == "0") ||
                        (parametersIn["contid"] == null) || (parametersIn["contid"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Missing parameter: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                        response.IsSuccess = false;
                        response.Error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
                        response.Value = null;
                        return response;
                    }
                    else
                    {
                        bool bHashOk = false;

                        if (_useHash.Equals("true"))
                        {
                            string strCalculatedHash = CalculateHash(strHashString);
                            string strCalculatedHashJavaBouncyCastle = CalculateHashJavaBouncyCastle(strHashString);

                            if ((strCalculatedHash == strHash) && (strCalculatedHashJavaBouncyCastle == strHash))
                                bHashOk = true;
                        }
                        else
                            bHashOk = true;

                        if (!bHashOk)
                        {
                            //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_InvalidAuthenticationHash);
                            Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Bad hash: xmlIn= {0}", parametersIn), LoggerSeverities.Error);
                            response.IsSuccess = false;
                            response.Error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.Value = null;
                            return response;
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            if (parametersIn["contid"] != null)
                            {
                                if (parametersIn["contid"].ToString().Trim().Length > 0)
                                    nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            }

                            // Use token for verification
                            string strToken = parametersIn["mui"].ToString();

                            // *** TEMP - For testing purposes
                            int nMobileUserId = 0;
                            if (strToken.Length < 10)
                            {
                                nMobileUserId = Convert.ToInt32(strToken);
                            }
                            else
                            {
                                // Try to obtain user from token
                                // Send contract Id as 0 so that it uses the global users connection
                                nMobileUserId = GetUserFromToken(strToken, 0);

                                if (nMobileUserId <= 0)
                                {
                                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Could not obtain user from token: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                    //return xmlOut;
                                    response.IsSuccess = false;
                                    response.Error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.Value = null;
                                    return response;
                                }
                                else
                                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                // Determine if token is valid
                                TokenValidationResult tokenResult = DefaultVerification(strToken);

                                if (tokenResult != TokenValidationResult.Passed)
                                {
                                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Token not valid: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                    //return xmlOut;
                                    response.IsSuccess = false;
                                    response.Error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.Value = null;
                                    return response;
                                }
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get plate list for user
                            // Send contract Id as 0 so that it uses the global users connection
                            string strPlateList = "";
                            GetUserPlates(nMobileUserId, out strPlateList, 0);

                            // Get parking data for assigned plates
                            string strContractList = ConfigurationManager.AppSettings["ContractList"].ToString();
                            if (strContractList.Length == 0)
                                strContractList = "0," + parametersIn["contid"].ToString();
                            if (!GetUserOperationData(parametersIn, DATE_FORMAT_DAYS, out operationList, strContractList, strPlateList))
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Could not obtain operation data: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.Value = null;
                                return response;
                            }

                            if (operationList.Count > 0)
                                parametersOut["lst"] = operationList;

                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            //xmlOut = GenerateXMLOuput(parametersOut);
                        }
                    }
                }
                else
                {
                    //xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                    response.IsSuccess = false;
                    response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.Value = null;
                    return response;
                }
            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUserOperationsXML::Error: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.IsSuccess = false;
                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.Value = null;
                return response;
            }

            //return xmlOut;
            response.IsSuccess = true;
            response.Error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            List<Operation> lista = new List<Operation>();
            SortedList listOps = (SortedList)parametersOut["lst"];
            ConfigMapModel configMapModel = new ConfigMapModel();
            var config = configMapModel.configOperation();
            IMapper iMapper = config.CreateMapper();
            if (listOps != null) foreach (System.Collections.DictionaryEntry op in listOps) 
            {
                Operation ope = iMapper.Map<SortedList, Operation>((SortedList)op.Value);
                lista.Add(ope);
            } 
            response.Value = lista.ToArray();
            return response;
        }

        /*
 * 
 * The parameters of method QueryUserXML are:
    a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <mui>Mobile user id (authorization token)</mui>
                <ah>authentication hash</ah> - *This parameter is optional
            </arinpark_in>

    b.	Result: is also a string containing an xml with the result of the method:
            <arinpark_out>
                <r>Result of the method</r>
                <un>Username</un>
                <em>E-Mail</em>
                <fs>First Surname</fs>
                <ss>Second Surname</ss>
                <na>Names</na>
                <nif>NIF, NIE or CIF</nif>
                <mmp>Main Mobile Phone</mmp>
                <amp>Alternative Mobile Phone</amp>
                <asn>Address: Street Name</asn>
                <abn>Address: Building Number</abn>
                <adf>Address: Department Floor</adf>
                <add>Address: Department Door</add>
                <ads>Address: Department Stair</ads>
                <adl>Address: Department Letter or Number</adl>
                <apc>Address: Postal Code</apc>
                <aci>Address: City</aci>
                <apr>Address: Province</apr>
                <token_user>Token user ID</token_user>
                <token_id>Token ID</token_id>
                <notifications>
                    <fn>Fine notifications? (1:true, 0:false)</fn>
                    <unp>UnParking notifications? (1:true, 0:false)</unp>
                    <t_unp>minutes before the limit (unparking notifications)</t_unp>
                    <re>recharge notifications? (1:true, 0:false)</re>
                    … [Por definir]
                </notifications>
                <plates>
                    <plate>
                        <p>Plate</p>
                        <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                        <sp>sector</sp> ***
                    </plate>
                    <plate>
                        <p>Plate</p>
                        <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                        <sp>sector</sp> ***
                    </plate>
                    ...
                    <plate>
                        <p>Plate</p>
                        <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                        <sp>sector</sp> ***
                    </plate>
                </plates>
            </arinpark_out>

    *** Not required when plate status is rotative

    The tag <r> of the method will have these possible values:
    a.	1: User data come after this tag
    b.	-1: Invalid authentication hash
    c.	-9: Generic Error (for example database or execution error.)
    d.	-10: Invalid input parameter
    e.	-11: Missing input parameter
    f.	-12: OPS System error
    g.	-20: Mobile user id not found



 * 
 * 
 */

        /// <summary>
        /// Devuelve la información del usuario
        /// </summary>
        /// <param name="userQuery">Objeto de tipo UserQuery con la información necesaria</param>
        /// <returns>Devuelve un objeto Result indicando si la respuesta ha sido correcta, el resultado (datos del usuario) y si ha habido error</returns>
        [HttpPost]
        [Route("QueryUserAPI")]
        public Result QueryUserAPI([FromBody] UserQuery userQuery)
        {
            //string xmlOut = "";
            int nMobileUserId = -1;
            Result response = new Result();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            PropertyInfo[] properties = typeof(UserQuery).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(userQuery);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                //SortedList parametersOut = null;
                SortedList plateDataList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUserAPI: parametersIn= {0}", parametersIn), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["contid"] == null) || (parametersIn["contid"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Missing parameter: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                        response.IsSuccess = false;
                        response.Error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
                        response.Value = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else
                    {
                        bool bHashOk = false;

                        if (_useHash.Equals("true"))
                        {
                            string strCalculatedHash = CalculateHash(strHashString);
                            string strCalculatedHashJavaBouncyCastle = CalculateHashJavaBouncyCastle(strHashString);

                            if ((strCalculatedHash == strHash) && (strCalculatedHashJavaBouncyCastle == strHash))
                                bHashOk = true;
                        }
                        else
                            bHashOk = true;

                        if (!bHashOk)
                        {
                            //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_InvalidAuthenticationHash);
                            Logger_AddLogMessage(string.Format("QueryUserAPI::Error - Bad hash: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                            response.IsSuccess = false;
                            response.Error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.Value = Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                            return response;
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            if (parametersIn["contid"] != null)
                            {
                                if (parametersIn["contid"].ToString().Trim().Length > 0)
                                    nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            }
                            // Set Contract Id to 0 to force all user queries to use the global users connection
                            nContractId = 0;

                            // Use token for verification
                            string strToken = parametersIn["mui"].ToString();

                            // Try to obtain user from token
                            nMobileUserId = GetUserFromToken(strToken, nContractId);

                            if (nMobileUserId <= 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryUserAPI::Error - Could not obtain user from token: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryUserAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryUserAPI::Error - Token not valid: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get user data
                            if (!GetUserData(Convert.ToInt32(parametersIn["mui"]), out parametersOut, nContractId))
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserXML::Error - Could not obtain user data: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }

                            // Get parking data for assigned plates
                            if (!GetPlateData(Convert.ToInt32(parametersIn["mui"]), out plateDataList, nContractId))
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserXML::Error - Could not obtain plate data: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                //return xmlOut;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }

                            if (plateDataList.Count > 0)
                                parametersOut["plates"] = plateDataList;
                            else
                            {
                                plateDataList = new SortedList();
                                plateDataList["plate1"] = "";
                                parametersOut["plates"] = plateDataList;
                            }

                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            //xmlOut = GenerateXMLOuput(parametersOut);
                        }
                    }
                }
                else
                {
                    //xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryUserAPI::Error: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                    response.IsSuccess = false;
                    response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }
            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUserAPI::Error: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.IsSuccess = false;
                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            //return xmlOut;
            response.IsSuccess = true;
            response.Error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

            ConfigMapModel configMapModel = new ConfigMapModel();
            var configUser = configMapModel.configUser();
            IMapper iMapperUser = configUser.CreateMapper();
            User usu = iMapperUser.Map<SortedList, User>((SortedList)parametersOut);
            var configNot = configMapModel.configNotifications();
            IMapper iMapperNot = configNot.CreateMapper();
            SortedList notifications = (SortedList)parametersOut["notifications"];
            if (notifications != null)
            {
                Notification not = iMapperNot.Map<SortedList, Notification>(notifications);
                usu.notifications = not;
            }

            List<Plate> lista = new List<Plate>();
            SortedList listPlates = (SortedList)parametersOut["plates"];
            var configPlate = configMapModel.configPlate();
            IMapper iMapperPlate = configPlate.CreateMapper();
            if (listPlates != null) foreach (System.Collections.DictionaryEntry plate in listPlates)
            {
                Plate pl = iMapperPlate.Map<SortedList, Plate>((SortedList)plate.Value);
                lista.Add(pl);
            }
            usu.plates = lista.ToArray();
            response.Value = usu;

            return response;//nMobileUserId;
        }

        /*
         * 
         * The parameters of method RecoverPasswordXML are:
            a.	xmlIn: xml containing input parameters of the method:
                    <arinpark_in>
                        <un>User name or email</>
                        <contid>Contract ID</contid> - *This parameter is optional
                        <ah>authentication hash</ah> - *This parameter is optional
	                </arinpark_in>

            b.	Result: is an integer with the next possible values:
                a.	1: Email sent to user correctly
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error.)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
                g.  -20: Mobile user not found
         
         * 
         * 
         */

        [HttpPost]
        [Route("RecoverPasswordAPI")]
        public Result RecoverPasswordAPI([FromBody] UserRecover userRecover)
        {
            //string xmlOut = "";
            int nMobileUserId = -1;
            Result response = new Result();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            PropertyInfo[] properties = typeof(UserRecover).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(userRecover);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("RecoverPasswordAPI: parametersIn= {0}", parametersIn), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if (((parametersIn["un"] == null || (parametersIn["un"] != null && parametersIn["un"].ToString().Length == 0)) &&
                        (parametersIn["email"] == null) || (parametersIn["email"] != null && parametersIn["email"].ToString().Length == 0)) ||
                        (parametersIn["contid"] == null || (parametersIn["contid"].ToString().Length == 0)))
                    {
                        //iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error - Missing parameter: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                        response.IsSuccess = false;
                        response.Error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
                        response.Value = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else
                    {
                        bool bHashOk = false;

                        if (_useHash.Equals("true"))
                        {
                            string strCalculatedHash = CalculateHash(strHashString);
                            string strCalculatedHashJavaBouncyCastle = CalculateHashJavaBouncyCastle(strHashString);

                            if ((strCalculatedHash == strHash) && (strCalculatedHashJavaBouncyCastle == strHash))
                                bHashOk = true;
                        }
                        else
                            bHashOk = true;

                        if (!bHashOk)
                        {
                            //iRes = Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash);
                            Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error - Bad hash: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                            response.IsSuccess = false;
                            response.Error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.Value = Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                            return response;
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            if (parametersIn["contid"] != null)
                            {
                                if (parametersIn["contid"].ToString().Trim().Length > 0)
                                    nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            }
                            // Set Contract Id to 0 to force all user queries to use the global users connection
                            nContractId = 0;

                            // Try to obtain user ID from login, then from email
                            if (parametersIn["un"] != null)
                                nMobileUserId = GetUserFromLogin(parametersIn["un"].ToString(), nContractId);
                            if (nMobileUserId < 0)
                                nMobileUserId = GetUserFromEmail(parametersIn["email"].ToString(), nContractId);
                            if (nMobileUserId < 0)
                            {
                                Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error - Mobile user not found: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_Mobile_User_Not_Found;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_Mobile_User_Not_Found, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_Mobile_User_Not_Found).ToString();
                                return response;
                            }

                            Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Mobile user ID: {0}", nMobileUserId), LoggerSeverities.Info);

                            // Generate recovery code
                            string strRecoveryCode = RandomString(8);

                            // Assign code to user
                            if (!AssignRecoveryCode(nMobileUserId, strRecoveryCode, nContractId))
                            {
                                Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error - Could not assign recovery code {0} to user {1}", strRecoveryCode, nMobileUserId), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_OPS_Error;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_OPS_Error, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_OPS_Error).ToString();
                                return response;
                            }

                            Logger_AddLogMessage(string.Format("RecoverPasswordXML::Assigned recovery code {0} to user {1}", strRecoveryCode, nMobileUserId), LoggerSeverities.Info);

                            // Send email to user with recovery code
                            string strEmail = GetUserEmail(nMobileUserId, nContractId);
                            if (strEmail.Length <= 0)
                            {
                                Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error - Could not obtain email for user {0}", nMobileUserId), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_OPS_Error;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_OPS_Error, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_OPS_Error).ToString();
                                return response;
                            }

                            if (!SendRecoveryEmail(strRecoveryCode, strEmail))
                            {
                                Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error - Could not send email to user {0} at {1}", nMobileUserId, strEmail), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_OPS_Error;
                                response.IsSuccess = false;
                                response.Error = new Error((int)ResultType.Result_Error_OPS_Error, (int)SeverityError.Critical);
                                response.Value = Convert.ToInt32(ResultType.Result_Error_OPS_Error).ToString();
                                return response;
                            }

                            Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Email sent to user {0} at {1}", nMobileUserId, strEmail), LoggerSeverities.Info);

                            //iRes = (int)ResultType.Result_OK;
                        }
                    }
                }
                else
                {
                    //iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                    response.IsSuccess = false;
                    response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                    response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }
            }
            catch (Exception e)
            {
                //iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.IsSuccess = false;
                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            //return iRes;
            response.IsSuccess = true;
            response.Error = new Error((int)ResultType.Result_OK, (int)SeverityError.Critical);
            response.Value = Convert.ToInt32(ResultType.Result_OK).ToString();
            return response;
        }

        /*
             * 
             * The parameters of method LoginUserXML are:
             a.	xmlIn: xml containing input parameters of the method:
                 <arinpark_in>
                     <mui>Mobile user ID (authorization token)<mui> - ** Login is performed with token or combination of username/password
                     <un>Username</un> - **
                     <pw>Password</pw> - **
                     <v>App version</v>
                     <cid>Cloud ID. Used in ‘balance’ notifications because this notification is not associated with specific operation.<cid>
                     <os>Operative System (1: Android, 2: iOS)</os>
                     <contid>Contract ID</contid> - *This parameter is optional
                     <ah>authentication hash</ah> - *This parameter is optional
                 </arinpark_in>

                 The authentication hash will be a string generated using the input parameters. Using this value we will detect the method call has been made by a well known client.

                 If the login or email is sent along with the password, then an authorization token is generated and saved with the user information. This token is then returned as
                 the user ID.
                 If the token is used for the login, then it is verified with the current token value saved with the user information. If the token is no longer valid, but corresponds
                 with the previous token, then a new token is generated, saved and returned. However, if the token does not correspond to the previous one, then an error is returned.

             b.	Result: is a string with the possible values:
                 a.	>0: Mobile user id - the user ID is now the authorization token
                 b.	-1: Invalid authentication hash
                 c.	-9: Generic Error (for example database or execution error.)
                 d.	-10: Invalid input parameter
                 e.	-11: Missing input parameter
                 f.	-12: OPS System error
                 g.	-23: Login invalid
                 h.	-27: update required
                 i.  -29: validation required
             * 
             * 
         */

        /*
        [HttpPost]
        [Route("LoginUserXML")]
        public string LoginUserXML(string xmlIn)
        {
            string strToken = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();

            try
            {
                SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("LoginUserXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if (((parametersIn["un"] == null && parametersIn["pw"] == null) && parametersIn["mui"] == null) ||
                        (parametersIn["cid"] == null) ||
                        (parametersIn["os"] == null) ||
                        (parametersIn["v"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        Logger_AddLogMessage(string.Format("LoginUserXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                        return Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    }
                    else
                    {
                        bool bHashOk = false;

                        if (_useHash.Equals("true"))
                        {
                            string strCalculatedHash = CalculateHash(strHashString);
                            string strCalculatedHashJavaBouncyCastle = CalculateHashJavaBouncyCastle(strHashString);

                            if ((strCalculatedHash == strHash) && (strCalculatedHashJavaBouncyCastle == strHash))
                                bHashOk = true;
                        }
                        else
                            bHashOk = true;

                        if (!bHashOk)
                        {
                            Logger_AddLogMessage(string.Format("LoginUserXML::Error - Bad hash: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                            return Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            if (parametersIn["contid"] != null)
                            {
                                if (parametersIn["contid"].ToString().Trim().Length > 0)
                                    nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            }
                            // Set Contract Id to 0 to force all user queries to use the global users connection
                            nContractId = 0;

                            // Check application version
                            int nVersionResult = CheckApplicationVersion(parametersIn["v"].ToString(), nContractId);
                            if (nVersionResult == 0)
                            {
                                Logger_AddLogMessage(string.Format("LoginUserXML::Incorrect app version - update needed: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_App_Update_Required).ToString();
                            }
                            else if (nVersionResult < 0)
                            {
                                Logger_AddLogMessage(string.Format("LoginUserXML::Error - Could not verify version: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                            }

                            // Check to see if login is valid
                            bool bUseLogin = false;
                            bool bUseToken = false;
                            if (parametersIn["un"] != null)
                            {
                                if (parametersIn["un"].ToString().Length > 0)
                                    bUseLogin = true;
                            }
                            if (parametersIn["mui"] != null)
                            {
                                if (parametersIn["mui"].ToString().Length > 0)
                                    bUseToken = true;
                            }

                            // Use login for verification
                            if (bUseLogin)
                            {
                                int nMobileUserId = IsLoginValid(parametersIn["un"].ToString(), parametersIn["pw"].ToString(), nContractId);

                                if (nMobileUserId <= 0)
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserXML::Error - Could not validate user: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    return Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                }
                                else
                                    Logger_AddLogMessage(string.Format("LoginUserXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                // Check user validation
                                if (!IsUserValidated(nMobileUserId, nContractId))
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserXML::User not validated - needs to activate account: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    return Convert.ToInt32(ResultType.Result_Error_User_Not_Validated).ToString();
                                }

                                // Generate authorization token
                                strToken = GetNewToken();

                                if (!UpdateWebCredentials(nMobileUserId, parametersIn["cid"].ToString(), Convert.ToInt32(parametersIn["os"]), strToken, parametersIn["v"].ToString(), nContractId))
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserXML::Error - Could not update web credentials: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    return Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                }
                            }
                            else if (bUseToken)
                            {
                                // Use token for verification
                                strToken = parametersIn["mui"].ToString();

                                // Try to obtain user from token
                                int nMobileUserId = GetUserFromToken(strToken, nContractId);

                                if (nMobileUserId <= 0)
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    return Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                }
                                else
                                    Logger_AddLogMessage(string.Format("LoginUserXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                // Determine if token is valid
                                TokenValidationResult tokenResult = DefaultVerification(strToken);

                                if (tokenResult != TokenValidationResult.Passed && tokenResult != TokenValidationResult.TokenExpired)
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    return Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                }

                                // If token is valid, but has expired, issue a new token
                                if (tokenResult == TokenValidationResult.TokenExpired)
                                {
                                    strToken = GetNewToken();
                                    Logger_AddLogMessage(string.Format("LoginUserXML: Token expired, issued new one = {0}", strToken), LoggerSeverities.Info);
                                }

                                if (!UpdateWebCredentials(nMobileUserId, parametersIn["cid"].ToString(), Convert.ToInt32(parametersIn["os"]), strToken, parametersIn["v"].ToString(), nContractId))
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserXML::Error - Could not update web credentials: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    return Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                }
                            }
                        }
                    }
                }
                else
                {
                    Logger_AddLogMessage(string.Format("LoginUserXML::Error - Incorrect input format: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                    return Convert.ToInt32(rt).ToString();
                }
            }
            catch (Exception e)
            {
                strToken = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                Logger_AddLogMessage(string.Format("LoginUserXML::Error - {0}: xmlIn= {1}", e.Message, xmlIn), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return strToken;
        }
        */

        /*
        * 
        * The parameters of method UpdateUserXML are:
        a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <contid>Installation ID</contid>
                <mui>Mobile user Id (authorization token)</mui>
                <un>Username</un> - *This parameter is optional
                <pw>Password</pw> - *This parameter is optional
                <em>E-Mail</em>
                <fs>First Surname</fs>
                <ss>Second Surname</ss> - *This parameter is optional
                <na>Names</na>
                <nif>NIF, NIE or CIF</nif> - *This parameter is optional
                <mmp>Main Mobile Phone</mmp>
                <amp>Alternative Mobile Phone</amp> - *This parameter is optional
                <asn>Address: Street Name</asn> - *This parameter is optional
                <abn>Address: Building Number</abn> - *This parameter is optional
                <adf>Address: Department Floor</adf> - *This parameter is optional
                <add>Address: Department Door</add> - *This parameter is optional
                <ads>Address: Department Stair</ads> - *This parameter is optional
                <adl>Address: Department Letter or Number</adl> - *This parameter is optional
                <apc>Address: Postal Code</apc> - *This parameter is optional
                <aci>Address: City</aci> - *This parameter is optional
                <apr>Address: Province</apr> - *This parameter is optional
                <val>Validate new conditions is necessary</val> 
                <plates>
                    <p>Plate</p>
                    <p>Plate</p>
                    ...
                    <p>Plate</p>
                </plates>
                <notifications>
	                <fn>Fine notifications? (1:true, 0:false)</fn>
	                <unp>UnParking notifications? (1:true, 0:false)</unp>
	                <t_unp>minutes before the limit (unparking notifications)</t_unp>
	                <re>recharge notifications? (1:true, 0:false)</re>
	                <ba>low balance notification</ba>
                    <q_ba>low balance amount</q_ba>
                </notifications>
                <ah>authentication hash</ah> - *This parameter is optional
	        </arinpark_in>

	        The authentication hash will be a string generated using the input parameters. Using this value we will detect the method call has been made by a well known client.

        
        b.	Result: is also a string containing an xml with the result of the method:
                    <arinpark_out>
                        <r>Result of the method</r>
                        <un>Username</un>
                        <em>E-Mail</em>
                        <fs>First Surname</fs>
                        <ss>Second Surname</ss>
                        <na>Names</na>
                        <nif>NIF, NIE or CIF</nif>
                        <mmp>Main Mobile Phone</mmp>
                        <amp>Alternative Mobile Phone</amp>
                        <asn>Address: Street Name</asn>
                        <abn>Address: Building Number</abn>
                        <adf>Address: Department Floor</adf>
                        <add>Address: Department Door</add>
                        <ads>Address: Department Stair</ads>
                        <adl>Address: Department Letter or Number</adl>
                        <apc>Address: Postal Code</apc>
                        <aci>Address: City</aci>
                        <apr>Address: Province</apr>
                        <val>Validate new conditions is necessary</val>
                        <notifications>
	                        <fn>Fine notifications? (1:true, 0:false)</fn>
	                        <unp>UnParking notifications? (1:true, 0:false)</unp>
	                        <t_unp>minutes before the limit (unparking notifications)</t_unp>
	                        <re>recharge notifications? (1:true, 0:false)</re>
	                        <ba>low balance notification</ba>
                            <q_ba>low balance amount</q_ba>
                        </notifications>
                        <plates>
                            <plate>
                                <p>Plate</p>
                                <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                                <sp>sector</sp> ***
                            </plate>
                            <plate>
                                <p>Plate</p>
                                <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                                <sp>sector</sp> ***
                            </plate>
                            ...
                            <plate>
                                <p>Plate</p>
                                <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                                <sp>sector</sp> ***
                            </plate>
                        </plates>
	                </arinpark_out>

            *** Not required when plate status is rotative
         
            The tag <r> of the method will have these possible values:
            a.	>0: Mobile user id
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error
            g.	-20: Mobile user id not found
            h.	-21: User name already registered
            i.	-22: e-mail already registered
        * 
        * 
    */
        /*
        [HttpPost]
        [Route("UpdateUserXML")]
        public string UpdateUserXML(string xmlIn)
        {
            string xmlOut = "";

            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                SortedList plateDataList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("UpdateUserXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["em"] == null) ||
                        (parametersIn["fs"] == null) ||
                        (parametersIn["na"] == null) ||
                        (parametersIn["mmp"] == null) ||
                        (parametersIn["val"] == null) ||
                        (parametersIn["plates"] == null) ||
                        (parametersIn["fn"] == null) ||
                        (parametersIn["unp"] == null) ||
                        (parametersIn["t_unp"] == null) ||
                        (parametersIn["re"] == null) ||
                        (parametersIn["ba"] == null) ||
                        (parametersIn["q_ba"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UpdateUserXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                    }
                    else
                    {
                        bool bHashOk = false;

                        if (_useHash.Equals("true"))
                        {
                            string strCalculatedHash = CalculateHash(strHashString);
                            string strCalculatedHashJavaBouncyCastle = CalculateHashJavaBouncyCastle(strHashString);

                            if ((strCalculatedHash == strHash) && (strCalculatedHashJavaBouncyCastle == strHash))
                                bHashOk = true;
                        }
                        else
                            bHashOk = true;

                        if (!bHashOk)
                        {
                            xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_InvalidAuthenticationHash);
                            Logger_AddLogMessage(string.Format("UpdateUserXML::Error - Bad hash: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            if (parametersIn["contid"] != null)
                            {
                                if (parametersIn["contid"].ToString().Trim().Length > 0)
                                    nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            }
                            // Set Contract Id to 0 to force all user queries to use the global users connection
                            nContractId = 0;

                            // Use token for verification
                            string strToken = parametersIn["mui"].ToString();

                            // Try to obtain user from token
                            int nMobileUserId = GetUserFromToken(strToken, nContractId);

                            if (nMobileUserId <= 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("UpdateUserXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }
                            else
                                Logger_AddLogMessage(string.Format("UpdateUserXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("UpdateUserXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            if (CheckMobileUserName(parametersIn["mui"].ToString(), parametersIn["un"].ToString(), nContractId) != 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Mobile_User_Already_Registered);
                                Logger_AddLogMessage(string.Format("UpdateUserXML::Error - User name already registered: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (CheckMobileUserEmail(parametersIn["mui"].ToString(), parametersIn["em"].ToString(), nContractId) != 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Mobile_User_Email_Already_Registered);
                                Logger_AddLogMessage(string.Format("UpdateUserXML::Error - Email already registered: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            nMobileUserId = ModifyMobileUser(parametersIn, nContractId);

                            if (nMobileUserId <= 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("UpdateUserXML::Error - Failed to modify user: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            Logger_AddLogMessage(string.Format("UpdateUserXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Get user data
                            if (!GetUserData(Convert.ToInt32(parametersIn["mui"]), out parametersOut, nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("UpdateUserXML::Error - Could not obtain user data: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Get parking data for assigned plates
                            if (!GetPlateData(Convert.ToInt32(parametersIn["mui"]), out plateDataList, nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("UpdateUserXML::Error - Could not obtain plate data: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (plateDataList.Count > 0)
                                parametersOut["plates"] = plateDataList;

                            parametersOut["r"] = nMobileUserId.ToString();
                            xmlOut = GenerateXMLOuput(parametersOut);
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("UpdateUserXML::Error - Incorrect input format: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("UpdateUserXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }
        */

        /*
         * The parameters of method QueryUserOperationsXML are:

            a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <mui>Mobile user id (authorization token)</mui>
                    <d>Last x days from now</d>
                    <ots>Filter. List of Operation types 
                        <ot>(1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing)</ot>
                        …
                        <ot>(1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing)</ot>
                    </ots>   
                    <ah>authentication hash</ah> - *This parameter is optional
	            </arinpark_in>

            b.	Result: is also a string containing an xml with the result of the method:
                <arinpark_out>
                    <r>Result of the method</r>
                    <lst>
                        <o>
                            <contname>Contract name</contname>
                            <on>Operation number</on>
                            <ot>Operation type (1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing, 104: Unpaid fines)</ot>
                            <pl>Plate</pl>
                            <zo>Zone</zo>
                            <zonecolor>Zone color</zonecolor> **
                            <sd>Parking start date (Format: hh24missddMMYY)</sd>
                            <ed>Parking end date (Format: hh24missddMMYY)</ed>
                            <pm>Payment method (1: Chip-Card, 2: Credit Card, 3: Cash, 4: Web, 5: Phone)</pm>
                            <pp>Post-Paid (0: False, 1: True)</pp>
                            <pa>Payment amount (Expressed in Euro cents)</pa>
                            <fd>Fine payment date (Format: hh24missddMMYY)</fd> ***
                            <fn>Fine number</fn>
                            <fpd>Fine processing date (Format: hh24missddMMYY)</fpd>
                            <fs>Fine status (1: Payable, 2:Expired, 3:Not payable)</fs> ****
                            <farticle>Fine article</farticle>
                            <fmake>Car make</fmake>
                            <fcolor>Car color</fcolor>
                            <fstreet>Fine street</fstreet>
                            <fstrnum>Fine street number</fstrnum>
                            <rd>Recharge date (Format: hh24missddMMYY)</rd> *****
                            sta>status: 1 (UNPARKED), 2 (PARKED)</sta> ** 
                        </o>
                        <o>...</o>
                        ...
                        <o>...</o>
                    </lst>
	            </arinpark_out>

                ** Only for parking operations.
                *** Doubt: Only if the fine has been paid?
                **** Status conditionated by server datetime.
                ***** Only included if it the operation type is a recharge

                The tag <r> of the method will have these possible values:
                a.	1: User operations come after this tag
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error.)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
                g.	-20: Mobile user id not found

         */

        /*
        [HttpPost]
        [Route("QueryUserOperationsXML")]
        public string QueryUserOperationsXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = new SortedList();
                SortedList operationList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUserOperationsXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["d"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryUserOperationsXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                    }
                    else
                    {
                        bool bHashOk = false;

                        if (_useHash.Equals("true"))
                        {
                            string strCalculatedHash = CalculateHash(strHashString);
                            string strCalculatedHashJavaBouncyCastle = CalculateHashJavaBouncyCastle(strHashString);

                            if ((strCalculatedHash == strHash) && (strCalculatedHashJavaBouncyCastle == strHash))
                                bHashOk = true;
                        }
                        else
                            bHashOk = true;

                        if (!bHashOk)
                        {
                            xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_InvalidAuthenticationHash);
                            Logger_AddLogMessage(string.Format("QueryUserOperationsXML::Error - Bad hash: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            if (parametersIn["contid"] != null)
                            {
                                if (parametersIn["contid"].ToString().Trim().Length > 0)
                                    nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            }

                            // Use token for verification
                            string strToken = parametersIn["mui"].ToString();

                            // *** TEMP - For testing purposes
                            int nMobileUserId = 0;
                            if (strToken.Length < 10)
                            {
                                nMobileUserId = Convert.ToInt32(strToken);
                            }
                            else
                            {
                                // Try to obtain user from token
                                // Send contract Id as 0 so that it uses the global users connection
                                nMobileUserId = GetUserFromToken(strToken, 0);

                                if (nMobileUserId <= 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                    Logger_AddLogMessage(string.Format("QueryUserOperationsXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    return xmlOut;
                                }
                                else
                                    Logger_AddLogMessage(string.Format("QueryUserOperationsXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                // Determine if token is valid
                                TokenValidationResult tokenResult = DefaultVerification(strToken);

                                if (tokenResult != TokenValidationResult.Passed)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                    Logger_AddLogMessage(string.Format("QueryUserOperationsXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    return xmlOut;
                                }
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get plate list for user
                            // Send contract Id as 0 so that it uses the global users connection
                            string strPlateList = "";
                            GetUserPlates(nMobileUserId, out strPlateList, 0);

                            // Get parking data for assigned plates
                            string strContractList = ConfigurationManager.AppSettings["ContractList"].ToString();
                            if (strContractList.Length == 0)
                                strContractList = "0," + parametersIn["contid"].ToString();
                            if (!GetUserOperationData(parametersIn, DATE_FORMAT_DAYS, out operationList, strContractList, strPlateList))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserOperationsXML::Error - Could not obtain operation data: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (operationList.Count > 0)
                                parametersOut["lst"] = operationList;

                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            xmlOut = GenerateXMLOuput(parametersOut);
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryUserOperationsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUserOperationsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }
        */

        /*
 * 
 * The parameters of method QueryUserXML are:
    a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <mui>Mobile user id (authorization token)</mui>
                <ah>authentication hash</ah> - *This parameter is optional
            </arinpark_in>

    b.	Result: is also a string containing an xml with the result of the method:
            <arinpark_out>
                <r>Result of the method</r>
                <un>Username</un>
                <em>E-Mail</em>
                <fs>First Surname</fs>
                <ss>Second Surname</ss>
                <na>Names</na>
                <nif>NIF, NIE or CIF</nif>
                <mmp>Main Mobile Phone</mmp>
                <amp>Alternative Mobile Phone</amp>
                <asn>Address: Street Name</asn>
                <abn>Address: Building Number</abn>
                <adf>Address: Department Floor</adf>
                <add>Address: Department Door</add>
                <ads>Address: Department Stair</ads>
                <adl>Address: Department Letter or Number</adl>
                <apc>Address: Postal Code</apc>
                <aci>Address: City</aci>
                <apr>Address: Province</apr>
                <token_user>Token user ID</token_user>
                <token_id>Token ID</token_id>
                <notifications>
                    <fn>Fine notifications? (1:true, 0:false)</fn>
                    <unp>UnParking notifications? (1:true, 0:false)</unp>
                    <t_unp>minutes before the limit (unparking notifications)</t_unp>
                    <re>recharge notifications? (1:true, 0:false)</re>
                    … [Por definir]
                </notifications>
                <plates>
                    <plate>
                        <p>Plate</p>
                        <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                        <sp>sector</sp> ***
                    </plate>
                    <plate>
                        <p>Plate</p>
                        <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                        <sp>sector</sp> ***
                    </plate>
                    ...
                    <plate>
                        <p>Plate</p>
                        <stp>status (1:Rotative, 2:Resident, 3:VIP</stp>
                        <sp>sector</sp> ***
                    </plate>
                </plates>
            </arinpark_out>

    *** Not required when plate status is rotative

    The tag <r> of the method will have these possible values:
    a.	1: User data come after this tag
    b.	-1: Invalid authentication hash
    c.	-9: Generic Error (for example database or execution error.)
    d.	-10: Invalid input parameter
    e.	-11: Missing input parameter
    f.	-12: OPS System error
    g.	-20: Mobile user id not found



 * 
 * 
 */

        /*
        [HttpPost]
        [Route("QueryUserXML")]
        public string QueryUserXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                SortedList plateDataList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUserXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) ||
                        (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UpdateUserXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                    }
                    else
                    {
                        bool bHashOk = false;

                        if (_useHash.Equals("true"))
                        {
                            string strCalculatedHash = CalculateHash(strHashString);
                            string strCalculatedHashJavaBouncyCastle = CalculateHashJavaBouncyCastle(strHashString);

                            if ((strCalculatedHash == strHash) && (strCalculatedHashJavaBouncyCastle == strHash))
                                bHashOk = true;
                        }
                        else
                            bHashOk = true;

                        if (!bHashOk)
                        {
                            xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_InvalidAuthenticationHash);
                            Logger_AddLogMessage(string.Format("QueryUserXML::Error - Bad hash: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            if (parametersIn["contid"] != null)
                            {
                                if (parametersIn["contid"].ToString().Trim().Length > 0)
                                    nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            }
                            // Set Contract Id to 0 to force all user queries to use the global users connection
                            nContractId = 0;

                            // Use token for verification
                            string strToken = parametersIn["mui"].ToString();

                            // Try to obtain user from token
                            int nMobileUserId = GetUserFromToken(strToken, nContractId);

                            if (nMobileUserId <= 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryUserXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryUserXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryUserXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get user data
                            if (!GetUserData(Convert.ToInt32(parametersIn["mui"]), out parametersOut, nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserXML::Error - Could not obtain user data: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Get parking data for assigned plates
                            if (!GetPlateData(Convert.ToInt32(parametersIn["mui"]), out plateDataList, nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserXML::Error - Could not obtain plate data: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (plateDataList.Count > 0)
                                parametersOut["plates"] = plateDataList;
                            else
                            {
                                plateDataList = new SortedList();
                                plateDataList["plate1"] = "";
                                parametersOut["plates"] = plateDataList;
                            }

                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            xmlOut = GenerateXMLOuput(parametersOut);
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryUserXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUserXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }
        */

        /*
         * 
         * The parameters of method RecoverPasswordXML are:
            a.	xmlIn: xml containing input parameters of the method:
                    <arinpark_in>
                        <un>User name or email</>
                        <contid>Contract ID</contid> - *This parameter is optional
                        <ah>authentication hash</ah> - *This parameter is optional
	                </arinpark_in>

            b.	Result: is an integer with the next possible values:
                a.	1: Email sent to user correctly
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error.)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
                g.  -20: Mobile user not found
         
         * 
         * 
         */

        /*
        [HttpPost]
        [Route("RecoverPasswordXML")]
        public int RecoverPasswordXML(string xmlIn)
        {
            int iRes = 0;
            try
            {
                SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("RecoverPasswordXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["un"] == null) ||
                        (parametersIn["un"].ToString().Length == 0) ||
                        (parametersIn["contid"] == null))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("RecoverPasswordXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                    }
                    else
                    {
                        bool bHashOk = false;

                        if (_useHash.Equals("true"))
                        {
                            string strCalculatedHash = CalculateHash(strHashString);
                            string strCalculatedHashJavaBouncyCastle = CalculateHashJavaBouncyCastle(strHashString);

                            if ((strCalculatedHash == strHash) && (strCalculatedHashJavaBouncyCastle == strHash))
                                bHashOk = true;
                        }
                        else
                            bHashOk = true;

                        if (!bHashOk)
                        {
                            iRes = Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash);
                            Logger_AddLogMessage(string.Format("RecoverPasswordXML::Error - Bad hash: xmlIn= {0}, xmlOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            if (parametersIn["contid"] != null)
                            {
                                if (parametersIn["contid"].ToString().Trim().Length > 0)
                                    nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            }
                            // Set Contract Id to 0 to force all user queries to use the global users connection
                            nContractId = 0;

                            // Try to obtain user ID from login, then from email
                            int nMobileUserId = GetUserFromLogin(parametersIn["un"].ToString(), nContractId);
                            if (nMobileUserId < 0)
                                nMobileUserId = GetUserFromEmail(parametersIn["un"].ToString(), nContractId);
                            if (nMobileUserId < 0)
                            {
                                Logger_AddLogMessage(string.Format("RecoverPasswordXML::Error - Mobile user not found: xmlIn= {0}, xmlOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                return (int)ResultType.Result_Error_Mobile_User_Not_Found;
                            }

                            Logger_AddLogMessage(string.Format("RecoverPasswordXML::Mobile user ID: {0}", nMobileUserId), LoggerSeverities.Info);

                            // Generate recovery code
                            string strRecoveryCode = RandomString(8);

                            // Assign code to user
                            if (!AssignRecoveryCode(nMobileUserId, strRecoveryCode, nContractId))
                            {
                                Logger_AddLogMessage(string.Format("RecoverPasswordXML::Error - Could not assign recovery code {0} to user {1}", strRecoveryCode, nMobileUserId), LoggerSeverities.Error);
                                return (int)ResultType.Result_Error_OPS_Error;
                            }

                            Logger_AddLogMessage(string.Format("RecoverPasswordXML::Assigned recovery code {0} to user {1}", strRecoveryCode, nMobileUserId), LoggerSeverities.Info);

                            // Send email to user with recovery code
                            string strEmail = GetUserEmail(nMobileUserId, nContractId);
                            if (strEmail.Length <= 0)
                            {
                                Logger_AddLogMessage(string.Format("RecoverPasswordXML::Error - Could not obtain email for user {0}", nMobileUserId), LoggerSeverities.Error);
                                return (int)ResultType.Result_Error_OPS_Error;
                            }

                            if (!SendRecoveryEmail(strRecoveryCode, strEmail))
                            {
                                Logger_AddLogMessage(string.Format("RecoverPasswordXML::Error - Could not send email to user {0} at {1}", nMobileUserId, strEmail), LoggerSeverities.Error);
                                return (int)ResultType.Result_Error_OPS_Error;
                            }

                            Logger_AddLogMessage(string.Format("RecoverPasswordXML::Email sent to user {0} at {1}", nMobileUserId, strEmail), LoggerSeverities.Info);

                            iRes = (int)ResultType.Result_OK;
                        }
                    }
                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("RecoverPasswordXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("RecoverPasswordXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return iRes;
        }
        */

        /*
            * 
            * The parameters of method ChangePasswordXML are:
            a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <un>User name or email</un>
                    <pw>Password</pw>
                    <recode>Recovery code</recode>
                    <contid>Contract ID</contid> - *This parameter is optional
                    <ah>authentication hash</ah> - *This parameter is optional
                </arinpark_in>
                
            b.	Result: is a string with the possible values:
                a.	>0: New authorization token
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error.)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
                g.  -20: Mobile user not found
                h.  -31: Recovery code not found
                i.  -32: Invalid recovery code
                j.  -33: Recovery code expired
            * 
            * 
        */

        [HttpPost]
        [Route("ChangePasswordXML")]
        public string ChangePasswordXML(string xmlIn)
        {
            string strToken = ResultType.Result_Error_Generic.ToString();

            try
            {
                SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("ChangePasswordXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if (parametersIn["un"] == null ||
                        parametersIn["pw"] == null ||
                        parametersIn["recode"] == null ||
                        parametersIn["contid"] == null)
                    {
                        Logger_AddLogMessage(string.Format("ChangePasswordXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                        return Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    }
                    else
                    {
                        bool bHashOk = false;

                        if (_useHash.Equals("true"))
                        {
                            string strCalculatedHash = CalculateHash(strHashString);
                            string strCalculatedHashJavaBouncyCastle = CalculateHashJavaBouncyCastle(strHashString);

                            if ((strCalculatedHash == strHash) && (strCalculatedHashJavaBouncyCastle == strHash))
                                bHashOk = true;
                        }
                        else
                            bHashOk = true;

                        if (!bHashOk)
                        {
                            Logger_AddLogMessage(string.Format("ChangePasswordXML::Error - Bad hash: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                            return Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            if (parametersIn["contid"] != null)
                            {
                                if (parametersIn["contid"].ToString().Trim().Length > 0)
                                    nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            }
                            // Set Contract Id to 0 to force all user queries to use the global users connection
                            nContractId = 0;

                            // Try to obtain user ID from login, then from email
                            int nMobileUserId = GetUserFromLogin(parametersIn["un"].ToString(), nContractId);
                            if (nMobileUserId < 0)
                                nMobileUserId = GetUserFromEmail(parametersIn["un"].ToString(), nContractId);
                            if (nMobileUserId < 0)
                            {
                                string xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Mobile_User_Not_Found);
                                Logger_AddLogMessage(string.Format("ChangePasswordXML::Error - Mobile user not found: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }
                            Logger_AddLogMessage(string.Format("ChangePasswordXML::Mobile user ID: {0}", nMobileUserId), LoggerSeverities.Info);

                            // Get current recovery code
                            string strCurRecoveryCode = GetUserRecoveryCode(nMobileUserId, nContractId);
                            if (strCurRecoveryCode.Length <= 0)
                            {
                                Logger_AddLogMessage(string.Format("ChangePasswordXML::Error - No recovery password was found for user {0}", nMobileUserId), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Not_Found).ToString();
                            }

                            // Verify recovery code
                            if (!strCurRecoveryCode.Equals(parametersIn["recode"].ToString()))
                            {
                                Logger_AddLogMessage(string.Format("ChangePasswordXML::Error - Received recovery code {0} does not match current recovery code {1} for user {2}", parametersIn["recode"].ToString(), strCurRecoveryCode, nMobileUserId), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Invalid).ToString();
                            }

                            // Check recovery code expiration date
                            if (!VerifyRecoveryCode(nMobileUserId, strCurRecoveryCode, nContractId))
                            {
                                Logger_AddLogMessage(string.Format("ChangePasswordXML::Error - Received recovery code {0} has expired for user {1}", strCurRecoveryCode, nMobileUserId), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Expired).ToString();
                            }

                            // Generate authorization token
                            strToken = GetNewToken();

                            if (!UpdateWebCredentials(nMobileUserId, strToken, parametersIn["pw"].ToString(), nContractId))
                            {
                                Logger_AddLogMessage(string.Format("ChangePasswordXML::Error - Could not update web credentials: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                            }
                        }
                    }
                }
                else
                {
                    Logger_AddLogMessage(string.Format("ChangePasswordXML::Error - Incorrect input format: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                    return Convert.ToInt32(rt).ToString();
                }
            }
            catch (Exception e)
            {
                strToken = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                Logger_AddLogMessage(string.Format("ChangePasswordXML::Error - {0}: xmlIn= {1}", e.Message, xmlIn), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return strToken;
        }

        #region private methods

        private string CalculateHash(string strInput)
        {
            string xmlOut = "";
            try
            {
                if (_mac3des != null)
                {
                    byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(strInput);
                    byte[] hash = _mac3des.ComputeHash(inputBytes);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }
                    xmlOut = sb.ToString();
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("CalculateHash::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        private string CalculateHashJavaBouncyCastle(string strInput)
        {
            string xmlOut = "";
            try
            {
                if (_mac3des != null)
                {
                    byte[] inputBytesTemp = System.Text.Encoding.UTF8.GetBytes(strInput);
                    byte[] inputBytes = null;

                    if (inputBytesTemp.Length % 8 == 0)
                    {
                        inputBytes = new byte[inputBytesTemp.Length + 8];
                        Array.Clear(inputBytes, 0, inputBytes.Length);
                        Array.Copy(inputBytesTemp, inputBytes, inputBytesTemp.Length);
                    }
                    else
                    {
                        inputBytes = new byte[inputBytesTemp.Length];
                        Array.Copy(inputBytesTemp, inputBytes, inputBytesTemp.Length);
                    }

                    byte[] hash = _mac3des.ComputeHash(inputBytes);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }
                    xmlOut = sb.ToString();
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("CalculateHashJavaBouncyCastle::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        private string CalculatePartnerHash(string strInput)
        {
            string xmlOut = "";
            try
            {
                if (_partnerMac3des != null)
                {
                    byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(strInput);
                    byte[] hash = _partnerMac3des.ComputeHash(inputBytes);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }
                    xmlOut = sb.ToString();
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("CalculatePartnerHash::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        private ResultType FindInputParametersAPI(SortedList parameters, out string strHash, out string strHashString)
        {
            //HttpWebRequest request = new HttpWebRequest();
            //System.Collections.Specialized.NameValueCollection parametersIn = request.RequestUri.ParseQueryString();

            SortedList parametersNew = new SortedList();

            ResultType rtRes = ResultType.Result_OK;
            //parameters = new SortedList();
            strHash = "";
            strHashString = "";

            try
            {
                //XmlDocument xmlInDoc = new XmlDocument();

                string[] values = null;
                //xmlInDoc.LoadXml(xmlIn);
                //XmlNodeList Nodes = xmlInDoc.SelectNodes("//" + _xmlTagName + IN_SUFIX + "/*");

                //foreach (XmlNode Node in Nodes)
                foreach (string key in parameters.Keys)
                {
                    if (parameters[key] != null)
                    {
                        
                        switch (key)
                        {
                            case "plates":
                                //int nNumPlates = 0;
                                //SortedList PlateList = new SortedList();
                                //XmlNodeList PlateNodes = Node.ChildNodes; 
                                SortedList PlateList = (SortedList)parameters[key];
                                foreach (var PlateNode in PlateList.Values)
                                {
                                    strHashString += (string)PlateNode;
                                    //nNumPlates++;
                                    //if (PlateNode.Name.Equals("p"))
                                    //PlateList["p" + nNumPlates.ToString()] = PlateNode.p.Trim();
                                }
                                //if (PlateList.Count > 0)
                                //    parameters[key] = PlateList;
                                break;
                            case "notifications":
                                //XmlNodeList NotifNodes = Node.ChildNodes;
                                //foreach (Notifications NotifNode in parametersN)
                                //{
                                SortedList notList = (SortedList)parameters[key];
                                foreach (var notNode in notList.Values)
                                {
                                    strHashString += (string)notNode;
                                    //nNumPlates++;
                                    //if (PlateNode.Name.Equals("p"))
                                    //PlateList["p" + nNumPlates.ToString()] = PlateNode.p.Trim();
                                }
                                //Notifications NotifNode = (Notifications)parameters[key];
                                //strHashString += NotifNode.ba + NotifNode.fn + NotifNode.q_ba + NotifNode.re + NotifNode.t_unp + NotifNode.unp;
                                //parameters[key] = NotifNode;
                                //}
                                break;
                            case "ots":
                                //List<string> parametersN = ((string)parameters[key]).Split(',').ToList();
                                //int nNumOts = 0;
                                SortedList OtsList = (SortedList)parameters[key]; ;
                                //XmlNodeList OtsNodes = Node.ChildNodes;
                                foreach (var OtsNode in OtsList.Values)
                                {
                                    strHashString += OtsNode;
                                    //nNumOts++;
                                    //OtsList["ot" + nNumOts.ToString()] = OtsNode.Trim();
                                }
                                //if (OtsList.Count > 0)
                                //    parameters[key] = OtsList;
                                break;
                            case "ah":
                                strHash = parameters[key].ToString().Trim();
                                break;
                            default:
                                strHashString += parameters[key];
                                //parameters[key] = parameters[key].ToString().Trim();
                                break;

                        }

                    }
                }



            }
            catch (Exception e)
            {
                rtRes = ResultType.Result_Error_Generic;
                Logger_AddLogMessage("FindInputParameters::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return rtRes;
        }

        private ResultType FindInputParameters(string xmlIn, out SortedList parameters, out string strHash, out string strHashString)
        {
            ResultType rtRes = ResultType.Result_OK;
            parameters = new SortedList();
            strHash = "";
            strHashString = "";

            try
            {
                XmlDocument xmlInDoc = new XmlDocument();
                try
                {
                    xmlInDoc.LoadXml(xmlIn);
                    XmlNodeList Nodes = xmlInDoc.SelectNodes("//" + _xmlTagName + IN_SUFIX + "/*");
                    int nNumPlates = 0;
                    foreach (XmlNode Node in Nodes)
                    {
                        switch (Node.Name)
                        {
                            case "plates":
                                if (nNumPlates == 0)
                                {
                                    SortedList PlateList = new SortedList();
                                    XmlNodeList PlateNodes = Node.ChildNodes;
                                    foreach (XmlNode PlateNode in PlateNodes)
                                    {
                                        strHashString += PlateNode.InnerText;
                                        nNumPlates++;
                                        //if (PlateNode.Name.Equals("p"))
                                        PlateList["p" + nNumPlates.ToString()] = PlateNode.InnerText.Trim();
                                    }
                                    if (PlateList.Count > 0)
                                        parameters[Node.Name] = PlateList;
                                }
                                else
                                {
                                    SortedList PlateList = (SortedList)parameters["plates"];
                                    XmlNodeList PlateNodes = Node.ChildNodes;
                                    foreach (XmlNode PlateNode in PlateNodes)
                                    {
                                        strHashString += PlateNode.InnerText;
                                        nNumPlates++;
                                        PlateList["p" + nNumPlates.ToString()] = PlateNode.InnerText.Trim();
                                    }
                                    if (PlateList.Count > 0)
                                        parameters[Node.Name] = PlateList;
                                }
                                break;
                            case "notifications":
                                XmlNodeList NotifNodes = Node.ChildNodes;
                                foreach (XmlNode NotifNode in NotifNodes)
                                {
                                    strHashString += NotifNode.InnerText;
                                    parameters[NotifNode.Name] = NotifNode.InnerText.Trim();
                                }
                                break;
                            case "ots":
                                int nNumOts = 0;
                                SortedList OtsList = new SortedList();
                                XmlNodeList OtsNodes = Node.ChildNodes;
                                foreach (XmlNode OtsNode in OtsNodes)
                                {
                                    strHashString += OtsNode.InnerText;
                                    nNumOts++;
                                    OtsList["ot" + nNumOts.ToString()] = OtsNode.InnerText.Trim();
                                }
                                if (OtsList.Count > 0)
                                    parameters[Node.Name] = OtsList;
                                break;
                            case "ah":
                                strHash = Node.InnerText.Trim();
                                break;
                            default:
                                strHashString += Node.InnerText;
                                parameters[Node.Name] = Node.InnerText.Trim();
                                break;

                        }

                    }

                    if (Nodes.Count == 0)
                    {
                        Logger_AddLogMessage(string.Format("FindInputParameters: Bad Input XML: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                        rtRes = ResultType.Result_Error_Invalid_Input_Parameter;

                    }


                }
                catch
                {
                    Logger_AddLogMessage(string.Format("FindInputParameters: Bad Input XML: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                    rtRes = ResultType.Result_Error_Invalid_Input_Parameter;
                }

            }
            catch (Exception e)
            {
                rtRes = ResultType.Result_Error_Generic;
                Logger_AddLogMessage("FindInputParameters::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return rtRes;
        }

        private int CheckApplicationVersion(string strUserVersion, int nContractId = 0)
        {
            int nRes = (int)ResultType.Result_Error_Generic;
            string strCurVersion = "0";
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT PAR_VALUE FROM PARAMETERS WHERE PAR_DESCSHORT = '{0}' AND PAR_VALID = 1 AND PAR_DELETED = 0", ConfigurationManager.AppSettings["Parameter.AppVersion"].ToString());
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strCurVersion = dataReader.GetString(0);

                    int nResult = String.CompareOrdinal(strCurVersion, strUserVersion);

                    if (nResult <= 0)
                        nRes = (int)ResultType.Result_OK;
                    else
                        nRes = 0;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("CheckApplicationVersion::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return nRes;
        }

        private bool IsUserValidated(int nUserId, int nContractId = 0)
        {
            bool bRes = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT MU_ACTIVATE_ACCOUNT FROM MOBILE_USERS WHERE MU_ID = {0} AND MU_VALID = 1 AND MU_DELETED = 0", nUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    int nAccountActivated = dataReader.GetInt32(0);
                    if (nAccountActivated == 1)
                        bRes = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("IsUserValidated::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bRes;
        }

        private int IsLoginValid(string strUserId, string strPassword, int nContractId = 0)
        {
            int nMobileUserId = (int)ResultType.Result_Error_Generic;
            string strEncrpytedPassword = "";
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = "";

                // User can log in with login name or email
                if (strUserId.Contains('@'))
                    strSQL = string.Format("SELECT MU_ID, MU_PASSWORD FROM MOBILE_USERS WHERE MU_EMAIL = '{0}' AND MU_VALID = 1 AND MU_DELETED = 0", strUserId);
                else
                    strSQL = string.Format("SELECT MU_ID, MU_PASSWORD FROM MOBILE_USERS WHERE MU_LOGIN = '{0}' AND MU_VALID = 1 AND MU_DELETED = 0", strUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nMobileUserId = dataReader.GetInt32(0);
                    strEncrpytedPassword = dataReader.GetString(1);
                }

                if (nMobileUserId > 0)
                {
                    string strDecrpytedPassword = Decrypt(nMobileUserId, strEncrpytedPassword);
                    if (!strPassword.Equals(strDecrpytedPassword))
                        nMobileUserId = (int)ResultType.Result_Error_Generic;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("IsLoginValid::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return nMobileUserId;
        }

        private int GetUserFromToken(string strTokenId, int nContractId = 0)
        {
            int nMobileUserId = (int)ResultType.Result_Error_Generic;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT MU_ID FROM MOBILE_USERS WHERE MU_AUTH_TOKEN = '{0}' AND MU_VALID = 1 AND MU_DELETED = 0", strTokenId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nMobileUserId = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserFromToken::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return nMobileUserId;
        }

        private int GetUserFromLogin(string strLogin, int nContractId = 0)
        {
            int nMobileUserId = (int)ResultType.Result_Error_Generic;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT MU_ID FROM MOBILE_USERS WHERE MU_LOGIN = '{0}' AND MU_VALID = 1 AND MU_DELETED = 0", strLogin);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nMobileUserId = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserFromLogin::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return nMobileUserId;
        }

        private int GetUserFromEmail(string strEmail, int nContractId = 0)
        {
            int nMobileUserId = (int)ResultType.Result_Error_Generic;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT MU_ID FROM MOBILE_USERS WHERE MU_EMAIL = '{0}' AND MU_VALID = 1 AND MU_DELETED = 0", strEmail);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nMobileUserId = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserFromEmail::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return nMobileUserId;
        }

        private int IsValidMobileUserId(int nMobileUserId, int nContractId = 0)
        {
            int nResult = -1;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT COUNT(*) FROM MOBILE_USERS WHERE MU_ID = {0} AND MU_VALID = 1 AND MU_DELETED = 0", nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nResult = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("IsValidMobileUserId::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return nResult;
        }

        private bool UpdateWebCredentials(int nMobileUserId, string strCloudId, int nOs, string strToken, string strVersion, int nContractId = 0)
        {
            bool bResult = false;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("update MOBILE_USERS set mu_cloud_token = '{0}', mu_device_os = {1}, mu_auth_token = '{2}', mu_app_version = '{3}' WHERE MU_ID = {4}", strCloudId, nOs, strToken, strVersion, nMobileUserId);

                oraCmd.CommandText = strSQL;

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("UpdateWebCredentials::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        private bool UpdateWebCredentials(int nMobileUserId, string strToken, string strPassword, int nContractId = 0)
        {
            bool bResult = false;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("update MOBILE_USERS set mu_auth_token = '{0}', mu_password = '{1}' WHERE MU_ID = {2}", strToken, strPassword, nMobileUserId);

                oraCmd.CommandText = strSQL;

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("UpdateWebCredentials::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        public string GetNewToken()
        {
            var jot = new JotProvider();

            var token = jot.Create();

            return jot.Encode(token);
        }

        public TokenValidationResult DefaultVerification(string encodedTokenFromWebPage)
        {
            var jot = new JotProvider();

            return jot.Validate(encodedTokenFromWebPage);
        }

        private bool GetUserOperationData(SortedList parametersIn, int nDateFormat, out SortedList operationList, string strContractList, string strPlateList)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            string strFilterList = "";
            int nNumFilters = 0;
            int nNumOperations = 0;
            bool bListFinePayments = false;
            bool bListFines = false;
            bool bListOperations = false;
            string strSQLSelect = "";
            string strSQLWhere = "";
            int nNumDays = 0;
            bool bUseHistoricData = false;
            string strContractName = "";

            operationList = new SortedList();
            string sConn = "";

            try
            {
                string[] strContractListSplit = strContractList.Split(new char[] { ',' });

                foreach (string strContractId in strContractListSplit)
                {
                    sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                    if (!strContractId.Equals("0"))
                        sConn = ConfigurationManager.AppSettings["ConnectionString" + strContractId].ToString();

                    // Get contract name
                    GetParameter("P_SYSTEM_NAME", out strContractName, Convert.ToInt32(strContractId));

                    oraConn = new OracleConnection(sConn);

                    oraCmd = new OracleCommand();
                    oraCmd.Connection = oraConn;
                    oraCmd.Connection.Open();

                    if (oraCmd == null)
                        throw new Exception("Oracle command is null");

                    // Conexion BBDD?
                    if (oraCmd.Connection == null)
                        throw new Exception("Oracle connection is null");

                    if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                        throw new Exception("Oracle connection is not open");

                    // Get the operation filters
                    if (parametersIn["ots"] != null)
                    {
                        SortedList filterList = (SortedList)parametersIn["ots"];
                        foreach (DictionaryEntry item in filterList)
                        {
                            if (item.Value.ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.UnpaidFines"].ToString()))
                                bListFines = true;
                            else if (item.Value.ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.Payment"].ToString()))
                                bListFinePayments = true;
                            else
                            {
                                if (nNumFilters >= 1)
                                    strFilterList += ", ";
                                strFilterList += item.Value.ToString();
                                nNumFilters++;
                                bListOperations = true;
                            }
                        }
                    }
                    else
                    {
                        bListFines = true;
                        bListFinePayments = true;
                        bListOperations = true;
                    }

                    // Determine if using historic or current tables
                    if (nDateFormat == DATE_FORMAT_DAYS)
                        nNumDays = Convert.ToInt32(parametersIn["d"].ToString());
                    else
                    {
                        DateTime dtStartDate = DateTime.ParseExact(parametersIn["d1"].ToString(), "HHmmssddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                        DateTime dtEndDate = DateTime.ParseExact(parametersIn["d2"].ToString(), "HHmmssddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                        TimeSpan tsRangeStart = DateTime.Now - dtStartDate;
                        TimeSpan tsRangeEnd = DateTime.Now - dtEndDate;
                        if (tsRangeStart > tsRangeEnd)
                            nNumDays = tsRangeStart.Days;
                        else
                            nNumDays = tsRangeEnd.Days;
                    }
                    int nNumDaysOperations = Convert.ToInt32(ConfigurationManager.AppSettings["NumDaysOperations"].ToString());
                    if (nNumDays >= nNumDaysOperations - 1)
                        bUseHistoricData = true;

                    if (bListFines)
                    {
                        // Search for fines
                        if (bUseHistoricData)
                        {
                            strSQLSelect = string.Format("SELECT HFIN_ID, TO_CHAR( HFIN_DATE, 'hh24missddMMYY'), NVL( HFIN_VEHICLEID, 'NOPLATE' ) AS HFIN_VEHICLEID, NVL( HFIN_GRP_ID_ZONE, 60001 ) AS HFIN_GRP_ID_ZONE, HFIN_DATE, DFIN_DESCSHORT, NVL( HFIN_MANUFACTURER, ' ' ) AS HFIN_MANUFACTURER, NVL( HFIN_COLOUR, ' ' ) AS HFIN_COLOUR, NVL( HFIN_STRNUMBER, 0) AS HFIN_STRNUMBER, STR_DESC, GRP_DESCSHORT, NVL(GRP_COLOUR, ' ') AS GRP_COLOUR FROM FINES_HIS, FINES_DEF, STREETS, GROUPS ");
                            strSQLWhere = string.Format("WHERE HFIN_STATUSADMON = {0} AND HFIN_DFIN_ID = DFIN_ID AND DFIN_COD_ID = {1} AND HFIN_STR_ID = STR_ID ",
                                ConfigurationManager.AppSettings["FineStatusAdmonDef.Pending"].ToString(), ConfigurationManager.AppSettings["FinesDefCode.Fine"].ToString());
                            strSQLWhere += string.Format("AND HFIN_VEHICLEID IN ({0}) ", strPlateList);
                            if (nDateFormat == DATE_FORMAT_DAYS)
                                strSQLWhere += string.Format("AND HFIN_DATE > SYSDATE - {0} ", parametersIn["d"].ToString());
                            else
                                strSQLWhere += string.Format("AND HFIN_DATE BETWEEN TO_DATE( '{0}', 'hh24missddMMYY') AND TO_DATE( '{1}', 'hh24missddMMYY') ", parametersIn["d1"].ToString(), parametersIn["d2"].ToString());
                            strSQLWhere += " AND HFIN_GRP_ID_ZONE = GRP_ID ORDER BY HFIN_DATE DESC";
                        }
                        else
                        {
                            strSQLSelect = string.Format("SELECT FIN_ID, TO_CHAR( FIN_DATE, 'hh24missddMMYY'), NVL( FIN_VEHICLEID, 'NOPLATE' ) AS FIN_VEHICLEID, NVL( FIN_GRP_ID_ZONE, 60001 ) AS FIN_GRP_ID_ZONE, FIN_DATE, DFIN_DESCSHORT, NVL( FIN_MANUFACTURER, ' ' ) AS FIN_MANUFACTURER, NVL( FIN_COLOUR, ' ' ) AS FIN_COLOUR, NVL(FIN_STRNUMBER, 0) AS FIN_STRNUMBER, STR_DESC, GRP_DESCSHORT, NVL(GRP_COLOUR, ' ') AS GRP_COLOUR FROM FINES, FINES_DEF, STREETS, GROUPS ");
                            strSQLWhere = string.Format("WHERE FIN_STATUSADMON = {0} AND FIN_DFIN_ID = DFIN_ID AND DFIN_COD_ID = {1} AND FIN_STR_ID = STR_ID ",
                                ConfigurationManager.AppSettings["FineStatusAdmonDef.Pending"].ToString(), ConfigurationManager.AppSettings["FinesDefCode.Fine"].ToString());
                            strSQLWhere += string.Format("AND FIN_VEHICLEID IN ({0}) ", strPlateList);
                            if (nDateFormat == DATE_FORMAT_DAYS)
                                strSQLWhere += string.Format("AND FIN_DATE > SYSDATE - {0} ", parametersIn["d"].ToString());
                            else
                                strSQLWhere += string.Format("AND FIN_DATE BETWEEN TO_DATE( '{0}', 'hh24missddMMYY') AND TO_DATE( '{1}', 'hh24missddMMYY') ", parametersIn["d1"].ToString(), parametersIn["d2"].ToString());
                            strSQLWhere += " AND FIN_GRP_ID_ZONE = GRP_ID ORDER BY FIN_DATE DESC";
                        }
                        oraCmd.CommandText = strSQLSelect + strSQLWhere;

                        dataReader = oraCmd.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                SortedList dataList = new SortedList();

                                dataList["contid"] = strContractId;
                                dataList["contname"] = strContractName;
                                dataList["ot"] = ConfigurationManager.AppSettings["OperationsDef.UnpaidFines"].ToString();
                                dataList["pl"] = dataReader.GetString(2);
                                dataList["zo"] = dataReader.GetInt32(3).ToString();
                                dataList["fn"] = dataReader.GetInt32(0).ToString();
                                dataList["fpd"] = dataReader.GetString(1);
                                dataList["farticle"] = dataReader.GetString(5);
                                dataList["fmake"] = dataReader.GetString(6);
                                dataList["fcolor"] = dataReader.GetString(7);
                                dataList["fstrnum"] = dataReader.GetInt32(8).ToString();
                                dataList["fstreet"] = dataReader.GetString(9);
                                dataList["zonename"] = dataReader.GetString(10);
                                if (!dataReader.IsDBNull(11))
                                    dataList["zonecolor"] = dataReader.GetString(11);

                                // check to see if it is still possible to pay
                                int iPayAmount = 0;
                                int iPayStatus = Convert.ToInt32(ConfigurationManager.AppSettings["FineCancellation.NotPayable"]);
                                if (IsFinePayable(Convert.ToInt32(dataList["fn"]), out iPayAmount, out iPayStatus, Convert.ToInt32(strContractId)))
                                    dataList["fs"] = ConfigurationManager.AppSettings["FineCancellation.Payable"].ToString();
                                else
                                    dataList["fs"] = iPayStatus.ToString();
                                dataList["pa"] = iPayAmount.ToString();

                                nNumOperations++;
                                operationList["o" + nNumOperations.ToString("00000")] = dataList;
                            }
                        }
                    }

                    if (bListFinePayments)
                    {
                        // Search for fine payments
                        if (bUseHistoricData)
                        {
                            strSQLSelect = string.Format("SELECT HOPE_ID, HOPE_DOPE_ID, HOPE_GRP_ID, HOPE_DPAY_ID, NVL(HOPE_POST_PAY,0), HOPE_VALUE_VIS, TO_CHAR( HOPE_MOVDATE, 'hh24missddMMYY'), HOPE_FIN_ID, TO_CHAR( HFIN_DATE, 'hh24missddMMYY'), HFIN_STATUSADMON, HOPE_MOVDATE, HFIN_VEHICLEID, GRP_DESCSHORT, NVL(GRP_COLOUR, ' ') AS GRP_COLOUR FROM OPERATIONS_HIS, FINES_HIS, GROUPS ");
                            strSQLWhere = string.Format("WHERE HOPE_MOBI_USER_ID = {0} AND HOPE_DOPE_ID = {1} AND HOPE_FIN_ID = HFIN_ID ",
                                parametersIn["mui"].ToString(), ConfigurationManager.AppSettings["OperationsDef.Payment"].ToString());
                            if (nDateFormat == DATE_FORMAT_DAYS)
                                strSQLWhere += string.Format("AND HOPE_MOVDATE > SYSDATE - {0} ", parametersIn["d"].ToString());
                            else
                                strSQLWhere += string.Format("AND HOPE_MOVDATE BETWEEN TO_DATE( '{0}', 'hh24missddMMYY') AND TO_DATE( '{1}', 'hh24missddMMYY') ", parametersIn["d1"].ToString(), parametersIn["d2"].ToString());
                            strSQLWhere += " AND HOPE_GRP_ID = GRP_ID ORDER BY HOPE_MOVDATE DESC";
                        }
                        else
                        {
                            strSQLSelect = string.Format("SELECT OPE_ID, OPE_DOPE_ID, OPE_GRP_ID, OPE_DPAY_ID, NVL(OPE_POST_PAY,0), OPE_VALUE_VIS, TO_CHAR( OPE_MOVDATE, 'hh24missddMMYY'), OPE_FIN_ID, TO_CHAR( FIN_DATE, 'hh24missddMMYY'), FIN_STATUSADMON, OPE_MOVDATE, FIN_VEHICLEID, GRP_DESCSHORT, NVL(GRP_COLOUR, ' ') AS GRP_COLOUR FROM OPERATIONS, FINES, GROUPS ");
                            strSQLWhere = string.Format("WHERE OPE_MOBI_USER_ID = {0} AND OPE_DOPE_ID = {1} AND OPE_FIN_ID = FIN_ID ",
                                parametersIn["mui"].ToString(), ConfigurationManager.AppSettings["OperationsDef.Payment"].ToString());
                            if (nDateFormat == DATE_FORMAT_DAYS)
                                strSQLWhere += string.Format("AND OPE_MOVDATE > SYSDATE - {0} ", parametersIn["d"].ToString());
                            else
                                strSQLWhere += string.Format("AND OPE_MOVDATE BETWEEN TO_DATE( '{0}', 'hh24missddMMYY') AND TO_DATE( '{1}', 'hh24missddMMYY') ", parametersIn["d1"].ToString(), parametersIn["d2"].ToString());
                            strSQLWhere += " AND OPE_GRP_ID = GRP_ID ORDER BY OPE_MOVDATE DESC";
                        }
                        oraCmd.CommandText = strSQLSelect + strSQLWhere;

                        if (dataReader != null)
                        {
                            dataReader.Close();
                            dataReader.Dispose();
                        }

                        dataReader = oraCmd.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                SortedList dataList = new SortedList();

                                dataList["contid"] = strContractId;
                                dataList["contname"] = strContractName;
                                dataList["on"] = dataReader.GetInt32(0).ToString();
                                dataList["ot"] = dataReader.GetInt32(1).ToString();
                                dataList["zo"] = dataReader.GetInt32(2).ToString();
                                dataList["pm"] = dataReader.GetInt32(3).ToString();
                                // *** Temporary patch
                                if (dataList["pm"].ToString().Equals(ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString()))
                                    dataList["pm"] = ConfigurationManager.AppSettings["PayTypesDef.Telephone"].ToString();
                                dataList["pp"] = dataReader.GetInt32(4).ToString();
                                dataList["pa"] = dataReader.GetInt32(5).ToString();
                                if (!dataReader.IsDBNull(6))
                                    dataList["fd"] = dataReader.GetString(6);
                                if (!dataReader.IsDBNull(7))
                                    dataList["fn"] = dataReader.GetInt32(7).ToString();
                                if (!dataReader.IsDBNull(8))
                                    dataList["fpd"] = dataReader.GetString(8);
                                dataList["fs"] = ConfigurationManager.AppSettings["FineCancellation.NotPayable"].ToString();
                                dataList["pl"] = dataReader.GetString(11);
                                dataList["zonename"] = dataReader.GetString(12);
                                if (!dataReader.IsDBNull(13))
                                    dataList["zonecolor"] = dataReader.GetString(13);

                                nNumOperations++;
                                operationList["o" + nNumOperations.ToString("00000")] = dataList;
                            }
                        }
                    }

                    // Search for all operations except fine payments
                    if (bListOperations)
                    {
                        if (bUseHistoricData)
                        {
                            strSQLSelect = string.Format("SELECT HOPE_ID, HOPE_DOPE_ID, HOPE_VEHICLEID, HOPE_GRP_ID, TO_CHAR( HOPE_INIDATE, 'hh24missddMMYY'), TO_CHAR( HOPE_ENDDATE, 'hh24missddMMYY'), HOPE_DPAY_ID, NVL(HOPE_POST_PAY,0), HOPE_VALUE_VIS, TO_CHAR( HOPE_MOVDATE, 'hh24missddMMYY'), HOPE_MOVDATE, CASE WHEN (HOPE_ENDDATE - SYSDATE > 0) THEN 2 ELSE 1 END, HOPE_RECHARGE_TYPE, GRP_DESCSHORT, NVL(GRP_COLOUR, ' ') AS GRP_COLOUR FROM OPERATIONS_HIS, GROUPS ");
                            strSQLWhere = string.Format("WHERE HOPE_MOBI_USER_ID = {0} ", parametersIn["mui"].ToString());
                            if (nDateFormat == DATE_FORMAT_DAYS)
                                strSQLWhere += string.Format("AND HOPE_MOVDATE > SYSDATE - {0} ", parametersIn["d"].ToString());
                            else
                                strSQLWhere += string.Format("AND HOPE_MOVDATE BETWEEN TO_DATE( '{0}', 'hh24missddMMYY') AND TO_DATE( '{1}', 'hh24missddMMYY')", parametersIn["d1"].ToString(), parametersIn["d2"].ToString());
                            if (nNumFilters > 0)
                                strSQLWhere += "AND HOPE_DOPE_ID IN (" + strFilterList + ") ";
                            else
                                strSQLWhere += "AND HOPE_DOPE_ID <> " + ConfigurationManager.AppSettings["OperationsDef.Payment"].ToString();
                            strSQLWhere += " AND HOPE_GRP_ID = GRP_ID ORDER BY HOPE_MOVDATE DESC";
                        }
                        else
                        {
                            strSQLSelect = string.Format("SELECT OPE_ID, OPE_DOPE_ID, OPE_VEHICLEID, OPE_GRP_ID, TO_CHAR( OPE_INIDATE, 'hh24missddMMYY'), TO_CHAR( OPE_ENDDATE, 'hh24missddMMYY'), OPE_DPAY_ID, NVL(OPE_POST_PAY,0), OPE_VALUE_VIS, TO_CHAR( OPE_MOVDATE, 'hh24missddMMYY'), OPE_MOVDATE, CASE WHEN (OPE_ENDDATE - SYSDATE > 0) THEN 2 ELSE 1 END, OPE_RECHARGE_TYPE, GRP_DESCSHORT, NVL(GRP_COLOUR, ' ') AS GRP_COLOUR FROM OPERATIONS, GROUPS ");
                            strSQLWhere = string.Format("WHERE OPE_MOBI_USER_ID = {0} ", parametersIn["mui"].ToString());
                            if (nDateFormat == DATE_FORMAT_DAYS)
                                strSQLWhere += string.Format("AND OPE_MOVDATE > SYSDATE - {0} ", parametersIn["d"].ToString());
                            else
                                strSQLWhere += string.Format("AND OPE_MOVDATE BETWEEN TO_DATE( '{0}', 'hh24missddMMYY') AND TO_DATE( '{1}', 'hh24missddMMYY')", parametersIn["d1"].ToString(), parametersIn["d2"].ToString());
                            if (nNumFilters > 0)
                                strSQLWhere += "AND OPE_DOPE_ID IN (" + strFilterList + ") ";
                            else
                                strSQLWhere += "AND OPE_DOPE_ID <> " + ConfigurationManager.AppSettings["OperationsDef.Payment"].ToString();
                            strSQLWhere += " AND OPE_GRP_ID = GRP_ID ORDER BY OPE_MOVDATE DESC";
                        }
                        oraCmd.CommandText = strSQLSelect + strSQLWhere;

                        if (dataReader != null)
                        {
                            dataReader.Close();
                            dataReader.Dispose();
                        }

                        dataReader = oraCmd.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                SortedList dataList = new SortedList();

                                dataList["contid"] = strContractId;
                                dataList["contname"] = strContractName;
                                dataList["on"] = dataReader.GetInt32(0).ToString();
                                dataList["ot"] = dataReader.GetInt32(1).ToString();
                                if (!dataReader.IsDBNull(2))
                                    dataList["pl"] = dataReader.GetString(2);
                                dataList["zo"] = dataReader.GetInt32(3).ToString();
                                if (!dataReader.IsDBNull(4))
                                    dataList["sd"] = dataReader.GetString(4);
                                if (!dataReader.IsDBNull(5))
                                    dataList["ed"] = dataReader.GetString(5);
                                dataList["pm"] = dataReader.GetInt32(6).ToString();
                                // *** Temporary patch
                                if (dataList["pm"].ToString().Equals(ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString()))
                                    dataList["pm"] = ConfigurationManager.AppSettings["PayTypesDef.Telephone"].ToString();
                                dataList["pp"] = dataReader.GetInt32(7).ToString();
                                dataList["pa"] = dataReader.GetInt32(8).ToString();
                                if (dataList["ot"].ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.Recharge"].ToString())
                                    || dataList["ot"].ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.Postpayment"].ToString())
                                    || dataList["ot"].ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.ResidentSticker"].ToString())
                                    || dataList["ot"].ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.ElectricRecharge"].ToString())
                                    || dataList["ot"].ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.Bycing"].ToString()))
                                    dataList["rd"] = dataReader.GetString(9);
                                if (dataList["ot"].ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString())
                                    || dataList["ot"].ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.Extension"].ToString())
                                    || dataList["ot"].ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.Refund"].ToString()))
                                {
                                    if (!dataReader.IsDBNull(11))
                                    {
                                        // If the operation is a refund, then the vehicle is considered not to be parked any longer regardless of the date
                                        if (dataList["ot"].ToString() == ConfigurationManager.AppSettings["OperationsDef.Refund"].ToString())
                                            dataList["sta"] = ConfigurationManager.AppSettings["OperationStatus.Unparked"].ToString();
                                        else
                                            dataList["sta"] = dataReader.GetInt32(11).ToString();
                                    }
                                }
                                if (dataList["ot"].ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.Recharge"].ToString()))
                                {
                                    if (!dataReader.IsDBNull(12))
                                        dataList["bns"] = dataReader.GetInt32(12).ToString();
                                }
                                dataList["zonename"] = dataReader.GetString(13);
                                if (!dataReader.IsDBNull(14))
                                    dataList["zonecolor"] = dataReader.GetString(14);

                                nNumOperations++;
                                operationList["o" + nNumOperations.ToString("00000")] = dataList;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserOperationData::Exception " + strSQLSelect + strSQLWhere, LoggerSeverities.Error);
                Logger_AddLogException(e);
                bResult = false;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        private bool GetParameter(string strParameter, out string strValue, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            strValue = "";

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT PAR_VALUE FROM PARAMETERS WHERE PAR_DESCSHORT = '{0}'", strParameter);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    if (!dataReader.IsDBNull(0))
                        strValue = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetParameter::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
                bResult = false;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        private bool AssignRecoveryCode(int nMobileUser, string strRecoveryCode, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            int nCount = 0;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT COUNT(MUPC_MU_ID) FROM MOBILE_USERS_PASSWORD_CODES WHERE MUPC_MU_ID = {0}", nMobileUser);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    if (!dataReader.IsDBNull(0))
                        nCount = dataReader.GetInt32(0);
                }

                if (nCount > 0)
                    strSQL = string.Format("UPDATE MOBILE_USERS_PASSWORD_CODES SET MUPC_CODE = '{0}', MUPC_DATE = SYSDATE WHERE MUPC_MU_ID = {1}", strRecoveryCode, nMobileUser);
                else
                    strSQL = string.Format("INSERT INTO MOBILE_USERS_PASSWORD_CODES (MUPC_MU_ID, MUPC_CODE) VALUES ({0}, '{1}')", nMobileUser, strRecoveryCode);

                oraCmd.CommandText = strSQL;

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("AssignRecoveryCode::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
                bResult = false;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        private string GetUserRecoveryCode(int nMobileUserId, int nContractId = 0)
        {
            string strRecoveryCode = "";
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT NVL(MUPC_CODE, '') AS MUPC_CODE FROM MOBILE_USERS_PASSWORD_CODES WHERE MUPC_MU_ID = {0}", nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strRecoveryCode = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserRecoveryCode::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return strRecoveryCode;
        }

        private bool VerifyRecoveryCode(int nMobileUserId, string strRecoveryCode, int nContractId = 0)
        {
            bool bResult = false;
            string strExpDate = "";
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT TO_CHAR(MUPC_DATE, 'DDMMYYYYHH24MI') AS MUPC_DATE FROM MOBILE_USERS_PASSWORD_CODES WHERE MUPC_MU_ID = {0} AND MUPC_CODE = '{1}'", nMobileUserId, strRecoveryCode);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strExpDate = dataReader.GetString(0);
                    DateTime dtStartDate = DateTime.ParseExact(strExpDate, "ddMMyyyyHHmm", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime dtCurDate = DateTime.Now;
                    TimeSpan tsRange = new TimeSpan();
                    tsRange = dtCurDate - dtStartDate;
                    int nExpPeriod = Convert.ToInt32(ConfigurationManager.AppSettings["RecoveryCodeExpTime"].ToString());
                    if (tsRange.TotalHours <= nExpPeriod)
                        bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("VerifyRecoveryCode::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        bool SendRecoveryEmail(string strRecoveryCode, string email)
        {
            bool bResult = false;

            try
            {
                MailMessage MyMailMessage = new MailMessage();
                MyMailMessage.From = new MailAddress(ConfigurationManager.AppSettings["SendAddress"].ToString());
                MyMailMessage.To.Add(email);

                //Create Body
                string emailSubject = "Recuperación de contraseña";
                string emailHead = "<p>Estimad@ Sr./Sra.:</p>";
                string strExpPeriod = ConfigurationManager.AppSettings["RecoveryCodeExpTime"].ToString();
                string emailBody = "<p>Su solicitud ha sido procesada con éxito. Para reestablecerla debe introducir la nueva contraseña que se le ha generado aleatoriamente en el cuadro de recuperación de la aplicación. "
                    + "Una vez introducida, accederá automáticamente a la aplicación, desde la cual le recomendamos cambiar la contraseña proporcionada en este email.</p>";
                string emailCode = "<p> Contraseña de recuperación: <b>" + strRecoveryCode + "</b></p>";
                string emailFeet = "<p>Si tiene alguna duda o consulta, puede contactar el soporte técnico de ArinPark en: <a href=\"soporte.arinpark@gerteksa.eus\" target=\"_blank\" > soporte.arinpark@gerteksa.eus</a></p>";
                string bodyMessage = emailHead +
                                     emailBody +
                                     emailCode;

                bodyMessage += emailFeet;

                MyMailMessage.Subject = emailSubject;
                MyMailMessage.IsBodyHtml = true;
                MyMailMessage.Body = bodyMessage;
                MyMailMessage.Priority = System.Net.Mail.MailPriority.High;

                SmtpClient SMTPServer = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"].ToString());
                SMTPServer.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                SMTPServer.UseDefaultCredentials = false;
                SMTPServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["EmailUser"].ToString(), ConfigurationManager.AppSettings["EmailPassword"].ToString());
                SMTPServer.EnableSsl = true;
                SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;

                // Eliminate invalid remote certificate error 
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

                SMTPServer.Send(MyMailMessage);

                bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("SendRecoveryEmail::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return bResult;
        }

        private bool GetFirstVirtualUnit(ref int nVirtualUnit, int nContractId = 0)
        {
            nVirtualUnit = -1;
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = "SELECT NVL(GVU_UNI_ID,-1) FROM GROUP_VIRTUAL_UNIT ORDER BY GVU_UNI_ID";
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nVirtualUnit = dataReader.GetInt32(0);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetFirstVirtualUnit::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        private bool GetVirtualUnit(int nGroup, ref int nVirtualUnit, int nContractId = 0)
        {
            nVirtualUnit = -1;
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT NVL(GVU_UNI_ID,-1) FROM GROUP_VIRTUAL_UNIT WHERE GVU_GRP_ID = {0}", nGroup);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nVirtualUnit = dataReader.GetInt32(0);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetVirtualUnit::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        private bool GetFineGroup(int iFineId, ref int iGroupId, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            iGroupId = -1;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT FIN_GRP_ID_ZONE FROM FINES WHERE FIN_ID = {0}", iFineId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    if (!dataReader.IsDBNull(0))
                        iGroupId = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetFineGroup::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
                bResult = false;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        private bool IsFinePayable(int iFineId, out int iPayAmount, out int iPayStatus, int nContractId = 0)
        {
            bool bResult = false;
            iPayAmount = 0;
            iPayStatus = Convert.ToInt32(ConfigurationManager.AppSettings["FineCancellation.NotPayable"]);

            try
            {
                // Find group of fine to determine the corresponding virtual unit
                string strDate = DateTime.Now.ToString("HHmmssddMMyy");
                int iGroupId = -1;
                int iVirtualUnit = -1;

                if (GetFineGroup(iFineId, ref iGroupId, nContractId))
                {
                    if (iGroupId > 0)
                        GetVirtualUnit(iGroupId, ref iVirtualUnit, nContractId);
                    else
                        Logger_AddLogMessage(string.Format("GetFineData::Error - Could not find group for fine: {0}", iFineId), LoggerSeverities.Error);
                }
                else
                    Logger_AddLogMessage(string.Format("GetFineData::Error obtaining group for fine: {0}", iFineId), LoggerSeverities.Error);

                if (iVirtualUnit < 0)
                {
                    if (GetFirstVirtualUnit(ref iVirtualUnit, nContractId))
                    {
                        if (iVirtualUnit < 0)
                        {
                            Logger_AddLogMessage(string.Format("GetFineData::Error - could not obtain first virtual unit"), LoggerSeverities.Error);
                            return bResult;
                        }
                    }
                    else
                    {
                        Logger_AddLogMessage(string.Format("GetFineData::Error obtaining first virtual unit"), LoggerSeverities.Error);
                        return bResult;
                    }
                }

                SortedList parametersIn = new SortedList();
                parametersIn["f"] = iFineId.ToString();
                parametersIn["d"] = strDate;
                parametersIn["m"] = "1";  //Es pago por móvil

                Hashtable parametersInMapping = new Hashtable();

                parametersInMapping["f"] = "f";
                parametersInMapping["d"] = "d";
                parametersInMapping["m"] = "m";

                Hashtable parametersOutMapping = new Hashtable();

                parametersOutMapping["r"] = "r";
                parametersOutMapping["p"] = "p";
                parametersOutMapping["q"] = "q";
                parametersOutMapping["y"] = "y";

                SortedList parametersOut = new SortedList();

                int iRes = SendM5(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOut, nContractId);
                if (iRes > 0)
                {
                    if (parametersOut["r"].ToString().Equals("1"))
                    {
                        bResult = true;
                        iPayStatus = Convert.ToInt32(ConfigurationManager.AppSettings["FineCancellation.Payable"]);
                    }
                }
                else
                {
                    Logger_AddLogMessage(string.Format("GetFineData::Error sending M5: iRes={0}", iRes), LoggerSeverities.Error);
                    if (parametersOut["r"].ToString().Equals(ConfigurationManager.AppSettings["M05.ErrorCodes.TypeNotPayable"].ToString()))
                        iPayStatus = Convert.ToInt32(ConfigurationManager.AppSettings["FineCancellation.NotPayable"]);
                    else if (parametersOut["r"].ToString().Equals(ConfigurationManager.AppSettings["M05.ErrorCodes.TimeExpired"].ToString()))
                        iPayStatus = Convert.ToInt32(ConfigurationManager.AppSettings["FineCancellation.Expired"]);
                }

                if (parametersOut["q"] != null)
                    iPayAmount = Convert.ToInt32(parametersOut["q"].ToString());
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetFineData::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return bResult;
        }

        private ResultType FindOPSMessageOutputParameters(string xmlOut, out SortedList parametersOut)
        {
            ResultType rtRes = ResultType.Result_OK;
            parametersOut = new SortedList();

            try
            {
                XmlDocument xmlInDoc = new XmlDocument();
                try
                {
                    xmlInDoc.LoadXml(xmlOut);
                    XmlNodeList Nodes = xmlInDoc.SelectNodes("//ap/*");
                    foreach (XmlNode Node in Nodes)
                    {
                        parametersOut[Node.Name] = Node.InnerText;
                    }

                    if (parametersOut["Acst"] != null)
                    {
                        parametersOut.RemoveAt(parametersOut.IndexOfKey("Acst"));

                        Nodes = xmlInDoc.SelectNodes("//ap/Acst/*");
                        foreach (XmlNode Node in Nodes)
                        {
                            parametersOut[Node.Name] = Node.InnerText;
                        }

                    }


                    if (Nodes.Count == 0)
                    {
                        Nodes = xmlInDoc.SelectNodes("//ap");

                        if (Nodes.Count == 0)
                        {
                            Logger_AddLogMessage(string.Format("FindOPSMessageOutputParameters: Bad Input XML: xmlOut= {0}", xmlOut), LoggerSeverities.Error);
                            rtRes = ResultType.Result_Error_OPS_Error;
                        }
                    }


                }
                catch
                {
                    Logger_AddLogMessage(string.Format("FindOPSMessageOutputParameters: Bad Input XML: xmlOut= {0}", xmlOut), LoggerSeverities.Error);
                    rtRes = ResultType.Result_Error_OPS_Error;
                }

            }
            catch (Exception e)
            {
                Logger_AddLogMessage("FindOPSMessageOutputParameters::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return rtRes;
        }


        /// <summary>
        /// Pass a DateTime to a string in format (hhmmssddmmyy)
        /// </summary>
        /// <param name="dt">DateTime to convert</param>
        /// <returns>string in OPS-dtx format</returns>
        public string DtxToString(DateTime dt)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(dt.Hour.ToString("D2"));
            sb.Append(dt.Minute.ToString("D2"));
            sb.Append(dt.Second.ToString("D2"));
            sb.Append(dt.Day.ToString("D2"));
            sb.Append(dt.Month.ToString("D2"));
            int year = dt.Year - 2000;                  // We use only 2 digits.
            sb.Append(year.ToString("D2"));
            return sb.ToString();

        }

        /// <summary>
        /// Comprueba si una fecha es día festivo o no (mira en la tabla DAYS)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="nContractId"></param>
        /// <returns>deveulve si es festivo o no</returns>
        private bool IsHollyday(DateTime dt, int nContractId)
        {
            int nResult = -1;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strDateTime = DtxToString(dt);
                string strSQL = string.Format("select count(*) " +
                                                "from days " +
                                                "where day_date = to_date('{0}', 'HH24MISSDDMMYY')",
                                                strDateTime);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nResult = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("IsHollyday::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return (nResult>0);
        }

        /// <summary>
        /// Comprueba si la multa está fuera de plazo o no
        /// </summary>
        /// <param name="dtFineDatetime"></param>
        /// <param name="dtOpePaymentDateTime"></param>
        /// <param name="iTimeForPayment"></param>
        /// <param name="strDayCode"></param>
        /// <param name="nContractId"></param>
        /// <returns>devuelve si es pagable o no</returns>
        public bool IsFinePaymentInTime(DateTime dtFineDatetime, DateTime dtOpePaymentDateTime, int iTimeForPayment, string strDayCode, int nContractId)
        {
            bool bReturn = false;

            try
            {

                if (iTimeForPayment > 0)
                {
                    int iMaxNumDays = (iTimeForPayment / (24 * 60));
                    int iCurrNumDays = 0;
                    int iCurrLoops = 0;
                    DateTime dtTemp = dtFineDatetime;
                    TimeSpan ts1Day = new TimeSpan(1, 0, 0, 0);
                    dtTemp = dtTemp + ts1Day;


                    DateTime dtWork = new DateTime(dtTemp.Year, dtTemp.Month, dtTemp.Day, 0, 0, 0);

                    while ((iCurrNumDays < iMaxNumDays) && (iCurrLoops < 60))
                    {
                        if (!IsHollyday(dtWork, nContractId))
                        {
                            int index = Convert.ToInt32(dtWork.DayOfWeek) - 1;

                            if (index < 0)
                            {
                                index += 7;
                            }

                            if (strDayCode[index] == '1')
                            {
                                iCurrNumDays++;
                            }
                        }
                        dtWork += ts1Day;
                        iCurrLoops++;
                    }

                    bReturn = ((dtFineDatetime < dtOpePaymentDateTime) && (dtOpePaymentDateTime < dtWork));

                    Logger_AddLogMessage(string.Format("FinePaymentInTime={0}, Fecha Multa: {1}; Fecha Límite: {2}, Fecha Operación: {3}",
                                                        bReturn, dtFineDatetime.ToString(), dtWork.ToString(),
                                                        dtOpePaymentDateTime.ToString()), LoggerSeverities.Info);


                }

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// Metodo para sustituir el proceso de pedir la información del Process de M05 vía servicio asmx (mirar Process de M05)
        /// Devuelve una SortedList con toda la información
        /// </summary>
        /// <param name="fine_id"></param>
        /// <param name="nContractId"></param>
        /// <returns>SortedList con la información</returns>
        private SortedList OPSMessage_M05Process(string fine_id, int nContractId = 0)
        {
            SortedList parametersOut = new SortedList();

            string FINES_DEF_CODES_FINE = ConfigurationManager.AppSettings["FinesDefCodes.Fine"].ToString();
            // Data to include in the response
            string responseFineNumber = fine_id;
            int responseFineDefId = -1; // May be NULL
            string responseVehicleId = null; // May be NULL
            double responseQuantity = -1.0; // May be NULL
            DateTime responseDate = DateTime.MinValue; // May be NULL
            int responseResult = -99; // Cannot be NULL. Default result is Error generic
            int responsePayed = 0;
            int responseGrpId = 0;
            bool responseIsHollyday = false;
            string strDayCode = "";
            // Auxiliar variables
            int payInPdm;

            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT FIN_DFIN_ID, "
                            + "       FIN_VEHICLEID, "
                            + "       FIN_DATE, "
                            + "       fdq.DFINQ_VALUE, "
                            + "       fd1.DFIN_PAYINPDM, "
                            + "		  FIN_STATUSADMON, "
                            + "		  FIN_GRP_ID_ZONE, "
                            + "		  DDAY_CODE, "
                            + "		  DFINQ_INI_MINUTE, "
                            + "		  DFINQ_END_MINUTE, "
                            //+ "       CASE WHEN (trunc(f.fin_date + 1) in (SELECT day_date FROM DAYS)) THEN 1 ELSE 0 END as IsHollyday, "
                            + "		  trunc((CURRENT_DATE - f.fin_date) * 24 * 60) ELAPSED_MINUTES  "
                            + " FROM DAYS_DEF dd, FINES f "
                            + " INNER JOIN FINES_DEF fd1 ON f.FIN_DFIN_ID = fd1.DFIN_ID, FINES_DEF fd2 "
                            + " INNER JOIN FINES_DEF_QUANTITY fdq ON fd2.DFIN_ID = fdq.DFINQ_ID "
                            + " WHERE FIN_ID = " + fine_id + " and fd1.dfin_pay_dday_id=dday_id "
                            + "   AND fd1.DFIN_COD_ID = 1 "
                            + "   and fd1.dfin_id = fd2.dfin_id "
                            + "   and f.fin_date >= fdq.dfinq_inidate "
                            + "   and f.fin_date < fdq.dfinq_endate");
                
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    bool bExit = false;
                    while (dataReader.Read() && !bExit)
                    {
                        responseResult = 0;

                        responseFineDefId = dataReader.GetInt32(0);// "FIN_DFIN_ID"
                        responseVehicleId = dataReader.GetString(1);// "FIN_VEHICLEID"
                        responseQuantity = dataReader.GetDouble(3);//"DFINQ_VALUE"
                        responseDate = dataReader.GetDateTime(2);//"FIN_DATE"
                        payInPdm = dataReader.GetInt32(4);//"DFIN_PAYINPDM"
                        responseGrpId = dataReader.GetInt32(6);//"FIN_GRP_ID_ZONE"
                        strDayCode = dataReader.GetString(7);//"DDAY_CODE"

                        if (dataReader.GetInt32(5) != CFineManager.C_ADMON_STATUS_PENDIENTE)//"FIN_STATUSADMON"
                        {
                            responsePayed = 1;
                        }


                        int iFinQIniMinute = dataReader.GetInt32(8);//"DFINQ_INI_MINUTE"
                        int iFinQEndMinute = dataReader.GetInt32(9);//"DFINQ_END_MINUTE"
                        int iElapsedMinutes = dataReader.GetInt32(10);//"ELAPSED_MINUTES"

                        //responseIsHollyday = dataReader.GetBoolean(10);//IsHollyday


                        //CFineManager oFineManager = new CFineManager();
                        //bool bFinePaymentInTime = oFineManager.IsFinePaymentInTime(responseDate, DateTime.Now, payInPdm, strDayCode);

                        bool bFinePaymentInTime = IsFinePaymentInTime(responseDate, DateTime.Now, payInPdm, strDayCode, nContractId);

                        if (payInPdm == 0)
                        {
                            responseResult = -1;
                            bExit = true;
                        }
                        else if (bFinePaymentInTime)
                        {
                            if ((iElapsedMinutes > iFinQIniMinute) && (iElapsedMinutes <= iFinQEndMinute))
                            {
                                responseResult = 1;
                                bExit = true;
                            }
                            else
                            {
                                responseResult = -2;
                            }
                        }
                        else
                        {
                            responseResult = -2;
                            bExit = true;
                        }
                    }
                }
                else
                {
                    //todavía no existe la multa
                    // existe alguna operación de pago de la misma

                    strSQL = String.Format("SELECT count(*) FROM operations WHERE ope_fin_id = {0}", Convert.ToInt64(fine_id));
                    oraCmd.CommandText = strSQL;

                    if (dataReader != null)
                    {
                        dataReader.Close();
                        dataReader.Dispose();
                    }

                    dataReader = oraCmd.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        dataReader.Read();
                        if (dataReader.GetInt32(0) > 0)
                            responsePayed = 1;
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("CheckMobileUserName::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }
            parametersOut.Add("r", responseResult);//1
            parametersOut.Add("f", responseFineNumber);//20999998
            parametersOut.Add("y", responseFineDefId);//1
            parametersOut.Add("m", responseVehicleId);//1234GTK
            parametersOut.Add("q", responseQuantity);//320
            parametersOut.Add("d", responseDate);//114516060521
            parametersOut.Add("g", responseGrpId);//60002
            parametersOut.Add("p", responsePayed);//0

            return parametersOut;
        }

        private int SendM5(SortedList parametersIn, Hashtable parametersInMapping, Hashtable parametersOutMapping, int iVirtualUnit, out SortedList parametersOut, int nContractId = 0)
        {
            int iRes = Convert.ToInt32(ResultType.Result_OK);
            parametersOut = null;

            try
            {
                SortedList parametersM5In = new SortedList();

                foreach (DictionaryEntry item in parametersIn)
                {

                    if (parametersInMapping[item.Key.ToString()] != null)
                    {
                        parametersM5In[parametersInMapping[item.Key.ToString()]] = item.Value.ToString();
                    }
                }

                string strM5In = GenerateOPSMessage("m5", parametersM5In);

                if (strM5In.Length > 0)
                {

                    Logger_AddLogMessage(string.Format("SendM5::OPSMessageIn = {0}", strM5In), LoggerSeverities.Info);

                    string strM5Out = null;

                    //parametersM5In: d=CURRENT_DATE, f=fin_id, m=1(pago móvil)
                    SortedList parametersM5Out = OPSMessage_M05Process(parametersM5In["f"].ToString(), nContractId);

                    if (parametersM5Out["r"].ToString() != "-99")//No hay error generico
                    //if (OPSMessage(strM5In, iVirtualUnit, out strM5Out, nContractId))
                    {
                        //Logger_AddLogMessage(string.Format("SendM5::OPSMessageOut = {0}", strM5Out), LoggerSeverities.Info);

                        //ResultType rtM5Out = FindOPSMessageOutputParameters(strM5Out, out parametersM5Out);

                        //if (rtM5Out == ResultType.Result_OK)
                        //{
                            parametersOut = new SortedList();
                            iRes = Convert.ToInt32(ResultType.Result_Error_Generic);

                            foreach (DictionaryEntry item in parametersM5Out)
                            {
                                if (parametersOutMapping[item.Key.ToString()] != null)
                                {
                                    parametersOut[parametersOutMapping[item.Key.ToString()]] = item.Value.ToString();
                                }

                            }

                            if (parametersM5Out["p"].ToString() == "1")
                            {
                                iRes = Convert.ToInt32(ResultType.Result_Error_FineNumberAlreadyPayed);
                            }
                            else
                            {
                                if (parametersM5Out["r"].ToString() == ConfigurationManager.AppSettings["M05.ErrorCodes.NotFound"].ToString())
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_FineNumberNotFound);
                                }
                                else if (parametersM5Out["r"].ToString() == ConfigurationManager.AppSettings["M05.ErrorCodes.OK"].ToString())
                                {
                                    iRes = Convert.ToInt32(parametersM5Out["q"].ToString());
                                }
                                else if (parametersM5Out["r"].ToString() == ConfigurationManager.AppSettings["M05.ErrorCodes.TypeNotPayable"].ToString())
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_FineNumberFoundButNotPayable);
                                }
                                else if (parametersM5Out["r"].ToString() == ConfigurationManager.AppSettings["M05.ErrorCodes.TimeExpired"].ToString())
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_FineNumberFoundButTimeExpired);
                                }
                                else
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                }
                            }

                        //}
                        //else
                        //{
                        //    iRes = Convert.ToInt32(rtM5Out);
                        //    Logger_AddLogMessage(string.Format("SendM5::Error In MessageOut = {0}", strM5Out), LoggerSeverities.Error);
                        //}



                    }
                    else
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_OPS_Error);
                        Logger_AddLogMessage(string.Format("SendM5::Error Managing MessageIn = {0}", strM5In), LoggerSeverities.Error);
                    }
                }
                else
                {
                    iRes = Convert.ToInt32(ResultType.Result_Error_OPS_Error);
                    Logger_AddLogMessage("SendM5::Error Generationg OPS M5 Message", LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage("SendM5::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return iRes;
        }

        public string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateOPSMessage(string strMessageType, SortedList parametersM1)
        {
            string xmlOut = "";

            try
            {
                XmlDocument xmlOutDoc = new XmlDocument();

                XmlElement root = xmlOutDoc.CreateElement(strMessageType);
                root.SetAttribute("id", DateTime.Now.ToString("ddmmssfff"));
                xmlOutDoc.AppendChild(root);
                XmlNode rootNode = xmlOutDoc.SelectSingleNode(strMessageType);

                foreach (DictionaryEntry item in parametersM1)
                {
                    XmlElement node = xmlOutDoc.CreateElement(item.Key.ToString());
                    node.InnerText = item.Value.ToString();
                    rootNode.AppendChild(node);
                }

                xmlOut = xmlOutDoc.OuterXml;

            }
            catch (Exception e)
            {
                xmlOut = "";
                Logger_AddLogMessage("GenerateOPSMessage::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return xmlOut;
        }

        /*private bool OPSMessage(string strMessageIn, int iVirtualUnit, out string strMessageOut, int nContractId = 0)
        {
            bool bRdo = false;
            strMessageOut = null;

            try
            {
                //OPSWS11Message.Messages wsMessages = new OPSWS11Message.Messages();

                //string sUrl = ConfigurationManager.AppSettings["OPSWS11Message.Messages"].ToString();
                //if (nContractId > 0)
                //    sUrl = ConfigurationManager.AppSettings["OPSWS11Message.Messages" + nContractId.ToString()].ToString();
                //if (sUrl == null)
                //    throw new Exception("No web service url configuration");

                //wsMessages.Url = sUrl;

                //// Eliminate invalid remote certificate error 
                //ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

                //strMessageOut = wsMessages.Message(strMessageIn);

                //string strSal = "";
                //XmlDocument m5 = new XmlDocument();
                //m5.LoadXml(strMessageIn);
                //Msg05 msg05 = new Msg05(m5);
                //StringCollection sc05 = msg05.Process();
                //System.Collections.Specialized.StringEnumerator myEnumerator05 = sc05.GetEnumerator();
                //while (myEnumerator05.MoveNext())
                //    strSal += myEnumerator05.Current + "\n";

                IRecvMessage msg = null;
                msg = MessageFactory.GetReceivedMessage(strMessageIn);

                //msg.Session = ((MessagesSession)Session["MessagesSession"]);

                StringCollection sc = msg.Process();

                System.Collections.Specialized.StringEnumerator myEnumerator = sc.GetEnumerator();
                while (myEnumerator.MoveNext())
                    strMessageOut += myEnumerator.Current + "\n";

                if (strMessageOut != "")
                {
                    strMessageOut = strMessageOut.Replace("\r", "").Replace("\n", "");
                    string strIP = System.Web.HttpContext.Current.Request.UserHostAddress;//Context.Request.UserHostAddress;
                    LogMsgDB(strMessageIn, strMessageOut, iVirtualUnit, nContractId);
                    bRdo = true;

                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("OPSMessage::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
                bRdo = false;
            }

            return bRdo;
        }*/

        /*private bool LogMsgDB(string xmlIn, string xmlOut, int iVirtualUnit, int nContractId = 0)
        {
            bool bResult = false;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {

                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                string CCunitId = ConfigurationManager.AppSettings["UnitID"].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();


                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlIn);


                string strSQL = string.Format("insert into msgs_log (lmsg_src_uni_id," +
                "lmsg_dst_uni_id,lmsg_date,lmsg_type,lmsg_xml_in," +
                "lmsg_xml_out) values ({0},{1},sysdate,'{2}','{3}','{4}')",
                iVirtualUnit, CCunitId, doc.DocumentElement.Name, xmlIn, xmlOut.Replace("\n", ""));

                oraCmd.CommandText = strSQL;

                oraCmd.ExecuteNonQuery();

                bResult = true;


            }
            catch (Exception e)
            {
                Logger_AddLogMessage("LogMsgDB::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
            finally
            {

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }


            }

            return bResult;
        }*/

        private string GenerateXMLOuput(SortedList parametersOut)
        {
            string xmlOut = "";
            try
            {
                XmlDocument xmlOutDoc = new XmlDocument();

                XmlElement root = xmlOutDoc.CreateElement(_xmlTagName + OUT_SUFIX);
                xmlOutDoc.AppendChild(root);
                XmlNode rootNode = xmlOutDoc.SelectSingleNode(_xmlTagName + OUT_SUFIX);

                foreach (DictionaryEntry item in parametersOut)
                {
                    if (item.Value.GetType() != typeof(SortedList))
                    {
                        XmlElement node = xmlOutDoc.CreateElement(item.Key.ToString());
                        node.InnerText = item.Value.ToString();
                        rootNode.AppendChild(node);
                    }
                    else
                    {
                        XmlElement nodeLST = xmlOutDoc.CreateElement(item.Key.ToString());
                        SortedList stepList = (SortedList)item.Value;
                        if (nodeLST.Name.Equals("notifications"))
                        {
                            foreach (DictionaryEntry itemStepList in stepList)
                            {
                                XmlElement nodeStepList = xmlOutDoc.CreateElement(itemStepList.Key.ToString());
                                nodeStepList.InnerText = itemStepList.Value.ToString();
                                nodeLST.AppendChild(nodeStepList);
                            }
                        }
                        else if (nodeLST.Name.Equals("lst"))
                        {
                            foreach (DictionaryEntry itemStepList in stepList)
                            {
                                XmlElement nodeStepList = xmlOutDoc.CreateElement("o");
                                SortedList step = (SortedList)itemStepList.Value;
                                foreach (DictionaryEntry itemStep in step)
                                {
                                    XmlElement nodeStep = xmlOutDoc.CreateElement(itemStep.Key.ToString());
                                    nodeStep.InnerText = itemStep.Value.ToString().Replace("-<RES>", "");
                                    nodeStepList.AppendChild(nodeStep);
                                }

                                nodeLST.AppendChild(nodeStepList);
                            }
                        }
                        else if (nodeLST.Name.Equals("plates"))
                        {
                            foreach (DictionaryEntry itemStepList in stepList)
                            {
                                XmlElement nodeStepList = xmlOutDoc.CreateElement("plate");
                                if (itemStepList.Value.ToString().Length > 0)
                                {
                                    SortedList step = (SortedList)itemStepList.Value;
                                    foreach (DictionaryEntry itemStep in step)
                                    {
                                        if (itemStep.Value.GetType() != typeof(SortedList))
                                        {
                                            XmlElement nodeStep = xmlOutDoc.CreateElement(itemStep.Key.ToString());
                                            nodeStep.InnerText = itemStep.Value.ToString();
                                            nodeStepList.AppendChild(nodeStep);
                                        }
                                        else
                                        {
                                            XmlElement sectorLST = xmlOutDoc.CreateElement(itemStep.Key.ToString());
                                            SortedList sectorList = (SortedList)itemStep.Value;
                                            if (sectorLST.Name.Equals("sectors"))
                                            {
                                                foreach (DictionaryEntry itemSector in sectorList)
                                                {
                                                    XmlElement nodeSector = xmlOutDoc.CreateElement("sp");
                                                    nodeSector.InnerText = itemSector.Value.ToString();
                                                    sectorLST.AppendChild(nodeSector);
                                                }
                                            }

                                            nodeStepList.AppendChild(sectorLST);
                                        }
                                    }
                                }

                                nodeLST.AppendChild(nodeStepList);
                            }
                        }
                        else if (nodeLST.Name.Equals("bonuslist"))
                        {
                            foreach (DictionaryEntry itemStepList in stepList)
                            {
                                XmlElement nodeStepList = xmlOutDoc.CreateElement("bonus");
                                if (itemStepList.Value.ToString().Length > 0)
                                {
                                    SortedList step = (SortedList)itemStepList.Value;
                                    foreach (DictionaryEntry itemStep in step)
                                    {
                                        XmlElement nodeStep = xmlOutDoc.CreateElement(itemStep.Key.ToString());
                                        nodeStep.InnerText = itemStep.Value.ToString();
                                        nodeStepList.AppendChild(nodeStep);
                                    }
                                }

                                nodeLST.AppendChild(nodeStepList);
                            }
                        }
                        else if (nodeLST.Name.Equals("servicelist"))
                        {
                            foreach (DictionaryEntry itemStepList in stepList)
                            {
                                XmlElement nodeStepList = xmlOutDoc.CreateElement("service");
                                if (itemStepList.Value.ToString().Length > 0)
                                {
                                    SortedList step = (SortedList)itemStepList.Value;
                                    foreach (DictionaryEntry itemStep in step)
                                    {
                                        XmlElement nodeStep = xmlOutDoc.CreateElement(itemStep.Key.ToString());
                                        nodeStep.InnerText = itemStep.Value.ToString();
                                        nodeStepList.AppendChild(nodeStep);
                                    }
                                }

                                nodeLST.AppendChild(nodeStepList);
                            }
                        }
                        else if (nodeLST.Name.Equals("spaces"))
                        {
                            foreach (DictionaryEntry itemStepList in stepList)
                            {
                                XmlElement nodeStepList = xmlOutDoc.CreateElement("spc");
                                if (itemStepList.Value.ToString().Length > 0)
                                {
                                    SortedList step = (SortedList)itemStepList.Value;
                                    foreach (DictionaryEntry itemStep in step)
                                    {
                                        XmlElement nodeStep = xmlOutDoc.CreateElement(itemStep.Key.ToString());
                                        nodeStep.InnerText = itemStep.Value.ToString();
                                        nodeStepList.AppendChild(nodeStep);
                                    }
                                }

                                nodeLST.AppendChild(nodeStepList);
                            }
                        }
                        else
                        {
                            foreach (DictionaryEntry itemStepList in stepList)
                            {
                                XmlElement nodeStepList = xmlOutDoc.CreateElement("st");
                                SortedList step = (SortedList)itemStepList.Value;
                                foreach (DictionaryEntry itemStep in step)
                                {
                                    XmlElement nodeStep = xmlOutDoc.CreateElement(itemStep.Key.ToString());
                                    nodeStep.InnerText = itemStep.Value.ToString();
                                    nodeStepList.AppendChild(nodeStep);
                                }

                                nodeLST.AppendChild(nodeStepList);
                            }
                        }

                        rootNode.AppendChild(nodeLST);

                    }

                }

                xmlOut = xmlOutDoc.OuterXml;

            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GenerateXMLOuput::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return xmlOut;
        }

        private string GenerateXMLErrorResult(ResultType rt)
        {
            string xmlOut = "";
            try
            {
                XmlDocument xmlOutDoc = new XmlDocument();

                XmlElement root = xmlOutDoc.CreateElement(_xmlTagName + OUT_SUFIX);
                xmlOutDoc.AppendChild(root);
                XmlNode rootNode = xmlOutDoc.SelectSingleNode(_xmlTagName + OUT_SUFIX);
                XmlElement result = xmlOutDoc.CreateElement("r");
                result.InnerText = ((int)rt).ToString();
                rootNode.AppendChild(result);
                xmlOut = xmlOutDoc.OuterXml;

            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GenerateXMLErrorResult::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return xmlOut;
        }

        private string GenerateJSONErrorResult(ResultType rt)
        {
            string jsonOut = "";
            try
            {

                string strXmlOut = GenerateXMLErrorResult(rt);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GenerateJSONErrorResult::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return jsonOut;
        }

        private int CheckMobileUserName(string strMobileUserId, string strMobileUserName, int nContractId = 0)
        {
            int nResult = -1;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT COUNT(*) FROM MOBILE_USERS WHERE MU_LOGIN = '{0}' AND MU_ID <> {1} AND MU_VALID = 1 AND MU_DELETED = 0", strMobileUserName, strMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nResult = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("CheckMobileUserName::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return nResult;
        }

        private int CheckMobileUserEmail(string strMobileUserId, string strMobileUserEmail, int nContractId = 0)
        {
            int nResult = -1;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT COUNT(*) FROM MOBILE_USERS WHERE MU_EMAIL = '{0}' AND MU_ID <> {1} AND MU_VALID = 1 AND MU_DELETED = 0", strMobileUserEmail, strMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nResult = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("CheckMobileUserEmail::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return nResult;
        }

        private int ModifyMobileUser(SortedList parametersIn, int nContractId = 0)
        {
            int nMobileUserId = (int)ResultType.Result_Error_Generic;

            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL1 = string.Format("update MOBILE_USERS set mu_name = INITCAP('{0}'), mu_surname1 = INITCAP('{1}'), mu_email = '{2}', mu_mobile_telephone = '{3}', mu_login = '{4}', mu_activate_account = {5}, mu_addr_country = '{6}', mu_fine_notify = {7}, mu_unpark_notify = {8}, mu_unpark_notify_time = {9}, mu_recharge_notify = {10}, mu_balance_notify = {11}, mu_balance_notify_amount = {12}, mu_accept_cond = {13} ",
                                parametersIn["na"].ToString().Replace('\'', ','), parametersIn["fs"].ToString().Replace('\'', ','), parametersIn["em"].ToString(), parametersIn["mmp"].ToString(), parametersIn["un"].ToString().Replace('\'', ','), ConfigurationManager.AppSettings["ActivateAccount.Yes"].ToString(), ConfigurationManager.AppSettings["AddressCountry.Spain"].ToString(),
                                parametersIn["fn"].ToString(), parametersIn["unp"].ToString(), parametersIn["t_unp"].ToString(), parametersIn["re"].ToString(), parametersIn["ba"].ToString(), parametersIn["q_ba"].ToString(), parametersIn["val"]);

                if (parametersIn["pw"] != null)
                {
                    strSQL1 += " , mu_password = '" + parametersIn["pw"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["ss"] != null)
                {
                    strSQL1 += " , mu_surname2 = INITCAP('" + parametersIn["ss"].ToString().Replace('\'', ',') + "')";
                }
                if (parametersIn["nif"] != null)
                {
                    strSQL1 += " , mu_dni = UPPER('" + parametersIn["nif"].ToString() + "')";
                }
                if (parametersIn["amp"] != null)
                {
                    strSQL1 += " , mu_mobile_telephone2 = '" + parametersIn["amp"].ToString() + "'";
                }
                if (parametersIn["asn"] != null)
                {
                    strSQL1 += " , mu_addr_street = '" + parametersIn["asn"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["abn"] != null)
                {
                    strSQL1 += " , mu_addr_number = '" + parametersIn["abn"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["adf"] != null)
                {
                    strSQL1 += " , mu_addr_level = '" + parametersIn["adf"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["add"] != null)
                {
                    strSQL1 += " , mu_door_number = '" + parametersIn["add"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["ads"] != null)
                {
                    strSQL1 += " , mu_addr_stair = '" + parametersIn["ads"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["adl"] != null)
                {
                    strSQL1 += " , mu_addr_letter = '" + parametersIn["adl"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["apc"] != null)
                {
                    strSQL1 += " , mu_addr_postal_code = '" + parametersIn["apc"].ToString() + "'";
                }
                if (parametersIn["aci"] != null)
                {
                    strSQL1 += " , mu_addr_city = '" + parametersIn["aci"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["apr"] != null)
                {
                    strSQL1 += " , mu_addr_province = '" + parametersIn["apr"].ToString().Replace('\'', ',') + "'";
                }
                string strSQL2 = string.Format(" where mu_id = {0} and mu_valid = 1 and mu_deleted = 0", parametersIn["mui"]);

                oraCmd.CommandText = strSQL1 + strSQL2;

                if (oraCmd.ExecuteNonQuery() > 0)
                    nMobileUserId = Convert.ToInt32(parametersIn["mui"]);

                string strSQL = string.Format("UPDATE MOBILE_USERS_PLATES SET MUP_VALID = 0, MUP_DELETED = 1 WHERE MUP_MU_ID = {0}", nMobileUserId);
                oraCmd.CommandText = strSQL;
                oraCmd.ExecuteNonQuery();

                if (parametersIn["plates"] != null)
                {
                    SortedList PlateList = (SortedList)parametersIn["plates"];
                    foreach (string sPlate in PlateList.Values)
                    {
                        string filteredPlate = Regex.Replace(sPlate, @"[^a-zA-Z0-9]+", "");
                        strSQL = string.Format("UPDATE MOBILE_USERS_PLATES SET MUP_VALID = 1, MUP_DELETED = 0 WHERE MUP_MU_ID = {0} AND MUP_PLATE = '{1}'", nMobileUserId, filteredPlate.ToUpper());
                        oraCmd.CommandText = strSQL;
                        if (oraCmd.ExecuteNonQuery() <= 0)
                        {
                            strSQL = string.Format("INSERT INTO MOBILE_USERS_PLATES (MUP_MU_ID, MUP_PLATE) VALUES ({0}, '{1}')", nMobileUserId, filteredPlate.ToUpper());
                            oraCmd.CommandText = strSQL;
                            oraCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                nMobileUserId = (int)ResultType.Result_Error_Generic;
                Logger_AddLogMessage("ModifyMobileUser::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return nMobileUserId;
        }

        private bool GetUserData(int nMobileUserId, out SortedList parametersOut, int nContractId = 0)
        {
            bool bResult = false;
            parametersOut = null;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                parametersOut = new SortedList();

                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT MU_LOGIN, MU_EMAIL, MU_SURNAME1, NVL(MU_SURNAME2,''), MU_NAME, NVL(MU_DNI,''), MU_MOBILE_TELEPHONE, NVL(MU_MOBILE_TELEPHONE2, ''), MU_ADDR_STREET, NVL(MU_ADDR_NUMBER,''), NVL(MU_ADDR_LEVEL,''), NVL(MU_DOOR_NUMBER,''), NVL(MU_ADDR_STAIR, ''), NVL(MU_ADDR_LETTER, ''), MU_ADDR_POSTAL_CODE, MU_ADDR_CITY, MU_ADDR_PROVINCE, MU_FINE_NOTIFY, MU_UNPARK_NOTIFY, MU_UNPARK_NOTIFY_TIME, MU_RECHARGE_NOTIFY, MU_BALANCE_NOTIFY, MU_BALANCE_NOTIFY_AMOUNT, MU_ACCEPT_COND, NVL(MU_TOKEN_USER_ID, -1), NVL(MU_TOKEN_ID, '') FROM MOBILE_USERS WHERE MU_ID = {0}", nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    parametersOut["un"] = dataReader.GetString(0);
                    parametersOut["em"] = dataReader.GetString(1);
                    parametersOut["fs"] = dataReader.GetString(2);
                    if (dataReader.IsDBNull(3))
                        parametersOut["ss"] = "";
                    else
                        parametersOut["ss"] = dataReader.GetString(3);
                    parametersOut["na"] = dataReader.GetString(4);
                    if (dataReader.IsDBNull(5))
                        parametersOut["nif"] = "";
                    else
                        parametersOut["nif"] = dataReader.GetString(5);
                    parametersOut["mmp"] = dataReader.GetString(6);
                    if (dataReader.IsDBNull(7))
                        parametersOut["amp"] = "";
                    else
                        parametersOut["amp"] = dataReader.GetString(7);
                    if (dataReader.IsDBNull(8))
                        parametersOut["asn"] = "";
                    else
                        parametersOut["asn"] = dataReader.GetString(8);
                    if (dataReader.IsDBNull(9))
                        parametersOut["abn"] = "";
                    else
                        parametersOut["abn"] = dataReader.GetString(9);
                    if (dataReader.IsDBNull(10))
                        parametersOut["adf"] = "";
                    else
                        parametersOut["adf"] = dataReader.GetString(10);
                    if (dataReader.IsDBNull(11))
                        parametersOut["add"] = "";
                    else
                        parametersOut["add"] = dataReader.GetString(11);
                    if (dataReader.IsDBNull(12))
                        parametersOut["ads"] = "";
                    else
                        parametersOut["ads"] = dataReader.GetString(12);
                    if (dataReader.IsDBNull(13))
                        parametersOut["adl"] = "";
                    else
                        parametersOut["adl"] = dataReader.GetString(13);
                    if (dataReader.IsDBNull(14))
                        parametersOut["apc"] = "";
                    else
                        parametersOut["apc"] = dataReader.GetString(14);
                    if (dataReader.IsDBNull(15))
                        parametersOut["aci"] = "";
                    else
                        parametersOut["aci"] = dataReader.GetString(15);
                    if (dataReader.IsDBNull(16))
                        parametersOut["apr"] = "";
                    else
                        parametersOut["apr"] = dataReader.GetString(16);
                    parametersOut["val"] = dataReader.GetInt32(23).ToString();
                    if (dataReader.IsDBNull(24))
                        parametersOut["token_user"] = "";
                    else
                    {
                        if (dataReader.GetInt32(24) > 0)
                            parametersOut["token_user"] = dataReader.GetInt32(24).ToString();
                        else
                            parametersOut["token_user"] = "";
                    }
                    if (dataReader.IsDBNull(25))
                        parametersOut["token_id"] = "";
                    else
                        parametersOut["token_id"] = dataReader.GetString(25);

                    SortedList notifyList = new SortedList();
                    notifyList["fn"] = dataReader.GetInt32(17).ToString();
                    notifyList["unp"] = dataReader.GetInt32(18).ToString();
                    notifyList["t_unp"] = dataReader.GetInt32(19).ToString();
                    notifyList["re"] = dataReader.GetInt32(20).ToString();
                    notifyList["ba"] = dataReader.GetInt32(21).ToString();
                    notifyList["q_ba"] = dataReader.GetInt32(22).ToString();
                    parametersOut["notifications"] = notifyList;

                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserData::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        private bool GetPlateData(int nMobileUserId, out SortedList plateDataList, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            plateDataList = new SortedList();

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                // Get a list of all the plates associated with the user
                ArrayList plateList = new ArrayList();
                string strSQL = string.Format("SELECT MUP_PLATE FROM MOBILE_USERS_PLATES WHERE MUP_MU_ID = {0} AND MUP_VALID = 1 AND MUP_DELETED = 0", nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    plateList.Add(dataReader.GetString(0));
                }

                bool bPlateDone = false;
                int nPlateIndex = 0;
                foreach (string strPlate in plateList)
                {
                    bPlateDone = false;
                    SortedList plateData = new SortedList();
                    plateData["p"] = strPlate;

                    // First check to see if the plate is assigned in the residents list
                    int nSectorIndex = 0;
                    SortedList sectorList = new SortedList();

                    strSQL = string.Format("SELECT RES_GRP_ID FROM RESIDENTS WHERE RES_VEHICLEID = '{0}'", strPlate);

                    oraCmd.CommandText = strSQL;

                    if (dataReader != null)
                    {
                        dataReader.Close();
                        dataReader.Dispose();
                    }

                    dataReader = oraCmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        nSectorIndex++;
                        sectorList["sp" + nSectorIndex.ToString()] = dataReader.GetInt32(0).ToString();
                    }

                    if (sectorList.Count > 0)
                    {
                        bPlateDone = true;
                        plateData["stp"] = ConfigurationManager.AppSettings["ArticleType.Resident"].ToString();
                        plateData["sectors"] = sectorList;
                    }

                    // Next check VIPs list
                    if (!bPlateDone)
                    {
                        strSQL = string.Format("SELECT COUNT(*) FROM VIPS WHERE VIP_VEHICLEID = '{0}'", strPlate);

                        oraCmd.CommandText = strSQL;

                        if (dataReader != null)
                        {
                            dataReader.Close();
                            dataReader.Dispose();
                        }

                        dataReader = oraCmd.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            dataReader.Read();

                            int nCount = dataReader.GetInt32(0);

                            if (nCount > 0)
                            {
                                bPlateDone = true;
                                plateData["stp"] = ConfigurationManager.AppSettings["ArticleType.Vip"].ToString();
                            }
                        }
                    }

                    // If not assigned to resident or VIPs, then it is considered to be rotation
                    if (!bPlateDone)
                    {
                        plateData["stp"] = ConfigurationManager.AppSettings["ArticleType.Rotation"].ToString();
                    }

                    nPlateIndex++;
                    plateDataList["plate" + nPlateIndex.ToString()] = plateData;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetPlateData::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
                bResult = false;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        private bool GetUserPlates(int nMobileUserId, out string strPlateList, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            int nNumPlates = 0;

            strPlateList = "";

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                // Get a list of all the plates associated with the user
                string strSQL = string.Format("SELECT MUP_PLATE FROM MOBILE_USERS_PLATES WHERE MUP_MU_ID = {0} AND MUP_VALID = 1 AND MUP_DELETED = 0", nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    if (nNumPlates > 0)
                        strPlateList += ",";
                    strPlateList += "'" + dataReader.GetString(0) + "'";
                    nNumPlates++;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserPlates::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
                bResult = false;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return bResult;
        }

        private int GetUserCredit(int nMobileUserId, int nContractId = 0)
        {
            int nCredit = -1;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT NVL(MU_FUNDS,0) FROM MOBILE_USERS WHERE MU_ID = {0}", nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nCredit = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserCredit::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return nCredit;
        }

        private string GetUserName(int nMobileUserId, int nContractId = 0)
        {
            string strUserName = "";
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT MU_LOGIN FROM MOBILE_USERS WHERE MU_ID = {0}", nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strUserName = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserName::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return strUserName;
        }

        private string GetUserEmail(int nMobileUserId, int nContractId = 0)
        {
            string strEmail = "";
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                oraCmd = new OracleCommand();
                oraCmd.Connection = oraConn;
                oraCmd.Connection.Open();

                if (oraCmd == null)
                    throw new Exception("Oracle command is null");

                // Conexion BBDD?
                if (oraCmd.Connection == null)
                    throw new Exception("Oracle connection is null");

                if (oraCmd.Connection.State != System.Data.ConnectionState.Open)
                    throw new Exception("Oracle connection is not open");

                string strSQL = string.Format("SELECT NVL(MU_EMAIL, '-') AS MU_EMAIL FROM MOBILE_USERS WHERE MU_ID = {0}", nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strEmail = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserEmail::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                    dataReader = null;
                }

                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }

                if (oraConn != null)
                {
                    oraConn.Close();
                    oraConn.Dispose();
                    oraConn = null;
                }
            }

            return strEmail;
        }

        #endregion

        #region Static Methods

        private static void InitializeStatic()
        {
            System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
            if (_logger == null)
            {
                _logger = new Logger(MethodBase.GetCurrentMethod().DeclaringType);
                DatabaseFactory.Logger = _logger;
            }

            if (_MacTripleDesKey == null)
            {
                _MacTripleDesKey = ((string)appSettings.GetValue("MOBILE_PAYMENT_MACTRIPLEDES_KEY", typeof(string)));
            }

            if (_normTripleDesKey == null)
            {
                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(_MacTripleDesKey);
                _normTripleDesKey = new byte[24];
                int iSum = 0;

                for (int i = 0; i < 24; i++)
                {
                    if (i < keyBytes.Length)
                    {
                        iSum += keyBytes[i];
                    }
                    else
                    {
                        iSum += i;
                    }
                    _normTripleDesKey[i] = Convert.ToByte((iSum * BIG_PRIME_NUMBER) % (Byte.MaxValue + 1));
                }
            }

            if (_mac3des == null)
            {
                _mac3des = new MACTripleDES(_normTripleDesKey);
            }

            if (_xmlTagName == null)
            {
                _xmlTagName = ((string)appSettings.GetValue("XML_TAG_NAME", typeof(string)));
            }

            if (_partnerMacTripleDesKey == null)
            {
                _partnerMacTripleDesKey = ((string)appSettings.GetValue("PARTNER_MACTRIPLEDES_KEY", typeof(string)));
            }

            if (_partnerNormTripleDesKey == null)
            {
                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(_partnerMacTripleDesKey);
                _partnerNormTripleDesKey = new byte[24];
                int iSum = 0;

                for (int i = 0; i < 24; i++)
                {
                    if (i < keyBytes.Length)
                    {
                        iSum += keyBytes[i];
                    }
                    else
                    {
                        iSum += i;
                    }
                    _partnerNormTripleDesKey[i] = Convert.ToByte((iSum * BIG_PRIME_NUMBER) % (Byte.MaxValue + 1));
                }
            }

            if (_partnerMac3des == null)
            {
                _partnerMac3des = new MACTripleDES(_partnerNormTripleDesKey);
            }

            if (_useHash == null)
            {
                _useHash = ((string)appSettings.GetValue("EnableHash", typeof(string)));
            }
        }

        static private string Decrypt(int nUserId, string strEncrypted)
        {
            string strDecrypted = "";
            string strLocEncrypted = "";

            try
            {
                string strKey = GetKeyToApply(nUserId.ToString());

                if ((strEncrypted.Substring(0, 6) == "%&!()=") && (strEncrypted.Substring(strEncrypted.Length - 6, 6) == "%&!()="))
                {
                    strLocEncrypted = strEncrypted.Substring(6, strEncrypted.Length - 12);
                }

                byte[] byEncrypt = HexString_To_Bytes(strLocEncrypted);

                TripleDESCryptoServiceProvider TripleDesProvider = new TripleDESCryptoServiceProvider();
                int sizeKey = System.Text.Encoding.Default.GetByteCount(strKey);
                byte[] byKey;
                byKey = new byte[sizeKey];
                System.Text.Encoding.Default.GetBytes(strKey, 0, strKey.Length, byKey, 0);
                TripleDesProvider.Mode = CipherMode.ECB;
                TripleDesProvider.Key = byKey;
                Array.Clear(TripleDesProvider.IV, 0, TripleDesProvider.IV.Length);

                OPSTripleDesEncryptor OPSTripleDesEnc = new OPSTripleDesEncryptor(TripleDesProvider);
                byte[] byDecrypt;

                byDecrypt = OPSTripleDesEnc.Desencriptar(byEncrypt);

                strDecrypted = GetDataAsString(byDecrypt, 0);

                char[] chTrim = new char[1];
                chTrim[0] = '\0';

                strDecrypted = strDecrypted.TrimEnd(chTrim);
            }
            catch
            {
            }

            return strDecrypted;
        }

        static private byte[] HexString_To_Bytes(string strInput)
        {
            // i variable used to hold position in string
            int i = 0;
            // x variable used to hold byte array element position
            int x = 0;
            // allocate byte array based on half of string length
            byte[] bytes = new byte[(strInput.Length) / 2];
            // loop through the string - 2 bytes at a time converting it to decimal equivalent and store in byte array
            while (strInput.Length > i + 1)
            {
                long lngDecimal = Convert.ToInt32(strInput.Substring(i, 2), 16);
                bytes[x] = Convert.ToByte(lngDecimal);
                i = i + 2;
                ++x;
            }
            // return the finished byte array of decimal values
            return bytes;
        }

        static private string Bytes_To_HexString(byte[] bytes_Input)
        {
            // convert the byte array back to a true string
            string strTemp = "";
            for (int x = 0; x <= bytes_Input.GetUpperBound(0); x++)
            {
                int number = int.Parse(bytes_Input[x].ToString());
                strTemp += number.ToString("X").PadLeft(2, '0');
            }
            // return the finished string of hex values
            return strTemp;
        }

        static private string GetDataAsString(byte[] data, int ignoreLastBytes)
        {
            if (data.Length < ignoreLastBytes)
                return "";

            System.Text.Decoder utf8Decoder = System.Text.Encoding.UTF8.GetDecoder();
            int charCount = utf8Decoder.GetCharCount(data, 0, (data.Length - ignoreLastBytes));
            char[] recievedChars = new char[charCount];
            utf8Decoder.GetChars(data, 0, data.Length - ignoreLastBytes, recievedChars, 0);
            String recievedString = new String(recievedChars);
            return recievedString;
        }

        private static string GetKeyToApply(string key)
        {
            string strRes = KEY_MESSAGE_TCP_5;
            int iSum = 0;
            int iMod;

            if (key.Length == 0)
            {
                strRes = KEY_MESSAGE_TCP_5;
            }
            else if (key.Length >= 24)
            {
                strRes = key.Substring(0, 24);
            }
            else
            {
                for (int i = 0; i < key.Length; i++)
                {
                    iSum += Convert.ToInt32(key[i]);
                }

                iMod = iSum % 8;

                switch (iMod)
                {
                    case 0:
                        strRes = KEY_MESSAGE_TCP_0;
                        break;
                    case 1:
                        strRes = KEY_MESSAGE_TCP_1;
                        break;
                    case 2:
                        strRes = KEY_MESSAGE_TCP_2;
                        break;
                    case 3:
                        strRes = KEY_MESSAGE_TCP_3;
                        break;
                    case 4:
                        strRes = KEY_MESSAGE_TCP_4;
                        break;
                    case 5:
                        strRes = KEY_MESSAGE_TCP_5;
                        break;
                    case 6:
                        strRes = KEY_MESSAGE_TCP_6;
                        break;
                    case 7:
                        strRes = KEY_MESSAGE_TCP_7;
                        break;

                    default:
                        strRes = KEY_MESSAGE_TCP_0;
                        break;
                }

                strRes = key + strRes.Substring(0, 24 - key.Length);
            }

            return strRes;
        }

        private string CalculateSHA1(string strInput)
        {
            string strOutput = "";
            try
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(strInput);
                byte[] outputBytes;

                SHA1 sha = new SHA1CryptoServiceProvider();

                outputBytes = sha.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < outputBytes.Length; i++)
                {
                    sb.Append(outputBytes[i].ToString("X2"));
                }
                strOutput = sb.ToString();
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("CalculateSHA1::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return strOutput;
        }

        public static string DecodeFrom64(string data)
        {
            byte[] binary = Convert.FromBase64String(data);
            return Encoding.GetEncoding(1252).GetString(binary);
        }

        public static string Encrypt(string textKey, string content)
        {
            byte[] key = Encoding.GetEncoding(1252).GetBytes(textKey);
            byte[] iv = new byte[8];
            byte[] data = Encoding.GetEncoding(1252).GetBytes(content);
            byte[] enc = new byte[0];
            System.Security.Cryptography.TripleDES tdes = System.Security.Cryptography.TripleDES.Create();
            tdes.IV = iv;
            tdes.Key = key;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.Zeros;
            ICryptoTransform ict = tdes.CreateEncryptor();
            enc = ict.TransformFinalBlock(data, 0, data.Length);
            return Encoding.GetEncoding(1252).GetString(enc);
        }

        public static byte[] EncriptarTripleDES_IV_0(string texto, byte[] key)
        {
            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                byte[] iv_0 = { 0, 0, 0, 0, 0, 0, 0, 0 };  //same IV that Redsys
                byte[] toEncryptArray = Encoding.ASCII.GetBytes(texto);
                tdes.IV = iv_0;
                //assign the secret key
                tdes.Key = key;
                tdes.Mode = CipherMode.CBC;
                tdes.Padding = PaddingMode.Zeros;
                ICryptoTransform cTransform = tdes.CreateEncryptor();
                //transform the specified region of bytes array to resultArray
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                //Clear to Best Practices
                tdes.Clear();
                return resultArray;
            }
        }

        public static string HashHMAC(string data, string key)
        {
            key = key ?? "";
            var encoding = Encoding.GetEncoding(1252);
            byte[] keyByte = encoding.GetBytes(key);
            byte[] messageBytes = encoding.GetBytes(data);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

        public static string EncodeTo64(string data)
        {
            byte[] toEncodeAsBytes = Encoding.GetEncoding(1252).GetBytes(data);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static byte[] TripleDESEncrypt(string texto, byte[] key)
        {
            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                byte[] iv_0 = { 0, 0, 0, 0, 0, 0, 0, 0 };

                byte[] toEncryptArray = Encoding.ASCII.GetBytes(texto);

                tdes.IV = iv_0;

                //assign the secret key
                tdes.Key = key;

                tdes.Mode = CipherMode.CBC;

                tdes.Padding = PaddingMode.Zeros;

                ICryptoTransform cTransform = tdes.CreateEncryptor();
                //transform the specified region of bytes array to resultArray
                byte[] resultArray =
                  cTransform.TransformFinalBlock(toEncryptArray, 0,
                  toEncryptArray.Length);

                //Clear to Best Practices
                tdes.Clear();

                return resultArray;
            }
        }

        public static byte[] EncriptarSHA256(string texto, byte[] privateKey)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] key = privateKey;
            var myhmacsha256 = new HMACSHA256(key);
            byte[] hashValue = myhmacsha256.ComputeHash(encoding.GetBytes(texto));
            myhmacsha256.Clear();
            return hashValue;
        }

        private static void Logger_AddLogMessage(string msg, LoggerSeverities severity)
        {
            _logger.AddLog(msg, severity);
        }

        private static void Logger_AddLogException(Exception ex)
        {
            _logger.AddLog(ex);
        }

        #endregion

    }
}

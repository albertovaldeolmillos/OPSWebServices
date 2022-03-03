using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Xml;
using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Jot;
using Newtonsoft.Json;
//using OPS.Comm;
//using OPS.Components.Data;
//using OPS.Comm.Becs.Messages;
//using OPS.Comm.Cryptography.TripleDes;
//using OPS.Components.Data;
//using OPS.FineLib;
using OPSWebServicesAPI.Helpers;
using OPSWebServicesAPI.Models;
using Oracle.ManagedDataAccess.Client;
using static OPSWebServicesAPI.Models.ErrorText;

namespace OPSWebServicesAPI.Controllers
{
    public class WSUserManagementController : ApiController
    {
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static ILogger _logger = null;
        static int CFineManager_C_ADMON_STATUS_PENDIENTE = 0;
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



        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            InitializeStatic();
        }

        public static ILogger Logger
        {
            get { return _logger; }
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
        /// Returns authorization token to login user or error
        /// </summary>
        /// <param name="userLogin">Objet UserLogin</param>
        /// <returns>Return authorization code or error code</returns>
        [HttpPost]
        [Route("LoginUserAPI")]
        public ResultLoginInfo LoginUserAPI([FromBody] UserLogin userLogin)
        {
            string strToken = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
            ResultLoginInfo response = new ResultLoginInfo();

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

            string token = null;
            TokenRequest.TryTokenRequest(Request, out token);
            parametersIn.Add("mui", token);

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
                    if ((parametersIn["un"] == null || (parametersIn["un"] != null && parametersIn["un"].ToString().Length == 0)) &&
                        (parametersIn["pw"] == null || (parametersIn["pw"] != null && parametersIn["pw"].ToString().Length == 0)) &&
                        (parametersIn["mui"] == null) || (parametersIn["mui"] != null && parametersIn["mui"].ToString().Length == 0))
                    {
                        Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken_UserNme_Password, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;//Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    }
                    else if ((parametersIn["cid"] == null) || (parametersIn["cid"].ToString().Length == 0))
                    {
                        Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_CloudToken, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;//Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    }
                    else if ((parametersIn["os"] == null) || (parametersIn["os"].ToString().Length == 0))
                    {
                        Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_OperativeSystem, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;//Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    }
                    else if ((parametersIn["v"] == null) || (parametersIn["v"].ToString().Length == 0))
                    {
                        Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_Version, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;//Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    }
                    else if ((parametersIn["contid"] == null) || (parametersIn["contid"].ToString().Length == 0))
                    {
                        Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
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
                            Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Bad hash: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.value = null; //Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
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
                                Logger_AddLogMessage(string.Format("LoginUserAPI::Incorrect app version - update needed: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_App_Update_Required, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_App_Update_Required).ToString();
                                return response;//Convert.ToInt32(ResultType.Result_Error_App_Update_Required).ToString();
                            }
                            else if (nVersionResult < 0)
                            {
                                Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Could not verify version: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
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
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Could not validate user: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                    return response;//Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                }
                                else
                                    Logger_AddLogMessage(string.Format("LoginUserAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                // Check user validation
                                if (!IsUserValidated(nMobileUserId, nContractId))
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::User not validated - needs to activate account: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_User_Not_Validated, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_User_Not_Validated).ToString();
                                    return response;//Convert.ToInt32(ResultType.Result_Error_User_Not_Validated).ToString();
                                }

                                // Generate authorization token
                                strToken = GetNewToken();

                                if (!UpdateWebCredentials(nMobileUserId, parametersIn["cid"].ToString(), Convert.ToInt32(parametersIn["os"]), strToken, parametersIn["v"].ToString(), nContractId))
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Could not update web credentials: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
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
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Could not obtain user from token: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                    return response;//Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                }
                                else
                                    Logger_AddLogMessage(string.Format("LoginUserAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                // Determine if token is valid
                                TokenValidationResult tokenResult = DefaultVerification(strToken);

                                if (tokenResult != TokenValidationResult.Passed && tokenResult != TokenValidationResult.TokenExpired)
                                {
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Token not valid: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                    response.isSuccess = false;
                                    response.error = new Error(-230 - (int)tokenResult, (int)SeverityError.Critical);//new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
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
                                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Could not update web credentials: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    return response;//Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                }
                            }
                        }
                    }
                }
                else
                {
                    Logger_AddLogMessage(string.Format("LoginUserAPI::Error - Incorrect input format: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;//Convert.ToInt32(rt).ToString();
                }
            }
            catch (Exception e)
            {
                strToken = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                Logger_AddLogMessage(string.Format("LoginUserAPI::Error - {0}: parametersIn= {1}", e.Message, SortedListToString(parametersIn)), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;//Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
            }

            response.isSuccess = true;
            response.error = null;// new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            response.value = strToken;
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
        /// Update user data and returns user id or error
        /// </summary>
        /// <param name="user">Objet User</param>
        /// <returns>Return user id or error</returns>
        [HttpPost]
        [Route("UpdateUserAPI")]
        public ResultUpdateUserInfo UpdateUserAPI([FromBody] User user)
        {
            //string xmlOut = "";
            int nMobileUserId = -1;
            ResultUpdateUserInfo response = new ResultUpdateUserInfo();

            SortedList notificationList = new SortedList();
            SortedList plateList = new SortedList();
            int numPlates = 0;
            SortedList parametersIn = new SortedList();

            string token;
            if (!TokenRequest.TryTokenRequest(Request, out token))
            {
                int iRes = Convert.ToInt32(ResultType.Result_Error_No_Bearer_Token);
                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error: No Bearer Token, iOut={0}", iRes), LoggerSeverities.Error);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_No_Bearer_Token, (int)SeverityError.Critical);
                response.value = null;
                return response;
            }
            else
            {
                TokenValidationResult tokenResult = DefaultVerification(token);
                if (tokenResult != TokenValidationResult.Passed)
                {
                    int iRes = -230 - (int)tokenResult;
                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error: Token invalid, iOut={0}", iRes), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error(iRes, (int)SeverityError.Critical);
                    response.value = null;
                    return response;
                }
            }
            parametersIn.Add("mui", token);

            PropertyInfo[] properties = typeof(User).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                if (NombreAtributo == "plates" && user.plates != null)
                {        
                    foreach (Plate plate in user.plates)
                    {
                        plateList.Add("p" + numPlates, plate.plate);
                        numPlates++;
                    }
                    parametersIn.Add("plates", plateList);
                }
                else if (NombreAtributo == "notifications" && user.notifications != null)
                {
                    PropertyInfo[] propertiesNot = typeof(Notification).GetProperties();
                    foreach (PropertyInfo propertyNot in propertiesNot)
                    {
                        var attributeNot = propertyNot.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                        string NombreAtributoNot = (attribute == null) ? propertyNot.Name : attributeNot.DisplayName;
                        parametersIn.Add(NombreAtributoNot, propertyNot.GetValue(user.notifications));
                    }
                }
                else if (NombreAtributo == "notifications" && user.notifications == null)
                {
                    parametersIn.Add("ba", 1); parametersIn.Add("fn", 1); parametersIn.Add("re", 1); parametersIn.Add("unp", 1);
                    parametersIn.Add("q_ba", 300); parametersIn.Add("t_unp", 10);
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

                Logger_AddLogMessage(string.Format("UpdateUserAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if ((parametersIn["em"] == null) || (parametersIn["em"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_Email, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if ((parametersIn["fs"] == null) || (parametersIn["fs"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_FirstSurname, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if ((parametersIn["na"] == null) || (parametersIn["na"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_Name, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if ((parametersIn["mmp"] == null) || (parametersIn["mmp"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_MainMobilePhone, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if ((parametersIn["val"] == null) || (parametersIn["val"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_ValidateConditions, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    //else if ((parametersIn["plates"] == null))
                    //{
                    //    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                    //    Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                    //    response.isSuccess = false;
                    //    response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_No_Plates, (int)SeverityError.Critical);
                    //    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    //    return response;
                    //}
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
                            Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Bad hash: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.value = null; //Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
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
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Could not obtain user from token: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }
                            else
                                Logger_AddLogMessage(string.Format("UpdateUserXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Token not valid: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error(-230 - (int)tokenResult, (int)SeverityError.Critical);//new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            if (CheckMobileUserName(parametersIn["mui"].ToString(), parametersIn["un"].ToString(), nContractId) != 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Mobile_User_Already_Registered);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - User name already registered: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Mobile_User_Already_Registered, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Mobile_User_Already_Registered).ToString();
                                return response;
                            }

                            if (CheckMobileUserEmail(parametersIn["mui"].ToString(), parametersIn["em"].ToString(), nContractId) != 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Mobile_User_Email_Already_Registered);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Email already registered: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Mobile_User_Email_Already_Registered, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Mobile_User_Email_Already_Registered).ToString();
                                return response;
                            }

                            nMobileUserId = ModifyMobileUser(parametersIn, nContractId);

                            if (nMobileUserId <= 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Failed to modify user: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }

                            Logger_AddLogMessage(string.Format("UpdateUserAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Get user data
                            if (!GetUserData(Convert.ToInt32(parametersIn["mui"]), out parametersOut, nContractId))
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Could not obtain user data: parametersIn= {0}, error={1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }

                            // Get parking data for assigned plates
                            if (!GetPlateData(Convert.ToInt32(parametersIn["mui"]), out plateDataList, nContractId))
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Could not obtain plate data: parametersIn= {0}, error={1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
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
                    Logger_AddLogMessage(string.Format("UpdateUserAPI::Error - Incorrect input format: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }
            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("UpdateUserAPI::Error: parametersIn= {0}, error={1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            response.isSuccess = true;
            response.error = null;// new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            response.value = nMobileUserId.ToString();
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
        /// Returns user operation list
        /// </summary>
        /// <param name="userOperation">Object UserOperation</param>
        /// <returns>Returns user ooeration list or error</returns>        
        [HttpPost]
        [Route("QueryUserOperationsAPI")]
        public ResultListOperationInfo QueryUserOperationsAPI([FromBody] UserOperation userOperation)
        {
            //string xmlOut = "";
            ResultListOperationInfo response = new ResultListOperationInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            string token;
            if (!TokenRequest.TryTokenRequest(Request, out token))
            {
                int iRes = Convert.ToInt32(ResultType.Result_Error_No_Bearer_Token);
                Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error: No Bearer Token, iOut={0}", iRes), LoggerSeverities.Error);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_No_Bearer_Token, (int)SeverityError.Critical);
                response.value = null;
                return response;
            }
            else
            {
                TokenValidationResult tokenResult = DefaultVerification(token);
                if (tokenResult != TokenValidationResult.Passed)
                {
                    int iRes = -230 - (int)tokenResult;
                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error: Token invalid, iOut={0}", iRes), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error(iRes, (int)SeverityError.Critical);
                    response.value = null;
                    return response;
                }
            }
            parametersIn.Add("mui", token);

            PropertyInfo[] properties = typeof(UserOperation).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                if (NombreAtributo == "ots" && userOperation.operationTypeList != null)
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

                Logger_AddLogMessage(string.Format("QueryUserOperationsAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, (int)SeverityError.Critical);
                        response.value = null;
                        return response;
                    }
                    else if ((parametersIn["d"] == null) || (parametersIn["d"].ToString() == "0"))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_Date, (int)SeverityError.Critical);
                        response.value = null;
                        return response;
                    }
                    else if ((parametersIn["contid"] == null) || (parametersIn["contid"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, (int)SeverityError.Critical);
                        response.value = null;
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
                            Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Bad hash: parametersIn= {0}, error = {1}", SortedListToString(parametersIn), "Result_Error_InvalidAuthenticationHash"), LoggerSeverities.Error);
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.value = null;
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
                                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Could not obtain user from token: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                    //return xmlOut;
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.value = null;
                                    return response;
                                }
                                else
                                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                // Determine if token is valid
                                TokenValidationResult tokenResult = DefaultVerification(strToken);

                                if (tokenResult != TokenValidationResult.Passed)
                                {
                                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Token not valid: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                    //return xmlOut;
                                    response.isSuccess = false;
                                    response.error = new Error(-230 - (int)tokenResult, (int)SeverityError.Critical);//new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.value = null;
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
                                Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error - Could not obtain operation data: parametersIn= {0}, error = {1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null;
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
                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error: parametersIn= {0}, error = {1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.value = null;
                    return response;
                }
            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUserOperationsXML::Error: parametersIn= {0}, error = {1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null;
                return response;
            }

            //return xmlOut;
            response.isSuccess = true;
            response.error = null;// new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
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
            response.value = lista.ToArray();
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
        /// Returns user information
        /// </summary>
        /// <returns>Returns user data or error</returns>
        [HttpGet]
        [Route("QueryUserAPI")]
        //public ResultUserInfo QueryUserAPI([FromBody] UserQuery userQuery)
        public ResultUserInfo QueryUserAPI()
        {
            //string xmlOut = "";
            int nMobileUserId = -1;
            ResultUserInfo response = new ResultUserInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            string token;
            if (!TokenRequest.TryTokenRequest(Request, out token))
            {
                int iRes = Convert.ToInt32(ResultType.Result_Error_No_Bearer_Token);
                Logger_AddLogMessage(string.Format("QueryUserAPI::Error: No Bearer Token, iOut={0}", iRes), LoggerSeverities.Error);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_No_Bearer_Token, (int)SeverityError.Critical);
                response.value = null;
                return response;
            }
            else
            {
                TokenValidationResult tokenResult = DefaultVerification(token);
                if (tokenResult != TokenValidationResult.Passed)
                {
                    int iRes = -230 - (int)tokenResult;
                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error: Token invalid, iOut={0}", iRes), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error(iRes, (int)SeverityError.Critical);
                    response.value = null;
                    return response;
                }
            }
            parametersIn.Add("mui", token);

            //PropertyInfo[] properties = typeof(UserQuery).GetProperties();
            //foreach (PropertyInfo property in properties)
            //{
            //    var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
            //    string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
            //    //string NombreAtributo = property.Name;
            //    var Valor = property.GetValue(userQuery);
            //    parametersIn.Add(NombreAtributo, Valor);
            //}

            try
            {
                //SortedList parametersIn = null;
                //SortedList parametersOut = null;
                SortedList plateDataList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUserAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
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
                            Logger_AddLogMessage(string.Format("QueryUserAPI::Error - Bad hash: parametersIn= {0}, error = {1}", SortedListToString(parametersIn), "Result_Error_InvalidAuthenticationHash"), LoggerSeverities.Error);
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.value = null; //Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                            return response;
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            //if (parametersIn["contid"] != null)
                            //{
                            //    if (parametersIn["contid"].ToString().Trim().Length > 0)
                            //        nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            //}
                            // Set Contract Id to 0 to force all user queries to use the global users connection
                            nContractId = 0;

                            // Use token for verification
                            string strToken = parametersIn["mui"].ToString();

                            // Try to obtain user from token
                            nMobileUserId = GetUserFromToken(strToken, nContractId);

                            if (nMobileUserId <= 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryUserAPI::Error - Could not obtain user from token: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryUserAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryUserAPI::Error - Token not valid: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error(-230 - (int)tokenResult, (int)SeverityError.Critical);//new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get user data
                            if (!GetUserData(Convert.ToInt32(parametersIn["mui"]), out parametersOut, nContractId))
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserAPI::Error - Could not obtain user data: parametersIn= {0}, error = {1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }

                            // Get parking data for assigned plates
                            if (!GetPlateData(Convert.ToInt32(parametersIn["mui"]), out plateDataList, nContractId))
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserAPI::Error - Could not obtain plate data: parametersIn= {0}, error = {1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
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
                    Logger_AddLogMessage(string.Format("QueryUserAPI::Error: parametersIn= {0}, error = {1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }
            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUserAPI::Error: parametersIn= {0}, error = {1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            //return xmlOut;
            response.isSuccess = true;
            response.error = null;// new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

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
            response.value = usu;

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

        /// <summary>
        /// Send a mail with code to revover
        /// </summary>
        /// <param name="userRecover">User recover object</param>
        /// <returns>mail sended status</returns>
        [HttpPost]
        [Route("RecoverPasswordAPI")]
        public ResultRecoverPasswordInfo RecoverPasswordAPI([FromBody] UserRecover userRecover)
        {
            //string xmlOut = "";
            int nMobileUserId = -1;
            ResultRecoverPasswordInfo response = new ResultRecoverPasswordInfo();
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
                    if ((parametersIn["un"] == null || (parametersIn["un"] != null && parametersIn["un"].ToString().Length == 0)) &&
                        (parametersIn["email"] == null || (parametersIn["email"] != null && parametersIn["email"].ToString().Length == 0)))
                    {
                        //iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error - Missing parameter: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_UserName_Email, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    //else if (parametersIn["contid"] == null || (parametersIn["contid"].ToString().Length == 0))
                    //{
                    //    //iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                    //    Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error - Missing parameter: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                    //    response.isSuccess = false;
                    //    response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, (int)SeverityError.Critical);
                    //    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    //    return response;
                    //}
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
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.value = null; //Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
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
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Mobile_User_Not_Found, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Mobile_User_Not_Found).ToString();
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
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_OPS_Error, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_OPS_Error).ToString();
                                return response;
                            }

                            Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Assigned recovery code {0} to user {1}", strRecoveryCode, nMobileUserId), LoggerSeverities.Info);

                            // Send email to user with recovery code
                            string strEmail = GetUserEmail(nMobileUserId, nContractId);
                            if (strEmail.Length <= 0)
                            {
                                Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error - Could not obtain email for user {0}", nMobileUserId), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_OPS_Error;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_OPS_Error, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_OPS_Error).ToString();
                                return response;
                            }

                            if (!SendRecoveryEmail(strRecoveryCode, strEmail))
                            {
                                Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error - Could not send email to user {0} at {1}", nMobileUserId, strEmail), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_OPS_Error;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_OPS_Error, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_OPS_Error).ToString();
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
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }
            }
            catch (Exception e)
            {
                //iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("RecoverPasswordAPI::Error: parametersIn= {0}", parametersIn), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            //return iRes;
            response.isSuccess = true;
            response.error = null;// new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            response.value = Convert.ToInt32(ResultType.Result_OK).ToString();
            return response;
        }

        /*
         * 
         * The parameters of method VerifyRecoveryPasswordXML are:
            a.	xmlIn: xml containing input parameters of the method:
                    <arinpark_in>
                        <un>User name or email</un>
                        <recode>Recovery code</recode>
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
                h.  -31: Recovery code not found
                i.  -32: Invalid recovery code
                j.  -33: Recovery code expired
         
         * 
         * 
         */

        /// <summary>
        /// Verify code received from email
        /// </summary>
        /// <param name="userRecoverVerify">object UserRecoverVerify</param>
        /// <returns>code recover verification</returns>
        [HttpPost]
        [Route("VerifyRecoveryPasswordAPI")]
        public ResultVerifyRecoverPasswordInfo VerifyRecoveryPasswordAPI([FromBody] UserRecoverVerify userRecoverVerify)
        {
            int iRes = 0;
            int nMobileUserId = -1;
            ResultVerifyRecoverPasswordInfo response = new ResultVerifyRecoverPasswordInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            PropertyInfo[] properties = typeof(UserRecoverVerify).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(userRecoverVerify);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["un"] == null || (parametersIn["un"] != null && parametersIn["un"].ToString().Length == 0)) &&
                        (parametersIn["email"] == null || (parametersIn["email"] != null && parametersIn["email"].ToString().Length == 0))) 
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter_UserName_Email);
                        Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI::Error - Missing parameter: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_UserName_Email, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if ((parametersIn["recode"] == null) || (parametersIn["recode"].ToString().Length == 0))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter_Recode);
                        Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI::Error - Missing parameter: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_Recode, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    //else if ((parametersIn["contid"] == null) || (parametersIn["contid"].ToString().Length == 0))
                    //{
                    //    iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter_ContractId);
                    //    Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI::Error - Missing parameter: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                    //    response.isSuccess = false;
                    //    response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, (int)SeverityError.Critical);
                    //    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    //    return response;
                    //}
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
                            Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI::Error - Bad hash: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.value = null; //Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
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
                                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI::Error - Mobile user not found: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_Mobile_User_Not_Found;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Mobile_User_Not_Found, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Mobile_User_Not_Found).ToString();
                                return response;
                            }

                            Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI::Mobile user ID: {0}", nMobileUserId), LoggerSeverities.Info);

                            // Get current recovery code
                            string strCurRecoveryCode = GetUserRecoveryCode(nMobileUserId, nContractId);
                            if (strCurRecoveryCode.Length <= 0)
                            {
                                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI::Error - No recovery password was found for user {0}", nMobileUserId), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_Recovery_Code_Not_Found;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Recovery_Code_Not_Found, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Not_Found).ToString();
                                return response;
                            }

                            // Verify recovery code
                            if (!strCurRecoveryCode.Equals(parametersIn["recode"].ToString()))
                            {
                                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI::Error - Received recovery code {0} does not match current recovery code {1} for user {2}", parametersIn["recode"].ToString(), strCurRecoveryCode, nMobileUserId), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_Recovery_Code_Invalid;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Recovery_Code_Invalid, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Invalid).ToString();
                                return response;
                            }

                            // Check recovery code expiration date
                            if (!VerifyRecoveryCode(nMobileUserId, strCurRecoveryCode, nContractId))
                            {
                                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI::Error - Received recovery code {0} has expired for user {1}", strCurRecoveryCode, nMobileUserId), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_Recovery_Code_Expired;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Recovery_Code_Expired, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Expired).ToString();
                                return response;
                            }

                            iRes = (int)ResultType.Result_OK;
                        }
                    }
                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI::Error:  parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordAPI::Error:  parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            //return iRes;
            response.isSuccess = true;
            response.error = null;// new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            response.value = Convert.ToInt32(ResultType.Result_OK).ToString();
            return response;
        }

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

        /// <summary>
        /// Change password to user
        /// </summary>
        /// <param name="userRecoverVerify">Object UserRecoverVerify</param>
        /// <returns>new token generated</returns>
        [HttpPost]
        [Route("ChangePasswordAPI")]
        public ResultChangePasswordInfo ChangePasswordAPI([FromBody] UserChangePassword userChangePassword)
        {
            int nMobileUserId = -1;
            string strToken = ResultType.Result_Error_Generic.ToString();
            ResultChangePasswordInfo response = new ResultChangePasswordInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            PropertyInfo[] properties = typeof(UserChangePassword).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(userChangePassword);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("ChangePasswordAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["un"] == null || (parametersIn["un"] != null && parametersIn["un"].ToString().Length == 0)) &&
                        (parametersIn["email"] == null || (parametersIn["email"] != null && parametersIn["email"].ToString().Length == 0)))
                    {
                        Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        //return Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_UserName_Email, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if ((parametersIn["pw"] == null) || (parametersIn["pw"].ToString().Length == 0))
                    {
                        Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        //return Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_Password, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if ((parametersIn["recode"] == null) || (parametersIn["recode"].ToString().Length == 0))
                    {
                        Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        //return Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_Recode, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    //else if ((parametersIn["contid"] == null) || (parametersIn["contid"].ToString().Length == 0))
                    //{
                    //    Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                    //    //return Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    //    response.isSuccess = false;
                    //    response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, (int)SeverityError.Critical);
                    //    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                    //    return response;
                    //}
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
                            Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - Bad hash: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                            //return Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.value = null; //Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
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
                                //string xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Mobile_User_Not_Found);
                                Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - Mobile user not found: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), "Result_Error_Mobile_User_Not_Found"), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Mobile_User_Not_Found, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Mobile_User_Not_Found).ToString();
                                return response;
                            }
                            Logger_AddLogMessage(string.Format("ChangePasswordAPI::Mobile user ID: {0}", nMobileUserId), LoggerSeverities.Info);

                            // Get current recovery code
                            string strCurRecoveryCode = GetUserRecoveryCode(nMobileUserId, nContractId);
                            if (strCurRecoveryCode.Length <= 0)
                            {
                                Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - No recovery password was found for user {0}", nMobileUserId), LoggerSeverities.Error);
                                //return Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Not_Found).ToString();
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Recovery_Code_Not_Found, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Not_Found).ToString();
                                return response;
                            }

                            // Verify recovery code
                            if (!strCurRecoveryCode.Equals(parametersIn["recode"].ToString()))
                            {
                                Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - Received recovery code {0} does not match current recovery code {1} for user {2}", parametersIn["recode"].ToString(), strCurRecoveryCode, nMobileUserId), LoggerSeverities.Error);
                                //return Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Invalid).ToString();
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Recovery_Code_Invalid, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Invalid).ToString();
                                return response;
                            }

                            // Check recovery code expiration date
                            if (!VerifyRecoveryCode(nMobileUserId, strCurRecoveryCode, nContractId))
                            {
                                Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - Received recovery code {0} has expired for user {1}", strCurRecoveryCode, nMobileUserId), LoggerSeverities.Error);
                                //return Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Expired).ToString();
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Recovery_Code_Expired, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Recovery_Code_Expired).ToString();
                                return response;
                            }

                            // Generate authorization token
                            strToken = GetNewToken();

                            if (!UpdateWebCredentials(nMobileUserId, strToken, parametersIn["pw"].ToString(), nContractId))
                            {
                                Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - Could not update web credentials: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }
                        }
                    }
                }
                else
                {
                    Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - Incorrect input format: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                    //return Convert.ToInt32(rt).ToString();
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }
            }
            catch (Exception e)
            {
                strToken = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                Logger_AddLogMessage(string.Format("ChangePasswordAPI::Error - {0}: parametersIn= {1}", e.Message, SortedListToString(parametersIn)), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            //return strToken;
            response.isSuccess = true;
            response.error = null;// new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            response.value = strToken;
            return response;
        }

        /*
        * 
        * The parameters of method RegisterUserXML are:
        a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <un>Username</un>
                <pw>Password</pw>
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

        b.	Result: is an integer with the next possible values:
            a.	>0: Mobile user id
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error
            g.	-21: User name already registered
            h.	-22: e-mail already registered

        * 
        * 
    */

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="userRegister">Object UserRegister</param>
        /// <returns>User id</returns>
        [HttpPost]
        [Route("RegisterUserAPI")]
        public ResultRegisterUserInfo RegisterUserAPI([FromBody] UserRegister userRegister)
        {
            int nMobileUserId = (int)ResultType.Result_Error_Generic;
            string strToken = "";

            ResultRegisterUserInfo response = new ResultRegisterUserInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            SortedList notificationList = new SortedList();
            SortedList plateList = new SortedList();
            int numPlates = 0;
            PropertyInfo[] properties = typeof(UserRegister).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                if (NombreAtributo == "plates" && userRegister.plates != null)
                {
                    foreach (Plate plate in userRegister.plates)
                    {
                        plateList.Add("p" + numPlates, plate.plate);
                        numPlates++;
                    }
                    parametersIn.Add("plates", plateList);
                }
                else if (NombreAtributo == "notifications" && userRegister.notifications != null)
                {
                    PropertyInfo[] propertiesNot = typeof(Notification).GetProperties();
                    foreach (PropertyInfo propertyNot in propertiesNot)
                    {
                        var attributeNot = propertyNot.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                        string NombreAtributoNot = (attribute == null) ? propertyNot.Name : attributeNot.DisplayName;
                        parametersIn.Add(NombreAtributoNot, propertyNot.GetValue(userRegister.notifications));
                    }
                }
                else if (NombreAtributo == "notifications" && userRegister.notifications == null)
                {
                    parametersIn.Add("ba", 1); parametersIn.Add("fn", 1); parametersIn.Add("re", 1); parametersIn.Add("unp", 1);
                    parametersIn.Add("q_ba", 300); parametersIn.Add("t_unp", 10);
                }
                else
                {
                    var Valor = property.GetValue(userRegister);
                    parametersIn.Add(NombreAtributo, Valor);
                }
            }

            try
            {
                //SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("RegisterUserAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["em"] == null) || (parametersIn["em"].ToString().Length == 0))
                    {
                        Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        //return (int)ResultType.Result_Error_Missing_Input_Parameter;
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_Email, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    //else if ((parametersIn["un"] == null) || (parametersIn["un"].ToString().Length == 0))
                    //{
                        //Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        ////return (int)ResultType.Result_Error_Missing_Input_Parameter;
                        //response.isSuccess = false;
                        //response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_UserName, (int)SeverityError.Critical);
                        //response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        //return response;
                    //}
                    else if ((parametersIn["pw"] == null) || (parametersIn["pw"].ToString().Length == 0))
                    {
                        Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        //return (int)ResultType.Result_Error_Missing_Input_Parameter;
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_Password, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    //else if ((parametersIn["fs"] == null) || (parametersIn["fs"].ToString().Length == 0))
                    //{
                        //Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        ////return (int)ResultType.Result_Error_Missing_Input_Parameter;
                        //response.isSuccess = false;
                        //response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_FirstSurname, (int)SeverityError.Critical);
                        //response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        //return response;
                    //}
                    //else if ((parametersIn["na"] == null) || (parametersIn["na"].ToString().Length == 0))
                    //{
                        //Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        ////return (int)ResultType.Result_Error_Missing_Input_Parameter;
                        //response.isSuccess = false;
                        //response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_Name, (int)SeverityError.Critical);
                        //response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        //return response;
                    //}
                    //else if ((parametersIn["mmp"] == null) || (parametersIn["mmp"].ToString().Length == 0))
                    //{
                        //Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        ////return (int)ResultType.Result_Error_Missing_Input_Parameter;
                        //response.isSuccess = false;
                        //response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_MainMobilePhone, (int)SeverityError.Critical);
                        //response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        //return response;
                    //}
                    else if ((parametersIn["plates"] == null))
                    {
                        Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        //return (int)ResultType.Result_Error_Missing_Input_Parameter;
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_No_Plates, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else
                    {
                        if ((parametersIn["un"] == null) || (parametersIn["un"].ToString().Length == 0))
                            parametersIn["un"] = parametersIn["em"];

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
                            Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - Bad hash: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                            //return (int)ResultType.Result_Error_InvalidAuthenticationHash;
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.value = null; //Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
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

                            if (CheckMobileUserName("0", parametersIn["un"].ToString(), nContractId) != 0)
                            {
                                Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - User name already registered: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_Mobile_User_Already_Registered;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Mobile_User_Already_Registered, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Mobile_User_Already_Registered).ToString();
                                return response;
                            }

                            if (CheckMobileUserEmail("0", parametersIn["em"].ToString(), nContractId) != 0)
                            {
                                Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - Email already registered: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return (int)ResultType.Result_Error_Mobile_User_Email_Already_Registered;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Mobile_User_Email_Already_Registered, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Mobile_User_Email_Already_Registered).ToString();
                                return response;
                            }

                            nMobileUserId = AddMobileUser(parametersIn, out strToken, nContractId);

                            if (nMobileUserId > 0)
                            {
                                Logger_AddLogMessage(string.Format("RegisterUserAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                if (parametersIn["na"] == null) parametersIn["na"] = parametersIn["em"];
                                if (parametersIn["fs"] == null) parametersIn["fs"] = "";

                                SendConfEmail(strToken, parametersIn["na"].ToString(), parametersIn["fs"].ToString(), parametersIn["em"].ToString());
                            }
                            else
                                Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - Failed to add user: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        }
                    }
                }
                else
                {
                    Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - Incorrect input format: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                    //return (int)rt;
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }
            }
            catch (Exception e)
            {
                nMobileUserId = (int)ResultType.Result_Error_Generic;
                Logger_AddLogMessage(string.Format("RegisterUserAPI::Error - {0}: parametersIn= {1}", e.Message, SortedListToString(parametersIn)), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            //return nMobileUserId;
            response.isSuccess = true;
            response.error = null;// new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            response.value = nMobileUserId +"";
            return response;
        }

        /*
         * The parameters of method QueryUserCreditXML are:

            a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <mui>Mobile user id (authorization token)</mui>
                    <ah>authentication hash</ah> - *This parameter is optional
	            </arinpark_in>

            b.	Result: is an integer with the next possible values:
                a.	>0: Credit total expressed in Euro cents
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error.)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
                g.	-20: User not found.

         */

        /// <summary>
        /// return user credit in euro cents
        /// </summary>
        /// <param name="userQuery">Object UserQuery with authorizationToken to request</param>
        /// <returns>
        ///>0: Credit total expressed in Euro cents 
        ///-1: Invalid authentication hash
        ///-9: Generic Error (for example database or execution error.)
        ///-10: Invalid input parameter
        ///-11: Missing input parameter
        ///-12: OPS System error
        ///-20: Mobile user not found
        /// </returns>
        [HttpGet]
        [Route("QueryUserCreditAPI")]
        //public ResultCreditUserInfo QueryUserCreditAPI([FromBody] UserQuery userQuery)
        public ResultCreditUserInfo QueryUserCreditAPI()
        {
            int iRes = 0;

            ResultCreditUserInfo response = new ResultCreditUserInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            string token;
            if (!TokenRequest.TryTokenRequest(Request, out token))
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_No_Bearer_Token);
                Logger_AddLogMessage(string.Format("QueryUserCreditAPI::Error: No Bearer Token, iOut={0}", iRes), LoggerSeverities.Error);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_No_Bearer_Token, (int)SeverityError.Critical);
                response.value = null;
                return response;
            }
            else
            {
                TokenValidationResult tokenResult = DefaultVerification(token);
                if (tokenResult != TokenValidationResult.Passed)
                {
                    iRes = -230 - (int)tokenResult;
                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error: Token invalid, iOut={0}", iRes), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error(iRes, (int)SeverityError.Critical);
                    response.value = null;
                    return response;
                }
            }
            parametersIn.Add("mui", token);

            //PropertyInfo[] properties = typeof(UserQuery).GetProperties();
            //foreach (PropertyInfo property in properties)
            //{
            //    var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
            //    string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
            //    //string NombreAtributo = property.Name;
            //    var Valor = property.GetValue(userQuery);
            //    parametersIn.Add(NombreAtributo, Valor);
            //}

            try
            {
                //SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUserCreditAPI: xmlIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken);
                        Logger_AddLogMessage(string.Format("QueryUserCreditAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
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
                            iRes = Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash);
                            Logger_AddLogMessage(string.Format("QueryUserCreditAPI::Error - Bad hash: parametersIn= {0}, iOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.value = null; //Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                            return response;
                        }
                        else
                        {
                            // Determine contract ID if any
                            int nContractId = 0;
                            //if (parametersIn["contid"] != null)
                            //{
                            //    if (parametersIn["contid"].ToString().Trim().Length > 0)
                            //        nContractId = Convert.ToInt32(parametersIn["contid"].ToString());
                            //}
                            // Set Contract Id to 0 to force all user queries to use the global users connection
                            nContractId = 0;

                            // Use token for verification
                            string strToken = parametersIn["mui"].ToString();

                            // Try to obtain user from token
                            int nMobileUserId = GetUserFromToken(strToken, nContractId);

                            if (nMobileUserId <= 0)
                            {
                                Logger_AddLogMessage(string.Format("QueryUserCreditAPI::Error - Could not obtain user from token: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryUserCreditAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                Logger_AddLogMessage(string.Format("QueryUserCreditAPI::Error - Token not valid: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                                response.isSuccess = false;
                                response.error = new Error(-230 - (int)tokenResult, (int)SeverityError.Critical);//new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get user credit
                            iRes = GetUserCredit(Convert.ToInt32(parametersIn["mui"]), nContractId);
                            if (iRes < 0)
                            {
                                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserCreditAPI::Error - Could not obtain user credit: parametersIn= {0}, iOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                                //return iRes;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }
                        }
                    }
                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("QueryUserCreditAPI::Error: parametersIn= {0}, iOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)rt, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(rt).ToString();
                    return response;
                }

            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUserCreditAPI::Error: parametersIn= {0}, iOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            response.isSuccess = true;
            response.error = null;// new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            response.value = iRes + "";
            return response;

            //return iRes;
        }

        /*
         The parameters of method RechargeUserCreditXML are:

            a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <mui>Mobile user id (authorization token)</mui>
                    <am>Amount (expressed in Euro cents)</am>
                    <cid>Cloud ID. ID used in official notification clouds<cid>
                    <os>Operative System (1: Android, 2: iOS)</os>
                    <sim>Simulate Sermepa response (0 = false, 1 = true) *Optional and only for testing
                    <ah>authentication hash</ah> - *This parameter is optional	            
                </arinpark_in>


            b.	b.	Result: is also a string containing an xml with the result of the method:
                <arinpark_out>
                    <r>Result of the method</r>
                    <or>Order ID</or>
                    <mu>Notification URL for payment gateway response</mu>
                </arinpark_out>

                The tag <r> of the method will have these possible values:
                    a.	1: Data come after this tag
                    b.	-1: Invalid authentication hash
                    c.	-9: Generic Error (for example database or execution error.)
                    d.	-10: Invalid input parameter
                    e.	-11: Missing input parameter
                    f.	-12: OPS System error
                    g.	-20: User not found.
         */

        /// <summary>
        /// return information for user recharge credit
        /// </summary>
        /// <param name="userRechargeQuery">Object UserRechargeQuery with amount and user information</param>
        /// <returns>
        ///1: User recharge credit information
        ///-1: Invalid authentication hash
        ///-9: Generic Error (for example database or execution error)
        ///-10: Invalid input parameter
        ///-11: Missing input parameter
        ///-12: OPS System error
        ///-20: User not found.
        /// </returns>
        [HttpPost]
        [Route("RechargeUserCreditAPI")]
        public ResultUserRechargeInfo RechargeUserCreditAPI([FromBody] UserRechargeQuery userRechargeQuery)
        {
            //string xmlOut = "";

            ResultUserRechargeInfo response = new ResultUserRechargeInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            string token;
            if (!TokenRequest.TryTokenRequest(Request, out token))
            {
                int iRes = Convert.ToInt32(ResultType.Result_Error_No_Bearer_Token);
                Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error: No Bearer Token, iOut={0}", iRes), LoggerSeverities.Error);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_No_Bearer_Token, (int)SeverityError.Critical);
                response.value = null;
                return response;
            }
            else
            {
                TokenValidationResult tokenResult = DefaultVerification(token);
                if (tokenResult != TokenValidationResult.Passed)
                {
                    int iRes = -230 - (int)tokenResult;
                    Logger_AddLogMessage(string.Format("QueryUserOperationsAPI::Error: Token invalid, iOut={0}", iRes), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error(iRes, (int)SeverityError.Critical);
                    response.value = null;
                    return response;
                }
            }
            parametersIn.Add("mui", token);

            PropertyInfo[] properties = typeof(UserRechargeQuery).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(userRechargeQuery);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                //SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("RechargeUserCreditAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if (parametersIn["mui"] == null || (parametersIn["mui"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if (parametersIn["am"] == null || (parametersIn["am"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_AmountInCents, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if (parametersIn["cid"] == null || (parametersIn["cid"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_CloudToken, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if (parametersIn["os"] == null || (parametersIn["os"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_OperativeSystem, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if (parametersIn["contid"] == null || (parametersIn["contid"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
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
                            Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error - Bad hash: parametersIn= {0}, error={1}", SortedListToString(parametersIn), "Result_Error_InvalidAuthenticationHash"), LoggerSeverities.Error);
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.value = null; //Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
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
                            int nMobileUserId = GetUserFromToken(strToken, nContractId);

                            if (nMobileUserId <= 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error - Could not obtain user from token: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }
                            else
                                Logger_AddLogMessage(string.Format("RechargeUserCreditAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error - Token not valid: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error(-230 - (int)tokenResult, (int)SeverityError.Critical);//new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                return response;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Obtain transaction data
                            if (!GetTransactionData(parametersIn["mui"].ToString(), Convert.ToInt32(parametersIn["am"]), out parametersOut, nContractId))
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error - Could not obtain transaction data: parametersIn= {0}, error={1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }

                            // Update cloud information
                            if (!UpdateWebCredentials(Convert.ToInt32(parametersIn["mui"]), parametersIn["cid"].ToString(), Convert.ToInt32(parametersIn["os"]), nContractId))
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error - Could not update web credentials: parametersIn= {0}, error={1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }

                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            //xmlOut = GenerateXMLOuput(parametersOut);

                            if (parametersIn["sim"] != null)
                            {
                                if (Convert.ToInt32(parametersIn["sim"]) == 1)
                                    SimulateResponse(parametersOut["or"].ToString(), parametersIn["mui"].ToString(), Convert.ToInt32(parametersIn["am"]), nContractId);
                            }
                        }
                    }
                }
                else
                {
                    //xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error: parametersIn= {0}, error={1}", SortedListToString(parametersIn), rt.ToString()), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)rt, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(rt).ToString();
                    return response;
                }
            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("RechargeUserCreditAPI::Error: parametersIn= {0}, error={1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            response.isSuccess = true;
            response.error = null;// new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

            UserRechargeInfo userRechargeInfo = new UserRechargeInfo();
            ConfigMapModel configMapModel = new ConfigMapModel();

            var config = configMapModel.configUserRecharge();
            IMapper iMapper = config.CreateMapper();
            userRechargeInfo = iMapper.Map<SortedList, UserRechargeInfo>((SortedList)parametersOut);

            response.value = userRechargeInfo;
            return response;

            //return xmlOut;
        }

        /*
         * The parameters of method QueryUserReportXML are:

        a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <mui>Mobile user id (authorization token)</mui>
                <d1>Report start date (Format: hh24missddMMYY)</d1>
                <d2>Report end date (Format: hh24missddMMYY)</d2>
                <ots>Filter. List of Operation types 
                    <ot>(1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing)</ot>
                    …
                    <ot>(1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing)</ot>
                </ots>
                <mail>User email</mail>
                <rfmt>Report Format (1: PDF, something else?)</rfmt>
                <ah>authentication hash</ah> - *This parameter is optional
	        </arinpark_in>

        b.	Result: is an integer with the next possible values:<<
            a.	1: saved without errors
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error
            g.	-20: Mobile user id not found

         */

        /// <summary>
        /// return report with user operations information
        /// </summary>
        /// <param name="userReportQuery">Object UserReportQuery with dates and user information to request</param>
        /// <returns>
        ///1: saved without errors
        ///-1: Invalid authentication hash
        ///-9: Generic Error (for example database or execution error)
        ///-10: Invalid input parameter
        ///-11: Missing input parameter
        ///-12: OPS System error
        ///-20: User not found.
        /// </returns>
        [HttpPost]
        [Route("QueryUserReportAPI")]
        public ResultUserReportInfo QueryUserReportAPI([FromBody] UserReportQuery userReportQuery)
        {
            int iRes = 0;

            ResultUserReportInfo response = new ResultUserReportInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            string token;
            if (!TokenRequest.TryTokenRequest(Request, out token))
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_No_Bearer_Token);
                Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error: No Bearer Token, iOut={0}", iRes), LoggerSeverities.Error);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_No_Bearer_Token, (int)SeverityError.Critical);
                response.value = null;
                return response;
            }
            else
            {
                TokenValidationResult tokenResult = DefaultVerification(token);
                if (tokenResult != TokenValidationResult.Passed)
                {
                    iRes = -230 - (int)tokenResult;
                    Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error: Token invalid, iOut={0}", iRes), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error(iRes, (int)SeverityError.Critical);
                    response.value = null;
                    return response;
                }
            }
            parametersIn.Add("mui", token);

            PropertyInfo[] properties = typeof(UserReportQuery).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(userReportQuery);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                SortedList parametersReport = new SortedList();
                SortedList parametersUser = null;
                SortedList operationList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUserReportAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if (parametersIn["mui"] == null || (parametersIn["mui"].ToString().Length == 0))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken);
                        Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_AuthorizationToken, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if (parametersIn["d1"] == null || (parametersIn["d1"].ToString().Length == 0))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter_DateStart);
                        Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_DateStart, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if (parametersIn["d2"] == null || (parametersIn["d2"].ToString().Length == 0))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter_DateEnd);
                        Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_DateEnd, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if (parametersIn["mail"] == null || (parametersIn["mail"].ToString().Length == 0))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter_Email);
                        Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_Email, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if (parametersIn["rfmt"] == null || (parametersIn["rfmt"].ToString().Length == 0))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter_ReportFormat);
                        Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_ReportFormat, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        return response;
                    }
                    else if (parametersIn["contid"] == null || (parametersIn["contid"].ToString().Length == 0))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter_ContractId);
                        Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter_ContractId, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
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
                            iRes = Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash);
                            Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Bad hash: parametersIn= {0}, iOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_InvalidAuthenticationHash, (int)SeverityError.Critical);
                            response.value = null; //Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
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
                                    Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Could not obtain user from token: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                    //return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                    return response;
                                }
                                else
                                    Logger_AddLogMessage(string.Format("QueryUserReportAPI: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                // Determine if token is valid
                                TokenValidationResult tokenResult = DefaultVerification(strToken);

                                if (tokenResult != TokenValidationResult.Passed)
                                {
                                    Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Token not valid: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                                    //return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                                    response.isSuccess = false;
                                    response.error = new Error(-230 - (int)tokenResult, (int)SeverityError.Critical);//new Error((int)ResultType.Result_Error_Invalid_Login, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                    return response;
                                }
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get parking data for assigned plates
                            string strContractList = ConfigurationManager.AppSettings["ContractList"].ToString();
                            if (strContractList.Length == 0)
                                strContractList = "0," + parametersIn["contid"].ToString();
                            if (!GetUserOperationReportData(parametersIn, DATE_FORMAT_RANGE, out operationList, strContractList))
                            {
                                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Could not obtain operation data: parametersIn= {0}, iOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                                //return iRes;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                return response;
                            }

                            if (operationList.Count >= 0)
                            {
                                parametersReport["mui"] = parametersIn["mui"];
                                parametersReport["d1"] = parametersIn["d1"];
                                parametersReport["d2"] = parametersIn["d2"];
                                parametersReport["oper"] = operationList;

                                string strCif = "";
                                GetCIF(out strCif, nContractId);
                                if (strCif.Length > 0)
                                    parametersReport["cif"] = "CIF: " + strCif;
                                else
                                    parametersReport["cif"] = " ";

                                // Obtain user data for report
                                // Send contract Id as 0 so that it uses the global users connection
                                GetUserData(Convert.ToInt32(parametersReport["mui"].ToString()), out parametersUser, 0);

                                // Generate report file
                                string strFilePath = ConfigurationManager.AppSettings["ReportPath"].ToString() + ConfigurationManager.AppSettings["ReportFilePrefix"].ToString() + parametersReport["mui"] + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                                if (GeneratePDF(parametersReport, parametersUser, strFilePath))
                                {
                                    if (!SendEmail(parametersIn["mail"].ToString(), strFilePath))
                                    {
                                        iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                        Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Error sending email: parametersIn= {0}, iOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                                        //return iRes;
                                        response.isSuccess = false;
                                        response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                        return response;
                                    }
                                }
                                else
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error - Error generating PDF: parametersIn= {0}, iOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                                    //return iRes;
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    return response;
                                }
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryUserReportAPI::No operations were found: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                            iRes = Convert.ToInt32(ResultType.Result_OK);
                        }
                    }
                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error: parametersIn= {0}, iOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)rt, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(rt).ToString();
                    return response;
                }
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUserReportAPI::Error: parametersIn= {0}, iOut={1}", SortedListToString(parametersIn), iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            response.isSuccess = true;
            response.error = null;// new Error((int)ResultType.Result_OK, (int)SeverityError.Critical);
            response.value = iRes + "";
            return response;

            //return iRes;
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
         * The parameters of method VerifyRecoveryPasswordXML are:
            a.	xmlIn: xml containing input parameters of the method:
                    <arinpark_in>
                        <un>User name or email</un>
                        <recode>Recovery code</recode>
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
                h.  -31: Recovery code not found
                i.  -32: Invalid recovery code
                j.  -33: Recovery code expired
         
         * 
         * 
         */

        /*
        [HttpPost]
        [Route("VerifyRecoveryPasswordXML")]
        public int VerifyRecoveryPasswordXML(string xmlIn)
        {
            int iRes = 0;
            try
            {
                SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["un"] == null) || (parametersIn["un"].ToString().Length == 0)
                        || (parametersIn["recode"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordXML::Error - Bad hash: xmlIn= {0}, xmlOut={1}", xmlIn, iRes), LoggerSeverities.Error);
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
                                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordXML::Error - Mobile user not found: xmlIn= {0}, xmlOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                return (int)ResultType.Result_Error_Mobile_User_Not_Found;
                            }

                            Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordXML::Mobile user ID: {0}", nMobileUserId), LoggerSeverities.Info);

                            // Get current recovery code
                            string strCurRecoveryCode = GetUserRecoveryCode(nMobileUserId, nContractId);
                            if (strCurRecoveryCode.Length <= 0)
                            {
                                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordXML::Error - No recovery password was found for user {0}", nMobileUserId), LoggerSeverities.Error);
                                return (int)ResultType.Result_Error_Recovery_Code_Not_Found;
                            }

                            // Verify recovery code
                            if (!strCurRecoveryCode.Equals(parametersIn["recode"].ToString()))
                            {
                                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordXML::Error - Received recovery code {0} does not match current recovery code {1} for user {2}", parametersIn["recode"].ToString(), strCurRecoveryCode, nMobileUserId), LoggerSeverities.Error);
                                return (int)ResultType.Result_Error_Recovery_Code_Invalid;
                            }

                            // Check recovery code expiration date
                            if (!VerifyRecoveryCode(nMobileUserId, strCurRecoveryCode, nContractId))
                            {
                                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordXML::Error - Received recovery code {0} has expired for user {1}", strCurRecoveryCode, nMobileUserId), LoggerSeverities.Error);
                                return (int)ResultType.Result_Error_Recovery_Code_Expired;
                            }

                            iRes = (int)ResultType.Result_OK;
                        }
                    }
                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, iRes), LoggerSeverities.Error);
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

        /*
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
        */

        /*
        * 
        * The parameters of method RegisterUserXML are:
        a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <un>Username</un>
                <pw>Password</pw>
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

        b.	Result: is an integer with the next possible values:
            a.	>0: Mobile user id
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error
            g.	-21: User name already registered
            h.	-22: e-mail already registered

        * 
        * 
    */

        /*
        [HttpPost]
        [Route("RegisterUserXML")]
        public int RegisterUserXML(string xmlIn)
        {
            int nMobileUserId = (int)ResultType.Result_Error_Generic;
            string strToken = "";

            try
            {
                SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("RegisterUserXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["un"] == null) ||
                        (parametersIn["pw"] == null) ||
                        (parametersIn["em"] == null) ||
                        (parametersIn["fs"] == null) ||
                        (parametersIn["na"] == null) ||
                        (parametersIn["mmp"] == null) ||
                        (parametersIn["plates"] == null) ||
                        (parametersIn["fn"] == null) ||
                        (parametersIn["unp"] == null) ||
                        (parametersIn["t_unp"] == null) ||
                        (parametersIn["re"] == null) ||
                        (parametersIn["ba"] == null) ||
                        (parametersIn["q_ba"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        Logger_AddLogMessage(string.Format("RegisterUserXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                        return (int)ResultType.Result_Error_Missing_Input_Parameter;
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
                            Logger_AddLogMessage(string.Format("RegisterUserXML::Error - Bad hash: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                            return (int)ResultType.Result_Error_InvalidAuthenticationHash;
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

                            if (CheckMobileUserName("0", parametersIn["un"].ToString(), nContractId) != 0)
                            {
                                Logger_AddLogMessage(string.Format("RegisterUserXML::Error - User name already registered: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return (int)ResultType.Result_Error_Mobile_User_Already_Registered;
                            }

                            if (CheckMobileUserEmail("0", parametersIn["em"].ToString(), nContractId) != 0)
                            {
                                Logger_AddLogMessage(string.Format("RegisterUserXML::Error - Email already registered: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return (int)ResultType.Result_Error_Mobile_User_Email_Already_Registered;
                            }

                            nMobileUserId = AddMobileUser(parametersIn, out strToken, nContractId);

                            if (nMobileUserId > 0)
                            {
                                Logger_AddLogMessage(string.Format("RegisterUserXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                SendConfEmail(strToken, parametersIn["na"].ToString(), parametersIn["fs"].ToString(), parametersIn["em"].ToString());
                            }
                            else
                                Logger_AddLogMessage(string.Format("RegisterUserXML::Error - Failed to add user: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                        }
                    }
                }
                else
                {
                    Logger_AddLogMessage(string.Format("RegisterUserXML::Error - Incorrect input format: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                    return (int)rt;
                }
            }
            catch (Exception e)
            {
                nMobileUserId = (int)ResultType.Result_Error_Generic;
                Logger_AddLogMessage(string.Format("RegisterUserXML::Error - {0}: xmlIn= {1}", e.Message, xmlIn), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return nMobileUserId;
        }
        */

        /*
         * The parameters of method QueryUserCreditXML are:

            a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <mui>Mobile user id (authorization token)</mui>
                    <ah>authentication hash</ah> - *This parameter is optional
	            </arinpark_in>

            b.	Result: is an integer with the next possible values:
                a.	>0: Credit total expressed in Euro cents
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error.)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
                g.	-20: User not found.

         */

        /*
        [HttpPost]
        [Route("QueryUserCreditXML")]
        public int QueryUserCreditXML(string xmlIn)
        {
            int iRes = 0;
            try
            {
                SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUserCreditXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) ||
                        (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["contid"] == null))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryUserCreditXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("QueryUserCreditXML::Error - Bad hash: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
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
                                Logger_AddLogMessage(string.Format("QueryUserCreditXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryUserCreditXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                Logger_AddLogMessage(string.Format("QueryUserCreditXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get user credit
                            iRes = GetUserCredit(Convert.ToInt32(parametersIn["mui"]), nContractId);
                            if (iRes < 0)
                            {
                                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserCreditXML::Error - Could not obtain user credit: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                return iRes;
                            }
                        }
                    }
                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("QueryUserCreditXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                }

            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUserCreditXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return iRes;
        }
        */

        /*
         The parameters of method RechargeUserCreditXML are:

            a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <mui>Mobile user id (authorization token)</mui>
                    <am>Amount (expressed in Euro cents)</am>
                    <cid>Cloud ID. ID used in official notification clouds<cid>
                    <os>Operative System (1: Android, 2: iOS)</os>
                    <sim>Simulate Sermepa response (0 = false, 1 = true) *Optional and only for testing
                    <ah>authentication hash</ah> - *This parameter is optional	            
                </arinpark_in>


            b.	b.	Result: is also a string containing an xml with the result of the method:
                <arinpark_out>
                    <r>Result of the method</r>
                    <or>Order ID</or>
                    <mu>Notification URL for payment gateway response</mu>
                </arinpark_out>

                The tag <r> of the method will have these possible values:
                    a.	1: Data come after this tag
                    b.	-1: Invalid authentication hash
                    c.	-9: Generic Error (for example database or execution error.)
                    d.	-10: Invalid input parameter
                    e.	-11: Missing input parameter
                    f.	-12: OPS System error
                    g.	-20: User not found.
         */

        /*
        [HttpPost]
        [Route("RechargeUserCreditXML")]
        public string RechargeUserCreditXML(string xmlIn)
        {
            string xmlOut = "";

            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("RechargeUserCreditXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if (parametersIn["mui"] == null || (parametersIn["mui"].ToString().Length == 0) ||
                        parametersIn["am"] == null ||
                        parametersIn["cid"] == null ||
                        parametersIn["os"] == null ||
                        parametersIn["contid"] == null)
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("RechargeUserCreditXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("RechargeUserCreditXML::Error - Bad hash: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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
                                Logger_AddLogMessage(string.Format("RechargeUserCreditXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }
                            else
                                Logger_AddLogMessage(string.Format("RechargeUserCreditXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("RechargeUserCreditXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Obtain transaction data
                            if (!GetTransactionData(parametersIn["mui"].ToString(), Convert.ToInt32(parametersIn["am"]), out parametersOut, nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("RechargeUserCreditXML::Error - Could not obtain transaction data: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Update cloud information
                            if (!UpdateWebCredentials(Convert.ToInt32(parametersIn["mui"]), parametersIn["cid"].ToString(), Convert.ToInt32(parametersIn["os"]), nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("RechargeUserCreditXML::Error - Could not update web credentials: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            xmlOut = GenerateXMLOuput(parametersOut);

                            if (parametersIn["sim"] != null)
                            {
                                if (Convert.ToInt32(parametersIn["sim"]) == 1)
                                    SimulateResponse(parametersOut["or"].ToString(), parametersIn["mui"].ToString(), Convert.ToInt32(parametersIn["am"]), nContractId);
                            }
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("RechargeUserCreditXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("RechargeUserCreditXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }
        */

        /*
         * The parameters of method QueryUserReportXML are:

        a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <mui>Mobile user id (authorization token)</mui>
                <d1>Report start date (Format: hh24missddMMYY)</d1>
                <d2>Report end date (Format: hh24missddMMYY)</d2>
                <ots>Filter. List of Operation types 
                    <ot>(1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing)</ot>
                    …
                    <ot>(1: Parking, 2: Extension, 3: Refund, 4: Fine payment, 5: Recharge, 7: Postpaid, 101: Resident payment, 102: Power recharge, 103: Bycing)</ot>
                </ots>
                <mail>User email</mail>
                <rfmt>Report Format (1: PDF, something else?)</rfmt>
                <ah>authentication hash</ah> - *This parameter is optional
	        </arinpark_in>

        b.	Result: is an integer with the next possible values:<<
            a.	1: saved without errors
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error
            g.	-20: Mobile user id not found

         */

        /*
        [HttpPost]
        [Route("QueryUserReportXML")]
        public int QueryUserReportXML(string xmlIn)
        {
            int iRes = 0;

            try
            {
                SortedList parametersIn = null;
                SortedList parametersReport = new SortedList();
                SortedList parametersUser = null;
                SortedList operationList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUserReportXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["d1"] == null) ||
                        (parametersIn["d2"] == null) ||
                        (parametersIn["mail"] == null) ||
                        (parametersIn["rfmt"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryUserReportXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("QueryUserReportXML::Error - Bad hash: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
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
                                    Logger_AddLogMessage(string.Format("QueryUserReportXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                                }
                                else
                                    Logger_AddLogMessage(string.Format("QueryUserReportXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                                // Determine if token is valid
                                TokenValidationResult tokenResult = DefaultVerification(strToken);

                                if (tokenResult != TokenValidationResult.Passed)
                                {
                                    Logger_AddLogMessage(string.Format("QueryUserReportXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                                }
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get parking data for assigned plates
                            string strContractList = ConfigurationManager.AppSettings["ContractList"].ToString();
                            if (strContractList.Length == 0)
                                strContractList = "0," + parametersIn["contid"].ToString();
                            if (!GetUserOperationReportData(parametersIn, DATE_FORMAT_RANGE, out operationList, strContractList))
                            {
                                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserReportXML::Error - Could not obtain operation data: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                return iRes;
                            }

                            if (operationList.Count >= 0)
                            {
                                parametersReport["mui"] = parametersIn["mui"];
                                parametersReport["d1"] = parametersIn["d1"];
                                parametersReport["d2"] = parametersIn["d2"];
                                parametersReport["oper"] = operationList;

                                string strCif = "";
                                GetCIF(out strCif, nContractId);
                                if (strCif.Length > 0)
                                    parametersReport["cif"] = "CIF: " + strCif;
                                else
                                    parametersReport["cif"] = " ";

                                // Obtain user data for report
                                // Send contract Id as 0 so that it uses the global users connection
                                GetUserData(Convert.ToInt32(parametersReport["mui"].ToString()), out parametersUser, 0);

                                // Generate report file
                                string strFilePath = ConfigurationManager.AppSettings["ReportPath"].ToString() + ConfigurationManager.AppSettings["ReportFilePrefix"].ToString() + parametersReport["mui"] + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                                if (GeneratePDF(parametersReport, parametersUser, strFilePath))
                                {
                                    if (!SendEmail(parametersIn["mail"].ToString(), strFilePath))
                                    {
                                        iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                        Logger_AddLogMessage(string.Format("QueryUserReportXML::Error - Error sending email: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                        return iRes;
                                    }
                                }
                                else
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryUserReportXML::Error - Error generating PDF: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryUserReportXML::No operations were found: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                            iRes = Convert.ToInt32(ResultType.Result_OK);
                        }
                    }
                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("QueryUserReportXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUserReportXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return iRes;
        }
        */

        #region private new methods

        private string SortedListToString(SortedList lista)
        {
            string texto = "";
            foreach (string kvp in lista.Keys)
                texto = texto + string.Format(" {0}: {1} ,", kvp , lista[kvp]);
            return texto;
        }

        #endregion

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
                Logger_AddLogMessage("FindInputParametersAPI::Exception", LoggerSeverities.Error);
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

        private bool GetTransactionData(string strMobileUserId, int nAmount, out SortedList parametersOut, int nContractId = 0)
        {
            bool bResult = false;
            parametersOut = null;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            int nOrder = -1;

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

                string strSQL = string.Format("SELECT SEQ_ORDER.NEXTVAL FROM DUAL");
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nOrder = dataReader.GetInt32(0);
                }

                if (nOrder > 0)
                {
                    parametersOut["or"] = nOrder.ToString();
                    parametersOut["mu"] = ConfigurationManager.AppSettings["MerchantUrl"].ToString();

                    strSQL = string.Format("INSERT INTO MOBILE_ORDERS (MO_ID, MO_MU_ID, MO_DATE, MO_HORA, MO_AMOUNT, MO_CURRENCY, MO_TERMINAL, MO_TRANSACTION_TYPE, MO_ORIGIN) VALUES ({0}, {1}, TO_CHAR(SYSDATE, 'DD/MM/YYYY'), TO_CHAR(SYSDATE, 'HH24:MI'), {2}, {3}, {4}, '{5}', {6})",
                        nOrder.ToString(), strMobileUserId, nAmount, ConfigurationManager.AppSettings["MerchantCurrency"].ToString(), ConfigurationManager.AppSettings["MerchantTerminal"].ToString(), ConfigurationManager.AppSettings["MerchantTranstactionType"].ToString(), ConfigurationManager.AppSettings["Order.Origin"].ToString());
                    oraCmd.CommandText = strSQL;

                    if (oraCmd.ExecuteNonQuery() > 0)
                        bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetTransactionData::Exception", LoggerSeverities.Error);
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

        private bool SimulateResponse(string strOrderId, string strMobileUserId, int nAmount, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            int nOrderId = -1;

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

                string strSQL = string.Format("SELECT SEQ_OPERATIONS.NEXTVAL FROM DUAL");
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nOrderId = dataReader.GetInt32(0);
                }

                if (nOrderId > 0)
                {
                    strSQL = string.Format("INSERT INTO OPERATIONS (OPE_ID, OPE_DOPE_ID, OPE_GRP_ID, OPE_UNI_ID, OPE_DPAY_ID, OPE_MOVDATE, OPE_VALUE, OPE_DART_ID, OPE_MOBI_USER_ID, OPE_DOPE_ID_VIS, OPE_OP_ONLINE, OPE_INSDATE, OPE_DPAY_ID_VIS, OPE_VALUE_VIS) VALUES ({3}, {0}, 70001, 5101, 4, SYSDATE, {1}, 4, {2}, {0}, 1, SYSDATE, 4, {1})",
                        ConfigurationManager.AppSettings["OperationsDef.Recharge"].ToString(), nAmount.ToString(), strMobileUserId, nOrderId.ToString());
                    oraCmd.CommandText = strSQL;

                    if (oraCmd.ExecuteNonQuery() > 0)
                    {
                        strSQL = string.Format("UPDATE MOBILE_USERS SET MU_FUNDS = MU_FUNDS + {0} WHERE MU_ID = {1}", nAmount.ToString(), strMobileUserId);
                        oraCmd.CommandText = strSQL;

                        if (oraCmd.ExecuteNonQuery() > 0)
                            bResult = true;
                    }
                }


                strSQL = string.Format("UPDATE MOBILE_USERS SET MU_FUNDS = MU_FUNDS + {0} WHERE MU_ID = {1}", nAmount.ToString(), strMobileUserId);
                oraCmd.CommandText = strSQL;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("SimulateResponse::Exception", LoggerSeverities.Error);
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

        private bool UpdateWebCredentials(int nMobileUserId, string strCloudId, int nOs, int nContractId = 0)
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

                string strSQL = string.Format("update MOBILE_USERS set mu_cloud_token = '{0}', mu_device_os = {1} WHERE MU_ID = {2}", strCloudId, nOs, nMobileUserId);

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

        bool SendConfEmail(string activationID, string name, string surname1, string email)
        {
            bool bResult = false;

            try
            {
                MailMessage MyMailMessage = new MailMessage();
                MyMailMessage.From = new MailAddress(ConfigurationManager.AppSettings["SendAddress"].ToString());
                MyMailMessage.To.Add(email);

                //Create Body
                string urlActivation = ConfigurationManager.AppSettings["APP_URL"];
                string emailSubject = "Confirmación activación cuenta ArinPark / Baieztapen posta ArinPark harpidetza";
                string emailHead = "<p>Estimad@ Sr./Sra. ";
                string emailBody = ":</p><p>Su solicitud ha sido procesada con éxito. Para poder acceder al Servicio de pago por móvil e internet del Estacionamiento de ArinPark debe activar su cuenta pulsando sobre el siguiente enlace: ";
                string emailAccountID = "Activar Cuenta </a> </p>";
                string emailAlternativeURL = "Si no puede acceder con el enlace anterior copie y pegue la siguiente dirección en su navegador web: ";
                string emailFeet = "<p>Si tiene alguna duda o consulta, puede contactar en: <a href=\"soporte.arinpark@gerteksa.eus\" target=\"_blank\" > soporte.arinpark@gerteksa.eus</a></p>";
                string emailPreHead2 = "<p><p><p>";
                string emailHead2 = " Jaun/Andere agurgarria";
                string emailBody2 = ":</p><p>Zure eskaera zuzen gauzatu da. ArinPark-eko aparkalekuetako mugikor eta internet bidezko ordainketa zerbitzura sartzeko zure kontua aktibatu beharko duzu ondorengo estekan klikatuz: ";
                string emailAccountID2 = "Aktibatu Kontua </a> </p>";
                string emailAlternativeURL2 = "Ezin baduzu aurreko helbidera sartu, kopiatu eta itsatsi hurrengo helbidea zure sare nabigatzailean: ";
                string emailFeet2 = "<p>Zalantza edo kontsultaren bat baduzu kontaktatu dezakezu ArinPark serbitzuarekin hurrengo posta elektronikoan: <a href=\"soporte.arinpark@gerteksa.eus\" target=\"_blank\" > soporte.arinpark@gerteksa.eus</a></p>";
                string emailPreHead3 = "<p><p><p>";
                string emailHead3 = "Madame/Monsieur ";
                string emailBody3 = ":</p><p>Nous avons donné suite à votre demande. Pour pouvoir accéder au Service de paiement du stationnement de ArinPark, par téléphone portable et via Internet, vous devez activer votre compte en cliquant sur le lien suivant: ";
                string emailAccountID3 = "Activer compte </a> </p>";
                string emailAlternativeURL3 = "Si vous ne pouvez pas y accéder par ce moyen, faites un copié-collé de l’adresse suivante dans votre Navigateur Internet: ";
                string emailFeet3 = "<p>Au cas où vous auriez des doutes ou suggestions, n'hésitez pas à nous contacter sur ArinPark: <a href=\"soporte.arinpark@gerteksa.eus\" target=\"_blank\" > soporte.arinpark@gerteksa.eus</a></p>";
                string linkActivation = "<a href=\"" + urlActivation + "ActivationAccount.aspx?TokenID=" + activationID + "\" target=\"_blank\" > ";

                string bodyMessage = emailHead + name + " " + surname1 +
                                     emailBody + linkActivation +
                                     emailAccountID +
                                     emailAlternativeURL + urlActivation + "ActivationAccount.aspx?TokenID=" + activationID +
                                     emailFeet;

                string bodyMessage2 = emailPreHead2 + name + " " + surname1 + emailHead2 +
                                     emailBody2 + linkActivation +
                                     emailAccountID2 +
                                     emailAlternativeURL2 + urlActivation + "ActivationAccount.aspx?TokenID=" + activationID +
                                     emailFeet2;

                string bodyMessage3 = emailPreHead3 + emailHead3 + name + " " + surname1 +
                                     emailBody3 + linkActivation +
                                     emailAccountID3 +
                                     emailAlternativeURL3 + urlActivation + "ActivationAccount.aspx?TokenID=" + activationID +
                                     emailFeet3;

                MyMailMessage.Subject = emailSubject;
                MyMailMessage.IsBodyHtml = true;
                MyMailMessage.Body = bodyMessage + bodyMessage2 + bodyMessage3;
                MyMailMessage.Priority = System.Net.Mail.MailPriority.High;

                SmtpClient SMTPServer = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"].ToString());
                SMTPServer.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                SMTPServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["EmailUser"].ToString(), ConfigurationManager.AppSettings["EmailPassword"].ToString());
                SMTPServer.EnableSsl = true;

                // Eliminate invalid remote certificate error 
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

                SMTPServer.Send(MyMailMessage);

                bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("SendConfEmail::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return bResult;
        }

        private string GetNewToken()
        {
            var jot = new JotProvider();

            var token = jot.Create();

            return jot.Encode(token);
        }

        private TokenValidationResult DefaultVerification(string encodedTokenFromWebPage)
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

        private bool GetUserOperationReportData(SortedList parametersIn, int nDateFormat, out SortedList operationList, string strContractList)
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
                            strSQLSelect = string.Format("SELECT HFIN_ID, TO_CHAR( HFIN_DATE, 'dd/MM/YY hh24:mi'), HFIN_VEHICLEID, HFIN_GRP_ID_ZONE, GRP_DESCSHORT, HFIN_DATE, TO_CHAR( HFIN_DATE, 'YYYYMMddhh24miss') FROM FINES_HIS, FINES_DEF, GROUPS ");
                            strSQLWhere = string.Format("WHERE HFIN_STATUSADMON = {0} AND HFIN_DFIN_ID = DFIN_ID AND DFIN_COD_ID = {1} ",
                                ConfigurationManager.AppSettings["FineStatusAdmonDef.Pending"].ToString(), ConfigurationManager.AppSettings["FinesDefCode.Fine"].ToString());
                            strSQLWhere += string.Format("AND HFIN_VEHICLEID IN (SELECT MUP_PLATE FROM MOBILE_USERS_PLATES WHERE MUP_MU_ID = {0} AND MUP_VALID = 1 AND MUP_DELETED = 0) ", parametersIn["mui"].ToString());
                            if (nDateFormat == DATE_FORMAT_DAYS)
                                strSQLWhere += string.Format("AND HFIN_DATE > SYSDATE - {0} ", parametersIn["d"].ToString());
                            else
                                strSQLWhere += string.Format("AND HFIN_DATE BETWEEN TO_DATE( '{0}', 'hh24missddMMYY') AND TO_DATE( '{1}', 'hh24missddMMYY') ", parametersIn["d1"].ToString(), parametersIn["d2"].ToString());
                            strSQLWhere += " AND HFIN_GRP_ID_ZONE = GRP_ID ORDER BY HFIN_DATE DESC";
                        }
                        else
                        {
                            strSQLSelect = string.Format("SELECT FIN_ID, TO_CHAR( FIN_DATE, 'dd/MM/YY hh24:mi'), FIN_VEHICLEID, FIN_GRP_ID_ZONE, GRP_DESCSHORT, FIN_DATE, TO_CHAR( FIN_DATE, 'YYYYMMddhh24miss') FROM FINES, FINES_DEF, GROUPS ");
                            strSQLWhere = string.Format("WHERE FIN_STATUSADMON = {0} AND FIN_DFIN_ID = DFIN_ID AND DFIN_COD_ID = {1} ",
                                ConfigurationManager.AppSettings["FineStatusAdmonDef.Pending"].ToString(), ConfigurationManager.AppSettings["FinesDefCode.Fine"].ToString());
                            strSQLWhere += string.Format("AND FIN_VEHICLEID IN (SELECT MUP_PLATE FROM MOBILE_USERS_PLATES WHERE MUP_MU_ID = {0} AND MUP_VALID = 1 AND MUP_DELETED = 0) ", parametersIn["mui"].ToString());
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

                                dataList["ot"] = ConfigurationManager.AppSettings["OperationsDef.UnpaidFines"].ToString();
                                dataList["pl"] = dataReader.GetString(2);
                                dataList["zo"] = dataReader.GetString(4);
                                dataList["fn"] = dataReader.GetInt32(0).ToString();
                                dataList["fpd"] = dataReader.GetString(1);

                                // check to see if it is still possible to pay
                                int iPayAmount = 0;
                                int iPayStatus = Convert.ToInt32(ConfigurationManager.AppSettings["FineCancellation.NotPayable"]);
                                if (IsFinePayable(Convert.ToInt32(dataList["fn"]), out iPayAmount, out iPayStatus, Convert.ToInt32(strContractId)))
                                    dataList["fs"] = ConfigurationManager.AppSettings["FineCancellation.Payable"].ToString();
                                else
                                    dataList["fs"] = iPayStatus.ToString();
                                dataList["pa"] = iPayAmount.ToString();
                                string strDate = dataReader.GetString(6);

                                dataList["contid"] = strContractId;
                                dataList["contname"] = strContractName;

                                nNumOperations++;
                                operationList["o" + strDate + nNumOperations.ToString("00000")] = dataList;
                            }
                        }
                    }

                    if (bListFinePayments)
                    {
                        // Search for fine payments
                        if (bUseHistoricData)
                        {
                            strSQLSelect = string.Format("SELECT HOPE_ID, HOPE_DOPE_ID, HOPE_GRP_ID, HOPE_DPAY_ID, NVL(HOPE_POST_PAY,0), HOPE_VALUE_VIS, TO_CHAR( HOPE_MOVDATE, 'dd/MM/YY hh24:mi'), HOPE_FIN_ID, TO_CHAR( HFIN_DATE, 'dd/MM/YY hh24:mi'), HFIN_STATUSADMON, GRP_DESCSHORT, HOPE_MOVDATE, HFIN_VEHICLEID, TO_CHAR( HOPE_MOVDATE, 'YYYYMMddhh24miss') FROM OPERATIONS_HIS, FINES_HIS, GROUPS ");
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
                            strSQLSelect = string.Format("SELECT OPE_ID, OPE_DOPE_ID, OPE_GRP_ID, OPE_DPAY_ID, NVL(OPE_POST_PAY,0), OPE_VALUE_VIS, TO_CHAR( OPE_MOVDATE, 'dd/MM/YY hh24:mi'), OPE_FIN_ID, TO_CHAR( FIN_DATE, 'dd/MM/YY hh24:mi'), FIN_STATUSADMON, GRP_DESCSHORT, OPE_MOVDATE, FIN_VEHICLEID, TO_CHAR( OPE_MOVDATE, 'YYYYMMddhh24miss') FROM OPERATIONS, FINES, GROUPS ");
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

                                dataList["on"] = dataReader.GetInt32(0).ToString();
                                dataList["ot"] = dataReader.GetInt32(1).ToString();
                                dataList["zo"] = dataReader.GetString(10);
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
                                int nFineStatus = dataReader.GetInt32(9);
                                dataList["pl"] = dataReader.GetString(12);
                                string strDate = dataReader.GetString(13);

                                dataList["contid"] = strContractId;
                                dataList["contname"] = strContractName;

                                nNumOperations++;
                                operationList["o" + strDate + nNumOperations.ToString("00000")] = dataList;
                            }
                        }
                    }

                    // Search for all operations except fine payments
                    if (bListOperations)
                    {
                        if (bUseHistoricData)
                        {
                            strSQLSelect = string.Format("SELECT HOPE_ID, HOPE_DOPE_ID, HOPE_VEHICLEID, HOPE_GRP_ID, TO_CHAR( HOPE_INIDATE, 'dd/MM/YY hh24:mi'), TO_CHAR( HOPE_ENDDATE, 'dd/MM/YY hh24:mi'), HOPE_DPAY_ID, NVL(HOPE_POST_PAY,0), HOPE_VALUE_VIS, TO_CHAR( HOPE_MOVDATE, 'dd/MM/YY hh24:mi'), GRP_DESCSHORT, HOPE_MOVDATE, HOPE_RECHARGE_TYPE, HOPE_REFERENCE, TO_CHAR( HOPE_MOVDATE, 'YYYYMMddhh24miss'), TO_CHAR( HOPE_INIDATE, 'YYYYMMddhh24miss') FROM OPERATIONS_HIS, GROUPS ");
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
                            strSQLSelect = string.Format("SELECT OPE_ID, OPE_DOPE_ID, OPE_VEHICLEID, OPE_GRP_ID, TO_CHAR( OPE_INIDATE, 'dd/MM/YY hh24:mi'), TO_CHAR( OPE_ENDDATE, 'dd/MM/YY hh24:mi'), OPE_DPAY_ID, NVL(OPE_POST_PAY,0), OPE_VALUE_VIS, TO_CHAR( OPE_MOVDATE, 'dd/MM/YY hh24:mi'), GRP_DESCSHORT, OPE_MOVDATE, OPE_RECHARGE_TYPE, OPE_REFERENCE, TO_CHAR( OPE_MOVDATE, 'YYYYMMddhh24miss'), TO_CHAR( OPE_INIDATE, 'YYYYMMddhh24miss') FROM OPERATIONS, GROUPS ");
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
                                string strDate = "";
                                SortedList dataList = new SortedList();

                                dataList["on"] = dataReader.GetInt32(0).ToString();
                                dataList["ot"] = dataReader.GetInt32(1).ToString();
                                if (!dataReader.IsDBNull(2))
                                    dataList["pl"] = dataReader.GetString(2);
                                dataList["zo"] = dataReader.GetString(10);
                                if (!dataReader.IsDBNull(4))
                                {
                                    dataList["sd"] = dataReader.GetString(4);
                                    strDate = dataReader.GetString(15);
                                }
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
                                if (dataList["ot"].ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.Recharge"].ToString()))
                                {
                                    if (!dataReader.IsDBNull(12))
                                        dataList["bns"] = dataReader.GetInt32(12).ToString();
                                    // Don't show zone name for recharges
                                    dataList["zo"] = "";
                                }
                                if (strDate.Length == 0)
                                    strDate = dataReader.GetString(14);

                                dataList["contid"] = strContractId;
                                dataList["contname"] = strContractName;

                                nNumOperations++;
                                operationList["o" + strDate + nNumOperations.ToString("00000")] = dataList;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserOperationReportData::Exception", LoggerSeverities.Error);
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

        bool GeneratePDF(SortedList parametersReport, SortedList parametersUser, string strFilePath)
        {
            bool bResult = false;
            Document doc = new Document(PageSize.A4, 10f, 10f, 120f, 100f);

            try
            {
                PdfWriter pdfWriter = PdfWriter.GetInstance(doc, new FileStream(strFilePath, FileMode.CreateNew));

                string strUserHeaderData = parametersUser["na"].ToString() + " - " + parametersUser["fs"].ToString();
                if (parametersUser["ss"].ToString().Length > 0)
                    strUserHeaderData += " - " + parametersUser["ss"].ToString();
                if (parametersUser["nif"].ToString().Length > 0)
                    strUserHeaderData += " - " + parametersUser["nif"].ToString();
                if (parametersUser["asn"].ToString().Length > 0)
                    strUserHeaderData += " - " + parametersUser["asn"].ToString();
                if (parametersUser["abn"].ToString().Length > 0)
                    strUserHeaderData += " - " + parametersUser["abn"].ToString();
                if (parametersUser["apc"].ToString().Length > 0)
                    strUserHeaderData += " - " + parametersUser["apc"].ToString();
                if (parametersUser["aci"].ToString().Length > 0)
                    strUserHeaderData += " - " + parametersUser["aci"].ToString();

                string strFooterData = " ";

                pdfWriter.PageEvent = new ITextEvents(strUserHeaderData, strFooterData);
                doc.Open();

                // Blank line
                Paragraph parBlank = new Paragraph();
                parBlank.Add(System.Environment.NewLine);

                // Title
                Paragraph parTitle = new Paragraph();
                parTitle.Alignment = Element.ALIGN_CENTER;
                parTitle.Font = FontFactory.GetFont("Arial", 24);
                parTitle.Font.SetStyle(Font.BOLD);
                parTitle.Font.SetStyle(Font.UNDERLINE);
                parTitle.Add(ConfigurationManager.AppSettings["ReportTitle"].ToString());
                doc.Add(parBlank);
                doc.Add(parTitle);
                doc.Add(parBlank);

                // Dates
                Paragraph parDates = new Paragraph();
                parDates.Alignment = Element.ALIGN_CENTER;
                parDates.Font = FontFactory.GetFont("Arial", 16);
                parDates.Font.SetStyle(Font.BOLD);
                DateTime dtStartDate = DateTime.ParseExact(parametersReport["d1"].ToString(), "HHmmssddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                DateTime dtEndDate = DateTime.ParseExact(parametersReport["d2"].ToString(), "HHmmssddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                string strDateRange = dtStartDate.ToString("dd/MM/yyyy HH:mm") + " - " + dtEndDate.ToString("dd/MM/yyyy HH:mm");
                parDates.Add(strDateRange);
                doc.Add(parDates);
                doc.Add(parBlank);

                // CIF
                Paragraph parCif = new Paragraph();
                parCif.Alignment = Element.ALIGN_CENTER;
                parCif.Font = FontFactory.GetFont("Arial", 12);
                parCif.Font.SetStyle(Font.BOLD);
                parCif.Add(parametersReport["cif"].ToString());
                doc.Add(parCif);
                doc.Add(parBlank);

                // Table for operations
                BaseColor colorBack = new BaseColor(171, 194, 236);
                PdfPTable tableOper = new PdfPTable(5);
                tableOper.TotalWidth = 500f;
                tableOper.LockedWidth = true;
                float[] widths = new float[] { 90f, 70f, 170f, 50f, 140f };
                tableOper.SetWidths(widths);
                tableOper.HorizontalAlignment = 1;
                tableOper.SpacingBefore = 20f;
                tableOper.SpacingAfter = 30f;
                Paragraph pgTitle1 = new Paragraph("Operación", FontFactory.GetFont("Arial", 10));
                //pgTitle1.Font.SetStyle(Font.UNDERLINE);
                PdfPCell cellTitle1 = new PdfPCell(pgTitle1);
                cellTitle1.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                cellTitle1.BorderColor = BaseColor.WHITE;
                cellTitle1.BackgroundColor = colorBack;
                tableOper.AddCell(cellTitle1);
                Paragraph pgTitle2 = new Paragraph("Matrícula", FontFactory.GetFont("Arial", 10));
                //pgTitle2.Font.SetStyle(Font.UNDERLINE);
                PdfPCell cellTitle2 = new PdfPCell(pgTitle2);
                cellTitle2.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                cellTitle2.BorderColor = BaseColor.WHITE;
                cellTitle2.BackgroundColor = colorBack;
                tableOper.AddCell(cellTitle2);
                Paragraph pgTitle3 = new Paragraph("Fechas", FontFactory.GetFont("Arial", 10));
                //pgTitle3.Font.SetStyle(Font.UNDERLINE);
                PdfPCell cellTitle3 = new PdfPCell(pgTitle3);
                cellTitle3.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                cellTitle3.BorderColor = BaseColor.WHITE;
                cellTitle3.BackgroundColor = colorBack;
                tableOper.AddCell(cellTitle3);
                Paragraph pgTitle4 = new Paragraph("Importe", FontFactory.GetFont("Arial", 10));
                //pgTitle4.Font.SetStyle(Font.UNDERLINE);
                PdfPCell cellTitle4 = new PdfPCell(pgTitle4);
                cellTitle4.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                cellTitle4.BorderColor = BaseColor.WHITE;
                cellTitle4.BackgroundColor = colorBack;
                tableOper.AddCell(cellTitle4);
                Paragraph pgTitle5 = new Paragraph("Localidad", FontFactory.GetFont("Arial", 10));
                //pgTitle4.Font.SetStyle(Font.UNDERLINE);
                PdfPCell cellTitle5 = new PdfPCell(pgTitle5);
                cellTitle5.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                cellTitle5.BorderColor = BaseColor.WHITE;
                cellTitle5.BackgroundColor = colorBack;
                tableOper.AddCell(cellTitle5);

                // Operations
                decimal dTotalOperations = 0;
                decimal dTotalParkings = 0;
                decimal dTotalExtensions = 0;
                decimal dTotalRefunds = 0;
                decimal dTotalFinesPaid = 0;
                decimal dTotalFines = 0;
                decimal dTotalRecharges = 0;
                SortedList operationsList = (SortedList)parametersReport["oper"];
                if (operationsList.Count > 0)
                {
                    foreach (DictionaryEntry item in operationsList)
                    {
                        SortedList operationDataList = (SortedList)item.Value;

                        string strAmount = "";
                        decimal dAmount = Convert.ToDecimal(operationDataList["pa"]) * (decimal)0.01;
                        strAmount = dAmount.ToString();

                        int nOperType = Convert.ToInt32(operationDataList["ot"]);
                        string strOperType = "Aparcamiento";
                        if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Parking"]))
                        {
                            strOperType = "Aparcamiento";
                            dTotalParkings += dAmount;
                        }
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Extension"]))
                        {
                            strOperType = "Ampliación";
                            dTotalExtensions += dAmount;
                        }
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Refund"]))
                        {
                            strOperType = "Devolución";
                            dTotalRefunds += dAmount;
                        }
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Payment"]))
                        {
                            strOperType = "Pago denuncia";
                            dTotalFinesPaid += dAmount;
                        }
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Recharge"]))
                        {
                            strOperType = "Recarga";
                            dTotalRecharges += dAmount;
                        }
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Postpayment"]))
                            strOperType = "Postpago";
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.ResidentSticker"]))
                            strOperType = "Dist. Res.";
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.ElectricRecharge"]))
                            strOperType = "Recarga Elec.";
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Bycing"]))
                            strOperType = "Bycing";
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.UnpaidFines"]))
                        {
                            strOperType = "Denuncia";
                            dTotalFines += dAmount;
                        }
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Recharge"])
                            && Convert.ToInt32(operationDataList["bns"]) == 1)
                            strOperType = "Bonificación";

                        string strDate = "";
                        if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Recharge"])
                            || nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.ResidentSticker"])
                            || nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.ElectricRecharge"])
                            || nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Bycing"]))
                            strDate = operationDataList["rd"].ToString();
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Payment"]))
                            strDate = operationDataList["fpd"].ToString() + " " + operationDataList["fd"].ToString();
                        else if (nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.UnpaidFines"]))
                            strDate = operationDataList["fpd"].ToString();
                        else
                            strDate = operationDataList["sd"].ToString() + " " + operationDataList["ed"].ToString();
                        string strPlate = "";
                        if (operationDataList["pl"] != null)
                            strPlate = operationDataList["pl"].ToString().Replace("-<RES>", "");
                        string strRef = "";
                        if (operationDataList["contname"] != null)
                        {
                            strRef = operationDataList["contname"].ToString();
                            if (operationDataList["zo"] != null)
                            {
                                if (operationDataList["zo"].ToString().Length > 0)
                                    strRef += " - " + operationDataList["zo"].ToString();
                            }
                        }
                        if (strRef.Length >= 21)
                            strRef = strRef.Substring(0, 21);

                        PdfPCell cellData1 = new PdfPCell(new Phrase(strOperType, FontFactory.GetFont("Arial", 10)));
                        cellData1.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                        cellData1.BorderColor = BaseColor.WHITE;
                        tableOper.AddCell(cellData1);
                        PdfPCell cellData2 = new PdfPCell(new Phrase(strPlate, FontFactory.GetFont("Arial", 10)));
                        cellData2.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                        cellData2.BorderColor = BaseColor.WHITE;
                        tableOper.AddCell(cellData2);
                        PdfPCell cellData3 = new PdfPCell(new Phrase(strDate, FontFactory.GetFont("Arial", 10)));
                        cellData3.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                        cellData3.BorderColor = BaseColor.WHITE;
                        tableOper.AddCell(cellData3);
                        PdfPCell cellData4 = new PdfPCell(new Phrase(strAmount, FontFactory.GetFont("Arial", 10)));
                        cellData4.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                        cellData4.BorderColor = BaseColor.WHITE;
                        if ((nOperType != Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Refund"]) &&
                            nOperType != Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Recharge"])) && dAmount > 0)
                            cellData4.Phrase.Font.SetColor(255, 0, 0);
                        tableOper.AddCell(cellData4);
                        PdfPCell cellData5 = new PdfPCell(new Phrase(strRef, FontFactory.GetFont("Arial", 10)));
                        cellData5.HorizontalAlignment = 0; //0=Left, 1=Centre, 2=Right
                        cellData5.BorderColor = BaseColor.WHITE;
                        tableOper.AddCell(cellData5);
                    }
                }
                else
                {
                    PdfPCell cellData1 = new PdfPCell(new Phrase("No hay movimientos en el periodo solicitado", FontFactory.GetFont("Arial", 10)));
                    cellData1.Colspan = 5;
                    cellData1.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                    cellData1.BorderColor = BaseColor.WHITE;
                    tableOper.AddCell(cellData1);
                }

                doc.Add(tableOper);

                // Total amounts
                dTotalOperations = dTotalParkings + dTotalExtensions - dTotalRefunds;
                PdfPTable tableTotals = new PdfPTable(4);
                tableTotals.TotalWidth = 500f;
                tableTotals.LockedWidth = true;
                float[] widthsTotals = new float[] { 1f, 1f, 1f, 1f };
                tableTotals.SetWidths(widthsTotals);
                tableTotals.HorizontalAlignment = 1;
                tableTotals.SpacingBefore = 20f;
                tableTotals.SpacingAfter = 30f;
                tableTotals.KeepTogether = true;

                PdfPCell cellTotal0 = new PdfPCell(new Phrase("Resumen de operaciones:", FontFactory.GetFont("Arial", 10)));
                cellTotal0.Colspan = 4;
                cellTotal0.HorizontalAlignment = 0; //0=Left, 1=Centre, 2=Right
                cellTotal0.BorderColor = BaseColor.WHITE;
                tableTotals.AddCell(cellTotal0);

                PdfPCell cellTotal1 = new PdfPCell(new Phrase("Total Estacionamientos", FontFactory.GetFont("Arial", 10)));
                cellTotal1.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal1.BorderColor = BaseColor.WHITE;
                cellTotal1.BackgroundColor = colorBack;
                tableTotals.AddCell(cellTotal1);
                PdfPCell cellTotal2 = new PdfPCell(new Phrase("Total Ampliaciones", FontFactory.GetFont("Arial", 10)));
                cellTotal2.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal2.BorderColor = BaseColor.WHITE;
                cellTotal2.BackgroundColor = colorBack;
                tableTotals.AddCell(cellTotal2);
                PdfPCell cellTotal3 = new PdfPCell(new Phrase("Total Devoluciones", FontFactory.GetFont("Arial", 10)));
                cellTotal3.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal3.BorderColor = BaseColor.WHITE;
                cellTotal3.BackgroundColor = colorBack;
                tableTotals.AddCell(cellTotal3);
                PdfPCell cellTotal31 = new PdfPCell(new Phrase("Total Operaciones", FontFactory.GetFont("Arial", 10)));
                cellTotal31.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal31.BorderColor = BaseColor.WHITE;
                cellTotal31.BackgroundColor = colorBack;
                tableTotals.AddCell(cellTotal31);

                PdfPCell cellTotal4 = new PdfPCell(new Phrase(dTotalParkings.ToString() + " €", FontFactory.GetFont("Arial", 10)));
                cellTotal4.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal4.BorderColor = BaseColor.WHITE;
                cellTotal4.Phrase.Font.SetColor(255, 0, 0);
                tableTotals.AddCell(cellTotal4);
                PdfPCell cellTotal5 = new PdfPCell(new Phrase(dTotalExtensions.ToString() + " €", FontFactory.GetFont("Arial", 10)));
                cellTotal5.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal5.BorderColor = BaseColor.WHITE;
                cellTotal5.Phrase.Font.SetColor(255, 0, 0);
                tableTotals.AddCell(cellTotal5);
                PdfPCell cellTotal6 = new PdfPCell(new Phrase(dTotalRefunds.ToString() + " €", FontFactory.GetFont("Arial", 10)));
                cellTotal6.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal6.BorderColor = BaseColor.WHITE;
                tableTotals.AddCell(cellTotal6);
                PdfPCell cellTotal61 = new PdfPCell(new Phrase(Math.Abs(dTotalOperations).ToString() + " €", FontFactory.GetFont("Arial", 10)));
                cellTotal61.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal61.BorderColor = BaseColor.WHITE;
                if (dTotalOperations > 0)
                    cellTotal61.Phrase.Font.SetColor(255, 0, 0);
                tableTotals.AddCell(cellTotal61);

                PdfPCell cellTotal71 = new PdfPCell(new Phrase("Total Denuncias", FontFactory.GetFont("Arial", 10)));
                cellTotal71.Colspan = 2;
                cellTotal71.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal71.BorderColor = BaseColor.WHITE;
                cellTotal71.BackgroundColor = colorBack;
                tableTotals.AddCell(cellTotal71);
                PdfPCell cellTotal72 = new PdfPCell(new Phrase("Total Denuncias Pagadas", FontFactory.GetFont("Arial", 10)));
                cellTotal72.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal72.BorderColor = BaseColor.WHITE;
                cellTotal72.BackgroundColor = colorBack;
                tableTotals.AddCell(cellTotal72);
                PdfPCell cellTotal7 = new PdfPCell(new Phrase("Total Recargas", FontFactory.GetFont("Arial", 10)));
                cellTotal7.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal7.BorderColor = BaseColor.WHITE;
                cellTotal7.BackgroundColor = colorBack;
                tableTotals.AddCell(cellTotal7);

                PdfPCell cellTotal8 = new PdfPCell(new Phrase(dTotalFines.ToString() + " €", FontFactory.GetFont("Arial", 10)));
                cellTotal8.Colspan = 2;
                cellTotal8.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal8.BorderColor = BaseColor.WHITE;
                tableTotals.AddCell(cellTotal8);
                PdfPCell cellTotal81 = new PdfPCell(new Phrase(dTotalFinesPaid.ToString() + " €", FontFactory.GetFont("Arial", 10)));
                cellTotal81.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal81.BorderColor = BaseColor.WHITE;
                cellTotal81.Phrase.Font.SetColor(255, 0, 0);
                tableTotals.AddCell(cellTotal81);
                PdfPCell cellTotal82 = new PdfPCell(new Phrase(dTotalRecharges.ToString() + " €", FontFactory.GetFont("Arial", 10)));
                cellTotal82.HorizontalAlignment = 2; //0=Left, 1=Centre, 2=Right
                cellTotal82.BorderColor = BaseColor.WHITE;
                tableTotals.AddCell(cellTotal82);

                doc.Add(tableTotals);

                bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GeneratePDF::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                doc.Close();
            }

            return bResult;
        }

        private bool GetCIF(out string strCIF, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            strCIF = "";

            try
            {
                // Watch out - not using passed in Contract Id since this data is stored in the main DB
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
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

                string strSQL = string.Format("SELECT NVL(MCON_CIF, '') FROM MOBILE_CONTRACTS WHERE MCON_ID = {0}", nContractId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    if (!dataReader.IsDBNull(0))
                        strCIF = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetCIF::Exception", LoggerSeverities.Error);
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
                string emailBody = "<p>Su solicitud ha sido procesada con éxito y se ha generado el siguiente código de recuperación: ";
                string emailCode = "<b>" + strRecoveryCode + "</b></p>" + 
                    "<p>En la aplicación debe utilizar este código en el siguiente paso del proceso. Introduzca el código junto con la nueva contraseña y accederá automáticamente a la aplicación.</p>";
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

        bool SendEmail(string strDestAddress, string strFilePath)
        {
            bool bResult = false;

            try
            {
                MailMessage MyMailMessage = new MailMessage();
                MyMailMessage.From = new MailAddress(ConfigurationManager.AppSettings["SendAddress"].ToString());
                MyMailMessage.To.Add(strDestAddress);
                MyMailMessage.Subject = ConfigurationManager.AppSettings["EmailSubject"].ToString();
                Attachment attachFile = new Attachment(strFilePath);
                MyMailMessage.Attachments.Add(attachFile);

                SmtpClient SMTPServer = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"].ToString());
                SMTPServer.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                SMTPServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["EmailUser"].ToString(), ConfigurationManager.AppSettings["EmailPassword"].ToString());
                SMTPServer.EnableSsl = true;

                // Eliminate invalid remote certificate error 
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

                SMTPServer.Send(MyMailMessage);

                bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("SendEmail::Exception", LoggerSeverities.Error);
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
                        Logger_AddLogMessage(string.Format("IsFinePayable::Error - Could not find group for fine: {0}", iFineId), LoggerSeverities.Error);
                }
                else
                    Logger_AddLogMessage(string.Format("IsFinePayable::Error obtaining group for fine: {0}", iFineId), LoggerSeverities.Error);

                if (iVirtualUnit < 0)
                {
                    if (GetFirstVirtualUnit(ref iVirtualUnit, nContractId))
                    {
                        if (iVirtualUnit < 0)
                        {
                            Logger_AddLogMessage(string.Format("IsFinePayable::Error - could not obtain first virtual unit"), LoggerSeverities.Error);
                            return bResult;
                        }
                    }
                    else
                    {
                        Logger_AddLogMessage(string.Format("IsFinePayable::Error obtaining first virtual unit"), LoggerSeverities.Error);
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
                    Logger_AddLogMessage(string.Format("IsFinePayable::Error sending M5: iRes={0}", iRes), LoggerSeverities.Error);
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
                Logger_AddLogMessage("IsFinePayable::Exception", LoggerSeverities.Error);
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
        private string DtxToString(DateTime dt)
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
        private bool IsFinePaymentInTime(DateTime dtFineDatetime, DateTime dtOpePaymentDateTime, int iTimeForPayment, string strDayCode, int nContractId)
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

                        if (dataReader.GetInt32(5) != CFineManager_C_ADMON_STATUS_PENDIENTE)//"FIN_STATUSADMON"
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
                Logger_AddLogMessage("OPSMessage_M05Process::Exception", LoggerSeverities.Error);
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

        private string RandomString(int length)
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

        private int AddMobileUser(SortedList parametersIn, out string strToken, int nContractId = 0)
        {
            int nMobileUserId = (int)ResultType.Result_Error_Generic;

            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            strToken = "";

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

                string strSQL1 = " insert into MOBILE_USERS (mu_email, mu_login, mu_password, mu_activate_account, mu_addr_country, mu_fine_notify, mu_unpark_notify, mu_unpark_notify_time, mu_recharge_notify, mu_balance_notify, mu_balance_notify_amount";
                string strSQL2 = " ) VALUES( '" + parametersIn["em"].ToString() + "',";
                strSQL2 += "'" + parametersIn["un"].ToString().Replace('\'', ',') + "', '" + parametersIn["pw"].ToString().Replace('\'', ',') + "', " + ConfigurationManager.AppSettings["ActivateAccount.No"].ToString() + ", '";
                strSQL2 += ConfigurationManager.AppSettings["AddressCountry.Spain"].ToString() + "'," + parametersIn["fn"].ToString() + ", " + parametersIn["unp"].ToString() + ", " + parametersIn["t_unp"].ToString() + ", " + parametersIn["re"].ToString() + ", " + parametersIn["ba"].ToString() + ", " + parametersIn["q_ba"].ToString();
                if (parametersIn["nif"] != null)
                {
                    strSQL1 += " , mu_dni";
                    strSQL2 += ", UPPER('" + parametersIn["nif"].ToString() + "')";
                }
                if (parametersIn["na"] != null)
                {
                    strSQL1 += " , mu_name";
                    strSQL2 += ", INITCAP('" + parametersIn["na"].ToString().Replace('\'', ',') + "')";
                }
                if (parametersIn["fs"] != null)
                {
                    strSQL1 += " , mu_surname1";
                    strSQL2 += ", INITCAP('" + parametersIn["fs"].ToString().Replace('\'', ',') + "')";
                }
                if (parametersIn["ss"] != null)
                {
                    strSQL1 += " , mu_surname2";
                    strSQL2 += ", INITCAP('" + parametersIn["ss"].ToString().Replace('\'', ',') + "')";
                }
                if (parametersIn["mmp"] != null)
                {
                    strSQL1 += " , mu_mobile_telephone";
                    strSQL2 += ", '" + parametersIn["mmp"].ToString() + "'";
                }
                if (parametersIn["amp"] != null)
                {
                    strSQL1 += " , mu_mobile_telephone2";
                    strSQL2 += ", '" + parametersIn["amp"].ToString() + "'";
                }
                if (parametersIn["asn"] != null)
                {
                    strSQL1 += " , mu_addr_street";
                    strSQL2 += ", '" + parametersIn["asn"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["abn"] != null)
                {
                    strSQL1 += " , mu_addr_number";
                    strSQL2 += ", '" + parametersIn["abn"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["adf"] != null)
                {
                    strSQL1 += " , mu_addr_level";
                    strSQL2 += ", '" + parametersIn["adf"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["add"] != null)
                {
                    strSQL1 += " , mu_door_number";
                    strSQL2 += ", '" + parametersIn["add"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["ads"] != null)
                {
                    strSQL1 += " , mu_addr_stair";
                    strSQL2 += ", '" + parametersIn["ads"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["adl"] != null)
                {
                    strSQL1 += " , mu_addr_letter";
                    strSQL2 += ", '" + parametersIn["adl"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["apc"] != null)
                {
                    strSQL1 += " , mu_addr_postal_code";
                    strSQL2 += ", '" + parametersIn["apc"].ToString() + "'";
                }
                if (parametersIn["aci"] != null)
                {
                    strSQL1 += " , mu_addr_city";
                    strSQL2 += ", '" + parametersIn["aci"].ToString().Replace('\'', ',') + "'";
                }
                if (parametersIn["apr"] != null)
                {
                    strSQL1 += " , mu_addr_province";
                    strSQL2 += ", '" + parametersIn["apr"].ToString().Replace('\'', ',') + "'";
                }
                strSQL2 += ") returning MU_ID into :nReturnValue";

                oraCmd.CommandText = strSQL1 + strSQL2;

                oraCmd.Parameters.Add(new OracleParameter("nReturnValue", OracleDbType.Int32));
                oraCmd.Parameters["nReturnValue"].Direction = System.Data.ParameterDirection.ReturnValue;

                oraCmd.ExecuteNonQuery();

                nMobileUserId = Convert.ToInt32(oraCmd.Parameters["nReturnValue"].Value.ToString());

                if (parametersIn["plates"] != null)
                {
                    oraCmd.Parameters.Clear();
                    SortedList PlateList = (SortedList)parametersIn["plates"];
                    foreach (string sPlate in PlateList.Values)
                    {
                        string filteredPlate = Regex.Replace(sPlate, @"[^a-zA-Z0-9]+", "");
                        string strSQL = string.Format("INSERT INTO MOBILE_USERS_PLATES (MUP_MU_ID, MUP_PLATE) VALUES ({0}, '{1}')", nMobileUserId, filteredPlate.ToUpper());
                        oraCmd.CommandText = strSQL;
                        oraCmd.ExecuteNonQuery();
                    }
                }

                Guid tokenUSER = System.Guid.NewGuid();
                strToken = tokenUSER.ToString().Replace("-", "");

                StringBuilder sbSQL2 = new StringBuilder();

                sbSQL2.AppendFormat("insert into MOBILE_USERS_ACTIVATION (mu_activation_key, mu_id, mu_email_date) ");
                sbSQL2.AppendFormat("VALUES( '{0}', {1}, sysdate)", strToken, nMobileUserId.ToString());

                oraCmd.CommandText = sbSQL2.ToString();
                oraCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("AddMobileUser::Exception", LoggerSeverities.Error);
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

                if (parametersIn["plates"] != null)
                {
                    string strSQL = string.Format("UPDATE MOBILE_USERS_PLATES SET MUP_VALID = 0, MUP_DELETED = 1 WHERE MUP_MU_ID = {0}", nMobileUserId);
                    oraCmd.CommandText = strSQL;
                    oraCmd.ExecuteNonQuery();

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
                //Se crean 2 loggers uno para el API y otro para el OPS.Comm
                _logger = new Logger(MethodBase.GetCurrentMethod().DeclaringType);
                //Es necesario inicializar este logger para el OPS.Comm ya que lo pide en CS_M1.cs (linea 103)
                OPS.Components.Data.DatabaseFactory.Logger = new OPS.Comm.Logger(MethodBase.GetCurrentMethod().DeclaringType); //_logger;
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

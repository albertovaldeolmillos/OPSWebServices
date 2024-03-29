﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;
using System.Xml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Jot;
using Newtonsoft.Json;
using OPS.Comm;
using OPS.Comm.Becs.Messages;
using OPS.Components.Data;
using Oracle.ManagedDataAccess.Client;

namespace OPSWebServices
{
    /// <summary>
    /// Summary description for WSUserManagement
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WSUserManagement : System.Web.Services.WebService
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

        public WSUserManagement()
        {
            //Uncomment the following line if using designed components 
            //InitializeComponent();
            InitializeStatic();
        }

        public static ILogger Logger
        {
            get { return _logger; }
        }

        #region WebMethods

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

        [WebMethod]
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

        [WebMethod]
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
                    if (((parametersIn["un"] == null || parametersIn["pw"] == null) && parametersIn["mui"] == null) ||
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

        [WebMethod]
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
        [WebMethod]
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

        [WebMethod]
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
        [WebMethod]
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
        [WebMethod]
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

        /*
         * Será llamado desde el dispositivo móvil para des-registrar el identificador. El servidor tiene la responsabilidad de 
           eliminar ese identificador para mantener consistente la bbdd.

            The parameters of method UnregisterUserNotificationsXML are:

            a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <mui>Mobile user id (authorization token)</mui>
                    <cid>Cloud ID. ID used in official notification clouds<cid>
                    <ah>authentication hash</ah> - *This parameter is optional
                </arinpark_in>
         
            b.	Result: is an integer with the next possible values:
                a.	1: saved without errors
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error.)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
                g.	-20: Mobile user id not found
                h.	-26: Cloud id not found

         */
        [WebMethod]
        public int UnregisterUserNotificationsXML(string xmlIn)
        {
            int iRes = 0;
            try
            {
                SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("UnregisterUserNotificationsXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["cid"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UnregisterUserNotificationsXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("UnregisterUserNotificationsXML::Error - Bad hash: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
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
                                Logger_AddLogMessage(string.Format("UnregisterUserNotificationsXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                            }
                            else
                                Logger_AddLogMessage(string.Format("UnregisterUserNotificationsXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                Logger_AddLogMessage(string.Format("UnregisterUserNotificationsXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Update notification data
                            if (!CancelUserNotifications(Convert.ToInt32(parametersIn["mui"]), parametersIn["cid"].ToString(), nContractId))
                            {
                                iRes = Convert.ToInt32(ResultType.Result_Error_Cloud_Id_Not_Found);
                                Logger_AddLogMessage(string.Format("UnregisterUserNotificationsXML::Error - Cloud Id not found: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                            }
                            else
                            {
                                iRes = Convert.ToInt32(ResultType.Result_OK);
                                Logger_AddLogMessage(string.Format("UnregisterUserNotificationsXML::Cancelled user notifications: xmlIn= {0}", xmlIn), LoggerSeverities.Info);
                            }
                        }
                    }
                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("UnregisterUserNotificationsXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("UnregisterUserNotificationsXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return iRes;
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
        [WebMethod]
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

        /*
         * The parameters of method QueryBonus are:

        a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <mui>Mobile user id (authorization token)</mui>
                    <ah>authentication hash</ah> - *This parameter is optional
	            </arinpark_in>

        b.	Result: is also a string containing an xml with the result of the method:
                <arinpark_out>
                    <r>Result of the method</r>
                    <t>Current Date in format hh24missddMMYY</t>
                    <c_bns>Current score.</c_bns>
                    <bns>Score needed to apply the new bonus.</bns>
                    <bonuslist>
                        <bonus>
                            <on>Operation number.</on>
                            <bns_t>Date when bonus was applied </bns_t>
                            <pa>Payment amount (Expressed in Euro cents)</pa>
                        </bonus>
                        <bonus>...</bonus>
                        ...
                        <bonus>...</bonus>
                    </bonuslist>
	            </arinpark_out>

            The tag <r> of the method will have these possible values:
            a.	1: User operations come after this tag
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error
            g.	-20: Mobile user id not found

         */
        [WebMethod]
        public string QueryBonusXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = new SortedList();
                SortedList bonusList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryBonusXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) ||
                        (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryBonusXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("QueryBonusXML::Error - Bad hash: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            // Try to obtain user from token
                            // Send contract Id as 0 so that it uses the global users connection
                            int nMobileUserId = GetUserFromToken(strToken, 0);

                            if (nMobileUserId <= 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryBonusXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryBonusXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryBonusXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get spaces bonus information
                            int nNumSpacesForBonus = GetNumSpacesBonus(nContractId);
                            parametersOut["bns"] = nNumSpacesForBonus.ToString();
                            // Send contract Id as 0 so that it uses the global users connection
                            int nCurUserSpaces = GetUserNumSpaces(Convert.ToInt32(parametersIn["mui"]), 0);
                            parametersOut["c_bns"] = nCurUserSpaces.ToString();

                            // Get user bonus information
                            if (!GetBonusData(Convert.ToInt32(parametersIn["mui"]), out bonusList, nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryBonusXML::Error - Could not obtain bonus operation data: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (bonusList.Count > 0)
                                parametersOut["bonuslist"] = bonusList;
                            else
                            {
                                bonusList = new SortedList();
                                bonusList["bonus1"] = "";
                                parametersOut["bonuslist"] = bonusList;
                            }

                            parametersOut["t"] = DateTime.Now.ToString("HHmmssddMMyy");
                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            xmlOut = GenerateXMLOuput(parametersOut);
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryBonusXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryBonusXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        /*
         * The parameters of method QueryUserBonus are:

        a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <mui>Mobile user id (authorization token)</mui>
                    <ah>authentication hash</ah> - *This parameter is optional
	            </arinpark_in>

        b.	Result: is also a string containing an xml with the result of the method:
                <arinpark_out>
                    <r>Result of the method</r>
                    <t>Current Date in format hh24missddMMYY</t>
                    <c_bns>Current score.</c_bns>
                    <bns>Score needed to apply the new bonus.</bns>
                    <bonuslist>
                        <bonus>
                            <on>Operation number.</on>
                            <bns_t>Date when bonus was applied </bns_t>
                            <pa>Payment amount (Expressed in Euro cents)</pa>
                        </bonus>
                        <bonus>...</bonus>
                        ...
                        <bonus>...</bonus>
                    </bonuslist>
	            </arinpark_out>

            The tag <r> of the method will have these possible values:
            a.	1: User operations come after this tag
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error
            g.	-20: Mobile user id not found

         */
        [WebMethod]
        public string QueryUserBonusXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = new SortedList();
                SortedList bonusList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUserBonusXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) ||
                        (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryUserBonusXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("QueryUserBonusXML::Error - Bad hash: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            // Try to obtain user from token
                            // Send contract Id as 0 so that it uses the global users connection
                            int nMobileUserId = GetUserFromToken(strToken, 0);

                            if (nMobileUserId <= 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryUserBonusXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryUserBonusXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryUserBonusXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get spaces bonus information
                            // Send contract Id as 0 so that it uses the global users connection
                            int nCurUserSpaces = GetUserNumSpaces(Convert.ToInt32(parametersIn["mui"]), 0);
                            parametersOut["c_bns"] = nCurUserSpaces.ToString();

                            // Get user bonus information
                            if (!GetUserBonusData(Convert.ToInt32(parametersIn["mui"]), out bonusList, nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserBonusXML::Error - Could not obtain bonus operation data: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (bonusList.Count > 0)
                                parametersOut["bonuslist"] = bonusList;
                            else
                            {
                                bonusList = new SortedList();
                                bonusList["bonus1"] = "";
                                parametersOut["bonuslist"] = bonusList;
                            }

                            parametersOut["t"] = DateTime.Now.ToString("HHmmssddMMyy");
                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            xmlOut = GenerateXMLOuput(parametersOut);
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryUserBonusXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUserBonusXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        /*
         * The parameters of method QueryBonusServices are:

        a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <mui>Mobile user id (authorization token)</mui>
                    <ah>authentication hash</ah> - *This parameter is optional
	            </arinpark_in>

        b.	Result: is also a string containing an xml with the result of the method:
                <arinpark_out>
                    <r>Result of the method</r>
                    <t>Current Date in format hh24missddMMYY</t>
                    <c_bns>Current score.</c_bns>
                    <bns>Score needed to apply the new bonus.</bns>
                    <servicelist>
                        <service>
                            <pnr_id>Partner id</pnr_id>
                            <pnr_n>Partner name</pnr_n>
                            <pnr_tp>Partner phone</pnr_tp>
                            <pnr_lnk>Partner more info link</pnr_lnk>
                            <srv_id>Service id.</srv_id>
                            <srv_n>Service name</srv_n>
                            <srv_d>Service description</srv_d>
                            <srv_ico>Service icon</srv_ico>
                            <srv_type>Service type: 0:MANUAL, 1:AUTOMATIC</srv_type>
                            <srv_exp>Service expiration time</srv_exp>*
                            <pa>Payment amount (Expressed in number of parking location published)</pa>
                        </service>
                        <service>...</service>
                        ...
                        <service>...</service>
                    </servicelist>
	            </arinpark_out>

            * OPTIONAL. If is not defined, then the service is always public.
         
            The tag <r> of the method will have these possible values:
            a.	1: User operations come after this tag
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error
            g.	-20: Mobile user id not found

         */
        [WebMethod]
        public string QueryBonusServicesXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = new SortedList();
                SortedList serviceList = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryBonusServicesXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) ||
                        (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryBonusServicesXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("QueryBonusServicesXML::Error - Bad hash: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            // Try to obtain user from token
                            // Send contract Id as 0 so that it uses the global users connection
                            int nMobileUserId = GetUserFromToken(strToken, 0);

                            if (nMobileUserId <= 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryBonusServicesXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryBonusServicesXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryBonusServicesXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get bonus services information
                            // Send contract Id as 0 so that it uses the global users connection
                            int nCurUserSpaces = GetUserNumSpaces(Convert.ToInt32(parametersIn["mui"]), 0);
                            parametersOut["c_bns"] = nCurUserSpaces.ToString();

                            // Get user bonus information
                            if (!GetBonusServicesData(out serviceList, nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryBonusServicesXML::Error - Could not obtain bonus services data: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (serviceList.Count > 0)
                                parametersOut["servicelist"] = serviceList;
                            else
                            {
                                serviceList = new SortedList();
                                serviceList["service1"] = "";
                                parametersOut["servicelist"] = serviceList;
                            }

                            parametersOut["t"] = DateTime.Now.ToString("HHmmssddMMyy");
                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            xmlOut = GenerateXMLOuput(parametersOut);
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryBonusServicesXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryBonusServicesXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        /*
         * 
         * The parameters of method QueryFreeSpacesXML are:
            a.	xmlIn: xml containing input parameters of the method:
                    <arinpark_in>
                        <mui>Mobile user id (authorization token)</mui>
                        <min>minutes. Maximun minutes since the space was released</min> *
                        <lt>Latitude of current user location</lt>**
                        <ln>Longitude of current user location</ln>**
                        <ah>authentication hash</ah> - *This parameter is optional
	                </arinpark_in>

                    * OPTIONAL. If is not defined, then is the server who takes the decision. (By default 5 minutes).
                    ** OPTIONAL. To filter around the current user location. In this case is not necessary to return all spaces. Only those close to the user.


            b.	Result: is also a string containing an xml with the result of the method:
                    <arinpark_out>
                        <r>Result of the method</r>
                        <t>Current Date in format hh24missddMMYY </t>
                        <spaces>
                            <spc>
                                <spcid>Space id.</spcid>
                                <spc_t>Date when space was released in format hh24missddMMYY </spc_t>
                                <lt>Latitude of free space</lt>
                                <ln>Longitude of free space</ln>
                                <zo>zone: 60001..60030, 70001,70002</zo>
                            </spc>
                            <spc>...</spc>
                            ...
                            <spc>...</spc>
                        </spaces>
	                </arinpark_out>

                The tag <r> of the method will have these possible values:
                a.	1: User operations come after this tag
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error.)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
         
         * 
         * 
         */
        [WebMethod]
        public string QueryFreeSpacesXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = new SortedList();
                SortedList spacesList = null;
                string strHash = "";
                string strHashString = "";
                string strLattitude = "";
                string strLongitude = "";
                int iMinutes = 5;
                int iMaxFreeSpaces = 20;

                Logger_AddLogMessage(string.Format("QueryFreeSpacesXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) ||
                        (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryFreeSpacesXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("QueryFreeSpacesXML::Error - Bad hash: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            // Try to obtain user from token
                            // Send contract Id as 0 so that it uses the global users connection
                            int nMobileUserId = GetUserFromToken(strToken, 0);

                            if (nMobileUserId <= 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryFreeSpacesXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryFreeSpacesXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("QueryFreeSpacesXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Get free space data for query
                            if (parametersIn["lt"] != null)
                            {
                                if (Convert.ToInt32(ConfigurationManager.AppSettings["GlobalizeCommaGPS"]) == 1)
                                    strLattitude = parametersIn["lt"].ToString().Replace(",", ".");
                                else
                                    strLattitude = parametersIn["lt"].ToString();
                            }
                            if (parametersIn["ln"] != null)
                            {
                                if (Convert.ToInt32(ConfigurationManager.AppSettings["GlobalizeCommaGPS"]) == 1)
                                    strLongitude = parametersIn["ln"].ToString().Replace(",", ".");
                                else
                                    strLongitude = parametersIn["ln"].ToString();
                            }

                            if (parametersIn["min"] != null)
                                iMinutes = Convert.ToInt32(parametersIn["min"]);
                            else
                                iMinutes = GetFreeSpaceTime(nContractId);
                            if (iMinutes < 0)
                            {
                                Logger_AddLogMessage(string.Format("QueryFreeSpacesXML::Error - Could not obtain free space time, using default = 5: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                iMinutes = 5;
                            }
                            iMaxFreeSpaces = GetMaxFreeSpaces(nContractId);
                            if (iMaxFreeSpaces < 0)
                            {
                                Logger_AddLogMessage(string.Format("QueryFreeSpacesXML::Error - Could not obtain maximum free spaces, using default = 20: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                iMaxFreeSpaces = 20;
                            }

                            int iDistance = GetMaxDistanceSpaces(nContractId);

                            // Get free parking spaces
                            if (!GetFreeSpaces(iMinutes, iDistance, strLattitude, strLongitude, iMaxFreeSpaces, out spacesList, nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryFreeSpacesXML::Error - Could not obtain free spaces: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (spacesList.Count > 0)
                                parametersOut["spaces"] = spacesList;
                            else
                            {
                                spacesList = new SortedList();
                                spacesList["spc1"] = "";
                                parametersOut["spaces"] = spacesList;
                            }

                            parametersOut["t"] = DateTime.Now.ToString("HHmmssddMMyy");
                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            xmlOut = GenerateXMLOuput(parametersOut);
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryFreeSpacesXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryFreeSpacesXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        /*
         * The parameters of method BuyService are:

        a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <mui>Mobile user id (authorization token)</mui>
                    <pnr_id>Partner id</pnr_id>
                    <srv_id>Service id.</srv_id>
                    <ah>authentication hash</ah> - *This parameter is optional
	            </arinpark_in>

        b.	Result: is also a string containing an xml with the result of the method:
                <arinpark_out>
                    <r>Result of the method</r>
                    <t>Current Date in format hh24missddMMYY</t>
                    <c_bns>Current score.</c_bns>
                    <bns>Score needed to apply the new bonus.</bns>
                    <bonuslist>
                        <bonus>
                            <on>Operation number.</on>
                            <bns_t>Date when bonus was bought</bns_t>
                            <pnr_id>Partner id</pnr_id>
                            <pnr_n>Partner name</pnr_n>
                            <pnr_tp>Partner phone</pnr_tp>
                            <pnr_lnk>Partner more info link</pnr_lnk>
                            <srv_id>Service id.</srv_id>
                            <srv_n>Service name</srv_n>
                            <srv_d>Service description</srv_d>
                            <srv_ico>Service icon</srv_ico>
                            <srv_type>Service type: 0:MANUAL, 1:AUTOMATIC</srv_type>
                            <csm>Was the bonus consumed? 0:false, 1:true</csm>
                            <csm_t>Date when bonus was consumed</csm_t>
                            <exp_t>Expiration date</exp_t>
                            <pa>Payment amount (Expressed in number of parking location published)</pa>
                            <code>Barcode or QR code</code>
                            <code_type>Code type: 0:AZTEC, 1:CODABAR, 2:CODE_39, 4:CODE_128, 5:DATA_MATRIX, 6:EAN_8, 7:EAN_13, 8:ITF, 10:PDF_417, 11:QR_CODE, 14:UPC_A</code_type>
                        </bonus>
                        <bonus>...</bonus>
                        ...
                        <bonus>...</bonus>
                    </bonuslist>
	            </arinpark_out>
         
            The tag <r> of the method will have these possible values:
            a.	1: User operations come after this tag
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error
            g.	-20: Mobile user id not found
            h.	-25: User has no credit enough
            i.	-30: The service has expired

         */
        [WebMethod]
        public string BuyServiceXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = new SortedList();
                SortedList bonusList = null;
                SortedList partnerList = new SortedList();
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("BuyServiceXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["mui"] == null) || (parametersIn["mui"].ToString().Length == 0) ||
                        (parametersIn["pnr_id"] == null) ||
                        (parametersIn["srv_id"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Bad hash: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            // Try to obtain user from token
                            // Send contract Id as 0 so that it uses the global users connection
                            int nMobileUserId = GetUserFromToken(strToken, 0);

                            if (nMobileUserId <= 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }
                            else
                                Logger_AddLogMessage(string.Format("BuyServiceXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Login);
                                Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Check to see if user is valid
                            // Send contract Id as 0 so that it uses the global users connection
                            string strUserName = GetUserName(Convert.ToInt32(parametersIn["mui"]), 0);
                            if (strUserName.Length == 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Mobile_User_Not_Found);
                                Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Mobile user not found: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Check to see if user has enough credit (spaces which have been notified, not monetary credit) to buy service
                            // Send contract Id as 0 so that it uses the global users connection
                            int nUserCredit = GetUserNumSpaces(Convert.ToInt32(parametersIn["mui"]), 0);
                            int nServicePrice = GetServicePrice(Convert.ToInt32(parametersIn["pnr_id"]), Convert.ToInt32(parametersIn["srv_id"]), nContractId);
                            if (nUserCredit < nServicePrice)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Not_Enough_Credit);
                                Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Insufficient credit: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Try to buy requested service
                            int nType = GetServiceType(Convert.ToInt32(parametersIn["pnr_id"].ToString()), Convert.ToInt32(parametersIn["srv_id"].ToString()), nContractId);
                            partnerList["service_id"] = parametersIn["srv_id"];
                            partnerList["partner_id"] = parametersIn["pnr_id"];
                            partnerList["date"] = DateTime.Now.ToString("HHmmssddMMyy");
                            partnerList["op"] = DateTime.Now.ToString("yyMMddHHmmssfff");
                            partnerList["un"] = strUserName;

                            // If automatic, then contact partner
                            if (nType == 1)
                            {
                                strHashString = partnerList["service_id"].ToString() + partnerList["partner_id"].ToString() + partnerList["date"].ToString() +
                                    partnerList["op"].ToString() + partnerList["un"].ToString();
                                partnerList["ah"] = CalculatePartnerHash(strHashString);

                                string strJsonMessageOut = "";
                                string strJsonMessageIn = GenerateCollectServiceMessage(partnerList);

                                if (!CollectServiceMessage(strJsonMessageIn, out strJsonMessageOut))
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Error connecting to partner: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }

                                // Process response from partner
                                XmlDocument xmlCollectService = (XmlDocument)JsonConvert.DeserializeXmlNode(strJsonMessageOut);
                                ResultType rtPartner = ProcessPartnerMessage(xmlCollectService, out partnerList);

                                if (rtPartner != ResultType.Result_OK)
                                {
                                    xmlOut = GenerateXMLErrorResult(rtPartner);
                                    Logger_AddLogMessage(string.Format("BuyServiceXML::Error - cashing in service with partner: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }
                            }
                            else
                            {
                                // Manual service
                                // Check expiration date
                                if (!IsServiceValid(Convert.ToInt32(parametersIn["pnr_id"].ToString()), Convert.ToInt32(parametersIn["srv_id"].ToString()), nContractId))
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Service_Expired);
                                    Logger_AddLogMessage(string.Format("BuyServiceXML::Service expired: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }

                                // If manual service, then send emails to partner and user
                                string strPartnerEmail = GetServiceEmail(Convert.ToInt32(parametersIn["pnr_id"].ToString()), Convert.ToInt32(parametersIn["srv_id"].ToString()), nContractId);
                                if (strPartnerEmail.Equals("-"))
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("BuyServiceXML::Error - could not obtain partner email: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }
                                else
                                    partnerList["partnerEmail"] = strPartnerEmail;

                                // Send contract Id as 0 so that it uses the global users connection
                                string strUserEmail = GetUserEmail(Convert.ToInt32(parametersIn["mui"]), 0);
                                if (strUserEmail.Equals("-"))
                                {
                                    partnerList["userEmail"] = "";
                                    Logger_AddLogMessage(string.Format("BuyServiceXML::Could not obtain user email - no email will be sent"), LoggerSeverities.Error);
                                }
                                else
                                    partnerList["userEmail"] = strUserEmail;

                                partnerList["serviceName"] = GetServiceName(Convert.ToInt32(parametersIn["pnr_id"].ToString()), Convert.ToInt32(parametersIn["srv_id"].ToString()), nContractId);
                                partnerList["serviceDesc"] = GetServiceDesc(Convert.ToInt32(parametersIn["pnr_id"].ToString()), Convert.ToInt32(parametersIn["srv_id"].ToString()), nContractId);
                                partnerList["partnerName"] = GetPartnerName(Convert.ToInt32(parametersIn["pnr_id"].ToString()), Convert.ToInt32(parametersIn["srv_id"].ToString()), nContractId);
                                partnerList["exp_t"] = GetServiceExpDate(Convert.ToInt32(parametersIn["pnr_id"].ToString()), Convert.ToInt32(parametersIn["srv_id"].ToString()), nContractId);
                                partnerList["exp_date"] = GetFormattedServiceExpDate(Convert.ToInt32(parametersIn["pnr_id"].ToString()), Convert.ToInt32(parametersIn["srv_id"].ToString()), nContractId);
                                partnerList["partnerPhone"] = GetPartnerPhone(Convert.ToInt32(parametersIn["pnr_id"].ToString()), Convert.ToInt32(parametersIn["srv_id"].ToString()), nContractId);
                                partnerList["partnerLink"] = GetPartnerLink(Convert.ToInt32(parametersIn["pnr_id"].ToString()), Convert.ToInt32(parametersIn["srv_id"].ToString()), nContractId);
                                partnerList["code"] = "";
                                partnerList["codetype"] = "";

                                // Send mail to partner
                                // If this mail fails, return
                                if (!SendEmailServicePartner(partnerList))
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("BuyServiceXML::Error - could not send partner email to validate coupon: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }

                                // Send mail to user if have email
                                // If this mail fails, don't worry about it
                                if (partnerList["userEmail"].ToString().Length > 0)
                                {
                                    if (!SendEmailServiceUser(partnerList))
                                        Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Could not send email to user"), LoggerSeverities.Error);
                                }
                            }

                            // Insert service operation for user
                            long lServiceId = GetServiceId(Convert.ToInt32(parametersIn["pnr_id"]), Convert.ToInt32(parametersIn["srv_id"]), nContractId);
                            if (lServiceId <= 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Error retrieving service ID: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }
                            partnerList["mos_id"] = lServiceId.ToString();
                            partnerList["mu_id"] = parametersIn["mui"];
                            partnerList["price"] = nServicePrice.ToString();

                            if (!AddServiceOperation(partnerList, nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Error adding service operation: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Charge user for service
                            // Send contract Id as 0 so that it uses the global users connection
                            if (!UpdateUserSpaceNotifications(Convert.ToInt32(parametersIn["mui"]), nServicePrice, 2, 0))
                            {
                                // Have to cancel service operation since payment failed
                                if (!UpdateServiceOperationStatus(Convert.ToInt64(partnerList["op"].ToString()), Convert.ToInt32(ConfigurationManager.AppSettings["ServiceOperationsStatus.Cancelled"]), nContractId))
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Error cancelling service operation: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }

                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("BuyServiceXML::Error - Error charging user for service operation: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Get spaces bonus information
                            // Send contract Id as 0 so that it uses the global users connection
                            int nCurUserSpaces = GetUserNumSpaces(Convert.ToInt32(parametersIn["mui"]), 0);
                            parametersOut["c_bns"] = nCurUserSpaces.ToString();

                            // Get user bonus information
                            if (!GetUserBonusData(Convert.ToInt32(parametersIn["mui"]), out bonusList, nContractId))
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryUserBonusXML::Error - Could not obtain bonus operation data: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (bonusList.Count > 0)
                                parametersOut["bonuslist"] = bonusList;
                            else
                            {
                                bonusList = new SortedList();
                                bonusList["bonus1"] = "";
                                parametersOut["bonuslist"] = bonusList;
                            }

                            parametersOut["t"] = DateTime.Now.ToString("HHmmssddMMyy");
                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            xmlOut = GenerateXMLOuput(parametersOut);
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("BuyServiceXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("BuyServiceXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
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
        [WebMethod]
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
        [WebMethod]
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

        [WebMethod]
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

        [WebMethod]
        public void RegisterUserJSON(string jsonIn)
        {
            int iOut = (int)ResultType.Result_Error_Generic;

            try
            {
                Logger_AddLogMessage(string.Format("RegisterUserJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iOut = RegisterUserXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("RegisterUserJSON: jsonOut= {0}", iOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iOut);
            }
            catch (Exception e)
            {
                iOut = (int)ResultType.Result_Error_Invalid_Input_Parameter;
                Logger_AddLogMessage(string.Format("RegisterUserJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, iOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void LoginUserJSON(string jsonIn)
        {
            string strOut = ResultType.Result_Error_Generic.ToString();

            try
            {
                Logger_AddLogMessage(string.Format("LoginUserJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                strOut = LoginUserXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("LoginUserJSON: jsonOut= {0}", strOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(strOut);
            }
            catch (Exception e)
            {
                strOut = ResultType.Result_Error_Invalid_Input_Parameter.ToString();
                Logger_AddLogMessage(string.Format("LoginUserJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, strOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void UpdateUserJSON(string jsonIn)
        {
            string jsonOut = "";

            try
            {
                Logger_AddLogMessage(string.Format("UpdateUserJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = UpdateUserXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                // Modify vector format for case of only 1 plate
                int nStart = jsonOut.IndexOf("plate\":{");
                if (nStart > 0)
                {
                    string strTemp = jsonOut.Insert(nStart + 7, "[");
                    int nEnd = jsonOut.IndexOf("}}", nStart);
                    strTemp = strTemp.Insert(nEnd + 2, "]");
                    jsonOut = strTemp;
                    Logger_AddLogMessage(string.Format("UpdateUserJSON: Modified plate format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                }

                // Modify vector format for case of only 1 sector
                nStart = 0;
                int nIndex = 0;
                do
                {
                    nStart = jsonOut.IndexOf("sp\":\"", nIndex);
                    if (nStart > 0)
                    {
                        string strTemp = jsonOut.Insert(nStart + 4, "[");
                        int nEnd = jsonOut.IndexOf("\"}", nStart);
                        strTemp = strTemp.Insert(nEnd + 2, "]");
                        jsonOut = strTemp;
                        Logger_AddLogMessage(string.Format("UpdateUserJSON: Modified sector format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                        nIndex = nStart + 1;
                    }
                }
                while (nStart > 0);

                Logger_AddLogMessage(string.Format("UpdateUserJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("UpdateUserJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryUserJSON(string jsonIn)
        {
            string jsonOut = "";

            try
            {
                Logger_AddLogMessage(string.Format("QueryUserJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryUserXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                // Modify vector format for case of only 1 plate
                int nStart = jsonOut.IndexOf("plate\":{");
                if (nStart > 0)
                {
                    string strTemp = jsonOut.Insert(nStart + 7, "[");
                    int nEnd = jsonOut.IndexOf("}}", nStart);
                    strTemp = strTemp.Insert(nEnd + 2, "]");
                    jsonOut = strTemp;
                    Logger_AddLogMessage(string.Format("QueryUserJSON: Modified plate format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                }

                // Modify vector format for case of only 1 sector
                nStart = 0;
                int nIndex = 0;
                do
                {
                    nStart = jsonOut.IndexOf("sp\":\"", nIndex);
                    if (nStart > 0)
                    {
                        string strTemp = jsonOut.Insert(nStart + 4, "[");
                        int nEnd = jsonOut.IndexOf("\"}", nStart);
                        strTemp = strTemp.Insert(nEnd + 2, "]");
                        jsonOut = strTemp;
                        Logger_AddLogMessage(string.Format("QueryUserJSON: Modified sector format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                        nIndex = nStart + 1;
                    }
                }
                while (nStart > 0);

                Logger_AddLogMessage(string.Format("QueryUserJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryUserJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryUserCreditJSON(string jsonIn)
        {
            int iOut = (int)ResultType.Result_Error_Generic;

            try
            {
                Logger_AddLogMessage(string.Format("QueryUserCreditJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iOut = QueryUserCreditXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("QueryUserCreditJSON: jsonOut= {0}", iOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iOut);
            }
            catch (Exception e)
            {
                iOut = (int)ResultType.Result_Error_Invalid_Input_Parameter;
                Logger_AddLogMessage(string.Format("QueryUserCreditJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, iOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void RechargeUserCreditJSON(string jsonIn)
        {
            string jsonOut = "";

            try
            {
                Logger_AddLogMessage(string.Format("RechargeUserCreditJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = RechargeUserCreditXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("RechargeUserCreditJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("RechargeUserCreditJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryUserOperationsJSON(string jsonIn)
        {
            string jsonOut = "";

            try
            {
                Logger_AddLogMessage(string.Format("QueryUserOperationsJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryUserOperationsXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryUserOperationsJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                // Modify vector format for case of only 1 operation
                int nStart = jsonOut.IndexOf("o\":{");
                if (nStart > 0)
                {
                    string strTemp = jsonOut.Insert(nStart + 3, "[");
                    int nEnd = jsonOut.IndexOf("}},\"r", nStart);
                    strTemp = strTemp.Insert(nEnd + 2, "]");
                    jsonOut = strTemp;
                    Logger_AddLogMessage(string.Format("QueryUserJSON: Modified operation format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                }

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryUserOperationsJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void UnregisterUserNotificationsJSON(string jsonIn)
        {
            int iOut = (int)ResultType.Result_Error_Generic;

            try
            {
                Logger_AddLogMessage(string.Format("UnregisterUserNotificationsJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iOut = UnregisterUserNotificationsXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("UnregisterUserNotificationsJSON: jsonOut= {0}", iOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iOut);
            }
            catch (Exception e)
            {
                iOut = (int)ResultType.Result_Error_Invalid_Input_Parameter;
                Logger_AddLogMessage(string.Format("UnregisterUserNotificationsJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, iOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryUserReportJSON(string jsonIn)
        {
            int iOut = (int)ResultType.Result_Error_Generic;

            try
            {
                Logger_AddLogMessage(string.Format("QueryUserReportJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iOut = QueryUserReportXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("QueryUserReportJSON: jsonOut= {0}", iOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iOut);
            }
            catch (Exception e)
            {
                iOut = (int)ResultType.Result_Error_Invalid_Input_Parameter;
                Logger_AddLogMessage(string.Format("QueryUserReportJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, iOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryBonusJSON(string jsonIn)
        {
            string jsonOut = "";

            try
            {
                Logger_AddLogMessage(string.Format("QueryBonusJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryBonusXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryBonusJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                // Modify vector format for case of only 1 bonus operation
                int nStart = jsonOut.IndexOf("bonus\":{");
                if (nStart > 0)
                {
                    string strTemp = jsonOut.Insert(nStart + 7, "[");
                    int nEnd = jsonOut.IndexOf("}},\"c", nStart);
                    strTemp = strTemp.Insert(nEnd + 2, "]");
                    jsonOut = strTemp;
                    Logger_AddLogMessage(string.Format("QueryBonusJSON: Modified bonus format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                }

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryBonusJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryUserBonusJSON(string jsonIn)
        {
            string jsonOut = "";

            try
            {
                Logger_AddLogMessage(string.Format("QueryUserBonusJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryUserBonusXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryUserBonusJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                // Modify vector format for case of only 1 bonus operation
                int nStart = jsonOut.IndexOf("bonus\":{");
                if (nStart > 0)
                {
                    string strTemp = jsonOut.Insert(nStart + 7, "[");
                    int nEnd = jsonOut.IndexOf("}},\"c", nStart);
                    strTemp = strTemp.Insert(nEnd + 2, "]");
                    jsonOut = strTemp;
                    Logger_AddLogMessage(string.Format("QueryUserBonusJSON: Modified bonus format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                }

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryUserBonusJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryBonusServicesJSON(string jsonIn)
        {
            string jsonOut = "";

            try
            {
                Logger_AddLogMessage(string.Format("QueryBonusServicesJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryBonusServicesXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryBonusServicesJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                // Modify vector format for case of only 1 service
                int nStart = jsonOut.IndexOf("service\":{");
                if (nStart > 0)
                {
                    string strTemp = jsonOut.Insert(nStart + 9, "[");
                    int nEnd = jsonOut.IndexOf("}},\"t", nStart);
                    strTemp = strTemp.Insert(nEnd + 2, "]");
                    jsonOut = strTemp;
                    Logger_AddLogMessage(string.Format("QueryBonusServicesJSON: Modified bonus format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                }

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryBonusServicesJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryFreeSpacesJSON(string jsonIn)
        {
            string jsonOut = "";

            try
            {
                Logger_AddLogMessage(string.Format("QueryFreeSpacesJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryFreeSpacesXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryFreeSpacesJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                // Modify vector format for case of only 1 bonus operation
                int nStart = jsonOut.IndexOf("spc\":{");
                if (nStart > 0)
                {
                    string strTemp = jsonOut.Insert(nStart + 5, "[");
                    int nEnd = jsonOut.IndexOf("}},\"t", nStart);
                    strTemp = strTemp.Insert(nEnd + 2, "]");
                    jsonOut = strTemp;
                    Logger_AddLogMessage(string.Format("QueryFreeSpacesJSON: Modified space format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                }

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryFreeSpacesJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void BuyServiceJSON(string jsonIn)
        {
            string jsonOut = "";

            try
            {
                Logger_AddLogMessage(string.Format("BuyServiceJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = BuyServiceXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("BuyServiceJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                // Modify vector format for case of only 1 bonus operation
                int nStart = jsonOut.IndexOf("bonus\":{");
                if (nStart > 0)
                {
                    string strTemp = jsonOut.Insert(nStart + 7, "[");
                    int nEnd = jsonOut.IndexOf("}},\"c", nStart);
                    strTemp = strTemp.Insert(nEnd + 2, "]");
                    jsonOut = strTemp;
                    Logger_AddLogMessage(string.Format("BuyServiceJSON: Modified bonus format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                }

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("BuyServiceJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void RecoverPasswordJSON(string jsonIn)
        {
            int iOut = (int)ResultType.Result_Error_Generic;

            try
            {
                Logger_AddLogMessage(string.Format("RecoverPasswordJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iOut = RecoverPasswordXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("RecoverPasswordJSON: jsonOut= {0}", iOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iOut);
            }
            catch (Exception e)
            {
                iOut = (int)ResultType.Result_Error_Invalid_Input_Parameter;
                Logger_AddLogMessage(string.Format("RecoverPasswordJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, iOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void VerifyRecoveryPasswordJSON(string jsonIn)
        {
            int iOut = (int)ResultType.Result_Error_Generic;

            try
            {
                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iOut = VerifyRecoveryPasswordXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordJSON: jsonOut= {0}", iOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iOut);
            }
            catch (Exception e)
            {
                iOut = (int)ResultType.Result_Error_Invalid_Input_Parameter;
                Logger_AddLogMessage(string.Format("VerifyRecoveryPasswordJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, iOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void ChangePasswordJSON(string jsonIn)
        {
            string strOut = ResultType.Result_Error_Generic.ToString();

            try
            {
                Logger_AddLogMessage(string.Format("ChangePasswordJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                strOut = ChangePasswordXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("ChangePasswordJSON: jsonOut= {0}", strOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(strOut);
            }
            catch (Exception e)
            {
                strOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("ChangePasswordJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, strOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        #endregion

        #region Private Methods

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

        private bool LogMsgDB(string xmlIn, string xmlOut, int iVirtualUnit, int nContractId = 0)
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

                string strSQL1 = " insert into MOBILE_USERS (mu_name, mu_surname1, mu_email, mu_mobile_telephone, mu_login, mu_password, mu_activate_account, mu_addr_country, mu_fine_notify, mu_unpark_notify, mu_unpark_notify_time, mu_recharge_notify, mu_balance_notify, mu_balance_notify_amount";
                string strSQL2 = " ) VALUES( INITCAP('" + parametersIn["na"].ToString().Replace('\'', ',') + "'),INITCAP('" + parametersIn["fs"].ToString().Replace('\'', ',') + "'),'" + parametersIn["em"].ToString() + "','";
                strSQL2 += parametersIn["mmp"].ToString() + "', '" + parametersIn["un"].ToString().Replace('\'', ',') + "', '" + parametersIn["pw"].ToString().Replace('\'', ',') + "', " + ConfigurationManager.AppSettings["ActivateAccount.No"].ToString() + ", '";
                strSQL2 += ConfigurationManager.AppSettings["AddressCountry.Spain"].ToString() + "'," + parametersIn["fn"].ToString() + ", " + parametersIn["unp"].ToString() + ", " + parametersIn["t_unp"].ToString() + ", " + parametersIn["re"].ToString() + ", " + parametersIn["ba"].ToString() + ", " + parametersIn["q_ba"].ToString();
                if (parametersIn["nif"] != null)
                {
                    strSQL1 += " , mu_dni";
                    strSQL2 += ", UPPER('" + parametersIn["nif"].ToString() + "')";
                }
                if (parametersIn["ss"] != null)
                {
                    strSQL1 += " , mu_surname2";
                    strSQL2 += ", INITCAP('" + parametersIn["ss"].ToString().Replace('\'', ',') + "')";
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

        private int GetUserId(string strToken, int nContractId = 0)
        {
            int nUserId = -1;
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

                string strSQL = string.Format("SELECT MU_ID FROM MOBILE_USERS WHERE MU_AUTH_TOKEN = '{0}'", strToken);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nUserId = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserId::Exception", LoggerSeverities.Error);
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

            return nUserId;
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

        private string GenerateMerchantParm(int nAmount, int nOrder)
        {
            StringBuilder sbMerchantParm = new StringBuilder();

            sbMerchantParm.AppendFormat("{{");
            sbMerchantParm.AppendFormat(@"""DS_MERCHANT_MERCHANTCODE"":""{0}"",", MERCHANT_CODE);
            sbMerchantParm.AppendFormat(@"""DS_MERCHANT_TERMINAL"":""{0}"",", ConfigurationManager.AppSettings["MerchantTerminal"].ToString());
            sbMerchantParm.AppendFormat(@"""DS_MERCHANT_CURRENCY"":""{0}"",", ConfigurationManager.AppSettings["MerchantCurrency"].ToString());
            sbMerchantParm.AppendFormat(@"""DS_MERCHANT_URLKO"":""{0}"",", ConfigurationManager.AppSettings["MerchantUrlKo"].ToString());
            sbMerchantParm.AppendFormat(@"""DS_MERCHANT_URLOK"":""{0}"",", ConfigurationManager.AppSettings["MerchantUrlOk"].ToString());
            sbMerchantParm.AppendFormat(@"""DS_MERCHANT_AMOUNT"":""{0}"",", nAmount.ToString());
            sbMerchantParm.AppendFormat(@"""DS_MERCHANT_ORDER"":""{0}"",", nOrder.ToString());
            sbMerchantParm.AppendFormat(@"""DS_MERCHANT_TRANSACTIONTYPE"":""{0}"",", ConfigurationManager.AppSettings["MerchantTranstactionType"].ToString());
            sbMerchantParm.AppendFormat(@"""DS_MERCHANT_MERCHANTURL"":""{0}""", ConfigurationManager.AppSettings["MerchantUrl"].ToString());
            sbMerchantParm.AppendFormat("}}");

            // *** TEST
            //sbMerchantParm.AppendFormat("{{");
            //sbMerchantParm.AppendFormat(@"""DS_MERCHANT_MERCHANTCODE"":""{0}"",", "322319344");
            //sbMerchantParm.AppendFormat(@"""DS_MERCHANT_TERMINAL"":""{0}"",", "1");
            //sbMerchantParm.AppendFormat(@"""DS_MERCHANT_CURRENCY"":""{0}"",", "978");
            //sbMerchantParm.AppendFormat(@"""DS_MERCHANT_URLKO"":""{0}"",", "http://ops.ods.org:58100/OPSPayMobileWeb/ResponseKO.aspx");
            //sbMerchantParm.AppendFormat(@"""DS_MERCHANT_URLOK"":""{0}"",", "http://ops.ods.org:58100/OPSPayMobileWeb/ResponseOK.aspx");
            //sbMerchantParm.AppendFormat(@"""DS_MERCHANT_AMOUNT"":""{0}"",", "500");
            //sbMerchantParm.AppendFormat(@"""DS_MERCHANT_ORDER"":""{0}"",", "47519");
            //sbMerchantParm.AppendFormat(@"""DS_MERCHANT_TRANSACTIONTYPE"":""{0}"",", "0");
            //sbMerchantParm.AppendFormat(@"""DS_MERCHANT_MERCHANTURL"":""{0}""", "http://ops.ods.org:58100/OPSPayMobileWeb/CheckResponse.aspx");
            //sbMerchantParm.AppendFormat("}}");

            string data = EncodeTo64(sbMerchantParm.ToString());

            return data;
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

        private int RechargeCredit(SortedList parametersIn, int nContractId = 0)
        {
            int nRes = (int)ResultType.Result_Error_Generic;
            int nMobileOrdersId = 0;

            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            OracleDataReader dataReader = null;

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

                string strSQL = string.Format("SELECT MAX(MO_ID) + 1 FROM MOBILE_ORDERS");
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nMobileOrdersId = dataReader.GetInt32(0);
                }

                if (nMobileOrdersId > 0)
                {
                    strSQL = string.Format("INSERT INTO MOBILE_ORDERS (MO_ID, MO_MU_ID, MO_DATE, MO_HORA, MO_AMOUNT, MO_TERMINAL, MO_RESPONSE) VALUES ({0}, {1}, '{2}', '{3}', {4}, {5}, {6} )",
                        nMobileOrdersId, parametersIn["mui"], parametersIn["d"], parametersIn["t"], parametersIn["am"], ConfigurationManager.AppSettings["MobileOrdersTerminal.Web"].ToString(), parametersIn["resp"]);
                    oraCmd.CommandText = strSQL;
                    if (oraCmd.ExecuteNonQuery() > 0)
                    {
                        // Update table to launch trigger
                        if (Convert.ToInt32(parametersIn["resp"]) < 100)
                        {
                            strSQL = string.Format("UPDATE MOBILE_ORDERS SET MO_AMOUNT = {0}, MO_RESPONSE = {1} WHERE MO_ID = {2}", parametersIn["am"], parametersIn["resp"], nMobileOrdersId);
                            oraCmd.CommandText = strSQL;
                            if (oraCmd.ExecuteNonQuery() > 0)
                                nRes = (int)ResultType.Result_OK;
                        }
                        else
                            nRes = (int)ResultType.Result_OK;
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("RechargeCredit::Exception", LoggerSeverities.Error);
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

        private bool CancelUserNotifications(int nMobileUserId, string strCloudId, int nContractId = 0)
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

                string strSQL = string.Format("update MOBILE_USERS set mu_cloud_token = NULL WHERE MU_ID = {0} and MU_CLOUD_TOKEN = '{1}'", nMobileUserId, strCloudId);

                oraCmd.CommandText = strSQL;

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("CancelUserNotifications::Exception", LoggerSeverities.Error);
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

        private bool UpdateWebCredentials(int nMobileUserId, string strCloudId, int nOs, string strVersion, int nContractId = 0)
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

                string strSQL = string.Format("update MOBILE_USERS set mu_cloud_token = '{0}', mu_device_os = {1}, mu_app_version = '{2}' WHERE MU_ID = {3}", strCloudId, nOs, strVersion, nMobileUserId);

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

        bool SendEmailServiceUser(SortedList parametersService)
        {
            bool bResult = false;

            try
            {
                MailMessage MyMailMessage = new MailMessage();
                MyMailMessage.From = new MailAddress(ConfigurationManager.AppSettings["SendAddress"].ToString());
                MyMailMessage.To.Add(parametersService["userEmail"].ToString());

                // Prepare header
                string headerURL = ConfigurationManager.AppSettings["ReportHeaderLogo"].ToString();
                Attachment attachHeader = new Attachment(headerURL);
                MyMailMessage.Attachments.Add(attachHeader);
                string headerContentID = "imageheader001@host";
                attachHeader.ContentId = headerContentID;
                string strHeader = "<html><body><img src=\"cid:" + headerContentID + "\">";

                // Prepare title
                DateTime dtDate = DateTime.ParseExact(parametersService["date"].ToString(), "HHmmssddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                string strDate = dtDate.ToString("dd/MM/yy HH:mm:ss");
                string emailTitle = "<p style=\"text-align:center;font-size:160%;\">Canjeo bono</p><p style=\"text-align:center;\">" + strDate + "</p><br>";

                // Prepare body
                string emailBody = "<p>Se ha canjeado el bono siguiente:</p><p>" + parametersService["partnerName"].ToString() + " - " + parametersService["op"].ToString() + "</p><p><b>"
                    + parametersService["serviceName"].ToString() + "</b> - " + parametersService["serviceDesc"].ToString() + "</p>"
                    + "<p>Este bono caduca el día " + parametersService["exp_date"].ToString() + ".</p>"
                    + "<p></p><p>Presente esta información en las instalaciones del proveedor del servicio para consumir el bono adquirido.</p>"
                    + "<br><p>Si tiene alguna duda o consulta, puede contactar en: " + parametersService["partnerName"].ToString() + " Tlf: " + parametersService["partnerPhone"].ToString() + " <a href=\""
                    + parametersService["partnerLink"].ToString() + "target=\"_blank\" > " + parametersService["partnerLink"].ToString() + "</a></p>";

                // Prepare footer
                string footerURL = ConfigurationManager.AppSettings["ReportFooterLogo"].ToString();
                Attachment attachFooter = new Attachment(footerURL);
                MyMailMessage.Attachments.Add(attachFooter);
                string footerContentID = "imagefooter001@host";
                attachFooter.ContentId = footerContentID;
                string strFooter = "<img src=\"cid:" + footerContentID + "\"></body></html>";

                string bodyMessage = strHeader + emailTitle + emailBody + strFooter;
                string emailSubject = "Canjeo de bono Z+M " + parametersService["op"].ToString();

                MyMailMessage.Subject = emailSubject;
                MyMailMessage.IsBodyHtml = true;
                MyMailMessage.Body = bodyMessage;
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
                Logger_AddLogMessage("SendEmailServiceUser::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return bResult;
        }

        bool SendEmailServicePartner(SortedList parametersService)
        {
            bool bResult = false;

            try
            {
                MailMessage MyMailMessage = new MailMessage();
                MyMailMessage.From = new MailAddress(ConfigurationManager.AppSettings["SendAddress"].ToString());
                MyMailMessage.To.Add(parametersService["partnerEmail"].ToString());

                // Prepare header
                string headerURL = ConfigurationManager.AppSettings["ReportHeaderLogo"].ToString();
                Attachment attachHeader = new Attachment(headerURL);
                MyMailMessage.Attachments.Add(attachHeader);
                string headerContentID = "imageheader001@host";
                attachHeader.ContentId = headerContentID;
                string strHeader = "<html><body><img src=\"cid:" + headerContentID + "\">";

                // Prepare title
                DateTime dtDate = DateTime.ParseExact(parametersService["date"].ToString(), "HHmmssddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                string strDate = dtDate.ToString("dd/MM/yy HH:mm:ss");
                string emailTitle = "<p style=\"text-align:center;font-size:160%;\">Canjeo bono</p><p style=\"text-align:center;\">" + strDate + "</p><br>";

                // Prepare body
                string emailBody = "<p>El usuario <b>" + parametersService["un"].ToString() + "</b> ha canjeado el bono siguiente:</p><p>" + parametersService["op"].ToString() + "</p><p><b>"
                    + parametersService["serviceName"].ToString() + "</b> - " + parametersService["serviceDesc"].ToString() + "</p><br>";

                // Prepare footer
                string footerURL = ConfigurationManager.AppSettings["ReportFooterLogo"].ToString();
                Attachment attachFooter = new Attachment(footerURL);
                MyMailMessage.Attachments.Add(attachFooter);
                string footerContentID = "imagefooter001@host";
                attachFooter.ContentId = footerContentID;
                string strFooter = "<img src=\"cid:" + footerContentID + "\"></body></html>";

                string bodyMessage = strHeader + emailTitle + emailBody + strFooter;
                string emailSubject = "Canjeo de bono Z+M " + parametersService["op"].ToString();

                MyMailMessage.Subject = emailSubject;
                MyMailMessage.IsBodyHtml = true;
                MyMailMessage.Body = bodyMessage;
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
                Logger_AddLogMessage("SendEmailServicePartner::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
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
                Logger_AddLogMessage("SendEmail::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
            finally
            {
                doc.Close();
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

        private bool GetGroupName(int nGroupId, out string strGroupName, int nContractId = 0)
        {
            strGroupName = "";
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

                string strSQL = string.Format("SELECT GRP_DESCSHORT FROM GROUPS WHERE GRP_ID = {0}", nGroupId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strGroupName = dataReader.GetString(0);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetGroupName::Exception", LoggerSeverities.Error);
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

        private int GetNumSpacesBonus(int nContractId = 0)
        {
            int nBonus = 0;
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

                string strSQL = string.Format("SELECT NVL(PAR_VALUE,0) FROM PARAMETERS WHERE PAR_DESCSHORT = '{0}'", PARM_NUM_SPACES_BONUS);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nBonus = Convert.ToInt32(dataReader.GetString(0));
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetNumSpacesBonus::Exception", LoggerSeverities.Error);
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

            return nBonus;
        }

        private int GetUserNumSpaces(int nMobileUserId, int nContractId = 0)
        {
            int nNumSpaces = 0;
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

                string strSQL = string.Format("SELECT NVL(MU_NUM_SHARED_SPACES,0) FROM MOBILE_USERS WHERE MU_ID = {0}", nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nNumSpaces = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetUserNumSpaces::Exception", LoggerSeverities.Error);
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

            return nNumSpaces;
        }

        private long GetServiceId(int nPartnerId, int nServiceId, int nContractId = 0)
        {
            long lId = -1;
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

                string strSQL = string.Format("SELECT MOS_ID FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1}", nPartnerId, nServiceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    lId = dataReader.GetInt64(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetServiceId::Exception", LoggerSeverities.Error);
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

            return lId;
        }

        private int GetServicePrice(int nPartnerId, int nServiceId, int nContractId = 0)
        {
            int nPrice = -1;
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

                string strSQL = string.Format("SELECT MOS_COST FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1}", nPartnerId, nServiceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nPrice = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetServicePrice::Exception", LoggerSeverities.Error);
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

            return nPrice;
        }

        private int GetServiceType(int nPartnerId, int nServiceId, int nContractId = 0)
        {
            int nType = -1;
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

                string strSQL = string.Format("SELECT MOS_TYPE FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1}", nPartnerId, nServiceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nType = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetServiceType::Exception", LoggerSeverities.Error);
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

            return nType;
        }

        private string GetServiceEmail(int nPartnerId, int nServiceId, int nContractId = 0)
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

                string strSQL = string.Format("SELECT NVL(MOS_PARTNER_EMAIL, '-') AS MOS_PARTNER_EMAIL FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1}", nPartnerId, nServiceId);
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
                Logger_AddLogMessage("GetServiceEmail::Exception", LoggerSeverities.Error);
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

        private string GetServiceName(int nPartnerId, int nServiceId, int nContractId = 0)
        {
            string strName = "";
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

                string strSQL = string.Format("SELECT NVL(MOS_NAME, '-') AS MOS_NAME FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1}", nPartnerId, nServiceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strName = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetServiceName::Exception", LoggerSeverities.Error);
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

            return strName;
        }

        private string GetServiceDesc(int nPartnerId, int nServiceId, int nContractId = 0)
        {
            string strDesc = "";
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

                string strSQL = string.Format("SELECT NVL(MOS_DESC, '-') AS MOS_DESC FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1}", nPartnerId, nServiceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strDesc = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetServiceDesc::Exception", LoggerSeverities.Error);
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

            return strDesc;
        }

        private string GetServiceExpDate(int nPartnerId, int nServiceId, int nContractId = 0)
        {
            string strDate = "";
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

                string strSQL = string.Format("SELECT TO_CHAR((SYSDATE + MOS_EXP_RANGE), 'HH24MISSDDMMYY') AS MOS_EXP_DATE FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1}", nPartnerId, nServiceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strDate = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetServiceExpDate::Exception", LoggerSeverities.Error);
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

            return strDate;
        }

        private string GetFormattedServiceExpDate(int nPartnerId, int nServiceId, int nContractId = 0)
        {
            string strDate = "";
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

                string strSQL = string.Format("SELECT TO_CHAR((SYSDATE + MOS_EXP_RANGE), 'DD/MM/YYYY') AS MOS_EXP_DATE FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1}", nPartnerId, nServiceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strDate = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetFormattedServiceExpDate::Exception", LoggerSeverities.Error);
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

            return strDate;
        }

        private string GetPartnerName(int nPartnerId, int nServiceId, int nContractId = 0)
        {
            string strName = "";
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

                string strSQL = string.Format("SELECT NVL(MOS_PARTNER_NAME, '-') AS MOS_PARTNER_NAME FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1}", nPartnerId, nServiceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strName = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetPartnerName::Exception", LoggerSeverities.Error);
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

            return strName;
        }

        private string GetPartnerPhone(int nPartnerId, int nServiceId, int nContractId = 0)
        {
            string strPhone = "";
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

                string strSQL = string.Format("SELECT NVL(MOS_PARTNER_PHONE, '-') AS MOS_PARTNER_PHONE FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1}", nPartnerId, nServiceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strPhone = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetPartnerPhone::Exception", LoggerSeverities.Error);
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

            return strPhone;
        }

        private string GetPartnerLink(int nPartnerId, int nServiceId, int nContractId = 0)
        {
            string strLink = "";
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

                string strSQL = string.Format("SELECT NVL(MOS_PARTNER_LINK, '-') AS MOS_PARTNER_LINK FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1}", nPartnerId, nServiceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strLink = dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetPartnerLink::Exception", LoggerSeverities.Error);
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

            return strLink;
        }

        private bool IsServiceValid(int nPartnerId, int nServiceId, int nContractId = 0)
        {
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

                string strSQL = string.Format("SELECT COUNT(*) FROM MOBILE_SERVICES WHERE MOS_PARTNER_ID = {0} AND MOS_SERVICE_ID = {1} AND SYSDATE < NVL(MOS_EXP_DATE, SYSDATE + 1)", nPartnerId, nServiceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    if (dataReader.GetInt32(0) > 0)
                        bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("OperationAlreadyExists::Exception", LoggerSeverities.Error);
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

        private bool GetBonusData(int nMobileUserId, out SortedList bonusDataList, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            bonusDataList = new SortedList();
            int nBonusIndex = 0;

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

                // Get a list of all the bonus operations associated with the user
                ArrayList bonusList = new ArrayList();
                string strSQL = string.Format("SELECT OPE_ID, TO_CHAR( OPE_MOVDATE, 'hh24missddMMYY'), OPE_VALUE_VIS FROM OPERATIONS WHERE OPE_MOBI_USER_ID = {0} AND OPE_DOPE_ID = {1} AND OPE_RECHARGE_TYPE = {2} ORDER BY OPE_MOVDATE DESC",
                    nMobileUserId, ConfigurationManager.AppSettings["OperationsDef.Recharge"].ToString(), ConfigurationManager.AppSettings["RechargeType.Bonus"].ToString());
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    SortedList bonusData = new SortedList();
                    bonusData["on"] = dataReader.GetInt32(0).ToString();
                    bonusData["bns_t"] = dataReader.GetString(1);
                    bonusData["pa"] = dataReader.GetInt32(2).ToString();
                    nBonusIndex++;
                    bonusDataList["bonus" + nBonusIndex.ToString()] = bonusData;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetBonusData::Exception", LoggerSeverities.Error);
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

        private bool GetUserBonusData(int nMobileUserId, out SortedList bonusDataList, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            bonusDataList = new SortedList();
            int nBonusIndex = 0;

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

                // Get a list of all the bonus operations associated with the user
                ArrayList bonusList = new ArrayList();
                string strSQL = string.Format("SELECT MOP_ID, TO_CHAR( MOP_DATE, 'hh24missddMMYY'), MOS_PARTNER_ID, MOS_PARTNER_NAME, MOS_SERVICE_ID, MOS_NAME, MOS_DESC, MOS_ICON, MOS_TYPE, MOP_STATE, TO_CHAR( MOP_DATE_USED, 'hh24missddMMYY'), TO_CHAR( MOP_EXP_DATE, 'hh24missddMMYY'), MOP_PAYMENT, MOP_BARCODE, MOP_BARCODE_TYPE, MOS_PARTNER_PHONE, MOS_PARTNER_LINK FROM MOBILE_SERVICES, MOBILE_SERVICES_OPERATIONS WHERE MOP_MOS_ID = MOS_ID AND MOP_MU_ID = {0} ORDER BY MOP_DATE DESC, MOS_PARTNER_ID, MOS_SERVICE_ID ",
                    nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    SortedList bonusData = new SortedList();
                    bonusData["on"] = dataReader.GetInt64(0).ToString();
                    bonusData["bns_t"] = dataReader.GetString(1);
                    bonusData["pnr_id"] = dataReader.GetInt32(2).ToString();
                    if (!dataReader.IsDBNull(3))
                        bonusData["pnr_n"] = dataReader.GetString(3);
                    else
                        bonusData["pnr_n"] = " ";
                    bonusData["srv_id"] = dataReader.GetInt32(4).ToString();
                    if (!dataReader.IsDBNull(5))
                        bonusData["srv_n"] = dataReader.GetString(5);
                    else
                        bonusData["srv_n"] = " ";
                    if (!dataReader.IsDBNull(6))
                        bonusData["srv_d"] = dataReader.GetString(6);
                    else
                        bonusData["srv_d"] = " ";
                    if (!dataReader.IsDBNull(7))
                        bonusData["srv_ico"] = dataReader.GetString(7);
                    else
                        bonusData["srv_ico"] = " ";
                    bonusData["srv_type"] = dataReader.GetInt32(8).ToString();
                    bonusData["csm"] = dataReader.GetInt32(9).ToString();
                    if (!dataReader.IsDBNull(10))
                        bonusData["csm_t"] = dataReader.GetString(10);
                    else
                        bonusData["csm_t"] = " ";
                    if (!dataReader.IsDBNull(11))
                        bonusData["exp_t"] = dataReader.GetString(11);
                    else
                        bonusData["exp_t"] = " ";
                    bonusData["pa"] = dataReader.GetInt32(12).ToString();
                    if (!dataReader.IsDBNull(13))
                        bonusData["code"] = dataReader.GetString(13);
                    else
                        bonusData["code"] = " ";
                    if (!dataReader.IsDBNull(14))
                        bonusData["code_type"] = dataReader.GetInt32(14).ToString();
                    else
                        bonusData["code_type"] = " ";
                    if (!dataReader.IsDBNull(15))
                        bonusData["pnr_tp"] = dataReader.GetString(15);
                    else
                        bonusData["pnr_tp"] = " ";
                    if (!dataReader.IsDBNull(16))
                        bonusData["pnr_lnk"] = dataReader.GetString(16);
                    else
                        bonusData["pnr_lnk"] = " ";
                    nBonusIndex++;
                    bonusDataList["bonus" + nBonusIndex.ToString()] = bonusData;

                    // 2016/11/03 - Remove this for now because the documentation was incorrect, may have to add it again later
                    //// Retrieve a maximum of 6 operations
                    //if (nBonusIndex >= 6)
                    //    break;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetBonusServicesData::Exception", LoggerSeverities.Error);
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

        private bool GetBonusServicesData(out SortedList serviceDataList, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            serviceDataList = new SortedList();
            int nBonusIndex = 0;

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

                // Get a list of all the bonus operations associated with the user
                ArrayList bonusList = new ArrayList();
                string strSQL = string.Format("SELECT MOS_PARTNER_ID, MOS_PARTNER_NAME, MOS_SERVICE_ID, MOS_NAME, MOS_DESC, MOS_ICON, MOS_TYPE, TO_CHAR( MOS_EXP_DATE, 'hh24missddMMYY'), MOS_COST, MOS_PARTNER_PHONE, MOS_PARTNER_LINK FROM MOBILE_SERVICES WHERE SYSDATE < NVL(MOS_EXP_DATE, SYSDATE + 1) ORDER BY MOS_PARTNER_ID, MOS_SERVICE_ID");
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    SortedList serviceData = new SortedList();
                    serviceData["pnr_id"] = dataReader.GetInt32(0).ToString();
                    if (!dataReader.IsDBNull(1))
                        serviceData["pnr_n"] = dataReader.GetString(1);
                    else
                        serviceData["pnr_n"] = " ";
                    serviceData["srv_id"] = dataReader.GetInt32(2).ToString();
                    if (!dataReader.IsDBNull(3))
                        serviceData["srv_n"] = dataReader.GetString(3);
                    else
                        serviceData["srv_n"] = " ";
                    if (!dataReader.IsDBNull(4))
                        serviceData["srv_d"] = dataReader.GetString(4);
                    else
                        serviceData["srv_d"] = " ";
                    if (!dataReader.IsDBNull(5))
                        serviceData["srv_ico"] = dataReader.GetString(5);
                    else
                        serviceData["srv_ico"] = " ";
                    serviceData["srv_type"] = dataReader.GetInt32(6).ToString();
                    if (!dataReader.IsDBNull(7))
                        serviceData["srv_exp"] = dataReader.GetString(7);
                    serviceData["pa"] = dataReader.GetInt32(8).ToString();
                    if (!dataReader.IsDBNull(9))
                        serviceData["pnr_tp"] = dataReader.GetString(9);
                    else
                        serviceData["pnr_tp"] = " ";
                    if (!dataReader.IsDBNull(10))
                        serviceData["pnr_lnk"] = dataReader.GetString(10);
                    else
                        serviceData["pnr_lnk"] = " ";
                    nBonusIndex++;
                    serviceDataList["service" + nBonusIndex.ToString()] = serviceData;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetBonusServicesData::Exception", LoggerSeverities.Error);
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

        private int GetFreeSpaceTime(int nContractId = 0)
        {
            int iMinutes = -1;
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

                string strSQL = string.Format("SELECT NVL(PAR_VALUE,0) FROM PARAMETERS WHERE PAR_DESCSHORT = '{0}'", PARM_FREE_SPACE_TIME);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    iMinutes = Convert.ToInt32(dataReader.GetString(0));
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetFreeSpaceTime::Exception", LoggerSeverities.Error);
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

            return iMinutes;
        }

        private int GetMaxFreeSpaces(int nContractId = 0)
        {
            int iMaxSpaces = -1;
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

                string strSQL = string.Format("SELECT NVL(PAR_VALUE,0) FROM PARAMETERS WHERE PAR_DESCSHORT = '{0}'", PARM_MAX_FREE_SPACES);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    iMaxSpaces = Convert.ToInt32(dataReader.GetString(0));
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetMaxFreeSpaces::Exception", LoggerSeverities.Error);
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

            return iMaxSpaces;
        }

        private int GetMaxDistanceSpaces(int nContractId = 0)
        {
            int iMaxDist = -1;
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

                string strSQL = string.Format("SELECT NVL(PAR_VALUE,0) FROM PARAMETERS WHERE PAR_DESCSHORT = '{0}'", PARM_MAX_DIST_SPACES);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    iMaxDist = Convert.ToInt32(dataReader.GetString(0));
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetMaxDistanceSpaces::Exception", LoggerSeverities.Error);
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

            return iMaxDist;
        }

        private bool GetFreeSpaces(int iMinutes, int iDistance, string strLattitude, string strLongitude, int iMaxSpaces, out SortedList spacesList, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            spacesList = new SortedList();
            int iSpaceIndex = 0;

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

                // Get a list of all the bonus operations associated with the user
                ArrayList bonusList = new ArrayList();
                string strSQL = "";
                if (strLattitude.Length > 0 && strLongitude.Length > 0)
                {
                    // Use this formula to estimate distance (meters)
                    // (Abs(Lat2 - Lat1) + Abs(Lon2 - Lon1)) * 66667

                    strSQL = string.Format("SELECT PS_ID, TO_CHAR( PS_DATE_MOD, 'HH24missddMMYY'), PS_LATTITUDE, PS_LONGITUDE, PS_GRP_ID, ABS(ABS(PS_LATTITUDE) - ABS({0})) + ABS(ABS(PS_LONGITUDE) - ABS({1})) AS DISTANCE FROM PARKING_SPACES WHERE PS_STATE = {2} AND SYSDATE - PS_DATE_MOD <= {3} / 1440 AND (ABS(PS_LATTITUDE - {0}) + ABS(PS_LONGITUDE - {1})) * 6667 < {4} ORDER BY DISTANCE, PS_ID ",
                        strLattitude, strLongitude, ConfigurationManager.AppSettings["SpaceStatus.Free"].ToString(), iMinutes, iDistance);
                }
                else
                {
                    strSQL = string.Format("SELECT PS_ID, TO_CHAR( PS_DATE_MOD, 'HH24missddMMYY'), PS_LATTITUDE, PS_LONGITUDE, PS_GRP_ID FROM PARKING_SPACES WHERE PS_STATE = {0} AND SYSDATE - PS_DATE_MOD <= {1} / 1440 ORDER BY PS_LATTITUDE, PS_LONGITUDE, PS_ID ",
                        ConfigurationManager.AppSettings["SpaceStatus.Free"].ToString(), iMinutes);
                }
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    SortedList spaceData = new SortedList();
                    spaceData["spcid"] = dataReader.GetInt32(0).ToString();
                    spaceData["spc_t"] = dataReader.GetString(1);
                    spaceData["lt"] = dataReader.GetDouble(2).ToString();
                    spaceData["ln"] = dataReader.GetDouble(3).ToString();
                    if (Convert.ToInt32(ConfigurationManager.AppSettings["GlobalizeCommaGPS"]) == 1)
                    {
                        spaceData["lt"] = spaceData["lt"].ToString().Replace(",", ".");
                        spaceData["ln"] = spaceData["ln"].ToString().Replace(",", ".");
                    }
                    spaceData["zo"] = dataReader.GetInt32(4).ToString();
                    iSpaceIndex++;
                    if (iSpaceIndex < iMaxSpaces)
                        spacesList["spc" + iSpaceIndex.ToString()] = spaceData;
                    else
                        break;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetFreeSpaces::Exception", LoggerSeverities.Error);
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

        private bool AddServiceOperation(SortedList parametersOperation, int nContractId = 0)
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

                StringBuilder strSQL = new StringBuilder("INSERT INTO MOBILE_SERVICES_OPERATIONS (MOP_ID, MOP_MU_ID, MOP_MOS_ID, MOP_DATE, ");
                if (parametersOperation["code"].ToString().Length > 0)
                    strSQL.Append("MOP_BARCODE, ");
                if (parametersOperation["codetype"].ToString().Length > 0)
                    strSQL.Append("MOP_BARCODE_TYPE, ");
                if (parametersOperation["exp_t"] != null)
                {
                    if (parametersOperation["exp_t"].ToString().Length > 0)
                        strSQL.Append("MOP_EXP_DATE, ");
                }
                strSQL.Append("MOP_PAYMENT) VALUES (");
                strSQL.Append(parametersOperation["op"].ToString());
                strSQL.Append(", ");
                strSQL.Append(parametersOperation["mu_id"].ToString());
                strSQL.Append(", ");
                strSQL.Append(parametersOperation["mos_id"].ToString());
                strSQL.Append(", SYSDATE, ");
                if (parametersOperation["code"].ToString().Length > 0)
                {
                    strSQL.Append("'");
                    strSQL.Append(parametersOperation["code"].ToString());
                    strSQL.Append("', ");
                }
                if (parametersOperation["codetype"].ToString().Length > 0)
                {
                    strSQL.Append(parametersOperation["codetype"].ToString());
                    strSQL.Append(", ");
                }
                if (parametersOperation["exp_t"] != null)
                {
                    if (parametersOperation["exp_t"].ToString().Length > 0)
                    {
                        strSQL.Append("TO_DATE('");
                        strSQL.Append(parametersOperation["exp_t"].ToString());
                        strSQL.Append("', 'HH24MISSDDMMYY'), ");
                    }
                }
                strSQL.Append(parametersOperation["price"].ToString());
                strSQL.Append(")");

                oraCmd.CommandText = strSQL.ToString();

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("AddServiceOperation::Exception", LoggerSeverities.Error);
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

        private bool AddServiceOperation(SortedList parametersOperation, out long lOperId, int nContractId = 0)
        {
            bool bResult = false;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            lOperId = -1;

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

                string strSQL = string.Format("INSERT INTO MOBILE_SERVICES_OPERATIONS (MOP_MU_ID, MOP_MOS_ID, MOP_DATE, MOP_BARCODE, MOP_BARCODE_TYPE, MOP_PAYMENT) VALUES ({0}, {1}, SYSDATE, '{2}', {3}, {4})",
                    parametersOperation["mu_id"].ToString(), parametersOperation["mos_id"].ToString(), parametersOperation["code"].ToString(), parametersOperation["codetype"].ToString(), parametersOperation["price"].ToString());
                strSQL += " returning MOP_ID into :nReturnValue";
                oraCmd.CommandText = strSQL;

                oraCmd.Parameters.Add(new OracleParameter("nReturnValue", OracleDbType.Int64));
                oraCmd.Parameters["nReturnValue"].Direction = System.Data.ParameterDirection.ReturnValue;

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;

                lOperId = Convert.ToInt64(oraCmd.Parameters["nReturnValue"].Value.ToString());
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("AddServiceOperation::Exception", LoggerSeverities.Error);
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

        private bool UpdateServiceOperationStatus(long lServiceOperationId, int iStatus, int nContractId = 0)
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

                string strSQL = string.Format("update mobile_services_operations set mop_state = {0}, mop_date_used = sysdate where mop_id = {1}",
                iStatus, lServiceOperationId);

                oraCmd.CommandText = strSQL;

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("UpdateServiceOperationStatus::Exception", LoggerSeverities.Error);
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

        // OperationType 0 = Reset, 1 = Add, 2 = Subtract
        private bool UpdateUserSpaceNotifications(int iUserId, int iNumNotif, int nOperationType, int nContractId = 0)
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

                string strSQL = "";
                if (nOperationType == 1)
                    strSQL = string.Format("update mobile_users set mu_num_shared_spaces = mu_num_shared_spaces + {0} where mu_id = {1}", iNumNotif, iUserId);
                else if (nOperationType == 2)
                    strSQL = string.Format("update mobile_users set mu_num_shared_spaces = mu_num_shared_spaces - {0} where mu_id = {1}", iNumNotif, iUserId);
                else
                    strSQL = string.Format("update mobile_users set mu_num_shared_spaces = 0 where mu_id = {0}", iUserId);

                oraCmd.CommandText = strSQL;

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("UpdateUserSpaceNotifications::Exception", LoggerSeverities.Error);
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

        private bool OPSMessage(string strMessageIn, int iVirtualUnit, out string strMessageOut, int nContractId = 0)
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
                    string strIP = Context.Request.UserHostAddress;
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
        }

        private string GenerateCollectServiceMessage(SortedList parametersOut)
        {
            string xmlOut = "";
            string jsonOut = "";

            try
            {
                XmlDocument xmlOutDoc = new XmlDocument();

                string xmlTagName = ConfigurationManager.AppSettings["XML_PARTNER_TAG_NAME"].ToString();
                XmlElement root = xmlOutDoc.CreateElement(xmlTagName + IN_SUFIX);
                xmlOutDoc.AppendChild(root);
                XmlNode rootNode = xmlOutDoc.SelectSingleNode(xmlTagName + IN_SUFIX);

                foreach (DictionaryEntry item in parametersOut)
                {
                    XmlElement node = xmlOutDoc.CreateElement(item.Key.ToString());
                    node.InnerText = item.Value.ToString();
                    rootNode.AppendChild(node);
                }

                xmlOut = xmlOutDoc.OuterXml;
                jsonOut = JsonConvert.SerializeXmlNode(xmlOutDoc);

                Logger_AddLogMessage(string.Format("GenerateCollectServiceMessage: Message generated = {0}", jsonOut), LoggerSeverities.Info);
            }
            catch (Exception e)
            {
                jsonOut = "";
                Logger_AddLogMessage("GenerateCollectServiceMessage::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return jsonOut;
        }

        private bool CollectServiceMessage(string strMessageIn, out string strMessageOut)
        {
            bool bRdo = false;
            strMessageOut = null;

            try
            {
                // This service is not available for Mugipark
                // WSApparcaService.WSApparcaSoapClient wsCollectService = new WSApparcaService.WSApparcaSoapClient();
                //strMessageOut = wsCollectService.canjearServicio(strMessageIn);

                string strStatus = Context.Response.Status;

                if (strMessageOut != "")
                {
                    strMessageOut = strMessageOut.Replace("\r", "").Replace("\n", "");
                    Logger_AddLogMessage(string.Format("CollectServiceMessage: Message received = {0}", strMessageOut), LoggerSeverities.Info);
                    bRdo = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("CollectServiceMessage::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
                bRdo = false;
            }

            return bRdo;
        }

        private ResultType ProcessPartnerMessage(XmlDocument xmlInDoc, out SortedList parameters)
        {
            ResultType rtRes = ResultType.Result_Error_Generic;
            parameters = new SortedList();

            try
            {
                try
                {
                    string xmlTagName = ConfigurationManager.AppSettings["XML_PARTNER_TAG_NAME"].ToString();
                    XmlNodeList Nodes = xmlInDoc.SelectNodes("//" + xmlTagName + OUT_SUFIX + "/*");
                    foreach (XmlNode Node in Nodes)
                    {
                        switch (Node.Name)
                        {
                            case "ah":
                                break;
                            case "expiration_date":
                                parameters["exp_t"] = Node.InnerText.Trim();
                                break;
                            default:
                                parameters[Node.Name] = Node.InnerText.Trim();
                                break;
                        }
                    }

                    if (Nodes.Count == 0)
                    {
                        Logger_AddLogMessage(string.Format("ProcessPartnerMessage: Bad Input XML"), LoggerSeverities.Error);
                        rtRes = ResultType.Result_Error_Invalid_Input_Parameter;
                    }

                    if (!parameters.ContainsKey("r"))
                    {
                        Logger_AddLogMessage(string.Format("ProcessPartnerMessage: Bad Input XML"), LoggerSeverities.Error);
                        rtRes = ResultType.Result_Error_Missing_Input_Parameter;
                    }
                    else
                    {
                        switch (parameters["r"].ToString())
                        {
                            case "0":
                                rtRes = ResultType.Result_OK;
                                break;
                            case "-1":
                            case "-2":
                            case "-3":
                                rtRes = ResultType.Result_Error_Generic;
                                break;
                            case "-4":
                                rtRes = ResultType.Result_Error_Service_Expired;
                                break;
                            default:
                                rtRes = ResultType.Result_Error_Generic;
                                break;
                        }
                    }
                }
                catch
                {
                    Logger_AddLogMessage(string.Format("ProcessPartnerMessage: Bad Input XML"), LoggerSeverities.Error);
                    rtRes = ResultType.Result_Error_Invalid_Input_Parameter;
                }
            }
            catch (Exception e)
            {
                rtRes = ResultType.Result_Error_Generic;
                Logger_AddLogMessage("ProcessPartnerMessage::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return rtRes;
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

                    if (OPSMessage(strM5In, iVirtualUnit, out strM5Out, nContractId))
                    {
                        Logger_AddLogMessage(string.Format("SendM5::OPSMessageOut = {0}", strM5Out), LoggerSeverities.Info);
                        SortedList parametersM5Out = new SortedList();
                        ResultType rtM5Out = FindOPSMessageOutputParameters(strM5Out, out parametersM5Out);


                        if (rtM5Out == ResultType.Result_OK)
                        {
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

                        }
                        else
                        {
                            iRes = Convert.ToInt32(rtM5Out);
                            Logger_AddLogMessage(string.Format("SendM5::Error In MessageOut = {0}", strM5Out), LoggerSeverities.Error);
                        }



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

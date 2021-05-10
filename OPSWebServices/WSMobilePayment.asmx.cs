using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Security.Cryptography;
using System.Reflection;
using System.Collections;
using System.Configuration;
using OPS.Comm.Becs.Messages;
using OPS.Comm;
using OPS.Components.Data;
using Jot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Text;
using System.Net;
using Oracle.ManagedDataAccess.Client;
using System.Net.Mail;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OPSWebServices
{
    public class Loc
    {
        private double lt;
        private double lg;

        public double Lg
        {
            get { return lg; }
            set { lg = value; }
        }

        public double Lt
        {
            get { return lt; }
            set { lt = value; }
        }

        public Loc(double lt, double lg)
        {
            this.lt = lt;
            this.lg = lg;
        }
    }

    /// <summary>
    /// Summary description for WSMobilePayment
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WSMobilePayment : System.Web.Services.WebService
    {
        static ILogger _logger = null;

        static MessagesSession _msgSession = null;
        static string _MacTripleDesKey = null;
        static byte[] _normTripleDesKey = null;
        static MACTripleDES _mac3des = null;
        static string _xmlTagName = null;
        static int _webServiceFlag = 1;
        static string _useHash = null;
        private const long BIG_PRIME_NUMBER = 2147483647;
        private const string IN_SUFIX = "_in";
        private const string OUT_SUFIX = "_out";
        private const int UNPARKED = 1;
        private const int PARKED = 2;
        internal const string PARM_NUM_SPACES_BONUS = "P_NUM_SPACES_BONUS";
        internal const string PARM_SPACES_BONUS = "P_SPACES_BONUS";

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
            Result_Error_Invalid_Login = -23,
            Result_Error_ParkingStartedByDifferentUser = -24,
            Result_Error_Not_Enough_Credit = -25,
            Result_Error_Cloud_Id_Not_Found = -26,
            Result_Error_App_Update_Required = -27,
            Result_Error_No_Return_For_Minimum = -28,
            Result_Error_User_Not_Validated = -29,
            Result_Error_Location_Not_Found = -30
        }

        public static ILogger Logger
        {
            get { return _logger; }
        }

        public WSMobilePayment()
        {

            //Uncomment the following line if using designed components 
            //InitializeComponent(); 
            InitializeStatic();
        }

        #region Messages

        /*
         * The parameters of method QueryServerDateTime are:
            a.	Result: is also a string containing an xml with the result of the method:
                <prestoparking_out>
	                <r>Result of the method</r>
                    <t>Date in format hh24missddMMYY </t>
                </prestoparking_out>
            The tag <r> of the method will have these possible values:
                a.	1: Result of the query ok. Result date time is in the “t” tag.
                b.	-9: Generic Error (for example database or execution error.)
                c.	-12: OPS System error

         * 
         */
        [WebMethod]
        public string QueryServerDateTimeXML()
        {
            string xmlOut = "";
            try
            {
                SortedList parametersOut = null;

                Logger_AddLogMessage(string.Format("QueryServerDateTimeXML"), LoggerSeverities.Info);

                int iVirtualUnit = -1;
                if (GetFirstVirtualUnit(ref iVirtualUnit))
                {
                    if (iVirtualUnit < 0)
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                        Logger_AddLogMessage(string.Format("QueryServerDateTimeXML::Error: xmlOut={0}", xmlOut), LoggerSeverities.Error);
                        return xmlOut;
                    }

                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                    Logger_AddLogMessage(string.Format("QueryServerDateTimeXML::Error: xmlOut={0}", xmlOut), LoggerSeverities.Error);
                    return xmlOut;
                }



                Hashtable parametersOutMapping = new Hashtable();

                parametersOutMapping["t"] = "t";

                ResultType rt = SendM59(parametersOutMapping, iVirtualUnit, out parametersOut);

                if (rt == ResultType.Result_OK)
                {
                    parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                    xmlOut = GenerateXMLOuput(parametersOut);


                    if (xmlOut.Length == 0)
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                        Logger_AddLogMessage(string.Format("QueryServerDateTimeXML::Error: xmlOut={0}", xmlOut), LoggerSeverities.Error);
                    }
                    else
                    {
                        Logger_AddLogMessage(string.Format("QueryServerDateTimeXML: xmlOut= {0}", xmlOut), LoggerSeverities.Info);
                    }

                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryServerDateTimeXML::Error: xmlOut={0}", xmlOut), LoggerSeverities.Error);
                }

            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryServerDateTimeXML::Error: xmlOut={0}", xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return xmlOut;
        }

        /*
         * 
         * The parameters of method QueryParkingStatusXML are:
            a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <p>plate</p>
	                <d>date in format hh24missddMMYY</d> - *This parameter is optional
                    <mui>mobile user identifier (authorization token)</mui>
                    <ah>authentication hash</ah> - *This parameter is optional
	            </arinpark_in>

	            The authentication hash will be a string generated using the input parameters. Using this value we will detect the method call has been made by a well known client.

            b.	Result: is also a string containing an xml with the result of the method:
                <arinpark_out>
                     <rot>
	                    <r>Result of the method</r>
	                    <sta>status: 1 (UNPARKED), 2 (PARKED)</sta>
                        <ex>Extension is permitted in the current sector?: 1 (YES), -2 (NO YET), -3 (The plate has used the maximun amount of time/money in the sector)</ex> *
                        <id>tariff ID: 101 (ESRO non resident), 103 (ESRE non resident)</id>* 
                        <ad>Tariff type of current operation: 4 (ROTATION), 6 (VIPS) </ad> *
                        <o>Current Operation Type: 1: First parking: 2: extension</o> *
                        <di>Initial date of current operation (in format hh24missddMMYY) of the parking: the same as the input date if the operation is a first parking, or the date of the end of parking operations chain if the operation is an extension</di> *
                        <df>End date of current operation (in format hh24missddMMYY) of the parking. In order to show the user the end of the current parking operation.</df>
                        <aq>Amount of Euro Cents accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation</aq> *
                        <at> Amount of minutes accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation </at> *
                        <g>parking sector of current operation</g> *
                        <sectorname>Sector name</sectorname>
                        <sectorcolor>Sector color</sectorcolor>
                        <zone>Zone</zone>
                        <zonename>Zone name</zonename>
                        <zonecolor>Zone color</zonecolor>
                        <lt>Latitude of current operation</lt> *
                        <lg>Longitude of current operation</lg> *
                        <re>Reference of current operation. 128 characters maximum</re> *
                        <rfd>Refundable tariff: 0 (NO), 1 (YES) </rfd> *
                        <od>Operation date</od>
                        <streetname>name of street</streetname>
                        <streetno>street address number</streetno>
                    </rot>
                    <res>
	                    <r>Result of the method</r>
	                    <sta>status: 1 (UNPARKED), 2 (PARKED)</sta>
                        <ex>Extension is permitted in the current sector?: 1 (YES), -2 (NO YET), -3 (The plate has used the maximun amount of time/money in the sector)</ex> *
                        <id>tariff ID: 102 (ESRO resident), 104 (1 DAY TICKET), 105(5 DAY TICKET), 106 (20 DAY TICKET)</id>* 
                        <ad>Tariff type of current operation: 5 (RESIDENTS), 104 (1 DAY TICKET), 105(5 DAY TICKET), 106 (20 DAY TICKET) </ad> *
                        <o>Current Operation Type: 1: First parking: 2: extension</o> *
                        <di>Initial date of current operation (in format hh24missddMMYY) of the parking: the same as the input date if the operation is a first parking, or the date of the end of parking operations chain if the operation is an extension</di> *
                        <df>End date of current operation (in format hh24missddMMYY) of the parking. In order to show the user the end of the current parking operation.</df>
                        <aq>Amount of Euro Cents accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation</aq> *
                        <at> Amount of minutes accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation </at> *
                        <g>parking sector of current operation</g> *
                        <sectorname>Sector name</sectorname>
                        <sectorcolor>Sector color</sectorcolor>
                        <zone>Zone</zone>
                        <zonename>Zone name</zonename>
                        <zonecolor>Zone color</zonecolor>
                        <lt>Latitude of current operation</lt> *
                        <lg>Longitude of current operation</lg> *
                        <re>Reference of current operation. 128 characters maximum </re> *
                        <rfd>Refundable tariff: 0 (NO), 1 (YES) </rfd> *
                        <od>Operation date</od>
                        <streetname>name of street</streetname>
                        <streetno>street address number</streetno>
                    </res>
	                <avtar>
		                <tarid1>Tariff 1 ID</tarid1>
		                <tardesc1>Tariff 1 description</tardesc1>
		                <tarad1>Tariff type for Tariff 1</tarad1>
		                <tarrfd1>If Tariff 1 is refundable: 0 (NO), 1 (YES)</tarrfd1>
		                .
		                .
		                .
		                <taridn>Tariff n ID</taridn>
		                <tardescn>Tariff n description</tardescn>
		                <taradn>Tariff type for Tariff n</taradn>
		                <tarrfdn>If Tariff n is refundable: 0 (NO), 1 (YES)</tarrfdn>
	                </avtar>
            </arinpark_out>

         
          * Only in case of parked plates

            The tag <r> of the method will have these possible values:
            a.	1: Success and the restrictions come after this tag.
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error
            g.  -23: Invalid Login
            h.	-24: User has no rights. Operation begun by another user

         * 
         * 
         */
        [WebMethod]
        public string QueryParkingStatusXML(string xmlIn)
        {
            string xmlOut = "";
            SortedList parametersOutRot = null;
            SortedList parametersOutRes = null;
            SortedList parametersOutAvtar = null;

            try
            {
                SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";
                long lRotOperId = -1;
                long lResOperId = -1;
                int nRotExtension = -1;
                int nZoneId = -1;
                string strZoneName = "";
                string strSectorName = "";
                string strZoneColor = "673AB7";
                string strSectorColor = "673AB7";

                Logger_AddLogMessage(string.Format("QueryParkingStatusXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                parametersOutRot = new SortedList();
                parametersOutRes = new SortedList();
                parametersOutAvtar = new SortedList();

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["p"] == null) ||
                        (parametersIn["mui"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
                        xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "", parametersOutAvtar);
                        Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error - missing input parameter: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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
                            parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                            parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_Error_InvalidAuthenticationHash).ToString();
                            xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "", parametersOutAvtar);
                            Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error - incorrect hash: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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
                                parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "", parametersOutAvtar);
                                Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryParkingStatusXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_Error_Invalid_Login).ToString();
                                xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "", parametersOutAvtar);
                                Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");

                            // Find last operation group for rotation and VIP articles
                            string strArticlesFilter = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString() + ", " + ConfigurationManager.AppSettings["ArticleType.VipList"].ToString();
                            if (!GetLastParkingOperation(parametersIn["p"].ToString(), parametersIn["d"].ToString(), strArticlesFilter, ref lRotOperId, nContractId))
                            {
                                parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "", parametersOutAvtar);
                                Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error getting last rotation operation: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            Logger_AddLogMessage(string.Format("QueryParkingStatusXML::RotOperId={0}", lRotOperId), LoggerSeverities.Error);

                            if (lRotOperId < 0)
                            {
                                parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                                parametersOutRot["sta"] = UNPARKED.ToString();
                            }
                            else
                            {
                                // Obtain current parking operation info
                                if (GetOperStatusData(lRotOperId, out parametersOutRot, nContractId))
                                {
                                    // Check to see if previous parking operation was started by another user
                                    int nPrevMobileUserId = Convert.ToInt32(parametersOutRot["mui"]);
                                    if (nPrevMobileUserId != -1)
                                    {
                                        if (Convert.ToInt32(parametersIn["mui"]) != nPrevMobileUserId)
                                            rt = ResultType.Result_Error_ParkingStartedByDifferentUser;
                                    }
                                }
                                else
                                {
                                    rt = ResultType.Result_Error_Generic;
                                    Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error getting operation info: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                }

                                // Get virtual unit
                                int iVirtualUnit = -1;
                                if (rt == ResultType.Result_OK)
                                {
                                    if (GetVirtualUnit(Convert.ToInt32(parametersOutRot["g"]), ref iVirtualUnit, nContractId))
                                    {
                                        if (iVirtualUnit < 0)
                                        {
                                            rt = ResultType.Result_Error_Invalid_Input_Parameter;
                                            Logger_AddLogMessage(string.Format("QueryParkingStatusXML::No virtual unit found: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                        }
                                    }
                                    else
                                    {
                                        rt = ResultType.Result_Error_Invalid_Input_Parameter;
                                        Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error getting virtual unit: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    }
                                }

                                // Send M1 for rotation
                                if (rt == ResultType.Result_OK)
                                {
                                    parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString();
                                    parametersIn["ad"] = parametersOutRot["ad"].ToString();
                                    parametersIn["cdl"] = "1"; //compute date limits (and time)
                                    parametersIn["u"] = iVirtualUnit.ToString();
                                    parametersIn["pt"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                                    parametersIn["dll"] = ConfigurationManager.AppSettings["M1RegParamsPath" + nContractId.ToString()].ToString();

                                    Hashtable parametersInMapping = new Hashtable();

                                    parametersInMapping["p"] = "m";
                                    parametersInMapping["d"] = "d";
                                    parametersInMapping["g"] = "g";
                                    parametersInMapping["o"] = "o";
                                    parametersInMapping["ad"] = "ad";
                                    parametersInMapping["cdl"] = "cdl";
                                    parametersInMapping["u"] = "u";
                                    parametersInMapping["pt"] = "pt";
                                    parametersInMapping["dll"] = "dll";

                                    Hashtable parametersOutMapping = new Hashtable();

                                    parametersOutMapping["Aad"] = "ad";
                                    parametersOutMapping["Aq1"] = "q1";
                                    parametersOutMapping["Aq2"] = "q2";
                                    parametersOutMapping["At1"] = "t1";
                                    parametersOutMapping["At2"] = "t2";
                                    parametersOutMapping["Ad1"] = "d1";
                                    parametersOutMapping["Ad2"] = "d2";
                                    parametersOutMapping["Ao"] = "o";
                                    parametersOutMapping["Adr0"] = "di";
                                    parametersOutMapping["Araq"] = "aq";
                                    parametersOutMapping["Arat"] = "at";
                                    parametersOutMapping["Ar"] = "r";

                                    SortedList parametersOutM1 = new SortedList();
                                    rt = SendM1(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOutM1, nContractId);

                                    if (rt == ResultType.Result_OK)
                                    {
                                        // Join the M1 data with the operation data
                                        parametersOutRot.Remove("mui");
                                        parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                                        parametersOutRot["sta"] = PARKED.ToString();
                                        parametersOutRot["ex"] = Convert.ToInt32(rt).ToString();
                                        nRotExtension = Convert.ToInt32(rt);
                                        parametersOutRot["aq"] = parametersOutM1["aq"];
                                        parametersOutRot["at"] = parametersOutM1["at"];
                                    }
                                    else if (rt == ResultType.Result_Error_MaxTimeAlreadyUsedInPark || rt == ResultType.Result_Error_ReentryTimeError)
                                    {
                                        // Join the M1 data with the operation data
                                        parametersOutRot.Remove("mui");
                                        parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                                        parametersOutRot["sta"] = PARKED.ToString();
                                        // If there is no extension, then the result is always -3
                                        parametersOutRot["ex"] = "-3";
                                        nRotExtension = Convert.ToInt32(rt);
                                        rt = ResultType.Result_OK;
                                    }

                                    if (parametersOutRot["g"] != null)
                                    {
                                        if (parametersOutRot["g"].ToString().Trim().Length > 0)
                                        {
                                            GetGroupName(Convert.ToInt32(parametersOutRot["g"].ToString()), out strSectorName, out strSectorColor, nContractId);
                                            if (strSectorName.Length > 0)
                                                parametersOutRot["sectorname"] = strSectorName;
                                            if (strSectorColor.Length > 0)
                                                parametersOutRot["sectorcolor"] = strSectorColor;

                                            if (GetGroupParent(Convert.ToInt32(parametersOutRot["g"].ToString()), ref nZoneId, nContractId))
                                            {
                                                if (nZoneId > 0)
                                                {
                                                    parametersOutRot["zone"] = nZoneId.ToString();
                                                    GetGroupName(nZoneId, out strZoneName, out strZoneColor, nContractId);
                                                    if (strZoneName.Length > 0)
                                                        parametersOutRot["zonename"] = strZoneName;
                                                    if (strZoneColor.Length > 0)
                                                        parametersOutRot["zonecolor"] = strZoneColor;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (rt != ResultType.Result_OK)
                                {
                                    if (parametersOutRot == null)
                                        parametersOutRot = new SortedList();
                                    else
                                        parametersOutRot.Clear();
                                    parametersOutRot["r"] = Convert.ToInt32(rt).ToString();
                                    xmlOut = GenerateXMLErrorResult(rt);
                                    Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error - M1 returned error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                }
                            }

                            // Find last operation group for resident articles
                            strArticlesFilter = ConfigurationManager.AppSettings["ArticleType.ResList"].ToString();
                            if (!GetLastParkingOperation(parametersIn["p"].ToString(), parametersIn["d"].ToString(), strArticlesFilter, ref lResOperId, nContractId))
                            {
                                parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "", parametersOutAvtar);
                                Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error getting last resident operation: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            Logger_AddLogMessage(string.Format("QueryParkingStatusXML::ResOperId={0}", lResOperId), LoggerSeverities.Error);

                            if (lResOperId < 0)
                            {
                                parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                                parametersOutRes["sta"] = UNPARKED.ToString();
                            }
                            else
                            {
                                // Check to see if previous parking operation was started by another user
                                if (GetOperStatusData(lResOperId, out parametersOutRes, nContractId))
                                {
                                    // Check to see if previous parking operation was started by another user
                                    int nPrevMobileUserId = Convert.ToInt32(parametersOutRes["mui"]);
                                    if (nPrevMobileUserId != -1)
                                    {
                                        if (Convert.ToInt32(parametersIn["mui"]) != nPrevMobileUserId)
                                            rt = ResultType.Result_Error_ParkingStartedByDifferentUser;
                                    }
                                }
                                else
                                    rt = ResultType.Result_Error_Generic;

                                // Get virtual unit
                                int iVirtualUnit = -1;
                                if (rt == ResultType.Result_OK)
                                {
                                    if (GetVirtualUnit(Convert.ToInt32(parametersOutRes["g"]), ref iVirtualUnit, nContractId))
                                    {
                                        if (iVirtualUnit < 0)
                                        {
                                            rt = ResultType.Result_Error_Invalid_Input_Parameter;
                                            Logger_AddLogMessage(string.Format("QueryParkingStatusXML::No virtual unit found: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                        }
                                    }
                                    else
                                    {
                                        rt = ResultType.Result_Error_Invalid_Input_Parameter;
                                        Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error getting virtual unit: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    }
                                }

                                // Send M1 for resident
                                if (rt == ResultType.Result_OK)
                                {
                                    parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString();
                                    parametersIn["ad"] = parametersOutRes["ad"].ToString();
                                    parametersIn["cdl"] = "1"; //compute date limits (and time)
                                    parametersIn["u"] = iVirtualUnit.ToString();
                                    parametersIn["pt"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                                    parametersIn["dll"] = ConfigurationManager.AppSettings["M1RegParamsPath" + nContractId.ToString()].ToString();

                                    Hashtable parametersInMapping = new Hashtable();

                                    parametersInMapping["p"] = "m";
                                    parametersInMapping["d"] = "d";
                                    parametersInMapping["g"] = "g";
                                    parametersInMapping["o"] = "o";
                                    parametersInMapping["ad"] = "ad";
                                    parametersInMapping["cdl"] = "cdl";
                                    parametersInMapping["u"] = "u";
                                    parametersInMapping["pt"] = "pt";
                                    parametersInMapping["dll"] = "dll";

                                    Hashtable parametersOutMapping = new Hashtable();

                                    parametersOutMapping["Aad"] = "ad";
                                    parametersOutMapping["Aq1"] = "q1";
                                    parametersOutMapping["Aq2"] = "q2";
                                    parametersOutMapping["At1"] = "t1";
                                    parametersOutMapping["At2"] = "t2";
                                    parametersOutMapping["Ad1"] = "d1";
                                    parametersOutMapping["Ad2"] = "d2";
                                    parametersOutMapping["Ao"] = "o";
                                    parametersOutMapping["Adr0"] = "di";
                                    parametersOutMapping["Araq"] = "aq";
                                    parametersOutMapping["Arat"] = "at";
                                    parametersOutMapping["Ar"] = "r";

                                    SortedList parametersOutM1 = new SortedList();
                                    rt = SendM1(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOutM1, nContractId);

                                    if (rt == ResultType.Result_OK)
                                    {
                                        // Join the M1 data with the operation data
                                        parametersOutRes.Remove("mui");
                                        parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                                        parametersOutRes["sta"] = PARKED.ToString();
                                        parametersOutRes["ex"] = Convert.ToInt32(rt).ToString();
                                        parametersOutRes["aq"] = parametersOutM1["aq"];
                                        parametersOutRes["at"] = parametersOutM1["at"];
                                    }
                                    else if (rt == ResultType.Result_Error_MaxTimeAlreadyUsedInPark || rt == ResultType.Result_Error_ReentryTimeError)
                                    {
                                        // Join the M1 data with the operation data
                                        parametersOutRes.Remove("mui");
                                        parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                                        parametersOutRes["sta"] = PARKED.ToString();
                                        // If there is no extension, then the result is always -3
                                        parametersOutRes["ex"] = "-3";
                                        rt = ResultType.Result_OK;
                                    }

                                    if (parametersOutRes["g"] != null)
                                    {
                                        if (parametersOutRes["g"].ToString().Trim().Length > 0)
                                        {
                                            GetGroupName(Convert.ToInt32(parametersOutRes["g"].ToString()), out strSectorName, out strSectorColor, nContractId);
                                            if (strSectorName.Length > 0)
                                                parametersOutRes["sectorname"] = strSectorName;
                                            if (strSectorColor.Length > 0)
                                                parametersOutRes["sectorcolor"] = strSectorColor;

                                            if (GetGroupParent(Convert.ToInt32(parametersOutRes["g"].ToString()), ref nZoneId, nContractId))
                                            {
                                                if (nZoneId > 0)
                                                {
                                                    parametersOutRes["zone"] = nZoneId.ToString();
                                                    GetGroupName(nZoneId, out strZoneName, out strZoneColor, nContractId);
                                                    if (strZoneName.Length > 0)
                                                        parametersOutRes["zonename"] = strZoneName;
                                                    if (strZoneColor.Length > 0)
                                                        parametersOutRes["zonecolor"] = strZoneColor;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (rt != ResultType.Result_OK)
                                {
                                    if (parametersOutRes == null)
                                        parametersOutRes = new SortedList();
                                    else
                                        parametersOutRes.Clear();
                                    parametersOutRes["r"] = Convert.ToInt32(rt).ToString();
                                    xmlOut = GenerateXMLErrorResult(rt);
                                    Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error - M1 returned error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                }
                            }

                            // Determine the available tariff types to offer the user
                            if (Convert.ToInt32(parametersOutRot["r"]) > 0 && Convert.ToInt32(parametersOutRes["r"]) > 0)
                            {
                                bool bIsResident = (IsResident(parametersIn["p"].ToString(), nContractId) > 0);
                                bool bIsVip = (IsVip(parametersIn["p"].ToString(), nContractId) > 0);
                                int nRotGroup = -1;
                                int nResGroup = -1;
                                int nCurGroup = -1;
                                if (parametersOutRot["g"] != null)
                                    nRotGroup = Convert.ToInt32(parametersOutRot["g"]);
                                if (parametersOutRes["g"] != null)
                                    nResGroup = Convert.ToInt32(parametersOutRes["g"]);
                                if (parametersIn["g"] != null)
                                    nCurGroup = Convert.ToInt32(parametersIn["g"]);
                                if (!DetermineAvailableTariffs(nRotGroup, nResGroup, nRotExtension, nCurGroup, bIsResident, bIsVip, ref parametersOutAvtar, nContractId))
                                {
                                    if (parametersOutRot == null)
                                        parametersOutRot = new SortedList();
                                    else
                                        parametersOutRot.Clear();
                                    if (parametersOutRes == null)
                                        parametersOutRes = new SortedList();
                                    else
                                        parametersOutRes.Clear();
                                    parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "", parametersOutAvtar);
                                    Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error getting available tariffs: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                }
                                else
                                {
                                    xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "avtar", parametersOutAvtar);

                                    if (xmlOut.Length == 0)
                                    {
                                        if (parametersOutRot == null)
                                            parametersOutRot = new SortedList();
                                        else
                                            parametersOutRot.Clear();
                                        if (parametersOutRes == null)
                                            parametersOutRes = new SortedList();
                                        else
                                            parametersOutRes.Clear();
                                        parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                        parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                        xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "", parametersOutAvtar);
                                        Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error generating XML output: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    }
                                    else
                                    {
                                        Logger_AddLogMessage(string.Format("QueryParkingStatusXML: xmlOut= {0}", xmlOut), LoggerSeverities.Info);
                                    }
                                }
                            }
                            else
                            {
                                xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "", parametersOutAvtar);
                                Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            }
                        }
                    }
                }
                else
                {
                    parametersOutRot["r"] = Convert.ToInt32(rt).ToString();
                    parametersOutRes["r"] = Convert.ToInt32(rt).ToString();
                    xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "", parametersOutAvtar);
                    Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error parsing input parameters: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                if (parametersOutRot == null)
                    parametersOutRot = new SortedList();
                if (parametersOutRes == null)
                    parametersOutRes = new SortedList();
                parametersOutRot["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                parametersOutRes["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                xmlOut = GenerateXMLOuput("rot", parametersOutRot, "res", parametersOutRes, "", parametersOutAvtar);
                Logger_AddLogMessage(string.Format("QueryParkingStatusXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }


        /*
         * 
         * The parameters of method QueryParkingOperationXML are:
            a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <p>plate</p>
	                <d>date in format hh24missddMMYY</d> - *This parameter is optional
                    <g>parking sector identifier. We have to agree numbering different sectors and zones</g>
                    <adc>article code: 104 (1 day resident), 105 (5 day resident), 106 (20 day resident) </adc> - This paramater is optional and is only necessary when specifying the article
                    <ah>authentication hash</ah> - *This parameter is optional
	            </arinpark_in>

	            The authentication hash will be a string generated using the input parameters. Using this value we will detect the method call has been made by a well known client.

            b.	Result: is also a string containing an xml with the result of the method:
            <arinpark_out>
	            <r>Result of the method</r>
                <ad>tariff type to apply: For example: 4 (ROTATION), 5 (RESIDENTS), 6 VIPS</ad>
                <q1>minimum amount to pay in Euro cents</q1>
                <q2>maximum amount to pay in Euro cents</q2>
                <t1>minimum amount of time to park in minutes</q1>
                <t2> minimum amount of time to park in minutes </q2>
                <d1>minimum date</d1>
                <d2>maximum date</d2> 
                <o>Operation Type: 1: First parking: 2: extension</o>
                <di>Initial date (in format hh24missddMMYY) of the parking: the same as the input date if the operation is a first parking, or the date of the end of parking operations chain if the operation is an extension</di>
                <aq>Amount of Euro Cents accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation</aq>
                <at> Amount of minutes accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation </at>
            </arinpark_out>

            The tag <r> of the method will have these possible values:
            a.	1: Parking of extension is possible and the restrictions come after this tag.
            b.	-1: Invalid authentication hash
            c.	-2: The plate has used the maximum amount of time/money in the sector, so the extension is not possible. 
            d.	-3: The plate has not waited enough to return to the current sector.
            e.	-9: Generic Error (for example database or execution error.)
            f.	-10: Invalid input parameter
            g.	-11: Missing input parameter
            h.	-12: OPS System error
         

         * 
         * 
         */
        [WebMethod]
        public string QueryParkingOperationXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryParkingOperationXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null) ||
                        (parametersIn["g"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

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
                            Logger_AddLogMessage(string.Format("QueryParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            int iVirtualUnit = -1;
                            if (GetVirtualUnit(Convert.ToInt32(parametersIn["g"]), ref iVirtualUnit, nContractId))
                            {
                                if (iVirtualUnit < 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }

                            }
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");
                            parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString();
                            if (parametersIn["adc"] == null || parametersIn["adc"].ToString().Length == 0)
                                parametersIn["ad"] = ConfigurationManager.AppSettings["ArticleType.Rotacion"].ToString();
                            else
                                parametersIn["ad"] = parametersIn["adc"];
                            parametersIn["cdl"] = "1"; //compute date limits (and time)
                            parametersIn["u"] = iVirtualUnit.ToString();
                            parametersIn["pt"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                            parametersIn["dll"] = ConfigurationManager.AppSettings["M1RegParamsPath" + nContractId.ToString()].ToString();

                            string strOriginalArticleId = parametersIn["ad"].ToString();
                            if (Convert.ToInt32(ConfigurationManager.AppSettings["MapArticles"]) == 1)
                            {
                                string[] articlesList = ConfigurationManager.AppSettings["ArticlesMap"].Split(',');
                                if (articlesList.Contains(strOriginalArticleId))
                                {
                                    int nMaxTime = -1;
                                    if (GetArticleMaxTime(Convert.ToInt32(strOriginalArticleId), Convert.ToInt32(parametersIn["g"]), ref nMaxTime, nContractId))
                                    {
                                        parametersIn["t"] = nMaxTime.ToString();

                                        int nEquivArticleId = -1;
                                        if (GetEquivalentArticle(Convert.ToInt32(strOriginalArticleId), ref nEquivArticleId, nContractId))
                                        {
                                            parametersIn["ad"] = nEquivArticleId.ToString();
                                            parametersIn["mineqmax"] = "1";

                                            Logger_AddLogMessage(string.Format("QueryParkingOperationXML: Substituted article {0} for article {1}", strOriginalArticleId, nEquivArticleId.ToString()), LoggerSeverities.Info);
                                        }
                                        else
                                        {
                                            xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                            Logger_AddLogMessage(string.Format("QueryParkingOperationXML::Error - Could not obtain equivalent article: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                            return xmlOut;
                                        }
                                    }
                                    else
                                    {
                                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                        Logger_AddLogMessage(string.Format("QueryParkingOperationXML::Error - Could not obtain max time for article: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                        return xmlOut;
                                    }
                                }
                            }

                            Hashtable parametersInMapping = new Hashtable();

                            parametersInMapping["p"] = "m";
                            parametersInMapping["d"] = "d";
                            parametersInMapping["g"] = "g";
                            parametersInMapping["o"] = "o";
                            parametersInMapping["ad"] = "ad";
                            parametersInMapping["cdl"] = "cdl";
                            parametersInMapping["u"] = "u";
                            parametersInMapping["pt"] = "pt";
                            if (parametersIn["t"] != null)
                                parametersInMapping["t"] = "t";
                            if (parametersIn["mineqmax"] != null)
                                parametersInMapping["mineqmax"] = "mineqmax";
                            parametersInMapping["dll"] = "dll";

                            Hashtable parametersOutMapping = new Hashtable();

                            parametersOutMapping["Aad"] = "ad";
                            parametersOutMapping["Aq1"] = "q1";
                            parametersOutMapping["Aq2"] = "q2";
                            parametersOutMapping["At1"] = "t1";
                            parametersOutMapping["At2"] = "t2";
                            parametersOutMapping["Ad1"] = "d1";
                            parametersOutMapping["Ad2"] = "d2";
                            parametersOutMapping["Ao"] = "o";
                            parametersOutMapping["Adr0"] = "di";
                            parametersOutMapping["Araq"] = "aq";
                            parametersOutMapping["Arat"] = "at";
                            parametersOutMapping["Ar"] = "r";

                            rt = SendM1(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOut, nContractId);

                            if (rt == ResultType.Result_OK)
                            {
                                if (Convert.ToInt32(ConfigurationManager.AppSettings["MapArticles"]) == 1)
                                    parametersOut["ad"] = strOriginalArticleId;

                                xmlOut = GenerateXMLOuput(parametersOut);

                                if (xmlOut.Length == 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                }
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationXML: xmlOut= {0}", xmlOut), LoggerSeverities.Info);
                                }

                            }
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(rt);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            }

                        }
                    }

                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

                }

            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return xmlOut;
        }






        /*
         * 
         The parameters of method QueryParkingOperationWithTimeStepsXML are:
         a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <p>plate</p>
	            <d>date in format hh24missddMMYY</d> - *This parameter is optional
                <g>parking sector</g>
                <ah>authentication hash</ah> - *This parameter is optional
	        </arinpark_in>
         
        b.	Result: is also a string containing an xml with the result of the method:
            <arinpark_out>
	            <r>Result of the method</r>
                <ad>tariff type to apply: For example: 4 (ROTATION), 5 (RESIDENTS), 6 VIPS</ad>
                <q1>minimum amount to pay in Euro cents</q1>
                <q2>maximum amount to pay in Euro cents</q2>
                <t1>minimum amount of time to park in minutes</q1>
                <t2> minimum amount of time to park in minutes </q2>
                <d1>minimum date</d1>
                <d2>maximum date</d2> 
                <o>Operation Type: 1: First parking: 2: extension</o>
                <di>Initial date (in format hh24missddMMYY) of the parking: the same as the input date if the operation is a first parking, or the date of the end of parking operations chain if the operation is an extension</di>
                <aq>Amount of Euro Cents accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation</aq>
                <at> Amount of minutes accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation </at>
                <lst>Tariff steps from <t1> to <t2> every a parameterized number of minutes (for example 5 minutes)
	                <st>Step 1
		                <t>Time in minutes. Call it T1 .First Step will be <t1></t>
		                <q>Cost in Euro Cents for T1 minutes</q>
                        <d>datetime given by <q> cents </d>
	                </st>
	                <st>Step 2
		                <t> T1+5 minutes. Call it T2</t>
                        <q>Cost in Euro Cents for T2 minutes</q>
                        <d>datetime given by <q> cents </d>		                		                            
	                </st>
	                …
	                <st>Last Step (Step N)
                        <t>TN-1+5(or less in last step) minutes. Call it TN. Last Step will be<t2></t>
		                <q>Cost in Euro Cents for TN minutes</q>	
                        <d>datetime given by <q> cents </d>
	                </st>
                </lst>	
            </arinpark_out>

            The tag <r> of the method will have these possible values:
                a.	1: Parking of extension is possible and the restrictions come after this tag.
                b.	-1: Invalid authentication hash
                c.	-2: The plate has used the maximum amount of time/money in the sector, so the extension is not possible. In Bilbao this depends on the colour of the zone and the tariff type.
                d.	-3: The plate has not waited enough to return to the current sector.
                e.	-9: Generic Error (for example database or execution error.)
                f.	-10: Invalid input parameter
                g.	-11: Missing input parameter
                h.	-12: OPS System error
         * 
         */
        [WebMethod]
        public string QueryParkingOperationWithTimeStepsXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null) ||
                        (parametersIn["g"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

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
                            Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            int iVirtualUnit = -1;
                            if (GetVirtualUnit(Convert.ToInt32(parametersIn["g"]), ref iVirtualUnit, nContractId))
                            {
                                if (iVirtualUnit < 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }

                            }
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");
                            parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString();
                            parametersIn["ad"] = ConfigurationManager.AppSettings["ArticleType.Rotacion"].ToString();
                            parametersIn["cdl"] = "1"; //compute date limits (and time)
                            parametersIn["u"] = iVirtualUnit.ToString();
                            parametersIn["pt"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                            parametersIn["ctst"] = "1"; //Calcular steps de tiempo;
                            parametersIn["stv"] = ConfigurationManager.AppSettings["TIME_STEPS_OFFSET_IN_MINUTES"].ToString();  //Calcular steps de tiempo cada "5" minutos
                            parametersIn["dll"] = ConfigurationManager.AppSettings["M1RegParamsPath" + nContractId.ToString()].ToString();


                            Hashtable parametersInMapping = new Hashtable();

                            parametersInMapping["p"] = "m";
                            parametersInMapping["d"] = "d";
                            parametersInMapping["g"] = "g";
                            parametersInMapping["o"] = "o";
                            parametersInMapping["ad"] = "ad";
                            parametersInMapping["cdl"] = "cdl";
                            parametersInMapping["u"] = "u";
                            parametersInMapping["pt"] = "pt";
                            parametersInMapping["ctst"] = "ctst";
                            parametersInMapping["stv"] = "stv";
                            parametersInMapping["dll"] = "dll";

                            Hashtable parametersOutMapping = new Hashtable();

                            parametersOutMapping["Aad"] = "ad";
                            parametersOutMapping["Aq1"] = "q1";
                            parametersOutMapping["Aq2"] = "q2";
                            parametersOutMapping["At1"] = "t1";
                            parametersOutMapping["At2"] = "t2";
                            parametersOutMapping["Ad1"] = "d1";
                            parametersOutMapping["Ad2"] = "d2";
                            parametersOutMapping["Ao"] = "o";
                            parametersOutMapping["Adr0"] = "di";
                            parametersOutMapping["Araq"] = "aq";
                            parametersOutMapping["Arat"] = "at";
                            parametersOutMapping["Ar"] = "r";
                            parametersOutMapping["Acst"] = "cst";

                            rt = SendM1(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOut, nContractId);

                            if (rt == ResultType.Result_OK)
                            {
                                //// Check to see if time steps were sent already in M1
                                //if (parametersOut["cst"] == null)
                                //{
                                //    CalculateTimeSteps(ref parametersOut);
                                //}

                                xmlOut = GenerateXMLOuput(parametersOut);

                                if (xmlOut.Length == 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                }
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsXML: xmlOut= {0}", xmlOut), LoggerSeverities.Info);
                                }

                            }
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(rt);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            }

                        }
                    }

                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

                }

            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return xmlOut;
        }



        /*
         * 
         The parameters of method QueryParkingOperationWithMoneyStepsXML are:
         a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <p>plate</p>
	            <d>date in format hh24missddMMYY</d> - *This parameter is optional
                <g>parking sector</g>
                <ah>authentication hash</ah> - *This parameter is optional
	        </arinpark_in>
         
        b.	Result: is also a string containing an xml with the result of the method:
            <arinpark_out>
	            <r>Result of the method</r>
                <ad>tariff type to apply: For example: 4 (ROTATION), 5 (RESIDENTS), 6 VIPS</ad>
                <q1>minimum amount to pay in Euro cents</q1>
                <q2>maximum amount to pay in Euro cents</q2>
                <t1>minimum amount of time to park in minutes</q1>
                <t2> minimum amount of time to park in minutes </q2>
                <d1>minimum date</d1>
                <d2>maximum date</d2> 
                <o>Operation Type: 1: First parking: 2: extension</o>
                <di>Initial date (in format hh24missddMMYY) of the parking: the same as the input date if the operation is a first parking, or the date of the end of parking operations chain if the operation is an extension</di>
                <aq>Amount of Euro Cents accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation</aq>
                <at> Amount of minutes accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation </at>
                <lst>Tariff steps from <t1> to <t2> every a parameterized number of minutes (for example 5 minutes)
                        <st>Step 1
		                    <q>Amount in cents. Call it Q1 .First Step will be <q1></q>
		                    <t>amount of time given by <q> cents</t>
		                    <d>date time given by <q> cents</d>

	                    </st>
	                    <st>Step 2
		                    <q> Q1+5 cents. Call it Q2</t>
		                    <t>amount of time given by <q> cents</t>
		                    <d>date time given by <q> cents</d>	
	                    </st>
	                    …
	                    <st>Last Step (Step N)
                            <q>QN-1+5(or less in last step) cents. Call it QN. 
                                 Last Step will be<q2></q>
		                    <t>amount of time given by <q> cents</t>
		                    <d>date time given by <q> cents</d>
	                    </st>

                </lst>	
            </arinpark_out>

            The tag <r> of the method will have these possible values:
                a.	1: Parking of extension is possible and the restrictions come after this tag.
                b.	-1: Invalid authentication hash
                c.	-2: The plate has used the maximum amount of time/money in the sector, so the extension is not possible. In Bilbao this depends on the colour of the zone and the tariff type.
                d.	-3: The plate has not waited enough to return to the current sector.
                e.	-9: Generic Error (for example database or execution error.)
                f.	-10: Invalid input parameter
                g.	-11: Missing input parameter
                h.	-12: OPS System error
         * 
         */
        [WebMethod]
        public string QueryParkingOperationWithMoneyStepsXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null) ||
                        (parametersIn["g"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

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
                            Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            int iVirtualUnit = -1;
                            if (GetVirtualUnit(Convert.ToInt32(parametersIn["g"]), ref iVirtualUnit, nContractId))
                            {
                                if (iVirtualUnit < 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }

                            }
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");
                            parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString();
                            parametersIn["ad"] = ConfigurationManager.AppSettings["ArticleType.Rotacion"].ToString();
                            parametersIn["cdl"] = "1"; //compute date limits (and time)
                            parametersIn["u"] = iVirtualUnit.ToString();
                            parametersIn["pt"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                            parametersIn["cqst"] = "1"; //Calcular steps de dinero;
                            parametersIn["stv"] = ConfigurationManager.AppSettings["MONEY_STEPS_OFFSET_IN_CENTS"].ToString(); //Calcular steps de tiempo cada "5" minutos
                            parametersIn["dll"] = ConfigurationManager.AppSettings["M1RegParamsPath" + nContractId.ToString()].ToString();


                            Hashtable parametersInMapping = new Hashtable();

                            parametersInMapping["p"] = "m";
                            parametersInMapping["d"] = "d";
                            parametersInMapping["g"] = "g";
                            parametersInMapping["o"] = "o";
                            parametersInMapping["ad"] = "ad";
                            parametersInMapping["cdl"] = "cdl";
                            parametersInMapping["u"] = "u";
                            parametersInMapping["pt"] = "pt";
                            parametersInMapping["cqst"] = "cqst";
                            parametersInMapping["stv"] = "stv";
                            parametersInMapping["dll"] = "dll";

                            Hashtable parametersOutMapping = new Hashtable();

                            parametersOutMapping["Aad"] = "ad";
                            parametersOutMapping["Aq1"] = "q1";
                            parametersOutMapping["Aq2"] = "q2";
                            parametersOutMapping["At1"] = "t1";
                            parametersOutMapping["At2"] = "t2";
                            parametersOutMapping["Ad1"] = "d1";
                            parametersOutMapping["Ad2"] = "d2";
                            parametersOutMapping["Ao"] = "o";
                            parametersOutMapping["Adr0"] = "di";
                            parametersOutMapping["Araq"] = "aq";
                            parametersOutMapping["Arat"] = "at";
                            parametersOutMapping["Ar"] = "r";
                            parametersOutMapping["Acst"] = "cst";

                            rt = SendM1(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOut, nContractId);

                            if (rt == ResultType.Result_OK)
                            {
                                xmlOut = GenerateXMLOuput(parametersOut);

                                if (xmlOut.Length == 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                }
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsXML: xmlOut= {0}", xmlOut), LoggerSeverities.Info);
                                }

                            }
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(rt);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            }

                        }
                    }

                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

                }

            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return xmlOut;
        }





        /*
         * 
         * The parameters of method QueryParkingOperationForTimeXML are:
            a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <p>plate</p>
	            <d>date in format hh24missddMMYY</d> - *This parameter is optional
                <g>parking sector</g>
                <t>time in minutes</t>
                <ah>authentication hash</ah> - *This parameter is optional
	        </arinpark_in>
            b.	Result: is also a string containing an xml with the result of the method:
            <prestoparking_out>
	             <r>Result of the method</r>
                <ad>tariff type to apply: in Bilbao for example: 4 (ROTATION), 5 (RESIDENTS), 6 VIPS</ad>
                <q>amount needed to arrive to the input parameter t </q>
                <d>Final date of the parking</d>
                <o>Operation Type: 1: First parking: 2: extension</o>
                <di>Initial date (in format hh24missddMMYY) of the parking: the same as the input date if the operation is a first parking, or the date of the end of 
                        parking operations chain if the operation is an extension</di>
                <aq>Amount of Euro Cents accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation</aq>
                <at> Amount of minutes accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation </at>
            </prestoparking_out>

            The tag <r> of the method will have these possible values:
                a.	1: Parking of extension is possible and the restrictions come after this tag.
                b.	-1: Invalid authentication hash
                c.	-2: The plate has used the maximum amount of time/money in the sector, so the extension is not possible. In Bilbao this depends on the colour of the zone and the tariff type.
                d.	-3: The plate has not waited enough to return to the current sector.
                e.	-9: Generic Error (for example database or execution error.)
                f.	-10: Invalid input parameter
                g.	-11: Missing input parameter
                h.	-12: OPS System error


         * 
         */

        [WebMethod]
        public string QueryParkingOperationForTimeXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null) ||
                        (parametersIn["g"] == null) ||
                        (parametersIn["t"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

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
                            Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            int iVirtualUnit = -1;
                            if (GetVirtualUnit(Convert.ToInt32(parametersIn["g"]), ref iVirtualUnit, nContractId))
                            {
                                if (iVirtualUnit < 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }

                            }
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");
                            parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString();
                            parametersIn["ad"] = ConfigurationManager.AppSettings["ArticleType.Rotacion"].ToString();
                            parametersIn["u"] = iVirtualUnit.ToString();
                            parametersIn["pt"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                            parametersIn["dll"] = ConfigurationManager.AppSettings["M1RegParamsPath" + nContractId.ToString()].ToString();

                            Hashtable parametersInMapping = new Hashtable();

                            parametersInMapping["p"] = "m";
                            parametersInMapping["d"] = "d";
                            parametersInMapping["g"] = "g";
                            parametersInMapping["t"] = "t";
                            parametersInMapping["o"] = "o";
                            parametersInMapping["ad"] = "ad";
                            parametersInMapping["u"] = "u";
                            parametersInMapping["pt"] = "pt";
                            parametersInMapping["dll"] = "dll";

                            Hashtable parametersOutMapping = new Hashtable();

                            parametersOutMapping["Aad"] = "ad";
                            parametersOutMapping["Aq2"] = "q";
                            parametersOutMapping["Ad"] = "d";
                            parametersOutMapping["Ao"] = "o";
                            parametersOutMapping["Adr0"] = "di";
                            parametersOutMapping["Araq"] = "aq";
                            parametersOutMapping["Arat"] = "at";
                            parametersOutMapping["Ar"] = "r";

                            rt = SendM1(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOut, nContractId);

                            if (rt == ResultType.Result_OK)
                            {
                                xmlOut = GenerateXMLOuput(parametersOut);

                                if (xmlOut.Length == 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                }
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeXML: xmlOut= {0}", xmlOut), LoggerSeverities.Info);
                                }

                            }
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(rt);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            }

                        }
                    }

                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

                }

            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return xmlOut;
        }

        /*
         * 
         * The parameters of method QueryParkingOperationForMoneyXML are:
            a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <p>plate</p>
	            <d>date in format hh24missddMMYY</d> - *This parameter is optional
                <g>parking sector</g>
                <q>quantity in cents</q>                
                <ah>authentication hash</ah> - *This parameter is optional
	        </arinpark_in>
            b.	Result: is also a string containing an xml with the result of the method:
            <prestoparking_out>
	             <r>Result of the method</r>
                <ad>tariff type to apply: in Bilbao for example: 4 (ROTATION), 5 (RESIDENTS), 6 VIPS</ad>
                <t>time in minutes given by the amount or money q</t>
                <d>Final date of the parking</d>
                <o>Operation Type: 1: First parking: 2: extension</o>
                <di>Initial date (in format hh24missddMMYY) of the parking: the same as the input date if the operation is a first parking, or the date of the end of 
                        parking operations chain if the operation is an extension</di>
                <aq>Amount of Euro Cents accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation</aq>
                <at> Amount of minutes accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation </at>
            </prestoparking_out>

            The tag <r> of the method will have these possible values:
                a.	1: Parking of extension is possible and the restrictions come after this tag.
                b.	-1: Invalid authentication hash
                c.	-2: The plate has used the maximum amount of time/money in the sector, so the extension is not possible. In Bilbao this depends on the colour of the zone and the tariff type.
                d.	-3: The plate has not waited enough to return to the current sector.
                e.	-9: Generic Error (for example database or execution error.)
                f.	-10: Invalid input parameter
                g.	-11: Missing input parameter
                h.	-12: OPS System error


         * 
         */

        [WebMethod]
        public string QueryParkingOperationForMoneyXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null) ||
                        (parametersIn["g"] == null) ||
                        (parametersIn["q"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

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
                            Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            int iVirtualUnit = -1;
                            if (GetVirtualUnit(Convert.ToInt32(parametersIn["g"]), ref iVirtualUnit, nContractId))
                            {
                                if (iVirtualUnit < 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }

                            }
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                return xmlOut;
                            }

                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");
                            parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString();
                            parametersIn["ad"] = ConfigurationManager.AppSettings["ArticleType.Rotacion"].ToString();
                            parametersIn["u"] = iVirtualUnit.ToString();
                            parametersIn["pt"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                            parametersIn["dll"] = ConfigurationManager.AppSettings["M1RegParamsPath" + nContractId.ToString()].ToString();

                            Hashtable parametersInMapping = new Hashtable();

                            parametersInMapping["p"] = "m";
                            parametersInMapping["d"] = "d";
                            parametersInMapping["g"] = "g";
                            parametersInMapping["q"] = "q";
                            parametersInMapping["o"] = "o";
                            parametersInMapping["ad"] = "ad";
                            parametersInMapping["u"] = "u";
                            parametersInMapping["pt"] = "pt";
                            parametersInMapping["dll"] = "dll";

                            Hashtable parametersOutMapping = new Hashtable();

                            parametersOutMapping["Aad"] = "ad";
                            parametersOutMapping["At"] = "t";
                            parametersOutMapping["Ad"] = "d";
                            parametersOutMapping["Ao"] = "o";
                            parametersOutMapping["Adr0"] = "di";
                            parametersOutMapping["Araq"] = "aq";
                            parametersOutMapping["Arat"] = "at";
                            parametersOutMapping["Ar"] = "r";

                            rt = SendM1(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOut, nContractId);

                            if (rt == ResultType.Result_OK)
                            {
                                xmlOut = GenerateXMLOuput(parametersOut);

                                if (xmlOut.Length == 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                }
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyXML: xmlOut= {0}", xmlOut), LoggerSeverities.Info);
                                }

                            }
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(rt);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            }

                        }
                    }

                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

                }

            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return xmlOut;
        }





        /*
         *
         * The parameters of method ConfirmParkingOperationXML are:
            a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <p>plate</p>
                <g>parking sector</g>
	            <d>date in format hh24missddMMYY</d> - *This parameter is optional
                <q>Amount of money paid in Euro cents</q>
                <t>Time in minutes obtained paying <q> cents</t>
                <ad>tariff type applied: For example: 4 (ROTATION), 5 (RESIDENTS), 6 VIPS</ad>
                <mui>mobile user identifier (authorization token)</mui>
                <cid>Cloud ID. Used for cloud notifications</cid>
                <os>Operating system: 1 (Android), 2 (iOS)</os>
                <lt>Latitude of current operation</lt> - *This parameter is optional
                <lg>Longitude of current operation</lg> - *This parameter is optional
                <re>Reference of current operation</re> - *This parameter is optional
                <spcid>Space id.</spcid> *This parameter is optional
                <streetname>name of street</streetname> *This parameter is optional
                <streetno>street address number</streetno> *This parameter is optional
                <ah>authentication hash</ah> - *This parameter is optional
            </arinpark_in>

        b.	Result: is an integer with the next possible values:
            a.	1: Operation saved without errors
            b.	-1: Invalid authentication hash
            c.	-2: The plate has used the maximum amount of time/money in the sector, so the extension is not possible. In Bilbao this depends on the colour of the zone and the tariff type.
            d.	-3: The plate has not waited enough to return to the current sector.
            e.	-9: Generic Error (for example database or execution error.)
            f.	-10: Invalid input parameter
            g.	-11: Missing input parameter
            h.	-12: OPS System error
            i.	-13: Operation already inserted
            j.  -20: Mobile user not found
            k.  -23: Invalid Login
            l.	-24: User has no rights. Operation begun by another user
            m.  -25: User does not have enough credit


         * 
         */

        [WebMethod]
        public int ConfirmParkingOperationXML(string xmlIn)
        {
            int iRes = 0;
            try
            {
                SortedList parametersIn = null;
                SortedList parametersM1Out = null;
                SortedList parametersM2In = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null) ||
                        (parametersIn["g"] == null) ||
                        (parametersIn["ad"] == null) ||
                        (parametersIn["q"] == null) ||
                        (parametersIn["mui"] == null) ||
                        (parametersIn["cid"] == null) ||
                        (parametersIn["os"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);

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
                            Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
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
                                Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                            }
                            else
                                Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Check to see if user exists, and if so, if they have enough credit
                            // Send contract Id as 0 so that it uses the global users connection
                            int nCredit = 0;
                            if (GetMobileUserCredit(Convert.ToInt32(parametersIn["mui"].ToString()), ref nCredit, 0) != 1)
                            {
                                iRes = Convert.ToInt32(ResultType.Result_Error_Mobile_User_Not_Found);
                                Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                return iRes;
                            }
                            else
                            {
                                if (Convert.ToInt32(parametersIn["q"]) > nCredit)
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Not_Enough_Credit);
                                    Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                            }

                            // If the date is not provided, then whether the operation exists or not cannot be determined since the query is based in part on the date
                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");
                            else
                            {
                                bool bOpExists = false;
                                if (!OperationAlreadyExists(parametersIn["p"].ToString(), parametersIn["d"].ToString(), ref bOpExists, nContractId))
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                                else if (bOpExists)
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Operation_Already_Inserted);
                                    Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                            }

                            // Check to make sure that it is the same user that started the operation
                            int nPrevMobileUserId = -1;
                            if (GetLastOperMobileUser(parametersIn["p"].ToString(), Convert.ToInt32(parametersIn["ad"]), parametersIn["d"].ToString(), out nPrevMobileUserId, nContractId))
                            {
                                if (nPrevMobileUserId > 0 && nPrevMobileUserId != Convert.ToInt32(parametersIn["mui"]))
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_ParkingStartedByDifferentUser);
                                    Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                            }

                            int iVirtualUnit = -1;
                            if (GetVirtualUnit(Convert.ToInt32(parametersIn["g"]), ref iVirtualUnit, nContractId))
                            {
                                if (iVirtualUnit < 0)
                                {
                                    iRes = iRes = Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter);
                                    Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }

                            }
                            else
                            {
                                iRes = iRes = Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter);
                                Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                return iRes;
                            }

                            parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString();
                            parametersIn["u"] = iVirtualUnit.ToString();
                            parametersIn["pt"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono


                            Hashtable parametersM1InMapping = new Hashtable();

                            parametersM1InMapping["p"] = "m";
                            parametersM1InMapping["d"] = "d";
                            parametersM1InMapping["g"] = "g";
                            parametersM1InMapping["ad"] = "ad";
                            parametersM1InMapping["o"] = "o";
                            parametersM1InMapping["q"] = "q";
                            parametersM1InMapping["u"] = "u";
                            parametersM1InMapping["pt"] = "pt";



                            Hashtable parametersM1OutMapping = new Hashtable();

                            parametersM1OutMapping["Ad"] = "d";
                            parametersM1OutMapping["Ao"] = "o";
                            parametersM1OutMapping["Adi"] = "di";
                            parametersM1OutMapping["Ar"] = "r";
                            parametersM1OutMapping["Aad"] = "ad";
                            parametersM1OutMapping["At"] = "t";
                            parametersM1OutMapping["App"] = "pp";

                            ResultType rtM1 = SendM1(parametersIn, parametersM1InMapping, parametersM1OutMapping, iVirtualUnit, out parametersM1Out, nContractId);

                            iRes = Convert.ToInt32(rtM1);
                            if (rtM1 == ResultType.Result_OK)
                            {

                                Hashtable parametersM2InMapping = new Hashtable();
                                parametersM2InMapping["m"] = "m";
                                parametersM2InMapping["y"] = "y";
                                parametersM2InMapping["ad"] = "ad";
                                parametersM2InMapping["g"] = "g";
                                parametersM2InMapping["u"] = "u";
                                parametersM2InMapping["p"] = "p";
                                parametersM2InMapping["d"] = "d";
                                parametersM2InMapping["d1"] = "d1";
                                parametersM2InMapping["d2"] = "d2";
                                parametersM2InMapping["t"] = "t";
                                parametersM2InMapping["q"] = "q";
                                parametersM2InMapping["pp"] = "pp";
                                parametersM2InMapping["om"] = "om";
                                parametersM2InMapping["mui"] = "mui";
                                parametersM2InMapping["cid"] = "cid";
                                parametersM2InMapping["os"] = "os";
                                if (parametersIn["lt"] != null && !parametersIn["lt"].ToString().Equals("") && !parametersIn["lt"].ToString().Equals("undefined"))
                                    parametersM2InMapping["lt"] = "lt";
                                if (parametersIn["lg"] != null && !parametersIn["lg"].ToString().Equals("") && !parametersIn["lg"].ToString().Equals("undefined"))
                                    parametersM2InMapping["lg"] = "lg";
                                if (parametersIn["re"] != null)
                                    parametersM2InMapping["re"] = "ref";
                                if (parametersIn["spcid"] != null)
                                    parametersM2InMapping["spcid"] = "spcid";

                                parametersM2In = new SortedList();
                                parametersM2In["m"] = parametersIn["p"];
                                parametersM2In["y"] = parametersM1Out["o"];
                                parametersM2In["ad"] = parametersM1Out["ad"];
                                parametersM2In["g"] = parametersIn["g"];
                                parametersM2In["u"] = iVirtualUnit.ToString();
                                parametersM2In["p"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                                parametersM2In["d"] = parametersIn["d"];
                                parametersM2In["d1"] = parametersM1Out["di"];
                                parametersM2In["d2"] = parametersM1Out["d"];
                                parametersM2In["t"] = parametersM1Out["t"];
                                parametersM2In["q"] = parametersIn["q"];
                                parametersM2In["pp"] = (parametersM1Out["pp"] == null) ? "0" : parametersM1Out["pp"];
                                parametersM2In["om"] = "1"; //operation is always online
                                parametersM2In["mui"] = parametersIn["mui"];
                                parametersM2In["cid"] = parametersIn["cid"];
                                parametersM2In["os"] = parametersIn["os"];
                                if (parametersIn["lt"] != null && !parametersIn["lt"].ToString().Equals("") && !parametersIn["lt"].ToString().Equals("undefined"))
                                {
                                    if (Convert.ToInt32(ConfigurationManager.AppSettings["GlobalizeCommaGPS"]) == 1)
                                        parametersM2In["lt"] = parametersIn["lt"].ToString().Replace(",", ".");
                                    else
                                        parametersM2In["lt"] = parametersIn["lt"];
                                }
                                if (parametersIn["lg"] != null && !parametersIn["lg"].ToString().Equals("") && !parametersIn["lg"].ToString().Equals("undefined"))
                                {
                                    if (Convert.ToInt32(ConfigurationManager.AppSettings["GlobalizeCommaGPS"]) == 1)
                                        parametersM2In["lg"] = parametersIn["lg"].ToString().Replace(",", ".");
                                    else
                                        parametersM2In["lg"] = parametersIn["lg"];
                                }
                                if (parametersIn["re"] != null && !parametersIn["re"].ToString().Equals(""))
                                    parametersM2In["re"] = parametersIn["re"];
                                if (parametersIn["spcid"] != null && !parametersIn["spcid"].ToString().Equals(""))
                                    parametersM2In["spcid"] = parametersIn["spcid"];

                                ResultType rtM2 = SendM2(parametersM2In, parametersM2InMapping, iVirtualUnit, nContractId);
                                iRes = Convert.ToInt32(rtM2);
                                Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: M2 result for {0} - {1} = {2}", parametersIn["mui"].ToString(), parametersIn["p"].ToString(), iRes), LoggerSeverities.Info);

                                if (rtM2 == ResultType.Result_OK)
                                {
                                    bool bSpaceUpdate = false;
                                    if (ConfigurationManager.AppSettings["EnableSpaceBonuses"].ToString().Equals("true"))
                                        bSpaceUpdate = true;

                                    // Update space information
                                    if (bSpaceUpdate)
                                    {
                                        int iSpaceId = -1;
                                        if (parametersIn["spcid"] != null && !parametersIn["spcid"].ToString().Equals(""))
                                        {
                                            // Space already exists
                                            int iSpaceStatus = 0;
                                            iSpaceId = Convert.ToInt32(parametersIn["spcid"]);
                                            GetSpaceStatus(iSpaceId, ref iSpaceStatus, nContractId);
                                            if (iSpaceStatus == Convert.ToInt32(ConfigurationManager.AppSettings["SpaceStatus.Occupied"]))
                                                Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: Space {0} was already occupied for {1} - {2}", iSpaceId, parametersIn["mui"].ToString(), parametersIn["p"].ToString()), LoggerSeverities.Info);
                                            if (!UpdateSpaceStatus(Convert.ToInt32(parametersIn["spcid"]), Convert.ToInt32(ConfigurationManager.AppSettings["SpaceStatus.Occupied"]), nContractId))
                                                Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: Could not update parking space {0} status for {1} - {2}", iSpaceId, parametersIn["mui"].ToString(), parametersIn["p"].ToString()), LoggerSeverities.Error);
                                        }
                                        else
                                        {
                                            // Space not specified, but have GPS coordinates
                                            if (parametersIn["lt"] != null && parametersIn["lg"] != null && !parametersIn["lt"].ToString().Equals("")
                                                 && !parametersIn["lg"].ToString().Equals("") && !parametersIn["lt"].ToString().Equals("undefined")
                                                 && !parametersIn["lg"].ToString().Equals("undefined"))
                                            {
                                                // Make sure space doesn't already exist
                                                iSpaceId = DoesParkingSpaceExist(parametersIn["lt"].ToString(), parametersIn["lg"].ToString(), nContractId);

                                                if (iSpaceId > 0)
                                                {
                                                    // Space already exists, update it
                                                    int iSpaceStatus = 0;
                                                    GetSpaceStatus(iSpaceId, ref iSpaceStatus, nContractId);
                                                    if (iSpaceStatus == Convert.ToInt32(ConfigurationManager.AppSettings["SpaceStatus.Occupied"]))
                                                        Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: Space {0} was already occupied for {1} - {2}", iSpaceId, parametersIn["mui"].ToString(), parametersIn["p"].ToString()), LoggerSeverities.Info);
                                                    if (!UpdateSpaceStatus(iSpaceId, Convert.ToInt32(ConfigurationManager.AppSettings["SpaceStatus.Occupied"]), nContractId))
                                                        Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: Could not update parking space {0} status for {1} - {2}", iSpaceId, parametersIn["mui"].ToString(), parametersIn["p"].ToString()), LoggerSeverities.Error);
                                                    parametersIn["spcid"] = iSpaceId.ToString();
                                                }
                                                else
                                                {
                                                    // Have to create new space
                                                    iSpaceId = AddParkingSpace(parametersIn["g"].ToString(), parametersIn["lt"].ToString(), parametersIn["lg"].ToString(), Convert.ToInt32(ConfigurationManager.AppSettings["SpaceStatus.Occupied"]), nContractId);
                                                    if (iSpaceId > 0)
                                                    {
                                                        Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: Added the parking space {0} for {1} - {2}", iSpaceId, parametersIn["mui"].ToString(), parametersIn["p"].ToString()), LoggerSeverities.Info);
                                                        parametersIn["spcid"] = iSpaceId.ToString();
                                                    }
                                                    else
                                                        Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: Error adding a new parking space for {0} - {1}", parametersIn["mui"].ToString(), parametersIn["p"].ToString()), LoggerSeverities.Info);
                                                }
                                            }
                                        }

                                        // Update space notification bonus (only for first parking operation, not extensions)
                                        if (bSpaceUpdate && iSpaceId > 0 && parametersM1Out["o"].ToString().Equals(ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString()))
                                        {
                                            UpdateUserSpaceNotifications(Convert.ToInt32(parametersIn["mui"]), 1, nContractId);

                                            // Recharge bonuses are no longer given, users buy services using the points    
                                            //if (UpdateUserSpaceNotifications(Convert.ToInt32(parametersIn["mui"]), 1))
                                            //{
                                            //    // Determine if user is elegible for bonus
                                            //    int nNumSpacesForBonus = GetNumSpacesBonus();
                                            //    int nCurUserSpaces = GetUserNumSpaces(Convert.ToInt32(parametersIn["mui"]));
                                            //    if (nCurUserSpaces >= nNumSpacesForBonus)
                                            //    {
                                            //        int iBonusAmount = GetSpacesBonus();
                                            //        int iOperId = -1;
                                            //        if (AddBonusOperation(parametersIn["g"].ToString(), iVirtualUnit, iBonusAmount, parametersIn["mui"].ToString(), out iOperId))
                                            //        {
                                            //            Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: Added the bonus operation {0} for user {1}", iOperId, parametersIn["mui"].ToString()), LoggerSeverities.Info);
                                            //        }
                                            //        else
                                            //            Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: Error adding new bonus operation for user {0}", parametersIn["mui"].ToString()), LoggerSeverities.Info);

                                            //        if (iOperId > 0)
                                            //        {
                                            //            // Send email to user
                                            //            string strEmail = "";
                                            //            if (GetMobileUserEmail( Convert.ToInt32(parametersIn["mui"]), ref strEmail))
                                            //            {
                                            //                decimal dBonusAmount = Convert.ToDecimal(iBonusAmount) * (decimal)0.01;
                                            //                string strSubject = ConfigurationManager.AppSettings["EmailSubject"].ToString() + dBonusAmount.ToString();
                                            //                if (!SendEmail(strEmail, strSubject, ""))
                                            //                    Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: Error sending bonus email to user {0}", parametersIn["mui"].ToString()), LoggerSeverities.Info);
                                            //            }
                                            //        }

                                            //        // Reset the spaces notified by the user
                                            //        if (!UpdateUserSpaceNotifications(Convert.ToInt32(parametersIn["mui"]), 0))
                                            //            Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: Error resetting space notifications for user {0}", parametersIn["mui"].ToString()), LoggerSeverities.Info);
                                            //    }
                                            //}
                                        }
                                    }

                                    // Temp
                                    Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML: Updating the operation data for {0} - {1}", parametersIn["mui"].ToString(), parametersIn["p"].ToString()), LoggerSeverities.Info);
                                    long lOperId = -1;
                                    UpdateOperationData(parametersIn, out lOperId, nContractId);
                                    // Set Contract Id to 0 to force the global users connection
                                    UpdateOperationPlateData(parametersIn, lOperId, 0);
                                }
                            }
                            else
                            {
                                iRes = Convert.ToInt32(rtM1);
                                Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                            }
                        }
                    }
                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                }

            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("ConfirmParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return iRes;
        }

        /*
         * 
         * The parameters of method QueryUnParkingOperationXML are:
            a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
	            <p>plate</p>
	            <d>date in format hh24missddMMYY</d> - *This parameter is optional
                <ah>authentication hash</ah> - *This parameter is optional
	        </arinpark_in>
	            See the parking sector is not a parameter in this case because the unparking operation will be done with the last parking operation existing in the system.


            b.	Result: is also a string containing an xml with the result of the method:
            <arinpark_out>
	            <r>Result of the method</r>	
                <q>quantity in Euro Cents to be refunded</q>
                <d1>Initial date (in format hh24missddMMYY) for the parking operation chain (first parking, extensions and unparking operation) after unparking</d1>
                <d2>End date (in format hh24missddMMYY) for the parking operation chain (first parking, extensions and unparking operation) after unparking</d2>
                <t>Tariff time in minutes for the parking operation chain after unparking (d2-d1) </t>
                <ad>tariff type applied: in Bilbao for example: 4 (ROTATION), 5 (RESIDENTS), 6 VIPS</ad>
            </arinpark_out>

            The tag <r> of the method will have these possible values:
                a.	1: UnParking is possible and the restrictions come after this tag.
                b.	-1: Invalid authentication hash
                c.	-4: Plate has no rights for doing an unparking operation
                d.	-9: Generic Error (for example database or execution error.)
                e.	-10: Invalid input parameter
                f.	-11: Missing input parameter
                g.	-12: OPS System error


         * 
         * 
         */

        [WebMethod]
        public string QueryUnParkingOperationXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            // Find group of last valid rotation or VIP parking to determine the corresponding virtual unit
                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");
                            long lOperId = -1;
                            int iGroupId = -1;
                            int iArticle = -1;
                            int iVirtualUnit = -1;
                            string strArticlesFilter = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString() + ", " + ConfigurationManager.AppSettings["ArticleType.VipList"].ToString()
                                + ", " + ConfigurationManager.AppSettings["ArticleType.ResList"].ToString();
                            if (GetLastParkingOperation(parametersIn["p"].ToString(), parametersIn["d"].ToString(), strArticlesFilter, ref lOperId, nContractId))
                            {
                                if (lOperId > 0)
                                {
                                    if (GetOperationGroup(lOperId, ref iGroupId, nContractId))
                                    {
                                        if (iGroupId > 0)
                                            GetVirtualUnit(iGroupId, ref iVirtualUnit, nContractId);
                                        else
                                            Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error - Could not find group for last operation: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    }
                                    else
                                        Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error obtaining group for last operation: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

                                    GetOperationArticle(lOperId, ref iArticle, nContractId);
                                    if (iArticle < 0)
                                        Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error - Could not find article for last operation: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                }
                                else
                                    Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error - Could not find last operation: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error obtaining last operation: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);

                            if (iVirtualUnit < 0)
                            {
                                if (GetFirstVirtualUnit(ref iVirtualUnit, nContractId))
                                {
                                    if (iVirtualUnit < 0)
                                    {
                                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                        Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                        return xmlOut;
                                    }

                                }
                                else
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    return xmlOut;
                                }
                            }

                            if (iArticle < 0)
                                iArticle = Convert.ToInt32(ConfigurationManager.AppSettings["ArticleType.Rotacion"]);

                            parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Refund"].ToString();
                            parametersIn["ad"] = iArticle.ToString();
                            parametersIn["u"] = iVirtualUnit.ToString();
                            parametersIn["pt"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                            parametersIn["dll"] = ConfigurationManager.AppSettings["M1RegParamsPath" + nContractId.ToString()].ToString();

                            Hashtable parametersInMapping = new Hashtable();

                            parametersInMapping["p"] = "m";
                            parametersInMapping["d"] = "d";
                            parametersInMapping["u"] = "u";
                            parametersInMapping["o"] = "o";
                            parametersInMapping["ad"] = "ad";
                            parametersInMapping["pt"] = "pt";
                            parametersInMapping["dll"] = "dll";

                            Hashtable parametersOutMapping = new Hashtable();

                            parametersOutMapping["Aad"] = "ad";
                            parametersOutMapping["Adi"] = "d1";
                            parametersOutMapping["Ad"] = "d2";
                            parametersOutMapping["Aq"] = "q";
                            parametersOutMapping["At"] = "t";
                            parametersOutMapping["Ar"] = "r";

                            rt = SendM1(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOut, nContractId);

                            if (rt == ResultType.Result_OK)
                            {
                                xmlOut = GenerateXMLOuput(parametersOut);

                                if (xmlOut.Length == 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                }
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML: xmlOut= {0}", xmlOut), LoggerSeverities.Info);
                                }
                            }
                            //else if (rt == ResultType.Result_Error_Plate_Has_No_Return)
                            //{
                            //    // If plate does not have return rights, send a different error if it is because the user did a return, but still has the minimum parking time left
                            //    lOperId = -1;
                            //    GetLastOperation(parametersIn["p"].ToString(), parametersIn["d"].ToString(), strArticlesFilter, ref lOperId);
                            //    if (lOperId > 0)
                            //    {
                            //        int iType = -1;
                            //        if (GetOperationType(lOperId, ref iType))
                            //        {
                            //            if (iType != Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Refund"]))
                            //            {
                            //                parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString();
                            //                parametersOutMapping["Ao"] = "o";

                            //                rt = SendM1(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOut);

                            //                // If user is still parked
                            //                if ((rt == ResultType.Result_OK && parametersOut["o"].ToString() == ConfigurationManager.AppSettings["OperationsDef.Extension"].ToString())
                            //                    || rt == ResultType.Result_Error_MaxTimeAlreadyUsedInPark)
                            //                    rt = ResultType.Result_Error_No_Return_For_Minimum;
                            //                else
                            //                    rt = ResultType.Result_Error_Plate_Has_No_Return;

                            //                parametersOut.Remove("o");
                            //            }
                            //        }
                            //    }

                            //    xmlOut = GenerateXMLErrorResult(rt);
                            //    Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            //}
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(rt);
                                Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            }
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUnParkingOperationXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return xmlOut;
        }



        /*
         * 
         * The parameters of method QueryUnParkingOperationQuantityXML are:
            a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
	                <p>plate</p>
	                <d>date in format hh24missddMMYY</d> - *This parameter is optional
                    <ah>authentication hash</ah> - *This parameter is optional
	            </arinpark_in>
	        See the parking sector is not a parameter in this case because the unparking operation will be done with the last parking operation existing in the system.


            b.	Result: is an integer with these possible values:
                a.	>0: Quantity in Euro Cents to be refunded
                b.	-1: Invalid authentication hash
                c.	-4: Plate has no rights for doing an unparking operation
                d.	-9: Generic Error (for example database or execution error.)
                e.	-10: Invalid input parameter
                f.	-11: Missing input parameter
                g.	-12: OPS System error

         * 
         */

        [WebMethod]
        public int QueryUnParkingOperationQuantityXML(string xmlIn)
        {
            int iRes = 0;

            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);

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
                            Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
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

                            // Find group of last valid rotation or VIP parking to determine the corresponding virtual unit
                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");
                            long lOperId = -1;
                            int iGroupId = -1;
                            int iArticle = -1;
                            int iVirtualUnit = -1;
                            string strArticlesFilter = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString() + ", " + ConfigurationManager.AppSettings["ArticleType.VipList"].ToString()
                                + ", " + ConfigurationManager.AppSettings["ArticleType.ResList"].ToString();
                            if (GetLastParkingOperation(parametersIn["p"].ToString(), parametersIn["d"].ToString(), strArticlesFilter, ref lOperId, nContractId))
                            {
                                if (lOperId > 0)
                                {
                                    if (GetOperationGroup(lOperId, ref iGroupId, nContractId))
                                    {
                                        if (iGroupId > 0)
                                            GetVirtualUnit(iGroupId, ref iVirtualUnit, nContractId);
                                        else
                                            Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error - Could not find group for last operation: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    }
                                    else
                                        Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error obtaining group for last operation: xmlIn= {0}", xmlIn), LoggerSeverities.Error);

                                    GetOperationArticle(lOperId, ref iArticle, nContractId);
                                    if (iArticle < 0)
                                        Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error - Could not find article for last operation: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                }
                                else
                                    Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error - Could not find last operation: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error obtaining last operation: xmlIn= {0}", xmlIn), LoggerSeverities.Error);

                            if (iVirtualUnit < 0)
                            {
                                if (GetFirstVirtualUnit(ref iVirtualUnit, nContractId))
                                {
                                    if (iVirtualUnit < 0)
                                    {
                                        iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                        Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                                        return iRes;
                                    }
                                }
                                else
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                            }

                            if (iArticle < 0)
                                iArticle = Convert.ToInt32(ConfigurationManager.AppSettings["ArticleType.Rotacion"]);

                            parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Refund"].ToString();
                            parametersIn["ad"] = iArticle.ToString();
                            parametersIn["u"] = iVirtualUnit.ToString();
                            parametersIn["pt"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                            parametersIn["dll"] = ConfigurationManager.AppSettings["M1RegParamsPath" + nContractId.ToString()].ToString();

                            Hashtable parametersInMapping = new Hashtable();

                            parametersInMapping["p"] = "m";
                            parametersInMapping["d"] = "d";
                            parametersInMapping["u"] = "u";
                            parametersInMapping["o"] = "o";
                            parametersInMapping["ad"] = "ad";
                            parametersInMapping["pt"] = "pt";
                            parametersInMapping["dll"] = "dll";

                            Hashtable parametersOutMapping = new Hashtable();

                            parametersOutMapping["Aq"] = "q";
                            parametersOutMapping["Ar"] = "r";

                            rt = SendM1(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOut, nContractId);

                            if (rt == ResultType.Result_OK)
                            {
                                if (parametersOut["q"] == null)
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                                }
                                else
                                {
                                    iRes = Convert.ToInt32(parametersOut["q"].ToString());
                                    Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML: iRes= {0}", iRes), LoggerSeverities.Info);
                                }

                            }
                            else
                            {
                                iRes = Convert.ToInt32(rt);
                                Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                            }

                        }
                    }

                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);

                }

            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return iRes;
        }



        /*
         * 
         * The parameters of method ConfirmUnParkingOperationXML are:
            a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
                <p>plate</p>
	            <d>date in format hh24missddMMYY </d> - *This parameter is optional
                <q>Amount refunded in Euro Cents</q>
                <mui>mobile user identifier (authorization token)</mui>
                <cid>Cloud ID. Used for cloud notifications</cid>
                <os>Operating system: 1 (Android), 2 (iOS)</os>
                <ah>authentication hash</ah> - *This parameter is optional
            </arinpark_in>

           b.	Result: is an integer with the next possible values:
                a.	1: Operation saved without errors
                b.	-1: Invalid authentication hash
                c.	-4: Plate has no rights for doing an unparking operation
                d.	-9: Generic Error (for example database or execution error.)
                e.	-10: Invalid input parameter
                f.	-11: Missing input parameter
                g.	-12: OPS System error
                h.	-13: Operation already inserted
                i.	-14: Quantity received different as the quantity calculated previously
                j.  -20: Mobile user not found
                k.  -23: Invalid Login
                l.	-24: User has no rights. Operation begun by another user


         * 
         * 
         */

        [WebMethod]
        public int ConfirmUnParkingOperationXML(string xmlIn)
        {
            int iRes = 0;
            try
            {
                SortedList parametersIn = null;
                SortedList parametersM1Out = null;
                SortedList parametersM2In = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null) ||
                        (parametersIn["q"] == null) ||
                        (parametersIn["mui"] == null) ||
                        (parametersIn["cid"] == null) ||
                        (parametersIn["os"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);

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
                            Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
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
                                Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                            }
                            else
                                Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // If the date is not provided, then whether the operation exists or not cannot be determined since the query is based in part on the date
                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");
                            else
                            {
                                bool bOpExists = false;
                                if (!OperationAlreadyExists(parametersIn["p"].ToString(), parametersIn["d"].ToString(), ref bOpExists, nContractId))
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                                else if (bOpExists)
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Operation_Already_Inserted);
                                    Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                            }

                            // Find group of last valid rotation or VIP parking to determine the corresponding virtual unit
                            long lOperId = -1;
                            int iGroupId = -1;
                            int iArticle = -1;
                            int iVirtualUnit = -1;
                            string strArticlesFilter = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString() + ", " + ConfigurationManager.AppSettings["ArticleType.VipList"].ToString()
                                + ", " + ConfigurationManager.AppSettings["ArticleType.ResList"].ToString();
                            if (GetLastParkingOperation(parametersIn["p"].ToString(), parametersIn["d"].ToString(), strArticlesFilter, ref lOperId, nContractId))
                            {
                                if (lOperId > 0)
                                {
                                    if (GetOperationGroup(lOperId, ref iGroupId, nContractId))
                                    {
                                        if (iGroupId > 0)
                                            GetVirtualUnit(iGroupId, ref iVirtualUnit, nContractId);
                                        else
                                            Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error - Could not find group for last operation: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                    }
                                    else
                                        Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error obtaining group for last operation: xmlIn= {0}", xmlIn), LoggerSeverities.Error);

                                    GetOperationArticle(lOperId, ref iArticle, nContractId);
                                    if (iArticle < 0)
                                        Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error - Could not find article for last operation: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                }
                                else
                                    Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error - Could not find last operation: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                            }
                            else
                                Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error obtaining last operation: xmlIn= {0}", xmlIn), LoggerSeverities.Error);

                            if (iVirtualUnit < 0)
                            {
                                if (GetFirstVirtualUnit(ref iVirtualUnit, nContractId))
                                {
                                    if (iVirtualUnit < 0)
                                    {
                                        iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                        Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                                        return iRes;
                                    }
                                }
                                else
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                            }

                            if (iArticle < 0)
                                iArticle = Convert.ToInt32(ConfigurationManager.AppSettings["ArticleType.Rotacion"]);

                            // Check to make sure that it is the same user that started the operation
                            int nPrevMobileUserId = -1;
                            if (GetLastOperMobileUser(parametersIn["p"].ToString(), iArticle, parametersIn["d"].ToString(), out nPrevMobileUserId, nContractId))
                            {
                                if (nPrevMobileUserId > 0 && nPrevMobileUserId != Convert.ToInt32(parametersIn["mui"]))
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_ParkingStartedByDifferentUser);
                                    Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                            }

                            parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Refund"].ToString();
                            parametersIn["ad"] = iArticle.ToString();
                            parametersIn["u"] = iVirtualUnit.ToString();
                            parametersIn["pt"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono

                            Hashtable parametersM1InMapping = new Hashtable();

                            parametersM1InMapping["p"] = "m";
                            parametersM1InMapping["d"] = "d";
                            parametersM1InMapping["u"] = "u";
                            parametersM1InMapping["o"] = "o";
                            parametersM1InMapping["ad"] = "ad";
                            parametersM1InMapping["pt"] = "pt";

                            Hashtable parametersM1OutMapping = new Hashtable();

                            parametersM1OutMapping["Aad"] = "ad";
                            parametersM1OutMapping["Adi"] = "d1";
                            parametersM1OutMapping["Ad"] = "d2";
                            parametersM1OutMapping["Ao"] = "o";
                            parametersM1OutMapping["Aq"] = "q";
                            parametersM1OutMapping["At"] = "t";
                            parametersM1OutMapping["Ar"] = "r";
                            parametersM1OutMapping["Aaq"] = "qr";


                            ResultType rtM1 = SendM1(parametersIn, parametersM1InMapping, parametersM1OutMapping, iVirtualUnit, out parametersM1Out, nContractId);

                            iRes = Convert.ToInt32(rtM1);
                            if (rtM1 == ResultType.Result_OK)
                            {

                                if (parametersIn["q"].ToString() == parametersM1Out["q"].ToString())
                                {
                                    Hashtable parametersM2InMapping = new Hashtable();
                                    parametersM2InMapping["m"] = "m";
                                    parametersM2InMapping["y"] = "y";
                                    parametersM2InMapping["ad"] = "ad";
                                    parametersM2InMapping["u"] = "u";
                                    parametersM2InMapping["p"] = "p";
                                    parametersM2InMapping["d"] = "d";
                                    parametersM2InMapping["d1"] = "d1";
                                    parametersM2InMapping["d2"] = "d2";
                                    parametersM2InMapping["t"] = "t";
                                    parametersM2InMapping["q"] = "q";
                                    parametersM2InMapping["qr"] = "qr";
                                    parametersM2InMapping["om"] = "om";
                                    parametersM2InMapping["mui"] = "mui";
                                    parametersM2InMapping["cid"] = "cid";
                                    parametersM2InMapping["os"] = "os";

                                    parametersM2In = new SortedList();
                                    parametersM2In["m"] = parametersIn["p"];
                                    parametersM2In["y"] = parametersM1Out["o"];
                                    parametersM2In["ad"] = parametersM1Out["ad"];
                                    parametersM2In["u"] = iVirtualUnit.ToString();
                                    parametersM2In["p"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                                    parametersM2In["d"] = parametersIn["d"];
                                    parametersM2In["d1"] = parametersM1Out["d1"];
                                    parametersM2In["d2"] = parametersM1Out["d2"];
                                    parametersM2In["t"] = parametersM1Out["t"];
                                    parametersM2In["q"] = parametersIn["q"];
                                    parametersM2In["qr"] = parametersM1Out["qr"];
                                    parametersM2In["om"] = "1"; //operation is always online
                                    parametersM2In["mui"] = parametersIn["mui"];
                                    parametersM2In["cid"] = parametersIn["cid"];
                                    parametersM2In["os"] = parametersIn["os"];

                                    ResultType rtM2 = SendM2(parametersM2In, parametersM2InMapping, iVirtualUnit, nContractId);
                                    iRes = Convert.ToInt32(rtM2);
                                    Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML: iOut= {0}", iRes), LoggerSeverities.Info);

                                    bool bSpaceUpdate = false;
                                    if (ConfigurationManager.AppSettings["EnableSpaceBonuses"].ToString().Equals("true"))
                                        bSpaceUpdate = true;
                                    if (rtM2 == ResultType.Result_OK)
                                    {
                                        if (bSpaceUpdate)
                                        {
                                            long lSpaceId = -1;
                                            if (GetSpaceIdOperation(lOperId, ref lSpaceId, nContractId))
                                            {
                                                //UpdateSpaceStatus(lSpaceId, Convert.ToInt32(ConfigurationManager.AppSettings["SpaceStatus.Free"]), parametersM1Out["d2"].ToString());
                                                UpdateSpaceStatus(lSpaceId, Convert.ToInt32(ConfigurationManager.AppSettings["SpaceStatus.Free"]), nContractId);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    iRes = iRes = Convert.ToInt32(ResultType.Result_Error_Quantity_To_Pay_Different_As_Calculated);
                                    Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                }

                            }
                            //else if (rtM1 == ResultType.Result_Error_Plate_Has_No_Return)
                            //{
                            //    lOperId = -1;
                            //    GetLastOperation(parametersIn["p"].ToString(), parametersIn["d"].ToString(), strArticlesFilter, ref lOperId);

                            //    if (lOperId > 0)
                            //    {
                            //        // If plate does not have return rights, send a different error if it is because the user did a return, but still has the minimum parking time left
                            //        int iType = -1;
                            //        if (GetOperationType(lOperId, ref iType))
                            //        {
                            //            if (iType != Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Refund"]))
                            //            {
                            //                parametersIn["o"] = ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString();
                            //                parametersM1OutMapping["Ao"] = "o";

                            //                rtM1 = SendM1(parametersIn, parametersM1InMapping, parametersM1OutMapping, iVirtualUnit, out parametersM1Out);

                            //                // If user is still parked
                            //                if ((rtM1 == ResultType.Result_OK && parametersM1Out["o"].ToString() == ConfigurationManager.AppSettings["OperationsDef.Extension"].ToString())
                            //                    || rtM1 == ResultType.Result_Error_MaxTimeAlreadyUsedInPark)
                            //                    rtM1 = ResultType.Result_Error_No_Return_For_Minimum;
                            //                else
                            //                    rtM1 = ResultType.Result_Error_Plate_Has_No_Return;

                            //                parametersM1Out.Remove("o");
                            //            }
                            //        }
                            //    }

                            //    iRes = Convert.ToInt32(rtM1);
                            //    Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                            //}
                            else
                            {
                                iRes = Convert.ToInt32(rtM1);
                                Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                            }

                        }
                    }

                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);

                }

            }
            catch (Exception e)
            {
                iRes = iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return iRes;
        }


        /***
         * 
         *The parameters of method QueryFinePaymentQuantityXML are:
            a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
	            <f>fine number</f>
	            <d>date in format hh24missddMMYY</d> - *This parameter is optional
                <ah>authentication hash</ah> - *This parameter is optional
	        </arinpark_in>

            b.	Result: is an integer with these possible values:
                a.	>0: Quantity in Euro Cents to be paid for the input fine number
                b.	-5: Fine number not found
                c.	-6: Fine number found but fine type is not payable.
                d.	-7: Fine number not found but payment period has expired.
                e.	-8: Fine number already paid.
                f.	-9: Generic Error (for example database or execution error.)
                g.	-10: Invalid input parameter
                h.	-11: Missing input parameter
                i.	-12: OPS System error

         * 
         */

        [WebMethod]
        public int QueryFinePaymentQuantityXML(string xmlIn)
        {
            int iRes = 0;

            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["f"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
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

                            // Find group of last valid rotation parking to determine the corresponding virtual unit
                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");
                            int iGroupId = -1;
                            int iVirtualUnit = -1;

                            if (GetFineGroup(Convert.ToInt32(parametersIn["f"]), ref iGroupId, nContractId))
                            {
                                if (iGroupId > 0)
                                    GetVirtualUnit(iGroupId, ref iVirtualUnit, nContractId);
                                else
                                    Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityXML::Error - Could not find group for fine: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                            }
                            else
                                Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityXML::Error obtaining group for fine: xmlIn= {0}", xmlIn), LoggerSeverities.Error);

                            if (iVirtualUnit < 0)
                            {
                                if (GetFirstVirtualUnit(ref iVirtualUnit, nContractId))
                                {
                                    if (iVirtualUnit < 0)
                                    {
                                        iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                        Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                                        return iRes;
                                    }
                                }
                                else
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                            }

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


                            iRes = SendM5(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOut, nContractId);
                            if (iRes > 0)
                            {
                                Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityXML: iRes= {0}", iRes), LoggerSeverities.Info);
                            }
                            else
                            {
                                Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                            }

                        }
                    }

                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);

                }

            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return iRes;
        }




        /*
         * 	
            The parameters of method ConfirmFinePaymentXML are:
            a.	xmlIn: xml containing input parameters of the method:
            <arinpark_in>
	            <f>fine number</f>
	            <d>date in format hh24missddMMYY</d> - *This parameter is optional
	            <q>quantity paid in Euro Cents for the fine</q>
                <mui>mobile user identifier (authorization token)</mui>
                <cid>Cloud ID. Used for cloud notifications</cid>
                <os>Operating system: 1 (Android), 2 (iOS)</os>
                <ah>authentication hash</ah> - *This parameter is optional
	        </arinpark_in>

            b.	Result: is an integer with the next possible values:
                a.	1: Fine payment saved without errors
                b.	-1: Invalid authentication hash
                c.	-5: Fine number not found
                d.	-6: Fine number found but fine type is not payable.
                e.	-7: Fine number not found but payment period has expired.
                f.	-8: Fine number already paid.
                g.	-9: Generic Error (for example database or execution error.)
                h.	-10: Invalid input parameter
                i.	-11: Missing input parameter
                j.	-12: OPS System error
                l.	-14: Quantity received different as the quantity calculated previously
                m.  -20: Mobile user not found
                n.  -23: Invalid Login
                o.  -25: User does not have enough credit

         * 
         */
        [WebMethod]
        public int ConfirmFinePaymentXML(string xmlIn)
        {
            int iRes = 0;
            try
            {
                SortedList parametersIn = null;
                SortedList parametersM5Out = null;
                SortedList parametersM4In = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["f"] == null) ||
                        (parametersIn["q"] == null) ||
                        (parametersIn["mui"] == null) ||
                        (parametersIn["cid"] == null) ||
                        (parametersIn["os"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);

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
                            Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
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
                                Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error - Could not obtain user from token: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                            }
                            else
                                Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML: MobileUserId = {0}", nMobileUserId), LoggerSeverities.Info);

                            // Determine if token is valid
                            TokenValidationResult tokenResult = DefaultVerification(strToken);

                            if (tokenResult != TokenValidationResult.Passed)
                            {
                                Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error - Token not valid: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                                return Convert.ToInt32(ResultType.Result_Error_Invalid_Login);
                            }

                            // Change parameter from token to user
                            parametersIn["mui"] = nMobileUserId.ToString();

                            // Check to see if user exists, and if so, if they have enough credit
                            // Send contract Id as 0 so that it uses the global users connection
                            int nCredit = 0;
                            if (GetMobileUserCredit(Convert.ToInt32(parametersIn["mui"].ToString()), ref nCredit, 0) != 1)
                            {
                                iRes = Convert.ToInt32(ResultType.Result_Error_Mobile_User_Not_Found);
                                Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                return iRes;
                            }
                            else
                            {
                                if (Convert.ToInt32(parametersIn["q"]) > nCredit)
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Not_Enough_Credit);
                                    Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                            }

                            // Find group of last valid rotation parking to determine the corresponding virtual unit
                            if (parametersIn["d"] == null || parametersIn["d"].ToString().Length == 0)
                                parametersIn["d"] = DateTime.Now.ToString("HHmmssddMMyy");
                            int iGroupId = -1;
                            int iVirtualUnit = -1;

                            if (GetFineGroup(Convert.ToInt32(parametersIn["f"]), ref iGroupId, nContractId))
                            {
                                if (iGroupId > 0)
                                    GetVirtualUnit(iGroupId, ref iVirtualUnit, nContractId);
                                else
                                    Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error - Could not find group for fine: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
                            }
                            else
                                Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error obtaining group for fine: xmlIn= {0}", xmlIn), LoggerSeverities.Error);

                            if (iVirtualUnit < 0)
                            {
                                if (GetFirstVirtualUnit(ref iVirtualUnit, nContractId))
                                {
                                    if (iVirtualUnit < 0)
                                    {
                                        iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                        Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                                        return iRes;
                                    }
                                }
                                else
                                {
                                    iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error: xmlIn= {0}, iRes={1}", xmlIn, iRes), LoggerSeverities.Error);
                                    return iRes;
                                }
                            }

                            parametersIn["m"] = "1";  //Es pago por móvil

                            Hashtable parametersM5InMapping = new Hashtable();

                            parametersM5InMapping["f"] = "f";
                            parametersM5InMapping["d"] = "d";
                            parametersM5InMapping["m"] = "m";

                            Hashtable parametersM5OutMapping = new Hashtable();

                            parametersM5OutMapping["r"] = "r";
                            parametersM5OutMapping["p"] = "p";
                            parametersM5OutMapping["q"] = "q";
                            parametersM5OutMapping["y"] = "y";

                            iRes = SendM5(parametersIn, parametersM5InMapping, parametersM5OutMapping, iVirtualUnit, out parametersM5Out, nContractId);

                            if (iRes > 0)
                            {
                                if (parametersIn["q"].ToString() == iRes.ToString())
                                {

                                    Hashtable parametersM4InMapping = new Hashtable();
                                    parametersM4InMapping["f"] = "f";
                                    parametersM4InMapping["y"] = "y";
                                    parametersM4InMapping["p"] = "p";
                                    parametersM4InMapping["q"] = "q";
                                    parametersM4InMapping["u"] = "u";
                                    parametersM4InMapping["d"] = "d";
                                    parametersM4InMapping["mui"] = "mui";
                                    parametersM4InMapping["cid"] = "cid";
                                    parametersM4InMapping["os"] = "os";

                                    parametersM4In = new SortedList();
                                    parametersM4In["f"] = parametersIn["f"];
                                    parametersM4In["y"] = parametersM5Out["y"];
                                    parametersM4In["p"] = ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString();  //Tipo de pago: teléfono
                                    parametersM4In["q"] = parametersIn["q"];
                                    parametersM4In["u"] = iVirtualUnit.ToString();
                                    parametersM4In["d"] = parametersIn["d"];
                                    parametersM4In["mui"] = parametersIn["mui"];
                                    parametersM4In["cid"] = parametersIn["cid"];
                                    parametersM4In["os"] = parametersIn["os"];

                                    ResultType rtM4 = SendM4(parametersM4In, parametersM4InMapping, iVirtualUnit, nContractId);
                                    iRes = Convert.ToInt32(rtM4);
                                    Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML: iOut= {0}", iRes), LoggerSeverities.Info);
                                }
                                else
                                {
                                    iRes = iRes = Convert.ToInt32(ResultType.Result_Error_Quantity_To_Pay_Different_As_Calculated);
                                    Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                                }

                            }
                            else
                            {
                                Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                            }

                        }
                    }

                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);

                }

            }
            catch (Exception e)
            {
                iRes = iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("ConfirmFinePaymentXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }

            return iRes;
        }

        /*
         * 
         * The parameters of method QueryFileXML are:
            a.	xmlIn: xml containing input parameters of the method:
                <geoslab_in>
                    <fid>file ID</fid>
                    <ah>authentication hash</ah> - *This parameter is optional
	            </geoslab_in>

	            The authentication hash will be a string generated using the input parameters. Using this value we will detect the method call has been made by a well known client.

            b.	Result: is also a string containing an xml with the result of the method:
            <geoslab_out>
	            <r>Result of the method</r>
                <fc>File contents in raw format</fc>
            </geoslab_out>

            The tag <r> of the method will have these possible values:
                a.	1: File returned without errors
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
         * 
         * 
         */
        [WebMethod]
        public string QueryFileXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryFileXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["fid"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryFileXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("QueryFileXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            if (GetFile(parametersIn["fid"].ToString(), out parametersOut, nContractId))
                            {
                                parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();

                                xmlOut = GenerateXMLOuput(parametersOut);

                                if (xmlOut.Length == 0)
                                {
                                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryFileXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                }
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryFileXML: xmlOut= {0}", xmlOut), LoggerSeverities.Info);
                                }
                            }
                            else
                            {
                                xmlOut = GenerateXMLErrorResult(rt);
                                Logger_AddLogMessage(string.Format("QueryFileXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            }
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryFileXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryFileXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        /*
         * 	
            The parameters of method UpdateFileXML are:
            a.	xmlIn: xml containing input parameters of the method:
                <geoslab_in>
	                <fid>file ID</fid>
                    <fc>File contents in raw format</fc>
                    <ah>authentication hash</ah> - *This parameter is optional
	            </geoslab_in>


            b.	Result: is an integer with the next possible values:
                a.	1: File saved without errors
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
         * 
         */
        [WebMethod]
        public int UpdateFileXML(string xmlIn)
        {
            int iRes = (int)ResultType.Result_Error_Generic;
            try
            {
                SortedList parametersIn = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("UpdateFileXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["fid"] == null) ||
                        (parametersIn["fc"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        iRes = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("UpdateFileXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("UpdateFileXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
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

                            if (UpdateFile(parametersIn["fid"].ToString(), parametersIn["fc"].ToString(), nContractId))
                            {
                                iRes = Convert.ToInt32(ResultType.Result_OK);
                                Logger_AddLogMessage(string.Format("UpdateFileXML: iOut= {0}", iRes), LoggerSeverities.Info);
                            }
                            else
                            {
                                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("UpdateFileXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                            }
                        }
                    }
                }
                else
                {
                    iRes = Convert.ToInt32(rt);
                    Logger_AddLogMessage(string.Format("UpdateFileXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("UpdateFileXML::Error: xmlIn= {0}, iOut={1}", xmlIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return iRes;
        }

        /*
         * 
         * The parameters of method QueryZoneXML are:
            a.	xmlIn: xml containing input parameters of the method:
                <geoslab_in>
                    <lt>lattitude</lt>
                    <lg>longitude</lg>
                    <streetname>name of street</streetname>
                    <streetno>street address number</streetno>
                    <ah>authentication hash</ah> - *This parameter is optional
	            </geoslab_in>

	            The authentication hash will be a string generated using the input parameters. Using this value we will detect the method call has been made by a well known client.

            b.	Result: is also a string containing an xml with the result of the method:
            <geoslab_out>
	            <r>Result of the method</r>
                <zone>Zone</zone>
                <sector>Sector</sector>
                <zonename>Zone name<zonename>
                <sectorname>Sector name<sectorname>
                <zonecolor>Zone color<zonecolor>
                <sectorcolor>Sector color<sectorcolor>
                <lt>lattitude</lt>
                <lg>longitude</lg>
                <streetname>name of street</streetname>
                <streetno>street address number</streetno>
            </geoslab_out>

            The tag <r> of the method will have these possible values:
                a.	1: Success and the restrictions come after this tag.
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
                g.  -30: Location not found
         * 
         * 
         */
        [WebMethod]
        public string QueryZoneXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = new SortedList();
                string strHash = "";
                string strHashString = "";
                string strZoneName = "";
                string strSectorName = "";
                string strZoneColor = "673AB7";
                string strSectorColor = "673AB7";
                bool bFoundStreetData = false;
                int nZoneId = -1;
                int nGroupId = -1;
                int nStreetId = -1;
                int nStretchId = -1;
                bool bFoundZone = false;

                Logger_AddLogMessage(string.Format("QueryZoneXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["lt"] == null) ||
                        (parametersIn["lg"] == null) ||
                        (parametersIn["streetname"] == null) ||
                        (parametersIn["streetno"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryZoneXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("QueryZoneXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            // Determine the group/sector
                            // First try to use the address by getting the GPS coordinates
                            string strContractName = "";
                            GetParameter("P_SYSTEM_NAME", out strContractName, nContractId);
                            Loc curLocation = null;
                            if (parametersIn["streetname"].ToString().Length > 0 && parametersIn["streetno"].ToString().Length > 0)
                            {
                                // First check locally in cached data
                                Logger_AddLogMessage(string.Format("QueryZoneXML::Searching location locally"), LoggerSeverities.Info);
                                int nLocationId = GetLocationIdFromCache(parametersIn["streetname"].ToString(), parametersIn["streetno"].ToString(), nContractId);
                                if (nLocationId > 0)
                                    curLocation = LocateGPSFromAddressLocal(nLocationId, nContractId);

                                // If nothing is found in cache, use Google to find coordinates
                                if (curLocation == null)
                                {
                                    Logger_AddLogMessage(string.Format("QueryZoneXML::Searching location with Google"), LoggerSeverities.Info);
                                    curLocation = LocateGPSFromAddress(parametersIn["streetname"].ToString() + " " + parametersIn["streetno"].ToString() + " " + strContractName + " España");

                                    if (curLocation != null)
                                        AddLocationToCache(curLocation.Lt.ToString().Replace(",", "."), curLocation.Lg.ToString().Replace(",", "."), parametersIn["streetname"].ToString(), parametersIn["streetno"].ToString(), nContractId);
                                }

                                if (curLocation != null)
                                {
                                    parametersOut["lt"] = curLocation.Lt.ToString().Replace(",", ".");
                                    parametersOut["lg"] = curLocation.Lg.ToString().Replace(",", ".");
                                    parametersOut["streetname"] = parametersIn["streetname"];
                                    parametersOut["streetno"] = parametersIn["streetno"];
                                    bFoundStreetData = true;
                                }
                            }

                            // If the address did not work, then use the GPS coordinates
                            if (curLocation == null)
                            {
                                // Filter parameters with values "0" and "0.0"
                                if (parametersIn["lt"].ToString().Equals("0") || parametersIn["lt"].ToString().Equals("0.0") || parametersIn["lt"].ToString().Equals("0,0"))
                                    parametersIn["lt"] = "";
                                if (parametersIn["lg"].ToString().Equals("0") || parametersIn["lg"].ToString().Equals("0.0") || parametersIn["lg"].ToString().Equals("0,0"))
                                    parametersIn["lg"] = "";

                                if (parametersIn["lt"].ToString().Length > 0 && parametersIn["lg"].ToString().Length > 0)
                                {
                                    string strLattitude = parametersIn["lt"].ToString();
                                    string strLongitude = parametersIn["lg"].ToString();

                                    if (Convert.ToInt32(ConfigurationManager.AppSettings["GlobalizeCommaGPS"]) == 1)
                                    {
                                        strLattitude = strLattitude.ToString().Replace(".", ",");
                                        strLongitude = strLongitude.ToString().Replace(".", ",");
                                    }
                                    curLocation = new Loc(Convert.ToDouble(strLattitude), Convert.ToDouble(strLongitude));

                                    string strStreet = "";
                                    string strNumber = "";
                                    string strCity = "";
                                    List<int> stretchList = null;
                                    List<Loc> areaList = null;

                                    Logger_AddLogMessage(string.Format("QueryZoneXML::Searching stretch areas"), LoggerSeverities.Info);
                                    GetStretchAreasToSearch(out stretchList, nContractId);
                                    if (stretchList.Count > 0)
                                    {
                                        foreach (int iStretch in stretchList)
                                        {
                                            GetStretchAreas(iStretch, out areaList, nContractId);
                                            if (IsPointInPolygon(areaList, curLocation))
                                            {
                                                // Found area which corresponds to a defined stretch, get the sector/street info
                                                Logger_AddLogMessage(string.Format("QueryZoneXML::Found the stretch - {0}", iStretch.ToString()), LoggerSeverities.Info);
                                                int nStreetNo = -1;
                                                if (GetStretchData(iStretch, out nZoneId, out nStreetId, out nStreetNo, nContractId))
                                                {
                                                    if (GetStreetName(nStreetId, out strStreet, nContractId))
                                                    {
                                                        parametersOut["streetname"] = strStreet;
                                                        parametersOut["streetno"] = nStreetNo.ToString();
                                                        bFoundStreetData = true;
                                                        bFoundZone = true;
                                                        GetGroupParent(nZoneId, ref nGroupId, nContractId);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (!bFoundStreetData)
                                    {
                                        Logger_AddLogMessage(string.Format("QueryZoneXML::Searching address locally"), LoggerSeverities.Info);
                                        string strContractName1 = "";
                                        string strContractName2 = "";
                                        GetContractNames(out strContractName1, out strContractName2, nContractId);
                                        if (!LocateAddressFromGPSLocal(curLocation, out strStreet, out strNumber, nContractId))
                                        {
                                            Logger_AddLogMessage(string.Format("QueryZoneXML::Searching location with Google"), LoggerSeverities.Info);
                                            // First make filtered check, and then if nothing is found, remove filter and take first result
                                            if (LocateAddressFromGPS(curLocation, true, out strStreet, out strNumber, out strCity))
                                            {
                                                if (strCity.ToUpper().Equals(strContractName1.ToUpper()) || strCity.ToUpper().Equals(strContractName2.ToUpper()))
                                                {
                                                    parametersOut["streetname"] = strStreet;
                                                    parametersOut["streetno"] = strNumber;
                                                    bFoundStreetData = true;

                                                    AddLocationToCache(curLocation.Lt.ToString().Replace(",", "."), curLocation.Lg.ToString().Replace(",", "."), strStreet, strNumber, nContractId);
                                                }
                                                else
                                                {
                                                    Logger_AddLogMessage(string.Format("QueryZoneXML::CurLoc: lat= {0}, long={1} gives the city {2} instead of {3}/{4}", curLocation.Lt.ToString(), curLocation.Lg.ToString(), strCity, strContractName1, strContractName2), LoggerSeverities.Info);
                                                    rt = ResultType.Result_Error_Location_Not_Found;
                                                }
                                            }
                                            else if (LocateAddressFromGPS(curLocation, false, out strStreet, out strNumber, out strCity))
                                            {
                                                if (strCity.ToUpper().Equals(strContractName1.ToUpper()) || strCity.ToUpper().Equals(strContractName2.ToUpper()))
                                                {
                                                    parametersOut["streetname"] = strStreet;
                                                    parametersOut["streetno"] = strNumber;
                                                    bFoundStreetData = true;

                                                    AddLocationToCache(curLocation.Lt.ToString().Replace(",", "."), curLocation.Lg.ToString().Replace(",", "."), strStreet, strNumber, nContractId);
                                                }
                                                else
                                                {
                                                    Logger_AddLogMessage(string.Format("QueryZoneXML::CurLoc: lat= {0}, long={1} gives the city {2} instead of {3}/{4}", curLocation.Lt.ToString(), curLocation.Lg.ToString(), strCity, strContractName1, strContractName2), LoggerSeverities.Info);
                                                    rt = ResultType.Result_Error_Location_Not_Found;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            parametersOut["streetname"] = strStreet;
                                            parametersOut["streetno"] = strNumber;
                                            bFoundStreetData = true;
                                        }
                                    }

                                    parametersOut["lt"] = parametersIn["lt"].ToString().Replace(",", ".");
                                    parametersOut["lg"] = parametersIn["lg"].ToString().Replace(",", ".");
                                }
                            }

                            if (curLocation != null)
                                Logger_AddLogMessage(string.Format("QueryZoneXML::CurLoc: lat= {0}, long={1}", curLocation.Lt.ToString(), curLocation.Lg.ToString()), LoggerSeverities.Error);
                            else
                                Logger_AddLogMessage(string.Format("QueryZoneXML::No location found"), LoggerSeverities.Error);

                            if (rt == ResultType.Result_OK)
                            {
                                // First try to find the group/sector by the street stretch
                                if (!bFoundZone)
                                {
                                    if (GetStreetId(parametersOut["streetname"].ToString(), out nStreetId, nContractId))
                                    {
                                        if (GetSectorFromStretch(nStreetId, parametersOut["streetno"].ToString(), out nZoneId, out nStretchId, nContractId))
                                        {
                                            bFoundZone = true;
                                            Logger_AddLogMessage(string.Format("QueryZoneXML::Found sector {0} and stretch {1} for street {2}, {3}", nZoneId, nStretchId, parametersOut["streetname"].ToString(), parametersOut["streetno"].ToString()), LoggerSeverities.Error);
                                            GetGroupParent(nZoneId, ref nGroupId, nContractId);
                                        }
                                        else
                                            Logger_AddLogMessage(string.Format("QueryZoneXML::No sector found for street {0} - {1}", nStreetId, parametersOut["streetname"].ToString()), LoggerSeverities.Error);
                                    }
                                    else
                                    {
                                        Logger_AddLogMessage(string.Format("QueryZoneXML::Could not find street {0}", parametersOut["streetname"].ToString()), LoggerSeverities.Error);
                                    }
                                }

                                if (!bFoundZone && ConfigurationManager.AppSettings["cont" + nContractId.ToString() + ".LocalStretchSearchOnly"].ToString().Equals("0"))
                                {
                                    Logger_AddLogMessage(string.Format("QueryZoneXML::Using GPS to find group/sector"), LoggerSeverities.Info);

                                    // Use the GPS coordinates to try and find the group/sector
                                    if (curLocation != null)
                                    {
                                        // Obtain GPS zone information
                                        string strContractPrefix = "";
                                        if (nContractId > 0)
                                            strContractPrefix = "cont" + nContractId.ToString() + ".";
                                        List<Loc> areaList = new List<Loc>();
                                        int nNumZones = Convert.ToInt32(ConfigurationManager.AppSettings[strContractPrefix + "NumZones"].ToString());
                                        int nZoneIndex = 0;
                                        bFoundZone = false;
                                        while (++nZoneIndex <= nNumZones && !bFoundZone)
                                        {
                                            int nZoneAreas = Convert.ToInt32(ConfigurationManager.AppSettings[strContractPrefix + "Zone" + nZoneIndex.ToString() + ".NumAreas"].ToString());
                                            for (int nAreaIndex = 1; nAreaIndex <= nZoneAreas; nAreaIndex++)
                                            {
                                                areaList.Clear();
                                                int nAreaNumPoints = Convert.ToInt32(ConfigurationManager.AppSettings[strContractPrefix + "Zone" + nZoneIndex.ToString() + ".Area" + nAreaIndex.ToString() + ".NumPoints"].ToString());
                                                for (int nPointIndex = 1; nPointIndex <= nAreaNumPoints; nPointIndex++)
                                                {
                                                    string strAreaPoints = ConfigurationManager.AppSettings[strContractPrefix + "Zone" + nZoneIndex.ToString() + ".Area" + nAreaIndex.ToString() + ".P" + nPointIndex.ToString()].ToString();
                                                    string[] strPoints = strAreaPoints.Split(new char[] { ':' });
                                                    areaList.Add(new Loc(Convert.ToDouble(strPoints[0]), Convert.ToDouble(strPoints[1])));
                                                }

                                                if (areaList.Count > 0)
                                                {
                                                    bFoundZone = IsPointInPolygon(areaList, curLocation);
                                                    if (bFoundZone)
                                                    {
                                                        nZoneId = Convert.ToInt32(ConfigurationManager.AppSettings[strContractPrefix + "Zone" + nZoneIndex.ToString() + ".Id"].ToString());
                                                        nGroupId = Convert.ToInt32(ConfigurationManager.AppSettings[strContractPrefix + "Zone" + nZoneIndex.ToString() + ".GroupId"].ToString());
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (nZoneId > 0 && nGroupId > 0 && bFoundStreetData)
                                parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                            else
                                parametersOut["r"] = Convert.ToInt32(ResultType.Result_Error_Location_Not_Found).ToString();

                            parametersOut["zone"] = nGroupId.ToString();
                            if (nGroupId > 0)
                                GetGroupName(nGroupId, out strZoneName, out strZoneColor, nContractId);
                            parametersOut["zonename"] = strZoneName;
                            if (strZoneColor.Length > 0)
                                parametersOut["zonecolor"] = strZoneColor;
                            parametersOut["sector"] = nZoneId.ToString();
                            if (nZoneId > 0)
                                GetGroupName(nZoneId, out strSectorName, out strSectorColor, nContractId);
                            parametersOut["sectorname"] = strSectorName;
                            if (strSectorColor.Length > 0)
                                parametersOut["sectorcolor"] = strSectorColor;

                            xmlOut = GenerateXMLOuput(parametersOut);

                            if (xmlOut.Length == 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryZoneXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            }
                            else
                            {
                                Logger_AddLogMessage(string.Format("QueryZoneXML: xmlOut= {0}", xmlOut), LoggerSeverities.Info);
                            }
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryZoneXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryZoneXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        /*
         * 
         * The parameters of method QueryPlaceXML are:
            a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <contid>Installation ID</contid>
                    <streetname>name of street to look for</streetname>
                    <ah>authentication hash</ah> - *This parameter is optional
	            </arinpark_in>

	            The authentication hash will be a string generated using the input parameters. Using this value we will detect the method call has been made by a well known client.

            b.	Result: is also a string containing an xml with the result of the method:
            <arinpark_out>
	            <r>Result of the method</r>
                <response>Response received from Google API</response>
            </arinpark_out>

            The tag <r> of the method will have these possible values:
                a.	1: Success and the restrictions come after this tag.
                b.	-1: Invalid authentication hash
                c.	-9: Generic Error (for example database or execution error)
                d.	-10: Invalid input parameter
                e.	-11: Missing input parameter
                f.	-12: OPS System error
                g.  -30: Location not found
         * 
         * 
         */
        [WebMethod]
        public string QueryPlaceXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = new SortedList();
                string strHash = "";
                string strHashString = "";
                string strResponse = "";

                Logger_AddLogMessage(string.Format("QueryPlaceXML: xmlIn= {0}", xmlIn), LoggerSeverities.Info);

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["streetname"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryPlaceXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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
                            Logger_AddLogMessage(string.Format("QueryPlaceXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
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

                            // First check the database to see if any results are found in the cached locations
                            int nLocateResult = 0;
                            int nNumLocations = GetNumPredictions(parametersIn["streetname"].ToString(), nContractId);

                            if (nNumLocations > 0)
                                nLocateResult = 1;
                            else
                            {
                                // Get contract coordenates
                                string strLat = "";
                                string strLong = "";
                                GetParameter("P_MAPS_INITIAL_LAT", out strLat, nContractId);
                                GetParameter("P_MAPS_INITIAL_LON", out strLong, nContractId);
                                if (Convert.ToInt32(ConfigurationManager.AppSettings["GlobalizeCommaGPS"]) == 1)
                                {
                                    strLat = strLat.ToString().Replace(".", ",");
                                    strLong = strLong.ToString().Replace(".", ",");
                                }
                                Loc contractLoc = new Loc(Convert.ToDouble(strLat), Convert.ToDouble(strLong));

                                nLocateResult = LocatePlace(parametersIn["streetname"].ToString(), contractLoc, out strResponse);
                                if (nLocateResult > 0)
                                {
                                    // Test string
                                    //strResponse = "{\"predictions\" : [{\"description\" : \"San Bartolome Kalea, Ordizia, Spain\",\"id\" : \"8eb44662c2b1300287268cb0eedbf7ee81d107b6\",\"matched_substrings\" : [{\"length\" : 3,\"offset\" : 0}],\"place_id\" : \"EiNTYW4gQmFydG9sb21lIEthbGVhLCBPcmRpemlhLCBTcGFpbg\",\"reference\" : \"CmRRAAAAVq07FhDE4SBfLfC15pyM9wxwV521xm9rZzVd8BUwvMVRrZyNMwPhjsJ7Ad9wASiT6b2E5FyWzZ0uclPvrjn0t8z6k850tHatMIx-J4uwzvygmMq9dSgtTzdDD-yAb23fEhAX-enj4-31iBkHZCH0LQmOGhQ0Mgz4V1CjcvWQgyupxHhZswOiqg\",\"structured_formatting\" : {\"main_text\" : \"San Bartolome Kalea\",\"main_text_matched_substrings\" : [{\"length\" : 3,\"offset\" : 0}],\"secondary_text\" : \"Ordizia, Spain\"},\"terms\" : [{\"offset\" : 0,\"value\" : \"San Bartolome Kalea\"},{\"offset\" : 21,\"value\" : \"Ordizia\"},{\"offset\" : 30,\"value\" : \"Spain\"}],\"types\" : [ \"route\", \"geocode\" ]},{\"description\" : \"San Inazio Kalea, Beasain, Spain\",\"id\" : \"cb80e0f2ce08275845bfba8d86925077a5379c92\",\"matched_substrings\" : [{\"length\" : 3,\"offset\" : 0}],\"place_id\" : \"EiBTYW4gSW5hemlvIEthbGVhLCBCZWFzYWluLCBTcGFpbg\",\"reference\" : \"ClROAAAAntS31pR9QSO9HRel-4k8WMUysGR82-hVboc_RsHVSxR9RH36-1x--zwxZlZDNbIRPG7I-U5R0oMT2El3Ux7XzvnscDHNw9JRWvLTDg4xEe8SEAZqny2i4vCZ-8KFvJkT8s8aFNY9kbZ1bdZf_MqILVLqKAnxcN03\",\"structured_formatting\" : {\"main_text\" : \"San Inazio Kalea\",\"main_text_matched_substrings\" : [{\"length\" : 3,\"offset\" : 0}],\"secondary_text\" : \"Beasain, Spain\"},\"terms\" : [{\"offset\" : 0,\"value\" : \"San Inazio Kalea\"},{\"offset\" : 18,\"value\" : \"Beasain\"},{\"offset\" : 27,\"value\" : \"Spain\"}],\"types\" : [ \"route\", \"geocode\" ]},{\"description\" : \"Santa Fe Kalea, Zaldibia, Spain\",\"id\" : \"da93dc47aa94e2b61f29992235046186b9b63d08\",\"matched_substrings\" : [{\"length\" : 3,\"offset\" : 0}],\"place_id\" : \"Eh9TYW50YSBGZSBLYWxlYSwgWmFsZGliaWEsIFNwYWlu\",\"reference\" : \"ClRNAAAAzDP6IN6wZ6yQvClaUCQkftA4In77OPBmTq7xnvnxVoWDaxp5Z4daVcbe1UgFLqk9LKAt-CjEyt7LA2_y4FmFNznzd2Lcv-IvDYY-6fUW82oSEOZHC6VLYAq4s17h39W3_L4aFLqpkLTd4rmFMw4IQggGk8nV5Mb-\",\"structured_formatting\" : {\"main_text\" : \"Santa Fe Kalea\",\"main_text_matched_substrings\" : [{\"length\" : 3,\"offset\" : 0}],\"secondary_text\" : \"Zaldibia, Spain\"},\"terms\" : [{\"offset\" : 0,\"value\" : \"Santa Fe Kalea\"},{\"offset\" : 16,\"value\" : \"Zaldibia\"},{\"offset\" : 26,\"value\" : \"Spain\"}],\"types\" : [ \"route\", \"geocode\" ]},{\"description\" : \"San Andres Kalea, Ormaiztegi, Spain\",\"id\" : \"5c8db4b9dda703cfbe9113efb465b2642c0c4892\",\"matched_substrings\" : [{\"length\" : 3,\"offset\" : 0}],\"place_id\" : \"EiNTYW4gQW5kcmVzIEthbGVhLCBPcm1haXp0ZWdpLCBTcGFpbg\",\"reference\" : \"CmRRAAAADiQsKBF7KAW9SgOnAp8Y5VeVkJ5QF-PlCYvc6-XIuAXzBR5DYlpttKGBnjnPB7Kmu6Y83pTuMVzrIrbtw9pnUV3JmTCrwukgfuEBqNwXvth3lt2GxIa-CmU15KGXRz9NEhCLs14q-UFJVfw62DNKqieJGhSmTZA04xqRPavXJxQ6aiI8KzHP4w\",\"structured_formatting\" : {\"main_text\" : \"San Andres Kalea\",\"main_text_matched_substrings\" : [{\"length\" : 3,\"offset\" : 0}],\"secondary_text\" : \"Ormaiztegi, Spain\"},\"terms\" : [{\"offset\" : 0,\"value\" : \"San Andres Kalea\"},{\"offset\" : 18,\"value\" : \"Ormaiztegi\"},{\"offset\" : 30,\"value\" : \"Spain\"}],\"types\" : [ \"route\", \"geocode\" ]},{\"description\" : \"San Gregorio Kalea, Zumarraga, Spain\",\"id\" : \"cde7d0d18b150d152d994391a608fb2cae999220\",\"matched_substrings\" : [{\"length\" : 3,\"offset\" : 0}],\"place_id\" : \"EiRTYW4gR3JlZ29yaW8gS2FsZWEsIFp1bWFycmFnYSwgU3BhaW4\",\"reference\" : \"CmRSAAAApTwiCStAuChIcwqGm4RdV7n6PddpEXuuOwxYdNZCm3tVluEiJvd3jGoslUSZqWxxRio5rLouBK4RJdcuS4cHpKMFHhi3QlFCLzmhjvqUE67EDFPdt56L46AxRg0Dx8D0EhCn2LCQTUK_qXXKLPt6eIkTGhSrMeov0h6W4c0xjTvdVDWEN3qXXg\",\"structured_formatting\" : {\"main_text\" : \"San Gregorio Kalea\",\"main_text_matched_substrings\" : [{\"length\" : 3,\"offset\" : 0}],\"secondary_text\" : \"Zumarraga, Spain\"},\"terms\" : [{\"offset\" : 0,\"value\" : \"San Gregorio Kalea\"},{\"offset\" : 20,\"value\" : \"Zumarraga\"},{\"offset\" : 31,\"value\" : \"Spain\"}],\"types\" : [ \"route\", \"geocode\" ]}],\"status\" : \"OK\"}";

                                    if (strResponse.Length > 0)
                                    {
                                        if (!AddResultsToCache(parametersIn["streetname"].ToString(), strResponse, nContractId))
                                        {
                                            Logger_AddLogMessage(string.Format("QueryPlaceXML::Error adding results to cache: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                            xmlOut = GenerateXMLOuput(parametersOut);
                                            return xmlOut;
                                        }
                                    }
                                    else
                                        nLocateResult = 0;
                                }
                                else if (nLocateResult == 0)
                                    Logger_AddLogMessage(string.Format("QueryPlaceXML::No results found for location"), LoggerSeverities.Info);
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryPlaceXML::Error obtaining place results: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    parametersOut["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    xmlOut = GenerateXMLOuput(parametersOut);
                                    return xmlOut;
                                }
                            }

                            string strLocations = "";
                            if (nLocateResult > 0)
                            {
                                if (!GetPredictionsFromCache(parametersIn["streetname"].ToString(), out strLocations, nContractId))
                                {
                                    Logger_AddLogMessage(string.Format("QueryPlaceXML::Error obtaining results from cache: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                                    parametersOut["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    xmlOut = GenerateXMLOuput(parametersOut);
                                    return xmlOut;
                                }
                            }

                            if (strLocations.Length > 0)
                            {
                                parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                                parametersOut["response"] = strLocations;
                            }
                            else
                                parametersOut["r"] = Convert.ToInt32(ResultType.Result_Error_Location_Not_Found).ToString();

                            xmlOut = GenerateXMLOuput(parametersOut);

                            if (xmlOut.Length == 0)
                            {
                                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryPlaceXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            }
                            else
                            {
                                Logger_AddLogMessage(string.Format("QueryPlaceXML: xmlOut= {0}", xmlOut), LoggerSeverities.Info);
                            }
                        }
                    }
                }
                else
                {
                    xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryPlaceXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryPlaceXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        /*
         * The parameters of method QueryContractsXML are:

        a.	There are not any input parameters

        b.	Result: is also a string containing an xml with the result of the method:
                <arinpark_out>
                    <r>Result of the method</r>
                    <t>Current Date in format hh24missddMMYY</t>
                    <cont_no>Number of contracts</cont_no>
                    <contractlist>
                        <contract>
                            <cont_id>Contract id</cont_id>
                            <lt>Lattitude</lt>
                            <lg>Longitude</lg>
                            <desc1>Description 1 (name in Spanish)</desc1>
                            <desc2>Description 2 (name in Basque)</desc2>
                            <image>Image path</image>
                            <email>Email</email>
                            <phone>Telephone</phone>
                            <addr>Address</addr>
                            <rad>Radius (in meters)</rad>
                        </contract>
                        <contract>...</contract>
                        ...
                        <contract>...</contract>
                    </contractlist>
	            </arinpark_out>
         
            The tag <r> of the method will have these possible values:
            a.	1: Contract info comes after this tag
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error

         */
        [WebMethod]
        public string QueryContractsXML()
        {
            string xmlOut = "";
            try
            {
                SortedList parametersOut = new SortedList();
                SortedList contractList = null;

                // Get contracts information
                if (!GetContractsData(out contractList))
                {
                    xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                    Logger_AddLogMessage(string.Format("QueryContractsXML::Error - Could not obtain contracts data: xmlOut={0}", xmlOut), LoggerSeverities.Error);
                    return xmlOut;
                }

                if (contractList.Count > 0)
                    parametersOut["contractlist"] = contractList;
                else
                {
                    contractList = new SortedList();
                    contractList["contract1"] = "";
                    parametersOut["contractlist"] = contractList;
                }

                parametersOut["t"] = DateTime.Now.ToString("HHmmssddMMyy");
                parametersOut["cont_no"] = contractList.Count.ToString();
                parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                xmlOut = GenerateXMLOuput(parametersOut);
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryContractsXML::Error: xmlOut={0}", xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        /*
         * The parameters of method QueryStreetsXML are:

        a.	xmlIn: xml containing input parameters of the method:
                <arinpark_in>
                    <contid>Contract id</contid>
	            </arinpark_in>

        b.	Result: is also a string containing an xml with the result of the method:
                <arinpark_out>
                    <r>Result of the method</r>
                    <t>Current Date in format hh24missddMMYY</t>
                    <st_no>Number of streets</st_no>
                    <streetlist>
                        <street>
                            <st_name>Street name</st_name>
                        </street>
                        <street>...</street>
                        ...
                        <street>...</street>
                    </streetlist>
	            </arinpark_out>
         
            The tag <r> of the method will have these possible values:
            a.	1: Street info comes after this tag
            b.	-1: Invalid authentication hash
            c.	-9: Generic Error (for example database or execution error.)
            d.	-10: Invalid input parameter
            e.	-11: Missing input parameter
            f.	-12: OPS System error

         */
        [WebMethod]
        public string QueryStreetsXML(string xmlIn)
        {
            string xmlOut = "";
            try
            {
                SortedList parametersIn = null;
                SortedList parametersOut = new SortedList();
                SortedList streetList = null;
                string strHash = "";
                string strHashString = "";

                ResultType rt = FindInputParameters(xmlIn, out parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if (parametersIn["contid"] == null)
                    {
                        xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryFreeSpacesXML::Error - Missing parameter: xmlIn= {0}", xmlIn), LoggerSeverities.Error);
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

                        // Get contracts information
                        if (!GetStreetsData(out streetList, nContractId))
                        {
                            xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                            Logger_AddLogMessage(string.Format("QueryStreetsXML::Error - Could not obtain streets data: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                            return xmlOut;
                        }

                        if (streetList.Count > 0)
                            parametersOut["streetlist"] = streetList;
                        else
                        {
                            streetList = new SortedList();
                            streetList["street1"] = "";
                            parametersOut["streetlist"] = streetList;
                        }

                        parametersOut["t"] = DateTime.Now.ToString("HHmmssddMMyy");
                        parametersOut["st_no"] = streetList.Count.ToString();
                        parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                        xmlOut = GenerateXMLOuput(parametersOut);
                    }
                }
            }
            catch (Exception e)
            {
                xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryStreetsXML::Error: xmlIn= {0}, xmlOut={1}", xmlIn, xmlOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return xmlOut;
        }

        [WebMethod]
        public void QueryServerDateTimeJSON()
        {
            string jsonOut = "";
            try
            {
                Logger_AddLogMessage(string.Format("QueryParkingOperationJSON"), LoggerSeverities.Info);

                string strXmlOut = QueryServerDateTimeXML();

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryServerDateTimeJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryServerDateTimeJSON::Error: jsonOut={0}", jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryParkingStatusJSON(string jsonIn)
        {
            string jsonOut = "";
            try
            {
                Logger_AddLogMessage(string.Format("QueryParkingStatusJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryParkingStatusXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryParkingStatusJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryParkingStatusJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryParkingOperationJSON(string jsonIn)
        {
            string jsonOut = "";
            try
            {
                Logger_AddLogMessage(string.Format("QueryParkingOperationJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryParkingOperationXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryParkingOperationJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryParkingOperationJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
        }



        [WebMethod]
        public void QueryParkingOperationWithTimeStepsJSON(string jsonIn)
        {
            string jsonOut = "";
            try
            {
                Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryParkingOperationWithTimeStepsXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                // Modify vector format for case of only 1 time step
                int nStart = jsonOut.IndexOf("{\"st\":{");
                if (nStart > 0)
                {
                    string strTemp = jsonOut.Insert(nStart + 6, "[");
                    int nEnd = jsonOut.IndexOf("}}", nStart);
                    strTemp = strTemp.Insert(nEnd + 2, "]");
                    jsonOut = strTemp;
                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsJSON: Modified time step format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                }

                Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
        }


        [WebMethod]
        public void QueryParkingOperationWithMoneyStepsJSON(string jsonIn)
        {
            string jsonOut = "";
            try
            {
                Logger_AddLogMessage(string.Format("QueryParkingOperationWithQuantityStepsJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryParkingOperationWithMoneyStepsXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryParkingOperationWithQuantityStepsJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryParkingOperationWithQuantityStepsJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
        }


        [WebMethod]
        public void QueryParkingOperationForTimeJSON(string jsonIn)
        {
            string jsonOut = "";
            try
            {
                Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryParkingOperationForTimeXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
        }


        [WebMethod]
        public void QueryParkingOperationForMoneyJSON(string jsonIn)
        {
            string jsonOut = "";
            try
            {
                Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryParkingOperationForMoneyXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
        }



        [WebMethod]
        public void ConfirmParkingOperationJSON(string jsonIn)
        {
            int iRes = 0;
            try
            {
                Logger_AddLogMessage(string.Format("ConfirmParkingOperationJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iRes = ConfirmParkingOperationXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("ConfirmParkingOperationJSON: iRes= {0}", iRes), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iRes);
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("ConfirmParkingOperationJSON::Error: jsonIn= {0}, iRes ={1}", jsonIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
        }

        [WebMethod]
        public void QueryUnParkingOperationJSON(string jsonIn)
        {
            string jsonOut = "";
            try
            {
                Logger_AddLogMessage(string.Format("QueryUnParkingOperationJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryUnParkingOperationXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryUnParkingOperationJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryUnParkingOperationJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
        }

        [WebMethod]
        public void QueryUnParkingOperationQuantityJSON(string jsonIn)
        {
            int iRes = 0;
            try
            {
                Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iRes = QueryUnParkingOperationQuantityXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityJSON: iRes= {0}", iRes), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iRes);
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryUnParkingOperationQuantityJSON::Error: jsonIn= {0}, iRes ={1}", jsonIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
        }


        [WebMethod]
        public void ConfirmUnParkingOperationJSON(string jsonIn)
        {
            int iRes = 0;
            try
            {
                Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iRes = ConfirmUnParkingOperationXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationJSON: iRes= {0}", iRes), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iRes);
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("ConfirmUnParkingOperationJSON::Error: jsonIn= {0}, iRes ={1}", jsonIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
        }


        [WebMethod]
        public void QueryFinePaymentQuantityJSON(string jsonIn)
        {
            int iRes = 0;
            try
            {
                Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iRes = QueryFinePaymentQuantityXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityJSON: iRes= {0}", iRes), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iRes);
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryFinePaymentQuantityJSON::Error: jsonIn= {0}, iRes ={1}", jsonIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
        }


        [WebMethod]
        public void ConfirmFinePaymentJSON(string jsonIn)
        {
            int iRes = 0;
            try
            {
                Logger_AddLogMessage(string.Format("ConfirmFinePaymentJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iRes = ConfirmFinePaymentXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("ConfirmFinePaymentJSON: iRes= {0}", iRes), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iRes);
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("ConfirmFinePaymentJSON::Error: jsonIn= {0}, iRes ={1}", jsonIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);

            }
        }

        [WebMethod]
        public void QueryFileJSON(string jsonIn)
        {
            string jsonOut = "";
            try
            {
                Logger_AddLogMessage(string.Format("QueryFileJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryFileXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryFileJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryFileJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void UpdateFileJSON(string jsonIn)
        {
            int iRes = 0;
            try
            {
                Logger_AddLogMessage(string.Format("UpdateFileJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                iRes = UpdateFileXML(xmlIn.OuterXml);

                Logger_AddLogMessage(string.Format("UpdateFileJSON: iRes= {0}", iRes), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(iRes);
            }
            catch (Exception e)
            {
                iRes = Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("UpdateFileJSON::Error: jsonIn= {0}, iRes ={1}", jsonIn, iRes), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryZoneJSON(string jsonIn)
        {
            string jsonOut = "";
            try
            {
                Logger_AddLogMessage(string.Format("QueryZoneJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryZoneXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryZoneJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryZoneJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryPlaceJSON(string jsonIn)
        {
            string jsonOut = "";
            try
            {
                Logger_AddLogMessage(string.Format("QueryPlaceJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryPlaceXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryPlaceJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryPlaceJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryContractsJSON()
        {
            string jsonOut = "";

            try
            {
                Logger_AddLogMessage(string.Format("QueryContractsJSON"), LoggerSeverities.Info);

                string strXmlOut = QueryContractsXML();

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryContractsJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                // Modify vector format for case of only 1 contract
                int nStart = jsonOut.IndexOf("contract\":{");
                if (nStart > 0)
                {
                    string strTemp = jsonOut.Insert(nStart + 10, "[");
                    int nEnd = jsonOut.IndexOf("}},\"r", nStart);
                    strTemp = strTemp.Insert(nEnd + 2, "]");
                    jsonOut = strTemp;
                    Logger_AddLogMessage(string.Format("QueryContractsJSON: Modified contract format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                }

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryContractsJSON::Error: jsonOut={0}", jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public void QueryStreetsJSON(string jsonIn)
        {
            string jsonOut = "";

            try
            {
                Logger_AddLogMessage(string.Format("QueryStreetsJSON: jsonIn= {0}", jsonIn), LoggerSeverities.Info);

                XmlDocument xmlIn = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonIn);

                string strXmlOut = QueryStreetsXML(xmlIn.OuterXml);

                XmlDocument xmlOut = new XmlDocument();
                xmlOut.LoadXml(strXmlOut);
                jsonOut = JsonConvert.SerializeXmlNode(xmlOut);

                Logger_AddLogMessage(string.Format("QueryStreetsJSON: jsonOut= {0}", jsonOut), LoggerSeverities.Info);

                // Modify vector format for case of only 1 street
                int nStart = jsonOut.IndexOf("street\":{");
                if (nStart > 0)
                {
                    string strTemp = jsonOut.Insert(nStart + 8, "[");
                    int nEnd = jsonOut.IndexOf("}},\"t", nStart);
                    strTemp = strTemp.Insert(nEnd + 2, "]");
                    jsonOut = strTemp;
                    Logger_AddLogMessage(string.Format("QueryStreetsJSON: Modified street format - jsonOut= {0}", jsonOut), LoggerSeverities.Info);
                }

                Context.Response.ContentType = "application/json";
                Context.Response.Write(jsonOut);
            }
            catch (Exception e)
            {
                jsonOut = GenerateJSONErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                Logger_AddLogMessage(string.Format("QueryStreetsJSON::Error: jsonIn= {0}, jsonOut={1}", jsonIn, jsonOut), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }
        }

        [WebMethod]
        public string CalculateHash(string strInput)
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

        #endregion Messages

        #region Private Methods



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

        //Alternative way to calculate hash
        private string CalculateHash2(string strInput)
        {
            string strRes = "";
            try
            {
                TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();

                des.KeySize = 192;
                des.Key = _normTripleDesKey;
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.Zeros;
                byte[] initVector = new byte[8];
                Array.Clear(initVector, 0, 8);
                des.IV = initVector;

                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(strInput);
                ICryptoTransform ict = des.CreateEncryptor();
                byte[] hash = ict.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                if (hash.Length >= 8)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = hash.Length - 8; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }
                    strRes = sb.ToString();
                }

            }
            catch (Exception e)
            {
                Logger_AddLogMessage("CalculateHash::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return strRes;
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
                    foreach (XmlNode Node in Nodes)
                    {
                        switch (Node.Name)
                        {
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



        private ResultType SendM59(Hashtable parametersOutMapping, int iVirtualUnit, out SortedList parametersOut, int nContractId = 0)
        {
            ResultType rtRes = ResultType.Result_OK;
            parametersOut = null;

            try
            {
                SortedList parametersM59In = new SortedList();
                string strM59In = GenerateOPSMessage("m59", parametersM59In);

                if (strM59In.Length > 0)
                {

                    Logger_AddLogMessage(string.Format("SendM59::OPSMessageIn = {0}", strM59In), LoggerSeverities.Info);

                    string strM59Out = null;

                    if (OPSMessage(strM59In, iVirtualUnit, out strM59Out, nContractId))
                    {
                        Logger_AddLogMessage(string.Format("SendM59::OPSMessageOut = {0}", strM59Out), LoggerSeverities.Info);
                        SortedList parametersM59Out = new SortedList();
                        ResultType rtM59Out = FindOPSMessageOutputParameters(strM59Out, out parametersM59Out);


                        if (rtM59Out == ResultType.Result_OK)
                        {
                            parametersOut = new SortedList();
                            rtRes = ResultType.Result_Error_Generic;

                            foreach (DictionaryEntry item in parametersM59Out)
                            {
                                if (parametersOutMapping[item.Key.ToString()] != null)
                                {
                                    parametersOut[parametersOutMapping[item.Key.ToString()]] = item.Value.ToString();
                                }

                            }

                            rtRes = ResultType.Result_OK;
                        }
                        else
                        {
                            rtRes = rtM59Out;
                            Logger_AddLogMessage(string.Format("SendM59::Error In MessageOut = {0}", strM59Out), LoggerSeverities.Error);
                        }



                    }
                    else
                    {
                        rtRes = ResultType.Result_Error_OPS_Error;
                        Logger_AddLogMessage(string.Format("SendM59::Error Managing MessageIn = {0}", strM59In), LoggerSeverities.Error);
                    }
                }
                else
                {
                    rtRes = ResultType.Result_Error_OPS_Error;
                    Logger_AddLogMessage("SendM59::Error Generationg OPS M59 Message", LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                rtRes = ResultType.Result_Error_Generic;
                Logger_AddLogMessage("SendM59::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return rtRes;
        }


        private ResultType SendM1(SortedList parametersIn, Hashtable parametersInMapping, Hashtable parametersOutMapping, int iVirtualUnit, out SortedList parametersOut, int nContractId = 0)
        {
            ResultType rtRes = ResultType.Result_OK;
            parametersOut = null;

            try
            {
                SortedList parametersM1In = new SortedList();

                foreach (DictionaryEntry item in parametersIn)
                {

                    if (parametersInMapping[item.Key.ToString()] != null)
                    {
                        parametersM1In[parametersInMapping[item.Key.ToString()]] = item.Value.ToString();
                    }
                }

                string strM1In = GenerateOPSMessage("m1", parametersM1In);

                if (strM1In.Length > 0)
                {

                    Logger_AddLogMessage(string.Format("SendM1::OPSMessageIn = {0}", strM1In), LoggerSeverities.Info);

                    string strM1Out = null;
                    string strNumber = null;

                    if (OPSMessage(strM1In, iVirtualUnit, out strM1Out, nContractId))
                    {
                        Logger_AddLogMessage(string.Format("SendM1::OPSMessageOut = {0}", strM1Out), LoggerSeverities.Info);
                        SortedList parametersM1Out = new SortedList();
                        ResultType rtM1Out = FindOPSMessageOutputParameters(strM1Out, out parametersM1Out);

                        if (rtM1Out == ResultType.Result_OK)
                        {
                            parametersOut = new SortedList();
                            rtRes = ResultType.Result_Error_Generic;

                            foreach (DictionaryEntry item in parametersM1Out)
                            {

                                if (parametersOutMapping[item.Key.ToString()] != null)
                                {
                                    parametersOut[parametersOutMapping[item.Key.ToString()]] = item.Value.ToString();

                                    if (item.Key.ToString() == "Ar")
                                    {
                                        if (item.Value.ToString() == ConfigurationManager.AppSettings["M01.ErrorCodes.NoError"].ToString())
                                        {
                                            rtRes = ResultType.Result_OK;
                                        }
                                        else if (item.Value.ToString() == ConfigurationManager.AppSettings["M01.ErrorCodes.TiempoPermanenciaSuperado"].ToString())
                                        {
                                            rtRes = ResultType.Result_Error_MaxTimeAlreadyUsedInPark;
                                        }
                                        else if (item.Value.ToString() == ConfigurationManager.AppSettings["M01.ErrorCodes.TiempoReentradaNoSuperado"].ToString())
                                        {
                                            rtRes = ResultType.Result_Error_ReentryTimeError;
                                        }
                                        else if (item.Value.ToString() == ConfigurationManager.AppSettings["M01.ErrorCodes.NoReturn"].ToString())
                                        {
                                            rtRes = ResultType.Result_Error_Plate_Has_No_Return;
                                        }
                                        else
                                        {
                                            rtRes = ResultType.Result_Error_Generic;
                                        }

                                        parametersOut[parametersOutMapping[item.Key.ToString()]] = Convert.ToInt32(rtRes).ToString();
                                    }


                                }
                                else if ((parametersOutMapping["Acst"] != null) &&
                                        (item.Key.ToString().Length >= 3) &&
                                        (item.Key.ToString().Substring(0, 3) == "stq"))
                                {

                                    strNumber = item.Key.ToString().Substring(3, item.Key.ToString().Length - 3);

                                    if (parametersOut["lst"] == null)
                                    {
                                        parametersOut["lst"] = new SortedList();
                                    }

                                    if (parametersOut["lst"] != null)
                                    {
                                        SortedList stepList = (SortedList)parametersOut["lst"];

                                        if (!stepList.ContainsKey(Convert.ToInt32(strNumber)))
                                        {
                                            stepList[Convert.ToInt32(strNumber)] = new SortedList();
                                        }

                                        if (stepList.ContainsKey(Convert.ToInt32(strNumber)))
                                        {
                                            SortedList step = (SortedList)stepList[Convert.ToInt32(strNumber)];
                                            step["q"] = item.Value.ToString();
                                        }
                                    }


                                }
                                else if ((parametersOutMapping["Acst"] != null) &&
                                        (item.Key.ToString().Length >= 3) &&
                                        (item.Key.ToString().Substring(0, 3) == "stt"))
                                {
                                    strNumber = item.Key.ToString().Substring(3, item.Key.ToString().Length - 3);

                                    if (parametersOut["lst"] == null)
                                    {
                                        parametersOut["lst"] = new SortedList();
                                    }

                                    if (parametersOut["lst"] != null)
                                    {
                                        SortedList stepList = (SortedList)parametersOut["lst"];

                                        if (!stepList.ContainsKey(Convert.ToInt32(strNumber)))
                                        {
                                            stepList[Convert.ToInt32(strNumber)] = new SortedList();
                                        }

                                        if (stepList.ContainsKey(Convert.ToInt32(strNumber)))
                                        {
                                            SortedList step = (SortedList)stepList[Convert.ToInt32(strNumber)];
                                            step["t"] = item.Value.ToString();
                                        }
                                    }


                                }
                                else if ((parametersOutMapping["Acst"] != null) &&
                                        (item.Key.ToString().Length >= 3) &&
                                        (item.Key.ToString().Substring(0, 3) == "std"))
                                {
                                    strNumber = item.Key.ToString().Substring(3, item.Key.ToString().Length - 3);

                                    if (parametersOut["lst"] == null)
                                    {
                                        parametersOut["lst"] = new SortedList();
                                    }

                                    if (parametersOut["lst"] != null)
                                    {
                                        SortedList stepList = (SortedList)parametersOut["lst"];

                                        if (!stepList.ContainsKey(Convert.ToInt32(strNumber)))
                                        {
                                            stepList[Convert.ToInt32(strNumber)] = new SortedList();
                                        }

                                        if (stepList.ContainsKey(Convert.ToInt32(strNumber)))
                                        {
                                            SortedList step = (SortedList)stepList[Convert.ToInt32(strNumber)];
                                            step["d"] = item.Value.ToString();
                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            rtRes = rtM1Out;
                            Logger_AddLogMessage(string.Format("SendM1::Error In MessageOut = {0}", strM1Out), LoggerSeverities.Error);
                        }


                    }
                    else
                    {
                        rtRes = ResultType.Result_Error_OPS_Error;
                        Logger_AddLogMessage(string.Format("SendM1::Error Managing MessageIn = {0}", strM1In), LoggerSeverities.Error);
                    }
                }
                else
                {
                    rtRes = ResultType.Result_Error_OPS_Error;
                    Logger_AddLogMessage("SendM1::Error Generationg OPS M1 Message", LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                rtRes = ResultType.Result_Error_Generic;
                Logger_AddLogMessage("SendM1::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return rtRes;
        }


        private ResultType SendM2(SortedList parametersIn, Hashtable parametersInMapping, int iVirtualUnit, int nContractId = 0)
        {
            ResultType rtRes = ResultType.Result_OK;

            try
            {
                SortedList parametersM2In = new SortedList();

                foreach (DictionaryEntry item in parametersIn)
                {

                    if (parametersInMapping[item.Key.ToString()] != null)
                    {
                        parametersM2In[parametersInMapping[item.Key.ToString()]] = item.Value.ToString();
                    }
                }

                string strM2In = GenerateOPSMessage("m2", parametersM2In);

                if (strM2In.Length > 0)
                {

                    Logger_AddLogMessage(string.Format("SendM2::OPSMessageIn = {0}", strM2In), LoggerSeverities.Info);

                    string strM2Out = null;

                    if (OPSMessage(strM2In, iVirtualUnit, out strM2Out, nContractId))
                    {
                        Logger_AddLogMessage(string.Format("SendM2::OPSMessageOut = {0}", strM2Out), LoggerSeverities.Info);
                        SortedList parametersM2Out = new SortedList();
                        rtRes = FindOPSMessageOutputParameters(strM2Out, out parametersM2Out);

                        if (rtRes != ResultType.Result_OK)
                        {
                            Logger_AddLogMessage(string.Format("SendM2::Error In MessageOut = {0}", strM2Out), LoggerSeverities.Error);
                        }


                    }
                    else
                    {
                        rtRes = ResultType.Result_Error_OPS_Error;
                        Logger_AddLogMessage(string.Format("SendM2::Error Managing MessageIn = {0}", strM2In), LoggerSeverities.Error);
                    }
                }
                else
                {
                    rtRes = ResultType.Result_Error_OPS_Error;
                    Logger_AddLogMessage("SendM2::Error Generationg OPS M2 Message", LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                rtRes = ResultType.Result_Error_Generic;
                Logger_AddLogMessage("SendM2::Exception", LoggerSeverities.Error);
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


        private ResultType SendM4(SortedList parametersIn, Hashtable parametersInMapping, int iVirtualUnit, int nContractId = 0)
        {
            ResultType rtRes = ResultType.Result_OK;

            try
            {
                SortedList parametersM4In = new SortedList();

                foreach (DictionaryEntry item in parametersIn)
                {

                    if (parametersInMapping[item.Key.ToString()] != null)
                    {
                        parametersM4In[parametersInMapping[item.Key.ToString()]] = item.Value.ToString();
                    }
                }

                string strM4In = GenerateOPSMessage("m4", parametersM4In);

                if (strM4In.Length > 0)
                {

                    Logger_AddLogMessage(string.Format("SendM4::OPSMessageIn = {0}", strM4In), LoggerSeverities.Info);

                    string strM4Out = null;

                    if (OPSMessage(strM4In, iVirtualUnit, out strM4Out, nContractId))
                    {
                        Logger_AddLogMessage(string.Format("SendM4::OPSMessageOut = {0}", strM4Out), LoggerSeverities.Info);
                        SortedList parametersM4Out = new SortedList();
                        rtRes = FindOPSMessageOutputParameters(strM4Out, out parametersM4Out);

                        if (rtRes != ResultType.Result_OK)
                        {
                            Logger_AddLogMessage(string.Format("SendM4::Error In MessageOut = {0}", strM4Out), LoggerSeverities.Error);
                        }


                    }
                    else
                    {
                        rtRes = ResultType.Result_Error_OPS_Error;
                        Logger_AddLogMessage(string.Format("SendM4::Error Managing MessageIn = {0}", strM4In), LoggerSeverities.Error);
                    }
                }
                else
                {
                    rtRes = ResultType.Result_Error_OPS_Error;
                    Logger_AddLogMessage("SendM4::Error Generationg OPS M4 Message", LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                rtRes = ResultType.Result_Error_Generic;
                Logger_AddLogMessage("SendM4::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }


            return rtRes;
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


        /*private bool OPSMessage(string strMessageIn, int iVirtualUnit, out string strMessageOut)
        {
            bool bRdo = false;
            strMessageOut = null;

            try
            {

                IRecvMessage msg = null;
                //Logger_AddLogMessage("OPSMessage: OPS Msg In: " + strMessageIn.Replace("\n", ""), LoggerSeverities.Debug);

                msg = MessageFactory.GetReceivedMessage(strMessageIn);

                msg.Session = _msgSession;

                StringCollection sc = msg.Process();

                System.Collections.Specialized.StringEnumerator myEnumerator = sc.GetEnumerator();
                while (myEnumerator.MoveNext())
                    strMessageOut += myEnumerator.Current;

                if (strMessageOut != "")
                {
                    strMessageOut = strMessageOut.Replace("\r", "").Replace("\n", "");
                    string strIP = Context.Request.UserHostAddress;
                    LogMsgDB(strMessageIn, strMessageOut, iVirtualUnit);
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


        private bool OPSMessage(string strMessageIn, int iVirtualUnit, out string strMessageOut, int nContractId = 0)
        {
            bool bRdo = false;
            strMessageOut = null;

            try
            {
                Logger_AddLogMessage("OPSMessage::Using web service 1", LoggerSeverities.Info);
                OPSWebServices.Messages wsMessages = new OPSWebServices.Messages();

                string sUrl = ConfigurationManager.AppSettings["OPSWebServices.Messages"].ToString();
                if (nContractId > 0)
                    sUrl = ConfigurationManager.AppSettings["OPSWebServices.Messages" + nContractId.ToString()].ToString();
                if (sUrl == null)
                    throw new Exception("No web service url configuration");

                wsMessages.Url = sUrl;

                Logger_AddLogMessage("OPSMessage::Using web service: " + sUrl, LoggerSeverities.Info);

                // Eliminate invalid remote certificate error 
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

                strMessageOut = wsMessages.Message(strMessageIn);

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

        private bool OPSMessageNewVersion(string strMessageIn, int iVirtualUnit, out string strMessageOut, int nContractId = 0)
        {
            bool bRdo = false;
            strMessageOut = null;

            // *** This version works without the web service, but doesn't work correctly since it doesn't work for each contract individually 
            // (it doesn't point to each individual scheme, always OPSMUGIPARK).

            try
            {

                //if (Session["MessagesSession"] == null)
                //{
                //    MessagesSession msgSession = new MessagesSession();
                //    Session["MessagesSession"] = msgSession;
                //}

                IRecvMessage msg = null;
                msg = MessageFactory.GetReceivedMessage(strMessageIn);

                //msg.Session = ((MessagesSession)Session["MessagesSession"]);

                StringCollection sc = msg.Process();

                System.Collections.Specialized.StringEnumerator myEnumerator = sc.GetEnumerator();
                while (myEnumerator.MoveNext())
                    strMessageOut += myEnumerator.Current + "\n";



                //_webServiceFlag = 1 - _webServiceFlag;
                //if (_webServiceFlag == 0)
                //{
                //    Logger_AddLogMessage("OPSMessage::Using web service 1", LoggerSeverities.Info);
                //    OPSWS11Message.Messages wsMessages = new OPSWS11Message.Messages();

                //    string sUrl = ConfigurationManager.AppSettings["OPSWS11Message.Messages"].ToString();
                //    if (nContractId > 0)
                //        sUrl = ConfigurationManager.AppSettings["OPSWS11Message.Messages" + nContractId.ToString()].ToString();
                //    if (sUrl == null)
                //        throw new Exception("No web service 1 url configuration");

                //    wsMessages.Url = sUrl;

                //    Logger_AddLogMessage("OPSMessage::Using web service 1: " + sUrl, LoggerSeverities.Info);

                //    // Eliminate invalid remote certificate error 
                //    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

                //    strMessageOut = wsMessages.Message(strMessageIn);
                //}
                //else
                //{
                //    Logger_AddLogMessage("OPSMessage::Using web service 2", LoggerSeverities.Info);
                //    OPSWS11Message2.Messages wsMessages = new OPSWS11Message2.Messages();

                //    string sUrl = ConfigurationManager.AppSettings["OPSWS11Message2.Messages"].ToString();
                //    if (nContractId > 0)
                //        sUrl = ConfigurationManager.AppSettings["OPSWS11Message2.Messages" + nContractId.ToString()].ToString();
                //    if (sUrl == null)
                //        throw new Exception("No web service 2 url configuration");

                //    wsMessages.Url = sUrl;

                //    Logger_AddLogMessage("OPSMessage::Using web service 2: " + sUrl, LoggerSeverities.Info);

                //    // Eliminate invalid remote certificate error 
                //    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

                //    strMessageOut = wsMessages.Message(strMessageIn);
                //}

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

                        if (nodeLST.Name.Equals("contractlist"))
                        {
                            foreach (DictionaryEntry itemStepList in stepList)
                            {
                                XmlElement nodeStepList = xmlOutDoc.CreateElement("contract");
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
                        else if (nodeLST.Name.Equals("streetlist"))
                        {
                            foreach (DictionaryEntry itemStepList in stepList)
                            {
                                XmlElement nodeStepList = xmlOutDoc.CreateElement("street");
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

        private string GenerateXMLOuput(string strNode1, SortedList parametersOut1, string strNode2, SortedList parametersOut2, string strNode3, SortedList parametersOut3)
        {
            string xmlOut = "";
            try
            {
                XmlDocument xmlOutDoc = new XmlDocument();

                XmlElement root = xmlOutDoc.CreateElement(_xmlTagName + OUT_SUFIX);
                xmlOutDoc.AppendChild(root);
                XmlNode rootNode = xmlOutDoc.SelectSingleNode(_xmlTagName + OUT_SUFIX);

                XmlElement node1 = xmlOutDoc.CreateElement(strNode1);
                rootNode.AppendChild(node1);

                foreach (DictionaryEntry item in parametersOut1)
                {
                    if (item.Value.GetType() != typeof(SortedList))
                    {
                        XmlElement node = xmlOutDoc.CreateElement(item.Key.ToString());
                        node.InnerText = item.Value.ToString();
                        node1.AppendChild(node);
                    }
                    else
                    {
                        XmlElement nodeLST = xmlOutDoc.CreateElement(item.Key.ToString());
                        SortedList stepList = (SortedList)item.Value;
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

                        node1.AppendChild(nodeLST);
                    }
                }

                XmlElement node2 = xmlOutDoc.CreateElement(strNode2);
                rootNode.AppendChild(node2);

                foreach (DictionaryEntry item in parametersOut2)
                {
                    if (item.Value.GetType() != typeof(SortedList))
                    {
                        XmlElement node = xmlOutDoc.CreateElement(item.Key.ToString());
                        node.InnerText = item.Value.ToString();
                        node2.AppendChild(node);
                    }
                    else
                    {
                        XmlElement nodeLST = xmlOutDoc.CreateElement(item.Key.ToString());
                        SortedList stepList = (SortedList)item.Value;
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

                        node2.AppendChild(nodeLST);
                    }
                }

                if (strNode3.Length > 0)
                {
                    XmlElement node3 = xmlOutDoc.CreateElement(strNode3);
                    rootNode.AppendChild(node3);

                    foreach (DictionaryEntry item in parametersOut3)
                    {
                        if (item.Value.GetType() != typeof(SortedList))
                        {
                            XmlElement node = xmlOutDoc.CreateElement(item.Key.ToString());
                            node.InnerText = item.Value.ToString();
                            node3.AppendChild(node);
                        }
                        else
                        {
                            XmlElement nodeLST = xmlOutDoc.CreateElement(item.Key.ToString());
                            SortedList stepList = (SortedList)item.Value;
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

                            node3.AppendChild(nodeLST);
                        }
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



        private bool OperationAlreadyExists(string strPlate, string strDate, ref bool bExists, int nContractId = 0)
        {
            bExists = false;
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


                string strSQL = string.Format("SELECT COUNT(*) FROM OPERATIONS WHERE OPE_VEHICLEID='{0}' AND OPE_MOVDATE = to_date('{1}','hh24missddmmyy')", strPlate, strDate);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    bExists = (dataReader.GetInt32(0) > 0);
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

        private bool GetLastOperationGroups(string strPlate, string strDate, ref int nRotGroup, ref int nResGroup, ref int nRotOperId, ref int nResOperId, int nContractId = 0)
        {
            bool bResult = true;
            bool bDone = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            nRotGroup = -1;
            nResGroup = -1;

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

                string strSQL = string.Format("SELECT MAX(OPE_ID), OPE_DART_ID, OPE_GRP_ID FROM OPERATIONS WHERE OPE_VEHICLEID='{0}' AND OPE_DOPE_ID IN ({1}, {2}, {3}) AND OPE_DART_ID IN ({4},{5}) AND to_date('{6}','hh24missddmmyy') <= OPE_ENDDATE GROUP BY OPE_DART_ID, OPE_GRP_ID ORDER BY 2, 1 DESC, 3",
                    strPlate, ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString(), ConfigurationManager.AppSettings["OperationsDef.Extension"].ToString(), ConfigurationManager.AppSettings["OperationsDef.Refund"].ToString(), ConfigurationManager.AppSettings["ArticleType.RotList"].ToString(), ConfigurationManager.AppSettings["ArticleType.ResList"].ToString(), strDate);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read() && bDone == false)
                {
                    string[] rotList = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString().Split(new char[] { ',' });
                    string[] resList = ConfigurationManager.AppSettings["ArticleType.ResList"].ToString().Split(new char[] { ',' });

                    if (nRotGroup == -1)
                    {
                        if (rotList.Contains(dataReader.GetInt32(1).ToString()))
                        {
                            nRotGroup = dataReader.GetInt32(2);
                            nRotOperId = dataReader.GetInt32(0);
                        }
                        else if (resList.Contains(dataReader.GetInt32(1).ToString()))
                        {
                            nResGroup = dataReader.GetInt32(2);
                            nResOperId = dataReader.GetInt32(0);
                            bDone = true;
                        }
                    }
                    else if (nResGroup == -1)
                    {
                        if (resList.Contains(dataReader.GetInt32(1).ToString()))
                        {
                            nResGroup = dataReader.GetInt32(2);
                            nResOperId = dataReader.GetInt32(0);
                            bDone = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetLastOperationGroups::Exception", LoggerSeverities.Error);
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

        private bool GetLastOperation(string strPlate, string strDate, string strArticlesFilter, ref long lOperId, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
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

                string strSQL = string.Format("SELECT MAX(OPE_ID) FROM OPERATIONS WHERE OPE_VEHICLEID='{0}' AND OPE_DOPE_ID IN ({1}, {2}, {3}) AND OPE_DART_ID IN ({4}) AND to_date('{5}','hh24missddmmyy') <= OPE_ENDDATE",
                    strPlate, ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString(), ConfigurationManager.AppSettings["OperationsDef.Extension"].ToString(), ConfigurationManager.AppSettings["OperationsDef.Refund"].ToString(), strArticlesFilter, strDate);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    if (!dataReader.IsDBNull(0))
                        lOperId = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetLastOperation::Exception", LoggerSeverities.Error);
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

        private bool GetLastParkingOperation(string strPlate, string strDate, string strArticlesFilter, ref long lOperId, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
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

                string strSQL = string.Format("SELECT MAX(OPE_ID) FROM OPERATIONS WHERE OPE_VEHICLEID='{0}' AND OPE_DOPE_ID IN ({1}, {2}, {3}) AND OPE_DART_ID IN ({4})",
                    strPlate, ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString(), ConfigurationManager.AppSettings["OperationsDef.Extension"].ToString(), ConfigurationManager.AppSettings["OperationsDef.Refund"].ToString(), strArticlesFilter);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    if (!dataReader.IsDBNull(0))
                        lOperId = dataReader.GetInt32(0);
                }

                // Obtain information about the operation to determine if vehicle still has valid parking
                if (lOperId > 0)
                {
                    int nOperType = -1;
                    int nStatus = -1;
                    strSQL = string.Format("SELECT OPE_DOPE_ID, CASE WHEN (OPE_ENDDATE - to_date('{0}','hh24missddmmyy') > 0) THEN 2 ELSE 1 END FROM OPERATIONS WHERE OPE_ID = {1}", strDate, lOperId);
                    oraCmd.CommandText = strSQL;

                    if (dataReader != null)
                    {
                        dataReader.Close();
                        dataReader.Dispose();
                    }

                    dataReader = oraCmd.ExecuteReader();
                    if (dataReader.Read())
                    {
                        if (!dataReader.IsDBNull(0) && !dataReader.IsDBNull(1))
                        {
                            nOperType = dataReader.GetInt32(0);
                            nStatus = dataReader.GetInt32(1);

                            // The vehicle is only considered to be parked if the last operation is a parking or an extension and the date is valid
                            if ((nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Parking"]) || nOperType == Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Extension"]))
                                && (nStatus == Convert.ToInt32(ConfigurationManager.AppSettings["OperationStatus.Parked"])))
                            {
                                Logger_AddLogMessage(string.Format("GetLastParkingOperation::Vehicle is considered to be parked - Operation type: {0}, Status: {1}", nOperType.ToString(), nStatus.ToString()), LoggerSeverities.Info);
                            }
                            else
                            {
                                Logger_AddLogMessage(string.Format("GetLastParkingOperation::Vehicle is considered to be unparked - Operation type: {0}, Status: {1}", nOperType.ToString(), nStatus.ToString()), LoggerSeverities.Info);
                                lOperId = -1;
                            }
                        }
                        else
                        {
                            Logger_AddLogMessage(string.Format("GetLastParkingOperation::Error - could not obtain information about operation {0}", lOperId.ToString()), LoggerSeverities.Error);
                            lOperId = -1;
                        }
                    }
                }

                //string strSQL = string.Format("SELECT MAX(OPE_ID) FROM OPERATIONS WHERE OPE_VEHICLEID='{0}' AND OPE_DOPE_ID IN ({1}, {2}) AND OPE_DART_ID IN ({3}) AND to_date('{4}','hh24missddmmyy') <= OPE_ENDDATE",
                //    strPlate, ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString(), ConfigurationManager.AppSettings["OperationsDef.Extension"].ToString(), strArticlesFilter, strDate);
                //oraCmd.CommandText = strSQL;

                //dataReader = oraCmd.ExecuteReader();
                //if (dataReader.Read())
                //{
                //    if (!dataReader.IsDBNull(0))
                //        lOperId = dataReader.GetInt32(0);
                //}

                //// Check for a later refund operation related to the previously found one
                //if (lOperId > 0)
                //{
                //    strSQL = string.Format("SELECT MAX(OPE_MOVDATE), OPE_ID FROM OPERATIONS WHERE OPE_VEHICLEID='{0}' AND OPE_DOPE_ID = {1} AND OPE_DART_ID IN ({2}) AND OPE_BASE_OPE_ID = {3} AND OPE_ID <> {3} GROUP BY OPE_ID",
                //    strPlate, ConfigurationManager.AppSettings["OperationsDef.Refund"].ToString(), strArticlesFilter, lOperId);
                //    oraCmd.CommandText = strSQL;

                //    dataReader = oraCmd.ExecuteReader();
                //    if (dataReader.Read())
                //    {
                //        if (!dataReader.IsDBNull(0))
                //        {
                //            // If a refund operation is found, then the vehicle is not parked any more, and thus, no parking operation should be returned
                //            lOperId = dataReader.GetInt32(1);
                //            if (lOperId >= 0)
                //                lOperId = -1;
                //        }
                //    }
                //}
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetLastParkingOperation::Exception", LoggerSeverities.Error);
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

        private bool GetLastOperMobileUser(string strPlate, int nGroup, int nArticle, out int nMobileUserId, int nContractId = 0)
        {
            bool bResult = false;
            nMobileUserId = -1;
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

                string strSQL = string.Format("SELECT MAX(OPE_ID), NVL(OPE_MOBI_USER_ID, 0) FROM OPERATIONS WHERE OPE_VEHICLEID='{0}' AND OPE_DOPE_ID IN ({1}, {2}, {3}) ",
                    strPlate, ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString(), ConfigurationManager.AppSettings["OperationsDef.Extension"].ToString(), ConfigurationManager.AppSettings["OperationsDef.Refund"].ToString());
                if (nGroup > 0)
                    strSQL += string.Format("AND OPE_GRP_ID = {0} ", nGroup);
                if (nArticle > 0)
                    strSQL += string.Format("AND OPE_DART_ID = {0} ", nArticle);
                else
                    strSQL += "AND OPE_DART_ID IN (SELECT DART_ID FROM ARTICLES_DEF WHERE DART_REFUNDABLE = 1) ";
                strSQL += "GROUP BY OPE_MOBI_USER_ID ORDER BY 1 DESC";
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nMobileUserId = dataReader.GetInt32(1);

                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetLastOperMobileUser::Exception", LoggerSeverities.Error);
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


        private bool GetLastOperMobileUser(string strPlate, int nArticle, string strDate, out int nMobileUserId, int nContractId = 0)
        {
            bool bResult = true;
            nMobileUserId = -1;
            string strArticlesFilter = nArticle.ToString();
            long lOperId = -1;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                // First determine if there are any valid parking operations
                if (GetLastParkingOperation(strPlate, strDate, strArticlesFilter, ref lOperId, nContractId))
                {
                    // If a valid parking operation is found, obtain the user ID
                    if (lOperId > 0)
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

                        string strSQL = string.Format("SELECT NVL(OPE_MOBI_USER_ID, 0) FROM OPERATIONS WHERE OPE_ID = {0} ", lOperId);
                        oraCmd.CommandText = strSQL;

                        dataReader = oraCmd.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            dataReader.Read();
                            nMobileUserId = dataReader.GetInt32(0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetLastOperMobileUser::Exception", LoggerSeverities.Error);
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

        private bool GetLastFineMobileUser(string strFine, out int nMobileUserId, int nContractId = 0)
        {
            bool bResult = false;
            nMobileUserId = -1;
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

                string strSQL = string.Format("SELECT MAX(OPE_ID), NVL(OPE_MOBI_USER_ID, 0) FROM OPERATIONS WHERE OPE_FIN_ID='{0}' AND OPE_DOPE_ID = {1} GROUP BY OPE_MOBI_USER_ID ORDER BY 1 DESC",
                    strFine, ConfigurationManager.AppSettings["OperationsDef.Payment"].ToString());
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nMobileUserId = dataReader.GetInt32(1);

                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetLastFineMobileUser::Exception", LoggerSeverities.Error);
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

        private bool GetOperationGroup(long lOperId, ref int iGroupId, int nContractId = 0)
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

                string strSQL = string.Format("SELECT OPE_GRP_ID FROM OPERATIONS WHERE OPE_ID = {0}", lOperId);
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
                Logger_AddLogMessage("GetOperationGroup::Exception", LoggerSeverities.Error);
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

        private bool GetOperationType(long lOperId, ref int iType, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            iType = -1;

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

                string strSQL = string.Format("SELECT OPE_DOPE_ID FROM OPERATIONS WHERE OPE_ID = {0}", lOperId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    if (!dataReader.IsDBNull(0))
                        iType = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetOperationType::Exception", LoggerSeverities.Error);
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

        private bool GetOperationArticle(long lOperId, ref int iArticle, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            iArticle = -1;

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

                string strSQL = string.Format("SELECT OPE_DART_ID FROM OPERATIONS WHERE OPE_ID = {0}", lOperId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    if (!dataReader.IsDBNull(0))
                        iArticle = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetOperationArticle::Exception", LoggerSeverities.Error);
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

        private bool GetFineGroup(long lFineId, ref int iGroupId, int nContractId = 0)
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

                string strSQL = string.Format("SELECT FIN_GRP_ID_ZONE FROM FINES WHERE FIN_ID = {0}", lFineId);
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

        private bool GetOperMobileUserId(int nOperId, ref int nMobileUserId, int nContractId = 0)
        {
            nMobileUserId = -1;
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

                string strSQL = string.Format("SELECT NVL(OPE_MOBI_USER_ID,-1) FROM OPERATIONS WHERE OPE_ID = {0}", nOperId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nMobileUserId = dataReader.GetInt32(0);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetOperMobileUserId::Exception", LoggerSeverities.Error);
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

        private bool GetOperStatusData(long lOperId, out SortedList parametersOut, int nContractId = 0)
        {
            bool bResult = false;
            parametersOut = null;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            long lValue = -1;
            long lAvailTarId = -1;

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

                string strSQL = string.Format("SELECT OPE_DART_ID, OPE_DOPE_ID, TO_CHAR(OPE_INIDATE, 'hh24missddMMYY'), TO_CHAR(OPE_ENDDATE, 'hh24missddMMYY'), NVL(OPE_MOBI_USER_ID,-1), OPE_LATITUDE, OPE_LONGITUD, OPE_REFERENCE, OPE_GRP_ID, OPE_VALUE_VIS, OPE_DURATION, TO_CHAR(OPE_MOVDATE, 'hh24missddMMYY'), OPE_ADDR_STREET, OPE_ADDR_NUMBER FROM OPERATIONS WHERE OPE_ID = {0}", lOperId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    parametersOut["ad"] = dataReader.GetInt32(0).ToString();
                    parametersOut["o"] = dataReader.GetInt32(1).ToString();
                    parametersOut["di"] = dataReader.GetString(2);
                    parametersOut["df"] = dataReader.GetString(3);
                    parametersOut["mui"] = dataReader.GetInt32(4).ToString();
                    if (!dataReader.IsDBNull(5))
                        parametersOut["lt"] = dataReader.GetDouble(5).ToString().Replace(",", ".");
                    if (!dataReader.IsDBNull(6))
                        parametersOut["lg"] = dataReader.GetDouble(6).ToString().Replace(",", ".");
                    if (!dataReader.IsDBNull(7))
                        parametersOut["re"] = dataReader.GetString(7);
                    // Operation cannot be refundable if it wasn't performed online
                    if (Convert.ToInt32(parametersOut["mui"]) > 0)
                        parametersOut["rfd"] = IsTariffRefundable(dataReader.GetInt32(8), dataReader.GetInt32(0), nContractId);
                    else
                        parametersOut["rfd"] = "0";
                    parametersOut["g"] = dataReader.GetInt32(8);
                    parametersOut["aq"] = dataReader.GetInt32(9);
                    parametersOut["at"] = dataReader.GetInt32(10);
                    parametersOut["od"] = dataReader.GetString(11);
                    if (!dataReader.IsDBNull(12))
                        parametersOut["streetname"] = dataReader.GetString(12);
                    if (!dataReader.IsDBNull(13))
                        parametersOut["streetno"] = dataReader.GetString(13);
                    bResult = true;
                }

                if (bResult)
                {
                    bResult = false;

                    strSQL = string.Format("SELECT MAT_ID FROM MOBILE_AVAILABLE_TARIFFS WHERE MAT_DART_ID = {0} AND MAT_GRP_ID = {1}",
                        parametersOut["ad"], parametersOut["g"]);
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

                        if (!dataReader.IsDBNull(0))
                            lAvailTarId = dataReader.GetInt32(0);

                        if (lAvailTarId > 0)
                        {
                            parametersOut["id"] = lAvailTarId.ToString();
                            bResult = true;
                        }
                    }

                    // If available tariff was not found by searching by article ID then search by tariff value
                    if (lAvailTarId < 0)
                    {
                        strSQL = string.Format("SELECT MAT_ID FROM MOBILE_AVAILABLE_TARIFFS WHERE MAT_VALUE = {0} AND MAT_GRP_ID = {1}",
                            parametersOut["aq"], parametersOut["g"]);
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

                            if (!dataReader.IsDBNull(0))
                                lAvailTarId = dataReader.GetInt32(0);

                            if (lAvailTarId > 0)
                            {
                                parametersOut["id"] = lAvailTarId.ToString();
                                bResult = true;
                            }
                        }
                    }

                    if (lAvailTarId < 0)
                        Logger_AddLogMessage(string.Format("GetOperStatusData::Error - could not find tariff id: ArtId: {0}, Value: {1}", parametersOut["ad"].ToString(), parametersOut["aq"].ToString()), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetOperStatusData::Exception", LoggerSeverities.Error);
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

        private int IsTariffRefundable(int nGroupId, int nArticleId, int nContractId = 0)
        {
            int nRefundable = 0;
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

                string strSQL = string.Format("SELECT MAT_REFUNDABLE FROM MOBILE_AVAILABLE_TARIFFS WHERE MAT_GRP_ID = {0} AND MAT_DART_ID = {1}", nGroupId, nArticleId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nRefundable = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("IsTariffRefundable::Exception", LoggerSeverities.Error);
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

            return nRefundable;
        }

        private bool GetGroupParent(int nGroupId, ref int nParentId, int nContractId = 0)
        {
            nParentId = -1;
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

                string strSQL = string.Format("SELECT CGRP_ID FROM GROUPS_CHILDS WHERE CGRP_TYPE = 'G' AND CGRP_CHILD = {0}", nGroupId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nParentId = dataReader.GetInt32(0);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetGroupParent::Exception", LoggerSeverities.Error);
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

        private bool GetGroupName(int nGroupId, out string strGroupName, out string strGroupColor, int nContractId = 0)
        {
            strGroupName = "";
            strGroupColor = "673AB7";
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

                string strSQL = string.Format("SELECT GRP_DESCSHORT, GRP_COLOUR FROM GROUPS WHERE GRP_ID = {0}", nGroupId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strGroupName = dataReader.GetString(0);
                    strGroupColor = dataReader.GetString(1);
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

        private int IsResident(string strPlate, int nContractId = 0)
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

                string strSQL = string.Format("SELECT COUNT(*) FROM RESIDENTS WHERE RES_VEHICLEID = '{0}'", strPlate);
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
                Logger_AddLogMessage("IsResident::Exception", LoggerSeverities.Error);
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

        private int IsVip(string strPlate, int nContractId = 0)
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

                string strSQL = string.Format("SELECT COUNT(*) FROM VIPS WHERE VIP_VEHICLEID = '{0}'", strPlate);
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
                Logger_AddLogMessage("IsVip::Exception", LoggerSeverities.Error);
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

        private int GetMobileUserCredit(int nMobileUserId, ref int nCredit, int nContractId = 0)
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

                string strSQL = string.Format("SELECT MU_FUNDS FROM MOBILE_USERS WHERE MU_ID = {0}", nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nCredit = dataReader.GetInt32(0);
                    nResult = 1;
                }
                else
                    nResult = -20;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetMobileUserCredit::Exception", LoggerSeverities.Error);
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

        private bool GetAvailableTariffData(ref SortedList parametersOut, int nContractId = 0)
        {
            bool bResult = false;
            string strWhereList = "";
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                int nIndex = 0;
                foreach (string strValue in parametersOut.Values)
                {
                    if (nIndex++ > 0)
                        strWhereList += ", ";

                    strWhereList += strValue;
                }
                parametersOut.Clear();

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

                string strSQL = string.Format("SELECT MAT_ID, MAT_DESC, MAT_DART_ID, MAT_REFUNDABLE FROM MOBILE_AVAILABLE_TARIFFS WHERE MAT_ID IN ({0})", strWhereList);
                oraCmd.CommandText = strSQL;

                nIndex = 1;
                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    parametersOut["tarid" + nIndex.ToString()] = dataReader.GetInt32(0).ToString();
                    parametersOut["tardesc" + nIndex.ToString()] = dataReader.GetString(1);
                    parametersOut["tarad" + nIndex.ToString()] = dataReader.GetInt32(2).ToString();
                    parametersOut["tarrfd" + nIndex.ToString()] = dataReader.GetInt32(3).ToString();
                    nIndex++;
                }

                if (parametersOut.Count > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetAvailableTariffData::Exception", LoggerSeverities.Error);
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

        private bool GetAvailableTariffData(string strGroupList, string strArticleList, ref SortedList parametersOut, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                parametersOut.Clear();

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

                string strSQL = string.Format("SELECT MAT_ID, MAT_DESC, MAT_DART_ID, MAT_REFUNDABLE FROM MOBILE_AVAILABLE_TARIFFS WHERE MAT_DART_ID IN ({0})", strArticleList);
                if (strGroupList.Length > 0)
                    strSQL += string.Format(" AND MAT_GRP_ID IN ({0})", strGroupList);
                oraCmd.CommandText = strSQL;

                int nIndex = 1;
                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    parametersOut["tarid" + nIndex.ToString()] = dataReader.GetInt32(0).ToString();
                    parametersOut["tardesc" + nIndex.ToString()] = dataReader.GetString(1);
                    parametersOut["tarad" + nIndex.ToString()] = dataReader.GetInt32(2).ToString();
                    parametersOut["tarrfd" + nIndex.ToString()] = dataReader.GetInt32(3).ToString();
                    nIndex++;
                }

                if (parametersOut.Count > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetAvailableTariffData::Exception", LoggerSeverities.Error);
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

        private bool DetermineAvailableTariffs(int nRotGroup, int nResGroup, int nRotExtension, int nCurGroup, bool bIsResident, bool bIsVip, ref SortedList parametersOut, int nContractId = 0)
        {
            bool bResult = false;

            try
            {
                parametersOut = new SortedList();
                string strGroupList = "";
                string strArticlesList = "";

                if (nRotGroup == -1 && nResGroup == -1)     // No parking
                {
                    if (nCurGroup > 0)
                        strGroupList = nCurGroup.ToString();

                    if (bIsVip)
                    {
                        // Even though considered VIP, still offer the possibility to park as rotation
                        strArticlesList = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString() + ", " + ConfigurationManager.AppSettings["ArticleType.Vip"].ToString();
                    }
                    else if (bIsResident)
                    {
                        // Even though considered resident, still offer the possibility to park as rotation
                        strArticlesList = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString() + ", " + ConfigurationManager.AppSettings["ArticleType.ResList"].ToString();
                    }
                    else
                    {
                        strArticlesList = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString();
                    }
                }
                else if (nRotGroup != -1 && nResGroup == -1)    // Only Rotation parking
                {
                    strGroupList = nRotGroup.ToString();
                    if (bIsVip)
                    {
                        // Even though considered VIP, still offer the possibility to park as rotation
                        strArticlesList = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString() + ", " + ConfigurationManager.AppSettings["ArticleType.Vip"].ToString();
                    }
                    else
                    {
                        strArticlesList = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString();
                    }
                }
                else if (nRotGroup == -1 && nResGroup != -1)    // Only Resident parking
                {
                    strGroupList = nResGroup.ToString();
                    if (bIsResident)
                    {
                        // Even though considered resident, still offer the possibility to park as rotation
                        strArticlesList = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString() + ", " + ConfigurationManager.AppSettings["ArticleType.ResList"].ToString();
                    }
                    else
                    {
                        strArticlesList = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString();
                    }
                }
                else     // Both Rotation and Resident parking
                {
                    strGroupList = nRotGroup.ToString() + ", " + nResGroup.ToString();
                    if (bIsVip)
                    {
                        // Even though considered VIP, still offer the possibility to park as rotation
                        strArticlesList = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString() + ", " + ConfigurationManager.AppSettings["ArticleType.Vip"].ToString();
                    }
                    else if (bIsResident)
                    {
                        // Even though considered resident, still offer the possibility to park as rotation
                        strArticlesList = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString() + ", " + ConfigurationManager.AppSettings["ArticleType.ResList"].ToString();
                    }
                    else
                    {
                        strArticlesList = ConfigurationManager.AppSettings["ArticleType.RotList"].ToString();
                    }
                }

                if (GetAvailableTariffData(strGroupList, strArticlesList, ref parametersOut, nContractId))
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("DetermineAvailableTariffs::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return bResult;
        }

        private bool GetEquivalentArticle(int nArticleId, ref int nEquivArticleId, int nContractId = 0)
        {
            nEquivArticleId = -1;
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

                string strSQL = string.Format("SELECT DART_REQUIRED FROM ARTICLES_DEF WHERE DART_ID = {0}", nArticleId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nEquivArticleId = dataReader.GetInt32(0);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetEquivalentArticle::Exception", LoggerSeverities.Error);
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

        private bool GetArticleMaxTime(int nArticleId, int nGroupId, ref int nMaxTime, int nContractId = 0)
        {
            nMaxTime = -1;
            int nConstraintId = -1;

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

                string strSQL = string.Format("SELECT RUL_CON_ID FROM ARTICLES_RULES WHERE RUL_DART_ID = {0} AND RUL_GRP_ID = {1}", nArticleId, nGroupId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nConstraintId = dataReader.GetInt32(0);
                }

                if (nConstraintId > 0)
                {
                    strSQL = string.Format("SELECT CON_VALUE FROM CONSTRAINTS WHERE CON_ID = {0} AND CON_DCON_ID = {1}", nConstraintId, ConfigurationManager.AppSettings["Constraints.MaxEstancia"].ToString());
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
                        nMaxTime = dataReader.GetInt32(0);
                        bResult = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetArticleMaxTime::Exception", LoggerSeverities.Error);
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

        private bool UpdateOperationData(SortedList ParametersIn, out long lOperId, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleCommand oraCmd2 = null;
            OracleCommand oraCmd3 = null;
            OracleConnection oraConn = null;
            OracleConnection oraConn2 = null;

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

                string strSQL = string.Format("SELECT MAX(OPE_ID) FROM OPERATIONS WHERE OPE_VEHICLEID='{0}' AND OPE_MOVDATE = to_date('{1}','hh24missddmmyy')", ParametersIn["p"].ToString(), ParametersIn["d"].ToString());
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    if (!dataReader.IsDBNull(0))
                        lOperId = dataReader.GetInt32(0);
                }

                if (lOperId > 0)
                {
                    string strUpdate = "";

                    if (ParametersIn["lt"] != null && ParametersIn["lg"] != null && ParametersIn["lt"].ToString().Length > 0 && ParametersIn["lg"].ToString().Length > 0 && !ParametersIn["lt"].ToString().Equals("undefined") && !ParametersIn["lg"].ToString().Equals("undefined"))
                        strUpdate = " OPE_LATITUDE = " + ParametersIn["lt"].ToString() + ", OPE_LONGITUD = " + ParametersIn["lg"].ToString();

                    if (ParametersIn["re"] != null && ParametersIn["re"].ToString().Length > 0)
                    {
                        if (strUpdate.Length > 0)
                            strUpdate += ", OPE_REFERENCE = '" + ParametersIn["re"].ToString() + "' ";
                        else
                            strUpdate = " OPE_REFERENCE = '" + ParametersIn["re"].ToString() + "' ";
                    }

                    if (ParametersIn["spcid"] != null && ParametersIn["spcid"].ToString().Length > 0)
                    {
                        if (strUpdate.Length > 0)
                            strUpdate += ", OPE_PS_ID = " + ParametersIn["spcid"].ToString() + " ";
                        else
                            strUpdate = " OPE_PS_ID = " + ParametersIn["spcid"].ToString() + " ";
                    }

                    if (ParametersIn["streetname"] != null && ParametersIn["streetname"].ToString().Length > 0)
                    {
                        if (strUpdate.Length > 0)
                            strUpdate += ", OPE_ADDR_STREET = '" + ParametersIn["streetname"].ToString() + "' ";
                        else
                            strUpdate = " OPE_ADDR_STREET = '" + ParametersIn["streetname"].ToString() + "' ";
                    }

                    if (ParametersIn["streetno"] != null && ParametersIn["streetno"].ToString().Length > 0)
                    {
                        if (strUpdate.Length > 0)
                            strUpdate += ", OPE_ADDR_NUMBER = '" + ParametersIn["streetno"].ToString() + "' ";
                        else
                            strUpdate = " OPE_ADDR_NUMBER = '" + ParametersIn["streetno"].ToString() + "' ";
                    }

                    if (strUpdate.Length > 0)
                    {
                        oraCmd2 = new OracleCommand();
                        oraCmd2.Connection = oraConn;

                        if (oraCmd2 == null)
                            throw new Exception("Oracle command is null");

                        strSQL = string.Format("UPDATE OPERATIONS SET {0} WHERE OPE_ID='{1}'", strUpdate, lOperId);
                        oraCmd2.CommandText = strSQL;
                        oraCmd2.ExecuteNonQuery();
                        bResult = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("UpdateOperationData::Exception", LoggerSeverities.Error);
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

                if (oraCmd2 != null)
                {
                    oraCmd2.Dispose();
                    oraCmd2 = null;
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

        private bool UpdateOperationPlateData(SortedList ParametersIn, long lOperId, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd3 = null;
            OracleConnection oraConn = null;

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                if (nContractId > 0)
                    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                if (sConn == null)
                    throw new Exception("No ConnectionString configuration");

                oraConn = new OracleConnection(sConn);

                string strUpdate = "";

                if (ParametersIn["cid"] != null && ParametersIn["cid"].ToString().Length > 0)
                    strUpdate = " MUP_CLOUD_TOKEN = '" + ParametersIn["cid"].ToString() + "' ";

                if (ParametersIn["os"] != null && ParametersIn["os"].ToString().Length > 0)
                {
                    if (strUpdate.Length > 0)
                        strUpdate += ", MUP_DEVICE_OS = " + ParametersIn["os"].ToString();
                    else
                        strUpdate = " MUP_DEVICE_OS = " + ParametersIn["os"].ToString();
                }

                if (strUpdate.Length > 0)
                {
                    oraConn = new OracleConnection(sConn);

                    oraCmd3 = new OracleCommand();
                    oraCmd3.Connection = oraConn;
                    oraCmd3.Connection.Open();

                    if (oraCmd3 == null)
                        throw new Exception("Oracle command is null");

                    string strSQL = string.Format("UPDATE MOBILE_USERS_PLATES SET {0} WHERE MUP_MU_ID = {1} AND MUP_PLATE = '{2}'", strUpdate, ParametersIn["mui"].ToString(), ParametersIn["p"].ToString());
                    oraCmd3.CommandText = strSQL;
                    oraCmd3.ExecuteNonQuery();
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("UpdateOperationPlateData::Exception", LoggerSeverities.Error);
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

                if (oraCmd3 != null)
                {
                    oraCmd3.Dispose();
                    oraCmd3 = null;
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

        private bool GetFile(string strFileId, out SortedList parametersOut, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            parametersOut = new SortedList();

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

                string strSQL = string.Format("SELECT MF_CONTENT FROM MOBILE_FILES WHERE MF_ID = '{0}'", strFileId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    parametersOut["fc"] = dataReader.GetString(0);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetFile::Exception", LoggerSeverities.Error);
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

        private bool UpdateFile(string strFileId, string strFileContents, int nContractId = 0)
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

                string strSQL = string.Format("SELECT COUNT(*) FROM MOBILE_FILES WHERE MF_ID = '{0}'", strFileId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    int iCount = dataReader.GetInt32(0);
                    if (iCount > 0)
                        bResult = true;
                }

                if (bResult)
                {
                    strSQL = string.Format("update mobile_files set mf_content = '{0}', mf_date = sysdate where mf_id = '{1}'",
                    strFileContents, strFileId);

                    oraCmd.CommandText = strSQL;

                    if (oraCmd.ExecuteNonQuery() > 0)
                        bResult = true;
                }
                else
                {
                    strSQL = string.Format("insert into mobile_files (mf_id, mf_content, mf_date) values ('{0}','{1}',sysdate)",
                    strFileId, strFileContents);

                    oraCmd.CommandText = strSQL;

                    if (oraCmd.ExecuteNonQuery() > 0)
                        bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("UpdateFile::Exception", LoggerSeverities.Error);
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

        private bool GetSpaceStatus(long lSpaceId, ref int iStatus, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            iStatus = -1;

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

                string strSQL = string.Format("SELECT PS_STATE FROM PARKING_SPACES WHERE PS_ID = {0}", lSpaceId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    if (!dataReader.IsDBNull(0))
                        iStatus = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetSpaceStatus::Exception", LoggerSeverities.Error);
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

        private bool UpdateSpaceStatus(long lSpaceId, int iStatus, int nContractId = 0)
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

                string strSQL = string.Format("update parking_spaces set ps_state = {0}, ps_date_mod = sysdate where ps_id = {1}",
                iStatus, lSpaceId);

                oraCmd.CommandText = strSQL;

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("UpdateSpaceStatus::Exception", LoggerSeverities.Error);
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

        private bool UpdateSpaceStatus(long lSpaceId, int iStatus, string strEndDate, int nContractId = 0)
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

                string strSQL = string.Format("update parking_spaces set ps_state = {0}, ps_date_mod = to_date( '{1}', 'hh24missddmmyy' ), ps_end_date = to_date( '{1}', 'hh24missddmmyy' ) where ps_id = {2}",
                iStatus, strEndDate, lSpaceId);

                oraCmd.CommandText = strSQL;

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("UpdateSpaceStatus::Exception", LoggerSeverities.Error);
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

        private int GetSpacesBonus(int nContractId = 0)
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

                string strSQL = string.Format("SELECT NVL(PAR_VALUE,0) FROM PARAMETERS WHERE PAR_DESCSHORT = '{0}'", PARM_SPACES_BONUS);
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
                Logger_AddLogMessage("GetSpacesBonus::Exception", LoggerSeverities.Error);
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

        private bool UpdateUserSpaceNotifications(int iUserId, int iNumNotif, int nContractId = 0)
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
                if (iNumNotif > 0)
                    strSQL = string.Format("update mobile_users set mu_num_shared_spaces = mu_num_shared_spaces + {0} where mu_id = {1}", iNumNotif, iUserId);
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

        private bool GetSpaceIdOperation(long lOperId, ref long lSpaceId, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            lSpaceId = -1;

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

                string strSQL = string.Format("SELECT OPE_PS_ID FROM OPERATIONS WHERE OPE_ID = {0}", lOperId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    if (!dataReader.IsDBNull(0))
                        lSpaceId = dataReader.GetInt64(0);
                    if (lSpaceId > 0)
                        bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetSpaceIdOperation::Exception", LoggerSeverities.Error);
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

        private int DoesParkingSpaceExist(string strLattitude, string strLongitude, int nContractId = 0)
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

                string strSQL = string.Format("SELECT MAX(PS_ID) FROM PARKING_SPACES WHERE PS_LATTITUDE = {0} AND PS_LONGITUDE = {1}", strLattitude, strLongitude);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    if (!dataReader.IsDBNull(0))
                        nResult = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("DoesParkingSpaceExist::Exception", LoggerSeverities.Error);
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

        private int AddParkingSpace(string strGroup, string strLattitude, string strLongitude, int iStatus, int nContractId = 0)
        {
            int nSpaceId = (int)ResultType.Result_Error_Generic;

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

                string strSQL1 = " insert into PARKING_SPACES (ps_state, ps_grp_id, ps_lattitude, ps_longitude, ps_date_mod";
                string strSQL2 = " ) VALUES( " + iStatus.ToString() + ", " + strGroup + ", " + strLattitude + ", " + strLongitude + ", SYSDATE";
                strSQL2 += ") returning PS_ID into :nReturnValue";

                oraCmd.CommandText = strSQL1 + strSQL2;

                oraCmd.Parameters.Add(new OracleParameter("nReturnValue",OracleDbType.Int32));
                oraCmd.Parameters["nReturnValue"].Direction = System.Data.ParameterDirection.ReturnValue;

                oraCmd.ExecuteNonQuery();

                nSpaceId = (int)oraCmd.Parameters["nReturnValue"].Value;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("AddParkingSpace::Exception", LoggerSeverities.Error);
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

            return nSpaceId;
        }

        private bool AddBonusOperation(string strGroupId, int iUnitId, int iAmount, string strMobileUserId, out int iOperId, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            iOperId = -1;

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
                    iOperId = dataReader.GetInt32(0);
                }

                if (iOperId > 0)
                {
                    strSQL = string.Format("INSERT INTO OPERATIONS (OPE_ID, OPE_DOPE_ID, OPE_GRP_ID, OPE_UNI_ID, OPE_DPAY_ID, OPE_MOVDATE, OPE_VALUE, OPE_DART_ID, OPE_MOBI_USER_ID, OPE_DOPE_ID_VIS, OPE_OP_ONLINE, OPE_INSDATE, OPE_DPAY_ID_VIS, OPE_VALUE_VIS, OPE_RECHARGE_TYPE) VALUES ({0}, {1}, {2}, {3}, {4}, SYSDATE, {5}, {6}, {7}, {8}, {9}, SYSDATE, {10}, {11}, {12})",
                        iOperId.ToString(), ConfigurationManager.AppSettings["OperationsDef.Recharge"].ToString(), strGroupId, iUnitId.ToString(), ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString(), iAmount.ToString(), ConfigurationManager.AppSettings["ArticleType.Default"].ToString(), strMobileUserId, ConfigurationManager.AppSettings["OperationsDef.Recharge"].ToString(), ConfigurationManager.AppSettings["Operation.Online"].ToString(), ConfigurationManager.AppSettings["PayTypesDef.WebPayment"].ToString(), iAmount.ToString(), ConfigurationManager.AppSettings["RechargeType.Bonus"].ToString());
                    oraCmd.CommandText = strSQL;

                    if (oraCmd.ExecuteNonQuery() > 0)
                        bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("AddBonusOperation::Exception", LoggerSeverities.Error);
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

        bool SendEmail(string strDestAddress, string strSubject, string strFilePath)
        {
            bool bResult = false;

            try
            {
                MailMessage MyMailMessage = new MailMessage();
                MyMailMessage.From = new MailAddress(ConfigurationManager.AppSettings["SendAddress"].ToString());
                MyMailMessage.To.Add(strDestAddress);
                MyMailMessage.Subject = strSubject;
                if (strFilePath.Length > 0)
                {
                    Attachment attachFile = new Attachment(strFilePath);
                    MyMailMessage.Attachments.Add(attachFile);
                }

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

        private bool GetMobileUserEmail(int nMobileUserId, ref string strEmail, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                strEmail = "";

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

                string strSQL = string.Format("SELECT MU_EMAIL FROM MOBILE_USERS WHERE MU_ID = {0}", nMobileUserId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strEmail = dataReader.GetString(0);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetMobileUserEmail::Exception", LoggerSeverities.Error);
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

        private bool GetContractNames(out string strName1, out string strName2, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            strName1 = "";
            strName2 = "";

            try
            {
                string sConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                // Don't use this since this data is only stored in main DB scheme
                //if (nContractId > 0)
                //    sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
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

                string strSQL = string.Format("SELECT MCON_DESC1, MCON_DESC2 FROM MOBILE_CONTRACTS WHERE MCON_ID = {0}", nContractId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    strName1 = dataReader.GetString(0);
                    strName2 = dataReader.GetString(1);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetContractNames::Exception", LoggerSeverities.Error);
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

        private bool IsPointInPolygon(List<Loc> poly, Loc point)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if ((((poly[i].Lt <= point.Lt) && (point.Lt < poly[j].Lt))
                        || ((poly[j].Lt <= point.Lt) && (point.Lt < poly[i].Lt)))
                        && (point.Lg < (poly[j].Lg - poly[i].Lg) * (point.Lt - poly[i].Lt)
                            / (poly[j].Lt - poly[i].Lt) + poly[i].Lg))

                    c = !c;
            }

            return c;
        }

        private Loc LocateGPSFromAddressLocal(string strStreet, string strNumber, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            Loc curLocation = null;

            try
            {
                int nDateRange = Convert.ToInt32(ConfigurationManager.AppSettings["Place.DateRange"].ToString());

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

                string strSQL = string.Format("SELECT MLC_LATITUDE, MLC_LONGITUDE FROM MOBILE_LOCATION_CACHE WHERE MLC_ADDR_STREET = '{0}' and MLC_ADDR_NUMBER = '{1}' AND SYSDATE - MLC_DATE < {2}",
                    strStreet, strNumber, nDateRange);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    string strLattitude = dataReader.GetDouble(0).ToString();
                    string strLongitude = dataReader.GetDouble(1).ToString();

                    if (Convert.ToInt32(ConfigurationManager.AppSettings["GlobalizeCommaGPS"]) == 1)
                    {
                        strLattitude = strLattitude.ToString().Replace(".", ",");
                        strLongitude = strLongitude.ToString().Replace(".", ",");
                    }
                    curLocation = new Loc(Convert.ToDouble(strLattitude), Convert.ToDouble(strLongitude));
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage(string.Format("LocateGPSFromAddressLocal::Error - {0}, Address - {1}", e.Message, strStreet + " " + strNumber), LoggerSeverities.Error);
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

            return curLocation;
        }

        private bool GetStretchAreasToSearch(out List<int> stretchList, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            stretchList = new List<int>();

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

                // Get a list of all the stretches that have defined areas
                string strSQL = string.Format("SELECT DISTINCT( MSA_MSS_ID ) FROM MOBILE_STRETCH_AREAS ORDER BY MSA_MSS_ID");
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    int iStretch = dataReader.GetInt32(0);
                    stretchList.Add(iStretch);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetStretchAreasToSearch::Exception", LoggerSeverities.Error);
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

        private bool GetStretchAreas(int iStretchId, out List<Loc> areaList, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            areaList = new List<Loc>();

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

                // Get a list of all the coordinates for the stretch area
                string strSQL = string.Format("SELECT MSA_COORDINATES FROM MOBILE_STRETCH_AREAS WHERE MSA_MSS_ID = {0}", iStretchId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    string strCoordinates = dataReader.GetString(0);
                    string[] strPoints = strCoordinates.Split(new char[] { ',' });
                    if (Convert.ToInt32(ConfigurationManager.AppSettings["GlobalizeCommaGPS"]) == 1)
                    {
                        strPoints[0] = strPoints[0].ToString().Replace(".", ",");
                        strPoints[1] = strPoints[1].ToString().Replace(".", ",");
                    }
                    areaList.Add(new Loc(Convert.ToDouble(strPoints[0]), Convert.ToDouble(strPoints[1])));
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetStretchAreas::Exception", LoggerSeverities.Error);
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


        private bool GetStretchData(int nStretchId, out int nGroupId, out int nStreetId, out int nStreetNo, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            nGroupId = -1;
            nStreetId = -1;
            nStreetNo = -1;

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

                string strSQL = string.Format("SELECT MSS_GRP_ID, MSS_MSTR_ID, MSS_STR_NUM_MIN FROM MOBILE_STREETS_STRETCHES WHERE MSS_ID = {0}",
                    nStretchId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    nGroupId = dataReader.GetInt32(0);
                    nStreetId = dataReader.GetInt32(1);
                    nStreetNo = dataReader.GetInt32(2);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage(string.Format("GetStretchData::Error - {0}", e.Message), LoggerSeverities.Error);
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

        private Loc LocateGPSFromAddressLocal(int nLocationId, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            Loc curLocation = null;

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

                string strSQL = string.Format("SELECT MLC_LATITUDE, MLC_LONGITUDE FROM MOBILE_LOCATION_CACHE WHERE MLC_ID = {0}",
                    nLocationId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    string strLattitude = dataReader.GetDouble(0).ToString();
                    string strLongitude = dataReader.GetDouble(1).ToString();

                    if (Convert.ToInt32(ConfigurationManager.AppSettings["GlobalizeCommaGPS"]) == 1)
                    {
                        strLattitude = strLattitude.ToString().Replace(".", ",");
                        strLongitude = strLongitude.ToString().Replace(".", ",");
                    }
                    curLocation = new Loc(Convert.ToDouble(strLattitude), Convert.ToDouble(strLongitude));
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage(string.Format("LocateGPSFromAddressLocal::Error - {0}, Location ID - {1}", e.Message, nLocationId), LoggerSeverities.Error);
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

            return curLocation;
        }

        private Loc LocateGPSFromAddress(string query)
        {
            string strApiKey = ConfigurationManager.AppSettings["Place.ApiKey"].ToString();

            WebRequest request = WebRequest
               .Create("https://maps.googleapis.com/maps/api/geocode/xml?sensor=false&address="
                  + HttpUtility.UrlEncode(query) + "&key=" + strApiKey);

            Logger_AddLogMessage(string.Format("LocateGPSFromAddress::Web Request = {0}", request.RequestUri.ToString()), LoggerSeverities.Error);

            try
            {
                // Eliminate invalid remote certificate error 
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        Logger_AddLogMessage(stream.ToString(), LoggerSeverities.Info);

                        XDocument document = XDocument.Load(new StreamReader(stream));

                        XElement longitudeElement = document.Descendants("lng").FirstOrDefault();
                        XElement latitudeElement = document.Descendants("lat").FirstOrDefault();

                        if (longitudeElement != null && latitudeElement != null)
                        {
                            return new Loc(Double.Parse(latitudeElement.Value, CultureInfo.InvariantCulture), Double.Parse(longitudeElement.Value, CultureInfo.InvariantCulture));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage(string.Format("LocateGPSFromAddress::Error - {0}, Web Request = {1}", e.Message, request.RequestUri.ToString()), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return null;
        }

        private bool LocateAddressFromGPS(Loc location, bool bInterpolate, out string strStreet, out string strNumber, out string strCity)
        {
            bool bResult = false;
            string strApiKey = ConfigurationManager.AppSettings["Place.ApiKey"].ToString();

            strStreet = "";
            strNumber = "";
            strCity = "";

            string strRequest = ("https://maps.googleapis.com/maps/api/geocode/xml?result_type=street_address&latlng="
                    + location.Lt.ToString().Replace(",", ".") + "," + location.Lg.ToString().Replace(",", ".") + "&key=" + strApiKey);
            if (bInterpolate)
                strRequest += "&location_type=RANGE_INTERPOLATED";
            WebRequest request = WebRequest.Create(strRequest);

            Logger_AddLogMessage(string.Format("LocateAddressFromGPS::Web Request = {0}", request.RequestUri.ToString()), LoggerSeverities.Error);

            try
            {
                // Eliminate invalid remote certificate error 
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        XmlDocument document = new XmlDocument();
                        document.Load(new StreamReader(stream));

                        XmlNode statusNode = document.SelectSingleNode("//GeocodeResponse/status");
                        string strStatus = statusNode.InnerText.Trim();
                        if (strStatus.Equals("OK"))
                        {
                            XmlNode streetNode = document.SelectSingleNode("//GeocodeResponse/result/address_component[type='route']");
                            strStreet = streetNode.FirstChild.InnerText;

                            XmlNode numberNode = document.SelectSingleNode("//GeocodeResponse/result/address_component[type='street_number']");
                            strNumber = numberNode.FirstChild.InnerText;

                            try
                            {
                                XmlNode cityNode = document.SelectSingleNode("//GeocodeResponse/result/address_component[type='locality']");
                                strCity = cityNode.FirstChild.InnerText;
                            }
                            catch(Exception ex)
                            {
                                Logger_AddLogMessage("LocateAddressFromGPS::City not fount", LoggerSeverities.Error);
                            }

                            bResult = true;
                        }
                        else
                        {
                            Logger_AddLogMessage(string.Format("LocateAddressFromGPS::Error - Google Response - {0}, Web Request = {1}", strStatus, request.RequestUri.ToString()), LoggerSeverities.Error);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage(string.Format("LocateAddressFromGPS::Error - {0}, Web Request = {1}", e.Message, request.RequestUri.ToString()), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            return bResult;
        }

        private bool LocateAddressFromGPSLocal(Loc location, out string strStreet, out string strNumber, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            string strLat = "";
            string strLong = "";

            strStreet = "";
            strNumber = "";

            try
            {
                int nDateRange = Convert.ToInt32(ConfigurationManager.AppSettings["Place.DateRange"].ToString());

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

                if (Convert.ToInt32(ConfigurationManager.AppSettings["GlobalizeCommaGPS"]) == 1)
                {
                    strLat = location.Lt.ToString().Replace(",", ".");
                    strLong = location.Lg.ToString().Replace(",", ".");
                }
                else
                {
                    strLat = location.Lt.ToString();
                    strLong = location.Lg.ToString();
                }

                string strSQL = string.Format("SELECT MLC_ADDR_STREET, MLC_ADDR_NUMBER FROM MOBILE_LOCATION_CACHE WHERE MLC_LATITUDE = {0} AND MLC_LONGITUDE = {1} AND SYSDATE - MLC_DATE < {2}",
                    strLat, strLong, nDateRange);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    strStreet = dataReader.GetString(0);
                    strNumber = dataReader.GetString(1);

                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage(string.Format("LocateAddressFromGPSLocal::Error - {0}, Address - {1}", e.Message, strStreet + " " + strNumber), LoggerSeverities.Error);
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

        private int LocatePlace(string strQuery, Loc location, out string strResults)
        {
            int nResult = 0;
            strResults = "";
            string strRequest = "";

            try
            {
                string strRadius = ConfigurationManager.AppSettings["Place.Radius"].ToString();
                string strApiKey = ConfigurationManager.AppSettings["Place.ApiKey"].ToString();
                strRequest = "https://maps.googleapis.com/maps/api/place/autocomplete/json?input=" + strQuery + "&types=address&location=" + location.Lt.ToString().Replace(",", ".") + ","
                   + location.Lg.ToString().Replace(",", ".") + "&radius=" + strRadius + "&components=country:es&strictbounds&key=" + strApiKey;

                WebRequest request = WebRequest
                   .Create(strRequest);

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader srReader = new StreamReader(stream);
                        strResults = srReader.ReadToEnd();

                        dynamic json = JsonConvert.DeserializeObject(strResults);
                        var queryStatus = (string)json["status"];
                        if (queryStatus.ToString().Equals("OK"))
                            nResult = 1;
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("LocatePlace::Exception " + strRequest, LoggerSeverities.Error);
                Logger_AddLogException(e);
                nResult = -1;
            }

            return nResult;
        }

        private bool AddResultsToCache(string strSearchMask, string strResponse, int nContractId)
        {
            try
            {
                string strContractName = "";
                GetParameter("P_SYSTEM_NAME", out strContractName, nContractId);

                if (strContractName.Length == 0)
                    throw (new Exception("Could not obtain contract name"));

                dynamic json = JsonConvert.DeserializeObject(strResponse);

                foreach (var prediction in json["predictions"])
                {
                    var secText = (string)prediction["structured_formatting"]["secondary_text"];
                    if (secText.ToString().ToUpper().Contains(strContractName.ToUpper()))
                    {
                        var placeId = (string)prediction["id"];
                        string strPrediction = JsonConvert.SerializeObject(prediction);
                        AddPredictionToCache(strSearchMask, placeId.ToString(), strPrediction, nContractId);
                    }
                    //foreach (var terms in prediction["terms"])
                    //{
                    //    var temp = (string)terms["value"];
                    //    if (temp.ToString().ToUpper().Contains(strContractName.ToUpper()))
                    //    {
                    //        string strPrediction = JsonConvert.SerializeObject(prediction);
                    //        AddPredictionToCache(strSearchMask, strPrediction, nContractId);
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("FilterResults::Exception", LoggerSeverities.Error);
                Logger_AddLogException(e);
                return false;
            }

            return true;
        }

        private bool AddPredictionToCache(string strSearchMask, string strPlaceId, string strPrediction, int nContractId = 0)
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

                string strSQL = string.Format("INSERT INTO MOBILE_GPS_CACHE (MGC_CONTRACT_ID, MGC_SEARCH, MGC_RESULT, MGC_PLACE_ID) VALUES ({0}, '{1}', '{2}', '{3}')",
                    nContractId, strSearchMask, strPrediction, strPlaceId);
                oraCmd.CommandText = strSQL;

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("AddPredictionToCache::Exception", LoggerSeverities.Error);
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

        private bool AddLocationToCache(string strLattitude, string strLongitude, string strStreet, string strNumber, int nContractId = 0)
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

                string strSQL = string.Format("INSERT INTO MOBILE_LOCATION_CACHE (MLC_CONTRACT_ID, MLC_LATITUDE, MLC_LONGITUDE, MLC_ADDR_STREET, MLC_ADDR_NUMBER) VALUES ({0}, {1}, {2}, '{3}', '{4}')",
                    nContractId, strLattitude, strLongitude, strStreet, strNumber);
                oraCmd.CommandText = strSQL;

                if (oraCmd.ExecuteNonQuery() > 0)
                    bResult = true;
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("AddLocationToCache::Exception", LoggerSeverities.Error);
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

        private int GetLocationIdFromCache(string strStreet, string strNumber, int nContractId = 0)
        {
            int nLocationId = 0;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                int nDateRange = Convert.ToInt32(ConfigurationManager.AppSettings["Place.DateRange"].ToString());

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

                string strSQL = string.Format("SELECT NVL(MIN(MLC_ID), 0) FROM MOBILE_LOCATION_CACHE WHERE MLC_ADDR_STREET = '{0}' and MLC_ADDR_NUMBER = '{1}' AND SYSDATE - MLC_DATE < {2}",
                    strStreet, strNumber, nDateRange);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nLocationId = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetLocationIdFromCache::Exception", LoggerSeverities.Error);
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

            return nLocationId;
        }

        private int GetNumPredictions(string strSearchMask, int nContractId = 0)
        {
            int nNumLocations = 0;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            try
            {
                int nDateRange = Convert.ToInt32(ConfigurationManager.AppSettings["Place.DateRange"].ToString());

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

                string strSQL = string.Format("SELECT COUNT(*) FROM MOBILE_GPS_CACHE WHERE MGC_CONTRACT_ID = {0} AND SUBSTR(UPPER(MGC_SEARCH), 0, {1}) = '{2}' AND SYSDATE - MGC_DATE < {3}",
                    nContractId, strSearchMask.Length, strSearchMask.ToUpper(), nDateRange);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    nNumLocations = dataReader.GetInt32(0);
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetNumPredictions::Exception", LoggerSeverities.Error);
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

            return nNumLocations;
        }

        private bool GetPredictionsFromCache(string strSearchMask, out string strLocations, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            strLocations = "";

            try
            {
                int nDateRange = Convert.ToInt32(ConfigurationManager.AppSettings["Place.DateRange"].ToString());
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

                string strSQL = string.Format("SELECT MGC_PLACE_ID, MGC_RESULT FROM MOBILE_GPS_CACHE WHERE MGC_CONTRACT_ID = {0} AND SUBSTR(UPPER(MGC_SEARCH), 0, {1}) = '{2}' AND SYSDATE - MGC_DATE < {3} ORDER BY MGC_PLACE_ID",
                    nContractId, strSearchMask.Length, strSearchMask.ToUpper(), nDateRange);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    int nCount = 0;
                    string strCurPlaceId = "";
                    strLocations = "{\"predictions\" : [";
                    while (dataReader.Read())
                    {
                        string strPlaceId = dataReader.GetString(0);
                        if (!strPlaceId.Equals(strCurPlaceId))
                        {
                            if (nCount > 0)
                                strLocations += ",";
                            strLocations += dataReader.GetString(1);
                            strCurPlaceId = strPlaceId;
                            nCount++;
                        }
                    }
                    strLocations += "],\"status\" : \"OK\"}";
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetPredictionsFromCache::Exception", LoggerSeverities.Error);
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

        private bool GetStreetId(string strStreetName, out int nStreetId, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            nStreetId = -1;

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

                //string strSQL = string.Format("SELECT MSTR_ID FROM MOBILE_STREETS WHERE UPPER(MSTR_DESC) = '{0}' AND MSTR_VALID = 1 AND MSTR_DELETED = 0", strStreetName.ToUpper());
                string strSQL = string.Format("SELECT MSTR_ID FROM MOBILE_STREETS WHERE INSTR('{0}', UPPER(MSTR_DESC)) > 0 AND MSTR_VALID = 1 AND MSTR_DELETED = 0", strStreetName.ToUpper());
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    nStreetId = dataReader.GetInt32(0);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetStreetId::Exception", LoggerSeverities.Error);
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

        private bool GetStreetName(int nStreetId, out string strStreetName, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            strStreetName = "";

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

                string strSQL = string.Format("SELECT MSTR_DESC FROM MOBILE_STREETS WHERE MSTR_ID = {0} AND MSTR_VALID = 1 AND MSTR_DELETED = 0", nStreetId);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    strStreetName = dataReader.GetString(0);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetStreetName::Exception", LoggerSeverities.Error);
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

        private bool GetSectorFromStretch(int iStreetId, string strStreetNum, out int iSectorId, out int iStretchId, int nContractId = 0)
        {
            bool bResult = false;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            int iStreetNum = 1;
            int iEven = 0;

            iSectorId = -1;
            iStretchId = -1;

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

                try
                {
                    string[] strStreetNumArray = strStreetNum.Split(new char[] { '-' });
                    string strOnlyNumber = new string(strStreetNumArray[0].Where(char.IsDigit).ToArray());
                    iStreetNum = Convert.ToInt32(strOnlyNumber);
                    iEven = 1 - iStreetNum % 2;
                }
                catch
                {
                    Logger_AddLogMessage("GetSectorFromStretch::Failed to convert street number, using 1", LoggerSeverities.Info);
                }

                string strSQL = string.Format("SELECT MSS_ID, MSS_GRP_ID FROM MOBILE_STREETS_STRETCHES WHERE MSS_MSTR_ID = {0} AND {1} >= MSS_STR_NUM_MIN AND {1} <= MSS_STR_NUM_MAX AND MSS_EVEN IN ({2}, 2) AND MSS_VALID = 1 AND MSS_DELETED = 0",
                    iStreetId, iStreetNum, iEven);
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                if (dataReader.Read())
                {
                    iStretchId = dataReader.GetInt32(0);
                    iSectorId = dataReader.GetInt32(1);
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetSectorFromStretch::Exception", LoggerSeverities.Error);
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

        private bool GetContractsData(out SortedList contractDataList)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            contractDataList = new SortedList();
            int nContractIndex = 0;

            try
            {
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

                // Get a list of all the contracts
                ArrayList bonusList = new ArrayList();
                string strSQL = string.Format("SELECT MCON_ID, MCON_LATITUDE, MCON_LONGITUDE, MCON_DESC1, MCON_DESC2, MCON_IMAGE_PATH, MCON_EMAIL, MCON_TELEPHONE, MCON_ADDRESS, MCON_RADIUS, MCON_WS_OPER, MCON_WS_USER FROM MOBILE_CONTRACTS WHERE MCON_ACTIVE = 1 ORDER BY MCON_ID");
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    SortedList contractData = new SortedList();
                    contractData["cont_id"] = dataReader.GetInt32(0).ToString();
                    if (!dataReader.IsDBNull(1))
                        contractData["lt"] = dataReader.GetDouble(1).ToString().Replace(",", ".");
                    else
                        contractData["lt"] = " ";
                    if (!dataReader.IsDBNull(2))
                        contractData["lg"] = dataReader.GetDouble(2).ToString().Replace(",", ".");
                    else
                        contractData["lg"] = " ";
                    if (!dataReader.IsDBNull(3))
                        contractData["desc1"] = dataReader.GetString(3);
                    else
                        contractData["desc1"] = " ";
                    if (!dataReader.IsDBNull(4))
                        contractData["desc2"] = dataReader.GetString(4);
                    else
                        contractData["desc2"] = " ";
                    if (!dataReader.IsDBNull(5))
                        contractData["image"] = dataReader.GetString(5);
                    else
                        contractData["image"] = " ";
                    if (!dataReader.IsDBNull(6))
                        contractData["email"] = dataReader.GetString(6);
                    else
                        contractData["email"] = " ";
                    if (!dataReader.IsDBNull(7))
                        contractData["phone"] = dataReader.GetString(7);
                    else
                        contractData["phone"] = " ";
                    if (!dataReader.IsDBNull(8))
                        contractData["addr"] = dataReader.GetString(8);
                    else
                        contractData["addr"] = " ";
                    if (!dataReader.IsDBNull(9))
                        contractData["rad"] = dataReader.GetInt32(9).ToString();
                    else
                        contractData["rad"] = " ";
                    if (!dataReader.IsDBNull(10))
                        contractData["wsoper"] = dataReader.GetString(10).ToString();
                    else
                        contractData["wsoper"] = " ";
                    if (!dataReader.IsDBNull(11))
                        contractData["wsuser"] = dataReader.GetString(11).ToString();
                    else
                        contractData["wsuser"] = " ";
                    nContractIndex++;
                    contractDataList["contract" + nContractIndex.ToString()] = contractData;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetContractsData::Exception", LoggerSeverities.Error);
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

        private bool GetStreetsData(out SortedList streetDataList, int nContractId = 0)
        {
            bool bResult = true;
            OracleDataReader dataReader = null;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;

            streetDataList = new SortedList();
            int nStreetIndex = 0;

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

                // Get a list of all the coordinates for the stretch area
                string strSQL = string.Format("SELECT MSTR_DESC FROM MOBILE_STREETS WHERE MSTR_SHOW_GUI = 1 AND MSTR_DELETED = 0 ORDER BY MSTR_DESC");
                oraCmd.CommandText = strSQL;

                dataReader = oraCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    SortedList streetData = new SortedList();
                    streetData["st_name"] = dataReader.GetString(0);
                    nStreetIndex++;
                    streetDataList["street" + nStreetIndex.ToString()] = streetData;
                }
            }
            catch (Exception e)
            {
                Logger_AddLogMessage("GetStreetsData::Exception", LoggerSeverities.Error);
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

        private bool LogMsgDB(string xmlIn, string xmlOut, int iVirtualUnit, int nContractId = 0)
        {
            bool bResult = false;
            OracleCommand oraCmd = null;
            OracleConnection oraConn = null;
            string strMsgIn = "";
            string strMsgOut = "";

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

                if (xmlIn.Length > 4000)
                    strMsgIn = xmlIn.Substring(0, 4000);
                else
                    strMsgIn = xmlIn;

                if (xmlOut.Length > 4000)
                    strMsgOut = xmlOut.Substring(0, 4000);
                else
                    strMsgOut = xmlOut;

                string strSQL = string.Format("insert into msgs_log (lmsg_src_uni_id," +
                "lmsg_dst_uni_id,lmsg_date,lmsg_type,lmsg_xml_in," +
                "lmsg_xml_out) values ({0},{1},sysdate,'{2}','{3}','{4}')",
                iVirtualUnit, CCunitId, doc.DocumentElement.Name, strMsgIn, strMsgOut.Replace("\n", ""));

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

        //private bool CalculateTimeSteps(ref SortedList parametersOut)
        //{
        //    bool bResult = true;
        //    int nTimeStepOffset = 0;
        //    int nNumSteps = 0;
        //    int nCurStep = 1;
        //    int nMinValue = 0;
        //    int nMaxValue = 0;
        //    int nMinTime = 0;
        //    int nMaxTime = 0;
        //    DateTime dtMinDate = DateTime.Now;
        //    DateTime dtMaxDate = DateTime.Now;

        //    try
        //    {
        //        SortedList parametersList = new SortedList();

        //        nTimeStepOffset = Convert.ToInt32(ConfigurationManager.AppSettings["TIME_STEPS_OFFSET_IN_MINUTES"]);
        //        TimeSpan tsTimeStepOffset = new TimeSpan(0, nTimeStepOffset, 0);

        //        if (nTimeStepOffset > 0)
        //        {
        //            // Get initial values
        //            nMinValue = Convert.ToInt32(parametersOut["q1"]);
        //            nMaxValue = Convert.ToInt32(parametersOut["q2"]);
        //            nMinTime = Convert.ToInt32(parametersOut["t1"]);
        //            nMaxTime = Convert.ToInt32(parametersOut["t2"]);
        //            dtMinDate = DateTime.ParseExact(parametersOut["d1"].ToString(), "HHmmssddMMyy", System.Globalization.CultureInfo.InvariantCulture);
        //            dtMaxDate = DateTime.ParseExact(parametersOut["d2"].ToString(), "HHmmssddMMyy", System.Globalization.CultureInfo.InvariantCulture);

        //            // Add minimum as first step
        //            SortedList parametersStep1 = new SortedList();
        //            parametersStep1["q"] = nMinValue.ToString();
        //            parametersStep1["t"] = nMinTime.ToString();
        //            parametersStep1["d"] = parametersOut["d1"].ToString();
        //            parametersList["st" + nCurStep.ToString()] = parametersStep1;
        //            nCurStep++;

        //            nNumSteps = (nMaxTime - nMinTime) / nTimeStepOffset;

        //            if (nNumSteps > 0)
        //            {
        //                for (int i = 2; i <= nNumSteps; i++)
        //                {
        //                    SortedList parametersStep = new SortedList();
        //                    int nCurValue = nMinValue + 
        //                    parametersStep["q"] = nMinValue.ToString();
        //                    parametersStep["t"] = nMinTime.ToString();
        //                    parametersStep["d"] = parametersOut["d1"].ToString();
        //                    parametersList["st" + i.ToString()] = parametersStep;
        //                }

        //            }
                        


        //            parametersOut["lst"] = parametersList;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Logger_AddLogMessage("CalculateTimeSteps::Exception", LoggerSeverities.Error);
        //        Logger_AddLogException(e);
        //        bResult = false;
        //    }

        //    return bResult;
        //}

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
                //_logger = new FileLogger(LoggerSeverities.Debug, ((string)appSettings.GetValue("ServiceLog", typeof(string))).Replace(".log", ".WSMobilePayment.log"));
                //OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
                //OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);
                //DatabaseFactory.Logger = _logger;

                _logger = new Logger(MethodBase.GetCurrentMethod().DeclaringType);
                DatabaseFactory.Logger = _logger;
            }


            if (_msgSession == null)
            {
                _msgSession = new MessagesSession();
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

            if (_useHash == null)
            {
                _useHash = ((string)appSettings.GetValue("EnableHash", typeof(string)));
            }
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

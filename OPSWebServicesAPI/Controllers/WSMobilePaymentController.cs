using AutoMapper;
using CS_OPS_TesM1;
using Jot;
using Newtonsoft.Json;
using OPS.Comm;
using OPS.Comm.Becs.Messages;
using OPS.Components;
using OPS.Components.Data;
using OPS.FineLib;
using OPSWebServicesAPI.Helpers;
using OPSWebServicesAPI.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;

namespace OPSWebServicesAPI.Controllers
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

    public class WSMobilePaymentController : ApiController
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

        public enum SeverityError
        {
            Warning = 1, //aviso a usuario
            Exception = 2, //error no controlado
            Critical = 3, //error de lógica
            Low = 4 //informativo (para logs)
        }


        public static ILogger Logger
        {
            get { return _logger; }
        }

        public WSMobilePaymentController()
        {

            //Uncomment the following line if using designed components 
            //InitializeComponent(); 
            InitializeStatic();
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

        /// <summary>
        /// Return contracts information
        /// </summary>
        /// <returns>Object ContractsInfo or error</returns>
        [HttpGet]
        [Route("QueryContractsAPI")]
        public ResultContractsInfo QueryContractsAPI()
        {
            //string xmlOut = "";
            ResultContractsInfo response = new ResultContractsInfo();
            SortedList contractList = null;
            ContractsInfo contInfo = new ContractsInfo();
            try
            {
                //SortedList parametersOut = new SortedList();
                

                // Get contracts information
                if (!GetContractsData(out contractList))
                {
                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                    Logger_AddLogMessage(string.Format("QueryContractsXML::Error - Could not obtain contracts data: xmlOut={0}", "Result_Error_Generic"), LoggerSeverities.Error);
                    //return xmlOut;
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }

                //if (contractList.Count > 0)
                //    parametersOut["contractlist"] = contractList;
                //else
                //{
                //    contractList = new SortedList();
                //    contractList["contract1"] = "";
                //    parametersOut["contractlist"] = contractList;
                //}

                //parametersOut["t"] = DateTime.Now.ToString("HHmmssddMMyy");
                //parametersOut["cont_no"] = contractList.Count.ToString();
                //parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                //xmlOut = GenerateXMLOuput(parametersOut);
            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryContractsXML::Error: xmlOut={0}", "Result_Error_Generic"), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            //return xmlOut;
            contInfo.time = DateTime.Now.ToString("HHmmssddMMyy");
            contInfo.contractsNumber = contractList.Count.ToString();
            contInfo.result = Convert.ToInt32(ResultType.Result_OK).ToString();
            List<Contract> lista = new List<Contract>();
            //SortedList listOps = (SortedList)parametersOut["lst"];
            ConfigMapModel configMapModel = new ConfigMapModel();
            var config = configMapModel.configContract();
            IMapper iMapper = config.CreateMapper();
            if (contractList != null) foreach (System.Collections.DictionaryEntry con in contractList)
                {
                    Contract contract = iMapper.Map<SortedList, Contract>((SortedList)con.Value);
                    lista.Add(contract);
                }
            contInfo.contractlist = lista.ToArray();

            response.isSuccess = true;
            response.error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            response.value = contInfo;
            return response;//strToken;
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

        /// <summary>
        /// Returns zone information
        /// </summary>
        /// <param name="zoneQuery">Object ZoneQuery with long-lat information or streetName-number information to request</param>
        /// <returns>zone information for query request</returns>
        [HttpPost]
        [Route("QueryZoneAPI")]
        public ResultZoneInfo QueryZoneAPI([FromBody] ZoneQuery zoneQuery)
        {
            //string xmlOut = "";

            ResultZoneInfo response = new ResultZoneInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            PropertyInfo[] properties = typeof(ZoneQuery).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(zoneQuery);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                //SortedList parametersOut = new SortedList();
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

                Logger_AddLogMessage(string.Format("QueryZoneAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["lt"] == null || (parametersIn["lt"] != null && parametersIn["lt"].ToString().Length == 0)) &&
                        (parametersIn["lg"] == null || (parametersIn["lg"] != null && parametersIn["lg"].ToString().Length == 0)) &&
                        (parametersIn["streetname"] == null || (parametersIn["streetname"] != null && parametersIn["streetname"].ToString().Length == 0)) &&
                        (parametersIn["streetno"] == null || (parametersIn["streetno"] != null && parametersIn["streetno"].ToString().Length == 0)) ||
                        (parametersIn["contid"] == null || (parametersIn["contid"].ToString().Length == 0)))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryZoneAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), "Result_Error_Missing_Input_Parameter"), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
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
                            Logger_AddLogMessage(string.Format("QueryZoneAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), "Result_Error_InvalidAuthenticationHash"), LoggerSeverities.Error);
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

                            // Determine the group/sector
                            // First try to use the address by getting the GPS coordinates
                            string strContractName = "";
                            GetParameter("P_SYSTEM_NAME", out strContractName, nContractId);
                            Loc curLocation = null;
                            if (parametersIn["streetname"].ToString().Length > 0 && parametersIn["streetno"].ToString().Length > 0)
                            {
                                // First check locally in cached data
                                Logger_AddLogMessage(string.Format("QueryZoneAPI::Searching location locally"), LoggerSeverities.Info);
                                int nLocationId = GetLocationIdFromCache(parametersIn["streetname"].ToString(), parametersIn["streetno"].ToString(), nContractId);
                                if (nLocationId > 0)
                                    curLocation = LocateGPSFromAddressLocal(nLocationId, nContractId);

                                // If nothing is found in cache, use Google to find coordinates
                                if (curLocation == null)
                                {
                                    Logger_AddLogMessage(string.Format("QueryZoneAPI::Searching location with Google"), LoggerSeverities.Info);
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

                                    Logger_AddLogMessage(string.Format("QueryZoneAPI::Searching stretch areas"), LoggerSeverities.Info);
                                    GetStretchAreasToSearch(out stretchList, nContractId);
                                    if (stretchList.Count > 0)
                                    {
                                        foreach (int iStretch in stretchList)
                                        {
                                            GetStretchAreas(iStretch, out areaList, nContractId);
                                            if (IsPointInPolygon(areaList, curLocation))
                                            {
                                                // Found area which corresponds to a defined stretch, get the sector/street info
                                                Logger_AddLogMessage(string.Format("QueryZoneAPI::Found the stretch - {0}", iStretch.ToString()), LoggerSeverities.Info);
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
                                        Logger_AddLogMessage(string.Format("QueryZoneAPI::Searching address locally"), LoggerSeverities.Info);
                                        string strContractName1 = "";
                                        string strContractName2 = "";
                                        GetContractNames(out strContractName1, out strContractName2, nContractId);
                                        if (!LocateAddressFromGPSLocal(curLocation, out strStreet, out strNumber, nContractId))
                                        {
                                            Logger_AddLogMessage(string.Format("QueryZoneAPI::Searching location with Google"), LoggerSeverities.Info);
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
                                                    Logger_AddLogMessage(string.Format("QueryZoneAPI::CurLoc: lat= {0}, long={1} gives the city {2} instead of {3}/{4}", curLocation.Lt.ToString(), curLocation.Lg.ToString(), strCity, strContractName1, strContractName2), LoggerSeverities.Info);
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
                                                    Logger_AddLogMessage(string.Format("QueryZoneAPI::CurLoc: lat= {0}, long={1} gives the city {2} instead of {3}/{4}", curLocation.Lt.ToString(), curLocation.Lg.ToString(), strCity, strContractName1, strContractName2), LoggerSeverities.Info);
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
                                Logger_AddLogMessage(string.Format("QueryZoneAPI::CurLoc: lat= {0}, long={1}", curLocation.Lt.ToString(), curLocation.Lg.ToString()), LoggerSeverities.Error);
                            else
                                Logger_AddLogMessage(string.Format("QueryZoneAPI::No location found"), LoggerSeverities.Error);

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
                                            Logger_AddLogMessage(string.Format("QueryZoneAPI::Found sector {0} and stretch {1} for street {2}, {3}", nZoneId, nStretchId, parametersOut["streetname"].ToString(), parametersOut["streetno"].ToString()), LoggerSeverities.Error);
                                            GetGroupParent(nZoneId, ref nGroupId, nContractId);
                                        }
                                        else
                                            Logger_AddLogMessage(string.Format("QueryZoneAPI::No sector found for street {0} - {1}", nStreetId, parametersOut["streetname"].ToString()), LoggerSeverities.Error);
                                    }
                                    else
                                    {
                                        Logger_AddLogMessage(string.Format("QueryZoneAPI::Could not find street {0}", parametersOut["streetname"].ToString()), LoggerSeverities.Error);
                                    }
                                }

                                if (!bFoundZone && ConfigurationManager.AppSettings["cont" + nContractId.ToString() + ".LocalStretchSearchOnly"].ToString().Equals("0"))
                                {
                                    Logger_AddLogMessage(string.Format("QueryZoneAPI::Using GPS to find group/sector"), LoggerSeverities.Info);

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

                            //xmlOut = GenerateXMLOuput(parametersOut);

                            if (parametersOut.Count == 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryZoneXML::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                            }
                            else
                            {
                                Logger_AddLogMessage(string.Format("QueryZoneXML: parametersOut= {0}", SortedListToString(parametersOut)), LoggerSeverities.Info);
                            }
                        }
                    }
                }
                else
                {
                    //xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryZoneAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), rt), LoggerSeverities.Error);
                }
            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryZoneAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                Logger_AddLogException(e);
            }

            response.isSuccess = true;
            response.error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

            ZoneInfo zoneInfo = new ZoneInfo();
            ConfigMapModel configMapModel = new ConfigMapModel();
            var config = configMapModel.configZone();
            IMapper iMapper = config.CreateMapper();
            zoneInfo = iMapper.Map<SortedList, ZoneInfo>((SortedList)parametersOut);
            response.value = zoneInfo;
            return response;

            //return xmlOut;
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

        /// <summary>
        /// return village streets
        /// </summary>
        /// <param name="streetsQuery">Object StreetsQuery with ContractId to request</param>
        /// <returns>village streets</returns>
        [HttpPost]
        [Route("QueryStreetsAPI")]
        public ResultStreetsInfo QueryStreetsAPI([FromBody] StreetsQuery streetsQuery)
        {
            //string xmlOut = "";

            ResultStreetsInfo response = new ResultStreetsInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            PropertyInfo[] properties = typeof(StreetsQuery).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(streetsQuery);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                //SortedList parametersOut = new SortedList();
                SortedList streetList = null;
                string strHash = "";
                string strHashString = "";

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if (parametersIn["contid"] == null || (parametersIn["contid"].ToString().Length == 0))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryStreetsAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
                        response.value = null; //Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
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

                        // Get contracts information
                        if (!GetStreetsData(out streetList, nContractId))
                        {
                            //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                            Logger_AddLogMessage(string.Format("QueryStreetsAPI::Error - Could not obtain streets data: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                            //return xmlOut;
                            response.isSuccess = false;
                            response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                            response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                            return response;
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
                        //xmlOut = GenerateXMLOuput(parametersOut);
                    }
                }
            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryStreetsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), "Result_Error_Generic"), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            response.isSuccess = true;
            response.error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

            SortedList listStreets = (SortedList)parametersOut["streetlist"];
            List<string> streetsNamelist = new List<string>();
            if (listStreets != null) foreach (System.Collections.DictionaryEntry st in listStreets)
                {
                    streetsNamelist.Add((string)((SortedList)(st.Value))["st_name"]);
                }
            parametersOut["streetlist"] = streetsNamelist.ToArray();

            StreetsInfo streetsInfo = new StreetsInfo();
            ConfigMapModel configMapModel = new ConfigMapModel();
            var config = configMapModel.configStreets();
            IMapper iMapper = config.CreateMapper();
            streetsInfo = iMapper.Map<SortedList, StreetsInfo>((SortedList)parametersOut);
            response.value = streetsInfo;
            return response;

            //return xmlOut;
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

        /// <summary>
        /// return information from google api link https://maps.googleapis.com/maps/api/place/autocomplete/json
        /// </summary>
        /// <param name="placeQuery">Object PlaceQuery with street name to request</param>
        /// <returns>place information or error</returns>
        [HttpPost]
        [Route("QueryPlaceAPI")]
        public ResultPlaceInfo QueryPlaceAPI([FromBody] PlaceQuery placeQuery)
        {
            //string xmlOut = "";

            ResultPlaceInfo response = new ResultPlaceInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            PropertyInfo[] properties = typeof(PlaceQuery).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(placeQuery);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                //SortedList parametersOut = new SortedList();
                string strHash = "";
                string strHashString = "";
                string strResponse = "";

                Logger_AddLogMessage(string.Format("QueryPlaceAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {
                    if ((parametersIn["streetname"] == null || (parametersIn["streetname"].ToString().Length == 0)) ||
                        (parametersIn["contid"] == null || (parametersIn["contid"].ToString().Length == 0)))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryPlaceAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), "Result_Error_Missing_Input_Parameter"), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
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
                            Logger_AddLogMessage(string.Format("QueryPlaceAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), "Result_Error_InvalidAuthenticationHash"), LoggerSeverities.Error);
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
                                            Logger_AddLogMessage(string.Format("QueryPlaceAPI::Error adding results to cache: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                            parametersOut["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                            //xmlOut = GenerateXMLOuput(parametersOut);
                                            //return xmlOut;
                                            response.isSuccess = false;
                                            response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                            response.value = null; //SortedListToString(parametersOut);
                                            return response;
                                        }
                                    }
                                    else
                                        nLocateResult = 0;
                                }
                                else if (nLocateResult == 0)
                                    Logger_AddLogMessage(string.Format("QueryPlaceAPI::No results found for location"), LoggerSeverities.Info);
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryPlaceAPI::Error obtaining place results: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                    parametersOut["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    //xmlOut = GenerateXMLOuput(parametersOut);
                                    //return xmlOut;
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                    response.value = null; //SortedListToString(parametersOut);
                                    return response;
                                }
                            }

                            string strLocations = "";
                            if (nLocateResult > 0)
                            {
                                if (!GetPredictionsFromCache(parametersIn["streetname"].ToString(), out strLocations, nContractId))
                                {
                                    Logger_AddLogMessage(string.Format("QueryPlaceAPI::Error obtaining results from cache: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                    parametersOut["r"] = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    //xmlOut = GenerateXMLOuput(parametersOut);
                                    //return xmlOut;
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                    response.value = null; //SortedListToString(parametersOut);
                                    return response;
                                }
                            }

                            if (strLocations.Length > 0)
                            {
                                parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK).ToString();
                                parametersOut["response"] = strLocations;
                            }
                            else
                                parametersOut["r"] = Convert.ToInt32(ResultType.Result_Error_Location_Not_Found).ToString();

                            //xmlOut = GenerateXMLOuput(parametersOut);

                            if (parametersOut.Count == 0)
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                Logger_AddLogMessage(string.Format("QueryPlaceAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                response.value = null; //SortedListToString(parametersOut);
                                return response;
                            }
                            else
                            {
                                Logger_AddLogMessage(string.Format("QueryPlaceAPI: parametersOut= {0}", SortedListToString(parametersOut)), LoggerSeverities.Info);
                            }
                        }
                    }
                }
                else
                {
                    //xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryPlaceAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)rt, (int)SeverityError.Critical);
                    response.value = null; //SortedListToString(parametersOut);
                    return response;
                }
            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryPlaceAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //SortedListToString(parametersOut);
                return response;
            }

            response.isSuccess = true;
            response.error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

            PlaceInfo placeInfo = new PlaceInfo();
            ConfigMapModel configMapModel = new ConfigMapModel();
            var config = configMapModel.configPlace();
            IMapper iMapper = config.CreateMapper();
            placeInfo = iMapper.Map<SortedList, PlaceInfo>((SortedList)parametersOut);
            response.value = placeInfo;
            return response;

            //return xmlOut;
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
                <t1>minimum amount of time to park in minutes</t1>
                <t2>maximum amount of time to park in minutes</t2>
                <d1>minimum date</d1>
                <d2>maximum date</d2> 
                <o>Operation Type: 1: First parking: 2: extension</o>
                <di>Initial date (in format hh24missddMMYY) of the parking: the same as the input date if the operation is a first parking, or the date of the end of parking operations chain if the operation is an extension</di>
                <aq>Amount of Euro Cents accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation</aq>
                <at>Amount of minutes accumulated in the current parking chain (first parking plus all the extensions) linked to the current operation</at>
                <lst>Tariff steps from t1 to t2 every a parameterized number of minutes (for example 5 minutes)
	                <st>Step 1
		                <t>Time in minutes. Call it T1 .First Step will be t1</t>
		                <q>Cost in Euro Cents for T1 minutes</q>
                        <d>datetime given by q cents </d>
	                </st>
	                <st>Step 2
		                <t> T1+5 minutes. Call it T2</t>
                        <q>Cost in Euro Cents for T2 minutes</q>
                        <d>datetime given by q cents </d>		                		                            
	                </st>
	                …
	                <st>Last Step (Step N)
                        <t>TN-1+5(or less in last step) minutes. Call it TN. Last Step will be t2</t>
		                <q>Cost in Euro Cents for TN minutes</q>	
                        <d>datetime given by q cents </d>
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

        /// <summary>
        /// return information for parking operation
        /// </summary>
        /// <param name="parkingStepsQuery">Object ParkingStepsQuery with sector and plate to request</param>
        /// <returns>parking information with time steps or error</returns>
        [HttpGet]
        [Route("QueryParkingOperationWithTimeStepsAPI")]
        public ResultParkingStepsInfo QueryParkingOperationWithTimeStepsAPI([FromBody] ParkingStepsQuery parkingStepsQuery)
        {
            //string xmlOut = "";

            ResultParkingStepsInfo response = new ResultParkingStepsInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            PropertyInfo[] properties = typeof(ParkingStepsQuery).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(parkingStepsQuery);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                //SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null || (parametersIn["p"].ToString().Length == 0)) ||
                        (parametersIn["g"] == null || (parametersIn["g"].ToString().Length == 0)) ||
                        (parametersIn["contid"] == null || (parametersIn["contid"].ToString().Length == 0)))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
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
                            Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
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

                            int iVirtualUnit = -1;
                            if (GetVirtualUnit(Convert.ToInt32(parametersIn["g"]), ref iVirtualUnit, nContractId))
                            {
                                if (iVirtualUnit < 0)
                                {
                                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                    //return xmlOut;
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Invalid_Input_Parameter, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter).ToString();
                                    return response;
                                }

                            }
                            else
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Invalid_Input_Parameter, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter).ToString();
                                return response;
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
                            //EN EL CODIGO ORIGINAL NO ESTAN DEFINIDOS ESTOS PARAMETROS (Afi,Afr,Afp) --> NO PODÍA DEVOLVER LA INFORMACIÓN CALCULADA DE ESTOS PARAMETROS
                            //SI SE QUIERE QUE APAREZCAN BASTA DESCOMENTARLOS Y DESCOMENTAR EL CODIGO DE OPSMessage_M01Process
                            //parametersOutMapping["Afi"] = "fi";
                            //parametersOutMapping["Afr"] = "fr";
                            //parametersOutMapping["Afp"] = "fp";

                            rt = SendM1(parametersIn, parametersInMapping, parametersOutMapping, iVirtualUnit, out parametersOut, nContractId);

                            if (rt == ResultType.Result_OK)
                            {
                                //// Check to see if time steps were sent already in M1
                                //if (parametersOut["cst"] == null)
                                //{
                                //    CalculateTimeSteps(ref parametersOut);
                                //}

                                //xmlOut = GenerateXMLOuput(parametersOut);

                                if (parametersOut.Count == 0)
                                {
                                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    return response;
                                }
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsAPI: parametersOut= {0}", SortedListToString(parametersOut)), LoggerSeverities.Info);
                                }

                            }
                            else
                            {
                                //xmlOut = GenerateXMLErrorResult(rt);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
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
                    //xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }

            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryParkingOperationWithTimeStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            response.isSuccess = true;
            response.error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

            ParkingStepsInfo parkingStepsInfo = new ParkingStepsInfo();
            ConfigMapModel configMapModel = new ConfigMapModel();

            var config = configMapModel.configParkingSteps();
            IMapper iMapper = config.CreateMapper();
            parkingStepsInfo = iMapper.Map<SortedList, ParkingStepsInfo>((SortedList)parametersOut);

            List<Step> lista = new List<Step>();
            SortedList listSteps = (SortedList)parametersOut["lst"];
            var configStep = configMapModel.configStep();
            IMapper iMapperStep = configStep.CreateMapper();
            if (listSteps != null) foreach (System.Collections.DictionaryEntry step in listSteps)
                {
                    Step st = iMapperStep.Map<SortedList, Step>((SortedList)step.Value);
                    lista.Add(st);
                }
            parkingStepsInfo.steps = lista.ToArray();

            response.value = parkingStepsInfo;
            return response;

            //return xmlOut;
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

        /// <summary>
        /// return information for parking operation
        /// </summary>
        /// <param name="parkingStepsQuery">Object ParkingStepsQuery with sector and plate to request</param>
        /// <returns>parking information with money steps or error</returns>
        [HttpPost]
        [Route("QueryParkingOperationWithMoneyStepsAPI")]
        public ResultParkingStepsInfo QueryParkingOperationWithMoneyStepsAPI([FromBody] ParkingStepsQuery parkingStepsQuery)
        {
            //string xmlOut = "";

            ResultParkingStepsInfo response = new ResultParkingStepsInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            PropertyInfo[] properties = typeof(ParkingStepsQuery).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(parkingStepsQuery);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                //SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null || (parametersIn["p"].ToString().Length == 0)) ||
                        (parametersIn["g"] == null || (parametersIn["g"].ToString().Length == 0)) ||
                        (parametersIn["contid"] == null || (parametersIn["contid"].ToString().Length == 0)))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
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
                            Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
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

                            int iVirtualUnit = -1;
                            if (GetVirtualUnit(Convert.ToInt32(parametersIn["g"]), ref iVirtualUnit, nContractId))
                            {
                                if (iVirtualUnit < 0)
                                {
                                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                    //return xmlOut;
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Invalid_Input_Parameter, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter).ToString();
                                    return response;
                                }

                            }
                            else
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Invalid_Input_Parameter, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter).ToString();
                                return response;
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
                                //xmlOut = GenerateXMLOuput(parametersOut);

                                if (parametersOut.Count == 0)
                                {
                                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    return response;
                                }
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsAPI: parametersOut= {0}", SortedListToString(parametersOut)), LoggerSeverities.Info);
                                }

                            }
                            else
                            {
                                //xmlOut = GenerateXMLErrorResult(rt);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
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
                    //xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsAPI::Error: xmlIn= {0}, xmlOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }

            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryParkingOperationWithMoneyStepsAPI::Error: xmlIn= {0}, xmlOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            response.isSuccess = true;
            response.error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

            ParkingStepsInfo parkingStepsInfo = new ParkingStepsInfo();
            ConfigMapModel configMapModel = new ConfigMapModel();

            var config = configMapModel.configParkingSteps();
            IMapper iMapper = config.CreateMapper();
            parkingStepsInfo = iMapper.Map<SortedList, ParkingStepsInfo>((SortedList)parametersOut);

            List<Step> lista = new List<Step>();
            SortedList listSteps = (SortedList)parametersOut["lst"];
            var configStep = configMapModel.configStep();
            IMapper iMapperStep = configStep.CreateMapper();
            if (listSteps != null) foreach (System.Collections.DictionaryEntry step in listSteps)
                {
                    Step st = iMapperStep.Map<SortedList, Step>((SortedList)step.Value);
                    lista.Add(st);
                }
            parkingStepsInfo.steps = lista.ToArray();

            response.value = parkingStepsInfo;
            return response;

            //return xmlOut;
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

        [HttpPost]
        [Route("QueryParkingOperationForTimeAPI")]
        public ResultParkingTimeInfo QueryParkingOperationForTimeAPI([FromBody] ParkingTimeQuery parkingTimeQuery)
        {
            //string xmlOut = "";

            ResultParkingTimeInfo response = new ResultParkingTimeInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            PropertyInfo[] properties = typeof(ParkingTimeQuery).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(parkingTimeQuery);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                //SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null) ||
                        (parametersIn["g"] == null) ||
                        (parametersIn["t"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
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
                            Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
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

                            int iVirtualUnit = -1;
                            if (GetVirtualUnit(Convert.ToInt32(parametersIn["g"]), ref iVirtualUnit, nContractId))
                            {
                                if (iVirtualUnit < 0)
                                {
                                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                    //return xmlOut;
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Invalid_Input_Parameter, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter).ToString();
                                    return response;
                                }

                            }
                            else
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Invalid_Input_Parameter, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter).ToString();
                                return response;
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
                                //xmlOut = GenerateXMLOuput(parametersOut);

                                if (parametersOut.Count == 0)
                                {
                                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    return response;
                                }
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeAPI: parametersOut= {0}", SortedListToString(parametersOut)), LoggerSeverities.Info);
                                }

                            }
                            else
                            {
                                //xmlOut = GenerateXMLErrorResult(rt);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
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
                    //xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }

            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryParkingOperationForTimeAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            response.isSuccess = true;
            response.error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

            ParkingTimeInfo parkingTimeInfo = new ParkingTimeInfo();
            ConfigMapModel configMapModel = new ConfigMapModel();

            var config = configMapModel.configParkingTime();
            IMapper iMapper = config.CreateMapper();
            parkingTimeInfo = iMapper.Map<SortedList, ParkingTimeInfo>((SortedList)parametersOut);

            response.value = parkingTimeInfo;
            return response;

            //return xmlOut;
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

        [HttpPost]
        [Route("QueryParkingOperationForMoneyAPI")]
        public ResultParkingMoneyInfo QueryParkingOperationForMoneyAPI([FromBody] ParkingMoneyQuery parkingMoneyQuery)
        {
            //string xmlOut = "";
            ResultParkingMoneyInfo response = new ResultParkingMoneyInfo();
            SortedList parametersOut = new SortedList();

            SortedList parametersIn = new SortedList();

            PropertyInfo[] properties = typeof(ParkingMoneyQuery).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string NombreAtributo = (attribute == null) ? property.Name : attribute.DisplayName;
                //string NombreAtributo = property.Name;
                var Valor = property.GetValue(parkingMoneyQuery);
                parametersIn.Add(NombreAtributo, Valor);
            }

            try
            {
                //SortedList parametersIn = null;
                //SortedList parametersOut = null;
                string strHash = "";
                string strHashString = "";

                Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyAPI: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Info);

                ResultType rt = FindInputParametersAPI(parametersIn, out strHash, out strHashString);

                if (rt == ResultType.Result_OK)
                {

                    if ((parametersIn["p"] == null) ||
                        (parametersIn["g"] == null) ||
                        (parametersIn["q"] == null) ||
                        (parametersIn["contid"] == null))
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                        response.isSuccess = false;
                        response.error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
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
                            Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
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

                            int iVirtualUnit = -1;
                            if (GetVirtualUnit(Convert.ToInt32(parametersIn["g"]), ref iVirtualUnit, nContractId))
                            {
                                if (iVirtualUnit < 0)
                                {
                                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                    //return xmlOut;
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Invalid_Input_Parameter, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter).ToString();
                                    return response;
                                }

                            }
                            else
                            {
                                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Invalid_Input_Parameter);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                //return xmlOut;
                                response.isSuccess = false;
                                response.error = new Error((int)ResultType.Result_Error_Invalid_Input_Parameter, (int)SeverityError.Critical);
                                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Invalid_Input_Parameter).ToString();
                                return response;
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
                                //xmlOut = GenerateXMLOuput(parametersOut);

                                if (parametersOut.Count == 0)
                                {
                                    //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                                    response.isSuccess = false;
                                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                                    return response;
                                }
                                else
                                {
                                    Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyAPI: parametersOut= {0}", SortedListToString(parametersOut)), LoggerSeverities.Info);
                                }

                            }
                            else
                            {
                                //xmlOut = GenerateXMLErrorResult(rt);
                                Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
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
                    //xmlOut = GenerateXMLErrorResult(rt);
                    Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                    response.isSuccess = false;
                    response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                    return response;
                }

            }
            catch (Exception e)
            {
                //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Generic);
                Logger_AddLogMessage(string.Format("QueryParkingOperationForMoneyAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), SortedListToString(parametersOut)), LoggerSeverities.Error);
                Logger_AddLogException(e);
                response.isSuccess = false;
                response.error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.value = null; //Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            response.isSuccess = true;
            response.error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

            ParkingMoneyInfo parkingMoneyInfo = new ParkingMoneyInfo();
            ConfigMapModel configMapModel = new ConfigMapModel();

            var config = configMapModel.configParkingMoney();
            IMapper iMapper = config.CreateMapper();
            parkingMoneyInfo = iMapper.Map<SortedList, ParkingMoneyInfo>((SortedList)parametersOut);

            response.value = parkingMoneyInfo;
            return response;

            //return xmlOut;
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

        /*
        [HttpPost]
        [Route("QueryContractsXML")]
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
        */

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

        /*
        [HttpPost]
        [Route("QueryZoneXML")]
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
        */

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

        /*
        [HttpPost]
        [Route("QueryStreetsXML")]
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
        */

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

        /*
        [HttpPost]
        [Route("QueryPlaceXML")]
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
        */

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

        /*
        [HttpPost]
        [Route("QueryParkingOperationWithTimeStepsXML")]
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
                            //EN EL CODIGO ORIGINAL NO ESTAN DEFINIDOS ESTOS PARAMETROS (Afi,Afr,Afp) --> NO PODÍA DEVOLVER LA INFORMACIÓN CALCULADA DE ESTOS PARAMETROS
                            //SI SE QUIERE QUE APAREZCAN BASTA DESCOMENTARLOS Y DESCOMENTAR EL CODIGO DE OPSMessage_M01Process
                            //parametersOutMapping["Afi"] = "fi";
                            //parametersOutMapping["Afr"] = "fr";
                            //parametersOutMapping["Afp"] = "fp";

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
        */

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

        /*
        [HttpPost]
        [Route("QueryParkingOperationWithMoneyStepsXML")]
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
        */

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

        /*
        [HttpPost]
        [Route("QueryParkingOperationForTimeXML")]
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
        */

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

        /*
        [HttpPost]
        [Route("QueryParkingOperationForMoneyXML")]
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
        */

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

        /*
        [HttpPost]
        [Route("ConfirmParkingOperationXML")]
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
        */

        #region private methods mios

        private string SortedListToString(SortedList lista)
        {
            string texto = "";
            foreach (string kvp in lista.Keys)
                texto = texto + string.Format(" {0}: {1} ,", kvp, lista[kvp]);
            return texto;
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
                            catch (Exception ex)
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
                        var placeId = (string)prediction["place_id"];
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

                    //strM1In: <m1 id="203229026"><ad>4</ad><cdl>1</cdl><ctst>1</ctst><d>164251260221</d><dll>SOFTWARE\OTS\OPSComputeSoria</dll><g>60001</g><m>5743DTN</m><o>1</o><pt>4</pt><stv>5</stv><u>5001</u></m1>
                    StringCollection collectionM1Out = OPSMessage_M01Process(parametersM1In, strM1In, nContractId);
                    //collectionM1Out: <ap id="203229026"><Ar>1</Ar><Aq1>5</Aq1><Aq2>85</Aq2><At1>4</At1><At2>73</At2><Ad1>165300300321</Ad1><Ad2>180200300321</Ad2><At>73</At><Ao>2</Ao><Ad>180200300321</Ad><Adi>164900300321</Adi><Ag>60001</Ag><Aad>4</Aad><Aaq>55</Aaq><Aat>48</Aat><Aaqag>-999</Aaqag><Aatag>-999</Aatag><Ad0>160112300321</Ad0><Aq>-999</Aq><Adr0>160112300321</Adr0><Araq>55</Araq><Arat>48</Arat><Amobi>1</Amobi><stq1>5</stq1><stt1>4</stt1><std1>165300300321</std1><stq2>11</stq2><stt2>9</stt2><std2>165800300321</std2><stq3>16</stq3><stt3>14</stt3><std3>170300300321</std3><stq4>23</stq4><stt4>19</stt4><std4>170800300321</std4><stq5>28</stq5><stt5>24</stt5><std5>171300300321</std5><stq6>34</stq6><stt6>29</stt6><std6>171800300321</std6><stq7>40</stq7><stt7>34</stt7><std7>172300300321</std7><stq8>46</stq8><stt8>39</stt8><std8>172800300321</std8><stq9>51</stq9><stt9>44</stt9><std9>173300300321</std9><stq10>58</stq10><stt10>49</stt10><std10>173800300321</std10><stq11>63</stq11><stt11>54</stt11><std11>174300300321</std11><stq12>69</stq12><stt12>59</stt12><std12>174800300321</std12><stq13>75</stq13><stt13>64</stt13><std13>175300300321</std13><stq14>80</stq14><stt14>69</stt14><std14>175800300321</std14><stq15>85</stq15><stt15>73</stt15><std15>180200300321</std15><Afi>21616699</Afi><Afr>-2</Afr><Afp>0</Afp></ap>
                    SortedList parametersM1Out = new SortedList();
                    
                    if (collectionM1Out.Count > 0)//No hay error generico
                    //if (OPSMessage(strM1In, iVirtualUnit, out strM1Out, nContractId))
                    {
                        strM1Out = collectionM1Out.Cast<string>().ToList().FirstOrDefault();
                        Logger_AddLogMessage(string.Format("SendM1::OPSMessageOut = {0}", strM1Out), LoggerSeverities.Info);
                        //SortedList parametersM1Out = new SortedList();
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

                    int resultM2Out = OPSMessage_M02Process(parametersM2In, strM2In, nContractId);
                    strM2Out = "<r>" + resultM2Out + "</r>";

                    if (resultM2Out > 0)//No hay error generico
                    //if (OPSMessage(strM2In, iVirtualUnit, out strM2Out, nContractId))
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

        /// <summary>
        /// Metodo para sustituir el proceso de pedir la información del Process de M01 vía servicio asmx (mirar Process de M01)
        /// Internamente se realizan varios procesos pero en ARINPARK sólo se usa el primero:
        /// 1- Inclusión en el xml final de los datos de la ejecución del CS_M1.Exectue (se ha dejado como estaba). Llama a su vez a OPSComputeM1
        /// 2- Inclusión en el xml final de los datos de la factura (para los _date, _vehicleId, nContractId dados) llamando a CalculateInfoAboutPlateFines_M05Process (sustituyendo el Process de M05 vía servicio asmx)
        /// 3- ApplyCoupons
        /// 4- ProcessParticularSystemIdentifiers
        /// 5- CheckVAOCards
        /// 6- CameraCheckArticleDef
        /// </summary>
        /// <param name="parametersM1In"></param>
        /// <param name="strM1In"></param>
        /// <param name="nContractId"></param>
        /// <returns></returns>
        public StringCollection OPSMessage_M01Process(SortedList parametersM1In, string strM1In, int nContractId)
        {
            int ERROR_NOERROR = Convert.ToInt32(ConfigurationManager.AppSettings["M01.ErrorCodes.NoError"].ToString());
            int CAD_UNDEFINED = -1;
            int M1_CARD_ALREADY_USED = -14; // User Card Already used in a current parking

            DateTime _date = DateTime.Now;
            DateTime _dateMax = DateTime.Now;
            string fecha = (parametersM1In.IndexOfKey("d") == -1) ? "" :parametersM1In.GetByIndex(parametersM1In.IndexOfKey("d")).ToString();
            string fechaMax = (parametersM1In.IndexOfKey("d2") == -1) ? "" : parametersM1In.GetByIndex(parametersM1In.IndexOfKey("d2")).ToString();
            if (fecha != "") _date = DateTime.ParseExact(fecha, "HHmmssddMMyy", null);
            if (fechaMax != "") _dateMax = DateTime.ParseExact(fechaMax, "HHmmssddMMyy", null);
            int _articleDefId = (parametersM1In.IndexOfKey("ad") == -1) ? 0 : Convert.ToInt32(parametersM1In.GetByIndex(parametersM1In.IndexOfKey("ad")).ToString());
            string _computeDllPath = (parametersM1In.IndexOfKey("dll") == -1) ? "" : (string)parametersM1In.GetByIndex(parametersM1In.IndexOfKey("dll"));
            string _vehicleId = (parametersM1In.IndexOfKey("m") == -1) ? "" : (string)parametersM1In.GetByIndex(parametersM1In.IndexOfKey("m"));
            int _cad = (parametersM1In.IndexOfKey("cad") == -1) ? CAD_UNDEFINED : Convert.ToInt32(parametersM1In.GetByIndex(parametersM1In.IndexOfKey("cad")).ToString());
            int _iNumCoupons=0;
            int C_MAX_COUPONS = 5;
            uint[] _ReturnCouponsId= new uint[C_MAX_COUPONS];
            int[] _ReturnCouponsError= new int[C_MAX_COUPONS];

            //  StringCollection result = null;
            //	result = new StringCollection();
            //	result.Add("<p><m1><r>1</r><q1>0</q1><q2>554</q2><t>202</t><d>180849171104</d></m1></p>");
            //	return result;
            StringCollection result = new StringCollection();
            //m_logger = DatabaseFactory.Logger;
            try
            {
                Logger_AddLogMessage("[Msg01]:Processing Message", LoggerSeverities.Debug);

                string outxml = "";
                int iError = ERROR_NOERROR;
                DateTime originalDate = _date;
                bool bVAOCardsOK = true;// CheckVAOCards();
                bool bCamaraCheck = true;// CameraCheckArticleDef(ref outxml, ref iError);

                //EN ARINPARK SIEMPRE bVAOCardsOK = true y bCamaraCheck = true
                if ((bVAOCardsOK) && (bCamaraCheck))
                {
                    if (iError == ERROR_NOERROR)
                    {

                        CS_M1 pCS_M1 = new CS_M1();

                        string strIn = strM1In;// this._docXml.InnerXml;
                        int startPos = strIn.IndexOf("<d>");
                        int endPos;

                        if (startPos > 0)
                        {
                            startPos += 3;
                            endPos = strIn.IndexOf("</d>", startPos);

                            if (endPos > 0)
                            {
                                endPos -= 1;

                                strIn = strIn.Substring(0, startPos) +
                                    Dtx.DtxToString(_date) +
                                    strIn.Substring(endPos + 1, strIn.Length - endPos - 1);
                            }

                        }

                        startPos = strIn.IndexOf("<ad>");

                        if (startPos > 0)
                        {
                            startPos += 4;
                            endPos = strIn.IndexOf("</ad>", startPos);

                            if (endPos > 0)
                            {
                                endPos -= 1;

                                strIn = strIn.Substring(0, startPos) +
                                    _articleDefId.ToString() +
                                    strIn.Substring(endPos + 1, strIn.Length - endPos - 1);
                            }

                        }


                        if (_dateMax > DateTime.MinValue)
                        {
                            TimeSpan ts = _dateMax - _date;
                            if (Math.Abs(ts.TotalSeconds) < 60)
                            {
                                TimeSpan tsSubs = new TimeSpan(0, 0, 0, 60, 0);
                                _date = _date.Subtract(tsSubs);

                                startPos = strIn.IndexOf("<d>");

                                if (startPos > 0)
                                {
                                    startPos += 3;
                                    endPos = strIn.IndexOf("</d>", startPos);

                                    if (endPos > 0)
                                    {
                                        endPos -= 1;

                                        strIn = strIn.Substring(0, startPos) +
                                            Dtx.DtxToString(_date) +
                                            strIn.Substring(endPos + 1, strIn.Length - endPos - 1);
                                    }

                                }

                            }

                        }

                        //NO HAY CUPONES EN ARINPARK
                        //strIn = ApplyCoupons(strIn);


                        pCS_M1.StrIn = strIn;
                        pCS_M1.ApplyHistory = true;
                        pCS_M1.UseDefaultArticleDef = false;
                        if (_computeDllPath.Length > 0)
                            pCS_M1.StrComputeDllPath = _computeDllPath;

                        Logger_AddLogMessage("[Msg01]:Process Parsing" + pCS_M1.StrIn.ToString(), LoggerSeverities.Debug);

                        if (pCS_M1.Exectue() != CS_M1.C_RES_OK)
                        {
                            Logger_AddLogMessage("[Msg01]:Process Parsing " + "Error Execute", LoggerSeverities.Debug);
                            //return ReturnNack(NackMessage.NackTypes.NACK_SEMANTIC);
                        }
                        Logger_AddLogMessage("[Msg01]:Process Parsing : Result" + pCS_M1.StrOut.ToString(), LoggerSeverities.Debug);

                        //NO HAY NINGUN SYSTEM TIPO SYSTEM_IDENTIFIER_ZARAGOZA EN ARINPARK
                        //ProcessParticularSystemIdentifiers(ref outxml);

                        //StringCollection result = 
                        string strRes = pCS_M1.StrOut.ToString().Replace("</ap>", outxml + "</ap>");

                        //Has plate fines


                        //EL CODIGO ORIGINAL CalculateInfoAboutPlateFines DEVUELVE SI EXISTE LOS DATOS DE 1 FACTURA PARA LOS _date, _vehicleId, nContractId DADOS
                        //SIN EMBARGO NO SIRVE DE NADA YA QUE EN LOS parametersOutMapping NO ESTAN DEFINIDOS afi,afr,afp (ID,RESULT y PAYABLE) EN QueryParkingOperationWithTimeStepsXML, POR LO QUE NUNCA SE DEVOLVERAN EN EL XMLOUT
                        //PARA SACAR ESOS DATOS BASTA CON DEFINIRLOS EN QueryParkingOperationWithTimeStepsXML
                        //EN ARINPARK SE OPTADO POR NO CALCULARLOS SIQUIERA
                        //SI SE QUIEREN OBTENER BASTA CON DESCOMENTAR EL CÓDIGO SIGUIENTE
                        //if (_vehicleId.Length > 0)
                        //{
                        //    //CalculateInfoAboutPlateFines(ref outxml);
                        //    SortedList parametersM5Out = CalculateInfoAboutPlateFines_M05Process(_date, _vehicleId, nContractId);
                        //    if (parametersM5Out["r"].ToString() != "-99")//No hay error generico
                        //    {
                        //        outxml = "<Afi>" + parametersM5Out["f"].ToString() + "</Afi><Afr>" + parametersM5Out["r"].ToString() + "</Afr><Afp>" + parametersM5Out["p"].ToString() + "</Afp>";
                        //        strRes = strRes.Replace("</ap>", outxml + "</ap>");
                        //    }                                
                        //}

                        //EN ARINPARK SIEMPRE _cad = CAD_UNDEFINED
                        if (_cad != CAD_UNDEFINED)
                        {

                            startPos = strRes.IndexOf("<Ad>");

                            if (startPos > 0)
                            {
                                startPos += 4;
                                endPos = strRes.IndexOf("</Ad>", startPos);

                                if (endPos > 0)
                                {
                                    endPos -= 1;

                                    strRes = strRes.Substring(0, startPos) +
                                        Dtx.DtxToString(_date) +
                                        strRes.Substring(endPos + 1, strRes.Length - endPos - 1);
                                }

                            }
                            else
                            {
                                strRes = strRes.Replace("</ap>", "<Ad>" + Dtx.DtxToString(_date) + "</Ad></ap>");

                            }
                        }



                        /*
                         * 		private int _iNumCoupons=0;
                                private uint[] _ReturnCouponsId= new uint[C_MAX_COUPONS];
                                private int[] _ReturnCouponsError= new int[C_MAX_COUPONS];
                                private int _TotalNumFreeMinutes=0;
                                private int _TotalNumFreeCents=0;
                         */

                        //EN ARINPARK SIEMPRE _iNumCoupons = 0
                        if (_iNumCoupons > 0)
                        {
                            for (int i = 0; i < _iNumCoupons; i++)
                            {
                                strRes = strRes.Replace("</ap>", string.Format("<Acpi{0}>{1}</Acpi{0}><Acpr{0}>{2}</Acpr{0}></ap>", i + 1, _ReturnCouponsId[i], _ReturnCouponsError[i]));
                            }
                        }


                        result.Add(strRes);

                        Logger_AddLogMessage("[Msg01]:Process Parsing : Result StringCollection" + result.ToString(), LoggerSeverities.Debug);

                    }
                    else
                    {
                        System.Text.StringBuilder ret = new System.Text.StringBuilder();
                        ret.Append("<Ar>" + iError.ToString() + "</Ar>");
                        ret.Append(outxml);
                        //string strResult = new AckMessage(_msgId, ret.ToString()).ToString();
                        //result.Add(strResult);
                        //Logger_AddLogMessage("[Msg01]:Process Parsing : Result" + strResult, LoggerSeverities.Debug);
                        Logger_AddLogMessage("[Msg01]:Process Parsing : Result" + ret.ToString(), LoggerSeverities.Debug);
                    }

                }
                else if (!bVAOCardsOK)
                {
                    System.Text.StringBuilder ret = new System.Text.StringBuilder();
                    iError = M1_CARD_ALREADY_USED;
                    ret.Append("<Ar>" + iError.ToString() + "</Ar>");
                    //string strResult = new AckMessage(_msgId, ret.ToString()).ToString();
                    //result.Add(strResult);
                    //Logger_AddLogMessage("[Msg01]:Process Parsing : Result" + strResult, LoggerSeverities.Debug);
                    Logger_AddLogMessage("[Msg01]:Process Parsing : Result" + ret.ToString(), LoggerSeverities.Debug);
                }
                else
                {
                    System.Text.StringBuilder ret = new System.Text.StringBuilder();
                    iError = -1;
                    ret.Append("<Ar>" + iError.ToString() + "</Ar>");
                    //string strResult = new AckMessage(_msgId, ret.ToString()).ToString();
                    //result.Add(strResult);
                    //Logger_AddLogMessage("[Msg01]:Process Parsing : Result" + strResult, LoggerSeverities.Debug);
                    Logger_AddLogMessage("[Msg01]:Process Parsing : Result" + ret.ToString(), LoggerSeverities.Debug);
                }


            }
            catch (Exception e)
            {
                Logger_AddLogMessage("[Msg01]:Process:Exception " + e.Message, LoggerSeverities.Debug);
            }

            return result;

        }

        public int OPSMessage_M02Process(SortedList parametersM2In, string strM2In, int nContractId)
        {
            /// Returns 0 = 0K, -1 = ERR, 1 = OPERACION YA EXISTENTE
            int nInsOperRdo = -1;

            //ILogger logger = null;
            IDbTransaction tran = null;
            //logger = DatabaseFactory.Logger;
            Logger_AddLogMessage("[Msg02:Process]", LoggerSeverities.Debug);

            CultureInfo culture = new CultureInfo("", false);

            int OPERATIONS_DEF_PARKING = Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Parking"].ToString());
            int OPERATIONS_DEF_EXTENSION = Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Extension"].ToString());
            int OPERATIONS_DEF_REFUND = Convert.ToInt32(ConfigurationManager.AppSettings["OperationsDef.Refund"].ToString());
            int OPERATIONS_GUARD_CLOCK_IN = 100;
            int OPERATIONS_UPLOCK_OPEN = 105;
            int OPERATIONS_DOWNLOCK_OPEN = 106;
            int OPERATIONS_BILLREADER_REFUNDRECEIPT = 107;

            int _operationDefId = Convert.ToInt32(parametersM2In["y"]); 
            int _unitId = Convert.ToInt32(parametersM2In["u"]);
            DateTime _date = OPS.Comm.Dtx.StringToDtx(parametersM2In["d"].ToString());
            DateTime _dateIni = OPS.Comm.Dtx.StringToDtx(parametersM2In["d1"].ToString());
            DateTime _dateEnd = OPS.Comm.Dtx.StringToDtx(parametersM2In["d2"].ToString());            
            int _time = Convert.ToInt32(parametersM2In["t"]);
            int _operationId = Convert.ToInt32(parametersM2In["o"]);
            double _quantity = Convert.ToDouble(parametersM2In["q"], (IFormatProvider)culture.NumberFormat);       
            int _groupId = Convert.ToInt32(parametersM2In["g"]);
            int _articleDefId = Convert.ToInt32(parametersM2In["ad"]);
            int _paytypeDefId = Convert.ToInt32(parametersM2In["p"]);
            int _iPostPay = Convert.ToInt32(parametersM2In["pp"]);
            int _iOS = Convert.ToInt32(parametersM2In["os"]);
            int _mobileUserId = Convert.ToInt32(parametersM2In["mui"]);
            string _szCloudId = parametersM2In["cid"].ToString();
            string _vehicleId = parametersM2In["m"].ToString();
            int _onlineMessage = Convert.ToInt32(parametersM2In["om"]);

            double _dLatitud = (parametersM2In["lt"] == null) ? 0 : Convert.ToDouble(parametersM2In["lt"], (IFormatProvider)culture.NumberFormat);//
            double _dLongitud = (parametersM2In["lg"] == null) ? 0 : Convert.ToDouble(parametersM2In["lg"], (IFormatProvider)culture.NumberFormat);//
            int _iSpaceId = (parametersM2In["spcid"] == null) ? 0 : Convert.ToInt32(parametersM2In["spcid"]);// 
            string _szReference = (parametersM2In["ref"] == null) ? "0" : parametersM2In["ref"].ToString();//

            DateTime _dtExpirDate = OPS.Comm.Dtx.StringToDtx(parametersM2In["td"].ToString());//
            uint _ulChipCardId = Convert.ToUInt32(parametersM2In["chi"]);//
            double _dChipCardCredit = Convert.ToDouble(parametersM2In["chc"], (IFormatProvider)culture.NumberFormat);//
            double _quantityReal = Convert.ToDouble(parametersM2In["rq"], (IFormatProvider)culture.NumberFormat);//
            int _quantityReturned = Convert.ToInt32(parametersM2In["qr"]);//  
            int _binType = Convert.ToInt32(parametersM2In["bt"]);//
            int _articleId = Convert.ToInt32(parametersM2In["a"]);//
            int _paytypeDefIdVis = Convert.ToInt32(parametersM2In["pvis"]);//           
            string _szCCName = parametersM2In["tm"].ToString();// 
            string _szCCNumber = parametersM2In["tn"].ToString();//
            string _szRechargeCCNumber = parametersM2In["rtn"].ToString();//
            string _szRechargeCCName = parametersM2In["rtm"].ToString();//
            string _szCCCodServ = parametersM2In["ts"].ToString();//
            string _szCCDiscData = parametersM2In["tdd"].ToString();//
            string _vaoCard1 = parametersM2In["vci1"].ToString();//
            string _vaoCard2 = parametersM2In["vci2"].ToString();//
            string _vaoCard3 = parametersM2In["vci3"].ToString();//
            string _coid = parametersM2In["coid"].ToString();//  
            int _ticketNumber = Convert.ToInt32(parametersM2In["tcn"]);//

            int _nStatus = -1;
            int STATUS_INSERT = 0;

            int _iNumCoupons = 0;
            int C_MAX_COUPONS = 5;
            uint[] _couponsId = new uint[C_MAX_COUPONS];
            int[] _ReturnCouponsError = new int[C_MAX_COUPONS];

            try
            {

                if (_operationDefId == OPERATIONS_GUARD_CLOCK_IN)
                {
                    CmpClockInDB cmp = new CmpClockInDB();
                    if (cmp.Insert(Convert.ToInt32(_vehicleId), _unitId, _date) < 0)
                    {
                        Logger_AddLogMessage("[Msg02:Process]:ERROR ON INSERT", LoggerSeverities.Debug);
                        //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                    }
                    else
                    {
                        Logger_AddLogMessage("[Msg02:Process]: RESULT OK", LoggerSeverities.Debug);
                        //return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
                    }

                }
                else if ((_operationDefId == OPERATIONS_UPLOCK_OPEN) || (_operationDefId == OPERATIONS_DOWNLOCK_OPEN))
                {

                    OracleConnection oraDBConn = null;
                    OracleCommand oraCmd = null;
                    OracleCommand selCmd = null;

                    try
                    {
                        Logger_AddLogMessage("[Msg02:Process]: UP OR DOWN LOCK OPENNING", LoggerSeverities.Info);

                        //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                        //						logger = DatabaseFactory.Logger;

                        string sConn = ConfigurationManager.AppSettings["ConnectionString" + nContractId.ToString()].ToString();
                        if (sConn == null)
                            throw new Exception("No ConnectionString configuration");

                        oraDBConn = new OracleConnection(sConn);
                        oraCmd = new OracleCommand();
                        oraCmd.Connection = oraDBConn;
                        oraCmd.Connection.Open();

                        tran = oraDBConn.BeginTransaction(IsolationLevel.Serializable);

                        String selectUE = String.Format("select count(*) from USER_EVENTS where ue_uni_id={0} and ue_ope_id={1}", _unitId, _operationId);

                        selCmd = new OracleCommand();
                        selCmd.Connection = (OracleConnection)oraDBConn;
                        selCmd.CommandText = selectUE;
                        selCmd.Transaction = (OracleTransaction)tran;

                        if (oraDBConn.State == System.Data.ConnectionState.Open)
                        {
                            int iNumRegs = Convert.ToInt32(selCmd.ExecuteScalar());

                            if (iNumRegs == 0)
                            {


                                String updateUE = String.Format("insert into USER_EVENTS (UE_DUE_ID, UE_UNI_ID, UE_DATE, UE_USER_ID, UE_OPE_ID) values " +
                                    "({0},{1}, to_date('{2}','hh24missddmmyy'),{3},{4})",
                                    _operationDefId,
                                    _unitId,
                                    OPS.Comm.Dtx.DtxToString(_date),
                                    _ulChipCardId,
                                    _operationId);

                                oraCmd = new OracleCommand();
                                oraCmd.Connection = (OracleConnection)oraDBConn;
                                oraCmd.CommandText = updateUE;
                                oraCmd.Transaction = (OracleTransaction)tran;

                                if (oraCmd.ExecuteNonQuery() != 1)
                                {

                                    if (oraCmd != null)
                                    {
                                        oraCmd.Dispose();
                                        oraCmd = null;
                                    }

                                    if (selCmd != null)
                                    {
                                        selCmd.Dispose();
                                        selCmd = null;
                                    }

                                    RollbackTrans(tran);

                                    Logger_AddLogMessage("[Msg02:Process]: Error executing sql " + updateUE, LoggerSeverities.Debug);

                                    //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                }
                            }
                        }
                        else
                        {
                            if (oraCmd != null)
                            {
                                oraCmd.Dispose();
                                oraCmd = null;
                            }

                            if (selCmd != null)
                            {
                                selCmd.Dispose();
                                selCmd = null;
                            }

                            RollbackTrans(tran);

                            Logger_AddLogMessage("[Msg02:Process]: Connection not opened", LoggerSeverities.Debug);

                            //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                        }


                        if (oraCmd != null)
                        {
                            oraCmd.Dispose();
                            oraCmd = null;
                        }

                        if (selCmd != null)
                        {
                            selCmd.Dispose();
                            selCmd = null;
                        }

                        CommitTrans(tran);

                        Logger_AddLogMessage("[Msg02:Process]: RESULT OK", LoggerSeverities.Debug);
                        //return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
                    }
                    catch (Exception exc)
                    {
                        if (oraCmd != null)
                        {
                            oraCmd.Dispose();
                            oraCmd = null;
                        }

                        if (selCmd != null)
                        {
                            selCmd.Dispose();
                            selCmd = null;
                        }

                        RollbackTrans(tran);

                        Logger_AddLogMessage("[Msg02:Process]" + exc.Message, LoggerSeverities.Debug);
                        //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS); ;
                    }
                }
                else if (_operationDefId == OPERATIONS_BILLREADER_REFUNDRECEIPT)
                {
                    CmpBillReaderRefundsDB cmp = new CmpBillReaderRefundsDB();
                    if (cmp.Insert(_unitId, _date, Convert.ToInt32(_quantity)) < 0)
                    {
                        Logger_AddLogMessage("[Msg02:Process]:ERROR ON INSERT", LoggerSeverities.Debug);
                        //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                    }
                    else
                    {
                        Logger_AddLogMessage("[Msg02:Process]: RESULT OK", LoggerSeverities.Debug);
                        //return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
                    }
                }
                else
                {

                    if (_groupId == -1)     // If  no group <g> passed search the 1st physical parent...
                    {
                        _groupId = new CmpGroupsChildsDB().GetFirstPhysicalParent(_unitId);
                        if (_groupId == -1) // If no group found that is an error...
                        {
                            //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                        }
                    }

                    int iBinFormat = -1;
                    if (_binType == -1)
                    {
                        CmpParametersDB cmpParam = new CmpParametersDB();
                        string strBinFormat = cmpParam.GetParameter("P_BIN_FORMAT");
                        if (strBinFormat != "")
                        {
                            iBinFormat = Convert.ToInt32(strBinFormat);
                        }
                    }
                    else
                    {
                        iBinFormat = _binType;
                    }

                    ILogger logger = DatabaseFactory.Logger;
                    CmpOperationsDB cmp = new CmpOperationsDB();
                    if (!Msg07.ListaNegra(logger, _szCCNumber, iBinFormat))
                    {

                        int nNewOperationID = 0;
                        int iRealTime = -1;
                        int iQuantity = -1;

                        if ((_operationDefId == OPERATIONS_DEF_PARKING) || (_operationDefId == OPERATIONS_DEF_EXTENSION))
                        {
                            GetM2CompData(_vehicleId, _groupId, _dateIni, _dateEnd, _articleDefId, _unitId, ref iRealTime, ref iQuantity);
                        }
                        else if (_operationDefId == OPERATIONS_DEF_REFUND)
                        {
                            GetM2CompData(_vehicleId, _groupId, _dateIni, _dateEnd, _articleDefId, _unitId, ref iRealTime, ref iQuantity);
                            _quantityReturned = iQuantity;
                        }

                        /// Returns 0 = 0K, -1 = ERR, 1 = OPERACION YA EXISTENTE
                        nInsOperRdo = cmp.InsertOperation(_operationDefId, _operationId, _articleId, _groupId, _unitId, _paytypeDefId, _date,
                            _dateIni, _dateEnd, _time, _quantity, _vehicleId, _articleDefId, _mobileUserId, _iPostPay, _dChipCardCredit, _ulChipCardId, -1, -1, iRealTime, _quantityReturned, _onlineMessage, _ticketNumber, ref nNewOperationID, out tran);


                        //if(nInsOperRdo==1)
                        //{
                        //	return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
                        //}
                        //else
                        if ((nInsOperRdo == 0) && (_iPostPay == 1))
                        //Antes era ( nInsOperRdo != -1 ), pero en el caso de ser 1, el método no le asigna un valor a trans, y por lo tanto las siguientes líneas dan una excepción
                        {
                            CFineManager oFineManager = new CFineManager();
                            oFineManager.SetLogger(logger);

                            oFineManager.SetDBTransaction(tran);
                            oFineManager.RevokeFinesWithPostpay(nNewOperationID);

                        }

                        if (nInsOperRdo == 0)
                        {


                            if (!UpdatePaymentTypeVis(_paytypeDefIdVis, nNewOperationID, tran))
                            {
                                RollbackTrans(tran);
                                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                            }

                            if (_iPostPay == 1)
                            {
                                CFineManager oFineManager = new CFineManager();
                                oFineManager.SetLogger(logger);
                                oFineManager.SetDBTransaction(tran);
                                oFineManager.RevokeFinesWithPostpay(nNewOperationID);
                            }

                            if (!UpdateVAOCards(_vaoCard1, _vaoCard2, _vaoCard3, nNewOperationID, tran))
                            {

                                RollbackTrans(tran);
                                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                            }

                            if (!UpdateCOID(_coid, nNewOperationID, tran))
                            {

                                RollbackTrans(tran);
                                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                            }


                            if (_iNumCoupons > 0)
                            {
                                CmpMoneyOffCoupons cmpCoupons = new CmpMoneyOffCoupons();
                                for (int i = 0; i < _iNumCoupons; i++)
                                {
                                    if (cmpCoupons.SetCouponAsUsed(tran, _couponsId[i], _date, _vehicleId, _paytypeDefId) <= 0)
                                    {
                                        RollbackTrans(tran);
                                        //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                    }
                                    if (!UpdateMoneyOffDiscount(nNewOperationID, i + 1, _couponsId[i], tran))
                                    {
                                        RollbackTrans(tran);
                                        //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                    }
                                }
                                if (!UpdateValueVis(nNewOperationID, _quantityReal, tran))
                                {
                                    RollbackTrans(tran);
                                    //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                }

                            }

                            if (!InsertGPSPosn(_dLatitud, _dLongitud, nNewOperationID, tran))
                            {
                                RollbackTrans(tran);
                                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                            }

                            if (!UpdateReference(_szReference, nNewOperationID, tran))
                            {
                                RollbackTrans(tran);
                                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                            }

                            if (!UpdateCloudData(_mobileUserId, _szCloudId, _iOS, nNewOperationID, tran))
                            {
                                RollbackTrans(tran);
                                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                            }

                            if (!UpdateSpaceInfo(_iSpaceId, nNewOperationID, tran))
                            {
                                RollbackTrans(tran);
                                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                            }

                            OPS.Components.Data.CmpCreditCardDB cmpCreditCard = null;
                            cmpCreditCard = new OPS.Components.Data.CmpCreditCardDB();

                            if (cmpCreditCard == null)
                            {
                                RollbackTrans(tran);
                                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                            }

                            try
                            {
                                if (_szCCNumber != "")
                                {
                                    if (logger != null)
                                        logger.AddLog("[Msg02:Process]: Operation WITH Card Id", LoggerSeverities.Debug);

                                    CmpCreditCardsTransactionsDB cmpCreditCardsTransactionsDB = new CmpCreditCardsTransactionsDB();
                                    if (_szCCNumber == "CCZ_OPERATIONID")
                                    {
                                        //szCCName contiene el número de transacción
                                        //nNewOperationID
                                        OracleConnection oraDBConn = null;
                                        OracleCommand oraCmd = null;


                                        //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                                        //logger = DatabaseFactory.Logger;
                                        oraDBConn = (OracleConnection)tran.Connection;


                                        //
                                        string state = String.Empty;
                                        string selectMFT = "select mft_status from mifare_transaction where MFT_UNI_TRANS_ID = " + _szCCName;
                                        selectMFT += " and MFT_UNI_ID = " + _unitId;

                                        if (oraDBConn.State == System.Data.ConnectionState.Open)
                                        {
                                            oraCmd = new OracleCommand();
                                            oraCmd.Connection = (OracleConnection)oraDBConn;
                                            oraCmd.CommandText = selectMFT;
                                            oraCmd.Transaction = (OracleTransaction)tran;

                                            OracleDataReader rd = oraCmd.ExecuteReader();

                                            while (rd.Read())
                                            {
                                                int i = rd.GetOrdinal("MFT_STATUS");
                                                state = (rd.GetInt32(rd.GetOrdinal("MFT_STATUS"))).ToString();
                                            }

                                        }
                                        else
                                        {
                                            RollbackTrans(tran);
                                            //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                        }

                                        if (state == "20")
                                        {
                                            string updateMFT = "update mifare_transaction ";
                                            updateMFT += "set mft_status = 30, mft_ope_id = " + nNewOperationID.ToString();
                                            updateMFT += "  where MFT_UNI_TRANS_ID= " + _szCCName + " and MFT_UNI_ID = " + _unitId;

                                            if (oraDBConn.State == System.Data.ConnectionState.Open)
                                            {
                                                oraCmd = new OracleCommand();
                                                oraCmd.Connection = (OracleConnection)oraDBConn;
                                                oraCmd.CommandText = updateMFT;
                                                oraCmd.Transaction = (OracleTransaction)tran;
                                                int numRowsAffected = oraCmd.ExecuteNonQuery();

                                                if (numRowsAffected == 0)
                                                {
                                                    RollbackTrans(tran);
                                                    //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                                }
                                            }
                                            else
                                            {
                                                RollbackTrans(tran);
                                                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);

                                            }
                                        }
                                        else
                                        {
                                            RollbackTrans(tran);
                                            //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                        }

                                        if (_szRechargeCCNumber != "")
                                        {
                                            Logger_AddLogMessage("[Msg02:Process]: Operation WITH Card Id", LoggerSeverities.Debug);
                                            _nStatus = STATUS_INSERT;
                                            if (cmpCreditCard.Insert(tran, nNewOperationID, _szRechargeCCNumber, _szRechargeCCName, _dtExpirDate, _nStatus, _szCCCodServ, _szCCDiscData) < 0)
                                            {
                                                RollbackTrans(tran);
                                                Logger_AddLogMessage("[Msg02:Process]:ERROR ON INSERT", LoggerSeverities.Debug);
                                                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                            }
                                            else
                                            {
                                                Logger_AddLogMessage("[Msg02:Process]: RESULT OK", LoggerSeverities.Debug);
                                            }
                                        }


                                    }
                                    else if (iBinFormat == Msg07.DEF_BIN_FORMAT_EMV_TAS && _szCCNumber == "TRANSACTION_ID")
                                    {
                                        int iTransId = -1;
                                        if (cmpCreditCardsTransactionsDB.InsertCommitTrans(tran, _szCCName, _date, nNewOperationID, Convert.ToInt32(_quantity), _unitId, _vehicleId, out iTransId) < 0)
                                        {
                                            RollbackTrans(tran);
                                            Logger_AddLogMessage("[Msg02:Process]:ERROR ON INSERT", LoggerSeverities.Debug);
                                            //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                        }
                                        else
                                        {
                                            Logger_AddLogMessage("[Msg02:Process]: RESULT OK", LoggerSeverities.Debug);
                                        }
                                    }
                                    else if (iBinFormat == Msg07.DEF_BIN_FORMAT_EMV_TAS && _szCCNumber != "TRANSACTION_ID")
                                    {
                                        RollbackTrans(tran);
                                        Logger_AddLogMessage("[Msg02:Process]:TRANSACTION ID IS NOT ATTACHED", LoggerSeverities.Debug);
                                        //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                    }
                                    else
                                    {
                                        _nStatus = STATUS_INSERT;
                                        if (cmpCreditCard.Insert(tran, nNewOperationID, _szCCNumber, _szCCName, _dtExpirDate, _nStatus, _szCCCodServ, _szCCDiscData) < 0)
                                        {
                                            RollbackTrans(tran);
                                            Logger_AddLogMessage("[Msg02:Process]:ERROR ON INSERT", LoggerSeverities.Debug);
                                            //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                        }
                                        else
                                        {
                                            Logger_AddLogMessage("[Msg02:Process]: RESULT OK", LoggerSeverities.Debug);
                                        }
                                    }
                                }
                                else if (_szRechargeCCNumber != "")
                                {
                                    Logger_AddLogMessage("[Msg02:Process]: Operation WITH Card Id", LoggerSeverities.Debug);
                                    _nStatus = STATUS_INSERT;
                                    if (cmpCreditCard.Insert(tran, nNewOperationID, _szRechargeCCNumber, _szRechargeCCName, _dtExpirDate, _nStatus, _szCCCodServ, _szCCDiscData) < 0)
                                    {
                                        RollbackTrans(tran);
                                        Logger_AddLogMessage("[Msg02:Process]:ERROR ON INSERT", LoggerSeverities.Debug);
                                        //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                                    }
                                    else
                                    {
                                        Logger_AddLogMessage("[Msg02:Process]: RESULT OK", LoggerSeverities.Debug);
                                    }
                                }
                                else
                                {
                                    Logger_AddLogMessage("[Msg02:Process]: Operation WITHOUT Card Id", LoggerSeverities.Debug);
                                }
                            }
                            catch (Exception exc)
                            {
                                Logger_AddLogMessage("[Msg02:Process]" + exc.Message, LoggerSeverities.Debug);
                                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS); ;
                            }
                        }
                        else if (nInsOperRdo == 1)
                        {
                            Logger_AddLogMessage("[Msg02:Process]: Operation already exists in DB", LoggerSeverities.Debug);
                            //return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
                        }
                        else
                        {
                            RollbackTrans(tran);
                            //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                        }
                    }
                    else
                    {
                        RollbackTrans(tran);
                        if (!InsertFraudMsgs(_unitId, _date, _szCCNumber, _szCCName, _dtExpirDate, _vehicleId, strM2In, tran))
                        {
                            //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                        }
                    }
                    

                    CommitTrans(tran);
                    //return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
                }

            }
            catch (Exception e)
            {
                Logger_AddLogMessage("[Msg02:Process] EXCEPTION: " + e.ToString(), LoggerSeverities.Debug);
                //return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
            }
            return nInsOperRdo;
        }

        private bool GetM2CompData(string _vehicleId, int _groupId, DateTime _dateIni, DateTime _dateEnd, int _articleDefId, int _unitId, ref int iResRealTime, ref int iResQuantity)
        {
            bool bRdo = true;
            int iResult = -1;
            int iQuantity = -1;
            int iRealTime = -1;

            try
            {
                string m1Tel;

                //m1Tel = "<m1 id=\"" + _msgId + "\">";
                m1Tel = "<m1 id=\"\">";
                m1Tel += "<m>" + _vehicleId + "</m>";
                m1Tel += "<g>" + _groupId.ToString() + "</g>";
                m1Tel += "<d>" + OPS.Comm.Dtx.DtxToString(_dateIni) + "</d>";
                m1Tel += "<d2>" + OPS.Comm.Dtx.DtxToString(_dateEnd) + "</d2>";
                m1Tel += "<ad>" + _articleDefId.ToString() + "</ad>";
                m1Tel += "<u>" + _unitId.ToString() + "</u>";
                m1Tel += "<o>1</o><rmon>0</rmon></m1>";

                CS_M1 pCS_M1 = new CS_M1();
                pCS_M1.StrIn = m1Tel;
                pCS_M1.ApplyHistory = false;
                pCS_M1.UseDefaultArticleDef = false;


                if (pCS_M1.Exectue() != CS_M1.C_RES_OK)
                {
                    Logger_AddLogMessage("[Msg02]:Process Parsing " + "Error Execute", LoggerSeverities.Debug);
                    bRdo = false;
                    return bRdo;
                }

                string m1Res = pCS_M1.StrOutM50.ToString();

                Logger_AddLogMessage("[Msg02]:Process Parsing : Result" + m1Res, LoggerSeverities.Debug);

                XmlDocument xmlM1Res = new XmlDocument();
                xmlM1Res.LoadXml(m1Res);

                XmlNode act;


                IEnumerator ienum = xmlM1Res.ChildNodes.Item(0).GetEnumerator();

                while (ienum.MoveNext())
                {
                    act = (XmlNode)ienum.Current;
                    switch (act.Name)
                    {

                        case "r":
                            iResult = int.Parse(act.InnerText);
                            break;
                        case "q2":
                            iQuantity = int.Parse(act.InnerText);
                            break;
                        case "rot":
                            iRealTime = int.Parse(act.InnerText);
                            break;
                        default:
                            break;

                    }
                }

                if (iResult > 0)
                {
                    if (iQuantity >= 0)
                    {
                        iResQuantity = iQuantity;
                    }
                    if (iRealTime >= 0)
                    {
                        iResRealTime = iRealTime;
                    }
                }
                else
                {
                    bRdo = false;
                }
            }
            catch
            {
                bRdo = false;
            }

            return bRdo;

        }

        private bool InsertFraudMsgs(int _unitId, DateTime _date, string _szCCNumber, string _szCCName, DateTime _dtExpirDate, string _vehicleId, string xml, IDbTransaction tran)
        {
            bool bOK = true;
            OracleConnection oraDBConn = null;
            OracleCommand oraCmd = null;
            ILogger logger = null;
            try
            {

                //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                logger = DatabaseFactory.Logger;
                oraDBConn = (OracleConnection)tran.Connection;
                if (oraDBConn.State == System.Data.ConnectionState.Open)
                {

                    oraCmd = new OracleCommand();
                    oraCmd.Connection = (OracleConnection)oraDBConn;
                    oraCmd.Transaction = (OracleTransaction)tran;

                    StringBuilder sqlQuery = new StringBuilder();
                    String strSQL = String.Format("insert into MSGS_XML_FRAUD_OPERATIONS (MXF_UNI_ID,MXF_MOVDATE,MXF_NUMBER,MXF_NAME,MXF_XPRTN_DATE,MXF_VEHICLEID,MXF_XML) values " +
                        "({0},to_date('{1}','hh24missddmmyy'),'{2}','{3}',to_date('{4}','hh24missddmmyy'),'{5}','{6}')",
                        _unitId,
                        OPS.Comm.Dtx.DtxToString(_date),
                        _szCCNumber,
                        _szCCName,
                        OPS.Comm.Dtx.DtxToString(_dtExpirDate),
                        _vehicleId,
                        xml //_root.OuterXml
                        );

                    sqlQuery.AppendFormat(strSQL);

                    oraCmd.CommandText = sqlQuery.ToString();

                    Logger_AddLogMessage(string.Format("[Msg02:Process]: Credit Card {0} is in blacklist", _szCCNumber), LoggerSeverities.Debug);

                    oraCmd.ExecuteNonQuery();



                }
            }
            catch (Exception e)
            {
                logger.AddLog("[Msg02:UpdateVAOCards]: Excepcion: " + e.Message, LoggerSeverities.Error);
                bOK = false;
            }
            finally
            {


                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }


            }
            return bOK;

        }

        bool UpdateVAOCards(string _vaoCard1, string _vaoCard2, string _vaoCard3, int iOperationID, IDbTransaction tran)
        {

            bool bOK = true;

            if ((_vaoCard1.Length > 0) || (_vaoCard2.Length > 0) || (_vaoCard3.Length > 0))
            {


                OracleConnection oraDBConn = null;
                OracleCommand oraCmd = null;
                ILogger logger = null;
                try
                {

                    //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                    logger = DatabaseFactory.Logger;
                    oraDBConn = (OracleConnection)tran.Connection;
                    if (oraDBConn.State == System.Data.ConnectionState.Open)
                    {

                        oraCmd = new OracleCommand();
                        oraCmd.Connection = (OracleConnection)oraDBConn;
                        oraCmd.Transaction = (OracleTransaction)tran;

                        StringBuilder sqlQuery = new StringBuilder();
                        sqlQuery.AppendFormat(" update operations o " +
                                                "set o.ope_vaocard1 = '{0}', o.ope_vaocard2 = '{1}', o.ope_vaocard3 = '{2}' " +
                                                "where ope_id = {3}", _vaoCard1, _vaoCard2, _vaoCard3, iOperationID);

                        oraCmd.CommandText = sqlQuery.ToString();

                        oraCmd.ExecuteNonQuery();



                    }
                }
                catch (Exception e)
                {
                    logger.AddLog("[Msg02:UpdateVAOCards]: Excepcion: " + e.Message, LoggerSeverities.Error);
                    bOK = false;
                }
                finally
                {


                    if (oraCmd != null)
                    {
                        oraCmd.Dispose();
                        oraCmd = null;
                    }


                }
            }

            return bOK;
        }


        bool UpdateMoneyOffDiscount(int iOperationID, int iCoupon, uint iCouponId, IDbTransaction tran)
        {

            bool bOK = true;




            OracleConnection oraDBConn = null;
            OracleCommand oraCmd = null;
            ILogger logger = null;
            try
            {

                //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                logger = DatabaseFactory.Logger;
                oraDBConn = (OracleConnection)tran.Connection;
                if (oraDBConn.State == System.Data.ConnectionState.Open)
                {

                    oraCmd = new OracleCommand();
                    oraCmd.Connection = (OracleConnection)oraDBConn;
                    oraCmd.Transaction = (OracleTransaction)tran;

                    StringBuilder sqlQuery = new StringBuilder();
                    sqlQuery.AppendFormat(" update operations o " +
                        "set o.OPE_COUP_ID_{0} = {1} " +
                        "where ope_id = {2}", iCoupon, iCouponId, iOperationID);

                    oraCmd.CommandText = sqlQuery.ToString();

                    oraCmd.ExecuteNonQuery();



                }
            }
            catch (Exception e)
            {
                logger.AddLog("[Msg02:UpdateVAOCards]: Excepcion: " + e.Message, LoggerSeverities.Error);
                bOK = false;
            }
            finally
            {


                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }


            }


            return bOK;
        }


        bool UpdateValueVis(int iOperationID, double dValueVis, IDbTransaction tran)
        {

            bool bOK = true;




            OracleConnection oraDBConn = null;
            OracleCommand oraCmd = null;
            ILogger logger = null;
            try
            {

                //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                logger = DatabaseFactory.Logger;
                oraDBConn = (OracleConnection)tran.Connection;
                if (oraDBConn.State == System.Data.ConnectionState.Open)
                {

                    oraCmd = new OracleCommand();
                    oraCmd.Connection = (OracleConnection)oraDBConn;
                    oraCmd.Transaction = (OracleTransaction)tran;

                    StringBuilder sqlQuery = new StringBuilder();
                    sqlQuery.AppendFormat(" update operations o " +
                        "set o.OPE_VALUE_VIS = {0} " +
                        "where ope_id = {1}", dValueVis, iOperationID);

                    oraCmd.CommandText = sqlQuery.ToString();

                    oraCmd.ExecuteNonQuery();



                }
            }
            catch (Exception e)
            {
                logger.AddLog("[Msg02:UpdateVAOCards]: Excepcion: " + e.Message, LoggerSeverities.Error);
                bOK = false;
            }
            finally
            {


                if (oraCmd != null)
                {
                    oraCmd.Dispose();
                    oraCmd = null;
                }


            }


            return bOK;
        }



        bool UpdatePaymentTypeVis(int _paytypeDefIdVis, int iOperationID, IDbTransaction tran)
        {

            bool bOK = true;

            if (_paytypeDefIdVis != -1)
            {


                OracleConnection oraDBConn = null;
                OracleCommand oraCmd = null;
                ILogger logger = null;
                try
                {

                    //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                    logger = DatabaseFactory.Logger;
                    oraDBConn = (OracleConnection)tran.Connection;
                    if (oraDBConn.State == System.Data.ConnectionState.Open)
                    {

                        oraCmd = new OracleCommand();
                        oraCmd.Connection = (OracleConnection)oraDBConn;
                        oraCmd.Transaction = (OracleTransaction)tran;

                        StringBuilder sqlQuery = new StringBuilder();
                        sqlQuery.AppendFormat(" update operations o " +
                            "set o.ope_dpay_id_vis = {0} " +
                            "where ope_id = {1}", _paytypeDefIdVis, iOperationID);

                        oraCmd.CommandText = sqlQuery.ToString();

                        oraCmd.ExecuteNonQuery();



                    }
                }
                catch (Exception e)
                {
                    logger.AddLog("[Msg02:UpdateVAOCards]: Excepcion: " + e.Message, LoggerSeverities.Error);
                    bOK = false;
                }
                finally
                {


                    if (oraCmd != null)
                    {
                        oraCmd.Dispose();
                        oraCmd = null;
                    }


                }
            }

            return bOK;
        }

        bool UpdateCOID(string _coid, int iOperationID, IDbTransaction tran)
        {

            bool bOK = true;

            if ((_coid.Length > 0))
            {


                OracleConnection oraDBConn = null;
                OracleCommand oraCmd = null;
                ILogger logger = null;
                try
                {

                    //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                    logger = DatabaseFactory.Logger;
                    oraDBConn = (OracleConnection)tran.Connection;
                    if (oraDBConn.State == System.Data.ConnectionState.Open)
                    {

                        oraCmd = new OracleCommand();
                        oraCmd.Connection = (OracleConnection)oraDBConn;
                        oraCmd.Transaction = (OracleTransaction)tran;

                        StringBuilder sqlQuery = new StringBuilder();
                        sqlQuery.AppendFormat(" update operations o " +
                            "set o.OPE_CAMOP_ID_ENTRY = {1} " +
                            "where ope_id = {0}", iOperationID, _coid);

                        oraCmd.CommandText = sqlQuery.ToString();

                        oraCmd.ExecuteNonQuery();



                    }
                }
                catch (Exception e)
                {
                    logger.AddLog("[Msg02:UpdateCOID]: Excepcion: " + e.Message, LoggerSeverities.Error);
                    bOK = false;
                }
                finally
                {


                    if (oraCmd != null)
                    {
                        oraCmd.Dispose();
                        oraCmd = null;
                    }


                }
            }

            return bOK;
        }

        bool InsertGPSPosn(double _dLatitud, double _dLongitud, int iOperationID, IDbTransaction tran)
        {
            bool bOK = true;

            if (_dLatitud != -999 && _dLongitud != -999)
            {
                CultureInfo culture = new CultureInfo("", false);
                OracleConnection oraDBConn = null;
                OracleCommand oraCmd = null;
                ILogger logger = null;
                try
                {
                    //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                    logger = DatabaseFactory.Logger;
                    oraDBConn = (OracleConnection)tran.Connection;
                    if (oraDBConn.State == System.Data.ConnectionState.Open)
                    {
                        oraCmd = new OracleCommand();
                        oraCmd.Connection = (OracleConnection)oraDBConn;
                        oraCmd.Transaction = (OracleTransaction)tran;

                        StringBuilder sqlQuery = new StringBuilder();
                        sqlQuery.AppendFormat(" update operations o " +
                            "set o.ope_latitude = {0}, o.ope_longitud = {1} " +
                            "where ope_id = {2}", Convert.ToString(_dLatitud, (IFormatProvider)culture.NumberFormat),
                            Convert.ToString(_dLongitud, (IFormatProvider)culture.NumberFormat), iOperationID);

                        oraCmd.CommandText = sqlQuery.ToString();

                        oraCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    logger.AddLog("[Msg02:InsertGPSPosn]: Excepcion: " + e.Message, LoggerSeverities.Error);
                    bOK = false;
                }
                finally
                {
                    if (oraCmd != null)
                    {
                        oraCmd.Dispose();
                        oraCmd = null;
                    }
                }
            }

            return bOK;
        }

        bool UpdateReference(string _szReference, int iOperationID, IDbTransaction tran)
        {
            bool bOK = true;

            if (_szReference.Length > 0)
            {
                OracleConnection oraDBConn = null;
                OracleCommand oraCmd = null;
                ILogger logger = null;
                try
                {
                    //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                    logger = DatabaseFactory.Logger;
                    oraDBConn = (OracleConnection)tran.Connection;
                    if (oraDBConn.State == System.Data.ConnectionState.Open)
                    {
                        oraCmd = new OracleCommand();
                        oraCmd.Connection = (OracleConnection)oraDBConn;
                        oraCmd.Transaction = (OracleTransaction)tran;

                        StringBuilder sqlQuery = new StringBuilder();
                        sqlQuery.AppendFormat(" update operations o " +
                            "set o.ope_reference = '{0}' " +
                            "where ope_id = {1}", _szReference, iOperationID);

                        oraCmd.CommandText = sqlQuery.ToString();

                        oraCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    logger.AddLog("[Msg02:UpdateReference]: Excepcion: " + e.Message, LoggerSeverities.Error);
                    bOK = false;
                }
                finally
                {
                    if (oraCmd != null)
                    {
                        oraCmd.Dispose();
                        oraCmd = null;
                    }
                }
            }

            return bOK;
        }

        bool UpdateCloudData(int _mobileUserId, string _szCloudId, int _iOS, int iOperationID, IDbTransaction tran)
        {
            bool bOK = true;

            if ((_mobileUserId > 0) && (_szCloudId.Length > 0 || _iOS > 0))
            {
                OracleConnection oraDBConn = null;
                OracleCommand oraCmd = null;
                ILogger logger = null;
                try
                {
                    //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                    logger = DatabaseFactory.Logger;
                    oraDBConn = (OracleConnection)tran.Connection;
                    if (oraDBConn.State == System.Data.ConnectionState.Open)
                    {
                        oraCmd = new OracleCommand();
                        oraCmd.Connection = (OracleConnection)oraDBConn;
                        oraCmd.Transaction = (OracleTransaction)tran;

                        StringBuilder sqlQuery = new StringBuilder();
                        if (_szCloudId.Length > 0 && _iOS > 0)
                        {
                            sqlQuery.AppendFormat(" update mobile_users m " +
                                "set m.mu_cloud_token = '{0}', m.mu_device_os = {1} " +
                                "where mu_id = {2}", _szCloudId, _iOS, _mobileUserId);
                        }
                        else if (_szCloudId.Length > 0)
                        {
                            sqlQuery.AppendFormat(" update mobile_users m " +
                                "set m.mu_cloud_token = '{0}' " +
                                "where mu_id = {1}", _szCloudId, _mobileUserId);
                        }
                        else
                        {
                            sqlQuery.AppendFormat(" update mobile_users m " +
                                "set m.mu_device_os = {0} " +
                                "where mu_id = {1}", _iOS, _mobileUserId);
                        }

                        oraCmd.CommandText = sqlQuery.ToString();

                        oraCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    logger.AddLog("[Msg02:UpdateCloudData]: Excepcion: " + e.Message, LoggerSeverities.Error);
                    bOK = false;
                }
                finally
                {
                    if (oraCmd != null)
                    {
                        oraCmd.Dispose();
                        oraCmd = null;
                    }
                }
            }

            return bOK;
        }

        bool UpdateSpaceInfo(int _iSpaceId, int iOperationID, IDbTransaction tran)
        {
            bool bOK = true;

            if (_iSpaceId > 0)
            {
                OracleConnection oraDBConn = null;
                OracleCommand oraCmd = null;
                ILogger logger = null;
                try
                {
                    //Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
                    logger = DatabaseFactory.Logger;
                    oraDBConn = (OracleConnection)tran.Connection;
                    if (oraDBConn.State == System.Data.ConnectionState.Open)
                    {
                        oraCmd = new OracleCommand();
                        oraCmd.Connection = (OracleConnection)oraDBConn;
                        oraCmd.Transaction = (OracleTransaction)tran;

                        StringBuilder sqlQuery = new StringBuilder();
                        sqlQuery.AppendFormat(" update operations o " +
                            "set o.ope_ps_id = {0} " +
                            "where ope_id = {1}", _iSpaceId, iOperationID);

                        oraCmd.CommandText = sqlQuery.ToString();

                        oraCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    logger.AddLog("[Msg02:UpdateSpaceInfo]: Excepcion: " + e.Message, LoggerSeverities.Error);
                    bOK = false;
                }
                finally
                {
                    if (oraCmd != null)
                    {
                        oraCmd.Dispose();
                        oraCmd = null;
                    }
                }
            }

            return bOK;
        }

        void CommitTrans(IDbTransaction tran)
        {

            IDbConnection con = null;
            try
            {
                if (tran != null)
                {
                    con = tran.Connection;
                    tran.Commit();
                }
                //	if (tra
            }
            catch
            {

            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                }
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                }

            }
        }

        void RollbackTrans(IDbTransaction tran)
        {

            IDbConnection con = null;
            try
            {
                if (tran != null)
                {
                    con = tran.Connection;
                    tran.Rollback();
                }
                //	if (tra
            }
            catch
            {

            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                }
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                }

            }
        }

        /// <summary>
        /// Metodo para sustituir el proceso de pedir la información del Process de M05 vía servicio asmx (mirar Process de M05)
        /// Devuelve una SortedList con toda la información
        /// NOTA: muy similar al OPSMEssage_M05Process del controller de User
        /// </summary>
        /// <param name="fine_id"></param>
        /// <param name="nContractId"></param>
        /// <returns>SortedList con la información</returns>
        private SortedList CalculateInfoAboutPlateFines_M05Process(DateTime _date, string _vehicleId, int nContractId = 0)
        {
            SortedList parametersOut = new SortedList();

            string fine_id = "0";

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
            OracleDataReader dr = null;

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

                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.AppendFormat("select fin_id from fines t " +
                                        "WHERE TO_char(FIN_DATE,'DDMMYY')='{0:ddMMyy}' " +
                                        "AND fin_vehicleid='{1}' " +
                                        "AND t.fin_statusadmon=0 and t.fin_status=30 " + //pendiente de pago y generada
                                        "ORDER BY FIN_DATE DESC", _date, _vehicleId);

                oraCmd.CommandText = sqlQuery.ToString();

                dr = oraCmd.ExecuteReader();

                if (dr.Read())
                {
                    fine_id = dr["fin_id"].ToString();
                    responseFineNumber = fine_id;
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

            return (nResult > 0);
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

        public TokenValidationResult DefaultVerification(string encodedTokenFromWebPage)
        {
            var jot = new JotProvider();

            return jot.Validate(encodedTokenFromWebPage);
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

                oraCmd.Parameters.Add(new OracleParameter("nReturnValue", OracleDbType.Int32));
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

        /*private bool OPSMessage(string strMessageIn, int iVirtualUnit, out string strMessageOut, int nContractId = 0)
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

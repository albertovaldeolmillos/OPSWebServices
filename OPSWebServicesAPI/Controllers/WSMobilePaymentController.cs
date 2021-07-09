using AutoMapper;
using OPS.Comm;
using OPS.Comm.Becs.Messages;
using OPS.Components.Data;
using OPSWebServicesAPI.Helpers;
using OPSWebServicesAPI.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
        [HttpPost]
        [Route("QueryContractsAPI")]
        public Result QueryContractsAPI()
        {
            //string xmlOut = "";
            Result response = new Result();
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
                    response.IsSuccess = false;
                    response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                    response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
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
                response.IsSuccess = false;
                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
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

            response.IsSuccess = true;
            response.Error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);
            response.Value = contInfo;
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
        public Result QueryZoneAPI([FromBody] ZoneQuery zoneQuery)
        {
            //string xmlOut = "";

            Result response = new Result();
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
                            Logger_AddLogMessage(string.Format("QueryZoneAPI::Error: parametersIn= {0}, parametersOut={1}", SortedListToString(parametersIn), "Result_Error_InvalidAuthenticationHash"), LoggerSeverities.Error);
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

            response.IsSuccess = true;
            response.Error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

            ZoneInfo zoneInfo = new ZoneInfo();
            ConfigMapModel configMapModel = new ConfigMapModel();
            var config = configMapModel.configZone();
            IMapper iMapper = config.CreateMapper();
            zoneInfo = iMapper.Map<SortedList, ZoneInfo>((SortedList)parametersOut);
            response.Value = zoneInfo;
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
        public Result QueryStreetsAPI([FromBody] StreetsQuery streetsQuery)
        {
            //string xmlOut = "";

            Result response = new Result();
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
                    if (parametersIn["contid"] == null)
                    {
                        //xmlOut = GenerateXMLErrorResult(ResultType.Result_Error_Missing_Input_Parameter);
                        Logger_AddLogMessage(string.Format("QueryStreetsAPI::Error - Missing parameter: parametersIn= {0}", SortedListToString(parametersIn)), LoggerSeverities.Error);
                        response.IsSuccess = false;
                        response.Error = new Error((int)ResultType.Result_Error_Missing_Input_Parameter, (int)SeverityError.Critical);
                        response.Value = Convert.ToInt32(ResultType.Result_Error_Missing_Input_Parameter).ToString();
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
                            response.IsSuccess = false;
                            response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Critical);
                            response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
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
                response.IsSuccess = false;
                response.Error = new Error((int)ResultType.Result_Error_Generic, (int)SeverityError.Exception);
                response.Value = Convert.ToInt32(ResultType.Result_Error_Generic).ToString();
                return response;
            }

            response.IsSuccess = true;
            response.Error = new Error((int)ResultType.Result_OK, (int)SeverityError.Low);

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
            response.Value = streetsInfo;
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

using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Commands;
using PDMDatabase.MemoryDatabase;
using PDMDatabase.Models;
using PDMDatabase.Repositories;
using PDMHelpers;
using System;
using System.Collections.Generic;

namespace PDMCompute
{
    public class OPSPDMDatabase
    {
        public const int C_NUMERO_MAX_CAMBIO_DIA = 70;
        public const int C_NUMERO_MAX_CAMBIOS_BLOQUE = 10;
        public const int C_NUMERO_MAX_ITERACIONES = (C_NUMERO_MAX_CAMBIO_DIA * C_NUMERO_MAX_CAMBIOS_BLOQUE);  //  cambios de bloque por maximo cambio de dia

        private OracleConnection _connection;
        private readonly ITraceable _trace;
        private readonly MemoryDatabase imd;

        private long m_lTariffConstraintsVersion = GlobalDefs.DEF_UNDEFINED_VALUE;
        private long m_lTariffIntervalsVersion = GlobalDefs.DEF_UNDEFINED_VALUE;

        public OPSPDMDatabase(ILoggerManager loggerManager)
        {
            _trace = loggerManager.CreateTracer(this.GetType());
            imd = new MemoryDatabase(loggerManager);
        }

        public void SetTracerEnabled(bool enabled)
        {
            if (_trace != null)
            {
                _trace.Enabled = enabled;
            }
        }

        public void SetTracerIMDEnabled(bool enabled)
        {

            if (imd != null)
            {
                imd.SetTracerEnabled(enabled);
            }
        }

        public bool Open(string connectionString)
        {
            bool bReturn = true;
            try
            {
                _connection = new OracleConnection(connectionString);
                _connection.Open();

                if (!IsOpened())
                {
                    bReturn = false;
                    _trace.Write(TraceLevel.Error, "Error openning OPS Database");
                }
                else
                {
                    _trace.Write(TraceLevel.Info, "OPS Database opened");
                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                bReturn = false;
            }

            return bReturn;
        }

        public bool IsOpened()
        {
            return _connection.State == System.Data.ConnectionState.Open;
        }

        public  bool Close()
        {
            bool bReturn = true;
            try
            {
                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _trace.Write(TraceLevel.Info, "OPS Database connection closed");
                }
            }
            catch (Exception error)
            {
                bReturn = false;
                _trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return bReturn;
        }

        public void Init()
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::Init");
            imd.LoadData(_connection);

            if (GeneralParams.UnitId.Equals(GlobalDefs.CC_UNIT_ID.ToString()))
            {
                _trace.Write(TraceLevel.Info, "OPSPDMDatabase GeneralParams.UnitId.Equals(GlobalDefs.CC_UNIT_ID)");

                GetTariffsConstraintsMaxVersion(ref m_lTariffConstraintsVersion);
                GetTariffsIntervalsMaxVersion(ref m_lTariffIntervalsVersion);
            }
            else
            {
                _trace.Write(TraceLevel.Info, "OPSPDMDatabase NOT GeneralParams.UnitId.Equals(GlobalDefs.CC_UNIT_ID)");
            }
        }

        /// <exception cref="InvalidOperationException">If IMD isn't ready throw an exception</exception>
        private void CheckImdIsReadyOrThrow()
        {
            if (imd == null)
            {
                throw new InvalidOperationException("Invalid MemDB handler");
            }
        }

        public bool TariffsHaveChanged()
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::TariffsHaveChanged");
            bool fnResult = false;

            try
            {
                if (GeneralParams.UnitId.Equals(GlobalDefs.CC_UNIT_ID.ToString()))
                {
                    _trace.Write(TraceLevel.Info, "GeneralParams.UnitId.Equals(GlobalDefs.CC_UNIT_ID)");

                    long lTariffConstraintsMaxVersion = GlobalDefs.DEF_UNDEFINED_VALUE;
                    long lTariffIntervalsMaxVersion = GlobalDefs.DEF_UNDEFINED_VALUE;
                    
                    GetTariffsConstraintsMaxVersion(ref lTariffConstraintsMaxVersion);
                    GetTariffsIntervalsMaxVersion(ref lTariffIntervalsMaxVersion);

                    if ((m_lTariffConstraintsVersion != lTariffConstraintsMaxVersion) ||
                        (m_lTariffIntervalsVersion != lTariffIntervalsMaxVersion))
                    {

                        if (!imd.ReloadTariffs())
                            throw new InvalidOperationException("ERROR : Reloading Tariffs");

                        m_lTariffConstraintsVersion = lTariffConstraintsMaxVersion;
                        m_lTariffIntervalsVersion = lTariffIntervalsMaxVersion;

                        fnResult = true;
                    }

                }
                else {
                    _trace.Write(TraceLevel.Info, "NOT GeneralParams.UnitId.Equals(GlobalDefs.CC_UNIT_ID)");
                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        private bool GetTariffsConstraintsMaxVersion(ref long version)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::GetTariffsConstraintsMaxVersion");
            bool fnResult = true;
            
            try
            {
                TariffsConstraintsMaxVersionCommand command = new TariffsConstraintsMaxVersionCommand(this._connection, this._trace);
                _trace.Write(TraceLevel.Info, $"Executing Query TariffsConstraintsMaxVersion");

                version = command.Execute();

                _trace.Write(TraceLevel.Debug, $"TariffsConstraintsMaxVersion = {version}");
            }
            catch (Exception error)
            {
                version = -1;
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        private bool GetTariffsIntervalsMaxVersion(ref long version)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::GetTariffsIntervalsMaxVersion");
            bool fnResult = true;

            try
            {
                TariffsIntervalsMaxVersionCommnad command = new TariffsIntervalsMaxVersionCommnad(this._connection, this._trace);
                _trace.Write(TraceLevel.Info, $"Executing Query TariffsIntervalsMaxVersion");

                version = command.Execute();
                _trace.Write(TraceLevel.Debug, $"TariffsIntervalsMaxVersion = {version}");
            }
            catch (Exception error)
            {
                version = -1;
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public bool GetGroupFromUnit(long lUnit, ref long lGroup, ref long lTypeOfGroup)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::GetGroupFromUnit");
            bool fnResult = true;

            try
            {
                CheckImdIsReadyOrThrow();

                _trace.Write(TraceLevel.Info, $"QUERY: Which is the Group of the unit({lUnit}) ?");

                IMT_Groups pGroups = imd.GetTable(MemoryDatabaseTables.Groups) as IMT_Groups;
                if (pGroups == null)
                    throw new InvalidOperationException("Pointer of Group is NULL");

                IMT_GroupsChilds pGroupsChilds = imd.GetTable(MemoryDatabaseTables.GroupsChilds) as IMT_GroupsChilds;
                if (pGroupsChilds == null)
                    throw new InvalidOperationException("Pointer of Group Child is NULL");

                bool bFind = pGroupsChilds.GetGroupFromUnit(lUnit, ref lGroup, ref lTypeOfGroup, pGroups);
                if (!bFind)
                    throw new InvalidOperationException("FAILED Call to pGroupsChilds->GetGroupFromUnit");

                if (bFind) {
                    _trace.Write(TraceLevel.Info, $"The Unit({lUnit}) BELONGS To Group({lGroup}) and its Type of group is ({lTypeOfGroup})");
                }

            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public bool IsVehicleIdResident(CM1GroupsTree pTree, ref COPSPlate poPlate, long pGroup, ref long poArticleDef, ref bool poIsResident)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::IsVehicleIdResident");
            bool fnResult = true;

            long lArticleDef = GlobalDefs.DEF_UNDEFINED_VALUE;
            long lSearchCurrentArticleDef = GlobalDefs.DEF_UNDEFINED_VALUE;
            long lCurrentArticleDef = GlobalDefs.DEF_UNDEFINED_VALUE;

            try
            {
                Guard.IsNull(pTree, nameof(pTree));
                Guard.IsNull(poPlate, nameof(poPlate));
                if (poPlate.IsEmpty())
                {
                    throw new ArgumentOutOfRangeException(nameof(poPlate), "Plate is empty");
                }

                poIsResident = false;

                if ((poArticleDef) != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    lSearchCurrentArticleDef = poArticleDef;
                }

                ResidentsRepository repository = new ResidentsRepository(_connection) {
                    Trace = this._trace
                };
                IEnumerable<Residents> residentsResponse = repository.GetByVehicleId(poPlate.ToString());

                foreach (Residents residentRecord in residentsResponse)
                {
                    lArticleDef = residentRecord.RES_DART_ID;

                    if (pTree.IsGroupInTree(residentRecord.RES_GRP_ID, pGroup))
                    {
                        lCurrentArticleDef = lArticleDef;
                        if (lSearchCurrentArticleDef == GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            poArticleDef = lCurrentArticleDef;
                            poIsResident = true;
                        }
                        else
                        {
                            if (lSearchCurrentArticleDef == lCurrentArticleDef)
                            {
                                poArticleDef = lCurrentArticleDef;
                                poIsResident = true;
                            }
                        }
                    }

                    if (poIsResident) {
                        break;
                    }
                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public bool GetVehicleLastOperationInfo(COPSPlate plate, COPSDate date, ref long oAmpArticleDef, ref long oAmpGroup)
        {
            _trace.Write(TraceLevel.Debug, "GetVehicleLastOperationGroup");
            bool fnResult = true;

            try
            {
                OperationsRepository repository = new OperationsRepository(_connection);
                repository.Trace = this._trace;
                Operations lastOperation = repository.GetLastOperationInfo(plate.ToString(), date.CopyToChar());

                oAmpArticleDef = lastOperation.OPE_DART_ID ?? GlobalDefs.DEF_UNDEFINED_VALUE;
                oAmpGroup = lastOperation.OPE_GRP_ID ?? GlobalDefs.DEF_UNDEFINED_VALUE;
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        public bool IsVehicleIdVIP(CM1GroupsTree pTree, ref COPSPlate poPlate, COPSDate pDate, long pGroup, ref long poArticleDef, ref bool poIsVIP)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::IsVehicleIdVIP");
            bool fnResult = true;

            long lSearchCurrentArticleDef = GlobalDefs.DEF_UNDEFINED_VALUE;
            long lCurrentArticleDef = GlobalDefs.DEF_UNDEFINED_VALUE;
            COPSDate opsDate = new COPSDate();
            COPSDate dtInitDateTemp = new COPSDate();
            COPSDate dtEndDateTemp = new COPSDate();

            try
            {
                Guard.IsNull(pTree, nameof(pTree));
                Guard.IsNull(poPlate, nameof(poPlate));
                if (poPlate.IsEmpty()) {
                    throw new ArgumentOutOfRangeException(nameof(poPlate), "Plate is empty");
                }

                poIsVIP = false;
                if ((poArticleDef) != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    lSearchCurrentArticleDef = poArticleDef;
                }

                VipsRepository repository = new VipsRepository(_connection);
                IEnumerable<Vips> vipsResponse = repository.GetByVehicleId(poPlate.ToString());

                foreach (Vips vipRecord in vipsResponse)
                {
                    lCurrentArticleDef = vipRecord.VIP_DART_ID;

                    poIsVIP = false;

                    if (lSearchCurrentArticleDef ==  GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        poIsVIP = true;
                    }
                    else
                    {
                        if (lSearchCurrentArticleDef == lCurrentArticleDef)
                        {
                            poIsVIP = true;
                        }
                    }

                    if (poIsVIP)
                    {
                        if (vipRecord.VIP_INIDATE.Length == 0 && vipRecord.VIP_ENDDATE.Length == 0 && vipRecord.VIP_DAYOFWEEK.Length == 0 && vipRecord.VIP_GRP_ID == GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            poIsVIP = true;
                        }
                        else
                        {

                            poIsVIP = true;

                            if (vipRecord.VIP_INIDATE.Length == GlobalDefs.DEF_DATE_LEN && poIsVIP)
                            {
                                opsDate.Set(vipRecord.VIP_INIDATE);
                                if (pDate >= opsDate)
                                {
                                    poIsVIP = true;
                                }
                                else
                                    poIsVIP = false;

                            }
                            if (vipRecord.VIP_ENDDATE.Length == GlobalDefs.DEF_DATE_LEN && poIsVIP)
                            {
                                opsDate.Set(vipRecord.VIP_ENDDATE);
                                poIsVIP = pDate <= opsDate;
                            }


                            if (vipRecord.VIP_DAYOFWEEK.Length == GlobalDefs.DDAY_CODE_LENGTH && poIsVIP)
                            {
                                int istrPos = pDate.GetDayOfWeek() - 2;
                                if (istrPos < 0)
                                {
                                    istrPos = 7 + istrPos;
                                }

                                poIsVIP = vipRecord.VIP_DAYOFWEEK[istrPos] == '1';
                            }

                            if (vipRecord.VIP_GRP_ID != GlobalDefs.DEF_UNDEFINED_VALUE && poIsVIP)
                            {
                                poIsVIP = pTree.IsGroupInTree(vipRecord.VIP_GRP_ID, pGroup);
                            }
                        }

                        if (vipRecord.VIP_INIHOUR != GlobalDefs.DEF_UNDEFINED_VALUE &&
                            vipRecord.VIP_INIMINUTE != GlobalDefs.DEF_UNDEFINED_VALUE && 
                            vipRecord.VIP_ENDHOUR != GlobalDefs.DEF_UNDEFINED_VALUE &&
                            vipRecord.VIP_ENDMINUTE != GlobalDefs.DEF_UNDEFINED_VALUE && 
                            poIsVIP)
                        {

                            dtInitDateTemp = new COPSDate(pDate.Value.Year, pDate.Value.Month, pDate.Value.Day, (int)vipRecord.VIP_INIHOUR, (int)vipRecord.VIP_INIMINUTE, 0);
                            dtEndDateTemp = new COPSDate(pDate.Value.Year, pDate.Value.Month, pDate.Value.Day, (int)vipRecord.VIP_ENDHOUR, (int)vipRecord.VIP_ENDMINUTE, 0);

                            poIsVIP = ((dtInitDateTemp <= pDate) && (pDate <= dtEndDateTemp));
                        }
                    }

                    if (poIsVIP)
                    {
                        poArticleDef = lCurrentArticleDef;
                        break;
                    }

                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        public bool GetVIPDates(CM1GroupsTree pTree, long lArticleId, COPSPlate poPlate, ref COPSDate pdtStartDate, ref COPSDate pdtEndDate)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::IsVehicleIdVIP");
            bool fnResult = true;

            long lSearchCurrentArticleDef = GlobalDefs.DEF_UNDEFINED_VALUE;
            long lCurrentArticleDef = GlobalDefs.DEF_UNDEFINED_VALUE;

            try
            {
                Guard.IsNull(pTree, nameof(pTree));
                Guard.IsNull(poPlate, nameof(poPlate));
                if (poPlate.IsEmpty())
                {
                    throw new ArgumentOutOfRangeException(nameof(poPlate), "Plate is empty");
                }

                lSearchCurrentArticleDef = lArticleId;

                VipsRepository repository = new VipsRepository(_connection);
                IEnumerable<Vips> vipsResponse = repository.GetByVehicleId(poPlate.ToString());

                foreach (Vips vipRecord in vipsResponse)
                {
                    lCurrentArticleDef = vipRecord.VIP_DART_ID;

                    if (lSearchCurrentArticleDef == lCurrentArticleDef)
                    {
                        COPSDate dtInitDateTemp = new COPSDate(vipRecord.VIP_INIDATE);
                        COPSDate dtEndDateTemp = new COPSDate(vipRecord.VIP_ENDDATE);

                        pdtStartDate.SetDateTime(dtInitDateTemp.Value.Year, dtInitDateTemp.Value.Month, dtInitDateTemp.Value.Day, dtInitDateTemp.Value.Hour, dtInitDateTemp.Value.Minute, 0);
                        pdtEndDate.SetDateTime(dtEndDateTemp.Value.Year, dtEndDateTemp.Value.Month, dtEndDateTemp.Value.Day, dtEndDateTemp.Value.Hour, dtEndDateTemp.Value.Minute, 0);
                    }
                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public bool SetTariffDates(CM1GroupsTree pTree, long lArticleId, COPSPlate pstrVehicleID)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::IsVehicleIdVIP");
            bool fnResult = false;

            COPSDate dtStartDate = new COPSDate(DateTime.Now);
            COPSDate dtEndDate = new COPSDate(DateTime.Now);
 
            try
            {
                // First get start and end dates
                if (GetVIPDates(pTree, lArticleId, pstrVehicleID, ref dtStartDate, ref dtEndDate))
                {
                    // Next determine the tariff ID
                    long lTarId = 0;

                    switch (lArticleId)
                    {
                        case 102:
                            lTarId = 60002;
                            break;
                        case 103:
                            lTarId = 60003;
                            break;
                        case 104:
                            lTarId = 60004;
                            break;
                        case 105:
                            lTarId = 60005;
                            break;
                        case 106:
                            lTarId = 60006;
                            break;
                        case 107:
                            lTarId = 60007;
                            break;
                        case 108:
                            lTarId = 60008;
                            break;
                        case 109:
                            lTarId = 60009;
                            break;
                        case 110:
                            lTarId = 60010;
                            break;
                        case 111:
                            lTarId = 60011;
                            break;
                        case 112:
                            lTarId = 60012;
                            break;
                        case 113:
                            lTarId = 60013;
                            break;
                        case 1031:
                            lTarId = 61031;
                            break;
                        case 1032:
                            lTarId = 61032;
                            break;
                        case 1033:
                            lTarId = 61033;
                            break;
                        case 1034:
                            lTarId = 61034;
                            break;
                        case 1041:
                            lTarId = 61041;
                            break;
                        case 1042:
                            lTarId = 61042;
                            break;
                        case 1043:
                            lTarId = 61043;
                            break;
                        case 1044:
                            lTarId = 61044;
                            break;
                        case 1061:
                            lTarId = 61061;
                            break;
                        case 1062:
                            lTarId = 61062;
                            break;
                        case 1063:
                            lTarId = 61063;
                            break;
                        case 1064:
                            lTarId = 61064;
                            break;
                        case 1071:
                            lTarId = 61071;
                            break;
                        case 1072:
                            lTarId = 61072;
                            break;
                        case 1073:
                            lTarId = 61073;
                            break;
                        case 1074:
                            lTarId = 61074;
                            break;
                        case 1091:
                            lTarId = 61091;
                            break;
                        case 1092:
                            lTarId = 61092;
                            break;
                        case 1093:
                            lTarId = 61093;
                            break;
                        case 1094:
                            lTarId = 61094;
                            break;
                        case 1101:
                            lTarId = 61101;
                            break;
                        case 1102:
                            lTarId = 61102;
                            break;
                        case 1103:
                            lTarId = 61103;
                            break;
                        case 1104:
                            lTarId = 61104;
                            break;
                        case 1121:
                            lTarId = 61121;
                            break;
                        case 1122:
                            lTarId = 61122;
                            break;
                        case 1123:
                            lTarId = 61123;
                            break;
                        case 1124:
                            lTarId = 61124;
                            break;
                        case 1131:
                            lTarId = 61131;
                            break;
                        case 1132:
                            lTarId = 61132;
                            break;
                        case 1133:
                            lTarId = 61133;
                            break;
                        case 1134:
                            lTarId = 61134;
                            break;
                    }

                    if (lTarId > 0)
                    {
                        if (ChangeTariffDates(lTarId, dtStartDate, dtEndDate))
                            fnResult = true;
                    }
                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public bool ChangeTariffDates(long lTariffId, COPSDate dtStartDate, COPSDate dtEndDate)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::ChangeTariffDates");

            bool fnResult = true;

            _trace.Write(TraceLevel.Debug, string.Format("Changing the start date {0} and end date {1} for article {2}",
                dtStartDate.ToString(), dtEndDate.ToString(), lTariffId));

            try
            {
                Guard.IsUndefined(lTariffId, nameof(lTariffId), "Invalid tariff id.");

                CheckImdIsReadyOrThrow();

                IMT_Tariffs tariffs = imd.GetTable<IMT_Tariffs>(MemoryDatabaseTables.Tariffs);

                switch (lTariffId)
                {
                    case 60002:
                    case 60005:
                    case 60008:
                    case 60011:

                       if (!tariffs.ChangeDates(lTariffId, 1, 1, dtStartDate, dtEndDate))
                            throw new InvalidOperationException(string.Format("Error changing dates for tariff {0} number 1", lTariffId));

                        if (!tariffs.ChangeDates(lTariffId, 2, 2, dtStartDate, dtEndDate))
                            throw new InvalidOperationException(string.Format("Error changing dates for tariff {0} number 2", lTariffId));

                        if (!tariffs.ChangeDates(lTariffId, 3, 3, dtStartDate, dtEndDate))
                            throw new InvalidOperationException(string.Format("Error changing dates for tariff {0} number 3", lTariffId));

                        break;

                    case 60003:
                    case 60004:
                    case 60006:
                    case 60007:
                    case 60009:
                    case 60010:
                    case 60012:
                    case 60013:
                    case 61031:
                    case 61032:
                    case 61033:
                    case 61034:
                    case 61041:
                    case 61042:
                    case 61043:
                    case 61044:
                    case 61061:
                    case 61062:
                    case 61063:
                    case 61064:
                    case 61071:
                    case 61072:
                    case 61073:
                    case 61074:
                    case 61091:
                    case 61092:
                    case 61093:
                    case 61094:
                    case 61101:
                    case 61102:
                    case 61103:
                    case 61104:
                    case 61121:
                    case 61122:
                    case 61123:
                    case 61124:
                    case 61131:
                    case 61132:
                    case 61133:
                    case 61134:
                        if (!tariffs.ChangeDates(lTariffId, 1, 1, dtStartDate, dtEndDate))
                            throw new InvalidOperationException(string.Format("Error changing dates for tariff {0} number 1", lTariffId));

                        if (!tariffs.ChangeDates(lTariffId, 2, 2, dtStartDate, dtEndDate))
                            throw new InvalidOperationException(string.Format("Error changing dates for tariff {0} number 2", lTariffId));

                        if (!tariffs.ChangeDates(lTariffId, 3, 2, dtStartDate, dtEndDate))
                            throw new InvalidOperationException(string.Format("Error changing dates for tariff {0} number 3", lTariffId));

                        if (!tariffs.ChangeDates(lTariffId, 4, 2, dtStartDate, dtEndDate))
                            throw new InvalidOperationException(string.Format("Error changing dates for tariff {0} number 4", lTariffId));

                        if (!tariffs.ChangeDates(lTariffId, 5, 2, dtStartDate, dtEndDate))
                            throw new InvalidOperationException(string.Format("Error changing dates for tariff {0} number 5", lTariffId));

                        if (!tariffs.ChangeDates(lTariffId, 6, 3, dtStartDate, dtEndDate))
                            throw new InvalidOperationException(string.Format("Error changing dates for tariff {0} number 6", lTariffId));

                        break;
                    default:
                        throw new InvalidOperationException("Invalid tariff to change");
                        break;
                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public bool ExistsGroup(long lGroup)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::ExistsGroup");
            bool fnResult = true;

            try
            {
                CheckImdIsReadyOrThrow();

                _trace.Write(TraceLevel.Info, $"QUERY: Exist the Group ({lGroup}) ?");

                IMT_Groups pGroups = imd.GetTable(MemoryDatabaseTables.Groups) as IMT_Groups;
                if (pGroups == null)
                    throw new InvalidOperationException("Pointer of Group is NULL");

                fnResult = pGroups.GetGroupType(lGroup) != default(long);
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        //TODO: Compare this method and the C++ version 
        public bool GetInfoArticle(long lArticle, ref COPSDate dtOper, ref COPSPlate strVehicleIdDB, ref long lUser, ref long lArticleDefDB, ref COPSDate dtArtIni, ref COPSDate dtArtEnd, ref bool bFind)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::GetInfoArticle");
            bool fnResult = true;

            try
            {
                if (imd != null)
                {
                    IMT_Articles articlesTable = imd.GetTable<IMT_Articles>(MemoryDatabaseTables.Articles);
                    bool hasBeenLoaded = articlesTable.LoadArticle(lArticle, dtOper.fstrGetTraceString());
                    if (hasBeenLoaded) {
                        articlesTable.FindArticle(lArticle, ref bFind, ref dtArtIni, ref dtArtEnd, ref lArticleDefDB, ref strVehicleIdDB, ref lUser);
                    }
                }
                else {
                    IMT_Articles articlesInMemoryTable = imd.GetTable(MemoryDatabaseTables.Articles) as IMT_Articles;

                    if (articlesInMemoryTable == null)
                        throw new InvalidOperationException("Pointer Articles table is NULL");

                    if (!articlesInMemoryTable.FindArticle(lArticle,ref bFind,ref dtArtIni,ref dtArtEnd,ref lArticleDefDB,ref strVehicleIdDB,ref lUser))
                    {
                        throw new InvalidOperationException("Error ocurred while searching article in memory DB");
                    }
                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public IEnumerable<Groups> GetAllGroups()
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::GetAllGroups");
            IEnumerable<Groups> grupos = null;
            try
            {
                CheckImdIsReadyOrThrow();

                IMT_Groups pGroups = imd.GetTable(MemoryDatabaseTables.Groups) as IMT_Groups;
                if (pGroups == null)
                    throw new InvalidOperationException("Pointer of Group is NULL");

                grupos = pGroups.GetAllGroups();
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return grupos;
        }
        
        public IEnumerable<GroupsChilds> GetAllGroupsChilds()
        {
            _trace.Write(TraceLevel.Debug, "::GetAllGroupsChilds");
            IEnumerable<GroupsChilds> grupos = null;
            try
            {
                CheckImdIsReadyOrThrow();

                IMT_GroupsChilds pGroupsChilds = imd.GetTable<IMT_GroupsChilds>(MemoryDatabaseTables.GroupsChilds);

                grupos = pGroupsChilds.GetAllGroupsChilds();
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return grupos;
        }

        public bool GetConstAndTar(long articleDef, long groupId, long groupType, COPSDate pdtWork,ref long constraint,ref long tariff)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::GetConstAndTar");
            bool fnResult = true;

           

            try
            {
                CheckImdIsReadyOrThrow();

                if (articleDef == GlobalDefs.DEF_UNDEFINED_VALUE || 
                    (groupId == GlobalDefs.DEF_UNDEFINED_VALUE && groupType == GlobalDefs.DEF_UNDEFINED_VALUE)
                ) {
                    _trace.Write(TraceLevel.Error, $"Undefined Value Type Art ({articleDef}) Group ({groupId}) Type Group ({groupType})");
                    throw new ArgumentException("Invalid input parameters");
                }

                // Get the timetables that include the operation date
                List<long> timeBlocks = GetBlocksFromTime(pdtWork);
                if(timeBlocks == null)
                    throw new InvalidOperationException("Could not obtain blocks");

                List<long> typeOfDays = GetDayTypes(pdtWork);
                if(typeOfDays == null )
                    throw new InvalidOperationException("Error in call to COPSPDMDB::GetDayTypes");

                IMT_ArticlesRules articleRules = imd.GetTable<IMT_ArticlesRules>(MemoryDatabaseTables.ArticlesRules);
                ArticlesRules foundArticleRule = default(ArticlesRules);

                //BUSQUEDA POR TYPE OF DAY AND TIMETABLE BLOCKS
                _trace?.Write(TraceLevel.Debug, $"TRY TO FIND CONSTRAINT AND TARIFF BY TYPE_OF_DAY AND TIMETABLE_BLOCK");
                foreach (long typeOfDay in typeOfDays)
                {
                    foreach (long timeBlock in timeBlocks)
                    {
                        // TODO: Add correct return to GetConstAndTar function 
                        bool isOK = articleRules.GetConstAndTar(pdtWork, groupId, groupType, articleDef, typeOfDay, timeBlock, out foundArticleRule);
                        if (!isOK)
                        {
                            _trace.Write(TraceLevel.Info, $"Cannot get constraints and tariff from:");
                            _trace.Write(TraceLevel.Info, $"Type of art. ({articleDef}) Group ({groupId}) Type of group ({groupType}) Type of day ({typeOfDay}) Timetable id ({timeBlock})");

                            throw new InvalidOperationException("ERROR getting constraints and tariffs");
                        }

                        if (foundArticleRule != default(ArticlesRules))
                        {
                            _trace.Write(TraceLevel.Info, $"Article Rule found for type of day {typeOfDay} and Timetible id {timeBlock}");
                            break;
                        }
                        else
                        {
                            _trace.Write(TraceLevel.Info, $"No Article Rule found for type of day {typeOfDay} and Timetible id {timeBlock}");
                        }
                    }

                    if (foundArticleRule != default(ArticlesRules))
                        break;
                }

                //BUSQUEDA POR TYPE OF DAY 
                if (foundArticleRule == default(ArticlesRules))
                {
                    _trace?.Write(TraceLevel.Debug, $"BUSQUEDA DE CONST AND TAR POR TYPE OF DAY");
                    foreach (long typeOfDay in typeOfDays)
                    {
                        // TODO: Add correct return to GetConstAndTar function 
                        bool isOK = articleRules.GetConstAndTar(pdtWork, groupId, groupType, articleDef, typeOfDay, GlobalDefs.DEF_UNDEFINED_VALUE, out foundArticleRule);
                        if (!isOK)
                        {
                            _trace.Write(TraceLevel.Info, $"Cannot get constraints and tariff from:");
                            _trace.Write(TraceLevel.Info, $"Type of art. ({articleDef}) Group ({groupId}) Type of group ({groupType}) Type of day ({typeOfDay}))");

                            throw new InvalidOperationException("ERROR getting constraints and tariffs");
                        }

                        if (foundArticleRule != default(ArticlesRules))
                        {
                            _trace.Write(TraceLevel.Info, $"Article Rule found for type of day {typeOfDay}");
                            break;
                        }
                        else
                        {
                            _trace.Write(TraceLevel.Info, $"No Article Rule found for type of day {typeOfDay}");
                        }
                    }
                }

                //BUSQUEDA POR TIMEBLOCKS 
                if (foundArticleRule == default(ArticlesRules)) {
                    _trace?.Write(TraceLevel.Debug, $"BUSQUEDA DE CONST AND TAR POR TIMETABLE BLOCKS");

                    foreach (long timeBlock in timeBlocks)
                    {
                        // TODO: Add correct return to GetConstAndTar function 
                        bool isOK = articleRules.GetConstAndTar(pdtWork, groupId, groupType, articleDef, GlobalDefs.DEF_UNDEFINED_VALUE, timeBlock, out foundArticleRule);
                        if (!isOK)
                        {
                            _trace.Write(TraceLevel.Info, $"Cannot get constraints and tariff from:");
                            _trace.Write(TraceLevel.Info, $"Type of art. ({articleDef}) Group ({groupId}) Type of group ({groupType})  Timetable id ({timeBlock})");

                            throw new InvalidOperationException("ERROR getting constraints and tariffs");
                        }

                        if (foundArticleRule != default(ArticlesRules))
                        {
                            _trace.Write(TraceLevel.Info, $"Article Rule found for Timetible id {timeBlock}");
                            break;
                        }
                        else
                        {
                            _trace.Write(TraceLevel.Info, $"No Article Rule found for Timetible id {timeBlock}");
                        }
                    }
                }

                //BUSQUEDA SIN TYPE OF DAY NI TIMETABLE BLOCKS
                if (foundArticleRule == default(ArticlesRules))
                {
                    _trace?.Write(TraceLevel.Debug, $"BUSQUEDA DE CONST AND TAR SIN TYPE OF DAY AND TIMETABLE BLOCKS");

                    // TODO: Add correct return to GetConstAndTar function 
                    bool isOK = articleRules.GetConstAndTar(pdtWork, groupId, groupType, articleDef, GlobalDefs.DEF_UNDEFINED_VALUE, GlobalDefs.DEF_UNDEFINED_VALUE, out foundArticleRule);
                    if (!isOK)
                    {
                        _trace.Write(TraceLevel.Info, $"Cannot get constraints and tariff from:");
                        _trace.Write(TraceLevel.Info, $"Type of art. ({articleDef}) Group ({groupId}) Type of group ({groupType}) )");

                        throw new InvalidOperationException("ERROR getting constraints and tariffs");
                    }
                }

                //ASSING RESULT IS THERE IS RESULT
                if (foundArticleRule != default(ArticlesRules))
                {
                    constraint = foundArticleRule.RUL_CON_ID.Value;
                    tariff  = foundArticleRule.RUL_TAR_ID.Value;
                }

            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public long HourDifference(long result)
        {
            long lRes = result;
            if (GeneralParams.UnitId == GlobalDefs.CC_UNIT_ID.ToString())
            {
                if (GetHourDifference() > 0)
                {
                    lRes = -1;
                }
            }

            return lRes;
        }

        private long GetHourDifference()
        {
            _trace.Write(TraceLevel.Debug, "GetHourDifference");
            long fnResult = 0;

            try
            {
                ParametersRepository repository = new ParametersRepository(_connection);
                repository.Trace = this._trace;
                fnResult = repository.GetHourDifference();
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return fnResult;

        }

        public bool GetVehicleOperations(string vehicleId, long articleId, ref IEnumerable<Operations> operationsOut, bool mustSort = false)
        {
            _trace?.Write(TraceLevel.Debug, "OPSPDMDatabase::GetVehicleOperations");
            bool fnResult = true;
            IEnumerable<Operations> operaciones = null;
            string methodUsed = string.Empty;
            try
            {
                if (GeneralParams.UnitId != GlobalDefs.CC_UNIT_ID.ToString())
                {
                    CheckImdIsReadyOrThrow();
                    //0871CTX

                    methodUsed = "pOperations->GetVehicleOperations";

                    IMT_Operations operationsTable = imd.GetTable<IMT_Operations>(MemoryDatabaseTables.Operations);
                    operationsOut = operationsTable.GetVehicleOperations(vehicleId, articleId, mustSort);
                }
                else {
                    methodUsed = "GetVehicleOperationsDB";

                    OperationsRepository  operationsRepository = new OperationsRepository(_connection);
                    operationsRepository.Trace = this._trace;
                    operationsOut = operationsRepository.GetByVehicleIdAndArticleDef(vehicleId, articleId, mustSort);
                }

                if (operationsOut == null)
                {
                    throw new InvalidOperationException($"FAILED call to {methodUsed}");
                }
            }
            catch (Exception error)
            {
                _trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
        internal bool GetConstraints(long constraintId, ref List<Constraints> constraintsList)
        {
            _trace?.Write(TraceLevel.Debug, "OPSPDMDatabase::GetConstraints");
            bool fnResult = true;

            try
            {
                CheckImdIsReadyOrThrow();

                if (constraintId == GlobalDefs.DEF_UNDEFINED_VALUE)
                    throw new ArgumentOutOfRangeException(nameof(constraintId), "Undefined constraint value");

                IMT_Constraints constraintsTable = imd.GetTable<IMT_Constraints>(MemoryDatabaseTables.Constraints);
                constraintsList = constraintsTable.GetConstraints(constraintId);
            }
            catch (Exception error)
            {
                _trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        public List<long> GetDayTypes(COPSDate pdtWork)
        {
            _trace?.Write(TraceLevel.Debug, $"OPSPDMDatabase::GetDayTypes pdtWork({pdtWork.fstrGetTraceString()})");
            List<long> fnResult = null;

            try
            {
                CheckImdIsReadyOrThrow();

                if (!pdtWork.IsValid())
                    throw new ArgumentOutOfRangeException(nameof(pdtWork), "Invalid date");

                IMT_Days days = imd.GetTable<IMT_Days>(MemoryDatabaseTables.Days);
                _trace?.Write(TraceLevel.Info, $"GetDayTypes - QUERY: Is the day ({pdtWork.fstrGetTraceString()}) an special day?");

                Days specialDay = days.IsSpecialDay(pdtWork);
                if (specialDay == null)
                {
                    _trace.Write(TraceLevel.Debug, $"GetDayTypes - {pdtWork.fstrGetTraceString()} day, is not an special day ==> Searching in table DAYS_DEF");

                    IMT_DaysDef daysDef = imd.GetTable<IMT_DaysDef>(MemoryDatabaseTables.DaysDef);
                    fnResult = daysDef.GetListTypeOfDays(pdtWork);
                }
                else {
                    _trace?.Write(TraceLevel.Info, $"{pdtWork.fstrGetTraceString()} day, is the special day {specialDay.DAY_ID} of type {specialDay.DAY_DDAY_ID}");

                    // As the day is special, put in the list, only the type of day that corresponds
                    fnResult = new List<long>() { specialDay.DAY_DDAY_ID};
                }
            }
            catch (Exception error)
            {
                _trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = null;
            }

            return fnResult;
        }

        private List<long> GetBlocksFromTime(COPSDate pdtWork)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::GetBlocksFromTime");
            List<long> fnResult = null;

            try
            {
                CheckImdIsReadyOrThrow();

                if (!pdtWork.IsValid()) {
                    throw new ArgumentOutOfRangeException(nameof(pdtWork),"Invalid date");
                }
                long minutesFromDate = pdtWork.TimeToMinutes();
                IMT_TimeTables timeTables = imd.GetTable<IMT_TimeTables>(MemoryDatabaseTables.TimeTables);
                fnResult = timeTables.GetTimFromDate(minutesFromDate);
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = null;
            }

            return fnResult;
        }

        internal long GetActualPayedQuantity(long? operationId)
        {
            _trace.Write(TraceLevel.Info, "OPSPDMDatabase::GetActualPayedQuantity");
            long fnResult = GlobalDefs.DEF_UNDEFINED_VALUE;

            try
            {
                Guard.IsNull(operationId, nameof(operationId));

                if (GeneralParams.UnitId != GlobalDefs.CC_UNIT_ID.ToString())
                {
                    CheckImdIsReadyOrThrow();
                    IMT_Operations operations = imd.GetTable<IMT_Operations>(MemoryDatabaseTables.Operations);

                    fnResult = operations.GetActualPayedQuantity(operationId.Value);
                }
                else
                {
                    //Estamos llamando desde el centro de control
                    fnResult = GetActualPayedQuantityDB(operationId.Value);
                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return fnResult;

        }

        private long GetActualPayedQuantityDB(long operationId)
        {
            OperationsRepository operationsRepository = new OperationsRepository(_connection);
            operationsRepository.Trace = this._trace;
            return operationsRepository.GetActualPayedQuantity(operationId);
        }

        public bool GetTariffBlock(long lTariff, ref List<long> dayTypes, ref COPSDate dtOper, long lNumDaysPassed, ref long lBlockID, 
                                    ref long lBlockIni, ref long lBlockEnd, ref long lSubTariff, ref bool bBlockFound, ref stTariff stTariffDesc, bool bConsiderCloseInterval)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::GetTariffBlock");
            bool fnResult = true;

            try
            {
                Guard.IsUndefined(lTariff, nameof(lTariff), "Invalid tariff id.");
                Guard.IsNull(dayTypes, nameof(dayTypes));
                Guard.IsLessThan(dayTypes.Count , 0, nameof(dayTypes), "No days in list");

                CheckImdIsReadyOrThrow();

                IMT_Tariffs tariffs = imd.GetTable<IMT_Tariffs>(MemoryDatabaseTables.Tariffs);
                IMT_TimeTables timetables = imd.GetTable<IMT_TimeTables>(MemoryDatabaseTables.TimeTables);

                Tariffs foundTariff = null;
                Timetables foundTimetables = null;

                foreach (long  dayType in dayTypes)
                {
                    bBlockFound = false;

                    if (!tariffs.GetTariffTimeTable(lTariff, dayType, dtOper, lNumDaysPassed, timetables, bConsiderCloseInterval, out foundTariff, out foundTimetables))
                    {
                        throw new InvalidOperationException("Error in call to pTariffs->GetTimeTable");
                    }

                    bBlockFound = (foundTariff != null && foundTimetables != null);
                    if (bBlockFound)
                    {
                        _trace.Write(TraceLevel.Info, $@"Block ID {foundTimetables.TIM_ID} Block Start [{foundTimetables.TIM_INI / 60:D2}:{foundTimetables.TIM_INI % 60:D2}] Block End [{foundTimetables.TIM_END / 60:D2}:{foundTimetables.TIM_END % 60:D2}] Type of day {dayType}");
                        bBlockFound = true;
                        break;
                    }
                    else
                    {
                        _trace.Write(TraceLevel.Info  , $"No Tariff for type of day {dayType}");
                    }

                }

                if (!bBlockFound)
                {
                    foreach (long dayType in dayTypes)
                    {
                        bBlockFound = false;

                        if (!tariffs.GetTariffTimeTable(lTariff, dayType, dtOper, GlobalDefs.DEF_UNDEFINED_VALUE, timetables, bConsiderCloseInterval, out foundTariff, out foundTimetables))
                        {
                            throw new InvalidOperationException("Error in call to pTariffs->GetTimeTable");
                        }

                        bBlockFound = (foundTariff != null && foundTimetables != null);

                        if (bBlockFound)
                        {
                            _trace.Write(TraceLevel.Info, $@"Block ID {foundTimetables.TIM_ID} Block Start [{foundTimetables.TIM_INI/60:D2}:{foundTimetables.TIM_INI% 60:D2}] Block End [{foundTimetables.TIM_END/60:D2}:{foundTimetables.TIM_END%60:D2}]  Type of day {dayType}");
                            bBlockFound = true;
                            break;
                        }
                        else
                        {
                            _trace.Write(TraceLevel.Info, $"No Tariff for type of day {dayType}");
                        }

                    }
                }



                if (bBlockFound && foundTimetables != null && foundTariff != null) {
                    // TIMETABLE
                    lBlockID = foundTimetables.TIM_ID;
                    lBlockIni = foundTimetables.TIM_INI;
                    lBlockEnd = foundTimetables.TIM_END;

                    // TARIFF
                    lSubTariff = foundTariff.TAR_STAR_ID.Value;
                    stTariffDesc.m_iNextBlock = foundTariff.TAR_NEXTBLOCK.Value;
                    stTariffDesc.m_iNextBlockConditionalValue= foundTariff.TAR_NB_CONDITIONAL_VALUE.Value;
                    stTariffDesc.m_iMaxTimeForNotApplyReentry = foundTariff.TAR_MAXTIMEFORNOTAPPLYREENTRY.Value;

                    stTariffDesc.m_bNextDay = (foundTariff.TAR_NEXTDAY.HasValue) ? (foundTariff.TAR_NEXTDAY.Value > 0) : false;
                    stTariffDesc.m_bResetNextBlockInt = (foundTariff.TAR_RNEXTBLOCKINT.HasValue) ? (foundTariff.TAR_RNEXTBLOCKINT.Value > 0) : false;
                    stTariffDesc.m_bResetNextBlockTime= (foundTariff.TAR_RNEXTBLOCKTIME.HasValue) ? (foundTariff.TAR_RNEXTBLOCKTIME.Value > 0) : false;
                    stTariffDesc.m_bResetNextDayInterval = (foundTariff.TAR_RNEXTDAYINT.HasValue) ? (foundTariff.TAR_RNEXTDAYINT.Value > 0) : false;
                    stTariffDesc.m_bResetNextDayTime = (foundTariff.TAR_RNEXTDAYTIME.HasValue) ? (foundTariff.TAR_RNEXTDAYTIME.Value > 0) : false;
                    stTariffDesc.m_bRoundEndOfDay = (foundTariff.TAR_ROUNDTOENDOFDAY.HasValue) ? (foundTariff.TAR_ROUNDTOENDOFDAY.Value > 0) : false;

                    bBlockFound = true;
                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        internal bool GetStatusFromBlock(long unitId, long groupId, long groupTypeId, long lBlockID, List<long> dayTypes, ref bool bFind, ref long lStatus)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::GetStatusFromBlock");
            bool fnResult = true;

            bool bLocalFind = false;
            long lStatusAct = GlobalDefs.DEF_UNDEFINED_VALUE;
            long idDay;

            try
            {
                CheckImdIsReadyOrThrow();

                IMT_Status statusTable = imd.GetTable<IMT_Status>(MemoryDatabaseTables.Status);

                lStatus = GlobalDefs.DEF_PDMOK;
                bFind = false;

                foreach (long dayType in dayTypes)
                {
                    if (!statusTable.GetDayBlockStatus(unitId, groupId, groupTypeId, lBlockID, dayType, ref bLocalFind, ref lStatusAct))
                        throw new InvalidOperationException("Error in call to pStatus->GetDayBlockStatus");

                    // Escogemos con el status mas restringido que es el mayor de ellos
                    if (bLocalFind)
                    {
                        if (bFind)
                        {
                            lStatus = Math.Max(lStatusAct, lStatus);
                        }
                        else
                        {
                            lStatus = lStatusAct;
                            bFind = true;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        internal bool GetIntervals(long lSubTariff, ref List<stINTERVAL> lstIntervals)
        {
            _trace.Write(TraceLevel.Debug, "OPSPDMDatabase::GetIntervals");
            bool fnResult = true;

            long lIntervalSize = 0;
            List<long> plIntervalSeconds = new List<long>();
            List<long> plIntervalMoney = new List<long>();
            List<int> piIntervalIntermediateValuesPossible = new List<int>();
            stINTERVAL stTmp;

            try
            {
                _trace.Write(TraceLevel.Info, $"Quering in memory for subtariff(INT_STAR_ID) {lSubTariff}");

                IMT_Intervals intervalsTable = imd.GetTable<IMT_Intervals>(MemoryDatabaseTables.Intervals);

                if (!intervalsTable.GetIntervals(lSubTariff, ref lIntervalSize, ref plIntervalSeconds, ref plIntervalMoney, ref piIntervalIntermediateValuesPossible))
                {
                    throw new InvalidOperationException("ERROR in call to pIntervals->GetIntervals");
                }

                for (int i = 0; i < lIntervalSize; i++)
                {
                    stTmp = new stINTERVAL();

                    stTmp.iTime = (int)plIntervalSeconds[i];
                    stTmp.iMoney = (int)plIntervalMoney[i];
                    stTmp.iIntervalIntermediateValuesPossible = piIntervalIntermediateValuesPossible[i];

                    lstIntervals.Add(stTmp);
                }
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        internal bool ExistTariffBlock(long lTariff, List<long> dayTypes, COPSDate dtOper, ref bool bBlockFound, bool bConsiderCloseInterval)
        {
            _trace?.Write(TraceLevel.Debug, "OPSPDMDatabase::GetTariffBlock");
            bool fnResult = true;

            try
            {
                Guard.IsUndefined(lTariff, nameof(lTariff), "Invalid Tariff id.");
                Guard.IsNull(dayTypes, nameof(dayTypes));
                Guard.IsNull(dtOper, nameof(dtOper));
                Guard.IsNull(bBlockFound, nameof(bBlockFound));

                if (dayTypes.Count <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(dayTypes), "Noo days in list");
                }

                bBlockFound = false;

                CheckImdIsReadyOrThrow();

                IMT_Tariffs tariffTable = imd.GetTable<IMT_Tariffs>(MemoryDatabaseTables.Tariffs);
                IMT_TimeTables timetablesTable = imd.GetTable<IMT_TimeTables>(MemoryDatabaseTables.TimeTables);


                foreach (long dayType in dayTypes)
                {
                    bBlockFound = false;

                    if (!tariffTable.ExistTimeTable(timetablesTable, lTariff, dayType, dtOper,ref  bBlockFound, bConsiderCloseInterval)) {
                        throw new InvalidOperationException("Error in call to pTariffs->GetTimeTable");
                    }

                    if (bBlockFound)
                    {
                        bBlockFound = true;
                        break;
                    }
                    else
                    {
                        _trace?.Write(TraceLevel.Debug, $"No Tariff for type of day {dayType}");
                    }
                }

            }
            catch (Exception error)
            {
                _trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
    }
}

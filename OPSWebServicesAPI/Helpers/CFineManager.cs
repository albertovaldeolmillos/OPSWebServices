using Oracle.ManagedDataAccess.Client;
using System;

namespace OPSWebServicesAPI.Helpers
{

    public class CFineManager
    {
        public const int C_ADMON_STATUS_PENDIENTE = 0;
        public const int C_ADMON_STATUS_CANCELADA = 1;
        public const int C_ADMON_STATUS_CANC_ERR_MULTIPLES = 2;
        public const int C_ADMON_STATUS_CANC_ERR_TIPO = 3;
        public const int C_ADMON_STATUS_CANC_ERR_IMPORTE = 4;
        public const int C_ADMON_STATUS_CANC_ERR_RETRASO = 5;
        public const int C_ADMON_STATUS_CANC_NO_CANCELABLE = 6;
        public const int C_ADMON_STATUS_ANULADA = 7;
        public const int C_ADMON_STATUS_ANULADA_POSTPAGO = 8;
        public const int C_ADMON_STATUS_ANULADA_VIGILANTE = 9;
        public const int C_ADMON_STATUS_INTERRUMPIDA_VIGILANTE = 10;
        public const int C_ADMON_STATUS_ANULACION_TRATADA = 11;
        public const int C_ADMON_STATUS_CANCELACION_TRATADA = 12;

        public const int C_STATUS_PEDIENTE = 10;
        public const int C_STATUS_ABORTADA = 20;
        public const int C_STATUS_GENERADA = 30;
        public const int C_STATUS_CANCELADA = 40;
        public const int C_STATUS_ANULADA = 50;

        public const int C_OPERACION_ESTACIONAMIENTO = 1;
        public const int C_OPERACION_PROLONGACION = 2;
        public const int C_OPERACION_PAGO_SANCION = 4;

        public const int MONTH_JULY = 7;
        public const int MONTH_AUGUST = 8;


        protected System.Data.IDbConnection dbConnection = null;
        protected System.Data.IDbTransaction dbTransaction = null;
        protected ILogger _logger;

        public CFineManager()
        {

        }

        public void SetDBConnection(System.Data.IDbConnection pdbConnection)
        {
            dbConnection = pdbConnection;
        }

        public void SetDBTransaction(System.Data.IDbTransaction pdbTransaction)
        {
            dbTransaction = pdbTransaction;
            dbConnection = pdbTransaction.Connection;
        }


        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }


        public void SetFineStatus(int iFineId)
        {
            OracleDataReader dr = null;
            OracleCommand selFineCmd = null;
            int iWorkFineId = 0;
            int iNumInterruptions = 0;
            int iNumRevokations = 0;
            int iNumPostPays = 0;
            bool bIsFineVIP = false;

            try
            {
                selFineCmd = new OracleCommand();
                selFineCmd.Connection = (OracleConnection)dbConnection;
                selFineCmd.Transaction = (OracleTransaction)dbTransaction;


                selFineCmd.CommandText = string.Format("select FIN_STATUS, FIN_STATUSADMON, FIN_VEHICLEID, FIN_FIN_ID, " +
                                                        "FIN_DFIN_ID, FIN_GRP_ID_ZONE, to_char(FIN_DATE,'HH24MISSDDMMYY') FIN_DATE " +
                                                        "from fines " +
                                                        "where fin_id={0}",
                                                        iFineId);

                dr = selFineCmd.ExecuteReader();


                if (dr.Read())
                {
                    int iFinStatus;
                    int iFinAdmonStatus;
                    int iFinFinId = -1;
                    int iFinType = -1;
                    string strVehicleId = "";
                    int iFinGrpId = -1;
                    int iCorrectCancelations = 0;
                    int iBadTypeCancelations = 0;
                    int iDelayedCancelations = 0;
                    int iBadQuantityCancelations = 0;
                    int iNoCancelInPDM = 0;
                    int iStatusAdmon = C_ADMON_STATUS_PENDIENTE;
                    DateTime dtFineDateTime;

                    try
                    {
                        iFinStatus = dr.GetInt32(dr.GetOrdinal("FIN_STATUS"));
                        iFinAdmonStatus = dr.GetInt32(dr.GetOrdinal("FIN_STATUSADMON"));
                        iFinType = dr.GetInt32(dr.GetOrdinal("FIN_DFIN_ID"));
                        dtFineDateTime = StringToDtx(dr.GetString(dr.GetOrdinal("FIN_DATE")).ToString());
                        strVehicleId = dr.GetString(dr.GetOrdinal("FIN_VEHICLEID")).ToString();
                        iFinGrpId = dr.GetInt32(dr.GetOrdinal("FIN_GRP_ID_ZONE"));

                        if (iFinAdmonStatus != C_ADMON_STATUS_ANULADA)
                        {
                            switch (iFinStatus)
                            {
                                case C_STATUS_ABORTADA:
                                case C_STATUS_GENERADA:
                                    iWorkFineId = iFineId;
                                    break;
                                case C_STATUS_CANCELADA:
                                case C_STATUS_ANULADA:

                                    if (!dr.IsDBNull(dr.GetOrdinal("FIN_FIN_ID")))
                                    {
                                        iFinFinId = dr.GetInt32(dr.GetOrdinal("FIN_FIN_ID"));
                                    }

                                    iWorkFineId = iFinFinId;
                                    break;
                                default:
                                    break;
                            }


                            iNumInterruptions = GetNumberInterruptions(iWorkFineId);
                            iNumRevokations = GetNumberRevokations(iWorkFineId);
                            GetNumberCancelations(iWorkFineId, iFinType, dtFineDateTime,
                                                    ref iCorrectCancelations,
                                                    ref iBadTypeCancelations,
                                                    ref iDelayedCancelations,
                                                    ref iBadQuantityCancelations,
                                                    ref iNoCancelInPDM);

                            if (IsFineTypePostPayable(iFinType))
                            {
                                iNumPostPays = GetNumberPostPays(dtFineDateTime, strVehicleId, iFinGrpId);
                            }

                            bIsFineVIP = IsFineVIPVehicleId(strVehicleId);

                            switch (iFinStatus)
                            {
                                case C_STATUS_ABORTADA:
                                case C_STATUS_CANCELADA:
                                    if (iNumInterruptions > 0)
                                    {
                                        iStatusAdmon = C_ADMON_STATUS_INTERRUMPIDA_VIGILANTE;
                                    }

                                    break;
                                case C_STATUS_GENERADA:
                                case C_STATUS_ANULADA:
                                    if (iNumRevokations > 0)
                                    {
                                        iStatusAdmon = C_ADMON_STATUS_ANULADA_VIGILANTE;
                                    }
                                    else if (bIsFineVIP)
                                    {
                                        iStatusAdmon = C_ADMON_STATUS_ANULADA;
                                    }
                                    else if (iNumPostPays > 0)
                                    {
                                        iStatusAdmon = C_ADMON_STATUS_ANULADA_POSTPAGO;
                                    }
                                    else if (iCorrectCancelations == 1)
                                    {
                                        iStatusAdmon = C_ADMON_STATUS_CANCELADA;
                                    }
                                    else if ((iCorrectCancelations > 1) &&
                                        (iBadTypeCancelations == 0) &&
                                        (iDelayedCancelations == 0) &&
                                        (iBadQuantityCancelations == 0) &&
                                        (iNoCancelInPDM == 0))
                                    {
                                        iStatusAdmon = C_ADMON_STATUS_CANC_ERR_MULTIPLES;
                                    }
                                    else if (iNoCancelInPDM > 0)
                                    {
                                        iStatusAdmon = C_ADMON_STATUS_CANC_NO_CANCELABLE;
                                    }
                                    else if (iBadQuantityCancelations > 0)
                                    {
                                        iStatusAdmon = C_ADMON_STATUS_CANC_ERR_IMPORTE;
                                    }
                                    else if (iDelayedCancelations > 0)
                                    {
                                        iStatusAdmon = C_ADMON_STATUS_CANC_ERR_RETRASO;
                                    }
                                    else if (iBadTypeCancelations > 0)
                                    {
                                        iStatusAdmon = C_ADMON_STATUS_CANC_ERR_TIPO;
                                    }

                                    break;
                                default:
                                    break;
                            }

                            UpdateAdmonStatusAuto(iWorkFineId, iStatusAdmon);

                            switch (iStatusAdmon)
                            {
                                case C_ADMON_STATUS_PENDIENTE:
                                case C_ADMON_STATUS_CANC_ERR_IMPORTE:
                                case C_ADMON_STATUS_CANC_ERR_RETRASO:
                                case C_ADMON_STATUS_CANC_NO_CANCELABLE:
                                    UpdateAdmonStatus(iWorkFineId, C_ADMON_STATUS_PENDIENTE);
                                    break;
                                case C_ADMON_STATUS_CANCELADA:
                                case C_ADMON_STATUS_CANC_ERR_MULTIPLES:
                                case C_ADMON_STATUS_CANC_ERR_TIPO:
                                    UpdateAdmonStatus(iWorkFineId, C_ADMON_STATUS_CANCELADA);
                                    break;
                                case C_ADMON_STATUS_ANULADA:
                                case C_ADMON_STATUS_ANULADA_POSTPAGO:
                                    UpdateAdmonStatus(iWorkFineId, C_ADMON_STATUS_ANULADA);
                                    break;
                                case C_ADMON_STATUS_ANULADA_VIGILANTE:
                                    UpdateAdmonStatus(iWorkFineId, C_ADMON_STATUS_ANULADA);
                                    UpdateRevokations(iWorkFineId);
                                    break;
                                case C_ADMON_STATUS_INTERRUMPIDA_VIGILANTE:
                                    UpdateAdmonStatus(iWorkFineId, C_ADMON_STATUS_ANULADA);
                                    UpdateInterruptions(iWorkFineId);
                                    break;
                                default:
                                    break;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Logger_AddLogException(e);
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }

            finally
            {

                if (dr != null)
                {
                    dr.Close();
                    dr = null;
                }
                if (selFineCmd != null)
                {
                    selFineCmd.Dispose();
                    selFineCmd = null;
                }

            }

        }


        public void RevokeFinesWithPostpay(int iOpeId)
        {
            OracleDataReader dr = null;
            OracleCommand selFineCmd = null;
            int iFinId = -1;

            try
            {
                selFineCmd = new OracleCommand();
                selFineCmd.Connection = (OracleConnection)dbConnection;
                selFineCmd.Transaction = (OracleTransaction)dbTransaction;

                selFineCmd.CommandText = string.Format("select fin_id " +
                                                        "from operations o, fines f " +
                                                        "where o.ope_dope_id in ({0}, {1}) " +
                                                        "  and o.ope_post_pay = 1 " +
                                                        "  and o.ope_vehicleid = f.fin_vehicleid " +
                                                        "  and o.ope_grp_id = f.fin_grp_id_zone " +
                                                        "  and f.fin_date between o.ope_inidate and o.ope_enddate " +
                                                        "  and f.fin_status = {2} " +
                                                        "  and ope_id = {3}",
                                                        C_OPERACION_ESTACIONAMIENTO,
                                                        C_OPERACION_PROLONGACION,
                                                        C_STATUS_GENERADA, iOpeId);



                dr = selFineCmd.ExecuteReader();


                while (dr.Read())
                {
                    try
                    {
                        iFinId = dr.GetInt32(dr.GetOrdinal("FIN_ID"));
                        SetFineStatus(iFinId);

                    }
                    catch (Exception e)
                    {
                        Logger_AddLogException(e);
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }

            finally
            {

                if (dr != null)
                {
                    dr.Close();
                    dr = null;
                }
                if (selFineCmd != null)
                {
                    selFineCmd.Dispose();
                    selFineCmd = null;
                }

            }
        }


        public bool IsFinePaymentInTime(DateTime dtFineDatetime, DateTime dtOpePaymentDateTime, int iTimeForPayment, string strDayCode)
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
                        if (!IsHollyday(dtWork))
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

        public bool IsFinePaymentInTimeHondarribia(DateTime dtFineDatetime, DateTime dtOpePaymentDateTime, int iTimeForPayment, string strDayCode)
        {
            bool bReturn = false;
            Logger_AddLogMessage(string.Format("dtFineDatetime={0}, dtOpePaymentDateTime={1}, iTimeForPayment={2}, strDayCode={3}",
                dtFineDatetime.ToString("dd/MM/yyyy HH:mm:ss"), dtOpePaymentDateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                iTimeForPayment.ToString(), strDayCode.ToString()),
                LoggerSeverities.Debug);
            try
            {

                if (iTimeForPayment > 0)
                {
                    int iMaxNumDays = (iTimeForPayment / (24 * 60));
                    int iMaxMinutes = iTimeForPayment - (iMaxNumDays * 24 * 60);
                    int iCurrNumDays = 0;
                    int iCurrLoops = 0;
                    DateTime dtTemp = dtFineDatetime;
                    TimeSpan ts1Day = new TimeSpan(1, 0, 0, 0);
                    dtTemp = dtTemp + ts1Day;

                    int difMinutes = (dtOpePaymentDateTime.Hour * 60 + dtOpePaymentDateTime.Minute) - (dtFineDatetime.Hour * 60 + dtFineDatetime.Minute);

                    DateTime dtWork = new DateTime(dtTemp.Year, dtTemp.Month, dtTemp.Day, 0, 0, 0);

                    Logger_AddLogMessage(string.Format("iMaxNumDays={0}, iMaxMinutes={1}, iCurrNumDays={2}, iCurrLoops={3}, difMinutes={4}, dtWork={5}",
                        iMaxNumDays.ToString(), iMaxMinutes.ToString(), iCurrNumDays.ToString(), iCurrLoops.ToString(), difMinutes.ToString(), dtWork.ToString("dd/MM/yyyy HH:mm:ss")),
                        LoggerSeverities.Debug);

                    while ((iCurrNumDays < iMaxNumDays) && (iCurrLoops < 60))
                    {
                        if (!IsHollyday(dtWork))
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

                    bReturn = ((dtFineDatetime < dtOpePaymentDateTime) && (dtOpePaymentDateTime < dtWork) && (difMinutes <= iMaxMinutes));

                    Logger_AddLogMessage(string.Format("FinePaymentInTime={0}, Fecha Multa: {1}; Fecha Límite: {2}, Fecha Operación: {3}, Minutos para abonar: {4}",
                        bReturn, dtFineDatetime.ToString(), dtWork.ToString(),
                        dtOpePaymentDateTime.ToString(), iTimeForPayment.ToString()), LoggerSeverities.Info);


                }

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
                bReturn = false;
            }

            return bReturn;
        }

        private int GetNumberInterruptions(int iFineId)
        {
            OracleCommand selCmd = null;
            int iRes = 0;
            try
            {
                selCmd = new OracleCommand();
                selCmd.Connection = (OracleConnection)dbConnection;
                selCmd.Transaction = (OracleTransaction)dbTransaction;

                selCmd.CommandText = string.Format("select count(*) " +
                                                "from fines f " +
                                                "where f.fin_status={0} and fin_fin_id={1}",
                                                C_STATUS_CANCELADA, iFineId);
                iRes = int.Parse(selCmd.ExecuteScalar().ToString());

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }
            finally
            {
                if (selCmd != null)
                {
                    selCmd.Dispose();
                    selCmd = null;
                }
            }
            return iRes;
        }

        private void UpdateInterruptions(int iFineId)
        {
            OracleCommand updCmd = null;
            try
            {
                updCmd = new OracleCommand();
                updCmd.Connection = (OracleConnection)dbConnection;
                updCmd.Transaction = (OracleTransaction)dbTransaction;
                updCmd.CommandText = string.Format("update " +
                    "fines set fin_statusadmonauto={0}, fin_statusadmon={1} " +
                    "where fin_status={2} and fin_fin_id={3}",
                    C_ADMON_STATUS_CANCELACION_TRATADA,
                    C_ADMON_STATUS_ANULADA,
                    C_STATUS_CANCELADA, iFineId);

                updCmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }
            finally
            {
                if (updCmd != null)
                {
                    updCmd.Dispose();
                    updCmd = null;
                }
            }
            return;
        }


        private int GetNumberRevokations(int iFineId)
        {
            OracleCommand selCmd = null;
            int iRes = 0;
            try
            {
                selCmd = new OracleCommand();
                selCmd.Connection = (OracleConnection)dbConnection;
                selCmd.Transaction = (OracleTransaction)dbTransaction;
                selCmd.CommandText = string.Format("select count(*) " +
                    "from fines f " +
                    "where f.fin_status={0} and fin_fin_id={1}",
                    C_STATUS_ANULADA, iFineId);
                iRes = int.Parse(selCmd.ExecuteScalar().ToString());

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }
            finally
            {
                if (selCmd != null)
                {
                    selCmd.Dispose();
                    selCmd = null;
                }
            }
            return iRes;
        }

        private void UpdateRevokations(int iFineId)
        {
            OracleCommand updCmd = null;
            try
            {
                updCmd = new OracleCommand();
                updCmd.Connection = (OracleConnection)dbConnection;
                updCmd.Transaction = (OracleTransaction)dbTransaction;
                updCmd.CommandText = string.Format("update " +
                    "fines set fin_statusadmonauto={0}, fin_statusadmon={1} " +
                    "where fin_status={2} and fin_fin_id={3}",
                    C_ADMON_STATUS_ANULACION_TRATADA,
                    C_ADMON_STATUS_ANULADA,
                    C_STATUS_ANULADA, iFineId);

                updCmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }
            finally
            {
                if (updCmd != null)
                {
                    updCmd.Dispose();
                    updCmd = null;
                }
            }
            return;
        }


        private int GetNumberPostPays(DateTime dtFineDatetime, string strVehicleId, int iFineGrpId)
        {
            OracleCommand selCmd = null;
            int iRes = 0;
            try
            {
                string strFineDateTime;
                strFineDateTime = DtxToString(dtFineDatetime);
                selCmd = new OracleCommand();
                selCmd.Connection = (OracleConnection)dbConnection;
                selCmd.Transaction = (OracleTransaction)dbTransaction;

                selCmd.CommandText = string.Format("select count(*) " +
                                                "from operations o " +
                                                "where o.ope_dope_id in ({0},{1}) and " +
                                                "	o.ope_post_pay=1 and " +
                                                "	o.ope_vehicleid='{2}' and " +
                                                "	o.ope_grp_id='{3}' and " +
                                                "	to_date('{4}','HH24MISSDDMMYY') between o.ope_inidate and o.ope_enddate",
                                                C_OPERACION_ESTACIONAMIENTO,
                                                C_OPERACION_PROLONGACION,
                                                strVehicleId, iFineGrpId, strFineDateTime);

                iRes = int.Parse(selCmd.ExecuteScalar().ToString());

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }
            finally
            {
                if (selCmd != null)
                {
                    selCmd.Dispose();
                    selCmd = null;
                }
            }
            return iRes;
        }

        private bool IsFineTypePostPayable(int iFineType)
        {
            OracleCommand selCmd = null;
            int iRes = 0;
            bool bReturn = false;
            try
            {
                selCmd = new OracleCommand();
                selCmd.Connection = (OracleConnection)dbConnection;
                selCmd.Transaction = (OracleTransaction)dbTransaction;
                selCmd.CommandText = string.Format("select count(*) " +
                                                 "from fines_def  " +
                                                 "where dfin_id={0} and dfin_postpayable=1", iFineType);

                iRes = int.Parse(selCmd.ExecuteScalar().ToString());
                bReturn = (iRes > 0);

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }
            finally
            {
                if (selCmd != null)
                {
                    selCmd.Dispose();
                    selCmd = null;
                }
            }
            return bReturn;
        }

        private bool IsFineVIPVehicleId(string strVehicleId)
        {
            OracleCommand selCmd = null;
            int iRes = 0;
            bool bReturn = false;
            try
            {
                selCmd = new OracleCommand();
                selCmd.Connection = (OracleConnection)dbConnection;
                selCmd.Transaction = (OracleTransaction)dbTransaction;

                selCmd.CommandText = string.Format("select count(*) from fines_vip t where vfin_vehicleid='{0}'",
                    strVehicleId);
                iRes = int.Parse(selCmd.ExecuteScalar().ToString());
                bReturn = (iRes > 0);

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }
            finally
            {
                if (selCmd != null)
                {
                    selCmd.Dispose();
                    selCmd = null;
                }
            }
            return bReturn;
        }


        private void UpdateAdmonStatus(int iFineId, int iFineAdmonStatus)
        {
            OracleCommand updCmd = null;
            try
            {
                updCmd = new OracleCommand();
                updCmd.Connection = (OracleConnection)dbConnection;
                updCmd.Transaction = (OracleTransaction)dbTransaction;
                updCmd.CommandText = string.Format("update " +
                    "fines set fin_statusadmon={0} " +
                    "where fin_id={1}",
                    iFineAdmonStatus,
                    iFineId);

                updCmd.ExecuteNonQuery();
                Logger_AddLogMessage(string.Format("La multa {0} pasa a tener Status Admon           : {1} - {2}",
                    iFineId, iFineAdmonStatus, getAdmonStatusString(iFineAdmonStatus)), LoggerSeverities.Info);

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }
            finally
            {
                if (updCmd != null)
                {
                    updCmd.Dispose();
                    updCmd = null;
                }
            }
            return;
        }

        private void UpdateAdmonStatusAuto(int iFineId, int iFineAdmonStatus)
        {
            OracleCommand updCmd = null;
            try
            {
                updCmd = new OracleCommand();
                updCmd.Connection = (OracleConnection)dbConnection;
                updCmd.Transaction = (OracleTransaction)dbTransaction;
                updCmd.CommandText = string.Format("update " +
                    "fines set fin_statusadmonauto={0} " +
                    "where fin_id={1}",
                    iFineAdmonStatus,
                    iFineId);

                updCmd.ExecuteNonQuery();
                Logger_AddLogMessage(string.Format("La multa {0} pasa a tener Status Admon Automático: {1} - {2}",
                    iFineId, iFineAdmonStatus, getAdmonStatusString(iFineAdmonStatus)), LoggerSeverities.Info);

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }
            finally
            {
                if (updCmd != null)
                {
                    updCmd.Dispose();
                    updCmd = null;
                }
            }
            return;
        }

        private bool GetNumberCancelations(int iFineId, int iFineType, DateTime dtFineDatetime,
                                            ref int iCorrectCancelations,
                                            ref int iBadTypeCancelations,
                                            ref int iDelayedCancelations,
                                            ref int iBadQuantityCancelations,
                                            ref int iNoCancelInPDM)
        {
            OracleCommand selCmd = null;
            OracleDataReader dr = null;
            DateTime dtMovdate = DateTime.Now;
            int iOpeValue = 0;
            int iOpeFinType = 0;
            int iTypeValue = 0;
            int iTimeForPayment = 0;
            bool bFinePaymentInTime;
            bool bQuantityCorrect;
            bool bTypeCorrect;
            bool bCancelInPDM;
            string strFineDateTime;
            string strDayCode;

            bool bReturn = true;
            iCorrectCancelations = 0;
            iBadTypeCancelations = 0;
            iDelayedCancelations = 0;
            iBadQuantityCancelations = 0;
            iNoCancelInPDM = 0;

            try
            {
                strFineDateTime = DtxToString(dtFineDatetime);
                selCmd = new OracleCommand();
                selCmd.Connection = (OracleConnection)dbConnection;
                selCmd.Transaction = (OracleTransaction)dbTransaction;
                selCmd.CommandText = string.Format("select to_char(ope_movdate,'HH24MISSDDMMYY') ope_movdate," +
                                                 "ope_value,ope_fin_dfin_id,dfinq_value,dfin_payinpdm,dday_code " +
                                                 "from operations o,fines_def f, fines_def_quantity fdq, days_def dd " +
                                                 "where o.ope_dope_id={0} and dfin_pay_dday_id=dday_id and " +
                                                 "f.dfin_id=o.ope_fin_dfin_id and fdq.dfinq_id=f.dfin_id and " +
                                                 "to_date('{2}','HH24MISSDDMMYY')>= fdq.dfinq_inidate and " +
                                                 "to_date('{2}','HH24MISSDDMMYY')< fdq.dfinq_endate and " +
                                                 "((o.ope_movdate-to_date('{2}','HH24MISSDDMMYY'))*24*60)>fdq.DFINQ_INI_MINUTE and " +
                                                 "((o.ope_movdate-to_date('{2}','HH24MISSDDMMYY'))*24*60)<=fdq.DFINQ_END_MINUTE and " +
                                                 "o.ope_fin_id={1} and o.ope_movdate > to_date('{2}','HH24MISSDDMMYY')",
                                                 C_OPERACION_PAGO_SANCION, iFineId, strFineDateTime);

                dr = selCmd.ExecuteReader();

                while (dr.Read())
                {
                    try
                    {
                        dtMovdate = StringToDtx(dr.GetString(dr.GetOrdinal("OPE_MOVDATE")).ToString());
                        iOpeValue = dr.GetInt32(dr.GetOrdinal("OPE_VALUE"));
                        iOpeFinType = dr.GetInt32(dr.GetOrdinal("OPE_FIN_DFIN_ID"));
                        iTypeValue = dr.GetInt32(dr.GetOrdinal("DFINQ_VALUE"));
                        iTimeForPayment = dr.GetInt32(dr.GetOrdinal("DFIN_PAYINPDM"));
                        strDayCode = dr.GetString(dr.GetOrdinal("DDAY_CODE"));

                        bFinePaymentInTime = IsFinePaymentInTime(dtFineDatetime, dtMovdate, iTimeForPayment, strDayCode);
                        bQuantityCorrect = (iOpeValue == iTypeValue);
                        bTypeCorrect = (iOpeFinType == iFineType);
                        bCancelInPDM = (iTimeForPayment > 0);


                        if (bFinePaymentInTime && bQuantityCorrect && bTypeCorrect && bCancelInPDM)
                        {
                            iCorrectCancelations++;
                        }
                        else if (!bCancelInPDM)
                        {
                            iNoCancelInPDM++;
                        }
                        else if (!bQuantityCorrect)
                        {
                            iBadQuantityCancelations++;
                        }
                        else if (!bFinePaymentInTime)
                        {
                            iDelayedCancelations++;
                        }
                        else if (!bTypeCorrect)
                        {
                            iBadTypeCancelations++;
                        }

                    }
                    catch (Exception e)
                    {
                        Logger_AddLogException(e);
                    }
                }


            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
                bReturn = false;
            }
            finally
            {
                if (dr != null)
                {
                    dr.Close();
                    dr = null;
                }

                if (selCmd != null)
                {
                    selCmd.Dispose();
                    selCmd = null;
                }
            }
            return bReturn;
        }


        private bool IsHollyday(DateTime dt)
        {
            OracleCommand selCmd = null;

            bool bReturn = false;
            int iRes = 0;

            try
            {

                string strDateTime = DtxToString(dt);
                selCmd = new OracleCommand();
                selCmd.Connection = (OracleConnection)dbConnection;
                selCmd.Transaction = (OracleTransaction)dbTransaction;
                selCmd.CommandText = string.Format("select count(*) " +
                                                "from days " +
                                                "where day_date = to_date('{0}', 'HH24MISSDDMMYY')",
                                                strDateTime);
                iRes = int.Parse(selCmd.ExecuteScalar().ToString());
                bReturn = (iRes > 0);

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
                bReturn = false;
            }
            finally
            {

                if (selCmd != null)
                {
                    selCmd.Dispose();
                    selCmd = null;
                }
            }
            return bReturn;
        }

        public DateTime StringToDtx(string s)
        {
            int hour = Convert.ToInt32(s.Substring(0, 2));
            int minute = Convert.ToInt32(s.Substring(2, 2));
            int second = Convert.ToInt32(s.Substring(4, 2));
            int day = Convert.ToInt32(s.Substring(6, 2));
            int month = Convert.ToInt32(s.Substring(8, 2));
            int year = Convert.ToInt32(s.Substring(10, 2));
            DateTime dt = new DateTime(2000 + year, month, day, hour, minute, second);
            return dt;
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

        private void Logger_AddLogMessage(string msg, LoggerSeverities severity)
        {
            _logger.AddLog(msg, severity);
        }

        private void Logger_AddLogException(Exception ex)
        {
            _logger.AddLog(ex);
        }

        private string getAdmonStatusString(int iAdmonStatus)
        {
            string strResult = "";

            switch (iAdmonStatus)
            {
                case C_ADMON_STATUS_PENDIENTE:
                    strResult = "Multa Pendiente";
                    break;
                case C_ADMON_STATUS_CANCELADA:
                    strResult = "Multa Cancelada";
                    break;
                case C_ADMON_STATUS_CANC_ERR_MULTIPLES:
                    strResult = "Multa cancelada múltiples veces";
                    break;
                case C_ADMON_STATUS_CANC_ERR_TIPO:
                    strResult = "Multa cancelada con error de tipo";
                    break;
                case C_ADMON_STATUS_CANC_ERR_IMPORTE:
                    strResult = "Multa cancelada con importe erróneo";
                    break;
                case C_ADMON_STATUS_CANC_ERR_RETRASO:
                    strResult = "Multa cancelada con retraso";
                    break;
                case C_ADMON_STATUS_CANC_NO_CANCELABLE:
                    strResult = "Multa cancelada en parquímetro sin ser cancelable en parquímetro";
                    break;
                case C_ADMON_STATUS_ANULADA:
                    strResult = "Multa anulada";
                    break;
                case C_ADMON_STATUS_ANULADA_POSTPAGO:
                    strResult = "Multa anulada por postpago";
                    break;
                case C_ADMON_STATUS_ANULADA_VIGILANTE:
                    strResult = "Multa anulada por vigilante";
                    break;
                case C_ADMON_STATUS_INTERRUMPIDA_VIGILANTE:
                    strResult = "Multa interrumpida por vigilante";
                    break;
                default:
                    break;
            }

            return strResult;


        }

        public void UpdateOperationFineNumber(long lOperId, string sFineNumber)
        {
            OracleCommand updCmd = null;
            try
            {
                updCmd = new OracleCommand();
                updCmd.Connection = (OracleConnection)dbConnection;
                updCmd.Transaction = (OracleTransaction)dbTransaction;
                updCmd.CommandText = string.Format("update " +
                    "operations set ope_fin_number='{0}' " +
                    "where ope_id={1}",
                    sFineNumber,
                    lOperId);

                updCmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }
            finally
            {
                if (updCmd != null)
                {
                    updCmd.Dispose();
                    updCmd = null;
                }
            }
            return;
        }

    }
}
using PDMDatabase;
using PDMDatabase.Models;
using PDMHelpers;
using PDMHelpers.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace PDMCompute
{
    public struct stTariff {
        public bool m_bNextDay;
        public int m_iNextBlock;
        public int m_iNextBlockConditionalValue;
        public int m_iMaxTimeForNotApplyReentry;
        public bool m_bResetNextBlockTime;
        public bool m_bResetNextBlockInt;
        public bool m_bResetNextDayInterval;
        public bool m_bResetNextDayTime;
        public bool m_bRoundEndOfDay;
        public int m_nTarID;
        public int m_nNumber;
    }

    public struct stINTERVAL
    {
        public long iTime;
        public long iMoney;
        public long iIntervalIntermediateValuesPossible;
    };

    public struct stStepCalculation
    {
        public long lMoney;
        public long lMinutes;
        public COPSDate dtDate;
    };


    public partial class M1ComputeEx0 : TariffCalculator
    {
        private bool m_bApplyVehicleHistory;
        private bool m_bNotPrunedTreeInit;
        private M1ComputeEx0 m_pm1ComputeExAux;

        private CM1GroupsTree m_Tree;                   // GroupsTree
        private CM1GroupsTree m_NotPrunedTree;

        // M1 parameters
        private long m_lUnitId;
        private long m_lOperType;
        private long m_lGroupId;
        private long m_lTypeOfGroup;
        private long m_lArticleDef;
        private long m_lArticle;
        private int m_iPaymentType;
        private COPSPlate m_strVehicleId = new COPSPlate();
        private COPSDate m_dtOper = new COPSDate();
        private COPSDate m_dtLimit = new COPSDate();
        private COPSDate m_dtFirst = new COPSDate();
        private long m_lOperMoney;
        private bool m_bOperMoneySet;
        private bool m_bTimeLimitsCompute;
        private long m_lMaxTime;
        private long m_lMinTime;
        private COPSDate m_dtMinOperDate;

        // M1 internal data
        private long m_lResult;
        private COPSDate m_dtOperFinal = new COPSDate();
        private COPSDate m_dtOperInitial = new COPSDate();
        private COPSDate m_dtOperTariffInitial = new COPSDate();
        private long m_lMaxMoney;
        private long m_lMinMoney;
        private long m_lMaxMinutes;
        private long m_lCurrMoney;
        private long m_lCurrMinutes;
        private long m_lTariffMoney;
        private long m_lAccMoney;
        private long m_lAccMinutes;
        private long m_lAccMoneyGroup;
        private long m_lAccMinutesGroup;
        private long m_lTariff;
        private stINTERVAL m_stInitialIntervalOffset;
        private bool m_bTimeReset;
        private bool m_bIntReset;
        private COPSDate m_dtOperEfectiveInit = new COPSDate();
        private bool m_dtDayChange;
        private long m_lBlocksWithTariff;
        private long m_lRealAccMoney;
        private long m_lRealAccMinutes;
        private int m_iWholeOperationWithChipCard;
        private int m_iWholeOperationWithMobile;
        private COPSDate m_dtOperDateRealIni = new COPSDate();
        private long m_lRealCurrMoney;
        private long m_lRealCurrMinutes;
        private bool m_bPostPay;
        private bool m_bHistOpCourtesy;
        private bool m_bHistOpPostPay;
        private long m_lPostPayMinutes;
        private long m_lFirstGroupId;
        private long m_lPreHistMaxMoney;
        private long m_lPreHistMaxMinutes;
        private bool m_bHistPostPay;
        private bool m_bOperMaxMoneySet;
        private long m_lTimeLimit;
        private bool m_bIgnoreResets;
        private bool m_bHistOnlyWithSamePaymentType;
        private stINTERVAL m_stIntervalOffset;
        private bool m_bConsiderCloseFirstInterval;
        private int m_iRemainingMinutesWithZeroValue;
        private bool m_bIntraZoneParkException;
        private bool m_bRoundEndOfDay;
        private COPSDate m_dtRoundEndOfDay = new COPSDate();
        private long m_lRoundEndOfDayMinutes;
        private long m_lRoundEndOfDayRealMinutes;
        private long m_lRoundEndOfDayMoney;
        private long m_lRoundEndOfDayRealMoney;
        private bool m_bRoundEndOfDayAmountIsAllowed;
        private bool m_bCurrentAmountIsAllowed;


        private COPSDate m_dtLastAllowedAmountEndDate = new COPSDate();
        private long m_lLastAllowedAmountMinutes;
        private long m_lLastAllowedAmountRealMinutes;
        private long m_lLastAllowedAmountMoney;
        private long m_lLastAllowedAmountRealMoney;
        private COPSDate m_dtFirstAllowedAmountEndDate = new COPSDate(); // calculo de limites
        private long m_lFirstAllowedAmountRealMinutes; /// calculo de limites
        private long m_lFirstAllowedAmountMoney;//calculo de limites
        private long m_lFirstAllowedAmountRealMoney;//calculo de limites
        private long m_lInitialTimeLastInterval;
        private long m_lEndTimeLastInterval;
        private long m_lCurrentTimeLastInterval;


        private long m_lInAddFreeMinutesQuantity;
        private long m_lInAddFreeMoneyQuantity;
        private long m_lOutAddFreeMinutesQuantity;
        private long m_lOutAddFreeMoneyQuantity;
        private bool m_bHistFreeMoneyUsed;

        private bool m_bCalculateTimeSteps;
        private bool m_bCalculateQuantitySteps;
        private int m_lCalculateSteps_StepValue;
        private int m_lCalculateSteps_CurrentValue;

        List<stStepCalculation> m_CalculateSteps_Steps;

        private COPSDate GetOperTariffInitialDateTime()
        {
            return m_dtOperTariffInitial;
        }
        private void SetOperTariffInitialDateTime(COPSDate date)
        {
            m_dtOperTariffInitial = date.Copy();
        }

        private long GetMinMoney()
        {
            return m_lMinMoney;
        }
        private void SetMinMoney(long minMoney)
        {
            m_lMinMoney = minMoney;
        }

        private long GetMoney()
        {
            return m_lOperMoney;
        }
        private void SetMoney(long money)
        {
            m_lOperMoney = money;
        }

        private long GetMaxMoney()
        {
            return m_lMaxMoney;
        }
        public void SetMaxMoney(long newMaxMoney)
        {
            m_lMaxMoney = newMaxMoney;
        }

        private COPSDate GetFirstDateTime()
        {
            return m_dtFirst;
        }
        private void SetFirstDateTime(COPSDate pdtIni)
        {
            m_dtFirst = pdtIni.Copy();
        }

        private COPSDate GetInitialDateTime()
        {
            return m_dtOperInitial;
        }
        public void SetInitialDateTime(COPSDate date)
        {
            m_dtOperInitial = date.Copy();
            SetOperEfectiveInitDateTime(date);
        }

        private COPSDate GetOperEfectiveInitDateTime()
        {
            return m_dtOperEfectiveInit;
        }
        public void SetOperEfectiveInitDateTime(COPSDate date)
        {
            m_dtOperEfectiveInit = date.Copy();
        }

        public void SetVehicleId(COPSPlate vehicleId)
        {
            m_strVehicleId = new COPSPlate(vehicleId.ToString());
        }
        private COPSPlate GetVehicleId()
        {
            return m_strVehicleId;
        }

        private long GetPaymentType()
        {
            return m_iPaymentType;
        }
        private void SetPaymentType(long paymentType)
        {
            m_iPaymentType = (int)paymentType;
        }

        private long GetArticleDef()
        {
            return m_lArticleDef;
        }
        public void SetArticleDef(long lArticleDef)
        {
            m_lArticleDef = lArticleDef;
        }

        private long GetUnitId()
        {
            return m_lUnitId;
        }
        private void SetUnitId(long unitId)
        {
            m_lUnitId = unitId;
        }

        public long GetGroupId()
        {
            return m_lGroupId;
        }
        public void SetGroupId(long groupId)
        {
            m_lGroupId = groupId;
        }

        private COPSDate GetLimitDateTime()
        {
            return m_dtLimit;
        }
        public void SetLimitDateTime(COPSDate date)
        {
            m_dtLimit = date.Copy();
        }

        private COPSDate GetDateTime()
        {
            return m_dtOper;
        }
        public void SetDateTime(COPSDate operationDate)
        {
            m_dtOper = operationDate.Copy();
        }

        private long GetOperType()
        {
            return m_lOperType;
        }
        public void SetOperType(long lOperType)
        {
            m_lOperType = lOperType;
        }

        private long GetMaxMinutes()
        {
            return m_lMaxMinutes;
        }
        private void SetMaxMinutes(long maxMinutes)
        {
            m_lMaxMinutes = maxMinutes;
        }

        private COPSDate GetFinalDateTime()
        {
            return m_dtOperFinal;
        }
        public void SetFinalDateTime(COPSDate date)
        {
            m_dtOperFinal = date.Copy();
        }

        private long GetTypeOfGroup()
        {
            return m_lTypeOfGroup;
        }

        private long GetCurrentTariff()
        {
            return m_lTariff;
        }
        private void SetCurrentTariff(long lTariff)
        {
            m_lTariff = lTariff;
        }

        private COPSDate GetOperDateRealIniDateTime()
        {
            return m_dtOperDateRealIni;
        }
        public void SetOperDateRealIniDateTime(COPSDate date)
        {
            m_dtOperDateRealIni = date.Copy();
        }

        private COPSDate GetRoundEndOfDayDateTime()
        {
            return m_dtRoundEndOfDay;
        }
        public void SetRoundEndOfDayDateTime(COPSDate date)
        {
            m_dtRoundEndOfDay = date.Copy();
        }

        private COPSDate GetLastAllowedAmountEndDate()
        {
            return m_dtLastAllowedAmountEndDate;
        }
        public void SetLastAllowedAmountEndDate(COPSDate date)
        {
            m_dtLastAllowedAmountEndDate = date.Copy();
        }


        private COPSDate GetFirstAllowedAmountEndDate()
        {
            return m_dtFirstAllowedAmountEndDate;
        }
        public void SetFirstAllowedAmountEndDate(COPSDate date)
        {
            m_dtFirstAllowedAmountEndDate = date.Copy();
        }


        private long GetRoundMoney(long money)
        {
            trace.Write(TraceLevel.Debug, "GetRoundMoney");
            long lReturnMoney = 0;

            try
            {
                if (GetMinCoinValue() != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    long mod = money % GetMinCoinValue();
                    long div = money / GetMinCoinValue();

                    if (mod <= (GetMinCoinValue() / 2))
                    {
                        lReturnMoney = div * GetMinCoinValue();
                    }
                    else
                    {
                        lReturnMoney = (div + 1) * GetMinCoinValue();
                    }
                }
                else
                    lReturnMoney = money;
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return lReturnMoney;

        }



        public M1ComputeEx0(ILoggerManager loggerManager, int type) : base(type) {
            m_pDBB = null;
            m_bApplyVehicleHistory = true;
            m_bNotPrunedTreeInit = false;
            m_pm1ComputeExAux = null;

            trace = loggerManager.CreateTracer(GetType());
            m_Tree = new CM1GroupsTree(loggerManager);
            m_NotPrunedTree = new CM1GroupsTree(loggerManager);

            //m_CalculateSteps_Steps = null;
            ResetOper();
        }
        private void ResetOper()
        {
            m_strVehicleId?.Empty();
            m_lGroupId = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lTypeOfGroup = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lUnitId = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lOperType = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lMaxMoney = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lMinMoney = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lMaxMinutes = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lMaxTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lMinTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lArticle = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lArticleDef = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lOperMoney = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lTariff = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lTimeLimit = GlobalDefs.DEF_UNDEFINED_VALUE;

            m_lCurrMoney = 0;
            m_lCurrMinutes = 0;
            m_lTariffMoney = 0;
            m_lAccMoney = 0;
            m_lAccMinutes = 0;
            m_lAccMoneyGroup = 0;
            m_lAccMinutesGroup = 0;
            m_stInitialIntervalOffset.iTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_stInitialIntervalOffset.iMoney = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_stIntervalOffset.iTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_stIntervalOffset.iMoney = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_bTimeReset = false;
            m_bIntReset = false;
            m_dtDayChange = false;
            m_lBlocksWithTariff = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lRealAccMoney = 0;
            m_lRealAccMinutes = 0;
            m_lRealCurrMoney = 0;
            m_lRealCurrMinutes = 0;
            m_iWholeOperationWithChipCard = 0;
            m_iWholeOperationWithMobile = 0;
            m_bTimeLimitsCompute = false;
            m_bHistOnlyWithSamePaymentType = false;

            m_lResult = M1_OP_OK;
            m_dtOper?.SetStatus(COPSDateStatus.Invalid);
            m_dtOper?.SetStatus(COPSDateStatus.Invalid);
            m_dtOperInitial?.SetStatus(COPSDateStatus.Invalid);
            m_dtOperFinal?.SetStatus(COPSDateStatus.Invalid);
            m_dtLimit?.SetStatus(COPSDateStatus.Invalid);
            m_dtFirst?.SetStatus(COPSDateStatus.Invalid);
            m_dtOperEfectiveInit?.SetStatus(COPSDateStatus.Invalid);
            m_dtOperDateRealIni?.SetStatus(COPSDateStatus.Invalid);
            m_dtOperTariffInitial?.SetStatus(COPSDateStatus.Invalid);
            GetTree().Reset();
            m_bPostPay = false;
            m_bHistOpCourtesy = false;
            m_bHistOpPostPay = false;
            m_lPostPayMinutes = 0;
            m_lFirstGroupId = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_bOperMoneySet = false;
            m_bOperMaxMoneySet = false;
            m_lPreHistMaxMoney = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lPreHistMaxMinutes = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_bHistPostPay = false;
            m_bIgnoreResets = false;
            m_bConsiderCloseFirstInterval = false;
            m_iRemainingMinutesWithZeroValue = 0;
            m_bIntraZoneParkException = false;
            m_bRoundEndOfDay = false;
            m_dtRoundEndOfDay?.SetStatus(COPSDateStatus.Null);
            m_lRoundEndOfDayMinutes = 0;
            m_lRoundEndOfDayRealMinutes = 0;
            m_bCurrentAmountIsAllowed = true;
            m_dtLastAllowedAmountEndDate?.SetStatus(COPSDateStatus.Null);
            m_lLastAllowedAmountMinutes = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lLastAllowedAmountRealMinutes = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lLastAllowedAmountMoney = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lLastAllowedAmountRealMoney = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_dtFirstAllowedAmountEndDate?.SetStatus(COPSDateStatus.Null);
            m_lFirstAllowedAmountRealMinutes = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lFirstAllowedAmountMoney = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lFirstAllowedAmountRealMoney = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lInitialTimeLastInterval = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lEndTimeLastInterval = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lCurrentTimeLastInterval = GlobalDefs.DEF_UNDEFINED_VALUE;


            m_lInAddFreeMinutesQuantity = 0;
            m_lInAddFreeMoneyQuantity = 0;

            m_lOutAddFreeMinutesQuantity = 0;
            m_lOutAddFreeMoneyQuantity = 0;
            m_bHistFreeMoneyUsed = false;

            m_bCalculateTimeSteps = false;
            m_bCalculateQuantitySteps = false;
            m_lCalculateSteps_StepValue = STEP_CALCULATION_DEFAULT_VALUE;
            m_lCalculateSteps_CurrentValue = GlobalDefs.DEF_UNDEFINED_VALUE;
        }
        public override CM1GroupsTree GetTree()
        {
            return m_Tree;
        }
        public override bool GetM1(CDatM1 pM1, bool bM1Plus, int nMaxMoney = 0, bool bApplyHistory = true)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::GetM1");
            bool fnResult = true;

            try
            {
                if (m_pDBB.TariffsHaveChanged()) {
                    m_bNotPrunedTreeInit = false;
                }

                ResetOper();

                if (!SetInput(pM1, bM1Plus, nMaxMoney))
                {
                    throw new InvalidOperationException("Could not set input data");
                }


                if (m_lResult == M1_OP_OK)
                {
                    if (m_strVehicleId.IsEmpty())
                    {
                        bApplyHistory = false;
                    }

                    //Compute tariff
                    if (!Compute(bM1Plus, bApplyHistory))
                    {
                        throw new InvalidOperationException("Could not compute tariff");
                    }
                }

                if (!SetOutput(pM1, bM1Plus))
                {
                    throw new InvalidOperationException("Could not set output data");
                }


            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public override void SetTracerEnabled(bool enabled)
        {
            base.SetTracerEnabled(enabled);
            m_Tree.SetTracerEnabled(enabled);
            m_NotPrunedTree.SetTracerEnabled(enabled);

        }

        private bool SetInput(CDatM1 pM1, bool bM1Plus, int nMaxMoney)
        {
            trace.Write(TraceLevel.Debug, "CM1ComputeEx0::SetInput");
            bool fnResult = true;

            try
            {
                SetUnitId(pM1.GetInUnit());
                SetOperType(pM1.GetInOperType());
                SetArticleDef(pM1.GetInArticleDef());
                SetDateTime(pM1.GetInDate());
                SetGroupId(pM1.GetInGroup());
                m_strVehicleId = pM1.GetInVehicleID();

                COPSPlate strPlate = GetVehicleId();
                if (strPlate != null && !strPlate.IsEmpty())
                {
                    SetVehicleId(strPlate);
                    ApplyVehicleHistory(true);
                }
                else
                {
                    ApplyVehicleHistory(false);
                }
                SetPaymentType(pM1.GetInPaymentType());

                if (pM1.GetInIntervalOffsetMinutes() != GlobalDefs.DEF_UNDEFINED_VALUE)
                    m_stIntervalOffset.iTime = (int)pM1.GetInIntervalOffsetMinutes();

                if (pM1.GetInIntervalOffsetMoney() != GlobalDefs.DEF_UNDEFINED_VALUE)
                    m_stIntervalOffset.iMoney = (int)pM1.GetInIntervalOffsetMoney();

                switch (m_lOperType)
                {
                    case OperationDat.DEF_OPERTYPE_RETN:
                        SetLimitDateTime(pM1.GetInDate());
                        // TIME OF THE FIRST OPERATION (IN THE CARD HISTORY)
                        SetInitialDateTime(pM1.GetOutOperDateIni0());
                        SetOperEfectiveInitDateTime(pM1.GetOutOperDateIni0());
                        // TIME OF THE OPERATION END
                        // Check if we want to do a refund at a time greater than the parking end
                        // We don't apply history because the operation could continue if the operation
                        // date is in the courtesy time
                        if (pM1.GetOutOperDateEnd() <= pM1.GetInDate())
                        {
                            m_lResult = M1_OP_NOK;
                        }
                        // If a return operation has to be computed, the history is in the card  itself
                        m_bApplyVehicleHistory = false;
                        // MONEY
                        if (pM1.GetOutAccumulateMoney() != GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            m_lOperMoney = pM1.GetOutAccumulateMoney();
                        }
                        break;
                    case OperationDat.DEF_OPERTYPE_AMP:
                    case OperationDat.DEF_OPERTYPE_PARK:
                        if (pM1.GetInMaxDate().IsValid())
                        {
                            SetLimitDateTime(pM1.GetInMaxDate());
                        }

                        // ACCUMULATED MONEY
                        m_lAccMoney = pM1.GetOutAccumulateMoney();
                        if (m_lAccMoney == GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            m_lAccMoney = 0;
                        }
                        // ACCUMULATED TIME
                        m_lAccMinutes = pM1.GetOutAccumulateTime();
                        if (m_lAccMinutes == GlobalDefs.DEF_UNDEFINED_VALUE)
                            m_lAccMinutes = 0;
                        // ACCUMULATED MONEY
                        m_lAccMoneyGroup = pM1.GetOutAccumulateMoneyAllGroup();
                        if (m_lAccMoneyGroup == GlobalDefs.DEF_UNDEFINED_VALUE)
                            m_lAccMoneyGroup = 0;
                        // ACCUMULATED TIME
                        m_lAccMinutesGroup = pM1.GetOutAccumulateTimeAllGroup();
                        if (m_lAccMinutesGroup == GlobalDefs.DEF_UNDEFINED_VALUE)
                            m_lAccMinutesGroup = 0;

                        // REMOTE PARKING DATE
                        if (bM1Plus && pM1.GetOutOperDateIni().IsValid())
                        {
                            SetInitialDateTime(pM1.GetOutOperDateIni());
                        }

                        SetMoney(nMaxMoney);

                        if (pM1.GetOutMaxImport() != GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            SetMaxMoney(pM1.GetOutMaxImport());
                            m_bOperMaxMoneySet = true;
                        }

                        if (pM1.GetOutMinImport() != GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            SetMinMoney(pM1.GetOutMinImport());
                        }

                        m_lRealAccMoney = 0;
                        m_lRealAccMinutes = 0;
                        SetOperDateRealIniDateTime(GetDateTime());

                        if (m_iPaymentType != OperationDat.DEF_OPERPAY_CHIPCARD)
                            m_iWholeOperationWithChipCard = 0;
                        else
                            m_iWholeOperationWithChipCard = 1;

                        if (m_iPaymentType != OperationDat.DEF_OPERPAY_MOBILE)
                            m_iWholeOperationWithMobile = 0;
                        else
                            m_iWholeOperationWithMobile = 1;

                        m_bTimeLimitsCompute = pM1.GetInComputeTimeLimits();

                        m_bHistOnlyWithSamePaymentType = pM1.GetInHistOnlyWithSamePaymentType();

                        if (pM1.GetInAddFreeMinutesQuantity() > 0)
                        {
                            m_lInAddFreeMinutesQuantity = pM1.GetInAddFreeMinutesQuantity();
                        }

                        if (pM1.GetInAddFreeMoneyQuantity() > 0)
                        {
                            m_lInAddFreeMoneyQuantity = pM1.GetInAddFreeMoneyQuantity();
                        }

                        m_lTimeLimit = pM1.GetInMaxTime();

                        if (!bM1Plus)
                        {
                            m_bCalculateTimeSteps = pM1.GetInCalculateTimeSteps();
                            m_bCalculateQuantitySteps = pM1.GetInCalculateQuantitySteps();
                            m_lCalculateSteps_StepValue = pM1.GetInCalculateSteps_StepValue();

                            if (((m_bCalculateTimeSteps) || (m_bCalculateQuantitySteps)) && (m_CalculateSteps_Steps == null))
                            {
                                m_CalculateSteps_Steps = new List<stStepCalculation>();
                            }
                        }

                        break;
                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        private bool Compute(bool bM1Plus, bool bApplyHistory)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::Compute");
            bool fnResult = true;

            bool bChangeTariff = false;
            long lMinutes = 0;
            long lMoney = 0;

            try
            {
                m_lResult = M1_OP_OK;

                if (!CheckInput())// As we will return false, it makes no sense to put an error
                    throw new InvalidOperationException("Invalid input parameters");

                if (m_lResult == M1_OP_OK) {

                    if (m_pm1ComputeExAux == null)
                    {
                        try
                        {
                            m_pm1ComputeExAux = new M1ComputeEx0(trace.Creator, this.Type);
                        }
                        catch (Exception)
                        {
                            throw new InvalidOperationException("m_pm1ComputeExAux is NULL!!!");
                        }
                    }

                    if (bM1Plus) {
                        trace.Write(TraceLevel.Info, "Applicating M1 Plus");
                    }

                    do
                    {
                        if (!BuildGroupsTree())
                            throw new InvalidOperationException("Error building groups tree");

                        if (m_lResult == M1_OP_OK)
                        {
                            if (!GetTree().GetBranchMaxAvailableMinutes(m_lGroupId, ref lMinutes))
                            {
                                throw new InvalidOperationException("Could not obtain branch available minutes");
                            }

                            m_lResult = m_pDBB.HourDifference(m_lResult);

                            if (bChangeTariff)
                            {
                                lMinutes = lMinutes + m_lRealCurrMinutes;
                                m_lMaxMinutes = lMinutes;
                            }

                            m_lPreHistMaxMinutes = lMinutes;
                        }

                        if (m_lResult == M1_OP_OK)
                        {
                            if (!GetTree().GetBranchMaxAvailableMoney(m_lGroupId, ref lMoney))
                            {
                                throw new InvalidOperationException("Could not obtain branch available money");
                            }

                            if (bChangeTariff)
                            {
                                lMoney = lMoney + m_lRealCurrMoney;
                                m_lMaxMoney = lMoney;
                            }


                            m_lPreHistMaxMoney = lMoney;
                        }

                        if ((m_lResult == M1_OP_OK) && (bApplyHistory))
                        {
                            // When bM1Plus is true, means that a remote M1 has succeeded and the history
                            // of the vehicle was computed remotely, so it has no sense to compute it here.
                            // Also in the remote computation returned the accumulated minutes and money
                            // for that vehicle
                            if ((!bM1Plus) || (GeneralParams.UnitId == GlobalDefs.CC_UNIT_ID.ToString()))
                            {
                                // Apply history of the vehicle, if vehicle plate was entered
                                // and the type of article needds it
                                if (m_bApplyVehicleHistory)
                                {
                                    // Modify the tree (constraints and last-date with operations)
                                    if (!ApplyHistory())
                                    {
                                        throw new InvalidOperationException("Error applying history of vehicle");
                                    }
                                }
                            }
                            else
                            {
                                if (!ApplyRemoteHistory(GetTree()))
                                {
                                    throw new InvalidOperationException("Error applying remote history");
                                }
                            }
                        }

                        // Get the minutes and money left in the group. The money and minutes left are
                        // the maximum money and minutes that a user can park when there are no operations
                        // affecting the current operation (this is, the history doesn't apply to the current
                        // operation), minus the money and minutes spent by past operations
                        if (m_lResult == M1_OP_OK)
                        {
                            if (!GetTree().GetBranchMaxAvailableMinutes(GetGroupId(), ref lMinutes))
                                throw new InvalidOperationException("Could not obtain branch available minutes");

                            m_lResult = m_pDBB.HourDifference(m_lResult);

                            if (bChangeTariff)
                            {
                                if (lMinutes >= 0)
                                {
                                    lMinutes = lMinutes + m_lRealCurrMinutes;
                                }
                            }

                            if (lMinutes <= 0)
                            {
                                // This means that the accumulated time for this vehicle in this group is greater or
                                // equal to the maximum allowed => NO PARKING ALLOWED
                                trace.Write(TraceLevel.Info, "No time left for parking");
                                m_lResult = M1_OP_TPERM;
                            }
                            else
                            {
                                m_lMaxMinutes = lMinutes;
                            }
                        }

                        if (m_lResult == M1_OP_OK)
                        {
                            if (!GetTree().GetBranchMaxAvailableMoney(GetGroupId(), ref lMoney))
                            {
                                throw new InvalidOperationException("Could not obtain branch available minutes");
                            }

                            if (bChangeTariff)
                            {
                                lMoney = lMoney + m_lRealCurrMoney;
                                SetMaxMoney(lMoney);
                            }


                            if (lMoney <= 0)
                            {
                                // This means that the accumulated money for this vehicle in this group is greater or
                                // equal to the maximum allowed => NO PARKING ALLOWED
                                trace.Write(TraceLevel.Info, "No money left for parking");
                                //						m_lResult = M1_OP_NOK;	// TODO: CREATE A SPECIFIC RESULT
                            }

                            if ((!bM1Plus) && (GetMaxMoney() == GlobalDefs.DEF_UNDEFINED_VALUE))
                            {
                                SetMaxMoney(lMoney);
                            }
                            else if ((bM1Plus) && (GetMaxMoney() == GlobalDefs.DEF_UNDEFINED_VALUE) && (GeneralParams.UnitId == GlobalDefs.CC_UNIT_ID.ToString()))
                            {
                                SetMaxMoney(lMoney);
                            }
                        }

                        if (m_lResult == M1_OP_OK)
                        {
                            // Get the money introduced by user (if any). If the user has not introduced any
                            // money, but a M1 is called, the maximum time and money that a user can
                            // spend is computed. To do so, the maximum money left for the group is used as
                            // the operations money
                            lMoney = GetMoney();

                            if (!bM1Plus)
                            {
                                if (lMoney == 0 || (lMoney == GlobalDefs.DEF_UNDEFINED_VALUE) || (bChangeTariff))
                                {
                                    SetMoney(GetMaxMoney());
                                }
                                else
                                {
                                    m_bOperMoneySet = true;
                                }
                            }
                            else
                            {

                                if (lMoney == GlobalDefs.DEF_UNDEFINED_VALUE)
                                {
                                    SetMoney(GetMaxMoney());
                                }
                                else
                                {

                                    if (GeneralParams.UnitId == GlobalDefs.CC_UNIT_ID.ToString())
                                    {
                                        SetMoney(lMoney);
                                        SetMaxMoney(lMoney);
                                    }
                                    m_bOperMoneySet = true;
                                }
                            }

                            // Check if the money of the current operation is greater than the minimun
                            // money to compute the operation
                            if (!GetTree().GetBranchMinMoney(GetGroupId(), ref lMoney))
                            {
                                throw new InvalidOperationException("Could not obtain branch minimum money");
                            }

                            if (lMoney > GetMoney())
                            {
                                //						m_lResult = M1_OP_NOK;	//TODO: CREATE A SPECIFIC RESULT
                            }

                            if (!bM1Plus)
                            {
                                if (GetOperType() == OperationDat.DEF_OPERTYPE_AMP)
                                {
                                    SetMinMoney(GetMinCoinValue());
                                }
                                else if (GetOperType() == OperationDat.DEF_OPERTYPE_PARK)
                                {

                                    if (!bChangeTariff)
                                    {
                                        SetMinMoney(lMoney);
                                    }
                                }
                                else if (GetOperType() == OperationDat.DEF_OPERTYPE_RETN)
                                {
                                    if (!bChangeTariff)
                                    {
                                        long lMinMoney = 0;
                                        GetTree().GetMinValueToChargeInRefund(GetGroupId(), ref lMinMoney);
                                        SetMinMoney(lMinMoney);
                                    }
                                }
                            }
                            else
                            {
                                if (GeneralParams.UnitId == GlobalDefs.CC_UNIT_ID.ToString())
                                {
                                    SetMinMoney(lMoney);
                                }
                            }

                        }

                        bChangeTariff = false;

                        if (m_lResult == M1_OP_OK)
                        {
                            if (GetOperTariffInitialDateTime().GetStatus() == COPSDateStatus.Invalid)
                            {
                                if (GetInitialDateTime().GetStatus() != COPSDateStatus.Invalid)
                                {
                                    SetOperTariffInitialDateTime(GetInitialDateTime().Copy());
                                }
                            }

                            switch (GetOperType())
                            {
                                case OperationDat.DEF_OPERTYPE_AMP:
                                case OperationDat.DEF_OPERTYPE_PARK:
                                    if (!ComputeTariff(ref bChangeTariff, bM1Plus))
                                        throw new InvalidOperationException("Error computing parking");
                                    break;
                                case OperationDat.DEF_OPERTYPE_RETN:
                                    if (!ComputeReturn(ref bChangeTariff))
                                    {
                                        throw new InvalidOperationException("Error computing return");
                                    }
                                    break;
                            }

                            if (bChangeTariff)
                            {
                                COPSDate dtWork;
                                // TODO: WE MUST INSERT THE OPERATION ???
                                dtWork = GetFinalDateTime().Copy();
                                SetOperTariffInitialDateTime(dtWork.Copy());
                            }
                            else
                            {
                                if ((!bM1Plus) && (GetOperType() == OperationDat.DEF_OPERTYPE_AMP) && (m_iRemainingMinutesWithZeroValue > 0))
                                {
                                    if (m_lRealCurrMinutes == m_iRemainingMinutesWithZeroValue)
                                    {
                                        SetMinMoney(0);
                                    }
                                }
                            }
                        }

                    } while (bChangeTariff);

                    if ((!bM1Plus) && (m_lResult == M1_OP_OK) && (GeneralParams.UnitId == GlobalDefs.CC_UNIT_ID.ToString()))
                    {

                        COPSDate dtOper = GetInitialDateTime().Copy();

                        COPSDate dtMaxDate = new COPSDate();
                        dtMaxDate.SetStatus(COPSDateStatus.Invalid);
                        dtMaxDate = GetFinalDateTime().Copy();

                        COPSDate dtResult = new COPSDate();
                        dtResult.SetStatus(COPSDateStatus.Invalid);

                        if (m_lInAddFreeMoneyQuantity > 0)
                        {

                            if (!AddMoneyToDate(m_bApplyVehicleHistory, dtOper, dtMaxDate, m_lInAddFreeMoneyQuantity, ref dtResult, ref m_lOutAddFreeMinutesQuantity))
                            {
                                m_lResult = M1_OP_NOK;
                            }

                        }

                        if (((m_lInAddFreeMinutesQuantity + m_lOutAddFreeMinutesQuantity) > 0) && (m_lResult == M1_OP_OK))
                        {
                            dtResult.SetStatus(COPSDateStatus.Invalid);

                            if (!AddMinutesToDate(m_bApplyVehicleHistory, ref dtOper, ref dtMaxDate, m_lInAddFreeMinutesQuantity + m_lOutAddFreeMinutesQuantity, ref dtResult, ref m_lOutAddFreeMoneyQuantity, ref m_lOutAddFreeMinutesQuantity))
                            {
                                m_lResult = M1_OP_NOK;
                                m_lOutAddFreeMoneyQuantity = 0;
                                m_lOutAddFreeMinutesQuantity = 0;

                            }
                        }
                    }

                    if (m_lResult == M1_OP_OK)
                    {
                        if (m_bTimeLimitsCompute)
                        {
                            COPSDate dtOper;
                            long lMinMoney;
                            if (m_bPostPay)
                            {
                                if (GetPaymentType() != OperationDat.DEF_OPERPAY_MOBILE)
                                {
                                    lMinMoney = GetRoundMoney(m_lFirstAllowedAmountRealMoney);
                                }
                                else
                                {
                                    lMinMoney = m_lFirstAllowedAmountRealMoney;
                                }
                            }
                            else
                            {
                                lMinMoney = m_lFirstAllowedAmountRealMoney;
                            }

                            dtOper = GetInitialDateTime().Copy();

                            COPSDate dtMaxDate = new COPSDate();
                            dtMaxDate.SetStatus(COPSDateStatus.Invalid);
                            dtMaxDate = GetLimitDateTime().Copy();

                            if (!AddMoneyToDate(bApplyHistory, dtOper, dtMaxDate, lMinMoney, ref m_dtFirstAllowedAmountEndDate, ref m_lFirstAllowedAmountRealMinutes))
                            {
                                throw new InvalidOperationException("Error computing bottom time limit");
                            }
                        }

                    }
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool AddMinutesToDate(bool bApplyHistory, ref COPSDate dtDate, ref COPSDate dtMaxDate, long lMinutes, ref COPSDate dtResult, ref long lMoney, ref long plMinutes)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::AddMinutesToDate");
            bool fnResult = true;

            COPSPlate strVehicleId;
            CDatM1 pM1 = null;

            try
            {
                m_pm1ComputeExAux = new M1ComputeEx0(trace.Creator, this.Type);
                m_pm1ComputeExAux.SetTracerEnabled(false);
                m_pm1ComputeExAux.SetDBB(m_pDBB);
                m_pm1ComputeExAux.SetOperType(OperationDat.DEF_OPERTYPE_PARK);
                m_pm1ComputeExAux.SetUnitId(GetUnitId());
                m_pm1ComputeExAux.SetArticleDef(GetArticleDef());
                m_pm1ComputeExAux.SetGroupId(GetGroupId());
                m_pm1ComputeExAux.SetMinCoinValue(GetMinCoinValue());

                if (dtMaxDate.IsValid())
                {
                    m_pm1ComputeExAux.SetLimitDateTime(dtMaxDate);
                }

                m_pm1ComputeExAux.SetDateTime(dtDate);
                m_pm1ComputeExAux.SetInitialDateTime(dtDate);
                SetPaymentType(GetPaymentType());
                m_pm1ComputeExAux.m_lTimeLimit = lMinutes;
                m_pm1ComputeExAux.ApplyVehicleHistory(bApplyHistory);
                // VEHICLE PLATE: If it exists we must apply history (except for a return operation)

                strVehicleId = GetVehicleId();
                if (!strVehicleId.IsEmpty())
                {
                    m_pm1ComputeExAux.SetVehicleId(strVehicleId);
                }

                if (!m_pm1ComputeExAux.Compute(false, bApplyHistory))
                {
                    throw new InvalidOperationException("Error computing m1 for current operation");
                }

                if (!m_pm1ComputeExAux.SetOutput(pM1, false))
                {
                    throw new InvalidOperationException("Could not set output data");
                }

                trace.Write(TraceLevel.Info, $"Tariff Efective Time:{m_pm1ComputeExAux.m_lCurrMinutes} | Tariff Efective Money:{m_pm1ComputeExAux.m_lTariffMoney}");

                dtResult = pM1.GetOutOperDateEnd().Copy();
                lMoney = m_pm1ComputeExAux.m_lRealCurrMoney;
                plMinutes = m_pm1ComputeExAux.m_lRealCurrMinutes;
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
        private bool AddMoneyToDate(bool bApplyHistory, COPSDate dtOper, COPSDate dtMaxDate, long lMoney, ref COPSDate dtResult, ref long lMinutes)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::AddMoneyToDate");
            bool fnResult = true;

            COPSPlate strVehicleId;
            CDatM1 pM1 = null;

            try
            {
                m_pm1ComputeExAux = new M1ComputeEx0(trace.Creator, this.Type);
                m_pm1ComputeExAux.SetTracerEnabled(false);
                m_pm1ComputeExAux.SetDBB(m_pDBB);
                m_pm1ComputeExAux.SetOperType(OperationDat.DEF_OPERTYPE_PARK);
                m_pm1ComputeExAux.SetUnitId(GetUnitId());
                m_pm1ComputeExAux.SetArticleDef(GetArticleDef());
                m_pm1ComputeExAux.SetGroupId(GetGroupId());
                m_pm1ComputeExAux.SetMinCoinValue(GetMinCoinValue());

                if (dtMaxDate.IsValid())
                {
                    m_pm1ComputeExAux.SetLimitDateTime(dtMaxDate);
                }

                m_pm1ComputeExAux.SetDateTime(dtOper);
                m_pm1ComputeExAux.SetInitialDateTime(dtOper);
                SetPaymentType(GetPaymentType());
                m_pm1ComputeExAux.SetMaxMoney(lMoney);
                m_pm1ComputeExAux.ApplyVehicleHistory(bApplyHistory);
                // VEHICLE PLATE: If it exists we must apply history (except for a return operation)

                strVehicleId = GetVehicleId();
                if (!strVehicleId.IsEmpty())
                {
                    m_pm1ComputeExAux.SetVehicleId(strVehicleId);
                }

                if (!m_pm1ComputeExAux.Compute(false, bApplyHistory))
                {
                    throw new InvalidOperationException("Error computing m1 for current operation");
                }


                pM1 = new CDatM1(trace.Creator);

                if (!m_pm1ComputeExAux.SetOutput(pM1, false))
                {
                    throw new InvalidOperationException("Could not set output data");
                }

                trace.Write(TraceLevel.Info, $"Tariff Efective Time: {m_pm1ComputeExAux.m_lCurrMinutes} | Tariff Efective Money:{m_pm1ComputeExAux.m_lTariffMoney}");

                dtResult = pM1.GetOutOperDateEnd().Copy();
                lMinutes = m_pm1ComputeExAux.m_lRealCurrMinutes;
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public void ApplyVehicleHistory(bool bApplyHistory)
        {
            m_bApplyVehicleHistory = bApplyHistory;
        }

        public override bool GetApplyVehicleHistory() { return m_bApplyVehicleHistory; }

        public override long GetRealCurrMinutes() { return m_lRealCurrMinutes; }

        private bool ComputeReturn(ref bool bChangeTariff)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::ComputeReturn");
            bool fnResult = true;

            COPSDate dtOper;
            COPSDate dtInitial;
            int iDaysChanged = 0;
            bool bDayChange = false;
            bool bStop = false;
            stINTERVAL stIntervalOffset = new stINTERVAL();
            long lOperMoney;
            COPSDate dtLast = new COPSDate();        // Date for the last used date in the iteration
            long lOperTime = 0;
            long lLimitTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            bool bFirstBlock = true;

            try
            {
                if (m_pDBB == null)
                {
                    throw new InvalidOperationException("Invalid DB handler");
                }

                dtOper = GetDateTime().Copy();
                trace.Write(TraceLevel.Info, $"Date of operation: {dtOper.fstrGetTraceString()}");
                dtInitial = GetInitialDateTime().Copy();
                trace.Write(TraceLevel.Info, $"Initial date of parking: {dtInitial.fstrGetTraceString()}");

                dtOper = GetOperTariffInitialDateTime().Copy();
                dtLast.SetStatus(COPSDateStatus.Null);
                if (GetLimitDateTime().IsValid() && GetLimitDateTime() < dtOper)
                {
                    SetLimitDateTime(dtOper);
                }

                lOperMoney = GetMoney();
                stIntervalOffset.iMoney = 0;
                stIntervalOffset.iTime = 0;

                if (m_stIntervalOffset.iTime != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    stIntervalOffset.iTime = m_stIntervalOffset.iTime;
                }

                if (m_stIntervalOffset.iMoney != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    stIntervalOffset.iMoney = m_stIntervalOffset.iMoney;
                }

                while (dtOper.IsValid() && iDaysChanged < OPSPDMDatabase.C_NUMERO_MAX_CAMBIO_DIA && !bStop && !(bChangeTariff))
                {
                    // If the day of operation is the day of limit, then we have to set the limit minutes
                    if (dtOper.Value.Year == GetLimitDateTime().Value.Year && dtOper.Value.Day == GetLimitDateTime().Value.Day)
                    {
                        lLimitTime = GetLimitDateTime().TimeToMinutes();
                    }

                    if (!IterateDay(ref dtOper,ref dtLast, ref bStop,ref stIntervalOffset,ref lOperMoney, ref lOperTime, lLimitTime, ref bDayChange,ref bChangeTariff, ref bFirstBlock))
                    {
                        throw new InvalidOperationException("Error iterating inside day");
                    }

                    if (bDayChange)
                    {
                        trace.Write(TraceLevel.Info, "DAY MUST BE INCREMENTED");

                        if (iDaysChanged < OPSPDMDatabase.C_NUMERO_MAX_CAMBIO_DIA)
                        {
                            // When we change the day, we must set also the hour to the start of the next 
                            // day (ie. 00:00:00 ) and set the last used date
                            dtLast = dtOper.Copy();
                            dtOper.SetDateTime(dtOper.Value.Year, dtOper.Value.Month, dtOper.Value.Day, 0, 0, 0);
                            dtOper.AddTimeSpan(new TimeSpan(1, 0, 0, 0));
                            iDaysChanged++;
                        }
                        else
                        {
                            trace.Write(TraceLevel.Info, "Maximum days changed");
                        }
                    }
                    else if (!(bChangeTariff))
                    {
                        dtOper.SetDateTime(dtOper.Value.Year, dtOper.Value.Month, dtOper.Value.Day, (int)(lOperTime / 60), (int)(lOperTime % 60), 0);
                    }

                    if (dtOper.IsValid())
                    {
                        SetFinalDateTime(dtOper);
                    }

                    trace.Write(TraceLevel.Info, $"New operation date: {GetFinalDateTime().fstrGetTraceString()}");
                }

                if (dtOper.IsValid())
                {
                    SetFinalDateTime(dtOper);
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        private bool ComputeTariff(ref bool bChangeTariff, bool bM1Plus)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::ComputeTariff");
            bool fnResult = true;

            COPSDate dtOper = new COPSDate();
            int iDaysChanged = 0;
            bool bDayChange = false;
            bool bStop = false;
            stINTERVAL stIntervalOffset;
            long lOperMoney;
            long lOperMoneyRest = 0;
            COPSDate dtLast = new COPSDate();        // Date for the last used date in the iteration
            long lOperTime = 0;
            long lLimitTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            bool bFirstBlock = true;

            try
            {
                Guard.IsNull(m_pDBB, nameof(m_pDBB), "Invalid DB handler");

                if (m_lResult == M1_OP_OK)
                {
                    if (m_bPostPay)
                    {
                        SetMaxMinutes(m_lPostPayMinutes);
                    }

                    trace.Write(TraceLevel.Info, $"Operation money: {GetMoney()}, Branch max. money: {GetMaxMoney()}, Branch max. minutes: {GetMaxMinutes()}");

                    // If we have history for this vehicle, we have an initial offset for the tariff that
                    // corresponds to the accumulated time in the group
                    if (!GetTree().SetInitialIntervalOffset(GetGroupId(), ref m_stInitialIntervalOffset))
                    {
                        throw new InvalidOperationException("Error initiating interval offset");
                    }

                    //dtOper = m_dtOperTariffInitial.IsValid() ? m_dtOperTariffInitial : GetDateTime();

                    if (GetOperTariffInitialDateTime().IsValid())
                    {
                        dtOper = GetOperTariffInitialDateTime().Copy();
                    } else {
                        dtOper = GetDateTime().Copy();
                    }
                    
                    trace.Write(TraceLevel.Info, $"Setting operation date to: {dtOper.fstrGetTraceString()}");

                    dtLast.SetStatus(COPSDateStatus.Null);

                    lOperMoney = GetMoney();
                    lOperMoneyRest = lOperMoney;

                    stIntervalOffset = m_stInitialIntervalOffset;

                    if (m_stIntervalOffset.iTime != GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        stIntervalOffset.iTime += m_stIntervalOffset.iTime;
                    }

                    if (m_stIntervalOffset.iMoney != GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        stIntervalOffset.iMoney += m_stIntervalOffset.iMoney;
                    }

                    // Iterate through days, blocks and intervals
                    while (dtOper.IsValid() && iDaysChanged < OPSPDMDatabase.C_NUMERO_MAX_CAMBIO_DIA && !bStop && !bChangeTariff)
                    {

                        // If the day of operation is the day of limit, then we have to set the limit minutes
                        if (dtOper.Value.Year == GetLimitDateTime().Value.Year &&
                            dtOper.Value.Day == GetLimitDateTime().Value.Day)
                        {
                            lLimitTime = GetLimitDateTime().TimeToMinutes();
                        }

                        if (!IterateDay(ref dtOper,ref dtLast,ref bStop,ref stIntervalOffset,ref lOperMoneyRest,ref lOperTime, lLimitTime,ref  bDayChange,ref bChangeTariff, ref bFirstBlock))
                        {
                            throw new InvalidOperationException("Error iterating inside day");
                        }

                        if (bDayChange)
                        {
                            trace.Write(TraceLevel.Info, "DAY MUST BE INCREMENTED");

                            if (iDaysChanged < OPSPDMDatabase.C_NUMERO_MAX_CAMBIO_DIA)
                            {
                                // When we change the day, we must set also the hour to the start of the next 
                                // day (ie. 00:00:00 ) and set the last used date
                                dtLast = dtOper.Copy();
                                dtOper.SetDateTime(dtOper.Value.Year, dtOper.Value.Month, dtOper.Value.Day ,0, 0, 0);

                                dtOper.AddTimeSpan(new TimeSpan(1,0,0,0));
                                iDaysChanged++;
                            }
                            else
                            {
                                trace.Write(TraceLevel.Info, "Maximum days changed");
                            }
                        }
                        else if (!bChangeTariff)
                        {
                            dtOper.SetDateTime(dtOper.Value.Year, dtOper.Value.Month, dtOper.Value.Day, (int)(lOperTime / 60), (int)(lOperTime % 60), 0);
                        }

                        if (dtOper.IsValid())
                        {
                            SetFinalDateTime(dtOper);
                        }
                        
                        trace.Write(TraceLevel.Info, $"New operation date: {GetFinalDateTime().fstrGetTraceString()}");
                    }

                    if ((lOperMoney == lOperMoneyRest) && (m_lRealCurrMinutes == 0) && (lOperMoney != 0) && (lOperMoneyRest != 0) && (!(bChangeTariff)))
                    {
                        trace.Write(TraceLevel.Info, "No money consumed => No tariff");
                        m_lResult = M1_NO_TARIFF;
                    }
                    else if (dtOper.IsValid())
                    {
                        SetFinalDateTime(dtOper);
                    }

                    if ((m_lRealCurrMinutes == 0) && (!bM1Plus) && ((!bChangeTariff)) && (lOperMoney > 0))
                    {
                        m_lResult = M1_OP_TPERM;
                    }
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool IterateDay(ref COPSDate dtOper,ref COPSDate dtLast,ref bool bStop,ref  stINTERVAL stIntervalOffset,ref  long lOperMoney,ref long lOperTime,long lTimeLimit, 
                                ref bool bDayChange,ref bool bChangeTariff,ref  bool bFirstBlock)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::IterateDay");
            bool fnResult = true;

            long lConstraint = GlobalDefs.DEF_UNDEFINED_VALUE;
            long lTariff = GlobalDefs.DEF_UNDEFINED_VALUE;
            bool bBlockFound = false;
            long lBlockIni = 0;
            long lBlockEnd = 0;
            long lSubTariff = 0;
            stTariff stTariffDesc = new stTariff();
            long lBlockID = GlobalDefs.DEF_UNDEFINED_VALUE;
            List<long> dayTypes = null;
            long lState = GlobalDefs.DEF_PDMOK;
            bool bIntervalsFound = false;

            try
            {
                bDayChange = false;

                if (dtLast.Value.Day != dtOper.Value.Day || dtLast.Value.Month != dtOper.Value.Month || dtLast.Value.Year != dtOper.Value.Year)
                {
                    trace.Write(TraceLevel.Info, $"Looking for types of day for {dtOper.fstrGetTraceString()}");

                    dayTypes = m_pDBB.GetDayTypes(dtOper);
                    if (dayTypes == null) {
                        throw new InvalidOperationException("Error in call to m_pPDMDB->GetDayTypes");
                    }

                    if (dayTypes.Count <= 0)
                    {
                        // If no types of day defined for the current operation date, 
                        // jump to next day and try again
                        trace.Write(TraceLevel.Error, "No types of days defined");
                        bDayChange = true;
                    }
                }

                // If the day has not changed yet (this is, types of day where defined for the current date)
                if (!bDayChange)
                {
                    // Check if the PDM for the current timetable (block) must be expending  tickets or not
                    trace.Write(TraceLevel.Info, $"Looking for machine status on {dtOper.fstrGetTraceString()}");

                    // The tariff and constraints can change at any time. If the tariff changes
                    // we stop iterating with the current tariff, insert the operations, and start
                    // a new operation with the new tariff
                    trace.Write(TraceLevel.Info, $"Looking for tariff on {dtOper.fstrGetTraceString()}");

                    // CONSTRAINTS AND TARIFF
                    if (!GetConstraintsAndTariff(dtOper, ref lConstraint, ref lTariff))
                    {
                        throw new InvalidProgramException("Could not obtain neither tariffs nor constraints");
                    }

                    if (lConstraint == GlobalDefs.DEF_UNDEFINED_VALUE || lTariff == GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        // If we don't find any tariff and constraints we must change the day, if allowed
                        trace.Write(TraceLevel.Error, $"Could not find constraints or tariff for: Type of article: {GetArticleDef()}, Group: {GetGroupId()}, Type of group: {GetTypeOfGroup()}");
                        bDayChange = true;
                    }
                    else if (lTariff != GetCurrentTariff())
                    {
                        // If tariff changed return
                        trace.Write(TraceLevel.Error, $"Tariff changed: Current: {GetCurrentTariff()}, Next: {lTariff}");
                        bChangeTariff = true;
                        bStop = true;
                    }
                    else
                    {
                        // Get the timetable (block) that corresponds to the current date/time
                        trace.Write(TraceLevel.Info, $"Looking for block on {dtOper.fstrGetTraceString()}");

                        bool bConsiderCloseInterval = bFirstBlock && m_bConsiderCloseFirstInterval;

                        //TODo : Why not Tariff and TimeTables outup params??
                        if (!m_pDBB.GetTariffBlock(lTariff, ref dayTypes, ref dtOper, 0, ref lBlockID, ref lBlockIni, ref lBlockEnd, ref lSubTariff, ref bBlockFound,
                                                    ref stTariffDesc, bConsiderCloseInterval))
                        {
                            throw new InvalidOperationException("Error in call to m_pPDMDB->GetTariffBlock");
                        }

                        if (bBlockFound)
                        {
                            if (bFirstBlock)
                            {
                                bFirstBlock = false;
                            }

                            trace.Write(TraceLevel.Info, $"SubTarif: {lSubTariff} from {lBlockIni/60:D2}:{lBlockIni%60:D2} to {lBlockEnd/60:D2}:{lBlockEnd%60:D2} (Block ID: {lBlockID})");

                            if (!GetBranchState(GetTree(), GetGroupId(), lBlockID, ref dayTypes, ref lState)) {
                                throw new InvalidOperationException("Error in call to CM1ComputeEx0::GetBranchState");
                            }
                            

                            if (lState == GlobalDefs.DEF_PDMOK)
                            {
                                lOperTime = dtOper.Value.Hour * 60 + dtOper.Value.Minute;

                                if (!IterateBlockIntervals(ref lOperTime, lSubTariff,ref dtOper, lBlockIni, lBlockEnd,
                                                            ref stTariffDesc,ref lOperMoney,ref stIntervalOffset,
                                                            lTimeLimit, ref bIntervalsFound,ref bDayChange,
                                                            ref bStop))
                                {
                                    throw new InvalidOperationException("Error iterating through intervals");
                                }

                                if (bIntervalsFound)
                                {
                                    COPSDate dtInitial;
                                    dtInitial = GetInitialDateTime().Copy();
                                    if (!dtInitial.IsValid())
                                    {
                                        trace.Write(TraceLevel.Info, $"Setting initial date of park: {dtOper.fstrGetTraceString()}");
                                        SetInitialDateTime(dtOper);
                                    }
                                }

                                // Update current money (money left to continue computing)
                                SetMoney(lOperMoney);
                            }
                            else
                            {
                                trace.Write(TraceLevel.Info, $"Branch state {lState}, changing block");

                                // As the PDM is not in operating mode in this block, just change to
                                // the next block (changing operation time) but without making 
                                // modifications in money left
                                if (!ChangeBlock(ref stTariffDesc, ref stIntervalOffset, ref lOperMoney,ref lOperTime, ref dtOper, lBlockIni, lBlockEnd,ref bDayChange,ref bStop))
                                {
                                    throw new InvalidOperationException("Could not change block");
                                }
                            }//if( lState == DEF_PDMOK )
                        }
                        else
                        {
                            // If we don't find any block we must change the day, if allowed
                            trace.Write(TraceLevel.Info, $"No blocks defined for {dtOper.fstrGetTraceString()}");

                            if (lTimeLimit != GlobalDefs.DEF_UNDEFINED_VALUE)
                            {
                                bStop = true;
                            }

                            bDayChange = true;

                            // TODO: INSTEAD OF DAY CHANGE WE WOULD HAVE TO SEARCH FOR THE NEXT VALID BLOCK 
                            // IN THE SAME DAY
                        }
                    }//if( lConstraint == DEF_UNDEFINED_VALUE || lTariff == DEF_UNDEFINED_VALUE )
                }//if( !*pbDayChanged )
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool IterateBlockIntervals(ref long lOperTime, long lSubTariff,ref COPSDate dtOper, long lBlockIni, long lBlockEnd, 
                                            ref stTariff stTariffDesc,ref long lOperMoney,ref stINTERVAL stIntervalOffset, long lTimeLimit, ref bool bIntervalsFound,ref  bool bDayChange,ref  bool bStop)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::IterateBlockIntervals");
            bool fnResult = true;

            bool bJumpNextBlock = false;
            long lOpMoney = 0;  // Stores the actual money for the interval, this is, adding offset, checking with group maximum, ...
            long lOpMinutes = 0;    // Stores the actual minutes for the interval, this is, adding offset, checking with group maximum, ...
            bool bTimeLimit = false;
            long lMinutesWithZeroValue = 0;

            try
            {
                Guard.IsNull(m_pDBB, nameof(SetDBB), "invalid DB handler");
                List<stINTERVAL> lstIntervals = new List<stINTERVAL>();
                if (!m_pDBB.GetIntervals(lSubTariff, ref lstIntervals)) {
                    throw new InvalidOperationException("Error in call to m_pDBB.GetIntervals");
                }

                //Initialize variables
                bStop = false;
                bDayChange = false;

                if (lstIntervals.Count == 0)
                {
                    trace.Write(TraceLevel.Info, $"No intervals found for subtariff {lSubTariff} ==>> Block change required");
                    if (lTimeLimit != GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        if (((lOperTime) <= lTimeLimit) && (lTimeLimit < lBlockEnd))
                        {
                            bTimeLimit = true;
                            bStop = true;
                        }
                    }

                    if (!bTimeLimit)
                        lOperTime = lBlockEnd;

                    bJumpNextBlock = true;
                    bIntervalsFound = false;
                }
                else {

                    if (m_dtDayChange)
                        m_lBlocksWithTariff++;

                    bIntervalsFound = true;
                    TraceIntervals(lstIntervals);

                    // If the money we have exceeds the maximum quantity of
                    // the group (and branch) we have to interpolat
                    // e to obtain the minutes that correspond
                    // to the money left. 
                    trace.Write(TraceLevel.Info, "Checking max. money of group constraint");
                    if (!CheckMaxMoney(lOperMoney, ref lOpMoney, ref lOpMinutes, ref stIntervalOffset,ref lstIntervals, ref bStop))
                    {
                        throw new InvalidOperationException("Error checking max money");
                    }


                    // If the money we introduced is not over the maximum, then we have to compute how many
                    // minutes will we obtain with the money introduced
                    if (!bStop)
                    {
                        lOpMoney = lOperMoney;

                        trace.Write(TraceLevel.Info, $"Getting minutes for current operation money left");
                        if (!MoneyToTime(lOpMoney, ref lOpMinutes,ref stIntervalOffset,ref lstIntervals, ref lMinutesWithZeroValue, ref m_bCurrentAmountIsAllowed))
                            throw new InvalidOperationException("Could not obtain money offset");
                    }

                    // Although we have money left, and although that money exceeds the maximum in group
                    // and retailed it, we have to ensure that the obtained minutes do not exceed the
                    // maximum for the group (or branch)
                    if (!bStop)
                    {
                        trace.Write(TraceLevel.Info, "Checking max. time of group constraint");
                        if (!CheckMaxTime(ref lOpMinutes, ref lOpMoney,ref stIntervalOffset,ref lstIntervals, ref bStop))
                        {
                            throw new InvalidOperationException("Error checking max time");
                        }
                    }

                    // If we have set a time limit, we have to treat it as if were a block end, but
                    // if reached we have to stop and not jump to next block
                    if (lTimeLimit != GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        if ((lOperTime + lOpMinutes) >= lTimeLimit)
                        {
                            trace.Write(TraceLevel.Info, $"Time limit ({lTimeLimit}) has been reached ({lOperTime + lOpMinutes})");

                            lOpMinutes = lTimeLimit - lOperTime;
                            if (lOpMinutes < 0)
                            {
                                lOpMinutes = 0;
                            }

                            if (!TimeToMoney(lOpMinutes, ref lOpMoney,ref stIntervalOffset,ref lstIntervals, ref m_bCurrentAmountIsAllowed))
                            {
                                throw new InvalidOperationException("Could not get offset money");
                            }

                            bStop = true;
                        }
                    }

                    // If in any operation we have reached the block end we will have to stop iterating
                    // in this block and continue in the next (if we are allowed to). But we need to know
                    // the needed money to reach the end of the block, and the used minutes are counting
                    // for the next interval
                    trace.Write(TraceLevel.Info, "Checking block end");

                    long lMaxAmountAllowed = 0;
                    long lTimeForMaxAmountAllowed =0;
                    long lMinAmountAllowed = GlobalDefs.DEF_UNDEFINED_VALUE;
                    bool bCalculateMinAmount = false;
                    long lCurrMinAmount = 0;

                    if ((m_lFirstAllowedAmountMoney == GlobalDefs.DEF_UNDEFINED_VALUE) && (m_bTimeLimitsCompute))
                    {

                        if (GetMinMoney() > m_lRealCurrMoney)
                        {
                            lCurrMinAmount = GetMinMoney() - m_lRealCurrMoney;
                        }
                        else
                        {
                            lCurrMinAmount = m_lRealCurrMoney;
                        }

                        bCalculateMinAmount = true;

                    }

                    stINTERVAL stBackupOffsetInterval = stIntervalOffset;

                    if (!CheckBlockEnd(lOperTime, lOperMoney, lBlockIni, lBlockEnd, ref lOpMinutes,ref lOpMoney,
                                        ref stIntervalOffset, ref lstIntervals, ref bJumpNextBlock, lMinutesWithZeroValue,
                                        ref m_bCurrentAmountIsAllowed, ref lMaxAmountAllowed,
                                        ref lTimeForMaxAmountAllowed, bCalculateMinAmount, lCurrMinAmount,ref lMinAmountAllowed))
                    {
                        throw new InvalidOperationException("Error checking block end");
                    }

                    if (m_bCurrentAmountIsAllowed)
                    {
                        GetLastAllowedAmountEndDate().SetDateTime(dtOper.Value.Year, dtOper.Value.Month, dtOper.Value.Day, 0, 0, 0);
                        GetLastAllowedAmountEndDate().AddTimeSpan(new TimeSpan(0, (int)((lOperTime + lOpMinutes) / 60), (int)((lOperTime + lOpMinutes) % 60),0));

                        m_lLastAllowedAmountMinutes = m_lCurrMinutes + lOpMinutes;
                        m_lLastAllowedAmountRealMinutes = m_lRealCurrMinutes + lOpMinutes;
                        m_lLastAllowedAmountMoney = m_lCurrMoney + lOpMoney;
                        m_lLastAllowedAmountRealMoney = m_lRealCurrMoney + lOpMoney;
                    }
                    else
                    {
                        if ((lMaxAmountAllowed != GlobalDefs.DEF_UNDEFINED_VALUE) && (lTimeForMaxAmountAllowed != GlobalDefs.DEF_UNDEFINED_VALUE))
                        {
                            GetLastAllowedAmountEndDate().SetDateTime(dtOper.Value.Year, dtOper.Value.Month, dtOper.Value.Day, 0, 0, 0);
                            GetLastAllowedAmountEndDate().AddTimeSpan( new TimeSpan(0, (int)(lOperTime + lTimeForMaxAmountAllowed) / 60, (int)(lOperTime + lTimeForMaxAmountAllowed) % 60, 0));

                            m_lLastAllowedAmountMinutes = m_lCurrMinutes + lTimeForMaxAmountAllowed;
                            m_lLastAllowedAmountRealMinutes = m_lRealCurrMinutes + lTimeForMaxAmountAllowed;
                            m_lLastAllowedAmountMoney = m_lCurrMoney + lMaxAmountAllowed;
                            m_lLastAllowedAmountRealMoney = m_lRealCurrMoney + lMaxAmountAllowed;
                        }
                    }

                    if ((bCalculateMinAmount) && (lMinAmountAllowed != GlobalDefs.DEF_UNDEFINED_VALUE))
                    {
                        m_lFirstAllowedAmountMoney = m_lCurrMoney + lMinAmountAllowed;
                        m_lFirstAllowedAmountRealMoney = m_lRealCurrMoney + lMinAmountAllowed;
                        SetMinMoney(m_lFirstAllowedAmountRealMoney);
                    }

                    m_lInitialTimeLastInterval = lBlockIni;
                    m_lEndTimeLastInterval = lBlockEnd;
                    m_lCurrentTimeLastInterval = lBlockIni + lOpMinutes;

                    if (m_bCalculateTimeSteps)
                    {
                        CalculateTimeSteps(lOpMoney, lOpMinutes, lOperTime,ref dtOper,ref stBackupOffsetInterval,ref lstIntervals);
                    }

                    if (m_bCalculateQuantitySteps)
                    {
                        CalculateQuantitySteps(lOpMoney, lOpMinutes, lOperTime, ref dtOper, ref stBackupOffsetInterval, ref lstIntervals);
                    }

                    lOperTime += lOpMinutes;
                    lOperMoney -= lOpMoney;
                    m_lCurrMinutes += lOpMinutes;
                    m_lCurrMoney += lOpMoney;
                    m_lTariffMoney += lOpMoney;
                    m_lRealCurrMoney += lOpMoney;
                    m_lRealCurrMinutes += lOpMinutes;
                    bStop = true;

                    if (lTimeLimit != GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        if ((lOperTime) >= lTimeLimit)
                            bTimeLimit = true;
                    }

                }

                // If the block jump is allowed
                bJumpNextBlock = bJumpNextBlock && (!bTimeLimit);
                bJumpNextBlock = bJumpNextBlock && ((m_lTariffMoney < GetMaxMoney()) || (GetMaxMoney() == 0));
                bJumpNextBlock = bJumpNextBlock && ((m_lCurrMinutes < GetMaxMinutes()) || (GetMaxMinutes() == 0) || (lstIntervals.Count == 0) || (lOpMinutes > 0));
                bJumpNextBlock = bJumpNextBlock && ((m_lRealCurrMinutes < m_lTimeLimit) || (m_lTimeLimit == GlobalDefs.DEF_UNDEFINED_VALUE) || (m_lTimeLimit == 0) || (lstIntervals.Count == 0) || (lOpMinutes > 0));


                if (bJumpNextBlock)
                {
                    if (!ChangeBlock(ref stTariffDesc,ref stIntervalOffset, ref lOperMoney,ref  lOperTime,ref dtOper, lBlockIni, lBlockEnd, ref bDayChange,ref bStop))
                    {
                        throw new InvalidOperationException("Could not change block");
                    }
                }
                else
                {
                    if ((bTimeLimit) && ((lOperTime) == lBlockEnd))
                    {
                        if (!DetectResets(ref stTariffDesc, ref stIntervalOffset, lBlockIni, lBlockEnd))
                        {
                            throw new InvalidOperationException("Could not change block");
                        }
                    }

                    if ((stTariffDesc.m_bRoundEndOfDay) && ((lOperTime) == lBlockEnd))
                    {
                        m_bRoundEndOfDay = true;
                        GetRoundEndOfDayDateTime().SetDateTime(dtOper.Value.Year, dtOper.Value.Month, dtOper.Value.Day, 0, 0, 0);
                        GetRoundEndOfDayDateTime().AddTimeSpan(new TimeSpan(0, (int)(lBlockEnd / 60), (int)(lBlockEnd % 60), 0));
                        
                        m_lRoundEndOfDayMinutes = m_lCurrMinutes;
                        m_lRoundEndOfDayRealMinutes = m_lRealCurrMinutes;
                        m_lRoundEndOfDayMoney = m_lCurrMoney;
                        m_lRoundEndOfDayRealMoney = m_lRealCurrMoney;
                        m_bRoundEndOfDayAmountIsAllowed = m_bCurrentAmountIsAllowed;
                    }

                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool ChangeBlock(ref stTariff stTariffDesc,ref stINTERVAL stOffsetInterval,ref long lOperMoney,ref  long lOperTime,ref COPSDate dtOper, long lBlockIni, long lBlockEnd,ref bool bDayChanged,ref  bool bStop)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::ChangeBlock");
            bool fnResult = true;

            try
            {
                Guard.IsNull(stTariffDesc, nameof(stTariffDesc));
                Guard.IsNull(stOffsetInterval, nameof(stOffsetInterval));

                bool bNextBlock = (stTariffDesc.m_iNextBlock == CTariffM1.TARIFFM1_NEXTBLOCK_ALLOWED);
                bNextBlock = bNextBlock || ((stTariffDesc.m_iNextBlock == CTariffM1.TARIFFM1_NEXTBLOCK_LESS_ALLOWED) && (stOffsetInterval.iMoney < stTariffDesc.m_iNextBlockConditionalValue));
                bNextBlock = bNextBlock || ((stTariffDesc.m_iNextBlock == CTariffM1.TARIFFM1_NEXTBLOCK_LESS_OR_EQUAL_ALLOWED) && (stOffsetInterval.iMoney <= stTariffDesc.m_iNextBlockConditionalValue));
                bNextBlock = bNextBlock || ((stTariffDesc.m_iNextBlock == CTariffM1.TARIFFM1_NEXTBLOCK_GREATER_ALLOWED) && ((stOffsetInterval.iMoney + (lOperMoney)) > stTariffDesc.m_iNextBlockConditionalValue));
                bNextBlock = bNextBlock || ((stTariffDesc.m_iNextBlock == CTariffM1.TARIFFM1_NEXTBLOCK_GREATER_OR_EQUAL_ALLOWED) && ((stOffsetInterval.iMoney + (lOperMoney)) >= stTariffDesc.m_iNextBlockConditionalValue));
                bNextBlock = bNextBlock || ((stTariffDesc.m_iNextBlock == CTariffM1.TARIFFM1_NEXTBLOCK_ALLOWED_IF_NO_FREE_TIME) && (m_lInAddFreeMinutesQuantity == 0) && (m_lInAddFreeMoneyQuantity == 0));
                bNextBlock = bNextBlock || ((stTariffDesc.m_iNextBlock == CTariffM1.TARIFFM1_NEXTBLOCK_NO_FREE_TIME_AND_LESS_ALLOWED) && (stOffsetInterval.iMoney < stTariffDesc.m_iNextBlockConditionalValue) &&
                                                    (m_lInAddFreeMinutesQuantity == 0) && (m_lInAddFreeMoneyQuantity == 0));
                bNextBlock = bNextBlock ||
                    ((stTariffDesc.m_iNextBlock == CTariffM1.TARIFFM1_NEXTBLOCK_NO_FREE_TIME_AND_LESS_OR_EQUAL_ALLOWED) &&
                                                    (stOffsetInterval.iMoney <= stTariffDesc.m_iNextBlockConditionalValue) &&
                                                    (m_lInAddFreeMinutesQuantity == 0) && (m_lInAddFreeMoneyQuantity == 0));
                bNextBlock = bNextBlock ||
                    ((stTariffDesc.m_iNextBlock == CTariffM1.TARIFFM1_NEXTBLOCK_NO_FREE_TIME_AND_GREATER_ALLOWED) &&
                                                    ((stOffsetInterval.iMoney + (lOperMoney)) > stTariffDesc.m_iNextBlockConditionalValue) &&
                                                    (m_lInAddFreeMinutesQuantity == 0) && (m_lInAddFreeMoneyQuantity == 0));
                bNextBlock = bNextBlock || ((stTariffDesc.m_iNextBlock == CTariffM1.TARIFFM1_NEXTBLOCK_NO_FREE_TIME_AND_GREATER_OR_EQUAL_ALLOWED) &&
                                                    ((stOffsetInterval.iMoney + (lOperMoney)) >= stTariffDesc.m_iNextBlockConditionalValue) &&
                                                    (m_lInAddFreeMinutesQuantity == 0) && (m_lInAddFreeMoneyQuantity == 0));

                if (stTariffDesc.m_bRoundEndOfDay)
                {
                    m_bRoundEndOfDay = true;
                    GetRoundEndOfDayDateTime().SetDateTime(dtOper.Value.Year, dtOper.Value.Month, dtOper.Value.Day,0, 0, 0);
                    GetRoundEndOfDayDateTime().AddTimeSpan(new TimeSpan(0, (int)(lBlockEnd / 60), (int)(lBlockEnd % 60), 0));

                    m_lRoundEndOfDayMinutes = m_lCurrMinutes;
                    m_lRoundEndOfDayRealMinutes = m_lRealCurrMinutes;
                    m_lRoundEndOfDayMoney = m_lCurrMoney;
                    m_lRoundEndOfDayRealMoney = m_lRealCurrMoney;
                    m_bRoundEndOfDayAmountIsAllowed = m_bCurrentAmountIsAllowed;
                }

                if (bNextBlock)
                {
                    trace.Write(TraceLevel.Info, "Block change allowed");
                    bStop = false;

                    // Do we have to reset the offset of the block jump? If so
                    // we start on the next block at the start of the interval
                    // not as if we started at the time spent in the last block
                    if ((stTariffDesc.m_bResetNextBlockInt) && (!m_bIgnoreResets))
                    {
                        trace.Write(TraceLevel.Info, "Reseting interval");
                        stOffsetInterval.iTime = 0;
                        stOffsetInterval.iMoney = 0;
                        m_lTariffMoney = 0;
                        m_bIntReset = true;

                    }

                    // Do we have to reset the accumulated time for this operation
                    // in this group? 
                    // TODO: As a parameter to reset the accumulated money in DB 
                    // is missing we assume that when time is reset, money is also 
                    // reset
                    if ((stTariffDesc.m_bResetNextBlockTime) && (!m_bIgnoreResets))
                    {
                        trace.Write(TraceLevel.Info, "Reseting accumulations");
                        m_lCurrMinutes = 0;
                        m_lCurrMoney = 0;
                        m_bTimeReset = true;


                        COPSDate newOperEfectiveInit = new COPSDate(dtOper);
                        newOperEfectiveInit.AddTimeSpan(new TimeSpan(0, (int)(lBlockEnd / 60), (int)(lBlockEnd % 60), 0));

                        SetOperEfectiveInitDateTime(newOperEfectiveInit);

                        if (m_bPostPay)
                        {
                            SetMaxMinutes(m_lPostPayMinutes - m_lRealCurrMinutes);
                        }
                        else
                        {
                            SetMaxMinutes(m_lPreHistMaxMinutes);
                        }

                        if (!m_bOperMoneySet)
                        {
                            if (m_lPreHistMaxMoney > GetMaxMoney())
                            {
                                lOperMoney = lOperMoney + (m_lPreHistMaxMoney - GetMaxMoney());
                            }
                        }
                        if (!m_bOperMaxMoneySet)
                            SetMaxMoney(m_lPreHistMaxMoney);

                    }

                    // If the block we are using is the last of the day, we change the block only if
                    // a jump to the next day is allowed
                    if (lBlockEnd == 1440)  // 24 (hours)*60 = 1440
                    {
                        if (stTariffDesc.m_bNextDay)
                        {
                            trace.Write(TraceLevel.Info, "Day change allowed in block");
                            bDayChanged = true;
                            lOperTime = lBlockEnd;

                            if (!m_dtDayChange)
                            {
                                m_dtDayChange = true;
                                m_lBlocksWithTariff = 0;
                            }

                            // Do we have to reset the offset of the day jump? If so
                            // we start on the next block at the start of the interval
                            // not as if we started at the time spent in the last block
                            if ((stTariffDesc.m_bResetNextDayInterval) && (!m_bIgnoreResets))
                            {
                                stOffsetInterval.iTime = 0;
                                stOffsetInterval.iMoney = 0;
                                m_lTariffMoney = 0;
                                m_bIntReset = true;
                            }

                            // Do we have to reset the accumulated time for this operation
                            // in this group? 
                            // TODO: As a parameter to reset the accumulated money in DB 
                            // is missing we assume that when time is reset, money is also 
                            // reset
                            if ((stTariffDesc.m_bResetNextDayTime) && (!m_bIgnoreResets))
                            {
                                m_lCurrMinutes = 0;
                                m_lCurrMoney = 0;
                                m_bTimeReset = true;

                                COPSDate newOperEfectiveInit = new COPSDate(dtOper);
                                newOperEfectiveInit.AddTimeSpan(new TimeSpan(0, (int)(lBlockEnd / 60), (int)(lBlockEnd % 60), 0));

                                SetOperEfectiveInitDateTime(newOperEfectiveInit);

                                if (m_bPostPay)
                                {
                                    SetMaxMinutes(m_lPostPayMinutes - m_lRealCurrMinutes);
                                }
                                else
                                {
                                    SetMaxMinutes(m_lPreHistMaxMinutes);
                                }

                                if (!m_bOperMoneySet)
                                {
                                    if (m_lPreHistMaxMoney > GetMaxMoney())
                                        lOperMoney = lOperMoney + (m_lPreHistMaxMoney - GetMaxMoney());
                                }

                                if (!m_bOperMaxMoneySet)
                                    SetMaxMoney(m_lPreHistMaxMoney);

                            }

                        }
                        else
                        {
                            trace.Write(TraceLevel.Info, "Day change not allowed in block");
                            bStop = true;
                        }
                    }
                    else
                    {
                        // Set the time to the block end only if we are not changing day
                        lOperTime = lBlockEnd;
                    }

                }
                else
                {
                    trace.Write(TraceLevel.Info, "Block change not allowed");
                    bStop = true;
                    DetectResets(ref stTariffDesc,ref  stOffsetInterval, lBlockIni, lBlockEnd);
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        private bool DetectResets(ref stTariff stTariffDesc,ref stINTERVAL stIntervalOffset, long lBlockIni, long lBlockEnd)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::DetectResets");
            bool fnResult = true;

            try
            {
                Guard.IsNull(stTariffDesc, nameof(stTariffDesc));

                if ((stTariffDesc.m_bResetNextBlockInt) && (!m_bIgnoreResets))
                {
                    trace.Write(TraceLevel.Info, "Reseting interval");
                    m_bIntReset = true;
                    stIntervalOffset.iTime = 0;
                    stIntervalOffset.iMoney = 0;
                    m_lTariffMoney = 0;
                }

                // Do we have to reset the accumulated time for this operation
                // in this group? 
                // TODO: As a parameter to reset the accumulated money in DB 
                // is missing we assume that when time is reset, money is also 
                // reset
                if ((stTariffDesc.m_bResetNextBlockTime) && (!m_bIgnoreResets))
                {
                    trace.Write(TraceLevel.Info, "Reseting accumulations");
                    m_bTimeReset = true;
                    m_lCurrMinutes = 0;
                    m_lCurrMoney = 0;
                    m_bTimeReset = true;

                }

                // If the block we are using is the last of the day, we change the block only if
                // a jump to the next day is allowed
                if (lBlockEnd == 1440)  // 24 (hours)*60 = 1440
                {
                    if ((stTariffDesc.m_bResetNextDayInterval) && (!m_bIgnoreResets))
                    {
                        m_bIntReset = true;
                        stIntervalOffset.iTime = 0;
                        stIntervalOffset.iMoney = 0;
                        m_lTariffMoney = 0;

                    }

                    // Do we have to reset the accumulated time for this operation
                    // in this group? 
                    // TODO: As a parameter to reset the accumulated money in DB 
                    // is missing we assume that when time is reset, money is also 
                    // reset
                    if ((stTariffDesc.m_bResetNextDayTime) && (!m_bIgnoreResets))
                    {
                        m_bTimeReset = true;
                        m_lCurrMinutes = 0;
                        m_lCurrMoney = 0;
                        m_bTimeReset = true;

                    }

                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool CalculateQuantitySteps(long lOpMoney, long lOpMinutes, long lOperTime, ref COPSDate dtOper, ref stINTERVAL stIntervalOffset, ref List<stINTERVAL> lstIntervals)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::CalculateQuantitySteps");
            bool fnResult = true;

            try
            {
                bool bCurrentAmountIsAllowed = false;
                long lMinutes = 0;
                long lMinutesWithZeroValue = 0;

                if (m_lCalculateSteps_CurrentValue == GlobalDefs.DEF_UNDEFINED_VALUE)
                {

                    if ((m_lRealCurrMoney + lOpMoney) >= GetMinMoney())
                    {
                        m_lCalculateSteps_CurrentValue = (int)GetMinMoney();
                    }
                }

                if (m_lCalculateSteps_CurrentValue != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    while ((m_lCalculateSteps_CurrentValue) <= (m_lRealCurrMoney + lOpMoney))
                    {
                        stStepCalculation pstStepCalculation = new stStepCalculation();

                        pstStepCalculation.lMoney = m_lCalculateSteps_CurrentValue;

                        bCurrentAmountIsAllowed = true;
                        lMinutes = 0;

                        if (!MoneyToTime(pstStepCalculation.lMoney - m_lRealCurrMoney, ref lMinutes,ref stIntervalOffset,ref lstIntervals, ref lMinutesWithZeroValue, ref bCurrentAmountIsAllowed))
                        {
                            throw new InvalidOperationException("Could not get offset money");
                        }

                        if (m_bRoundEndOfDay)
                        {
                            pstStepCalculation.lMinutes = m_lRoundEndOfDayMinutes;
                            pstStepCalculation.dtDate = GetRoundEndOfDayDateTime().Copy();
                        }
                        else
                        {
                            pstStepCalculation.lMinutes = lMinutes + m_lRealCurrMinutes;
                            pstStepCalculation.dtDate.SetDateTime(dtOper.Value.Year, dtOper.Value.Month, dtOper.Value.Day,
                                                    (int)(lOperTime + pstStepCalculation.lMinutes - m_lRealCurrMinutes) / 60,
                                                    (int)(lOperTime + pstStepCalculation.lMinutes - m_lRealCurrMinutes) % 60, 0);
                        }

                        if (bCurrentAmountIsAllowed)
                        {

                            if (m_CalculateSteps_Steps.Count == 0)
                            {
                                m_CalculateSteps_Steps.Add(pstStepCalculation);
                            }
                            else
                            {
                                stStepCalculation pLastStepCalculation = m_CalculateSteps_Steps.Last();
                                if (pLastStepCalculation.lMoney == pstStepCalculation.lMoney)
                                {

                                    pLastStepCalculation.lMinutes = pstStepCalculation.lMinutes;
                                    pLastStepCalculation.dtDate = pstStepCalculation.dtDate.Copy();
                                    pstStepCalculation = default(stStepCalculation);
                                }
                                else if (pLastStepCalculation.lMinutes == pstStepCalculation.lMinutes)
                                {
                                    pLastStepCalculation.lMoney = pstStepCalculation.lMoney;
                                    pLastStepCalculation.dtDate = pstStepCalculation.dtDate.Copy();
                                    pstStepCalculation = default(stStepCalculation);
                                }
                                else
                                {
                                    m_CalculateSteps_Steps.Add(pstStepCalculation);
                                }
                            }
                        }
                        else
                        {
                            pstStepCalculation = default(stStepCalculation);
                        }

                        m_lCalculateSteps_CurrentValue += m_lCalculateSteps_StepValue;

                    }
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool CalculateTimeSteps(long lOpMoney, long lOpMinutes, long lOperTime,ref  COPSDate dtOper, ref stINTERVAL stOffsetInterval,ref List<stINTERVAL> lstIntervals)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::CalculateTimeSteps");
            bool fnResult = true;

            try
            {
                bool bCurrentAmountIsAllowed = false;
                long lMoney = 0;
                long lMinutes = 0;
                long lMinutesWithZeroValue = 0;

                if (m_lCalculateSteps_CurrentValue == GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    if ((m_lRealCurrMoney + lOpMoney) >= GetMinMoney())
                    {
                        long lMinMoney = GetMinMoney();

                        if (!MoneyToTime(lMinMoney, ref lMinutes, ref stOffsetInterval,ref lstIntervals, ref lMinutesWithZeroValue, ref bCurrentAmountIsAllowed))
                        {
                            throw new InvalidOperationException("Could not get offset money");
                        }

                        SetMinMoney(lMinMoney);
                        m_lCalculateSteps_CurrentValue = (int)(m_lRealCurrMinutes + lMinutes);
                    }
                }

                if (m_lCalculateSteps_CurrentValue != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    while ((m_lCalculateSteps_CurrentValue) <= (m_lRealCurrMinutes + lOpMinutes))
                    {
                        stStepCalculation pstStepCalculation = new stStepCalculation();
                        pstStepCalculation.dtDate = new COPSDate();

                        pstStepCalculation.lMinutes = m_lCalculateSteps_CurrentValue;

                        bCurrentAmountIsAllowed = true;
                        lMoney = 0;
                        lMinutes = 0;

                        if (!TimeToMoney( pstStepCalculation.lMinutes - m_lRealCurrMinutes, ref lMoney,ref stOffsetInterval,ref lstIntervals, ref bCurrentAmountIsAllowed))
                        {
                            throw new InvalidOperationException("Could not get offset money");
                        }

                        pstStepCalculation.lMoney = lMoney + m_lRealCurrMoney;

                        if (!MoneyToTime(lMoney, ref lMinutes,ref stOffsetInterval,ref lstIntervals, ref lMinutesWithZeroValue, ref bCurrentAmountIsAllowed))
                        {
                            throw new InvalidOperationException("Could not get offset money");
                        }

                        if (m_bRoundEndOfDay)
                        {
                            pstStepCalculation.lMinutes = m_lRoundEndOfDayMinutes;
                            pstStepCalculation.dtDate = GetRoundEndOfDayDateTime().Copy();
                        }
                        else
                        {
                            pstStepCalculation.lMinutes = lMinutes + m_lRealCurrMinutes;
                            pstStepCalculation.dtDate.SetDateTime(dtOper.Value.Year, dtOper.Value.Month, dtOper.Value.Day,
                                                    (int)(lOperTime + pstStepCalculation.lMinutes - m_lRealCurrMinutes) / 60,
                                                    (int)(lOperTime + pstStepCalculation.lMinutes - m_lRealCurrMinutes) % 60, 0);
                        }

                        if (bCurrentAmountIsAllowed)
                        {

                            if (m_CalculateSteps_Steps.Count == 0)
                            {
                                m_CalculateSteps_Steps.Add(pstStepCalculation);
                            }
                            else
                            {
                                stStepCalculation pLastStepCalculation = m_CalculateSteps_Steps.Last();
                                if (pLastStepCalculation.lMoney == pstStepCalculation.lMoney)
                                {

                                    pLastStepCalculation.lMinutes = pstStepCalculation.lMinutes;
                                    pLastStepCalculation.dtDate = pstStepCalculation.dtDate.Copy();
                                    pstStepCalculation = default(stStepCalculation);
                                }
                                else if (pLastStepCalculation.lMinutes == pstStepCalculation.lMinutes)
                                {
                                    pLastStepCalculation.lMoney = pstStepCalculation.lMoney;
                                    pLastStepCalculation.dtDate = pstStepCalculation.dtDate.Copy();
                                    pstStepCalculation = default(stStepCalculation);
                                }
                                else
                                {
                                    m_CalculateSteps_Steps.Add(pstStepCalculation);
                                }
                            }

                        }
                        else
                        {
                            pstStepCalculation = default(stStepCalculation);
                        }

                        m_lCalculateSteps_CurrentValue += m_lCalculateSteps_StepValue;

                    }

                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool CheckBlockEnd( long lOperTime, long lOperMoney, long lBlockIni, long lBlockEnd,
                                    ref long lOpMinutes,ref long lOpMoney, 
                                    ref stINTERVAL stIntervalOffset, 
                                    ref List<stINTERVAL> lstIntervals,
                                    ref bool bJumpNextBlock, long lMinutesWithZeroValue, 
                                    ref bool bCurrentAmountIsAllowed,
                                    ref  long lMaxAmountAllowed, ref long lTimeForMaxAmountAllowed, 
                                    bool bCalculateMinAllowed, long lCurrentMinAmount, 
                                    ref long lMinAmountAllowed)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::CheckBlockEnd");
            bool fnResult = true;

            try
            {
                bJumpNextBlock = false;
                lMaxAmountAllowed = GlobalDefs.DEF_UNDEFINED_VALUE;
                lTimeForMaxAmountAllowed = GlobalDefs.DEF_UNDEFINED_VALUE;
                lMinAmountAllowed = GlobalDefs.DEF_UNDEFINED_VALUE;

                if ((lOperTime + lOpMinutes) >= lBlockEnd)
                {
                    trace.Write(TraceLevel.Info, $"Block end ({lBlockEnd}) has been reached ({lOperTime + lOpMinutes}), a block change is needed");

                    lOpMinutes = lBlockEnd - lOperTime;

                    if (!TimeToMoney(lOpMinutes, ref lOpMoney, ref stIntervalOffset, ref lstIntervals, ref bCurrentAmountIsAllowed))
                    {
                        throw new InvalidOperationException("Could not get offset money");
                    }
                }

                bool bCurrCurrentAmountIsAllowed = false;
                long lCurrMinutes = lOpMinutes - 1;
                long lCurrMoney = GlobalDefs.DEF_UNDEFINED_VALUE;

                if (!bCurrentAmountIsAllowed)
                {
                    while (!bCurrCurrentAmountIsAllowed && lCurrMinutes > 0)
                    {
                        if (!TimeToMoney( lCurrMinutes, ref lCurrMoney, ref stIntervalOffset,ref lstIntervals, ref bCurrCurrentAmountIsAllowed))
                        {
                            throw new InvalidOperationException("Could not get offset money");
                        }
                        
                        if (!bCurrCurrentAmountIsAllowed)
                        {
                            lCurrMinutes--;
                        }
                    }

                    if (bCurrCurrentAmountIsAllowed)
                    {
                        long lMinutesWithZeroValueTemp = 0;
                        if (!MoneyToTime( lCurrMoney, ref lCurrMinutes, ref stIntervalOffset,ref  lstIntervals, ref lMinutesWithZeroValueTemp, ref bCurrCurrentAmountIsAllowed))
                        {
                            throw new InvalidOperationException("Could not get offset money");
                        }

                        lMaxAmountAllowed = lCurrMoney;
                        lTimeForMaxAmountAllowed = lCurrMinutes;
                    }
                }
                else
                {
                    lMaxAmountAllowed = lOpMoney;
                    lTimeForMaxAmountAllowed = lOpMinutes;
                }

                if (bCalculateMinAllowed && (lTimeForMaxAmountAllowed != GlobalDefs.DEF_UNDEFINED_VALUE))
                {
                    bCurrCurrentAmountIsAllowed = false;

                    lCurrMinutes = 1;
                    lCurrMoney = GlobalDefs.DEF_UNDEFINED_VALUE;

                    while (!bCurrCurrentAmountIsAllowed && lCurrMinutes <= lTimeForMaxAmountAllowed)
                    {
                        if (!TimeToMoney( lCurrMinutes, ref lCurrMoney, ref stIntervalOffset, ref lstIntervals, ref bCurrCurrentAmountIsAllowed))
                        {
                            throw new InvalidOperationException("Could not get offset money");
                        }
                        
                        if (!bCurrCurrentAmountIsAllowed || lCurrMoney < lCurrentMinAmount) 
                        {
                            lCurrMinutes++;
                            bCurrCurrentAmountIsAllowed = false;
                        }

                    }

                    if (bCurrCurrentAmountIsAllowed)
                    {
                        long lMinutesWithZeroValueTemp = 0;
                        if (!MoneyToTime( lCurrMoney, ref lCurrMinutes, ref stIntervalOffset, ref lstIntervals, ref lMinutesWithZeroValueTemp,
                            ref bCurrCurrentAmountIsAllowed))
                        {
                            throw new InvalidOperationException("Could not get offset money");
                        }

                        lMinAmountAllowed = lCurrMoney;
                    }

                }

                if ((lOperTime + lOpMinutes) >= lBlockEnd)
                {
                    // Set offsets
                    stIntervalOffset.iTime += lOpMinutes;
                    stIntervalOffset.iMoney += lOpMoney;

                    if ((lOperMoney - lOpMoney > 0) || ((lOperMoney == 0) && ((lOpMoney) == 0)) || (((lOperMoney - lOpMoney) == 0) && ((lOperTime + lOpMinutes) == lBlockEnd)))
                    {
                        bJumpNextBlock = true;
                    }
                }

                if ((lBlockEnd - lOperTime) < lMinutesWithZeroValue)
                {
                    m_iRemainingMinutesWithZeroValue += (int)(lBlockEnd - lOperTime);
                }
                else
                {
                    m_iRemainingMinutesWithZeroValue += (int)lMinutesWithZeroValue;
                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        

        private bool CheckMaxTime(ref long lOpMinutes, ref long lOpMoney,ref stINTERVAL stIntervalOffset,ref List<stINTERVAL> lstIntervals, ref bool bStop)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::CheckMaxTime");
            bool fnResult = true;

            try
            {
                trace.Write(TraceLevel.Info, $"Available time: {GetMaxMinutes()}, Operation accumulated time: {m_lCurrMinutes}, Operation time: {lOpMinutes}");

                bool bCurrentAmountIsAllowed = false;

                if ((m_lTimeLimit != GlobalDefs.DEF_UNDEFINED_VALUE) && (!m_bPostPay))
                {
                    if ((lOpMinutes) + m_lRealCurrMinutes > m_lTimeLimit)
                    {
                        lOpMinutes = m_lTimeLimit - m_lRealCurrMinutes;

                        if (!TimeToMoney( lOpMinutes,ref lOpMoney, ref stIntervalOffset,ref  lstIntervals,ref bCurrentAmountIsAllowed))
                        {
                            throw new InvalidOperationException("Could not find minutes");
                        }

                        bStop = true;
                    }
                }

                if ((lOpMinutes) + m_lCurrMinutes > GetMaxMinutes())
                {
                    trace.Write(TraceLevel.Info, "MAX. TIME CONSTRAINT");

                    lOpMinutes = GetMaxMinutes() - m_lCurrMinutes;

                    if (!TimeToMoney(lOpMinutes, ref lOpMoney, ref stIntervalOffset,ref  lstIntervals,ref bCurrentAmountIsAllowed))
                    {
                        throw new InvalidOperationException("Could not find minutes");
                    }

                    bStop = true;
                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool TimeToMoney(long lOpMinutes,ref long lOpMoney, ref stINTERVAL stIntervalOffset,ref List<stINTERVAL> lstIntervals, ref bool bCurrentAmountIsAllowed)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::TimeToMoney");
            bool fnResult = true;

            stINTERVAL stLastInterval = new stINTERVAL();
            stINTERVAL stCurrentInterval = new stINTERVAL();
            double dTemp = 0;

            try
            {
                stLastInterval.iMoney = 0;
                stLastInterval.iTime = 0;
                stCurrentInterval.iMoney = 0;
                stCurrentInterval.iTime = 0;
                lOpMinutes += stIntervalOffset.iTime;
                bCurrentAmountIsAllowed = true;

                foreach (stINTERVAL interval in lstIntervals)
                {
                    stLastInterval = stCurrentInterval;
                    stCurrentInterval = interval;

                    if (stLastInterval.iTime <= (int)lOpMinutes * 60 && (int)lOpMinutes * 60 < stCurrentInterval.iTime)
                    {
                        if (!Interpolate(stLastInterval.iTime, stLastInterval.iMoney,
                                         stCurrentInterval.iTime, stCurrentInterval.iMoney,
                                         lOpMinutes * 60, ref dTemp))
                        {
                            throw new InvalidOperationException("Could not interpolate money");
                        }
                        lOpMoney = (long)(dTemp + 0.5);

                        if (stLastInterval.iMoney < (lOpMoney) && (lOpMoney) < stCurrentInterval.iMoney)
                        {
                            bCurrentAmountIsAllowed = (stCurrentInterval.iIntervalIntermediateValuesPossible == 1);
                        }
                        else
                        {
                            bCurrentAmountIsAllowed = true;
                        }

                        break;
                    }
                    else if ((int)lOpMinutes * 60 < stCurrentInterval.iTime)
                    {
                        if (!Interpolate(0, 0, stCurrentInterval.iTime, stCurrentInterval.iMoney, lOpMinutes * 60, ref dTemp))
                        {
                            throw new InvalidOperationException("Could not interpolate money");
                        }
                        lOpMoney = (long)(dTemp + 0.5);

                    }
                    else if ((int)lOpMinutes * 60 >= stCurrentInterval.iTime)
                    {
                        lOpMoney = stCurrentInterval.iMoney;
                    }
                }

                lOpMoney -= stIntervalOffset.iMoney;
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // CheckMaxMoney
        //	Checks if the money accumulated in operations for a vehicel is lower than the maximum money for 
        // the same vehicle in the current group. The accumulated money is the sum of the current operation
        // accumulated money and the historic (if exists) accumulated money for this vehicle in this group
        //
        // Parameters:
        //	1. lOperMoney [in]: Money introduced to compute the parking. 
        //	2. plMoney [out]: If the historic accumulated money plus the current operation accumulated money
        //					  is greater than the group maximum, plMoney will store the money that the user
        //					  can introduced to arrive to the maximum (i.e. If the user accumulates 180 cents,
        //					  the current operation is of 100 cents, and the maximum is 200 cents ==> 180+100 > 200
        //					  and plMoney will store 20 (200 - 180). Of course, it also involves the accumulation 
        //					  between blocks in the current operation
        //	3. plMinutes [out]: Will store the minutes that are obtained with plMoney. Take into account that
        //						an interval offset can exist
        //	4. pstOffsetInterval [in]: It stores the offset in minutes of the previous block, and how much
        //							   money do they mean in the current block
        //	5. plstIntervals [in]: List of intervals of this block
        //	6. pbStop [out]: If the money introduced is greater than the maximum, we have to stop iterating. But
        //					 if with the minutes obtained are greater than the block end we can continue iterating
        //					 but this will be checked by the function CheckBlockEnd
        //
        private bool CheckMaxMoney(long lOperMoney, ref long lMoney, ref long lOpMinutes, ref stINTERVAL stIntervalOffset,ref List<stINTERVAL> lstIntervals, ref bool bStop)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::CheckMaxMoney");
            bool fnResult = true;

            try
            {
                trace.Write(TraceLevel.Info, $"Available money: {GetMaxMoney()}, Operation accumulated money: {m_lCurrMoney}, Operation Money: {lOperMoney}");

                long lTemp = GlobalDefs.DEF_UNDEFINED_VALUE;
                bool bCurrentAmountIsAllowed = false;
                if (lOperMoney + m_lCurrMoney > GetMaxMoney())
                {
                    trace.Write(TraceLevel.Debug, "MAX. MONEY CONSTRAINT");

                    lMoney = GetMaxMoney() - m_lCurrMoney;

                    if (!MoneyToTime( lMoney, ref lOpMinutes, ref stIntervalOffset, ref lstIntervals, ref lTemp, ref bCurrentAmountIsAllowed))
                    {
                        throw new InvalidOperationException("Could not find minutes");
                    }

                    bStop = true;
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // MoneyToTime
        //	Returns the time in an interval that corresponds to a quantity of money. If the money does not
        // correspond to any of the intervals, an interpolation is made between the above and below intervals
        //
        private bool MoneyToTime(long lMoney, ref long lOpMinutes, ref stINTERVAL stIntervalOffset,ref List<stINTERVAL> lstIntervals, ref long plMinutesWithZeroValue, ref bool bCurrentAmountIsAllowed)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::MoneyToTime");
            bool fnResult = true;

            stINTERVAL stLastInterval = new stINTERVAL();
            stINTERVAL stCurrentInterval = new stINTERVAL();
            double dTemp = 0;

            try
            {
                stLastInterval.iMoney = 0;
                stLastInterval.iTime = 0;
                stCurrentInterval.iMoney = 0;
                stCurrentInterval.iTime = 0;

                trace.Write(TraceLevel.Info, $"Operation Money: {lMoney}, Offset: Money({stIntervalOffset.iMoney}), Time({stIntervalOffset.iTime})");

                lMoney += stIntervalOffset.iMoney;

                trace.Write(TraceLevel.Info, $"Acc Money: {lMoney}");
                bCurrentAmountIsAllowed = true;

                foreach (stINTERVAL interval in lstIntervals)
                {
                    stLastInterval = stCurrentInterval;

                    stCurrentInterval = interval;

                    if (stLastInterval.iMoney <= lMoney && lMoney < stCurrentInterval.iMoney)
                    {
                        if (!Interpolate(stLastInterval.iMoney, stLastInterval.iTime,
                                         stCurrentInterval.iMoney, stCurrentInterval.iTime,
                                         lMoney,ref dTemp))
                        {
                            throw new InvalidOperationException("Could not interpolate time");
                        }

                        lOpMinutes = (long)((dTemp / 60) + 0.5);

                        bCurrentAmountIsAllowed = ((stCurrentInterval.iIntervalIntermediateValuesPossible == 1) || (stLastInterval.iMoney == lMoney));

                        break;
                    }
                    else if ((int)lMoney < stCurrentInterval.iMoney)
                    {
                        if (!Interpolate(0, 0,stCurrentInterval.iMoney, stCurrentInterval.iTime, lMoney, ref dTemp))
                        {
                            throw new InvalidOperationException("Could not interpolate money");
                        }
                        lOpMinutes = (long)((dTemp / 60) + 0.5);
                        bCurrentAmountIsAllowed = (stCurrentInterval.iIntervalIntermediateValuesPossible == 1);
                    }
                    else if ((int)lMoney >= stCurrentInterval.iMoney)
                    {
                        lOpMinutes = (long)(((double)(stCurrentInterval.iTime) / 60) + 0.5);
                        if ((int)lMoney == stCurrentInterval.iMoney)
                        {
                            bCurrentAmountIsAllowed = true;
                            //break;
                        }
                    }

                    if ((stCurrentInterval.iMoney == 0) && ((lOpMinutes) > stIntervalOffset.iTime))
                    {
                        plMinutesWithZeroValue = (lOpMinutes) - stIntervalOffset.iTime;
                    }
                }

                trace.Write(TraceLevel.Info, $"Money: {lMoney} ==> Minutes {lOpMinutes}");

                lOpMinutes -= stIntervalOffset.iTime;

                trace.Write(TraceLevel.Info, $"Real Money: {lMoney - stIntervalOffset.iMoney} ==> Real minutes: {lOpMinutes}");

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool Interpolate(long x0, long fx0, long x1, long fx1, long x,ref double pfx, int iMode = 0)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::Interpolate");
            bool fnResult = true;

            try
            {
                Guard.IsNull(pfx, nameof(pfx));

                if (x == x0)
                {
                    pfx = fx0;
                }
                else if (x == x1)
                {
                    pfx = fx1;
                }
                else
                {

                    if (iMode == 0)
                    {
                        pfx = (double)(fx0) + (((double)(fx1 - fx0) / (double)(x1 - x0)) * (double)(x - x0));
                        trace.Write(TraceLevel.Info, $"x0={x0}, fx0={fx0}, x1={x1}, fx1={fx1}, x={x}=> fx={pfx}");
                    }
                    else
                    {
                        trace.Write(TraceLevel.Error, $"Mode {iMode} not supported");
                    }
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool TraceIntervals(List<stINTERVAL> lstIntervals)
        {

            trace.Write(TraceLevel.Debug, "M1ComputeEx0::TraceIntervals");
            bool fnResult = true;

            try
            {
                Guard.IsNull(lstIntervals, nameof(lstIntervals));

                trace.Write(TraceLevel.Info, "Listing interval ...");
                trace.Write(TraceLevel.Info, $"[ {"Position", -8} | {"Minutes",-7} | {"Money",-5} ]");

                int index = 0;
                foreach (stINTERVAL interval in lstIntervals)
                {
                    trace.Write(TraceLevel.Info, $"[ {index,-8} | {interval.iTime,-7} | {interval.iMoney,-5} ]");
                    index++;
                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool GetBranchState(CM1GroupsTree tree, long groupId, long lBlockID, ref List<long> dayTypes, ref long lState)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::GetBranchState");
            bool fnResult = true;

            long lCurrStatus = GlobalDefs.DEF_PDMOK;
            long lStatus = GlobalDefs.DEF_UNDEFINED_VALUE;
            bool bFind = false;

            try
            {
                Guard.IsNull(tree, nameof(tree), "Could not obtain tree");

                CM1Group group = tree.GetGroupFromGrpId(groupId);
                if (group == null)
                {
                    throw new InvalidOperationException("Could not obtain group of current operation");
                }

                while (group != null)
                {
                    if (!m_pDBB.GetStatusFromBlock(GetUnitId(), group.GetGrpId(), group.GetGrpTypeId(), lBlockID, dayTypes, ref bFind, ref lStatus))
                    {
                        throw new InvalidOperationException("Error in call to m_pPDMDB->GetStatusFromBlock");
                    }

                    if (bFind)
                    {
                        lCurrStatus = Math.Max(lStatus, lCurrStatus);
                    }

                    group = tree.GetGroupParent(group.GetGrpId());
                }

                lState = lCurrStatus;

                trace.Write(TraceLevel.Info, $"Status for branch of group {groupId}: {lState}");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool SetOutput(CDatM1 pM1, bool bM1Plus)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::SetOutput");
            bool fnResult = true;

            COPSDate dtOper;
            COPSPlate strPlate;
            long lImport = 0;
            bool bBackupCurrentAmountIsAllowed = true;

            try
            {
                // OPERATION TYPE
                pM1.SetInOperType((int)GetOperType());

                // FINAL RESULT
                pM1.SetOutResult((int)m_lResult);


                switch (GetOperType())
                {
                    case OperationDat.DEF_OPERTYPE_AMP:
                        dtOper = GetFirstDateTime().Copy();
                        pM1.SetOutOperDateIni0(dtOper);

                        dtOper = GetInitialDateTime().Copy();
                        pM1.SetOutOperDateIni(dtOper);
                        bBackupCurrentAmountIsAllowed = m_bCurrentAmountIsAllowed;

                        if ((!m_bRoundEndOfDay) && (m_bCurrentAmountIsAllowed))
                        {
                            dtOper = GetFinalDateTime().Copy();
                            pM1.SetOutOperDateEnd(dtOper);
                        }
                        else if ((!m_bRoundEndOfDay) && (!m_bCurrentAmountIsAllowed))
                        {
                            if (!bM1Plus)
                            {
                                if (m_lLastAllowedAmountRealMinutes != GlobalDefs.DEF_UNDEFINED_VALUE)
                                {
                                    pM1.SetOutOperDateEnd(GetLastAllowedAmountEndDate());
                                    m_lRealCurrMinutes = m_lLastAllowedAmountRealMinutes;
                                    m_lCurrMinutes = m_lLastAllowedAmountMinutes;
                                    m_lRealCurrMoney = m_lLastAllowedAmountRealMoney;
                                    m_lCurrMoney = m_lLastAllowedAmountMoney;
                                    m_bCurrentAmountIsAllowed = true;
                                }
                                else
                                {
                                    m_lResult = M1_OP_NOK;
                                    pM1.SetOutResult((int)m_lResult);

                                }
                            }
                            else
                            {
                                if ((m_lLastAllowedAmountRealMinutes != GlobalDefs.DEF_UNDEFINED_VALUE) &&
                                    (m_lRealCurrMoney == m_lLastAllowedAmountRealMoney))
                                {
                                    pM1.SetOutOperDateEnd(GetLastAllowedAmountEndDate());
                                    m_lRealCurrMinutes = m_lLastAllowedAmountRealMinutes;
                                    m_lCurrMinutes = m_lLastAllowedAmountMinutes;
                                    m_lRealCurrMoney = m_lLastAllowedAmountRealMoney;
                                    m_lCurrMoney = m_lLastAllowedAmountMoney;
                                    m_bCurrentAmountIsAllowed = true;
                                }
                            }

                        }
                        else
                        {
                            pM1.SetOutOperDateEnd(GetRoundEndOfDayDateTime());
                            m_lRealCurrMinutes = m_lRoundEndOfDayRealMinutes;
                            m_lCurrMinutes = m_lRoundEndOfDayMinutes;
                        }

                        if (GetMinMoney() > GetMaxMoney())
                        {
                            SetMinMoney(GetMaxMoney());
                        }

                        long lMinValue = GetMinMoney();

                        if ((GetPaymentType() != OperationDat.DEF_OPERPAY_MOBILE) && (pM1.GetInRoundMoney()))
                        {
                            pM1.SetOutMinImport(GetRoundMoney(lMinValue));
                        }
                        else
                        {
                            pM1.SetOutMinImport(lMinValue);
                        }

                        long lMaxValue = GetMaxMoney();
                        if ((bM1Plus) && (GeneralParams.UnitId == GlobalDefs.CC_UNIT_ID.ToString()) && (m_lTimeLimit != GlobalDefs.DEF_UNDEFINED_VALUE))
                        {
                            lMaxValue = m_lRealCurrMoney;
                        }

                        if ((GetPaymentType() != OperationDat.DEF_OPERPAY_MOBILE) && (pM1.GetInRoundMoney()))
                        {
                            pM1.SetOutMaxImport(GetRoundUpMoney(lMaxValue));
                        }
                        else
                        {
                            pM1.SetOutMaxImport(lMaxValue);
                        }

                        pM1.SetOutRealMinutes(m_lRealCurrMinutes);
                        pM1.SetOutRealAccumulateMoney(m_lRealAccMoney);
                        pM1.SetOutRealAccumulateTime((int)m_lRealAccMinutes);
                        pM1.SetOutOperDateRealIni(GetOperDateRealIniDateTime());
                        pM1.SetOutWholeOperationWithChipCard(m_iWholeOperationWithChipCard);
                        pM1.SetOutWholeOperationWithMobile(m_iWholeOperationWithMobile);

                        // WE MUST MODIFY OPERATION VARIABLES ONLY IF THE USER HAS INTRODUCED MONEY, NOT WHEN
                        // CALCULATING MAXIMUM MONEY AND MAXIMUM PARKING TIME, OR WHEN THE OPERATION IS A RETURN
                        /*if( pDatM1->GetOutIntAcumul() > 0 )
                        {*/
                        // ACCUMULATED MONEY (FOR THE NEXT OPERATION)
                        pM1.SetOutAccumulateMoney(m_stInitialIntervalOffset.iMoney);

                        // ACCUMULATED TIME (FOR THE NEXT OPERATION)
                        pM1.SetOutAccumulateTime((int)m_stInitialIntervalOffset.iTime);

                        // TIME CONSUMED IN THE CURRENT OPERATION
                        pM1.SetOutEfMaxTime((int)m_lCurrMinutes);

                        pM1.SetOutPostPay(m_bPostPay);

                        if (!bM1Plus)
                        {
                            pM1.SetOutHistFreeMoneyUsed(m_bHistFreeMoneyUsed);

                            long lCandidateMinMoney = GetMinMoney();

                            if ((GetPaymentType() != OperationDat.DEF_OPERPAY_MOBILE) && (pM1.GetInRoundMoney()))
                            {
                                pM1.SetOutMinImport(GetRoundMoney(lCandidateMinMoney));
                            }
                            else
                            {
                                pM1.SetOutMinImport(lCandidateMinMoney);
                            }

                            long lCandidateMaxMoney = m_lRealCurrMoney;
                            if ((GetPaymentType() != OperationDat.DEF_OPERPAY_MOBILE) && (pM1.GetInRoundMoney()))
                            {
                                lImport = GetRoundUpMoney(lCandidateMaxMoney);
                            }
                            else
                            {
                                lImport = lCandidateMaxMoney;
                            }

                            pM1.SetOutMaxImport(lImport);

                            if (m_bPostPay)
                            {
                                pM1.SetOutMinImport(lImport);
                            }
                        }

                        if (pM1.GetOutMaxImport() < pM1.GetOutMinImport())
                        {         
                            pM1.SetOutMaxImport(pM1.GetOutMinImport());
                        }

                        pM1.SetOutHistPostPay(m_bHistPostPay);

                        if (m_bTimeLimitsCompute)
                        {
                            pM1.SetOutMinTime(m_lFirstAllowedAmountRealMinutes);
                            pM1.SetOutMinOperDate(GetFirstAllowedAmountEndDate());

                            if (m_lRealCurrMinutes >= m_lFirstAllowedAmountRealMinutes)
                            {
                                pM1.SetOutMaxTime(m_lRealCurrMinutes);
                                if ((!m_bRoundEndOfDay) && (bBackupCurrentAmountIsAllowed))
                                {
                                    dtOper = GetFinalDateTime().Copy();
                                }
                                else if ((!m_bRoundEndOfDay) && (!bBackupCurrentAmountIsAllowed))
                                {
                                    if (!bM1Plus)
                                    {
                                        dtOper = GetLastAllowedAmountEndDate().Copy();
                                    }
                                    else
                                    {
                                        dtOper = GetFinalDateTime().Copy();
                                    }

                                }
                                else
                                {
                                    dtOper = GetRoundEndOfDayDateTime().Copy();
                                }
                                pM1.SetOutMaxOperDate(dtOper);
                            }
                            else
                            {
                                pM1.SetOutMaxTime(m_lFirstAllowedAmountRealMinutes);
                                pM1.SetOutMaxOperDate(GetFirstAllowedAmountEndDate());
                            }

                            if (pM1.GetInMinEqMax())
                            {
                                pM1.SetOutMinImport(pM1.GetOutMaxImport());
                                pM1.SetOutMinTime(pM1.GetOutMaxTime());
                                pM1.SetOutMinOperDate(pM1.GetOutMaxOperDate());
                            }


                        }

                        pM1.SetOutRemainingMinutesWithZeroValue(m_iRemainingMinutesWithZeroValue);
                        pM1.SetCurrentAmountIsAllowed(m_bCurrentAmountIsAllowed);

                        pM1.SetOutAddFreeMinutesQuantity(m_lOutAddFreeMinutesQuantity);
                        pM1.SetOutAddFreeMoneyQuantity(m_lOutAddFreeMoneyQuantity);

                        break;

                    case OperationDat.DEF_OPERTYPE_PARK:

                        dtOper = GetInitialDateTime().Copy();
                        pM1.SetOutOperDateIni0(dtOper);
                        pM1.SetOutOperDateIni(dtOper);
                        bBackupCurrentAmountIsAllowed = m_bCurrentAmountIsAllowed;

                        if ((!m_bRoundEndOfDay) && (m_bCurrentAmountIsAllowed))
                        {
                            dtOper = GetFinalDateTime().Copy();
                            pM1.SetOutOperDateEnd(dtOper);
                        }
                        else if ((!m_bRoundEndOfDay) && (!m_bCurrentAmountIsAllowed))
                        {
                            if (!bM1Plus)
                            {
                                if (m_lLastAllowedAmountRealMinutes != GlobalDefs.DEF_UNDEFINED_VALUE)
                                {
                                    pM1.SetOutOperDateEnd(GetLastAllowedAmountEndDate());
                                    m_lRealCurrMinutes = m_lLastAllowedAmountRealMinutes;
                                    m_lCurrMinutes = m_lLastAllowedAmountMinutes;
                                    m_lRealCurrMoney = m_lLastAllowedAmountRealMoney;
                                    m_lCurrMoney = m_lLastAllowedAmountMoney;
                                    m_bCurrentAmountIsAllowed = true;
                                }
                                else
                                {
                                    m_lResult = M1_OP_NOK;
                                    pM1.SetOutResult((int)m_lResult);
                                }
                            }
                            else
                            {
                                if ((m_lLastAllowedAmountRealMinutes != GlobalDefs.DEF_UNDEFINED_VALUE) &&
                                    (m_lRealCurrMoney == m_lLastAllowedAmountRealMoney))
                                {
                                    pM1.SetOutOperDateEnd(GetLastAllowedAmountEndDate());
                                    m_lRealCurrMinutes = m_lLastAllowedAmountRealMinutes;
                                    m_lCurrMinutes = m_lLastAllowedAmountMinutes;
                                    m_lRealCurrMoney = m_lLastAllowedAmountRealMoney;
                                    m_lCurrMoney = m_lLastAllowedAmountMoney;
                                    m_bCurrentAmountIsAllowed = true;
                                }

                            }

                        }
                        else
                        {
                            pM1.SetOutOperDateEnd(GetRoundEndOfDayDateTime());
                            m_lRealCurrMinutes = m_lRoundEndOfDayRealMinutes;
                            m_lCurrMinutes = m_lRoundEndOfDayMinutes;
                        }

                        long lMinValue2 = GetMinMoney();
                        if ((GetPaymentType() != OperationDat.DEF_OPERPAY_MOBILE) && (pM1.GetInRoundMoney()))
                        {
                            pM1.SetOutMinImport(GetRoundMoney(lMinValue2));
                        }
                        else
                        {
                            pM1.SetOutMinImport(lMinValue2);
                        }

                        long lMaxValue2 = GetMaxMoney();
                        if ((bM1Plus) && (GeneralParams.UnitId == GlobalDefs.CC_UNIT_ID.ToString()) && (m_lTimeLimit != GlobalDefs.DEF_UNDEFINED_VALUE))
                        {
                            lMaxValue2 = m_lRealCurrMoney;
                        }

                        if ((GetPaymentType() != OperationDat.DEF_OPERPAY_MOBILE) && (pM1.GetInRoundMoney()))
                        {
                            pM1.SetOutMaxImport(GetRoundUpMoney(lMaxValue2));
                        }
                        else
                        {
                            pM1.SetOutMaxImport(lMaxValue2);
                        }

                        pM1.SetOutRealAccumulateMoney(m_lRealAccMoney);
                        pM1.SetOutRealAccumulateTime((int)m_lRealAccMinutes);
                        pM1.SetOutRealMinutes(m_lRealCurrMinutes);
                        pM1.SetOutOperDateRealIni(GetOperDateRealIniDateTime());
                        pM1.SetOutWholeOperationWithChipCard(m_iWholeOperationWithChipCard);
                        pM1.SetOutWholeOperationWithMobile(m_iWholeOperationWithMobile);

                        // ACCUMULATED MONEY (FOR THE NEXT OPERATION)
                        pM1.SetOutAccumulateMoney(m_stInitialIntervalOffset.iMoney);

                        // ACCUMULATED TIME (FOR THE NEXT OPERATION)
                        pM1.SetOutAccumulateTime((int)m_stInitialIntervalOffset.iTime);

                        // TIME CONSUMED IN THE CURRENT OPERATION
                        pM1.SetOutEfMaxTime((int)m_lCurrMinutes);

                        if (!bM1Plus)
                        {
                            pM1.SetOutHistFreeMoneyUsed(m_bHistFreeMoneyUsed);

                            long lCandidateMinMoney = GetMinMoney();

                            if ((GetPaymentType() != OperationDat.DEF_OPERPAY_MOBILE) && (pM1.GetInRoundMoney()))
                            {
                                pM1.SetOutMinImport(GetRoundMoney(lCandidateMinMoney));
                            }
                            else
                            {
                                pM1.SetOutMinImport(lCandidateMinMoney);
                            }

                            long lCandidateMaxMoney = m_lRealCurrMoney;
                            if ((GetPaymentType() != OperationDat.DEF_OPERPAY_MOBILE) && (pM1.GetInRoundMoney()))
                            {
                                lImport = GetRoundUpMoney(lCandidateMaxMoney);
                            }
                            else
                            {
                                lImport = lCandidateMaxMoney;
                            }

                            pM1.SetOutMaxImport(lImport);

                            if (m_bPostPay)
                            {
                                pM1.SetOutMinImport(lImport);
                            }
                        }

                        if (pM1.GetOutMaxImport() < pM1.GetOutMinImport())
                        {
                            pM1.SetOutMaxImport(pM1.GetOutMinImport());
                        }

                        pM1.SetOutHistPostPay(m_bHistPostPay);

                        if (m_bTimeLimitsCompute)
                        {
                            pM1.SetOutMinTime(m_lFirstAllowedAmountRealMinutes);
                            pM1.SetOutMinOperDate(GetFirstAllowedAmountEndDate());

                            if (m_lRealCurrMinutes >= m_lFirstAllowedAmountRealMinutes)
                            {
                                pM1.SetOutMaxTime(m_lRealCurrMinutes);
                                if ((!m_bRoundEndOfDay) && (bBackupCurrentAmountIsAllowed))
                                {
                                    dtOper = GetFinalDateTime().Copy();
                                }
                                else if ((!m_bRoundEndOfDay) && (!bBackupCurrentAmountIsAllowed))
                                {
                                    if (!bM1Plus)
                                    {
                                        dtOper = GetLastAllowedAmountEndDate().Copy();
                                    }
                                    else
                                    {
                                        dtOper = GetFinalDateTime().Copy();
                                    }
                                }
                                else
                                {
                                    dtOper = GetRoundEndOfDayDateTime().Copy();
                                }
                                pM1.SetOutMaxOperDate(dtOper);
                            }
                            else
                            {
                                pM1.SetOutMaxTime(m_lFirstAllowedAmountRealMinutes);
                                pM1.SetOutMaxOperDate(GetFirstAllowedAmountEndDate());
                            }

                            if (pM1.GetInMinEqMax())
                            {
                                pM1.SetOutMinImport(pM1.GetOutMaxImport());
                                pM1.SetOutMinTime(pM1.GetOutMaxTime());
                                pM1.SetOutMinOperDate(pM1.GetOutMaxOperDate());
                            }


                        }
                        pM1.SetOutRemainingMinutesWithZeroValue(m_iRemainingMinutesWithZeroValue);
                        pM1.SetCurrentAmountIsAllowed(m_bCurrentAmountIsAllowed);
                        pM1.SetOutAddFreeMinutesQuantity(m_lOutAddFreeMinutesQuantity);
                        pM1.SetOutAddFreeMoneyQuantity(m_lOutAddFreeMoneyQuantity);

                        break;

                    case OperationDat.DEF_OPERTYPE_RETN:
                        long lRetMoney = 0;
                        stINTERVAL stIntervalOffset;
                        stIntervalOffset.iMoney = stIntervalOffset.iTime = 0;

                        if (m_stIntervalOffset.iMoney != GlobalDefs.DEF_UNDEFINED_VALUE)
                            stIntervalOffset.iMoney = m_stIntervalOffset.iMoney;

                        if ((m_lRealCurrMoney + stIntervalOffset.iMoney) < GetMinMoney())
                        {
                            lRetMoney = pM1.GetOutAccumulateMoney() - GetMinMoney();
                        }
                        else
                        {
                            lRetMoney = pM1.GetOutAccumulateMoney() - m_lRealCurrMoney;
                        }

                        //GetDateTime( &dtOper );
                        dtOper = GetFirstDateTime().Copy();
                        pM1.SetOutOperDateIni0(dtOper);
                        trace.Write(TraceLevel.Info, $"RETURN OPERATION -> OperDateIni0: {dtOper.fstrGetTraceString()}");

                        dtOper = GetFinalDateTime().Copy();
                        pM1.SetOutOperDateEnd(dtOper);
                        trace.Write(TraceLevel.Info, $"RETURN OPERATION -> OperDateEnd: {dtOper.fstrGetTraceString()}");

                        dtOper = GetInitialDateTime().Copy();
                        trace.Write(TraceLevel.Info, $"RETURN OPERATION -> InitialDateTime: {dtOper.fstrGetTraceString()}");

                        pM1.SetOutOperDateIni(dtOper);
                        trace.Write(TraceLevel.Info, $"RETURN OPERATION -> OperDateIni: {dtOper.fstrGetTraceString()}");

                        // MONEY CONSUMED IN THE CURRENT OPERATION
                        // ACCUMULATED MONEY (FOR THE NEXT OPERATION)
                        pM1.SetOutRetImport(lRetMoney);
                        pM1.SetOutAccumulateMoney(pM1.GetOutAccumulateMoney() - lRetMoney);
                        // TIME CONSUMED IN THE CURRENT OPERATION
                        pM1.SetOutEfMaxTime((int)m_lCurrMinutes);
                        // ACCUMULATED TIME (FOR THE NEXT OPERATION)
                        pM1.SetOutAccumulateTime((int)m_lCurrMinutes);
                        pM1.SetOutRemainingMinutesWithZeroValue(m_iRemainingMinutesWithZeroValue);

                        break;
                }

                if ((m_bCalculateTimeSteps) || (m_bCalculateQuantitySteps))
                {

                    stStepCalculation pstStepCalculation = new stStepCalculation();

                    if (m_CalculateSteps_Steps.Count == 0)
                    {
                        pstStepCalculation = new stStepCalculation
                        {
                            lMoney = pM1.GetOutMaxImport(),
                            lMinutes = pM1.GetOutRealMinutes(),
                            dtDate = pM1.GetOutMaxOperDate().Copy()
                        };

                        m_CalculateSteps_Steps.Add(pstStepCalculation);
                    }
                    else
                    {
                        stStepCalculation pLastStepCalculation = m_CalculateSteps_Steps.Last();
                        if (pM1.GetOutMaxImport() == pLastStepCalculation.lMoney)
                        {
                            pLastStepCalculation.lMinutes = pM1.GetOutRealMinutes();
                            pLastStepCalculation.dtDate = pM1.GetOutMaxOperDate().Copy();
                        }
                        else if (pM1.GetOutRealMinutes() == pLastStepCalculation.lMinutes)
                        {
                            pLastStepCalculation.lMoney = pM1.GetOutMaxImport();
                            pLastStepCalculation.dtDate = pM1.GetOutMaxOperDate().Copy();
                        }
                        else
                        {
                            pstStepCalculation = new stStepCalculation
                            {
                                lMoney = pM1.GetOutMaxImport(),
                                lMinutes = pM1.GetOutRealMinutes(),
                                dtDate = pM1.GetOutMaxOperDate().Copy()
                            };

                            m_CalculateSteps_Steps.Add(pstStepCalculation);
                        }
                    }

                }

                if (m_CalculateSteps_Steps != null && m_CalculateSteps_Steps.Count != 0)
                {
                    int i = 1;
                    foreach (stStepCalculation step in m_CalculateSteps_Steps)
                    {
                        if (!bM1Plus)
                        {
                            if ((m_bCalculateTimeSteps) || (m_bCalculateQuantitySteps))
                            {
                                pM1.AddStepToStepCalculationString(
                                    i,
                                    step.lMoney,
                                    step.lMinutes,
                                    step.dtDate);
                                i++;
                            }
                        }
                    }

                    m_CalculateSteps_Steps.Clear();
                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            pM1.Trace();

            return fnResult;

        }

        private long GetRoundUpMoney(long lMoney)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::GetRoundUpMoney");
            long fnResult = 0;

            try
            {
                if (GetMinCoinValue() != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    long mod = lMoney % GetMinCoinValue();
                    long div = lMoney / GetMinCoinValue();

                    if (mod != 0)
                    {
                        fnResult = (div + 1) * GetMinCoinValue();
                    }
                    else
                    {
                        fnResult = lMoney;
                    }
                }
                else
                {
                    fnResult = lMoney;
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return fnResult;
        }

        private bool ApplyHistory()
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::ApplyHistory");
            bool fnResult = true;
            int iHistoryRes = (int)M1GroupState.GRP_ON;
            bool bTimeReset = false;
            bool bLessMaxInterdateReeTime = false;
            COPSDate dtBackOpIni = new COPSDate();
            COPSDate dtBackOpEnd = new COPSDate();
            bool bDiscardOp = false;
            bool bChainedOperation = false;
            COPSDate dtRealOpIni = new COPSDate();
            COPSDate dtRealOpEnd = new COPSDate();
            COPSDate dtBakRealOpIni = new COPSDate();
            COPSDate dtBakRealOpEnd = new COPSDate();
            long lBackMoney;
            long lRealAccTime = 0;
            long lRealAccMoney = 0;
            bool bExit = false;
            bool bRemoteReturn = false;
            long lRemoteReturnPaymentType = GlobalDefs.DEF_UNDEFINED_VALUE;
            long lInitialTimeLastInterval = 0;
            long lEndTimeLastInterval = 0;
            long lCurrentTimeLastInterval = 0;

            try
            {
                bool shouldApplyCurrentOperation = GetTree().ApplyCurrentOperation(m_lGroupId, GetDateTime());
                if (!shouldApplyCurrentOperation)
                {
                    throw new InvalidOperationException("Could not apply current operation");
                }

                dtBackOpIni.SetStatus(COPSDateStatus.Invalid);
                dtBackOpEnd.SetStatus(COPSDateStatus.Invalid);

                dtBakRealOpIni.SetStatus(COPSDateStatus.Invalid);
                dtBakRealOpEnd.SetStatus(COPSDateStatus.Invalid);

                dtBakRealOpIni = GetDateTime().Copy();

                IEnumerable<Operations> operations = null;
                if (!m_pDBB.GetVehicleOperations(m_strVehicleId.ToString(), m_lArticleDef, ref operations, mustSort: true))
                {
                    throw new InvalidOperationException("FAILED call to m_pPDMDB->GetVehicleOperations");
                }

                trace.Write(TraceLevel.Info, $"Found {operations.Count()} operations for vehicle {m_strVehicleId.ToString()} and type of article {m_lArticleDef}");

                if (operations.Count() > 0) {
                    OperationTraceRowHeader();
                    foreach (Operations operationLog in operations)
                    {
                        OperationTraceRow(operationLog);
                    }
                }

                bool bAmpliationIsAllowed = true;
                GetTree().AmpliationIsAllowed(m_lGroupId, ref bAmpliationIsAllowed);

                int operationsIndex = 0;
                int operationsCount = operations.Count();
                Operations operation;

                // We must evaluate operations until a complete branch does not accept any operations or the
                // operations for the current vehicle has ended
                while (operationsIndex < operationsCount && !bExit)
                {
                    operation = operations.ElementAt(operationsIndex);

                    m_bHistOpCourtesy = false;
                    m_bHistOpPostPay = false;
                    bChainedOperation = false;
                    lBackMoney = operation.OPE_VALUE.GetValueOrDefault();

                    if (m_bHistOnlyWithSamePaymentType && (operation.OPE_DPAY_ID.GetValueOrDefault() != m_iPaymentType))
                    {
                        bExit = true;
                    }
                    //TODO: 14-12-2018 Usar IsIn de los metodos de extension de Enum
                    else if (operation.OPE_DOPE_ID == OperationDat.DEF_OPERTYPE_PARK || operation.OPE_DOPE_ID == OperationDat.DEF_OPERTYPE_AMP || operation.OPE_DOPE_ID == OperationDat.DEF_OPERTYPE_RETN) {
                        long lOpTypeBak = operation.OPE_DOPE_ID.Value;

                        if (bRemoteReturn) {
                            bRemoteReturn = (operation.OPE_DOPE_ID.GetValueOrDefault() == OperationDat.DEF_OPERTYPE_PARK || operation.OPE_DOPE_ID == OperationDat.DEF_OPERTYPE_AMP) && operation.OPE_DPAY_ID == lRemoteReturnPaymentType;
                        }

                        if (!bRemoteReturn) {

                            dtRealOpIni = operation.OPE_INIDATE.Copy();
                            dtRealOpEnd = operation.OPE_ENDDATE.Copy();

                            if (m_lFirstGroupId == GlobalDefs.DEF_UNDEFINED_VALUE) {
                                m_lFirstGroupId = operation.OPE_GRP_ID.GetValueOrDefault();
                            }
                            
                            if (!CorrectOperationData(GetTree(), m_strVehicleId, m_lArticleDef, ref dtBackOpIni, ref dtBackOpEnd, ref operation,  
                                                      ref lRealAccMoney, ref lRealAccTime, ref bTimeReset, ref bLessMaxInterdateReeTime, ref bDiscardOp, ref lInitialTimeLastInterval, 
                                                      ref m_lEndTimeLastInterval, ref lCurrentTimeLastInterval)) {
                                throw new InvalidOperationException("Error subdividing operation in suboperations");
                            }

                            if (!bDiscardOp && (operation.OPE_INIDATE < operation.OPE_ENDDATE))
                            {
                                trace.Write(TraceLevel.Info, $"Checking Corrected Operation: {operation.OPE_ID}");
                                trace.Write(TraceLevel.Info, $"[ {"Operation",-9} | {"Date",-21} | {"Op. Type",-8} | {"Group",-7} | {"IniDate",-21} | {"EndDate",-21} | {"Op.Money",-8} | {"Op.Time",-8} | {"Pay. Tipe",-8} | {"Time Reset",-9} | {"Less Max Ree Time",-21} ]");
                                trace.Write(TraceLevel.Info, $"[ {operation.OPE_ID,-9} | {operation.OPE_MOVDATE.fstrGetTraceString(),-21} | {operation.OPE_DOPE_ID,-8} | {operation.OPE_GRP_ID,-7} | {operation.OPE_INIDATE.fstrGetTraceString(),-21} | {operation.OPE_ENDDATE.fstrGetTraceString(),-21} | {operation.OPE_VALUE,-8} | {operation.OPE_DURATION,-8} | {operation.OPE_DPAY_ID,-8} | {bTimeReset,-9} | {bLessMaxInterdateReeTime,-21} ]");

                                if (iHistoryRes == (int)M1GroupState.GRP_ON)
                                {
                                    // Apply history to all the branch
                                    if (!ApplyHistoryBranch(GetTree(), operation, bLessMaxInterdateReeTime,ref  bChainedOperation))
                                    {
                                        throw new InvalidOperationException("Error applying history branch");
                                    }

                                    // Get the state of all the groups of the branch of the current operation
                                    if (!GetTree().CheckHistoryState(GetGroupId(), ref iHistoryRes))
                                    {
                                        throw new InvalidOperationException("Could not obtain history state");
                                    }

                                    if (iHistoryRes == (int)M1GroupState.GRP_ON && bTimeReset && bChainedOperation)
                                    {
                                        iHistoryRes = (int)M1GroupState.GRP_STOP;
                                    }
                                }


                                if ((GetOperType() == OperationDat.DEF_OPERTYPE_AMP) && (!bAmpliationIsAllowed))
                                {
                                    bExit = true;
                                    m_lResult = M1_OP_TPERM;
                                }
                                else
                                {
                                    bExit = (iHistoryRes != (int)M1GroupState.GRP_ON && iHistoryRes != (int)M1GroupState.GRP_STOP);
                                    if ((!bExit) && (dtBakRealOpIni.GetStatus() == COPSDateStatus.Valid))
                                    {
                                        bExit = ((dtRealOpEnd < dtBakRealOpIni) && (!m_bHistOpCourtesy) && (!m_bHistOpPostPay));
                                    }

                                    if ((!bExit) && (bChainedOperation))
                                    {

                                        GetTree().GetBranchAccumMoney(GetGroupId(), ref m_lRealAccMoney);
                                        GetTree().GetBranchAccumMinutes(GetGroupId(), ref m_lRealAccMinutes);

                                       SetOperDateRealIniDateTime(dtRealOpIni);

                                        dtBakRealOpIni = dtRealOpIni.Copy();
                                        dtBakRealOpEnd = dtRealOpEnd.Copy();

                                        m_iWholeOperationWithChipCard = ((m_iWholeOperationWithChipCard == 1) && (operation.OPE_DPAY_ID.GetValueOrDefault() == (long)OperationDat.DEF_OPERPAY_CHIPCARD)) ? 1 : 0;
                                        m_iWholeOperationWithMobile = ((m_iWholeOperationWithMobile == 1) && (operation.OPE_DPAY_ID.GetValueOrDefault() == OperationDat.DEF_OPERPAY_MOBILE)) ? 1: 0;
                                        long lRealQuantity = m_pDBB.GetActualPayedQuantity(operation.OPE_ID);
                                        if (lRealQuantity != GlobalDefs.DEF_UNDEFINED_VALUE)
                                        {
                                            m_bHistFreeMoneyUsed = m_bHistFreeMoneyUsed || (lRealQuantity < lBackMoney);
                                        }

                                        if ((operation.OPE_POST_DAY.GetValueOrDefault() == 1) && (GetOperType() == OperationDat.DEF_OPERTYPE_AMP))
                                        {
                                            m_bHistPostPay = true;
                                            bExit = true;
                                            m_lResult = M1_OP_TPERM;
                                        }
                                        else
                                        {

                                            if (operationsIndex == 0)
                                            {
                                                m_lInitialTimeLastInterval = lInitialTimeLastInterval;
                                                m_lEndTimeLastInterval = lEndTimeLastInterval;
                                                m_lCurrentTimeLastInterval = lCurrentTimeLastInterval;
                                            }
                                        }
                                    }

                                    if ((!bExit) && ((operation.OPE_OP_ONLINE == 0) || (lOpTypeBak == OperationDat.DEF_OPERTYPE_RETN)))
                                    {
                                        bExit = true;
                                    }
                                }
                            }
                        }

                        if (!bRemoteReturn)
                        {
                            bRemoteReturn = (
                                                (lOpTypeBak == OperationDat.DEF_OPERTYPE_RETN) &&
                                                (
                                                    operation.OPE_DPAY_ID.GetValueOrDefault() == OperationDat.DEF_OPERPAY_CHIPCARD || 
                                                    operation.OPE_DPAY_ID.GetValueOrDefault() == OperationDat.DEF_OPERPAY_MOBILE
                                                )
                                            );
                            lRemoteReturnPaymentType = operation.OPE_DPAY_ID.GetValueOrDefault();
                        }
                    }

                    if (bExit)
                    {
                        break;
                    }

                    operationsIndex++;
                }

                if (iHistoryRes == (int)M1GroupState.GRP_STOP)
                {
                    // No more operations affect to the operation in process, and tariff can be computed
                    trace.Write(TraceLevel.Info, "STOP");
                }
                else if (iHistoryRes == (int)M1GroupState.GRP_REE)
                {
                    // No more operations must be evaluated as we have found an operation that causes 
                    // that we can't compute tariff
                    trace.Write(TraceLevel.Info, "REE");
                    m_lResult = M1_OP_TREENT;
                }

                GetTree().TraceBranchM1ComputeEx0(GetGroupId());
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
        private bool ApplyHistoryBranch(CM1GroupsTree tree, Operations operation, bool bLessMaxInterdateReeTime,ref  bool bChainedOperation)
        {
            trace.Write(TraceLevel.Debug, "CM1ComputeEx0::ApplyHistoryBranch");
            bool fnResult = true;

            try
            {
                Guard.IsNull(tree, nameof(tree), "Could not obtaion tree");
                Guard.IsNull(operation.OPE_INIDATE, nameof(operation.OPE_INIDATE));
                Guard.IsNull(operation.OPE_ENDDATE, nameof(operation.OPE_ENDDATE));

                CM1Group group = tree.GetGroupFromGrpId(operation.OPE_GRP_ID.GetValueOrDefault());
                if (group == null)
                {
                    throw new InvalidOperationException("Group of operation not found in tree");
                }

                M1GroupState state = (M1GroupState)group.GetState();
                state.IsIn(M1GroupState.GRP_ON, M1GroupState.GRP_REE);

                if (group.GetState() == (long)M1GroupState.GRP_ON)
                {
                    if (!EvaluateHistoryConstraints(group, operation, bLessMaxInterdateReeTime,ref bChainedOperation))
                    {
                        throw new InvalidOperationException("Error evaluating group");
                    }

                    while ((group != null) && (group.GetState() != (long)M1GroupState.GRP_REE))
                    {
                        long lGrpId = group.GetGrpId();
                        group = tree.GetGroupParent(lGrpId);

                        if (group == null)
                            trace.Write(TraceLevel.Error, $"Group of operation ({lGrpId}) has no parents in tree");
                        else
                        {
                            trace.Write(TraceLevel.Info, $"\tFound group parent id {group.GetGrpId()} for group {lGrpId}");

                            if (group.GetState() == (long)M1GroupState.GRP_ON)
                            {
                                if (!EvaluateHistoryConstraints(group, operation, bLessMaxInterdateReeTime,ref  bChainedOperation))
                                {
                                    throw new InvalidOperationException("Error evaluating group");
                                }
                            }
                            else
                            {
                                trace.Write(TraceLevel.Info, $"Not evaluating group {group.GetGrpId()}, state {group.GetState()}:{ Enum.GetName(typeof(M1GroupState), group.GetState())}");
                            }
                        }
                    }
                }
                else
                {
                    trace.Write(TraceLevel.Info, $"Not evaluating group {group.GetGrpId()}, state {group.GetState()}:{ Enum.GetName(typeof(M1GroupState), group.GetState())}");
                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
        private bool ApplyRemoteHistory(CM1GroupsTree tree)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::ApplyRemoteHistory");
            bool fnResult = true;

            CM1Group pParentGrp = null;
            CM1Group pWorkGrp = null;

            try
            {
                Guard.IsNull(tree, nameof(tree), "Could not obtain tree");

                // Get the working group to add accumulated money
                pWorkGrp = tree.GetGroupFromGrpId(GetGroupId());

                if (pWorkGrp == null)
                {
                    trace.Write(TraceLevel.Error, "Group does not exist in tree");
                }
                else
                {
                    // Add the accumlated money/time in group
                    pWorkGrp.AddMoney(m_lAccMoney);
                    pWorkGrp.AddTime(m_lAccMinutes);
                    // Get the working group to add accumulated money in all the groups of
                    // the parent of the working group
                    pParentGrp = tree.GetGroupParent(GetGroupId());

                    if (pParentGrp == null)
                    {
                        trace.Write(TraceLevel.Error, "Group does not have a parent");
                    }
                    else
                    {
                        // Add the accumlated money/time in children
                        pParentGrp.AddMoney(m_lAccMoneyGroup);
                        pParentGrp.AddTime(m_lAccMinutesGroup);
                    }
                }

                tree.TraceBranchM1ComputeEx0(GetGroupId());
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group">[In] Pointer to the group to evaluate (from the tree of groups)</param>
        /// <param name="operation">[In] Operation Record</param>
        /// <param name="bLessMaxInterdateReeTime"></param>
        /// <param name="bChainedOperation"></param>
        /// <returns></returns>
        private bool EvaluateHistoryConstraints(CM1Group group, Operations operation, bool bLessMaxInterdateReeTime,ref  bool bChainedOperation)
        {
            trace.Write(TraceLevel.Debug, "CM1ComputeEx0::EvaluateHistoryConstraints");
            bool fnResult = true;

            bool bCourtesy = false;
            bool bReentry = false;
            bool bIntraZonePark = false;
            bool bIntraZoneCourtesy = false;
            bool bPostPay = false;
            COPSDate dtLastOper = new COPSDate();
            COPSDate dtFirstOper = new COPSDate();
            bool bTimeReset = false;
            long lPostPayTime = 0;
            bool bCalculateReentry = false;
            bool bHistOpPostPay;

            try
            {
                Guard.IsNull(group, nameof(group));
                Guard.IsNull(operation.OPE_INIDATE, nameof(operation.OPE_INIDATE));
                Guard.IsNull(operation.OPE_ENDDATE, nameof(operation.OPE_ENDDATE));

                trace.Write(TraceLevel.Info, $"Evaluating group: {group.GetGrpId()}");

                bool bGroupInCurrTree = GetTree().IsGroupInTree(group.GetGrpId(), GetGroupId());
                bool bOpGroupInGroupTree = GetTree().IsGroupInTree(GetGroupId(), operation.OPE_GRP_ID.GetValueOrDefault());
                bool bOpGroupInFirstGroupTree = GetTree().IsGroupInTree(operation.OPE_GRP_ID.GetValueOrDefault(), m_lFirstGroupId);

                float fIntraZoneParkException = CM1Constraint.CNSTR_UNDEFINED;
                if (!group.GetConstraint(CM1Constraint.CNSTR_INTRA_ZONE_PARK_EXCEPTION, ref fIntraZoneParkException))
                {
                    throw new InvalidOperationException("Could not get IntraZone Parking Exception of group");
                }

                if (fIntraZoneParkException != CM1Constraint.CNSTR_UNDEFINED)
                {
                    m_bIntraZoneParkException = (fIntraZoneParkException == 1);
                }

                bHistOpPostPay = operation.OPE_POST_DAY.GetValueOrDefault() == 1;

                if ((bGroupInCurrTree) && (group.GetState() == (long)M1GroupState.GRP_ON))
                {
                    // Get the last date of the group of the operation to evaluate
                    COPSDate dtGrpLast = group.GetLastDate().Copy();
                    if (dtGrpLast == null)
                    {
                        throw new InvalidOperationException("Could not get last date of group");
                    }

                    trace.Write(TraceLevel.Info, $"Last date of group: {dtGrpLast.fstrGetTraceString()}");
                    trace.Write(TraceLevel.Info, $"Initial date of operation: {operation.OPE_INIDATE.fstrGetTraceString()}");
                    trace.Write(TraceLevel.Info, $"Final date of operation: {operation.OPE_ENDDATE.fstrGetTraceString()}");

                    if (!ExistTimeResetBetweenDates(operation.OPE_ENDDATE, dtGrpLast, ref bTimeReset))
                    {
                        throw new InvalidOperationException("Error looking for time Reset");
                    }


                    if (!bOpGroupInGroupTree)
                    {
                        if (!EvaluateIntraZonePark(group, dtGrpLast, operation.OPE_ENDDATE, bTimeReset, ref bIntraZonePark))
                        {
                            throw new InvalidOperationException("Error evaluating Intra Zone Parking");
                        }
                    }

                    if (!bIntraZonePark)
                    {
                        bIntraZonePark = m_bIntraZoneParkException;
                    }

                    if (!bIntraZonePark && !bOpGroupInGroupTree)
                    {
                        group.SetState((long)M1GroupState.GRP_REE);
                        trace.Write(TraceLevel.Info, "GRP_REE");
                    }
                    else
                    {
                        if (!bOpGroupInGroupTree)
                        {
                            if (!EvaluateIntraZoneCourtesy(group, dtGrpLast, operation.OPE_ENDDATE, bTimeReset, ref bIntraZoneCourtesy))
                            {
                                throw new InvalidOperationException("Error evaluating Intra Zone courtesy time");
                            }
                        }

                        if (!EvaluateCourtesy(group, dtGrpLast, operation.OPE_ENDDATE, bTimeReset, ref bCourtesy))
                        {
                            throw new InvalidOperationException("Error evaluating courtesy time");
                        }

                        if (!EvaluatePostPayTime(group, dtGrpLast, operation.OPE_ENDDATE, bTimeReset, ref bPostPay, ref lPostPayTime))
                        {
                            throw new InvalidOperationException("Error evaluating post pay time");
                        }

                        if ((bIntraZoneCourtesy) || (bOpGroupInGroupTree))
                        {
                            if (((bPostPay) && (!bCourtesy)) || (m_bPostPay))
                            {
                                long lPostPayMinutes = Math.Max(m_lPostPayMinutes, lPostPayTime);
                                long lNewTime = group.GetAccMinutes() + operation.OPE_DURATION.GetValueOrDefault() + lPostPayMinutes;
                                if ((group.GetMaxMinutes() != GlobalDefs.DEF_UNDEFINED_VALUE) && (lNewTime > group.GetMaxMinutes()))
                                {
                                    group.SetState((long)M1GroupState.GRP_REE);
                                    trace.Write(TraceLevel.Info, "GRP_REE");
                                }
                                else
                                {
                                    m_lPostPayMinutes = lPostPayMinutes;
                                    m_bPostPay = true;
                                    m_bHistOpPostPay = bPostPay;
                                }

                            }

                            if (((bCourtesy) || (bPostPay)) &&/*(!bHistOpPostPay)&&*/(group.GetState() == (long)M1GroupState.GRP_ON))
                            {
                                m_bHistOpCourtesy = bCourtesy;
                                if (operation.OPE_DOPE_ID.GetValueOrDefault() == OperationDat.DEF_OPERTYPE_RETN)
                                {
                                    // If the operation type is a return, money and time must be substracted
                                    // not added
                                    operation.OPE_VALUE = operation.OPE_VALUE.GetValueOrDefault() * (-1);
                                    operation.OPE_DURATION = operation.OPE_DURATION.GetValueOrDefault() * (-1);
                                }


                                // When a valid previous operation is found, then the operation type changes to
                                // ampliation
                                SetOperType(OperationDat.DEF_OPERTYPE_AMP);
                                // If the current operation is below the end of the last operation plus the courtesy
                                // time then we assume that the current operation is an ampliation of the last
                                // If so, the last operation is set as the current operation and all the values of
                                // money and minutes are added to the group
                                trace.Write(TraceLevel.Info, $"Adding money: {operation.OPE_VALUE}");
                                group.AddMoney(operation.OPE_VALUE.GetValueOrDefault());
                                trace.Write(TraceLevel.Info, $"Adding minutes: {operation.OPE_DURATION}");
                                group.AddTime(operation.OPE_DURATION.GetValueOrDefault());
                                group.SetState((long)M1GroupState.GRP_ON);
                                group.SetLastDate(operation.OPE_INIDATE);
                                trace.Write(TraceLevel.Info, $"New last date of group: {operation.OPE_INIDATE.fstrGetTraceString()}");
                                // If the history must be applied, and this operation is an ampliation of a previous
                                // one, we must set the operation initial date/time to the first valid operation
                                // in the history (descending ordered)
                                dtLastOper = GetInitialDateTime().Copy();

                                if (!dtLastOper.IsValid())
                                {
                                    float fCortesyPay = 1;
                                    group.GetConstraint(CM1Constraint.CNSTR_CORTESY_PAY, ref fCortesyPay);

                                    if (fCortesyPay == CM1Constraint.CNSTR_UNDEFINED)
                                        fCortesyPay = 1;

                                    if (fCortesyPay == 0 && (dtGrpLast > operation.OPE_ENDDATE))
                                    {
                                        SetInitialDateTime(dtGrpLast);
                                        trace.Write(TraceLevel.Info, $"Setting operation date to: {dtGrpLast.fstrGetTraceString()}");
                                    }
                                    else
                                    {
                                        SetInitialDateTime(operation.OPE_ENDDATE);
                                        trace.Write(TraceLevel.Info, $"Setting operation date to: {operation.OPE_ENDDATE.fstrGetTraceString()}");
                                    }
                                }
                                else
                                {
                                    trace.Write(TraceLevel.Info, $"dtLastOper: {dtLastOper.fstrGetTraceString()}");
                                }


                                // If the history must be applied, and this operation is an ampliation of a previous
                                // one, we must set the first operation initial date/time to the last valid operation
                                // in the history (descending ordered). This is not used by the parking operation
                                // but by the return operation
                                SetFirstDateTime(operation.OPE_INIDATE);
                                trace.Write(TraceLevel.Info, $"Setting first operation date to: {operation.OPE_INIDATE.fstrGetTraceString()}");

                                bChainedOperation = true;
                            }
                        }


                        bCalculateReentry = ((!bIntraZoneCourtesy) && (!bOpGroupInGroupTree));
                        bCalculateReentry = bCalculateReentry || ((!bCourtesy) && (!m_bPostPay));
                        bCalculateReentry = bCalculateReentry || bHistOpPostPay;
                        bCalculateReentry = bCalculateReentry && (!m_bIntraZoneParkException);

                        if (bCalculateReentry)
                        {

                            if ((group.GetState() == (long)M1GroupState.GRP_ON) && (!bLessMaxInterdateReeTime))
                            {
                                // Is the current operation above the end of the last operation plus the reentry
                                // time ?
                                if (!EvaluateReentry(group, dtGrpLast, operation.OPE_ENDDATE, bTimeReset, bHistOpPostPay, ref bReentry))
                                    throw new InvalidOperationException("Error evaluating reentry time");

                                if (bReentry)
                                {
                                    // If the current operation is above the end of the last operation plus the courtesy
                                    // time then we have to check if the current operation is above the last
                                    // operation plus the reentry tiem. If so, we have to stop because the current
                                    // operation cannot be made.
                                    group.SetState((long)M1GroupState.GRP_REE);
                                    trace.Write(TraceLevel.Info, "GRP_REE");

                                    if (bHistOpPostPay)
                                    {
                                        m_bHistPostPay = true;
                                    }
                                }
                                else
                                {
                                    // If not, it means that we have to stop searching the history for this group
                                    // as the current operation does not interfere in the history
                                    group.SetState((long)M1GroupState.GRP_STOP);
                                    trace.Write(TraceLevel.Info, "GRP_STOP");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
        private bool EvaluateReentry(CM1Group group, COPSDate dtGrpLast, COPSDate pdtEnd, bool bTimeReset, bool bHistOpPostPay, ref bool bReentry)
        {
            trace.Write(TraceLevel.Debug, "CM1ComputeEx0::EvaluateReentry");
            bool fnResult = true;

            try
            {
                Guard.IsNull(group, nameof(group));
                Guard.IsNull(dtGrpLast, nameof(dtGrpLast));
                Guard.IsNull(pdtEnd, nameof(pdtEnd));

                trace.Write(TraceLevel.Info, $"Evaluating Reentry time for group: {group.GetGrpId()}");
                trace.Write(TraceLevel.Info, $"Last date of group: {dtGrpLast.fstrGetTraceString()}");
                trace.Write(TraceLevel.Info, $"Final date of operation: {pdtEnd.fstrGetTraceString()}");

                bReentry = false;

                if (dtGrpLast.IsValid() && dtGrpLast > pdtEnd)
                {
                    // Get courtesy time of the group of the operation to evaluate
                    float fReentryTime = GlobalDefs.DEF_UNDEFINED_VALUE;
                    if (!group.GetConstraint(CM1Constraint.CNSTR_REENTRY_TIME, ref fReentryTime))
                    {
                        throw new InvalidOperationException("Could not get Reentry time of group");
                    }

                    if (fReentryTime == CM1Constraint.CNSTR_UNDEFINED)
                    {
                        fReentryTime = 0;
                    }

                    trace.Write(TraceLevel.Info, $"Reentry time: {fReentryTime}");

                    COPSDate dtAddedDate = pdtEnd.Copy();
                    dtAddedDate.AddTimeSpan(new TimeSpan(0,(int)fReentryTime, 0));

                    trace.Write(TraceLevel.Info, $"Final date of operation plus Reentry time: {dtAddedDate.fstrGetTraceString()}, Exist Time Reset: {bTimeReset}");

                    bReentry = ((dtGrpLast <= dtAddedDate) && (!bTimeReset));

                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        private bool EvaluatePostPayTime(CM1Group group, COPSDate dtGrpLast, COPSDate pdtEnd, bool bTimeReset, ref bool bPostPay, ref long lPostPayTime)
        {

            trace.Write(TraceLevel.Debug, "CM1ComputeEx0::EvaluatePostPayTime");
            bool fnResult = true;

            try
            {
                Guard.IsNull(group, nameof(group));
                Guard.IsNull(dtGrpLast, nameof(dtGrpLast));
                Guard.IsNull(pdtEnd, nameof(pdtEnd));

                trace.Write(TraceLevel.Info, $"Evaluating Post Pay Time for group: {group.GetGrpId()}");
                trace.Write(TraceLevel.Info, $"Last date of group: {dtGrpLast.fstrGetTraceString()}");
                trace.Write(TraceLevel.Info, $"Final date of operation: {pdtEnd.fstrGetTraceString()}");

                bPostPay = false;

                if (dtGrpLast.IsValid() && dtGrpLast > pdtEnd)
                {
                    // Get courtesy time of the group of the operation to evaluate
                    float fPostPayTime = GlobalDefs.DEF_UNDEFINED_VALUE;
                    if (!group.GetConstraint(CM1Constraint.CNSTR_POSTPAY_TIME, ref fPostPayTime))
                    {
                        throw new InvalidOperationException("Could not get postpay time of group");
                    }

                    if (fPostPayTime == CM1Constraint.CNSTR_UNDEFINED)
                    {
                        fPostPayTime = 0;
                    }

                    lPostPayTime = (long)fPostPayTime;

                    trace.Write(TraceLevel.Info, $"Post Pay time: {fPostPayTime}");

                    COPSDate dtAddedDate = pdtEnd.Copy();
                    dtAddedDate.AddTimeSpan(new TimeSpan(0,(int)fPostPayTime, 0));

                    trace.Write(TraceLevel.Info, $"Final date of operation post pay time: {dtAddedDate.fstrGetTraceString()}, Exist Time Reset: {bTimeReset}");

                    bPostPay = ((dtGrpLast <= dtAddedDate) && (!bTimeReset));
                    
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        private bool EvaluateIntraZoneCourtesy(CM1Group group, COPSDate dtGrpLast, COPSDate pdtEnd, bool bTimeReset, ref bool bIntraZoneCourtesy)
        {
            trace.Write(TraceLevel.Debug, "CM1ComputeEx0::EvaluateIntraZoneCourtesy");
            bool fnResult = true;

            try
            {
                Guard.IsNull(group, nameof(group));
                Guard.IsNull(dtGrpLast, nameof(dtGrpLast));
                Guard.IsNull(pdtEnd, nameof(pdtEnd));

                trace.Write(TraceLevel.Info, $"Evaluating IntraZone courtesy time for group: {group.GetGrpId()}");
                trace.Write(TraceLevel.Info, $"Last date of group: {dtGrpLast.fstrGetTraceString()}");
                trace.Write(TraceLevel.Info, $"Final date of operation: {pdtEnd.fstrGetTraceString()}");

                bIntraZoneCourtesy = true;

                float fIntraZonePark = GlobalDefs.DEF_UNDEFINED_VALUE;
                if (!group.GetConstraint(CM1Constraint.CNSTR_INTRA_ZONE_PARK, ref fIntraZonePark))
                {
                    throw new InvalidOperationException("Could not get IntraZone Parking of group");
                }

                if (fIntraZonePark != CM1Constraint.CNSTR_UNDEFINED && fIntraZonePark != 1)
                {
                    // Get courtesy time of the group of the operation to evaluate
                    float fCourtesyTime = GlobalDefs.DEF_UNDEFINED_VALUE;
                    if (!group.GetConstraint(CM1Constraint.CNSTR_CORTESY_TIME, ref fCourtesyTime))
                    {
                        throw new InvalidOperationException("Could not get Courtesy time of group");
                    }

                    if (fCourtesyTime == CM1Constraint.CNSTR_UNDEFINED)
                    {
                        fCourtesyTime = 0;
                    }

                    trace.Write(TraceLevel.Info, $"Courtesy time: {fCourtesyTime}");

                    COPSDate dtAddedDate = pdtEnd.Copy();
                    dtAddedDate.AddTimeSpan(new TimeSpan(0, (int)fCourtesyTime, 0));

                    trace.Write(TraceLevel.Info, $"Final date of operation plus courtesy time: {dtAddedDate.fstrGetTraceString()}, Exist Time Reset: {bTimeReset}");

                    bIntraZoneCourtesy = ((dtGrpLast <= dtAddedDate) && (!bTimeReset));
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        private bool EvaluateCourtesy(CM1Group group, COPSDate dtGrpLast, COPSDate pdtEnd, bool bTimeReset, ref bool bZoneCourtesy)
        {
            trace.Write(TraceLevel.Debug, "CM1ComputeEx0::EvaluateCourtesy");
            bool fnResult = true;

            try
            {
                Guard.IsNull(group, nameof(group));
                Guard.IsNull(dtGrpLast, nameof(dtGrpLast));
                Guard.IsNull(pdtEnd, nameof(pdtEnd));

                trace.Write(TraceLevel.Info, $"Evaluating Courtesy time for group: {group.GetGrpId()}");
                trace.Write(TraceLevel.Info, $"Last date of group: {dtGrpLast.fstrGetTraceString()}");
                trace.Write(TraceLevel.Info, $"Final date of operation: {pdtEnd.fstrGetTraceString()}");

                bZoneCourtesy = true;

                if (dtGrpLast.IsValid() && dtGrpLast > pdtEnd)
                {
                    // Get courtesy time of the group of the operation to evaluate
                    float fCourtesyTime = GlobalDefs.DEF_UNDEFINED_VALUE;
                    if (!group.GetConstraint(CM1Constraint.CNSTR_CORTESY_TIME, ref fCourtesyTime))
                    {
                        throw new InvalidOperationException("Could not get Courtesy time of group");
                    }

                    if (fCourtesyTime == CM1Constraint.CNSTR_UNDEFINED)
                    {
                        fCourtesyTime = 0;
                    }

                    trace.Write(TraceLevel.Info, $"Courtesy time: {fCourtesyTime}");

                    COPSDate dtAddedDate = pdtEnd.Copy();
                    dtAddedDate.AddTimeSpan(new TimeSpan(0,(int)fCourtesyTime,0));

                    trace.Write(TraceLevel.Info, $"Final date of operation plus courtesy time: {dtAddedDate.fstrGetTraceString()}, Exist Time Reset: {bTimeReset}");

                    bZoneCourtesy = ((dtGrpLast <= dtAddedDate) && (!bTimeReset));
                    
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        private bool EvaluateIntraZonePark(CM1Group group, COPSDate dtGrpLast, COPSDate pdtEnd, bool bTimeReset, ref bool bIntraZonePark)
        {
            trace.Write(TraceLevel.Debug, "CM1ComputeEx0::EvaluateIntraZonePark");
            bool fnResult = true;

            try
            {
                Guard.IsNull(group, nameof(group));
                Guard.IsNull(dtGrpLast, nameof(dtGrpLast));
                Guard.IsNull(pdtEnd, nameof(pdtEnd));

                trace.Write(TraceLevel.Info, $"Evaluating IntraZone Parking for group: {group.GetGrpId()}");
                trace.Write(TraceLevel.Info, $"Last date of group: {dtGrpLast.fstrGetTraceString()}");
                trace.Write(TraceLevel.Info, $"Final date of operation: {pdtEnd.fstrGetTraceString()}");

                bIntraZonePark = true;

                float fIntraZonePark = GlobalDefs.DEF_UNDEFINED_VALUE;
                if (!group.GetConstraint(CM1Constraint.CNSTR_INTRA_ZONE_PARK, ref fIntraZonePark))
                {
                    throw new InvalidOperationException("Could not get IntraZone Parking of group");
                }

                if (fIntraZonePark != CM1Constraint.CNSTR_UNDEFINED && fIntraZonePark != 1)
                {
                    // Get courtesy time of the group of the operation to evaluate
                    float fReentryTime = GlobalDefs.DEF_UNDEFINED_VALUE;
                    if (!group.GetConstraint(CM1Constraint.CNSTR_REENTRY_TIME, ref fReentryTime))
                    {
                        throw new InvalidOperationException("Could not get reentry time of group");
                    }

                    if (fReentryTime == CM1Constraint.CNSTR_UNDEFINED)
                    {
                        fReentryTime = 0;
                    }

                    trace.Write(TraceLevel.Info, $"Reentry time: {fReentryTime}");

                    COPSDate dtAddedDate = pdtEnd.Copy();
                    dtAddedDate.AddTimeSpan( new TimeSpan(0,(int)fReentryTime,0));

                    if (dtGrpLast.IsValid())
                    {
                        trace.Write(TraceLevel.Info, $"Final date of operation plus reentry time: {dtAddedDate.fstrGetTraceString()}, Exist Time Reset: {bTimeReset}");

                        bIntraZonePark = ((dtGrpLast >= dtAddedDate) || (bTimeReset));
                    }
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
        private bool ExistTimeResetBetweenDates(COPSDate pdtDate1, COPSDate pdtDate2, ref bool bTimeReset)
        {

            trace.Write(TraceLevel.Debug, "M1ComputeEx::ExistTimeResetBetweenDates");
            bool fnResult = true;
            COPSPlate strVehicleId;
            try
            {
                m_pm1ComputeExAux = new M1ComputeEx0(trace.Creator, this.Type);
                m_pm1ComputeExAux.SetTracerEnabled(false);
                m_pm1ComputeExAux.SetDBB(m_pDBB);
                m_pm1ComputeExAux.SetOperType(OperationDat.DEF_OPERTYPE_PARK);
                m_pm1ComputeExAux.SetUnitId(GetUnitId());
                m_pm1ComputeExAux.SetArticleDef(GetArticleDef());
                m_pm1ComputeExAux.SetGroupId(GetGroupId());
                m_pm1ComputeExAux.SetMinCoinValue(GetMinCoinValue());

                m_pm1ComputeExAux.SetDateTime(pdtDate1);
                m_pm1ComputeExAux.SetInitialDateTime(pdtDate1);
                m_pm1ComputeExAux.SetLimitDateTime(pdtDate2);
                m_pm1ComputeExAux.m_bConsiderCloseFirstInterval = true;
                SetPaymentType(GetPaymentType());
                // VEHICLE PLATE: If it exists we must apply history (except for a return operation)

                strVehicleId = GetVehicleId();
                if (!strVehicleId.IsEmpty())
                {
                    m_pm1ComputeExAux.SetVehicleId(strVehicleId);
                }

                if (!m_pm1ComputeExAux.Compute(false, false))
                    throw new InvalidOperationException("Error computing m1 for current operation");

                trace.Write(TraceLevel.Info, $"Tariff Efective Time:{m_pm1ComputeExAux.m_lCurrMinutes} | Tariff Efective Money:{m_pm1ComputeExAux.m_lTariffMoney}");

                bTimeReset = m_pm1ComputeExAux.m_bTimeReset;
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private bool CorrectOperationData(CM1GroupsTree pTree, COPSPlate vehicleId, long lArticleDef, ref COPSDate dtBackOpIni, ref COPSDate dtBackOpEnd,ref  Operations operation, 
                                            ref long lRealAccMoney, ref long lRealAccTime, ref bool bTimeReset, ref bool bLessMaxInterdateReeTime, ref bool bDiscardOp, ref long lInitialTimeLastInterval,
                                            ref long m_lEndTimeLastInterval, ref long lCurrentTimeLastInterval)
        {
            trace.Write(TraceLevel.Debug, "CM1ComputeEx0::CorrectOperationData");
            bool fnResult = true;

            bool bIntReset = false;
            bool bDatesChanged = false;

            try
            {
                bDiscardOp = false;

                if (!CorrectOperationDates(ref dtBackOpIni, ref dtBackOpEnd,ref operation, ref bDatesChanged, ref bDiscardOp)) {
                    throw new InvalidOperationException("Error correcting operation dates");
                }

                if (!bDiscardOp) {
                    m_pm1ComputeExAux = new M1ComputeEx0(trace.Creator, this.Type);
                    m_pm1ComputeExAux.SetTracerEnabled(false);
                    bTimeReset = false;
                    bLessMaxInterdateReeTime = false;
                    m_pm1ComputeExAux.SetDBB(m_pDBB);

                    if(m_lOperType == OperationDat.DEF_OPERTYPE_RETN) 
                    {
                        m_pm1ComputeExAux.SetOperType(OperationDat.DEF_OPERTYPE_PARK);
                    }
                    else
                    {
                        m_pm1ComputeExAux.SetOperType(operation.OPE_DOPE_ID.Value);
                    }

                    m_pm1ComputeExAux.SetArticleDef(lArticleDef);
                    m_pm1ComputeExAux.SetDateTime(operation.OPE_INIDATE);
                    m_pm1ComputeExAux.SetInitialDateTime(operation.OPE_INIDATE);
                    m_pm1ComputeExAux.SetGroupId(operation.OPE_GRP_ID.GetValueOrDefault());
                    m_pm1ComputeExAux.SetMinCoinValue(GetMinCoinValue());

                    if (operation.OPE_DOPE_ID != OperationDat.DEF_OPERTYPE_RETN) {
                        m_pm1ComputeExAux.SetMaxMoney(operation.OPE_VALUE.GetValueOrDefault());
                    }
                    // VEHICLE PLATE: If it exists we must apply history (except for a return operation)
                    if (!vehicleId.IsEmpty())
                    {
                        m_pm1ComputeExAux.SetVehicleId(vehicleId);
                    }
                    m_pm1ComputeExAux.SetLimitDateTime(operation.OPE_ENDDATE);

                    if (!m_pm1ComputeExAux.Compute(false, false))
                        throw new InvalidOperationException("Error computing m1 for current operation");


                    trace.Write(TraceLevel.Info, $"Tariff Efective Time:{m_pm1ComputeExAux.m_lCurrMinutes} | Tariff Efective Money:{m_pm1ComputeExAux.m_lTariffMoney}");

                    bTimeReset = m_pm1ComputeExAux.m_bTimeReset;
                    bIntReset = m_pm1ComputeExAux.m_bIntReset;

                    if (operation.OPE_DOPE_ID.GetValueOrDefault() != OperationDat.DEF_OPERTYPE_RETN)
                    {
                        lRealAccMoney = operation.OPE_VALUE.GetValueOrDefault();
                    }
                    else
                    {
                        lRealAccMoney = m_pm1ComputeExAux.m_lRealCurrMoney;
                    }

                    lRealAccTime = m_pm1ComputeExAux.m_lRealCurrMinutes;

                    dtBackOpIni = operation.OPE_INIDATE.Copy();
                    dtBackOpEnd = operation.OPE_ENDDATE.Copy();

                    if (bTimeReset || bDatesChanged || operation.OPE_DOPE_ID.GetValueOrDefault() == OperationDat.DEF_OPERTYPE_RETN)
                    {
                        operation.OPE_DURATION  = m_pm1ComputeExAux.m_lCurrMinutes;
                        operation.OPE_INIDATE   = m_pm1ComputeExAux.GetOperEfectiveInitDateTime().Copy();
                        operation.OPE_ENDDATE   = m_pm1ComputeExAux.GetFinalDateTime().Copy();
                    }

                    if (bIntReset || bDatesChanged || (operation.OPE_DOPE_ID.GetValueOrDefault() == OperationDat.DEF_OPERTYPE_RETN))
                    {
                        operation.OPE_VALUE = GetRoundMoney(m_pm1ComputeExAux.m_lTariffMoney);
                    }

                    if ((m_pm1ComputeExAux.m_dtDayChange) && (m_pm1ComputeExAux.m_lBlocksWithTariff == 1))
                    {
                        long lMaxInDtRee = 0;
                        if (!m_pm1ComputeExAux.GetTree().GetBranchMaxInterdateReentry(m_pm1ComputeExAux.GetGroupId(),ref lMaxInDtRee))
                            throw new InvalidOperationException("Error getting Max Interdate Reentry time");

                        if (lMaxInDtRee != GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            bLessMaxInterdateReeTime = (operation.OPE_DURATION.GetValueOrDefault() < lMaxInDtRee);
                        }
                    }

                    if (operation.OPE_DOPE_ID.GetValueOrDefault() == OperationDat.DEF_OPERTYPE_RETN)
                    {
                        operation.OPE_DOPE_ID = OperationDat.DEF_OPERTYPE_PARK;
                    }
                    
                    lInitialTimeLastInterval = m_pm1ComputeExAux.m_lInitialTimeLastInterval;
                    m_lEndTimeLastInterval = m_pm1ComputeExAux.m_lEndTimeLastInterval;
                    lCurrentTimeLastInterval= m_pm1ComputeExAux.m_lCurrentTimeLastInterval;

                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        //TODO : dtOpEnd no se puede modificar ¿Que hacemos?
        private bool CorrectOperationDates(ref COPSDate dtBackOpIni,ref COPSDate dtBackOpEnd,ref Operations operation, ref bool bDatesChanged, ref bool bDiscardOp)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::CorrectOperationDates");
            bool fnResult = true;

            bDatesChanged= false;
            bDiscardOp = false;

            try
            {
                if ((dtBackOpIni.GetStatus() != COPSDateStatus.Invalid) && 
                    (dtBackOpEnd.GetStatus() != COPSDateStatus.Invalid))
                {

                    if (dtBackOpIni < operation.OPE_INIDATE)
                    {
                        bDiscardOp = true;
                    }
                    else
                    {
                        if (dtBackOpIni < operation.OPE_ENDDATE)
                        {
                            operation.OPE_ENDDATE = dtBackOpIni.Copy();
                            bDatesChanged = true;
                        }
                    }



                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        private void OperationTraceRow(Operations operation) {
            trace.Write(TraceLevel.Info, $"[ {operation.OPE_ID,-9} | {operation.OPE_MOVDATE.fstrGetTraceString(),-21} | {operation.OPE_DOPE_ID,-8} | {operation.OPE_GRP_ID,-7} | {operation.OPE_INIDATE.fstrGetTraceString(),-21} | {operation.OPE_ENDDATE.fstrGetTraceString(),-21} | {operation.OPE_VALUE,-8} | {operation.OPE_DURATION,-8} | {operation.OPE_DPAY_ID,-8} | {operation.OPE_POST_DAY,-8} | {operation.OPE_OP_ONLINE,-8} ]");
        }
        private void OperationTraceRowHeader() {
            trace.Write(TraceLevel.Info, $"[ {"Operation",-9} | {"Date",-21} | {"Op. Type",-8} | {"Group",-7} | {"IniDate",-21} | {"EndDate",-21} | {"Op.Money",-8} | {"Op.Time",-8} | {"Pay. Tipe",-8} | {"Post Pay",-8} | {"Online",-8} ]");
        }

        private bool ApplyConstraintsEx(long constraintId)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::ApplyConstraintsEx");
            bool fnResult = true;
            List<Constraints> constraints = null;

            try
            {
                CM1GroupsTree pTree = GetTree();
                if (pTree == null)
                    throw new InvalidOperationException("Tree pointer is null");

                if (!m_pDBB.GetConstraints(constraintId, ref constraints))
                    throw new InvalidOperationException("FAILED call to m_pPDMDB->GetConstraints");

                foreach (Constraints constraint in constraints)
                {
                    // Constraint for GROUP
                    if (constraint.CON_GRP_ID != GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        CM1Group pGrp = pTree.GetGroupFromGrpId(constraint.CON_GRP_ID.Value);

                        if (pGrp == null)
                        {
                            // Constraints for a group are defined in table CONSTRAINTS, but this group is not defined in table GROUPS
                            trace?.Write(TraceLevel.Error, $"Group {constraint.CON_GRP_ID} not defined but has constraints");
                            continue;
                        }

                        fnResult  = pTree.MergeOrAddConstraint(pGrp, constraint.CON_DCON_ID.Value, (float)constraint.CON_VALUE);

                    } // Constraint for GROUP_DEF
                    else if (constraint.CON_DGRP_ID != GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        CM1Group[] pGrp = pTree.GetGroups();
                        if (pGrp == null)
                        {
                            trace.Write(TraceLevel.Error, $"Type of group {constraint.CON_DGRP_ID} not defined but has constraints");
                            continue;
                        }

                        List<CM1Group> grupos = pGrp.Where(g => g.GetGrpId() != -1 && g.GetGrpTypeId() == constraint.CON_DGRP_ID).ToList();
                        foreach (var grupo in grupos)
                        {
                            fnResult = pTree.MergeOrAddConstraint(grupo, constraint.CON_DCON_ID.Value, (float)constraint.CON_VALUE);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Group and DefGroup undefined");
                    }
                } 
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

                                
            return fnResult;
        }
        public bool GetConstraintsAndTariff(COPSDate pdtWork, ref long constraint, ref long tariff)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::GetConstraintsAndTariff");
            bool fnResult = true;


            constraint = GlobalDefs.DEF_UNDEFINED_VALUE;
            tariff = GlobalDefs.DEF_UNDEFINED_VALUE;

            

            try
            {
                if (GetTree() == null)
                    throw new InvalidOperationException("Could not obtain tree");

                // Get group object of the group of the operation to evaluate
                CM1Group pGrp = GetTree().GetGroupFromGrpId(m_lGroupId);
                if (pGrp == null)
                    throw new InvalidOperationException("Could not obtain group of current operation");


                long innerConstraint;
                long innerTariff;

                while (pGrp != null) {

                    innerConstraint = GlobalDefs.DEF_UNDEFINED_VALUE;
                    innerTariff = GlobalDefs.DEF_UNDEFINED_VALUE;

                    if (!m_pDBB.GetConstAndTar(m_lArticleDef, pGrp.GetGrpId(), pGrp.GetGrpTypeId(), pdtWork, ref innerConstraint, ref innerTariff))
                    {
                        throw new InvalidOperationException("FAILED call to m_pPDMDB->GetConstAndTar");
                    }

                    if (innerConstraint != GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        trace?.Write(TraceLevel.Info, $"Found constraint {innerConstraint} for group {m_lGroupId}");
                        if (constraint == GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            constraint = innerConstraint;
                        }
                    }

                    if (innerTariff != GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        trace?.Write(TraceLevel.Info, $"Found tariff {innerTariff} for group {m_lGroupId}");
                        if (tariff == GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            tariff = innerTariff;
                        }
                    }

                    if (tariff != GlobalDefs.DEF_UNDEFINED_VALUE && constraint != GlobalDefs.DEF_UNDEFINED_VALUE) {
                        trace?.Write(TraceLevel.Info, $"Tariff set to {tariff} and constraint to {constraint}");
                        break;
                    }

                    pGrp = GetTree().GetGroupParent(pGrp.GetGrpId());
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        private bool BuildGroupsTree()
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::BuildGroupsTree");
            bool fnResult = true;

            long lConstraint = GlobalDefs.DEF_UNDEFINED_VALUE;
            long lTariff = GlobalDefs.DEF_UNDEFINED_VALUE;
            COPSDate dtWork = null;

            try
            {
                // Fill the tree with groups and group childs
                FillTree();

                //TODO:
                GetTree().TraceFullTree();

                if (GetOperTariffInitialDateTime()?.GetStatus() == COPSDateStatus.Invalid)
                {
                    dtWork = GetDateTime().Copy();
                }
                else
                {
                    dtWork = GetOperTariffInitialDateTime().Copy();
                }

                //TODO: DELETE TEST DATA
                //dtWork = new COPSDate(00, 00, 00, 02, 01, 2014);

                // CONSTRAINTS AND TARIFF
                if (!GetConstraintsAndTariff(dtWork, ref lConstraint, ref lTariff))
                {
                    throw new InvalidOperationException("Could not obtain neither tariffs nor constraints");
                }

                if (lConstraint == GlobalDefs.DEF_UNDEFINED_VALUE || lTariff == GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    trace?.Write(TraceLevel.Error, $"Could not find constraints for: Type of article: {m_lArticleDef}, Group: {m_lGroupId}, Type of group: {m_lTypeOfGroup}");
                    SetCurrentTariff(GlobalDefs.DEF_UNDEFINED_VALUE);
                    m_lResult = M1_OP_NOK;  // TODO: CREATE A SPECIFIC ERROR
                }
                else
                {
                    SetCurrentTariff(lTariff);

                    // Fill the tree with constraints
                    ApplyConstraintsEx(lConstraint);

                    // Prune the branches with no constraints
                    PruneTree();

                    //GetTree().TraceFullTree();

                    if (m_lOperType == OperationDat.DEF_OPERTYPE_RETN)
                    {
                        float fReturnAllowed = 0;
                        if (!GetTree().GetGroupFromGrpId(m_lGroupId).GetConstraint(CM1Constraint.CNSTR_RETN_ALLOW, ref fReturnAllowed))
                        {
                            throw new InvalidOperationException("Could not get return constraint");
                        }

                        if (fReturnAllowed == 0)
                            m_lResult = M1_OP_NOK;
                    }
                    else
                    {
                        //The group can have a constraint that avoids evaluating history
                        //CAUTION: WE ONLY LOOK AT THE GROUP WE ARE TRYING TO PARK
                        float fEvaluateHistory = 0;
                        if (!GetTree().GetGroupFromGrpId(m_lGroupId).GetConstraint(CM1Constraint.CNSTR_HISTORY_EVAL, ref fEvaluateHistory))
                        {
                            throw new InvalidOperationException("Could not get history evaluation");
                        }

                        m_bApplyVehicleHistory = m_strVehicleId.ToString() != "*****" && (fEvaluateHistory == CM1Constraint.CNSTR_UNDEFINED || fEvaluateHistory > 0);
                    }
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        
        // Fill the tree with groups and group childs
        public override bool FillTree()
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::FillTree");
            bool fnResult = true;

            try
            {
                CM1GroupsTree pTree = GetTree();

                if (pTree == null)
                    throw new InvalidOperationException("Tree pointer NULL");

                if (!m_bNotPrunedTreeInit)
                {
                    if (!LoadGroups(pTree))
                        throw new InvalidOperationException("LoadGroups Error");
                    if (!LoadGroupsChilds(pTree))
                        throw new InvalidOperationException("LoadGroupsChilds Error");

                    m_NotPrunedTree.Copy(pTree);
                    m_bNotPrunedTreeInit = true;
                }
                else
                {
                     pTree.Copy(m_NotPrunedTree);
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        private bool LoadGroups(CM1GroupsTree pTree)
        {
            trace.Write(TraceLevel.Info, ">>LoadGroups");
            bool fnResult = true;

            try
            {
                if (m_pDBB == null)
                    throw new InvalidOperationException("Invalid DB handler");

                IEnumerable<Groups> grupos = m_pDBB.GetAllGroups();
                if (grupos == null)
                    throw new InvalidOperationException("Could not load groups");

                trace.Write(TraceLevel.Info, $" {"Group",8} | {"GroupType",10}");
                foreach (Groups grupo in grupos)
                {
                    trace.Write(TraceLevel.Info, $" {grupo.GRP_ID,8} | {grupo.GRP_DGRP_ID,5}");
                }

                int addResult = GlobalDefs.DEF_UNDEFINED_VALUE;
                foreach (Groups grupo in grupos)
                {
                    addResult = pTree.AddGroup(grupo.GRP_ID, grupo.GRP_DGRP_ID);

                    switch (addResult)
                    {
                        case CM1GroupsTree.ADD_TREE_ERR:
                            trace.Write(TraceLevel.Error, $"Error {addResult} adding Group: {grupo.GRP_ID} Group Type: {grupo.GRP_DGRP_ID}");
                            break;
                        case CM1GroupsTree.ADD_TREE_OK:
                            trace.Write(TraceLevel.Info, $"Added Group: {grupo.GRP_ID} Group Type: {grupo.GRP_DGRP_ID}");
                            break;
                        case CM1GroupsTree.ADD_TREE_WAS:
                            trace.Write(TraceLevel.Info, $"Group: {grupo.GRP_ID} Group Type: {grupo.GRP_DGRP_ID} existed");
                            break;
                        default:
                            throw new InvalidOperationException("Undefined error");
                    }
                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            trace.Write(TraceLevel.Info, "<<LoadGroups");
            return fnResult;
        }
        private bool LoadGroupsChilds(CM1GroupsTree pTree)
        {
            trace.Write(TraceLevel.Info, ">>LoadGroupsChilds");
            bool fnResult = true;

            try
            {
                if (m_pDBB == null)
                    throw new InvalidOperationException("Invalid DB handler");

                IEnumerable<GroupsChilds> grupos = m_pDBB.GetAllGroupsChilds();
                if (grupos == null)
                    throw new InvalidOperationException("Could not load groups");

                trace.Write(TraceLevel.Info, $" {"Group",8} | {"ChildGroup",10}");
                foreach (GroupsChilds grupo in grupos)
                {
                    trace.Write(TraceLevel.Info, $" {grupo.CGRP_ID,8} | {grupo.CGRP_CHILD,5}");
                }

                int addResult = GlobalDefs.DEF_UNDEFINED_VALUE;
                foreach (GroupsChilds grupo in grupos)
                {
                    addResult = pTree.AddNode(grupo.CGRP_ID, grupo.CGRP_CHILD);
                    
                    switch (addResult)
                    {
                        case CM1GroupsTree.ADD_TREE_ERR:
                            trace.Write(TraceLevel.Error, $"Error {addResult} adding Group: {grupo.CGRP_ID} Child: {grupo.CGRP_CHILD}");
                            break;
                        case CM1GroupsTree.ADD_TREE_OK:
                            trace.Write(TraceLevel.Info, $"Added Group: {grupo.CGRP_ID} Child: {grupo.CGRP_CHILD}");
                            break;
                        case CM1GroupsTree.ADD_TREE_WAS:
                            trace.Write(TraceLevel.Info, $"Group: {grupo.CGRP_ID} Child: {grupo.CGRP_CHILD} existed");
                            break;
                        default:
                            throw new InvalidOperationException("Undefined error");
                    }
                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            trace.Write(TraceLevel.Info, "<<LoadGroupsChilds");

            return fnResult;
        }
        private bool PruneTree(int nFase = 0)
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::PruneTree");
            bool fnResult = true;

            try
            {
                // Branch is the set of groups from the child (without other childer) to the parent (without parents)
                CM1GroupsTree pTree = GetTree();
                if (pTree == null)
                    throw new InvalidOperationException("Tree pointer is null");

                PruneFase faseToApply = (PruneFase)nFase;

                if (faseToApply.IsIn(PruneFase.NO_FASE, PruneFase.FIRST)) pTree.PruneFase1();
                if (faseToApply.IsIn(PruneFase.NO_FASE, PruneFase.SECOND)) pTree.PruneFase2(m_lGroupId);
                if (faseToApply.IsIn(PruneFase.NO_FASE, PruneFase.THIRD)) pTree.PruneFase3();
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        private bool CheckInput()
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::CheckInput");
            bool fnResult = true;

            COPSDate dtOper;

            try
            {
                if (m_bApplyVehicleHistory) {

                    if (m_strVehicleId == null)
                        throw new InvalidOperationException("Could not obtain vehicle id");

                    if (m_strVehicleId.IsEmpty())
                    {
                        trace.Write(TraceLevel.Info, "Plate is empty, but history must be applied");
                        m_lResult = M1_OP_NOK;
                    }
                    else
                        trace.Write(TraceLevel.Info, $"Plate: {m_strVehicleId.ToString()}");
                }

                if(GetDateTime() == null)
                    throw new InvalidOperationException("Could not obtain operation date");

                if (!GetDateTime().IsValid())
                {
                    trace.Write(TraceLevel.Error, "Operation date is not valid");
                    m_lResult = M1_OP_NOK;  // TODO: CREATE AN SPECIFIC ERROR
                }

                trace.Write(TraceLevel.Info, $"Operation date: {GetDateTime().fstrGetTraceString()}");

                // UNIT, GROUP
                if (!CheckGroup())                         
                    throw new InvalidOperationException("Error checking group");

                if (m_lOperType == GlobalDefs.DEF_UNDEFINED_VALUE || m_lOperType == OperationDat.DEF_OPERTYPE_UNDEF)
                {
                    trace.Write(TraceLevel.Error, $"Operation type not defined: {m_lOperType}");
                    m_lResult = M1_OP_NOK;
                }
                else
                {
                    trace.Write(TraceLevel.Info, $"Operation type: {m_lOperType}");
                }

                // ARTICLE, TYPE OF ARTICLE
                if (!CheckArticle())
                {
                    throw new InvalidOperationException("Error checking article");
                }

                trace.Write(TraceLevel.Info, $"Max. Import: {m_lMaxMoney}");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        private bool CheckArticle()
        {
            trace.Write(TraceLevel.Debug, "M1ComputeEx0::CheckArticle");
            bool fnResult = true;

            COPSDate dtOper = null;
            long lArticle = m_lArticle;
            long lArticleDef = m_lArticleDef;
            long lArticleDefDB = GlobalDefs.DEF_UNDEFINED_VALUE;
            COPSPlate strVehicleId = null;
            COPSPlate strVehicleIdDB = null;
            COPSDate dtArtIni = null;
            COPSDate dtArtEnd = null;
            bool bFind = false;
            long lUser = GlobalDefs.DEF_UNDEFINED_VALUE;

            try
            {
                if (lArticleDef != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    trace.Write(TraceLevel.Info, $"Article def: {lArticleDef}");
                }
                else if (lArticle != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    trace.Write(TraceLevel.Info, $"Article: {m_lArticle}");

                    if (!m_pDBB.GetInfoArticle(lArticle,ref dtOper, ref strVehicleIdDB, ref lUser, ref lArticleDefDB,
                                           ref dtArtIni, ref dtArtEnd, ref bFind))
                    {
                        throw new InvalidOperationException("Error in call to m_pPDMDB.GetInfoArticle");
                    }
                }
                else
                {
                    trace.Write(TraceLevel.Info, "Neither article nor type of article defined, setting default type of article");
                    m_lArticleDef = 4;
                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        private bool CheckGroup()
        {
            trace.Write(TraceLevel.Debug, "CM1ComputeEx0::CheckGroup");
            bool fnResult = true;

            long lUnit;
            long lGroup;
            long lTypeOfGroup = GlobalDefs.DEF_UNDEFINED_VALUE;

            try
            {
                lUnit = m_lUnitId;
                lGroup = m_lGroupId;

                if (lGroup != GlobalDefs.DEF_UNDEFINED_VALUE)
                {

                    bool existGroup = m_pDBB.ExistsGroup(lGroup);
                    if( !existGroup)
                    {
                        trace.Write(TraceLevel.Error, $"Undefined group ({lGroup})");
                        //m_lResult = M1_OP_NOK;
                    }
                }
                else if (lUnit != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    if (!m_pDBB.GetGroupFromUnit(lUnit, ref lGroup, ref lTypeOfGroup))
                    {
                        trace.Write(TraceLevel.Error, $"Error calling to m_pPDMDB->GetGroupFromUnit");
                    }

                    if (lGroup == GlobalDefs.DEF_UNDEFINED_VALUE || lTypeOfGroup == GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        trace.Write(TraceLevel.Error, $"Undefined group ({lGroup}) or type of group ({lTypeOfGroup}) for unit {lUnit}");
                        m_lResult = M1_OP_NOK;
                    }
                    else
                    {
                        trace.Write(TraceLevel.Error, $"Found group ({lGroup}) and type of group ({lTypeOfGroup}) for unit {lUnit}");
                        m_lGroupId = lGroup;
                        m_lTypeOfGroup = lTypeOfGroup;
                    }
                } else {
                    trace.Write(TraceLevel.Error, "Neither group nor unit are defined in  the input data");
                    m_lResult = M1_OP_NOK;
                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        
    }
}

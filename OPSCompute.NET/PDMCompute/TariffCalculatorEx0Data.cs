using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMCompute
{
    internal class TariffCalculatorEx0Data
    {
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

        private COPSDate m_dtOperTariffInitial = new COPSDate();
        public COPSDate DtOperTariffInitial { get => m_dtOperTariffInitial; set => m_dtOperTariffInitial = value; }
        public COPSDate DtOper { get => m_dtOper; set => m_dtOper = value; }

        public void Reset()
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

            m_lResult = TariffCalculator.M1_OP_OK;
            m_dtOper?.SetStatus(COPSDateStatus.Invalid);
            m_dtOperInitial?.SetStatus(COPSDateStatus.Invalid);
            m_dtOperFinal?.SetStatus(COPSDateStatus.Invalid);
            m_dtLimit?.SetStatus(COPSDateStatus.Invalid);
            m_dtFirst?.SetStatus(COPSDateStatus.Invalid);
            m_dtOperEfectiveInit?.SetStatus(COPSDateStatus.Invalid);
            m_dtOperDateRealIni?.SetStatus(COPSDateStatus.Invalid);
            DtOperTariffInitial?.SetStatus(COPSDateStatus.Invalid);

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
            m_lCalculateSteps_StepValue = TariffCalculator.STEP_CALCULATION_DEFAULT_VALUE;
            m_lCalculateSteps_CurrentValue = GlobalDefs.DEF_UNDEFINED_VALUE;
        }

        private COPSPlate GetVehicleId()
        {
            return m_strVehicleId;
        }
        private void SetPaymentType(long paymentType)
        {
            m_iPaymentType = (int)paymentType;
        }
        private long GetPaymentType()
        {
            return m_iPaymentType;
        }
        private long GetArticleDef()
        {
            return m_lArticleDef;
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
        private void SetMinMoney(long minMoney)
        {
            m_lMinMoney = minMoney;
        }
        private void SetMoney(long money)
        {
            m_lOperMoney = money;
        }
        private long GetMoney()
        {
            return m_lOperMoney;
        }
        private long GetMaxMoney()
        {
            return m_lMaxMoney;
        }
        private long GetOperType()
        {
            return m_lOperType;
        }
        private void SetFirstDateTime(COPSDate pdtIni)
        {
            m_dtFirst = pdtIni;
        }
        private COPSDate GetInitialDateTime()
        {
            return m_dtOperInitial;
        }
        private long GetRoundMoney(long money)
        {
            trace.Write(TraceLevel.Info, "GetRoundMoney");
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
        public void SetLimitDateTime(COPSDate date)
        {
            m_dtLimit = date;
        }
        public void SetVehicleId(COPSPlate vehicleId)
        {
            m_strVehicleId = vehicleId;
        }
        public void SetMaxMoney(long newMaxMoney)
        {
            m_lMaxMoney = newMaxMoney;
        }
        public void SetInitialDateTime(COPSDate date)
        {
            m_dtOperInitial = date;
            m_dtOperEfectiveInit = date;
        }
        public void SetDateTime(COPSDate operationDate)
        {
            m_dtOper = operationDate;
        }
        public void SetArticleDef(long lArticleDef)
        {
            m_lArticleDef = lArticleDef;
        }
        public void SetOperType(long lOperType)
        {
            m_lOperType = lOperType;
        }

    }
}

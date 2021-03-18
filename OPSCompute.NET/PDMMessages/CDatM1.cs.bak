using System;
using PDMHelpers;
using PDMHelpers.Extensions;
using PDMMessages;

namespace PDMCompute
{
    public struct stMoneyOffDiscount
    {
        public string szCode;
        public ulong ulId;
        public long lResult;
    };

    public class CDatM1 : CDatM
    {
        public const int MAX_NUM_PHOTOS = 10;
        public const int MAX_NUM_USR_MSGS = 10;
        public const int MAX_DYN_TARIFF_PARS = 10;

        public const int MAX_MONEYOFF_DISCOUNTS = 5;
        public const int MAX_MONEYOFF_DISCOUNT_CODE_LENGTH =20;

        private COPSPlate m_strInVehicleID = new COPSPlate();
        private COPSDate m_dtInDate = new COPSDate();
        private int m_iInOperType;
        private int m_iInArticleDef;
        private int m_iInArticle;
        private int m_iInGroup;
        private int m_lInMoney;
        private long m_lOutImportAcc;
        private int m_iPaymentType;
        private COPSDate m_dtInMaxDate = new COPSDate();
        private bool m_bInComputeTimeLimits;
        private bool m_bRoundMoney;
        private int m_iInCheckArticle;
        private int m_lInOperationID;
        private int m_lInCurrRotation;
        private long m_lInMaxTime;
        private bool m_bInHistOnlyWithSamePaymentType;
        private bool m_bMinEqMax;
        private string m_szOutFineNumber;
        private int m_iOutFineResult;
        private int m_bFineAlreadyPayed;
        private long m_lInAddFreeMinutesQuantity;
        private long m_lInAddFreeMoneyQuantity;
        private bool m_bInCalculateTimeSteps;
        private bool m_bInCalculateQuantitySteps;
        private int m_iInCalculateSteps_StepValue;
        private string m_strOutStepsCalculationString;
        private long m_lOutAcummImport;
        private int m_nOutAcummTime;
        private long m_lOutMaxImport;
        private int m_lOutMinTime;
        private int m_lOutMaxTime;
        private int m_nOutEfMaxTime;
        private int m_lOutStepImport;
        private int m_lOutRetImport;
        private long m_lOutMinImport;
        private int m_iOutResult;
        private COPSDate m_dtOutOperDateRealIni = new COPSDate();
        private int m_iOutWholeOperationWithChipCard;
        private int m_iOutWholeOperationWithMobile;
        private long m_lOutRealAcummImport;
        private int m_nOutRealAcummTime;
        private int m_iOutPostPay;
        private int m_iOutIsResident;
        private int m_iOutIsVIP;
        private bool m_bOutHistPostPay;
        private int m_iOutRemainingMinutesWithZeroValue;
        private bool m_bCurrentAmountIsAllowed;
        private int m_iCurNumPhotos;
        private int m_iOutNumPhotosReceived;
        private COPSDate m_dtOutOperDateEnd = new COPSDate();
        private long m_lInIntervalOffsetMinutes;
        private long m_lInIntervalOffsetMoney;
        private long m_lOutAcummImportAllGroup;
        private int m_nOutAcummTimeAllGroup;
        private string m_szInGroupDesc;
        private string m_szInArticleDefDesc;
        private COPSDate m_dtOutOperDate = new COPSDate();
        private COPSDate m_dtOutOperDateIni = new COPSDate();
        private COPSDate m_dtOutOperDateIni0 = new COPSDate();
        private COPSDate m_dtOutMinOperDate = new COPSDate();
        private COPSDate m_dtOutMaxOperDate = new COPSDate();
        private string m_szVAOCardID1;
        private string m_szVAOCardID2;
        private string m_szVAOCardID3;
        PhotoStorage[] m_OutPhotoStorage = new PhotoStorage[MAX_NUM_PHOTOS];
        string[] m_szOutUserMsgs = new string[MAX_NUM_USR_MSGS];
        int[] m_OutDynTariffParameters = new int[MAX_DYN_TARIFF_PARS];
        private int m_lOutAddFreeMinutesQuantity;
        private int m_lOutAddFreeMoneyQuantity;
        private bool m_bHistFreeMoneyUsed;
        private int m_iNumMoneyOffDiscounts;
        stMoneyOffDiscount[] m_stMoneyOffDiscounts = new stMoneyOffDiscount[MAX_MONEYOFF_DISCOUNTS];
        private long m_lOutRealMinutes;

        public CDatM1() : base() {
            Clear();
        }
        public CDatM1(ILoggerManager loggerManager) : base(loggerManager)
        {
            trace = loggerManager.CreateTracer(this.GetType());
            Clear();
        }

        public override void Clear() {
            base.Clear();

            trace.Write(TraceLevel.Debug, "CDatM1::Clear");

            m_iSendingPriority = (int)Priority.PRIORITY_VERY_HIGH;
            m_szInGroupDesc = string.Empty;
            m_szInArticleDefDesc = string.Empty;
            m_iInOperType = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_iInArticle = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_iInArticleDef = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_iInGroup = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lInMoney = 0;
            m_dtInDate?.SetStatus(COPSDateStatus.Null);
            m_strInVehicleID?.Empty();
            m_iPaymentType = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_dtInMaxDate?.SetStatus(COPSDateStatus.Null);
            m_bInComputeTimeLimits = false;
            m_lInMaxTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_bInHistOnlyWithSamePaymentType = false;
            m_lInIntervalOffsetMinutes = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lInIntervalOffsetMoney = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_bRoundMoney = true;
            m_iInCheckArticle = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lInOperationID = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lInCurrRotation = 0;

            m_lInAddFreeMinutesQuantity = 0;
            m_lInAddFreeMoneyQuantity = 0;

            // Out
            m_iOutResult = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lOutMinImport = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lOutMaxImport = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lOutMinTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lOutMaxTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_nOutEfMaxTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lOutStepImport = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lOutRetImport = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lOutImportAcc = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_dtOutOperDate?.SetStatus(COPSDateStatus.Null);
            m_dtOutOperDateIni?.SetStatus(COPSDateStatus.Null);
            m_dtOutOperDateEnd?.SetStatus(COPSDateStatus.Null);
            m_dtOutOperDateIni0?.SetStatus(COPSDateStatus.Null);
            m_dtOutMinOperDate?.SetStatus(COPSDateStatus.Null);
            m_dtOutMaxOperDate?.SetStatus(COPSDateStatus.Null);
            m_lOutAcummImport = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_nOutAcummTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_lOutAcummImportAllGroup = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_nOutAcummTimeAllGroup = GlobalDefs.DEF_UNDEFINED_VALUE;

            m_iOutWholeOperationWithChipCard = 0;
            m_iOutWholeOperationWithMobile = 0;
            m_dtOutOperDateRealIni?.SetStatus(COPSDateStatus.Null);
            m_lOutRealAcummImport = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_nOutRealAcummTime = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_iOutPostPay = 0;
            m_iOutIsResident = 0;
            m_iOutIsVIP = 0;
            m_bOutHistPostPay = false;
            m_iOutRemainingMinutesWithZeroValue = 0;
            m_bCurrentAmountIsAllowed = false;
            m_iCurNumPhotos = 0;
            m_iOutNumPhotosReceived = 0;

            foreach (PhotoStorage photoStorage in m_OutPhotoStorage)
            {
                photoStorage?.Clear();
            }

            int i = 0;
            for (i = 0; i < MAX_NUM_USR_MSGS; i++)
                m_szOutUserMsgs[i] = string.Empty;

            m_szVAOCardID1 = string.Empty;
            m_szVAOCardID2 = string.Empty;
            m_szVAOCardID3 = string.Empty;

            m_bMinEqMax = false;

            m_szOutFineNumber = string.Empty;
            m_iOutFineResult = GlobalDefs.DEF_UNDEFINED_VALUE;
            m_bFineAlreadyPayed = 0;

            for (i = 0; i < MAX_DYN_TARIFF_PARS; i++)
            {
                m_OutDynTariffParameters[i] = -1;
            }

            m_lOutAddFreeMinutesQuantity = 0;
            m_lOutAddFreeMoneyQuantity = 0;
            m_bHistFreeMoneyUsed = false;

            m_iNumMoneyOffDiscounts = 0;
            for (i = 0; i < MAX_MONEYOFF_DISCOUNTS; i++)
            {
                //m_stMoneyOffDiscounts[i].szCode, 0, sizeof(m_stMoneyOffDiscounts[i].szCode));
                m_stMoneyOffDiscounts[i].ulId = 0;
                m_stMoneyOffDiscounts[i].lResult = 0;

            }

            m_bInCalculateTimeSteps=false;
		    m_bInCalculateQuantitySteps=false;
		    m_iInCalculateSteps_StepValue= GlobalDefs.DEF_UNDEFINED_VALUE;
            m_strOutStepsCalculationString = string.Empty;

            m_strIDMsg = "m1"; 
            SetMxType(typeof(MSG_M_TEXT).GetValueFromString<int>(m_strIDMsg));
        }

        public void Copy(CDatM1 pDatM1)
        {
            trace.Write(TraceLevel.Debug, "CDatM1::Copy");

            try
            {
                base.Copy(pDatM1);

                pDatM1.m_szInGroupDesc.CopyTo(0, m_szInGroupDesc.ToCharArray(), 0, pDatM1.m_szInGroupDesc.Length);
                pDatM1.m_szInArticleDefDesc.CopyTo(0, m_szInArticleDefDesc.ToCharArray(), 0, pDatM1.m_szInArticleDefDesc.Length);
                m_strInVehicleID = pDatM1.m_strInVehicleID;
                m_dtInDate = pDatM1.m_dtInDate.Copy();
                m_iInOperType = pDatM1.m_iInOperType;
                m_iInArticle = pDatM1.m_iInArticle;
                m_iInArticleDef = pDatM1.m_iInArticleDef;
                m_iInGroup = pDatM1.m_iInGroup;
                m_iPaymentType = pDatM1.m_iPaymentType;
                m_lInMoney = pDatM1.m_lInMoney;
                m_dtInMaxDate = pDatM1.m_dtInMaxDate.Copy();
                m_bInComputeTimeLimits = pDatM1.m_bInComputeTimeLimits;
                m_lInMaxTime = pDatM1.m_lInMaxTime;
                m_bInHistOnlyWithSamePaymentType = pDatM1.m_bInHistOnlyWithSamePaymentType;
                m_lInIntervalOffsetMinutes = pDatM1.m_lInIntervalOffsetMinutes;
                m_lInIntervalOffsetMoney = pDatM1.m_lInIntervalOffsetMoney;
                m_bRoundMoney = pDatM1.m_bRoundMoney;
                m_iInCheckArticle = pDatM1.m_iInCheckArticle;
                m_lInOperationID = pDatM1.m_lInOperationID;
                m_lInCurrRotation = pDatM1.m_lInCurrRotation;
                m_lInAddFreeMinutesQuantity = pDatM1.m_lInAddFreeMinutesQuantity;
                m_lInAddFreeMoneyQuantity = pDatM1.m_lInAddFreeMoneyQuantity;

                //Out
                m_iOutResult = pDatM1.m_iOutResult;
                m_lOutMinImport = pDatM1.m_lOutMinImport;
                m_lOutMaxImport = pDatM1.m_lOutMaxImport;
                m_lOutMinTime = pDatM1.m_lOutMinTime;
                m_lOutMaxTime = pDatM1.m_lOutMaxTime;
                m_nOutEfMaxTime = pDatM1.m_nOutEfMaxTime;
                m_lOutStepImport = pDatM1.m_lOutStepImport;
                m_lOutRetImport = pDatM1.m_lOutRetImport;
                m_lOutImportAcc = pDatM1.m_lOutImportAcc;
                m_dtOutOperDate = pDatM1.m_dtOutOperDate.Copy();
                m_dtOutOperDateIni = pDatM1.m_dtOutOperDateIni.Copy();
                m_dtOutOperDateEnd = pDatM1.m_dtOutOperDateEnd.Copy();
                m_dtOutOperDateIni0 = pDatM1.m_dtOutOperDateIni0.Copy();
                m_dtOutMinOperDate = pDatM1.m_dtOutMinOperDate.Copy();
                m_dtOutMaxOperDate = pDatM1.m_dtOutMaxOperDate.Copy();
                m_lOutAcummImport = pDatM1.m_lOutAcummImport;
                m_lOutAcummImportAllGroup = pDatM1.m_lOutAcummImportAllGroup;
                m_nOutAcummTime = pDatM1.m_nOutAcummTime;
                m_nOutAcummTimeAllGroup = pDatM1.m_nOutAcummTimeAllGroup;
                m_iOutWholeOperationWithChipCard = pDatM1.m_iOutWholeOperationWithChipCard;
                m_iOutWholeOperationWithMobile = pDatM1.m_iOutWholeOperationWithMobile;
                m_dtOutOperDateRealIni = pDatM1.m_dtOutOperDateRealIni.Copy();
                m_lOutRealAcummImport = pDatM1.m_lOutRealAcummImport;
                m_nOutRealAcummTime = (int)pDatM1.m_lOutRealAcummImport;
                m_iOutPostPay = pDatM1.m_iOutPostPay;
                m_iOutIsResident = pDatM1.m_iOutIsResident;
                m_iOutIsVIP = pDatM1.m_iOutIsVIP;
                m_iOutRemainingMinutesWithZeroValue = pDatM1.m_iOutRemainingMinutesWithZeroValue;
                m_szVAOCardID1 = String.Copy(pDatM1.m_szVAOCardID1);
                m_szVAOCardID2 = String.Copy(pDatM1.m_szVAOCardID2);
                m_szVAOCardID3 = String.Copy(pDatM1.m_szVAOCardID3);
                m_iCurNumPhotos = pDatM1.m_iCurNumPhotos;
                m_iOutNumPhotosReceived = pDatM1.m_iOutNumPhotosReceived;

                int i = 0;
                for (i = 0; i < m_iCurNumPhotos; i++)
                    m_OutPhotoStorage[i] = pDatM1.m_OutPhotoStorage[i];

                for (i = 0; i < MAX_NUM_USR_MSGS; i++)
                    m_szOutUserMsgs[i] = String.Copy(pDatM1.m_szOutUserMsgs[i]);


                for (i = 0; i < MAX_DYN_TARIFF_PARS; i++)
                {
                    m_OutDynTariffParameters[i] = pDatM1.m_OutDynTariffParameters[i];
                }

                m_bMinEqMax = pDatM1.m_bMinEqMax;

                m_lOutAddFreeMinutesQuantity = pDatM1.m_lOutAddFreeMinutesQuantity;
                m_lOutAddFreeMoneyQuantity = pDatM1.m_lOutAddFreeMoneyQuantity;

                m_szOutFineNumber = String.Copy(pDatM1.m_szOutFineNumber);
                m_iOutFineResult = pDatM1.m_iOutFineResult;
                m_bFineAlreadyPayed = pDatM1.m_bFineAlreadyPayed;

                m_iNumMoneyOffDiscounts = pDatM1.m_iNumMoneyOffDiscounts;
                for (i = 0; i < MAX_MONEYOFF_DISCOUNTS; i++)
                {
                    //_tcscpy(m_stMoneyOffDiscounts[i].szCode, pDatM1.m_stMoneyOffDiscounts[i].szCode);
                    m_stMoneyOffDiscounts[i].ulId = pDatM1.m_stMoneyOffDiscounts[i].ulId;
                    m_stMoneyOffDiscounts[i].lResult = pDatM1.m_stMoneyOffDiscounts[i].lResult;

                }

                m_bHistFreeMoneyUsed = pDatM1.m_bHistFreeMoneyUsed;

                m_bInCalculateTimeSteps = pDatM1.m_bInCalculateTimeSteps;
                m_bInCalculateQuantitySteps = pDatM1.m_bInCalculateQuantitySteps;
                m_iInCalculateSteps_StepValue = pDatM1.m_iInCalculateSteps_StepValue;
                m_strOutStepsCalculationString = pDatM1.m_strOutStepsCalculationString;

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }
        }

       

        public override bool SetData(COPSMsg message)
        {
            bool result = true;
            trace.Write(TraceLevel.Debug, "CDatM1::SetData");

            try
            {
                if (message == null)
                    throw new ArgumentNullException(nameof(message), "Invalid input parameter");

                base.SetData(message);

                m_strInVehicleID = message.GetElm().GetOPSPlate("m");
                m_dtInDate = message.GetElm().GetOPSDate("d", m_dtInDate);
                m_iInOperType = message.GetElm().GetInt("o", m_iInOperType);
                m_iInArticleDef = message.GetElm().GetInt("ad", m_iInArticleDef);
                m_iInArticle = message.GetElm().GetInt("a", m_iInArticle);
                m_iInGroup = message.GetElm().GetInt("g", m_iInGroup);
                m_lOutImportAcc = message.GetElm().GetLong("q", m_lOutImportAcc);
                m_iPaymentType= message.GetElm().GetInt("pt", m_iPaymentType);
                m_dtInMaxDate = message.GetElm().GetOPSDate("d2", m_dtInMaxDate);

                int iInComputeTimeLimits = 0;
                iInComputeTimeLimits = message.GetElm().GetInt("cdl", iInComputeTimeLimits);
                m_bInComputeTimeLimits = (iInComputeTimeLimits == 1);

                int iRoundMoney = 1;
                iRoundMoney = message.GetElm().GetInt("rmon", iRoundMoney);
                m_bRoundMoney = (iRoundMoney == 1);

                m_lInMaxTime = message.GetElm().GetLong("t", m_lInMaxTime);

                int iInHistOnlyWithSamePaymentType = 0;
                iInHistOnlyWithSamePaymentType = message.GetElm().GetInt("spt", iInHistOnlyWithSamePaymentType);
                m_bInHistOnlyWithSamePaymentType = (iInHistOnlyWithSamePaymentType == 1);

                int iMinEqMax = 0;
                iMinEqMax = message.GetElm().GetInt("mineqmax", iMinEqMax);
                m_bMinEqMax = (iMinEqMax == 1);

                m_lInAddFreeMinutesQuantity = message.GetElm().GetLong("aft", m_lInAddFreeMinutesQuantity);
                m_lInAddFreeMoneyQuantity = message.GetElm().GetLong("afm", m_lInAddFreeMoneyQuantity);

                int iTemp = 0;
                iTemp = message.GetElm().GetInt("ctst", iTemp);
                m_bInCalculateTimeSteps = (iTemp != 0);
                iTemp = 0;
                iTemp = message.GetElm().GetInt("cqst", iTemp);
                m_bInCalculateQuantitySteps = (iTemp != 0);
                if (m_bInCalculateQuantitySteps)
                    m_bInCalculateTimeSteps = false;

                m_iInCalculateSteps_StepValue = message.GetElm().GetInt("stv", m_iInCalculateSteps_StepValue);
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return result;
        }

        public int GetOutIsVIP()
        {
            return m_iOutIsVIP;
        }

        public int GetInArticleDef()
        {
            return m_iInArticleDef;
        }
        public void SetInArticleDef(int value)
        {
            m_iInArticleDef = value;
        }

        public int GetInArticle() { return m_iInArticle; }
        public void SetInArticle(int iValue) { m_iInArticle = iValue; }

        public void SetInGroup(int iGroup) { m_iInGroup = iGroup; }
        public int GetInGroup() { return m_iInGroup; }

        public void SetInOperType(int iOperType) { m_iInOperType = iOperType; }
        public int GetInOperType() { return m_iInOperType; }

        public bool SetInVehicleID(COPSPlate pstrVehicleID)
        {
            trace.Write(TraceLevel.Debug, "CDatM1::SetInVehicleID");
            bool fnResult = true;

            try
            {
                m_strInVehicleID = new COPSPlate(pstrVehicleID.ToString()) ?? throw new ArgumentNullException(nameof(pstrVehicleID), "Parameter NULL");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public COPSPlate GetInVehicleID()
        {
            trace.Write(TraceLevel.Debug, "CDatM1::GetInVehicleID");
            return m_strInVehicleID;
        }

        public bool SetInDate(COPSDate date)
        {
            trace.Write(TraceLevel.Debug, "CDatM1::SetInDate");
            bool fnResult = true;

            try
            {
                if(date == null)
                    throw new ArgumentNullException(nameof(date), "Parameter NULL");

                if (!date.IsValid())
                    throw new ArgumentOutOfRangeException(nameof(date), "Not a valid Date");

                m_dtInDate = date.Copy();
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public COPSDate GetInDate()
        {
            trace.Write(TraceLevel.Debug, "CDatM1::GetInDate");
            return m_dtInDate;
        }

        public bool SetInMaxDate(COPSDate date)
        {
            trace.Write(TraceLevel.Debug, "CDatM1::SetInMaxDate");
            bool fnResult = true;

            try
            {
                if (date == null)
                    throw new ArgumentNullException(nameof(date), "Parameter NULL");

                if (!date.IsValid())
                    throw new ArgumentOutOfRangeException(nameof(date), "Not a valid Date");

                m_dtInMaxDate = date.Copy();
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public COPSDate GetInMaxDate()
        {
            trace.Write(TraceLevel.Debug, "CDatM1::GetInMaxDate");
            return m_dtInMaxDate;
        }

        public int GetInPaymentType()
        {
            return m_iPaymentType;
        }
        public void SetPaymentType(int iPaymentType)
        {
            m_iPaymentType = iPaymentType;
        }

        public void SetInIntervalOffsetMinutes(long lInIntervalOffsetMinutes)
        {
            m_lInIntervalOffsetMinutes = lInIntervalOffsetMinutes;
        }
        public long GetInIntervalOffsetMinutes()
        {
            return m_lInIntervalOffsetMinutes;
        }

        public void SetInIntervalOffsetMoney(long lInIntervalOffsetMoney)
        {
            m_lInIntervalOffsetMoney = lInIntervalOffsetMoney;
        }
        public long GetInIntervalOffsetMoney()
        {
            return m_lInIntervalOffsetMoney;
        }

        public void SetInComputeTimeLimits(bool bInComputeTimeLimits)
        {
            m_bInComputeTimeLimits = bInComputeTimeLimits;
        }
        public bool GetInComputeTimeLimits()
        {
            return m_bInComputeTimeLimits;
        }

        public void SetInHistOnlyWithSamePaymentType(bool bInHistOnlyWithSamePaymentType)
        {
            m_bInHistOnlyWithSamePaymentType = bInHistOnlyWithSamePaymentType;
        }
        public bool GetInHistOnlyWithSamePaymentType()
        {
            return m_bInHistOnlyWithSamePaymentType;
        }

        public void SetInAddFreeMinutesQuantity(long lInAddFreeMinutesQuantity)
        {
            m_lInAddFreeMinutesQuantity = lInAddFreeMinutesQuantity;
        }
        public long GetInAddFreeMinutesQuantity()
        {
            return m_lInAddFreeMinutesQuantity;
        }

        public void SetInAddFreeMoneyQuantity(long lInAddFreeMoneyQuantity)
        {
            m_lInAddFreeMoneyQuantity = lInAddFreeMoneyQuantity;
        }
        public long GetInAddFreeMoneyQuantity()
        {
            return m_lInAddFreeMoneyQuantity;
        }

        public void SetInMaxTime(long lInMaxTime)
        {
            m_lInMaxTime = lInMaxTime;
        }
        public long GetInMaxTime()
        {
            return m_lInMaxTime;
        }

        public void SetInCalculateTimeSteps(bool bCalculateTimeSteps)
        {
            m_bInCalculateTimeSteps = bCalculateTimeSteps;
        }
        public bool GetInCalculateTimeSteps()
        {
            return m_bInCalculateTimeSteps;
        }

        public void SetInCalculateQuantitySteps(bool bCalculateQuantitySteps)
        {
            m_bInCalculateQuantitySteps = bCalculateQuantitySteps;
        }
        public bool GetInCalculateQuantitySteps()
        {
            return m_bInCalculateQuantitySteps;
        }

        public void SetInCalculateSteps_StepValue(int bCalculateQuantitySteps)
        {
            m_iInCalculateSteps_StepValue = bCalculateQuantitySteps;
        }
        public int GetInCalculateSteps_StepValue()
        {
            return m_iInCalculateSteps_StepValue;
        }

        public bool GetInMinEqMax()
        {
            return m_bMinEqMax;
        }
        public bool GetInRoundMoney()
        {
            return m_bRoundMoney;
        }

        public string GetOutStepsCalculationString()
        {
            return m_strOutStepsCalculationString;
        }

        public void SetOutAccumulateMoney(long lAcummImport) { m_lOutAcummImport = lAcummImport; }
        public long GetOutAccumulateMoney() { return m_lOutAcummImport; }

        public void SetOutAccumulateTime(int nAcummTime) { m_nOutAcummTime = nAcummTime; }
        public int GetOutAccumulateTime() { return m_nOutAcummTime; }

        public void SetOutMaxImport(long lMaxImport) { m_lOutMaxImport = lMaxImport; }
        public long GetOutMaxImport() { return m_lOutMaxImport; }

        public void SetOutIntAcumul(long lIntAcumul) { m_lOutImportAcc = lIntAcumul; }
        public long GetOutIntAcumul() { return m_lOutImportAcc; }

        public void SetOutEfMaxTime(int nEfMaxTime) { m_nOutEfMaxTime = nEfMaxTime; }
        public int GetOutEfMaxTime() { return m_nOutEfMaxTime; }

        public void SetOutMinImport(long lMinImport) { m_lOutMinImport = lMinImport; }
        public long GetOutMinImport() { return m_lOutMinImport; }

        public void SetOutResult(int iResult) { m_iOutResult = iResult; }
        public int GetOutResult() { return m_iOutResult; }

        public void SetOutIsVIP(bool isVIP)
        {
            m_iOutIsVIP = isVIP ? 1 : 0;
        }
        public void SetOutIsResident(bool isResident)
        {
            m_iOutIsResident = isResident ? 1 : 0;
        }

        public void SetOutPostPay(int iPostPay)
        {
            m_iOutPostPay = iPostPay;
        }
        public int GetOutPostPay()
        {
            return m_iOutPostPay;
        }

        public void SetOutWholeOperationWithChipCard(int iWholeOperationWithChipCard)
        {
            m_iOutWholeOperationWithChipCard = iWholeOperationWithChipCard;
        }
        public int GetOutWholeOperationWithChipCard()
        {
            return m_iOutWholeOperationWithChipCard;
        }

        public void SetOutWholeOperationWithMobile(int iWholeOperationWithMobile)
        {
            m_iOutWholeOperationWithMobile = iWholeOperationWithMobile;
        }
        public int GetOutWholeOperationWithMobile()
        {
            return m_iOutWholeOperationWithMobile;
        }

        public void SetOutRealAccumulateMoney(long lRealAcummImport)
        {
            m_lOutRealAcummImport = lRealAcummImport;
        }
        public long GetOutRealAccumulateMoney()
        {
            return m_lOutRealAcummImport;
        }

        public void SetOutRealAccumulateTime(int nRealAcummTime)
        {
            m_nOutRealAcummTime = nRealAcummTime;
        }
        public int GetOutRealAccumulateTime()
        {
            return m_nOutRealAcummTime;
        }

        public COPSDate GetOutOperDateEnd()
        {
            trace.Write(TraceLevel.Debug, "CDatM1::GetOutOperDateEnd");
            return m_dtOutOperDateEnd;
        }
        public void SetOutOperDateEnd(COPSDate dtOper)
        {
            m_dtOutOperDateEnd = dtOper.Copy();
        }

        public bool SetOutOperDateIni0(COPSDate date)
        {
            trace.Write(TraceLevel.Debug, "CDatM1::SetOutOperDateIni0");
            bool fnResult = true;

            try
            {
                if (date == null)
                    throw new ArgumentNullException(nameof(date), "Parameter NULL");

                if (!date.IsValid())
                    throw new ArgumentOutOfRangeException(nameof(date), "Not a valid Date");

                m_dtOutOperDateIni0 = date.Copy();
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public COPSDate GetOutOperDateIni0()
        {
            trace.Write(TraceLevel.Debug, "CDatM1::GetOutOperDateIni0");
            return m_dtOutOperDateIni0;
        }

        public bool SetOutOperDateIni(COPSDate date)
        {
            trace.Write(TraceLevel.Debug, "CDatM1::SetOutOperDateIni");
            bool fnResult = true;

            try
            {
                if (date == null)
                    throw new ArgumentNullException(nameof(date), "Parameter NULL");

                if (!date.IsValid())
                    throw new ArgumentOutOfRangeException(nameof(date), "Not a valid Date");

                m_dtOutOperDateIni = date.Copy();
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public COPSDate GetOutOperDateIni()
        {
            trace.Write(TraceLevel.Debug, "CDatM1::GetOutOperDateIni");
            return m_dtOutOperDateIni;
        }

        public bool SetOutOperDateRealIni(COPSDate date)
        {
            trace.Write(TraceLevel.Debug, "CDatM1::SetOutOperDateRealIni");
            bool fnResult = true;

            try
            {
                if (date == null)
                    throw new ArgumentNullException(nameof(date), "Parameter NULL");

                if (!date.IsValid())
                    throw new ArgumentOutOfRangeException(nameof(date), "Not a valid Date");

                m_dtOutOperDateRealIni = date.Copy();
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public COPSDate GetOutOperDateRealIni()
        {
            trace.Write(TraceLevel.Debug, "CDatM1::GetOutOperDateRealIni");
            return m_dtOutOperDateRealIni;
        }

        public long GetOutRetImport()
        {
            return m_lOutRetImport;
        }

        public void SetOutAccumulateMoneyAllGroup(long lAcummImport)
        {
            m_lOutAcummImportAllGroup = lAcummImport;
        }
        public long GetOutAccumulateMoneyAllGroup()
        {
            return m_lOutAcummImportAllGroup;
        }

        public void SetOutAccumulateTimeAllGroup(int nAcummTime)
        {
            m_nOutAcummTimeAllGroup = nAcummTime;
        }
        public int GetOutAccumulateTimeAllGroup()
        {
            return m_nOutAcummTimeAllGroup;
        }

        public long GetOutRealMinutes()
        {
            return m_lOutRealMinutes;
        }
        public void SetOutRealMinutes(long lRealMinutes)
        {
            m_lOutRealMinutes = lRealMinutes;
        }

        public int GetOutIsResident()
        {
            return m_iOutIsResident;
        }


        public void SetOutPostPay(bool iPostPay)
        {
            m_iOutPostPay = iPostPay ? 1 : 0;
        }

        public void SetOutHistFreeMoneyUsed(bool bHistFreeMoneyUsed)
        {
            m_bHistFreeMoneyUsed = bHistFreeMoneyUsed;
        }

        public void SetOutHistPostPay(bool bHistPostPay)
        {
            m_bOutHistPostPay = bHistPostPay;
        }

        public long GetOutMinTime()
        {
            return m_lOutMinTime;
        }
        public void SetOutMinTime(long lMinTime)
        {
            m_lOutMinTime = (int)lMinTime;
        }

        public COPSDate GetOutMinOperDate()
        {
            return m_dtOutMinOperDate;
        }
        public void SetOutMinOperDate(COPSDate pdtOperDate)
        {
            m_dtOutMinOperDate = pdtOperDate.Copy();
        }

        public long GetOutMaxTime()
        {
            return m_lOutMaxTime;
        }
        public void SetOutMaxTime(long lMaxTime)
        {
            m_lOutMaxTime = (int)lMaxTime;
        }

        public COPSDate GetOutMaxOperDate()
        {
            return  m_dtOutMaxOperDate;
        }
        public void SetOutMaxOperDate(COPSDate pdtOperDate)
        {
            m_dtOutMaxOperDate = pdtOperDate.Copy();
        }

        public void SetOutRemainingMinutesWithZeroValue(int iOutRemainingMinutesWithZeroValue)
        {
            m_iOutRemainingMinutesWithZeroValue = iOutRemainingMinutesWithZeroValue;
        }

        public void SetCurrentAmountIsAllowed(bool bCurrentAmountIsAllowed)
        {
            m_bCurrentAmountIsAllowed = bCurrentAmountIsAllowed;
        }

        public void SetOutAddFreeMinutesQuantity(long lOutAddFreeMinutesQuantity)
        {
            m_lOutAddFreeMinutesQuantity = (int)lOutAddFreeMinutesQuantity;
        }

        public void SetOutAddFreeMoneyQuantity(long lOutAddFreeMoneyQuantity)
        {
            m_lOutAddFreeMoneyQuantity = (int)lOutAddFreeMoneyQuantity;
        }

        public void SetOutRetImport(long lRetImport)
        {
            m_lOutRetImport = (int)lRetImport;
        }

        public bool AddStepToStepCalculationString(int iNumStep, long lMoney, long lMinutes, COPSDate dtDate)
        {
            trace.Write(TraceLevel.Debug, "CDatM1::AddStepToStepCalculationString");
            bool fnResult = true;

            try
            {
                m_strOutStepsCalculationString += $"<stq{iNumStep}>{lMoney}</stq{iNumStep}><stt{iNumStep}>{lMinutes}</stt{iNumStep}><std{iNumStep}>{dtDate.CopyToChar()}</std{iNumStep}>";
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        public override void Trace(TraceLevel iTraceLevel = TraceLevel.Info)
        {
            trace.Write(TraceLevel.Debug, "CDatM::Trace");

            try
            {
                COPSDate opsDate;
                COPSPlate strVehicle;

                trace.Write(iTraceLevel, $"------------------TRACE M1 DATA--------------------");
                trace.Write(iTraceLevel, $"INPUT----------------------------");

                trace.Write(iTraceLevel, $"	Unit ({GetInUnit()})");
                trace.Write(iTraceLevel, $"	Article ({GetInArticle()})");
                trace.Write(iTraceLevel, $"	Article Def ({GetInArticleDef()})");
                trace.Write(iTraceLevel, $"	Group ({GetInGroup()})");
                trace.Write(iTraceLevel, $"	Oper Type ({GetInOperType()})");

                strVehicle = GetInVehicleID();
                if (strVehicle.IsEmpty())
                {
                    trace.Write(iTraceLevel, $"	VehicleID (NO PLATE)");
                }
                else
                {
                    trace.Write(iTraceLevel, $"	VehicleID ({strVehicle.ToString()})");
                }

                trace.Write(iTraceLevel, $"	Date In ({GetInDate().fstrGetTraceString()})");
                trace.Write(iTraceLevel, $"OUTPUT----------------------------");
                trace.Write(iTraceLevel, $"	Accumulate Money  History ({GetOutAccumulateMoney()})");
                trace.Write(iTraceLevel, $"	Accumulate Time History ({GetOutAccumulateTime()})");
                trace.Write(iTraceLevel, $"	Maxim Import For Current Operation({GetOutMaxImport()})");
                trace.Write(iTraceLevel, $"	Money Acumulate For Current Operation ({GetOutIntAcumul()})");
                trace.Write(iTraceLevel, $"	Efective Max Time ({GetOutEfMaxTime()})");
                trace.Write(iTraceLevel, $"	MinImport ({GetOutMinImport()})");
                trace.Write(iTraceLevel, $"	MaxImport ({GetOutMaxImport()})");
                trace.Write(iTraceLevel, $"	Result ({GetOutResult()})");

                opsDate = GetOutOperDateIni0().Copy();
                if (opsDate != null)
                    trace.Write(iTraceLevel, $"	Date Ini 0 Oper ({opsDate.fstrGetTraceString()})");
                else
                    trace.Write(iTraceLevel, $"	Date Ini 0 Oper (NO DATE)");

                opsDate = GetOutOperDateIni().Copy();
                if (opsDate != null)
                    trace.Write(iTraceLevel, $"	Date Ini Oper ({opsDate.fstrGetTraceString()})");
                else
                    trace.Write(iTraceLevel, $"	Date Ini Oper (NO DATE)");

                opsDate = GetOutOperDateEnd().Copy();
                if (opsDate != null)
                    trace.Write(iTraceLevel, $"	Date End Oper ({opsDate.fstrGetTraceString()})");
                else
                    trace.Write(iTraceLevel, $"	Date End Oper (NO DATE)");

                trace.Write(iTraceLevel, $"	WholeOperationWithChipCard ({GetOutWholeOperationWithChipCard()})");
                trace.Write(iTraceLevel, $"	WholeOperationWithMobile({GetOutWholeOperationWithMobile()})");

                opsDate = GetOutOperDateRealIni().Copy();
                if (opsDate != null)
                    trace.Write(iTraceLevel, $"	Real Oper Ini Date ({opsDate.fstrGetTraceString()})");
                else
                    trace.Write(iTraceLevel, $"	Real Oper Ini Date (NO DATE)");

                trace.Write(iTraceLevel, $"	Real Accumulate Money ({GetOutRealAccumulateMoney()})");
                trace.Write(iTraceLevel, $"	Real Accumulate Time ({GetOutRealAccumulateTime()})");

                trace.Write(iTraceLevel, "------------------TRACE M1 DATA--------------------");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }
        }
    }
}
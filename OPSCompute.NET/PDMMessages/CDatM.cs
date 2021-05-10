using System;
using PDMHelpers;
using PDMMessages;

namespace PDMCompute
{
    public class CDatM
    {
        public const int DEF_DATA_LEN = 12;
        public const int DEF_TELTYPE_UNKNOW = -1;
        public const int DEF_MTYPE_MUNKNOW = -1;

        protected ITraceable trace;
        protected UInt32 m_dwTelID;

        // Header Attributes
        protected string m_szHdSrc;
        protected COPSDate m_dtHdDate = new COPSDate();

        // Message Attributtes
        protected int m_iIdentifier;
        protected int m_iRet;
        protected string m_szDst;
        protected int m_iPriority;
        protected int m_iSendingPriority;
        protected string m_strIDMsg;
        protected bool m_bLookingForType;
        protected int m_iInUnit;
        protected int m_iSent;
        protected CPDMMessagesStatistics m_pStatColl;

        private int m_iTelType;
        private int m_iMxType;
        private int m_iNbError;
        private bool m_bTagAtt;
        private string m_szTagValue;
        private string m_szTagName;

        public CDatM() {
            
        }
        public CDatM(ILoggerManager loggerManager) :this()
        {
            trace = loggerManager.CreateTracer(this.GetType());
            Clear();
        }

        public virtual void Clear() {

            trace.Write(TraceLevel.Debug, "CDatM::Clear");

            try
            {
                m_bLookingForType = false;
                m_strIDMsg = "";
                // Header
                m_dwTelID = 0;
                // Message Attributtes
                m_iIdentifier = GlobalDefs.DEF_UNDEFINED_VALUE;
                m_iRet = GlobalDefs.DEF_UNDEFINED_VALUE;
                m_iPriority = GlobalDefs.DEF_UNDEFINED_VALUE;
                m_iInUnit = GlobalDefs.DEF_UNDEFINED_VALUE;

                m_iTelType = DEF_TELTYPE_UNKNOW;
                m_iMxType = DEF_MTYPE_MUNKNOW;
                m_iNbError = 0;
                m_bTagAtt = false;

                m_dtHdDate?.SetStatus(COPSDateStatus.Null);

                m_szHdSrc = string.Empty;
                m_szDst = string.Empty;
                m_szTagValue = string.Empty;
                m_szTagName = string.Empty;

                m_iSent = 0;
                m_pStatColl = null;
                m_iSendingPriority = (int)Priority.PRIORITY_MEDIUM;
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }
        }
        public virtual void Copy(CDatM pDatM) {
            trace.Write(TraceLevel.Debug, "CDatM::Copy");

            try
            {
                if (pDatM == null)
                {
                    throw new ArgumentNullException(nameof(pDatM));
                }

                m_strIDMsg = pDatM.m_strIDMsg;
                // Header
                m_dwTelID = pDatM.m_dwTelID;
                // Message Attributtes
                m_iIdentifier = pDatM.m_iIdentifier;
                m_iRet = pDatM.m_iRet;
                m_iPriority = pDatM.m_iPriority;
                m_iInUnit = pDatM.m_iInUnit;

                // SAX
                m_bTagAtt = pDatM.m_bTagAtt;

                m_iTelType = pDatM.m_iTelType;
                m_iMxType = pDatM.m_iMxType;
                m_iNbError = pDatM.m_iNbError;

                m_dtHdDate = pDatM.m_dtHdDate.Copy();

                m_szHdSrc= pDatM.m_szHdSrc;
                m_szDst= pDatM.m_szDst;
                m_szTagValue= pDatM.m_szTagValue;
                m_szTagName = pDatM.m_szTagName;

                m_pStatColl = pDatM.m_pStatColl;
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }
        }
        public virtual bool Compare(CDatM pDatM) {
            return true;

        }

        public void SetTelID(UInt32 dwTelID) { m_dwTelID = dwTelID; }

        public bool SetSrc(string szSrc) {
            bool result = true;
            trace.Write(TraceLevel.Debug, "CDatM::SetSrc");

            try
            {
                m_szHdSrc = szSrc ?? throw new ArgumentNullException(nameof(szSrc), "Parameter NULL");
            }
            catch (Exception error) {
                trace.Write(TraceLevel.Error, error.ToLogString());
                result = false;
            }

            return result;
        }
        public string GetSrc() { return m_szHdSrc; }

        public bool SetDate(COPSDate pDate)
        {
            bool result = true;
            trace.Write(TraceLevel.Debug, "CDatM::SetDate");

            try
            {
                m_dtHdDate = pDate.Copy() ?? throw new ArgumentNullException(nameof(pDate), "Parameter NULL");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                result = false;
            }

            return result;
        }
        public COPSDate GetDate() { return m_dtHdDate; }

        public void SetIdentifier(int iIdentifier) { m_iIdentifier = iIdentifier; }
        public int GetIdentifier() { return m_iIdentifier; }

        public void SetRet(int iRet) { m_iRet = iRet; }
        public int GetRet() { return m_iRet; }

        public void SetInUnit(int iUnit) { m_iInUnit = iUnit; }
        public int GetInUnit() { return m_iInUnit; }

        public void SetTelType(int iTelType) { m_iTelType = iTelType; }
        public int GetTelType() { return m_iTelType; }

        public void SetMxType(int iMxType) { m_iMxType = iMxType; }
        public int GetMxType() { return m_iMxType; }

        public void SetNbError(int bNbError) { m_iNbError = bNbError; }
        public int GetNbError() { return m_iNbError; }

        public void SetPriority(int iPriority) { m_iPriority = iPriority; }
        public int GetPriority() { return m_iPriority; }

        public bool SetDst(string pDst) {
            bool result = true;
            trace.Write(TraceLevel.Debug, "CDatM::SetDst");

            try
            {
                m_szDst = pDst ?? throw new ArgumentNullException(nameof(pDst), "Parameter NULL");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                result = false;
            }

            return result;
        }
        public string GetDst() { return m_szDst; }

        public void SetSent(int iSent) { m_iSent = iSent; }
        public int GetSent() { return m_iSent; }

        public string GetIDMsg() { return m_strIDMsg; }

        public bool SetMsg(CPDMMessage message) {
            bool result = true;
            trace.Write(TraceLevel.Debug, "CDatM::SetMsg");

            try
            {
                if (message == null) {
                    throw new ArgumentNullException(nameof(message), "Parameter NULL");
                }

                if (m_strIDMsg == string.Empty)
                    throw new InvalidOperationException("m_strIDMsg Must be initialized in Ctor");

                message.SendingPriority = m_iSendingPriority;

                string szAuxTel = string.Empty;
                string szAuxData;
                string szAuxXml;

                if (m_szHdSrc.Length == 0)
                {
                    trace.Write(TraceLevel.Error, "Atributo Nodo Raiz 'p' - Src");
                    return false;
                }

                szAuxXml = $"<p src=\"{m_szHdSrc}\"";
                szAuxTel += szAuxXml;
                if (m_dtHdDate.IsValid())
                {
                    szAuxXml = $" dtx=\"{m_dtHdDate.CopyToChar()}\"";
                    szAuxTel += szAuxXml;
                }
                szAuxTel += ">";

                // Atributos Nodo Mensaje 'M' - id (Obligatorio)
                if (m_iIdentifier == GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    trace.Write(TraceLevel.Error, "Atributo Nodo Mensaje 'M' - id");
                    return false;
                }
                else
                {
                    szAuxXml = $"<{m_strIDMsg} id=\"{m_iIdentifier}\"";
                    szAuxTel += szAuxXml;
                }

                // Atributos Nodo Mensaje 'M' - ret (Opcional)
                if (m_iRet == GlobalDefs.DEF_UNDEFINED_VALUE || m_iRet == 0)
                {
                    trace.Write( TraceLevel.Info , "Atributo Nodo Mensaje 'M' - ret no se incluye");
                }
                else
                {
                    szAuxXml = $" ret=\"{m_iRet}\"";
                    szAuxTel += szAuxXml;
                }

                // Atributos Nodo Mensaje 'M' - dst (Opcional)
                if (m_szDst.Length == 0)
                {
                    trace.Write(TraceLevel.Info, "Atributo Nodo Mensaje 'M' - dst no se incluye");
                }
                else
                {
                    szAuxXml = $" dst=\"{m_szDst}\"";
                    szAuxTel += szAuxXml;
                }

                // Atributos Nodo Mensaje 'M' - pty (Opcional)
                if (m_iPriority == GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    trace.Write(TraceLevel.Info, "Atributo Nodo Mensaje 'M' - pty no se incluye");
                }
                else
                {
                    szAuxXml = $" pty=\"{m_iPriority}\"";
                    szAuxTel += szAuxXml;
                }

                szAuxTel += ">";

                if (!ProcessGetTelegram(szAuxTel))
                    return false;

                szAuxXml =$"</{m_strIDMsg}></p>";
                szAuxTel += szAuxXml;
                trace.Write(TraceLevel.Info, $"Aux XMl TEl: -{szAuxTel}-");

                message.SetTelegram(szAuxTel, szAuxTel.Length);

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                result = false;
            }

            return result;
        }
        public virtual bool ProcessGetTelegram(string szTelegram) { return false; }

        //*** DATE FUNCTIONS
        public bool IsDateOk(string pDate) {
            bool result = true;
            trace.Write(TraceLevel.Debug, "CDatM::IsDateOk");

            try
            {
                if (String.IsNullOrWhiteSpace(pDate))
                    throw new ArgumentNullException(nameof(pDate), "Invalid input parameter");

                if (pDate.Length != DEF_DATA_LEN)
                    throw new ArgumentNullException(nameof(pDate), "Invalid input parameter");

                COPSDate date = new COPSDate(pDate);
                return date.IsValid();
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return result;
        }
        public bool IsDateOk(COPSDate pDate) {
            return pDate.IsValid();
        }
        //*** DATE FUNCTIONS

        public void SetStatisticsCollector(CPDMMessagesStatistics pStatColl) {
            trace.Write(TraceLevel.Debug, "CDatM::SetStatisticsCollector");

            try
            {
                m_pStatColl = pStatColl ?? throw new ArgumentNullException(nameof(pStatColl), "pStatColl is NULL");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }
        }
        public CPDMMessagesStatistics GetStatisticsCollector() {
            trace.Write(TraceLevel.Debug, "CDatM::GetStatisticsCollector");

            try
            {
                return m_pStatColl ?? throw new InvalidOperationException("m_pStatColl is NULL");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return m_pStatColl;
        }

        public virtual bool ExtractData(COPSMsg message) {
            bool result = true;
            trace.Write(TraceLevel.Debug, "CDatM::ExtractData");

            try
            {
                if (message == null)
                    throw new ArgumentNullException(nameof(message), "Invalid input parameter");

                m_iTelType = message.GetCmd();
                m_dtHdDate = message.GetAtt().GetOPSDate("dtx", m_dtHdDate);
                m_iIdentifier = message.GetAtt().GetInt("id", m_iIdentifier);
                m_iRet = message.GetAtt().GetInt("ret", m_iRet); 
                m_iPriority = message.GetAtt().GetInt("pty", m_iPriority);
                m_iNbError = message.GetAtt().GetInt("error", m_iNbError);
                //m_iInUnit = message.GetElm().GetInt("u");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return result;
        }
        public virtual bool SetData(COPSMsg message)
        {
            bool result = true;
            trace.Write(TraceLevel.Debug, "CDatM::SetData");

            try
            {
                if (message == null)
                    throw new ArgumentNullException(nameof(message),"Invalid input parameter");

                m_iTelType = message.GetCmd();
                m_dtHdDate = message.GetAtt().GetOPSDate("dtx", m_dtHdDate);
                m_iIdentifier = message.GetAtt().GetInt("id", m_iIdentifier);
                m_iPriority = message.GetAtt().GetInt("pty", m_iPriority);
                m_iInUnit = message.GetElm().GetInt("u", m_iInUnit);
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }

            return result;
        }
        public virtual bool SetRespMessage(string szRespMsg, int iLen) {
            //TODO
            throw new NotImplementedException("");
        }

        public virtual void Trace(TraceLevel iTraceLevel = TraceLevel.Info)
        {
            trace.Write(TraceLevel.Debug, "CDatM::Trace");

            try
            {
                trace.Write(iTraceLevel, $"------------------GENERIC M DATA--------------------");
                trace.Write(iTraceLevel, $"Src		: {GetSrc()}");
                trace.Write(iTraceLevel, $"Dst		: {GetDst()}");
                trace.Write(iTraceLevel, $"Date		: {GetDate().fstrGetTraceString()}");
                trace.Write(iTraceLevel, $"Id		: {GetIdentifier()}");
                trace.Write(iTraceLevel, $"Ret		: {GetRet()}");
                trace.Write(iTraceLevel, $"Unit		: {GetInUnit()}");
                trace.Write(iTraceLevel, $"TelType	: {GetTelType()}");
                trace.Write(iTraceLevel, $"MxType	: {GetMxType()}");
                trace.Write(iTraceLevel, $"Priority	: {GetPriority()}");
                trace.Write(iTraceLevel, $"Sent		: {GetSent()}");
                trace.Write(iTraceLevel, $"------------------GENERIC M DATA--------------------");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }
        }
    }
}
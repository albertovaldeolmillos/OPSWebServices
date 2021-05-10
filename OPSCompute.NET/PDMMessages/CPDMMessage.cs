using PDMHelpers;
using System;

namespace PDMCompute
{
    public class CPDMMessage
    {
        public const int C_RES_OK = 1;
        public const int C_RES_NOK = -1;
        public const int C_RES_ERROR = -1;

        public const int C_TYPE_CMD = 1;
        public const int C_TYPE_ACK = 2;
        public const int C_TYPE_DATA = 3;
        public const int C_TYPE_QURY = 4;
        public const int C_TYPE_RESP = 5;
        public const int C_TYPE_NACK = 6;
        public const int C_TYPE_MAX = C_TYPE_NACK;
        public const int C_TYPE_MIN = C_TYPE_CMD;

        public const int C_TYPE_UNKNOW = C_RES_ERROR;
        public const int C_TYPE_CC = 1; //CONTROL CENTER 
        public const int C_TYPE_PD = 2;	//PERIOD TASK

        public const int C_DEVICE_UNKNOW = C_RES_ERROR;
        public const int C_DEVICE_CC = 1;   //CONTROL CENTER
        public const int C_DEVICE_PD = 2;	//PERIOD TASK


        private int m_iTelDev;
        private int m_iTelType;
        private string m_szBuffer;
        private ITraceable m_pTrace;   
        private int m_iSize;           
        private static int ms_iNextTelID;
        private static int ms_iNextMSPTelID;
        private int m_iTelID;
        private CPDMMessagesStatistics m_pStatColl;
        //TODO
        //private CCompressor m_oCompressor;
        private COPSDate m_dtQueueInsertionDate;
        
        private bool m_bIsWaitedFor;
        private Int64 m_i64Mask;
        public int SendingPriority { get; set; }

        public CPDMMessage(ILoggerManager loggerManager)
        {
            m_pTrace = loggerManager.CreateTracer(this.GetType());
        }

        public bool SetTelegram(string telegram, Int64 telegramLen)
        {
            m_pTrace.Write(TraceLevel.Info, "CPDMMessage::SetTelegram");
            bool fnResult = false;

            lock (this)
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(telegram)) 
                        throw new ArgumentNullException(nameof(telegram) , "Telegram parameter == NULL");

                    if (telegramLen <= 0)
                        throw new ArgumentOutOfRangeException(nameof(telegramLen), "telegramLen <= 0");

                    m_szBuffer = string.Empty;
                    m_szBuffer = telegram.Clone() as string;

                    m_iSize = m_szBuffer.Length;

                    m_iTelDev =  C_DEVICE_CC;
                    m_iTelType = C_TYPE_CC;

                    fnResult = true;
                }
                catch (Exception error)
                {
                    m_pTrace.Write(TraceLevel.Error, error.ToLogString());
                    fnResult = false;
                }
            }

            return fnResult;
        }
    }
}
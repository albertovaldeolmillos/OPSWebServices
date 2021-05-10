using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

namespace PDMMessages
{
    public class COPSPacket
    {
        public const int MAX_MSG_PACKET = 20;

        private readonly ILoggerManager loggerManager;
        private readonly ITraceable trace;

        private readonly List<KeyValuePair<string, string>> m_mpAtt;
        private readonly COPSMsg[] m_Msg;	
        private long m_nMsgCnt;

        public COPSPacket(ILoggerManager loggerManager)
        {
            this.loggerManager = loggerManager;
            trace = loggerManager.CreateTracer(this.GetType());

            m_Msg = new COPSMsg[MAX_MSG_PACKET];
            m_mpAtt = new List<KeyValuePair<string, string>>();
        }

        public string Parse(string packetXml)
        {
            trace.Write(TraceLevel.Info, "COPSPacket::Parse");

            string resultado = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(packetXml?.Trim()))
                {
                    throw new ArgumentNullException(nameof(packetXml));
                }

                XmlDocument xml = CreateWrappedXmlFromInput(packetXml);

                ExtractRootAttributesFromXml(xml);
                ExtractMessagesFromXml(xml);

                return resultado;
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                throw;
            }

            return resultado;
        }
        public void Reset() {
            m_mpAtt.Clear();
            m_nMsgCnt = 0;
            foreach (COPSMsg message in m_Msg.Where(w => w != null))
            {
                message.Reset();
            } 
        }

        public string fstrGetAtt(string strKey)
        {
            KeyValuePair<string, string> strVal;
            strVal = m_mpAtt.FirstOrDefault(f => f.Key == strKey);
            return strVal.Equals(default(KeyValuePair<string, string>)) ? null : strVal.Value;
        }
        public  List<KeyValuePair<string, string>> GetAtt()
        {
            return m_mpAtt;
        }
        public long GetMsgNum()
        {
            return m_nMsgCnt;
        }
        public COPSMsg GetMsg(long nId)
        {
            return nId < m_nMsgCnt ? m_Msg[nId] : null;
        }

        private XmlDocument CreateWrappedXmlFromInput(string packetXml)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml($"<p>{packetXml}</p>");
            return xml;
        }
        private bool IsRootNodeValid(XmlDocument xml)
        {
            return xml.FirstChild?.Name == "p";
        }
        private void ExtractRootAttributesFromXml(XmlDocument xml)
        {
            if (!IsRootNodeValid(xml))
            {
                throw new InvalidOperationException("Root no encontrado");
            }

            foreach (XmlAttribute attr in xml.FirstChild.Attributes)
            {
                trace.Write(TraceLevel.Info, $"Attr : {attr.Name} {attr.Value}");
                m_mpAtt.Add(new KeyValuePair<string, string>(attr.Name, attr.Value));
            }
        }
        private void ExtractMessagesFromXml(XmlDocument xml)
        {
            int nMsg = 0;
            XmlNode parentNode = xml.FirstChild;
            foreach (XmlNode mensaje in parentNode.ChildNodes)
            {
                nMsg++;
                m_nMsgCnt++;
                m_Msg[m_nMsgCnt - 1] = new COPSMsg(loggerManager, mensaje);
            }
        }
        
    }
}

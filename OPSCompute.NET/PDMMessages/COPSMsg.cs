using PDMHelpers;
using PDMHelpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace PDMMessages
{
    public enum MSG_N_TEXT {
        ap = 1,
        ne,
        nb
    }

    public enum MSG_M_TEXT
    {
        m1 = 1,
        m2,
        m3,
        m4,
        m5,
        m6,
        m7,
        m8,
        m9,
        m12,
        m14,
        m20,
        m21,
        m22,
        m23,
        m24,
        m25,
        m26,
        m27,
        m28,
        m29,
        m30,
        m31,
        m32,
        m33,
        m52,
        m59,
        m60,
        m62,
        m70,
        m71,
        m72,
        m73,
        m79,
        m80,
        m81,
        m83,
        m85,
        m90
    }

    public enum MessageType {
        Command = 0,
        Ack = 1
    }

    public class COPSMsg
    {
        private readonly ITraceable m_pTrace;

        public string m_strName;  // Nombre del mensaje
        public int? m_nMsgType; // Tipo del mensaje (CMD o ACK)
        public int? m_nCmdType; // Tipo de comando (M1, M2, ... )
        private COTSStrMap m_mpAtt; // Mapa de atributos
        private COTSStrMap m_mpElm; // Mapa de elementos

        public COPSMsg(ILoggerManager loggerManager)
        {
            m_pTrace = loggerManager.CreateTracer(this.GetType());

            m_mpAtt = new COTSStrMap();
            m_mpElm= new COTSStrMap();
        }
        public COPSMsg(ILoggerManager loggerManager, XmlNode xmlMessage) : this(loggerManager)
        {
            m_strName = xmlMessage.Name;
            m_pTrace.Write(TraceLevel.Info, $"Element: {m_strName }");

            SetMessageType();

            ExtractAttributesFromXmlNode(xmlMessage);
            ExtractElementsFromXmlNode(xmlMessage);
        }

        private void SetMessageType()
        {
            bool isMessage = typeof(MSG_M_TEXT).ExistValueFromString(m_strName);
            bool isN = typeof(MSG_N_TEXT).ExistValueFromString(m_strName);

            if (isMessage)
            {
                m_nMsgType = (int)MessageType.Command;
                m_nCmdType = typeof(MSG_M_TEXT).GetValueFromString<int>(m_strName);
            }

            if (isN)
            {
                m_nMsgType = (int)MessageType.Ack;
                m_nCmdType = typeof(MSG_N_TEXT).GetValueFromString<int>(m_strName);
            }
        }

        private void ExtractElementsFromXmlNode(XmlNode xmlMessage)
        {
            foreach (XmlNode properties in xmlMessage.ChildNodes)
            {
                m_pTrace.Write(TraceLevel.Info, $"Prop : {properties.Name} {properties.InnerText}");
                m_mpElm.Add(new KeyValuePair<string, string>(properties.Name, properties.InnerText));
            }
        }
        private void ExtractAttributesFromXmlNode(XmlNode xmlMessage)
        {
            foreach (XmlAttribute attr in xmlMessage.Attributes)
            {
                m_pTrace.Write(TraceLevel.Info, $"Attr : {attr.Name} {attr.Value}");
                m_mpAtt.Add(new KeyValuePair<string, string>(attr.Name, attr.Value));
            }
        }

        public void Reset() {
            m_nMsgType = null;
            m_nCmdType = null;

            m_strName = string.Empty;
            m_mpAtt.Clear();
            m_mpElm.Clear();
        }
        public string fstrGetName()
        {
            return m_strName;
        }
        public COTSStrMap GetAtt()
        {
            return m_mpAtt;
        }
        public string fstrGetAtt(string strKey)
        {
            return m_mpAtt.GetTCHAR(strKey);
        }

        public COTSStrMap GetElm()
        {
            return m_mpElm;
        }

        public string fstrGetElm(string strKey)
        {
            return m_mpElm.GetTCHAR(strKey);
        }
        public int GetMsgType() { return m_nMsgType.Value; }
        public int GetCmd() { return m_nCmdType.Value; }
    }
}

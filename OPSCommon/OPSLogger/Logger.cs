using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using log4net;

namespace OPS.Comm
{
    /// <summary>
    /// Summary description for LogWrapper.
    /// </summary>
    public class Logger : ILogger
    {
        #region -- Member variables --
        // Define a static logger variable so that it references the Logger instance
        private  ILog m_log = null;
        #endregion

        #region -- Constructor / Destructor  --
        public Logger(System.Type objType)
        {
            m_log = LogManager.GetLogger(objType);
            ConfigureFromSettings();
        }

        public Logger(System.Type objType, Dictionary<string, object> parametros)
        {
            m_log = LogManager.GetLogger(objType);
            ConfigureLog(parametros);
        }

        public Logger(System.Type objType, string configurationSectionName)
        {
            m_log = LogManager.GetLogger(objType);

            ConfigureFromSettings();

        }
        #endregion


        #region -- Configure Logger --
        private NameValueCollection ReadConfigurationFileSection(string configurationSectionName)
        {
            try
            {
                NameValueCollection logConfig = ConfigurationManager.GetSection(configurationSectionName) as NameValueCollection;
                return logConfig;
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException("No se ha encontrado la clave especificada", ex);
            }
        }

        private void ConfigureFromSettings(string configurationSectionName = "LogSettings") {
            try
            {
                IDictionary<string, object> parametros = new Dictionary<string, object>();
                NameValueCollection logConfigSection = ReadConfigurationFileSection(configurationSectionName);

                if (logConfigSection == null) { return; }

                foreach (var key in logConfigSection.AllKeys)
                {
                    parametros.Add(key, logConfigSection[key]);
                }

                ConfigureLog(parametros);
            }
            catch (Exception err)
            {
                log4net.Config.XmlConfigurator.Configure();
            } 
        }

        private void ConfigureLog(IEnumerable<KeyValuePair<string, object>> configuration)
        {
            try
            {
                foreach (KeyValuePair<string, object> parametro in configuration)
                {
                    log4net.GlobalContext.Properties[parametro.Key] = parametro.Value;
                }
                log4net.Config.XmlConfigurator.Configure();
            }
            catch (Exception err)
            {
                log4net.Config.XmlConfigurator.Configure();
            }
        }
        #endregion


        #region -- Trace functions --
        public void AddLog(string strMessage, LoggerSeverities severity)
        {
            switch (severity)
            {
                case LoggerSeverities.Debug: if (m_log.IsDebugEnabled) { m_log.Debug(strMessage); } break;
                case LoggerSeverities.Info: if (m_log.IsInfoEnabled) { m_log.Info(strMessage); }    break;
                case LoggerSeverities.Warning: if (m_log.IsWarnEnabled) { m_log.Warn(strMessage); }    break;
                case LoggerSeverities.Error: if (m_log.IsErrorEnabled) { m_log.Error(strMessage); } break;
                case LoggerSeverities.FatalError: if (m_log.IsFatalEnabled) { m_log.Fatal(strMessage); } break;
                default: /* DEBUG */	 if (m_log.IsDebugEnabled) { m_log.Debug(strMessage); }     break;
            }
        }

        public void AddLog(string strMessage, LoggerSeverities severity, Exception excObject)
        {
            switch (severity)
            {
                case LoggerSeverities.Debug: if (m_log.IsDebugEnabled) { m_log.Debug(strMessage, excObject); } break;
                case LoggerSeverities.Info: if (m_log.IsInfoEnabled) { m_log.Info(strMessage, excObject); } break;
                case LoggerSeverities.Warning: if (m_log.IsWarnEnabled) { m_log.Warn(strMessage, excObject); } break;
                case LoggerSeverities.Error: if (m_log.IsErrorEnabled) { m_log.Error(strMessage, excObject); } break;
                case LoggerSeverities.FatalError: if (m_log.IsFatalEnabled) { m_log.Fatal(strMessage, excObject); } break;
                default: /* DEBUG */	 if (m_log.IsDebugEnabled) { m_log.Debug(strMessage, excObject); } break;
            }
        }

        public void AddLog(string strMessage, Exception excObject)
        {
            string strException = excObject.GetType().ToString();

            if (m_log.IsErrorEnabled)
            {
                m_log.Error(strMessage, excObject);
            }
            else if (m_log.IsDebugEnabled)
            {
                m_log.Debug(strMessage, excObject);
            }
        }

        public void AddLog(Exception excObject)
        {
            this.AddLog(string.Empty, excObject);
        }
        

        #endregion
    }

}





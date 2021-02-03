using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;

namespace PDMHelpers
{
    public class ParamLoader
    {
        private readonly ITraceable _trace;
        private readonly Dictionary<string, string> m_Params;

        public ParamLoader(ILoggerManager loggerManager)
        {
            _trace = loggerManager.CreateTracer(this.GetType());
            m_Params = new Dictionary<string, string>();
        }

        /// <summary>
        /// Load application params from xml file specified by filepath 
        /// </summary>
        /// <param name="filepath">Xml file path </param>
        /// <returns>Bool with the result</returns>
        public virtual bool LoadParams(string filepath) {
            bool result = true;

            try
            {
                if (string.IsNullOrEmpty(filepath))
                {
                    throw new ArgumentException("ParamLoader filepath is null or empty", nameof(filepath));
                }
            
                if (!File.Exists(filepath)) {
                    throw new FileNotFoundException("File not found", filepath);
                }
           
                XmlDocument xmlDocument = LoadXmlFile(filepath);
                List<KeyValuePair<string, string>> parameters = ParseXmlToParams(xmlDocument);
                parameters.ForEach(p => InsertUpdateValue(p.Key, p.Value));

                _trace.Write(TraceLevel.Debug, $"LoadParams result is  : {result}");

                result = true;
            }
            catch (Exception error)
            {
                _trace.Write(TraceLevel.Error, error.ToLogString());
                result = false;
                throw;
            }

            return result;
        }

        private XmlDocument LoadXmlFile(string filepath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filepath);
            return xmlDocument;
        }

        private List<KeyValuePair<string, string>> ParseXmlToParams(XmlDocument xmlDocument)
        {
            XmlNode generalNode = xmlDocument.GetElementsByTagName("GENERAL").Item(0);

            var nodes = from XmlNode node in generalNode.ChildNodes
                        where node.NodeType == XmlNodeType.Element
                        select new KeyValuePair<string, string>(node.Attributes.GetNamedItem("NAME").Value, node.Attributes.GetNamedItem("VALUE").Value);

            return nodes.ToList();
        }

        /// <summary>
        /// Insert the parameter if it does not exist or update it if exits
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="paramValue">Parameter value</param>
        /// <returns>Result of the operation</returns>
        public bool InsertUpdateValue(string paramName, string paramValue) {
            m_Params.Add(paramName, paramValue);

            return true;
        }

        /// <summary>
        /// Gets a parameter value
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <returns>Parameter current value</returns>
        public string GetParam(string paramName) {

            if (!m_Params.ContainsKey(paramName)) {
                return null;
            }

            return m_Params[paramName];
        }
    }
}

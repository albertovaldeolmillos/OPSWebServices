using System;
using System.Xml;
using System.Collections;
using OPS.Comm;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// The interface for incoming messages validator objects
	/// </summary>
	public interface IMessageValidator
	{
		/// <summary>
		/// Checks the correctness of an incoming message
		/// </summary>
		/// <param name="packet">The datagram to validate</param>
		/// <returns>true if the message is correct, false otherwise</returns>
		bool Validate(OPSTelegrama packet);
	}

	/// <summary>
	/// An implementation of IMessageValidator that uses a set of XML schemas
	/// to validate the correctness of received messages
	/// </summary>
	public class FecsMessageValidator : IMessageValidator
	{
		#region Public API

		public FecsMessageValidator(string path)
		{
			LoadXMLSchemas(path);
		}
		/// <summary>
		/// Checks the correctness of an incoming message
		/// </summary>
		/// <param name="packet">The datagram to validate</param>
		/// <returns>true if the message is correct, false otherwise</returns>
		public bool Validate(OPSTelegrama packet)
		{
			// First of all, let's load the XML and let's check if it is well-formed
			System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
			try
			{
				doc.LoadXml(packet.XmlData);
			}
			catch
			{
				return false;
			}

			// For each of the messages in the packet...
			foreach(XmlNode item in doc.DocumentElement.ChildNodes)
			{
				// Second, is there any Schema that can apply to the document?
				if (!_xmlSchemas.ContainsKey(item.Name))
				{
					FecsMain.Logger.AddLog("[FecsMessageValidator::Validate]:There is no Schema",LoggerSeverities.Debug);
					return false;
				}

				// Third, let's validate against schema
				if (!((XmlSchemaCache)_xmlSchemas[item.Name]).ValidateXmlDocument(item))
				{
					FecsMain.Logger.AddLog("[FecsMessageValidator::Validate]:There is no Schema",LoggerSeverities.Debug);
					return false;
				}
			}
			return true;
		}

		#endregion // Public API

		#region Private methods

		private void LoadXMLSchemas(string path)
		{
			try 
			{
				string[] files = System.IO.Directory.GetFiles(path, "*.xsd");
				_xmlSchemas = new Hashtable(files.Length);

				foreach(string file in files) 
				{
					XmlSchemaCache aSchema = new XmlSchemaCache();
					if(!aSchema.Load(file))
					{
						//Logger.WriteLogMessage(3, "Cannot load XML Schema: {0}", file);
						FecsMain.Logger.AddLog("Cannot load XML Schema: " + file, LoggerSeverities.Error);
					}
					else
					{
						FecsMain.Logger.AddLog("Added" + file,LoggerSeverities.Debug);
						_xmlSchemas.Add(aSchema.Name, aSchema);
					}
				}
			} 
			catch(Exception ex) 
			{
				//Logger.WriteLogException(3, "Failed loading XML Schemas: {0}", e);
				FecsMain.Logger.AddLog(ex);
			}
		}

		#endregion // Private methods

		#region Private data members

		/// <summary>
		/// A cache for the schemas of known messages
		/// </summary>
		private Hashtable _xmlSchemas;

		#endregion // Private data members
	}
}

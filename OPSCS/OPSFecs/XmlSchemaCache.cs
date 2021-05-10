using System;
using System.Xml;
using System.Xml.Schema;
//using OPS.Comm.Common;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// Summary description for XmlSchemaCache.
	/// </summary>
	public class XmlSchemaCache
	{
		protected static bool _validation;
		protected string _name;
		protected XmlSchemaCollection _xsc;

		public XmlSchemaCache()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public bool Load(string path)
		{
			try
			{
				_xsc = new XmlSchemaCollection();
				_xsc.Add(null, path);
				System.IO.FileInfo fi = new System.IO.FileInfo(path);
				_name = fi.Name.Substring(0, fi.Name.Length - 4);
			}
			catch
			{
				return false;
			}
			return true;
		}

		public string Name
		{
			get { return _name; }
		}

		//		public XmlSchemaCollection XmlSchemaCollection
		//		{
		//			get { return _xsc; }
		//		}

		public bool ValidateXmlDocument(XmlNode fragment)
		{
			_validation = true;
			try
			{
				//Create the XmlParserContext.
				XmlParserContext context = new XmlParserContext(null, null, "", XmlSpace.None);
				//Implement the reader. 
				XmlValidatingReader reader = new XmlValidatingReader(fragment.OuterXml, XmlNodeType.Element, context);
				//Set the schema type and add the schema to the reader.
				reader.ValidationType = ValidationType.Schema;
				reader.ValidationEventHandler += new ValidationEventHandler(XmlSchemaCache.MyValidationEventHandler);
				reader.Schemas.Add(_xsc);
				//Start the validation
				while (reader.Read())
				{
				}
			}
			catch
			{
				return false;
			}
			return _validation;
		}

		private static void MyValidationEventHandler(object sender, ValidationEventArgs args)
		{
			_validation = false;
			FecsMain.Logger.AddLog("[XmlSchemaCache:MyValidationEventHandler]" + args.Message,OPS.Comm.LoggerSeverities.Debug);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

namespace OPS.Components.Data
{
	// XNA_08_10_2007
	/// <summary>
	/// Information on a field from the User Schema.
	/// </summary>
	public sealed class TableUserSchemaInfo
	{
		private ArrayList	_arrFieldUserInfo;
		private string		_where;

		public TableUserSchemaInfo(ArrayList arrFieldUserInfo, string where)
		{
			_arrFieldUserInfo = arrFieldUserInfo;	
			_where = where;
		}

		public ArrayList TableFieldUserSchemaInfo 
		{
			get { return _arrFieldUserInfo; }
		}
		public string Where 
		{
			get { return _where; }
		}
	}
	// XNA_08_10_2007

	/// <summary>
	/// Information on a field from the User Schema.
	/// </summary>
	public sealed class FieldUserSchemaInfo
	{
		private string _name;
		private bool   _visible;
		private int    _width;
		private string _align;
		private bool   _primaryKey;
		private string _relTable;
		private string _relField;
		private string _relShown;
		private bool   _relTranslate;
		private bool   _bReadOnly = false;


		public FieldUserSchemaInfo(string name, bool visible, int width, string align, bool primaryKey, 
			string relTable, string relField, string relShown, bool relTranslate, bool bReadOnly)
		{
			_name = name;	
			_visible = visible;
			_width = width;
			_align = align;
			_primaryKey = primaryKey;
			_relTable = relTable;
			_relField = relField;
			_relShown = relShown;
			_relTranslate = relTranslate;
			_bReadOnly = bReadOnly;
		}

		public string FieldName 
		{
			get { return _name; }
		}
		public bool FieldVisible 
		{
			get { return _visible; }
		}
		public int FieldWidth
		{
			get { return _width; }
		}
		public string FieldAlign
		{
			get { return _align; }
		}
		public bool FieldPrimaryKey
		{
			get { return _primaryKey; }
		}
		public string FieldRelTable
		{
			get { return _relTable; }
		}
		public string FieldRelField
		{
			get { return _relField; }
		}
		public string FieldRelShown
		{
			get { return _relShown; }
		}
		public bool FieldRelTranslate
		{
			get { return _relTranslate; }
		}
			
		public bool FieldReadOnly
		{
			get { return _bReadOnly; }
		}
	}


	/// <summary>
	/// Allows reading the schema of any source.
	/// </summary>
	interface IUserSchemaReader
	{
		/// <summary>
		/// Gets the names of all the tables we work with.
		/// </summary>
		/// <returns>An array with names of all the tables</returns>
		StringCollection GetTables();

		/// <summary>
		/// Retrieves information about the schema of a table.
		/// </summary>
		/// <param name="table">Name of the table.</param>
		/// <returns>Schema of the all the columns of the table as a Hasthable of FieldUserSchemaInfo objects.</returns>
		ArrayList GetTableSchema(string table);
	}


	/// <summary>
	/// Reads the User Schema from an Xml.
	/// </summary>
	public sealed class XmlUserSchemaReader : IUserSchemaReader
	{
		/// <summary>
		/// The Xml document loaded in the constructor.
		/// </summary>
		private XmlDocument _doc;

		/// <summary>
		/// Creates a new XmlUserSchemaReader.
		/// </summary>
		/// <param name="source">The path to the Xml file containing the data.</param>
		internal XmlUserSchemaReader(string source)
		{
			_doc = new XmlDocument();
			_doc.Load(source);
		}

		/// <summary>
		/// Gets the names of the tables for the User Schema.
		/// </summary>
		/// <returns>A StringCollection with the names of the tables.</returns>
		public StringCollection GetTables()
		{
			StringCollection tables = new StringCollection();

			XmlNodeList tableNodeList = _doc.DocumentElement.ChildNodes;
			foreach (XmlNode tableNode in tableNodeList)
			{
				XmlElement tableElement = (XmlElement) tableNode;
				tables.Add(tableElement.GetAttribute("name"));
			}
			
			return tables;
		}
		
		/// <summary>
		/// Returns the schema of a table.
		/// </summary>
		/// <param name="table">Table to retrieve.</param>
		/// <returns>An ArrayList of FieldUserSchemaInfo objects.</returns>
		public ArrayList GetTableSchema(string table)
		{
			ArrayList arrayFieldUser = new ArrayList();
			ArrayList arraySchema = new ArrayList();

			XmlNodeList fieldNodeList = _doc.SelectNodes("//table[@name='" + table + "']/field");
			foreach (XmlNode fieldNode in fieldNodeList)
			{
				XmlElement fieldElement = (XmlElement) fieldNode;
				string name = fieldElement.GetAttribute("name");
				bool visible = false;
				if (fieldElement.GetAttribute("visible") == "true")
					visible = true;
				int width = 0;
				if (fieldElement.GetAttribute("width") != "")
					width = int.Parse(fieldElement.GetAttribute("width"));
				string align = fieldElement.GetAttribute("align");
				bool primaryKey = false;
				if (fieldElement.GetAttribute("primarykey") == "true")
					primaryKey = true;
				string relTable = fieldElement.GetAttribute("reltable");
				string relField = fieldElement.GetAttribute("relfield");
				string relShown = fieldElement.GetAttribute("relshown");
				bool relTranslate = false;
				if (fieldElement.GetAttribute("reltranslate") == "true")
					relTranslate = true;
				bool bReadOnly = false;
				if(primaryKey)
				{
					string res = fieldElement.GetAttribute("readonly");
					if ( res == "true")
						bReadOnly = true;
				}

				FieldUserSchemaInfo fieldInfo = new FieldUserSchemaInfo(name, visible, width, align, primaryKey, 
					relTable, relField, relShown, relTranslate,bReadOnly);
				// // XNA_08_10_2007
				arrayFieldUser.Add(fieldInfo);
				// // XNA_08_10_2007
			}

			// XNA_08_10_2007
			string filter = null;
			XmlNode filterNode = _doc.SelectSingleNode("//table[@name='" + table + "']/filter");
			if (filterNode != null)
			{
				XmlElement filterElement = (XmlElement) filterNode;

				filter = filterElement.GetAttribute("where"); 
			}

			TableUserSchemaInfo tableInfo = new TableUserSchemaInfo(arrayFieldUser, filter);

			arraySchema.Add(tableInfo);
			// XNA_08_10_2007

			return arraySchema;
		}
	}

}

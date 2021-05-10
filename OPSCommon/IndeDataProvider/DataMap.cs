using System;

namespace OPS.Components.Data
{
	/// <summary>
	/// DataMap class makes a translation between logical names used in the Data Layer and
	/// physical names used in the Data Source
	/// </summary>
	public class DataMap
	{
		protected string _tablename;
		protected string _logicname;
		protected DataEntryCollection _entries;
		/// <summary>
		/// Creates a new DataMap to maintain a relationship between a physic table in the Data Source
		/// and a logic name in the Data Layer
		/// </summary>
		/// <param name="tn">Phyisical Name of the table in the Data Source</param>
		/// <param name="ln">Logical Name to use</param>
		public DataMap(string tn, string ln)
		{
			_tablename = tn;
			_logicname = ln;
			_entries = new DataEntryCollection();
		}

		/// <summary>
		/// Physical name of the table in the Database (read-only property)
		/// </summary>
		public string TableName 
		{
			get 
			{
				return _tablename;
			}
		}
		/// <summary>
		/// Logical name to be used in the Data Layer (read-only property)
		/// </summary>
		public string LogicName 
		{
			get 
			{
				return _logicname;
			}
		}
		/// <summary>
		/// Collection with all DataEntry objects
		/// </summary>
		public DataEntryCollection Entries 
		{
			get 
			{
				return _entries;
			}
		}
	}
}

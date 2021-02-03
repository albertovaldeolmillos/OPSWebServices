using System;
using System.Data;

namespace OPS.Components.Data
{
	/// <summary>
	/// DataEntry class mantains a relation between a physic field of a table, and a logical
	/// name used in the Data Layer
	/// </summary>
	public class DataEntry 
	{
		public const int NoSize = -1;
		/// <summary>
		/// Logical name of the field
		/// </summary>
		protected string _name;
		/// <summary>
		/// Physical name of the field
		/// </summary>
		protected string _colname;
	
		/// <summary>
		/// System.Data.DbType of the Parameter
		/// </summary>
		protected DbType _type;

		/// <summary>
		/// Size of the parameter
		/// </summary>
		protected int _size;

		public DataEntry (string colname, string logicname, DbType type, int size) 
		{
			_colname = colname;
			_name = logicname;
			_type = type;
			_size = size;
		}

		public DataEntry (string colname, string logicname, DbType type) 
		{
			_colname = colname;
			_name = logicname;
			_type = type;
			_size = -1;
		}


		public string LogicName 
		{
			get {return _name;}
		}
		public string ColumnName 
		{
			get {return _colname;}
		}
		public DbType Type 
		{
			get {return _type;}
		}
		/// <summary>
		/// Size (in bytes) of the parameter. -1 if no size applied
		/// </summary>
		public int Size
		{
			get {return _size;}
			set {
				if (value < NoSize) throw new ArgumentOutOfRangeException ("value",value,"DataEntry.NoSize is the only non-positive value accepted");
				_size = value;
			}
		}
	}
}

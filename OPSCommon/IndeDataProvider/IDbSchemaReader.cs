using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data.SqlClient;
//using Oracle.DataAccess.Client;

namespace OPS.Components.Data
{
	/// <summary>
	/// Schema of a field of the Database.
	/// </summary>
	public abstract class FieldSchemaInfo
	{
		protected string _name;
		protected string _type;
		protected int _size;
		protected bool _nullable;
		public FieldSchemaInfo (string name, string type, int size, bool nullable)
		{
			_name = name;
			_type = type;
			_size = size;
			_nullable = nullable;
		}

		public string FieldName 
		{
			get { return _name; }
		}	
		public string FieldTypeName 
		{
			get  { return _type; }
		}
		public int FieldSize 
		{
			get { return _size; }
		}
		public bool FieldNullable
		{
			get { return _nullable; }
		}
		public abstract object NativeType {get;}
	}

	/// <summary>
	/// Schema of a Field of an Oracle Database
	/// </summary>
	public sealed class OracleFieldSchemaInfo : FieldSchemaInfo
	{

        private OracleDbType _nativeType;
		
		public override object NativeType 
		{
			get { return _nativeType; }
		}

		public OracleFieldSchemaInfo(string name, string type, int size, bool nullable) : base(name, type, size, nullable)
		{
			// Oracle uses 0 for non-string types (and we are using -1 when no size info is needed)
			if (_size == 0) _size = -1;

            try
            {
                _nativeType = OTS.Data.OracleTypesHelper.Convert(type);
            }
            catch (DatabaseLoadException ex)
            {
                throw new DatabaseLoadException("Error loading schema - Oracle type '" + _type + "' is not implemented (Field='" + _name + "'.");
            }
		}
	}

	/// <summary>
	/// Schema of a Field of a SQL Server Database
	/// </summary>
	public sealed class SqlFieldSchemaInfo : FieldSchemaInfo 
	{
		private System.Data.SqlDbType _nativeType;

		public override object NativeType 
		{
			get { return _nativeType; }
		}

		public SqlFieldSchemaInfo(string name, string type, int size, bool nullable) : base(name, type, size, nullable) 
		{
			// Choose an SQL type based on _type value;
			switch (_type) 
			{
				case "bigint":
					_nativeType =  System.Data.SqlDbType.BigInt;
					break;
				case "binary":
					_nativeType =  System.Data.SqlDbType.Binary;
					break;
				case "bit":
					_nativeType =  System.Data.SqlDbType.Bit;
					break;
				case "char":
					_nativeType =  System.Data.SqlDbType.Char;
					break;
				case "datetime":
					_nativeType =  System.Data.SqlDbType.DateTime;
					break;
				case "decimal":
					_nativeType =  System.Data.SqlDbType.Decimal;
					break;
				case "float":
					_nativeType =  System.Data.SqlDbType.Float;
				break;
				case "image":
					_nativeType =  System.Data.SqlDbType.Image;
					break;
				case "int":
					_nativeType =  System.Data.SqlDbType.Int;
					break;
				case "money":
					_nativeType =  System.Data.SqlDbType.Money;
					break;
				case "nchar":
					_nativeType =  System.Data.SqlDbType.NChar;
					break;
				case "ntext":
					_nativeType =  System.Data.SqlDbType.NText;
					break;
				/*case "numeric":
					break;*/
				case "nvarchar":
					_nativeType =  System.Data.SqlDbType.NVarChar;
					break;
				case "real":
					_nativeType =  System.Data.SqlDbType.Real;
					break;
				case "smalldatetime":
					_nativeType =  System.Data.SqlDbType.SmallDateTime;
					break;
				case "smallint":
					_nativeType =  System.Data.SqlDbType.SmallInt;
					break;
				case "smallmoney":
					_nativeType =  System.Data.SqlDbType.SmallMoney;
					break;
				case "sql_variant":
					_nativeType =  System.Data.SqlDbType.Variant;
					break;
				case "text":
					_nativeType =  System.Data.SqlDbType.Text;
					break;
				case "timestamp":
					_nativeType =  System.Data.SqlDbType.Timestamp;
					break;
				case "tinyint":
					_nativeType =  System.Data.SqlDbType.TinyInt;
					break;
				case "uniqueidentifier":
					_nativeType =  System.Data.SqlDbType.UniqueIdentifier;
					break;
				case "varbinary":
					_nativeType =  System.Data.SqlDbType.VarBinary;
					break;
				case "varchar":
					_nativeType =  System.Data.SqlDbType.VarChar;
					break;
				default:
					throw new DatabaseLoadException ("Error loading schema - SQL-Server type '" + _type + "' is not implemented (Field='" + _name +"'.");
			}
		}
	}


	/// <summary>
	/// Allows reading the schema of a Database.
	/// </summary>
	interface IDbSchemaReader
	{
		/// <summary>
		/// Gets the names of all tables of the Database.
		/// </summary>
		/// <returns>A StringCollection with names of all the tables.</returns>
		StringCollection GetTables();

		/// <summary>
		/// Retrieves information about the schema of a table.
		/// </summary>
		/// <param name="table">Name of the table.</param>
		/// <returns>Schema of the table (columns, type, size) as a Hasthable of FieldSchemaInfo objects.</returns>
		Hashtable GetTableSchema(string table);
	}

	sealed class OracleSchemaReader : IDbSchemaReader
	{
		private string _scon;
		private string _owner;
		private string _tablespaceName;

		internal OracleSchemaReader (string scon, string owner, string tablespaceName) 
		{
			_scon = scon;
			_owner = owner;
			_tablespaceName = tablespaceName;
		}

		public StringCollection GetTables()
		{
            OracleConnection con = new OracleConnection(_scon);
			// Read the Oracle catalog to search for all tables beloning the namespace and owner specified
			// CFE - 20/11/2004
			/*
			 * OracleCommand cmd = new OracleCommand ("SELECT * FROM ALL_TABLES WHERE OWNER='" + 
													_owner + "' AND TABLESPACE_NAME='" + _tablespaceName + "'");
			*/
			
			OracleCommand cmd = new OracleCommand (string.Format("SELECT TABLE_NAME FROM ALL_TABLES where owner='{0}' " +
																 "UNION " +
																 "SELECT VIEW_NAME FROM ALL_VIEWS where owner='{0}'",_owner));
			cmd.Connection = con;
			cmd.Connection.Open();
			OracleDataReader dr = cmd.ExecuteReader();
			StringCollection tables = new StringCollection();
			while (dr.Read()) 
			{
				tables.Add (dr["TABLE_NAME"].ToString());
			}
			dr.Close();
			con.Close();
			return tables;
		}
		
		public Hashtable GetTableSchema(string table)
		{
            OracleConnection con = new OracleConnection(_scon);
			Hashtable tableSchema = new Hashtable();
            // Read the info about the table in the Oracle Catalog
            OracleCommand cmd = new OracleCommand("select * from ALL_TAB_COLUMNS where TABLE_NAME = '" + 
													table + "' AND OWNER = '" + _owner + "' ORDER BY COLUMN_ID");
			cmd.Connection = con;
			cmd.Connection.Open();			

			OracleDataReader dr = cmd.ExecuteReader();
			while (dr.Read()) 
			{
				string fieldName = dr["COLUMN_NAME"].ToString();
				tableSchema.Add(
					fieldName, 
					new OracleFieldSchemaInfo(
						fieldName, 
						dr["DATA_TYPE"].ToString(), 
						Int32.Parse(dr["CHAR_LENGTH"].ToString()),
						dr["NULLABLE"].ToString()=="Y"));
			}
			dr.Close();
			con.Close();

			return tableSchema;
		}
	}

	/// <summary>
	/// Reads the schema of a SQL Server Database
	/// </summary>
	sealed class SqlSchemaReader : IDbSchemaReader
	{
		private string _scon;

		internal SqlSchemaReader (string scon) 
		{
			_scon = scon;
		}

		public StringCollection GetTables()
		{
			SqlConnection con = new SqlConnection (_scon);
			// Read the SQL Catalog searching for tables
			SqlCommand cmd = new SqlCommand ("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_nativeType = 'BASE TABLE' ORDER BY TABLE_nativeType");
			cmd.Connection = con;
			cmd.Connection.Open();
			SqlDataReader dr = cmd.ExecuteReader();
			StringCollection tables = new StringCollection();
			while (dr.Read()) 
			{
				tables.Add (dr["TABLE_NAME"].ToString());
			}
			dr.Close();
			return tables;
		}

		public Hashtable GetTableSchema(string table)
		{
			SqlConnection con = new SqlConnection(_scon);
			Hashtable tableSchema = new Hashtable();
			// Read the info about the table in the SQL Catalog
			SqlCommand cmd = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + table + "' ORDER BY ORDINAL_POSITION");
			cmd.Connection = con;
			cmd.Connection.Open();			
			SqlDataReader dr = cmd.ExecuteReader();
			while (dr.Read()) 
			{
				string fieldName = dr["COLUMN_NAME"].ToString();
				object osize = dr["CHARACTER_MAXIMUM_LENGTH"];
				tableSchema.Add(fieldName, new SqlFieldSchemaInfo(
					fieldName, 
					dr["DATA_TYPE"].ToString(), 
					osize!=DBNull.Value ? (int)osize : -1,
					bool.Parse(dr["IS_NULLABLE"].ToString())));
			}
			dr.Close();
			return tableSchema;
		}
	}
}

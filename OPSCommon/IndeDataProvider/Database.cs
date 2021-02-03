using System;
using System.Data;
using System.Data.Common;
using System.Xml;

using System.Collections;
using OPS.Components.Data;

namespace OPS.Components.Data
{

	public class DatabaseLoadException : Exception
	{
		public DatabaseLoadException (string s) : base(s) {}
		public DatabaseLoadException() : base () {}
		public DatabaseLoadException (string s, Exception inner) : base (s, inner) { }
	}
	/// <summary>
	/// The Database class is the primary class of OPS.Components.Data namespace
	/// It contains a correspondence between logical names and physical names, and also a methods to obtain
	/// data provider specific objects
	/// </summary>
	public class Database
	{
		
		/// <summary>
		/// Data Providers that can be used
		/// </summary>
		public enum DataProvider
		{
			OLEDB = 1,
			SQL = 2,
			ORACLE = 3,
			ODBC = 4
		}

		protected IDataProviderSupport _dataprovider;
		protected DataProvider _dp;

		/// <summary>
		/// Contains the schema of the Database. It's a Hashtable of Hashtables.
		/// </summary>
		protected System.Collections.Hashtable _databaseSchema;

		/// <summary>
		/// Contains the schema of the user. Hashtable of Hashtables.
		/// </summary>
		protected System.Collections.Hashtable _userSchema;

		protected string _ownerStr;
		protected string _conStr;
		protected static DbType[] _dbtypes = {DbType.AnsiString, DbType.AnsiStringFixedLength, DbType.Binary, DbType.Boolean,	// 0-3
												  DbType.Byte, DbType.Currency, DbType.Date, DbType.DateTime, DbType.Decimal,	// 4-8
												  DbType.Double, DbType.Guid, DbType.Int16, DbType.Int32, DbType.Int64,			// 9-13
												  DbType.Object, DbType.SByte, DbType.Single, DbType.String,					// 14-17
												  DbType.StringFixedLength, DbType.Time, DbType.UInt16, DbType.UInt32,			// 18-21
												  DbType.UInt64, DbType.VarNumeric};											// 22-23

		#region Database Schema functions

		/// <summary>
		/// Loads and fills the Database schema (tables, fields, and types)
		/// </summary>
		public void LoadDatabaseSchema()
		{
			IDbSchemaReader schemaReader = null;
			switch (_dp) 
			{
				case DataProvider.SQL:
					schemaReader = new SqlSchemaReader(_conStr);
					break;
				case DataProvider.ORACLE:
					schemaReader = new OracleSchemaReader(_conStr, _ownerStr,"USERS");
					break;
			}

			System.Collections.Specialized.StringCollection tables = schemaReader.GetTables();
			_databaseSchema = new System.Collections.Hashtable (tables.Count);
			foreach (string table in tables) 
			{
				_databaseSchema.Add (table, schemaReader.GetTableSchema (table));
			}
		}

		/// <summary>
		/// Returns a reference to the schema of the table specified
		/// The schema is a Hashtable containing FieldSchemaInfo-derived objects
		/// </summary>
		public Hashtable this[string tablename]
		{
			get { return (Hashtable)_databaseSchema[tablename];}
		}

		/// <summary>
		/// Gets the info about a field of a specified table
		/// </summary>
		/// <param name="table">Name of the table </param>
		/// <param name="field">Field of the table</param>
		/// <returns>Info about the field (FieldSchemaInfo-derived object)</returns>
		public FieldSchemaInfo GetFieldInfo (string table, string field)
		{
			return (FieldSchemaInfo)((Hashtable)_databaseSchema[table])[field];
		}

		/// <summary>
		/// Gets the schema of the database (as a Hashtable of Hashtables).
		/// </summary>
		public Hashtable Schema 
		{
			get { return _databaseSchema; }
		}

		#endregion

		#region User Schema functions

		/// <summary>
		/// Loads the User Schema of the database.
		/// The User Schema is a customized view of the metadata of the database.
		/// </summary>
		/// <param name="source">The source (file path, etc.) to load.</param>
		public void LoadUserSchema(string source)
		{
			// Up to the moment we only have on possible User Schema reader: the Xml one
			IUserSchemaReader schemaReader = new XmlUserSchemaReader(source);

			// Get all the table names
			System.Collections.Specialized.StringCollection tables = schemaReader.GetTables();

			// Read the schema of each of the tables
			_userSchema = new System.Collections.Hashtable(tables.Count);

			foreach (string table in tables) 
			{
				_userSchema.Add(table, schemaReader.GetTableSchema(table));
			}
		}

		/// <summary>
		/// Gets the User Schema (as a Hashtable of Hashtables).
		/// </summary>
		public Hashtable UserSchema 
		{
			get { return _userSchema; }
		}

		#endregion

		/// <summary>
		/// Gets the connection string used
		/// </summary>
		public string ConnectionString 
		{
			get {return _conStr;}
		}

		/// <summary>
		/// Gets the schema owner string used
		/// </summary>
		public string SchemaOwner
		{
			get {return _ownerStr;}
		}

		/// <summary>
		/// Gets the Data Provider used (OLEDB, ORACLE, SQLSERVER, ODBC)
		/// </summary>
		public DataProvider DataProviderUsed
		{
			get {return _dp;}
		}

		/// <summary>
		/// Retrieves a new connection to the datasource. The connection must be opened by the caller
		/// </summary>
		/// <returns>The new connection</returns>
		public IDbConnection GetNewConnection() 
		{
			return _dataprovider.GetNewConnection();
		}

		/// <summary>
		/// Constructor for Database class
		/// </summary>
		/// <param name="d">DataProvider used</param>
		/// <param name="scon">Connection string</param>
		public Database (DataProvider d, string scon, string sowner) 
		{
			_conStr = scon;
			_dp = d;
			_ownerStr = sowner;
			switch (d) 
			{
				case DataProvider.OLEDB:
					_dataprovider = new OPS.Components.Data.OleDb.OleDbDataProviderSupport(scon);
					break;
				case DataProvider.ORACLE:
					_dataprovider = new OPS.Components.Data.Oracle.OracleDataProviderSupport(scon);

					break;
				case DataProvider.SQL:
					_dataprovider = new OPS.Components.Data.SqlClient.SqlDataProviderSupport(scon);
					break;
					/*
				case DataProvider.ODBC:
					_dataprovider = new OPS.Components.Data.ODBC.ODBCDataProviderSupport(scon);
					break;
					*/
			}
		}

		/// <summary>
		/// Receive a logical SQL statement and generates a Command object to execute that SQL
		/// The command has the ParametersCollection filled, but no value in the parameters
		/// </summary>
		/// <param name="ssql">Logical SQL statement to execute</param>
		/// <param name="addCon">If true a new connection is created and used for the Command</param>
		/// <returns>The Data Provider specific Command object</returns>
		public IDbCommand PrepareCommand(string ssql, bool addCon) 
		{
			return PrepareCommand (ssql,addCon, null);
		}

		/// <summary>
		/// Receive a logical SQL statement and generates a Command object to execute that SQL
		/// The command has the ParametersCollection filled, but no value in the parameters
		/// </summary>
		/// <param name="ssql">Logical SQL statement to execute</param>
		/// <returns>The Data Provider specific Command object</returns>
		public IDbCommand PrepareCommand (string ssql) 
		{
			return PrepareCommand (ssql, true, null);
		}

		/// <summary>
		/// Receive a logical SQL statement and generates a Command object to execute that SQL
		/// The command has the ParametersCollection filled, and the specified values in each Parameter object		/// </summary>
		/// <param name="ssql">Logical SQL statement to execute</param>
		/// <param name="values">Array of objects with the values of ALL parameters</param>
		/// <returns>The Data Provider specific Command object</returns>
		public IDbCommand PrepareCommand (string ssql, params object [] values) 
		{
			return PrepareCommand (ssql,true,values);
		}

		/// <summary>
		/// Receive a logical SQL statement and generates a Command object to execute that SQL
		/// The command has the ParametersCollection filled, and the specified values in each Parameter object
		/// </summary>
		/// <param name="ssql">Logical SQL statement to execute</param>
		/// <param name="addCon">If true a new connection is created and used for the Command</param>
		/// <param name="values">Array of objects with the values of ALL parameters</param>
		/// <returns>The Data Provider specific Command object</returns>
		public IDbCommand PrepareCommand (string ssql, bool addCon, params object [] values) 
		{
			System.Collections.ArrayList pars;
			IDbCommand cmd = _dataprovider.GetNewCommand();
			if (addCon) cmd.Connection = _dataprovider.GetNewConnection();
			cmd.CommandText = _dataprovider.TranslateSQL (ssql, _databaseSchema, out pars);
			
			if (pars!=null) 
			{
				for (int i=0; i<pars.Count;i++) 
				{
					cmd.Parameters.Add (pars[i]);
				}
			}
			if (values != null) 
			{
				int i=0;
				foreach (IDataParameter p in cmd.Parameters) 
				{
					p.Value = values[i++];
				}
			}
			return cmd;
		}
		
		/// <summary>
		/// Execute a logical SELECT statement
		/// </summary>
		/// <param name="query">Logical SQL SELECT statement</param>
		/// <param name="values">Values of the parameters (if any)</param>
		/// <returns>A specific Data-Provider IDataReader object</returns>
		public IDataReader ExecQuery (string query, params object [] values)
		{
			return ExecQuery (query, null, values);
		}


		/// <summary>
		/// Execute a logical SELECT statement
		/// </summary>
		/// <param name="query">Logical SQL SELECT statement</param>
		/// <param name="con">A Connection object. The connection MUST be opened by the caller</param>
		/// <param name="values">Values of the parameters (if any)</param>
		/// <returns>A specific Data-Provider IDataReader object. Closing the data reader will close the connection too.</returns>
		public IDataReader ExecQuery (string query, IDbConnection con, params object[] values)
		{
			bool bAddcon = (con==null);
			IDbCommand cmd = PrepareCommand (query, bAddcon, values);
			if (bAddcon) 
			{
				cmd.Connection.Open();
			}
			else
			{
				cmd.Connection = con;
			}
			IDataReader ret = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			return ret; 
		}

		/// <summary>
		/// Execute a logical scalar SELECT (like SELECT COUNT (*)) 
		/// </summary>
		/// <param name="ssql">Logical SQL SELECT statement</param>
		/// <param name="values">Values of the parameters (if any)</param>
		/// <returns>The value of the SQL Statement</returns>
		public object ExecuteScalar(string ssql, params object [] values) 
		{
			return ExecuteScalar (ssql, null, null, values);
		}

		public object ExecuteScalar(string ssql, IDbTransaction tran,params object [] values) 
		{
			return ExecuteScalar (ssql, tran.Connection, tran, values);
		}


		/// <summary>
		/// Execute a logical scalar SELECT (like SELECT COUNT (*)) 
		/// </summary>
		/// <param name="ssql">Logical SQL SELECT statement</param>
		/// <param name="con">A Connection object. The connection MUST be opened by the caller, and closed by the caller</param>
		/// <param name="tran">Transaction object used. Must be created by the Connection object passed in con parameter</param>		
		/// <param name="values">Values of the parameters (if any)</param>
		/// <returns>The value of the SQL Statement</returns>
		public object ExecuteScalar(string ssql, IDbConnection con, IDbTransaction tran, params object [] values) 
		{
			bool bAddCon = (con==null);
			IDbCommand cmd = PrepareCommand (ssql, bAddCon, values);
			if (bAddCon) 
			{
				cmd.Connection.Open();
			}
			else
			{
				cmd.Connection = con;
				if (tran!=null) cmd.Transaction = tran;
			}

			try 
			{
				return cmd.ExecuteScalar();
			}
			finally 
			{
				if (bAddCon) cmd.Connection.Close();
			}
		}


		/// <summary>
		/// Execute a logical non SELECT statement
		/// </summary>
		/// <param name="ssql">Logical SQL non SELECT statement</param>
		/// <param name="values">Values of the parameters (if any)</param>
		/// <returns>The number of rows affected by the SQL Statement</returns>
		public int ExecuteNonQuery (string ssql, params object [] values) 
		{
			IDbCommand cmd = PrepareCommand (ssql, true, values);
			try
			{
				cmd.Connection = _dataprovider.GetNewConnection();
				cmd.Connection.Open();
				using (cmd.Connection) 
				{
					return cmd.ExecuteNonQuery(); 
				}
			}
			finally
			{
				cmd.Connection.Close();
			}
		}

		/// <summary>
		/// Execute a logical non SELECT statement
		/// </summary>
		/// <param name="ssql">Logical SQL non SELECT statement</param>
		/// <param name="con">A Connection object. The connection MUST be opened by the caller, and closed by the caller</param>
		/// <param name="values">Values of the parameters (if any)</param>
		/// <returns>The number of rows affected by the SQL Statement</returns>
		public int ExecuteNonQuery (string ssql, IDbConnection con, params object [] values) 
		{
			return ExecuteNonQuery (ssql, con, null, values);
		}

		/// <summary>
		/// Execute a logical non SELECT statement
		/// </summary>
		/// <param name="ssql">Logical SQL non SELECT statement</param>
		/// <param name="con">A Connection object. The connection MUST be opened by the caller, and closed by the caller</param>
		/// <param name="tran">Transaction object used. Must be created by the Connection object passed in con parameter</param>
		/// <param name="values">Values of the parameters (if any)</param>
		/// <returns>The number of rows affected by the SQL Statement</returns>
		public int ExecuteNonQuery (string ssql, IDbConnection con, IDbTransaction tran, params object [] values) 
		{
			bool bAddCon = (con==null);
			IDbCommand cmd = PrepareCommand (ssql, bAddCon, values);
			if (bAddCon) cmd.Connection.Open();
			else 
			{
				cmd.Connection = con;
				if (tran!=null) cmd.Transaction = tran;
			}


			try 
			{
				int ret = cmd.ExecuteNonQuery();
				return ret;
			}
			/*catch( System.Exception e )
			{
				if( bAddCon ) cmd.Connection.Close();
				return 0;
			}*/
			finally 
			{
				if (bAddCon) cmd.Connection.Close();
			}
		}



		/// <summary>
		/// Returns a Data Provider specific Data Adapter object with NO commands
		/// </summary>
		/// <returns>The newly DataAdpater created</returns>
		public IDbDataAdapter GetDataAdapter() 
		{
			return _dataprovider.GetNewDataAdapter();
		}

		/// <summary>
		/// Returns a Data Provider sepecific Data Adapter object with the Select Command Specified
		/// </summary>
		/// <param name="ssql">Logical SQL select statement</param>
		/// <returns>The specific Data Adapter with a SelectCommand filled</returns>
		public IDbDataAdapter GetDataAdapter(string select) 
		{
			IDbCommand cmd = PrepareCommand (select);
			IDbDataAdapter da = _dataprovider.GetNewDataAdapter();
			da.SelectCommand = cmd;
			return da;
		}


		/// <summary>
		/// Returns a DataSet with the information specified
		/// That overload can be use to execute specific SQL sentences (that differs in syntaxis
		/// from one database to another).
		/// </summary>
		/// <param name="rawsql">Logical raw SQL sentence to use (including select, where, order by and subqueries)</param>
		/// <param name="table">Table to populate in DataSet</param>
		/// <returns>A new DataSet with the results</returns>
		public DataSet FillDataSet (string rawsql, string table)
		{
			return FillDataSet (rawsql, table, true,null);
		}


		/// <summary>
		/// Returns a DataTable with the information specified
		/// That overload can be use to execute specific SQL sentences (that differs in syntaxis
		/// from one database to another).
		/// </summary>
		/// <param name="rawsql">Logical raw SQL sentence to use (including select, where, order by and subqueries)</param>
		/// <param name="name">Name of the DataTable created</param>
		/// <returns>A new DataTable with the results</returns>
		public DataTable FillDataTable (string rawsql, string name)
		{
			return FillDataTable (rawsql, name, false,null);
		}


		/// <summary>
		/// Returns a DataSet with the information specified
		/// That overload can be use to execute specific SQL sentences (that differs in syntaxis
		/// from one database to another).
		/// </summary>
		/// <param name="rawsql">Logical raw SQL sentence to use (including select, where, order by and subqueries)</param>
		/// <param name="table">Table to populate in DataSet</param>
		/// <param name="values">Values of the parameters used in the SQL sentence (null if no parameters)</param>
		/// <returns>A new DataSet with the results</returns>
		public DataSet FillDataSet (string rawsql, string table, params object[] values)
		{
			DataTable dt = FillDataTable (rawsql, table, values);
			dt.TableName = table;
			DataSet ds = new DataSet();
			ds.Tables.Add (dt);
			return ds;
		}

		/// <summary>
		/// Returns a DataTable with the information specified
		/// That overload can be use to execute specific SQL sentences (that differs in syntaxis
		/// from one database to another).
		/// </summary>
		/// <param name="rawsql">Logical raw SQL sentence to use (including select, where, order by and subqueries)</param>
		/// <param name="table">Table to populate in DataSet</param>
		/// <param name="values">Values of the parameters used in the SQL sentence (null if no parameters)</param>
		/// <returns>A new DataTable with the results</returns>
		public DataTable FillDataTable (string rawsql, string table, params object[] values)
		{
			IDbCommand cmd = PrepareCommand (rawsql, values);
			cmd.Connection = _dataprovider.GetNewConnection();
			DataTable dt = _dataprovider.FillDataTable (cmd);
			dt.TableName = table;
			return dt;
		}


		/// <summary>
		/// Returns a dataset with the information requested
		/// </summary>
		/// <param name="select">Logical SQL statement used to fill the DataSet</param>
		/// <param name="from">Name of the table to select (FROM value)</param>
		/// <param name="orderby">Logical SQL order by clausule</param>
		/// <param name="where">Logical SQL where clausule</param>
		/// <param name="table">Table of the DataSet to fill</param>
		/// <param name="rowcount">Number of rows to fetch (use -1 to specify no limit)</param>
		/// <returns>DataSet with the Requested Info</returns>
		public DataSet FillDataSet  (string select, string from, string orderby, string where,string table, int rowcount)
		{
			return FillDataSet (select, from, orderby, where, table, rowcount, null);
		}

		/// <summary>
		/// Returns a DataTable with the information requested
		/// </summary>
		/// <param name="select">Logical SQL statement used to fill the DataTable</param>
		/// <param name="from">Name of the table to select (FROM value)</param>
		/// <param name="orderby">Logical SQL order by clausule</param>
		/// <param name="where">Logical SQL where clausule</param>
		/// <param name="table">Name of the DataTable to fill</param>
		/// <param name="rowcount">Number of rows to fetch (use -1 to specify no limit)</param>
		/// <returns>DataTable with the Requested Info</returns>
		public DataTable FillDataTable  (string select, string from,string orderby, string where,string table, int rowcount)
		{
			return FillDataTable (select, from, orderby, where, table, rowcount, null);
		}

		/// <summary>
		/// Returns a DataTable with the information requested
		/// </summary>
		/// <param name="select">Logical SQL statement used to fill de DS</param>
		/// <param name="from">Name of the table to select (FROM value)</param>		
		/// <param name="orderby">Logical SQL order by clausule</param>
		/// <param name="where">Logical SQL where clausule</param>
		/// <param name="name">Name of the DataTable to fill</param>
		/// <param name="rowcount">Number of rows to fetch (use -1 to specify no limit)</param>
		/// <param name="values">Values of all parameters of the SQL statement</param>
		/// <returns>DataSet with the Requested Info</returns>
		public DataTable FillDataTable  (string select, string from, string orderby, string where,string name, 
			int rowcount, params object [] values)
		{
			// Builds the query and runs it
			string ssql = _dataprovider.BuildFullSQLQuery (select, from, orderby, where, null, rowcount);
			IDbCommand cmd = PrepareCommand(ssql,values);
			// Creates an empty DataSet and fills it
			cmd.Connection = _dataprovider.GetNewConnection();
			DataTable dt = _dataprovider.FillDataTable(cmd);
			dt.TableName = name;
			return dt;
		}

		/// <summary>
		/// Returns a DataTable with the information requested
		/// </summary>
		/// <param name="select">Logical SQL statement used to fill de DS</param>
		/// <param name="from">Name of the table to select (FROM value)</param>		
		/// <param name="orderby">Logical SQL order by clausule</param>
		/// <param name="where">Logical SQL where clausule</param>
		/// <param name="groupby">Logical SQL group by clausule</param>
		/// <param name="name">Name of the DataTable to fill</param>
		/// <param name="rowcount">Number of rows to fetch (use -1 to specify no limit)</param>
		/// <param name="values">Values of all parameters of the SQL statement</param>
		/// <returns>DataSet with the Requested Info</returns>
		public DataTable FillDataTable  (string select, string from, string orderby, string where, string groupby, string name, 
			int rowcount, params object [] values)
		{
			// Builds the query and runs it
			string ssql = _dataprovider.BuildFullSQLQuery (select, from, orderby, where, groupby, rowcount);
			IDbCommand cmd = PrepareCommand(ssql,values);
			// Creates an empty DataSet and fills it
			cmd.Connection = _dataprovider.GetNewConnection();
			DataTable dt = _dataprovider.FillDataTable(cmd);
			dt.TableName = name;
			return dt;
		}

		/// <summary>
		/// Returns a DataSet with the information requested
		/// </summary>
		/// <param name="select">Logical SQL statement used to fill de DS</param>
		/// <param name="from">Name of the table to select (FROM value)</param>
		/// <param name="orderby">Logical SQL order by clausule</param>
		/// <param name="where">Logical SQL where clausule</param>
		/// <param name="table">Name of the table of the DataSet</param>
		/// <param name="rowcount">Number of rows to fetch (use -1 to specify no limit)</param>
		/// <param name="values">Values of all parameters of the SQL statement</param>
		/// <returns>DataSet with the Requested Info</returns>
		public DataSet FillDataSet  (string select, string from, string orderby, string where,string table, 
			int rowcount, params object [] values)
		{
			DataTable dt = FillDataTable (select,from,orderby,where,table,rowcount,values);
			DataSet ds = new DataSet();
			ds.Tables.Add (dt);
			return ds;
		}

		/// <summary>
		/// Returns a DataSet with the information requested
		/// </summary>
		/// <param name="select">Logical SQL statement used to fill de DS</param>
		/// <param name="from">Name of the table to select (FROM value)</param>
		/// <param name="orderby">Logical SQL order by clausule</param>
		/// <param name="where">Logical SQL where clausule</param>
		/// <param name="groupby">Logical SQL group by clausule</param>
		/// <param name="table">Name of the table of the DataSet</param>
		/// <param name="rowcount">Number of rows to fetch (use -1 to specify no limit)</param>
		/// <param name="values">Values of all parameters of the SQL statement</param>
		/// <returns>DataSet with the Requested Info</returns>
		public DataSet FillDataSet  (string select, string from, string orderby, string where, string groupby, string table, 
			int rowcount, params object [] values)
		{
			// Gets a DataAdapter with the select

			DataTable dt = FillDataTable (select,from,orderby,where, groupby, table,rowcount,values);
			DataSet ds = new DataSet();
			ds.Tables.Add (dt);
			return ds;
		}


		public void FillDataSet  (ref DataSet dataSet, string select, string from, string orderby, string where,string table, 
			int rowcount, params object [] values)
		{
			DataTable dt = FillDataTable (select,from,orderby,where,table,rowcount,values);
			dataSet.Tables.Add (dt);
		}

		public void FillDataSet  (ref DataSet dataSet, string select, string from, string orderby, string where, string groupby, string table, 
			int rowcount, params object [] values)
		{
			// Gets a DataAdapter with the select

			DataTable dt = FillDataTable (select,from,orderby,where, groupby, table,rowcount,values);
			dataSet.Tables.Add (dt);
		}



		/// <summary>
		/// Returns a DataSet with the information requested.
		/// The diference between this method and FillDataSet is that method returns
		/// a virtual page (starting at rowstart and ending at rowend).
		/// </summary>
		/// <param name="select">Logical SELECT statement</param>
		/// <param name="from">Name of the table to select (FROM value)</param>
		/// <param name="orderby">Logical ORDER BY clausule</param>
		/// <param name="where">Logical WHERE filter</param>
		/// <param name="table">Table of the DataSet to be created and populated</param>
		/// <param name="rowstart">First register to be retrieved</param>
		/// <param name="rowend">Last register to be retrieved</param>
		/// <param name="pk">Name of the fields which are the Primary Key</param>
		/// <returns>A DataSet with one DataTable named 'table' with the registers requiered</returns>
		public DataSet FillPagedDataSet (string select, string from, string orderby, string where, string table, int rowstart, int rowend, string[] pk)
		{
			return FillPagedDataSet (select, from, orderby, where, table, rowstart, rowend, pk, null);
		}

		/// <summary>
		/// Returns a DataSet with the information requested.
		/// The diference between this method and FillDataSet is that method returns
		/// a virtual page (starting at rowstart and ending at rowend).
		/// </summary>
		/// <param name="select">Logical SELECT statement</param>
		/// <param name="from">Name of the table to select (FROM value)</param>
		/// <param name="orderby">Logical ORDER BY clausule</param>
		/// <param name="where">Logical WHERE filter</param>
		/// <param name="table">Table of the DataSet to be created and populated</param>
		/// <param name="rowstart">First register to be retrieved</param>
		/// <param name="rowend">Last register to be retrieved</param>
		/// <param name="pk">Name of the field which are the Primary Key</param>
		/// <param name="values">Values of the parameters</param>
		/// <returns>A DataSet with one DataTable named 'table' with the registers requiered</returns>
		public DataSet FillPagedDataSet (string select, string from, string orderby, string where, string table, int rowstart, int rowend, 
			string[] pk, params object[] values)
		{
			DataTable dt = FillPagedDataTable (select, from, orderby, where, table, rowstart, rowend, pk,values);
			DataSet ds = new DataSet();
			ds.Tables.Add (dt);
			return ds;
		}

		/// <summary>
		/// Returns a DataTable with the information requested.
		/// The diference between this method and FillDataSet is that method returns
		/// a virtual page (starting at rowstart and ending at rowend).
		/// </summary>
		/// <param name="select">Logical SELECT statement</param>
		/// <param name="from">Name of the table to select (FROM value)</param>
		/// <param name="orderby">Logical ORDER BY clausule</param>
		/// <param name="where">Logical WHERE filter</param>
		/// <param name="table">Table of the DataSet to be created and populated</param>
		/// <param name="rowstart">First register to be retrieved</param>
		/// <param name="rowend">Last register to be retrieved</param>
		/// <param name="pk">Name of the field which are the Primary Key</param>
		/// <returns>A DataTable named 'table' with the registers requiered</returns>
		public DataTable FillPagedDataTable (string select, string from, string orderby, string where, string table, int rowstart, int rowend, string[] pk)
		{
			return FillPagedDataTable (select, from, orderby, where, table, rowstart, rowend, pk,null);
		}

		/// <summary>
		/// Returns a DataTable with the information requested.
		/// The diference between this method and FillDataSet is that method returns
		/// a virtual page (starting at rowstart and ending at rowend).
		/// </summary>
		/// <param name="select">Logical SELECT statement</param>
		/// <param name="from">Name of the table to select (FROM value)</param>		
		/// <param name="orderby">Logical ORDER BY clausule</param>
		/// <param name="where">Logical WHERE filter</param>
		/// <param name="table">Table of the DataSet to be created and populated</param>
		/// <param name="rowstart">First register to be retrieved. Note: If rowstart is 0</param>
		/// <param name="rowend">Last register to be retrieved</param>
		/// <param name="pk">Name of the field which are the Primary Key</param>
		/// <param name="values">Values of the parameters</param>
		/// <returns>A DataTable named 'table' with the registers requiered</returns>
		
		// NOTE: That is the one and only real-working method. The rest of FillPagedDataXXX are
		// just overloads....
		public DataTable FillPagedDataTable (string select, string from, string orderby, string where, string table, int rowstart, 
			int rowend, string[] pk, params object[] values)
		{
			// Build the query... but make a simple comparison here...
			string ssql = null;
			if (rowstart == 0) 
			{
				ssql =  _dataprovider.BuildFullSQLQuery (select, from, orderby, where, null, rowend);				
			}
			else
			{
				ssql= _dataprovider.BuildFullPagedSQLQuery (select, from, orderby, where, null, rowstart, rowend, pk);
			}
			// ... and runs it
			//			DataTable dt = _dataprovider.FillDataTable(cmd);

			IDbCommand cmd = PrepareCommand(ssql,values);
			cmd.Connection = _dataprovider.GetNewConnection();

			DataTable dt = _dataprovider.FillDataTable(cmd);
				dt.TableName = table;
				if (pk != null && pk.Length > 0) 
				{
					DataColumn[] pkc = new DataColumn[pk.Length];
					for (int ipk = 0; ipk < pk.Length; ipk++)
					{
						pkc[ipk] = dt.Columns[pk[ipk]];
					}
					dt.PrimaryKey = pkc;
				}
				return dt;

			
			


/*
			{
				cmd.Connection.Open();
				IDataReader myReader = cmd.ExecuteReader();

				DataTable schemaTable = myReader.GetSchemaTable();


				DataTable dt2 = new DataTable(table);

				foreach (DataRow myCol in schemaTable.Rows)
				{
					dt2.Columns.Add(myCol.ItemArray[0].ToString(),(System.Type)myCol.ItemArray[5]);
				}

				int aux = 0;

				while (myReader.Read() &&  aux < rowend)
				{
					aux ++;
					if (aux >= rowstart && aux < rowend)
					{
						DataRow dtr = dt2.NewRow();
					
						int myCounter = 0;
					
						for (int aux2 = 0; aux2 < schemaTable.Rows.Count; aux2++)
						{
							dtr[aux2] = myReader[myCounter];
							myCounter++;
						}
						dt2.Rows.Add(dtr);
					}

				}

				myReader.Close();
				cmd.Connection.Close();

				if (pk != null && pk.Length > 0) 
				{
					DataColumn[] pkc = new DataColumn[pk.Length];
					for (int ipk = 0; ipk < pk.Length; ipk++)
					{
						pkc[ipk] = dt2.Columns[pk[ipk]];
					}
					dt2.PrimaryKey = pkc;
				}			

				return dt2;
			}
			
*/
		}

		/// <summary>
		/// Updates a DataSet
		/// </summary>
		/// <param name="da">DataAdapter to use</param>
		/// <param name="ds">DataSet to update</param>
		/// <param name="table">Name of the DataTable to update</param>
		public void UpdateDataSet (IDbDataAdapter da, DataSet ds, string table) 
		{
			_dataprovider.Update (da,ds,table);

		}
		/// <summary>
		/// Updates a DataTable
		/// </summary>
		/// <param name="da">DataAdapter to use</param>
		/// <param name="table">DataTable to update</param>
		public void UpdateDataSet (IDbDataAdapter da, DataTable table) 
		{
			_dataprovider.Update(da, table);
		}

		~Database() 
		{
			_dataprovider = null;

		}
	}
}

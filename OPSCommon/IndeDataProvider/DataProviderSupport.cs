using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

using OPS.Comm;
using Oracle.ManagedDataAccess.Client;
//using Oracle.DataAccess.Client;


namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for DataProviderSupport.
	/// </summary>
	public interface IDataProviderSupport
	{
		/// <summary>
		/// Retrieves a connection with the Data Source
		/// </summary>
		IDbConnection GetNewConnection();

		/// <summary>
		/// Retrieves a new empty command to be used
		/// </summary>
		/// <returns>The Command object</returns>
		IDbCommand GetNewCommand();

		/// <summary>
		/// Retrives a DataAdpater object
		/// </summary>
		/// <returns>The DataAdapter object</returns>
		IDbDataAdapter GetNewDataAdapter();

		/// <summary>
		/// Translate a logical SQL statement to a physical SQL statement
		/// </summary>
		/// <param name="ssql">SQL statement to be translated</param>
		/// <param name="schema">Schema of the database (hashtable)</param>
		/// <param name="pars">Gets a list with the IdbDataParameter-derived objects</param>
		/// <returns>Translated SQL statement</returns>
		string TranslateSQL(string ssql, Hashtable schema, out ArrayList pars);
		/// <summary>
		/// Returns a DataTable containing the info and optionally the full schema retrieved with the command speciefied
		/// </summary>
		/// <param name="cmd">Command with the SQL query used to fill the DataSet</param>
		/// <returns>The DataSet object with data</returns>
		DataTable FillDataTable(IDbCommand cmd);

		/// <summary>
		/// Updates a table of a DataSet with a specified DataAdapter
		/// </summary>
		/// <param name="da">DataAdapter to use</param>
		/// <param name="ds">DataSet containing data</param>
		/// <param name="table">Name of the table to update</param>
		void Update (IDbDataAdapter da, DataSet ds, string table);
		/// <summary>
		/// Updates a DataTable using the DataAdapter specified
		/// </summary>
		/// <param name="da">DataAdapter to use</param>
		/// <param name="table">DataTable to Update</param>
		void Update (IDbDataAdapter da, DataTable table);

		/// <summary>
		/// Builds a full SQL Query (select + from + where + groupby + orderby).
		/// The SQL query fetchs a rowcount registers and has the order and sintaxys of correct database in each case 
		/// </summary>
		/// <param name="select">Select part of SQL query</param>
		/// <param name="from">Name of the table to select</param>
		/// <param name="orderby">Order by clausule</param>
		/// <param name="where">Where clausule</param>
		/// <param name="groupby">Group By clausule</param>
		/// <param name="rowcount">Number of rows to fetch</param>
		/// <returns>The complete SQL Query (select + where + order by) fetching only rowcount registers</returns>
		string BuildFullSQLQuery (string select, string from, string orderby, string where, string groupby, int rowcount);

		/// <summary>
		/// Builds a full SQL Query (select + from + where + groupby + orderby)
		/// The SQL query fetchs (rowend-rowstart) registers starting at register rowstart.
		/// That method helps the cobstruction of virtual filters
		/// </summary>
		/// <param name="select">Select part of SQL query</param>
		/// <param name="from">Name of the table to select</param>
		/// <param name="orderby">Order by clausule</param>
		/// <param name="where">Where clausule</param>
		/// <param name="groupby">Group By clausule</param>
		/// <param name="rowstart">First register to fetch (after order by and where applied)</param>
		/// <param name="rowend">Last register to fetch</param>
		/// <param name="pk">Name of the field which is the primary key</param>
		/// <returns>The complete SQL Query (select + where + order by) fetching the requiered registers</returns>
		string BuildFullPagedSQLQuery (string select, string from, string orderby, string where, string groupby, int rowstart, int rowend, string[] pk);

		/// <summary>
		/// Creates a derived IDbDataParameter for the table and fields specified
		/// </summary>
		/// <param name="table">Name of the table of the field asscoiated to the param</param>
		/// <param name="field">Name of the field associated to the parameter</param>
		/// <param name="nparam">Ordinal position of the parameter (0 is first)</param>
		/// <param name="tableInfo">Hashtable with the schema of the table</param>
		/// <param name="stringRep">Representation of the parameter in the SQL String (i.e. question-mark in OLEDB)</param>
		/// <param name="bOriginalVersion">If true the value of the parameter is bound to the original version of the value in the datarow</param>		
		/// <returns>The newly created parameter object</returns>
		IDataParameter CreateParameter (string table, string field, int nparam, Hashtable tableInfo,  out string stringRep, bool bOriginalVersion);
	}

	/// <summary>
	/// Class with methods to help IDataProviderSupport-implementing objects to do his work
	/// </summary>
	public class DataProviderSupportHelper
	{
		protected IDataProviderSupport _dp;
		protected int _paramCount;
		protected Hashtable _schema;
		protected ArrayList _params;
		public DataProviderSupportHelper (IDataProviderSupport dp)
		{
			_dp = dp;
			_paramCount = 0;
			_params = new ArrayList();
		}

		private string ParameterMatchChanger(Match m)
		{
			int idxseparator = m.Value.IndexOf ('.');
			string tname = null;
			string fname = null;
			string ret;
			char init = m.Value[0];		// init is '#' (originalVersion parameter) or @ (proposedVersion parameter)
			if (idxseparator == -1) 
			{
				// Table MUST appear
				throw new ArgumentException ("Parameter must be in format @table.param@ or in format #table.param#, but table has not found in parameter: " + m.Value);
			}
			else 
			{
				tname = m.Value.Substring (1, idxseparator - 1);
				fname = m.Value.Substring (idxseparator + 1, m.Value.Length - (idxseparator + 2));
			}
			// Creates the parameter
			_params.Add( _dp.CreateParameter(tname,fname, _paramCount,(Hashtable)_schema[tname],out ret, init=='#'));
			_paramCount++;
			return ret;
		}

		public string TranslateSQL(string ssql, Hashtable schema)
		{
			string buf = null;
			_schema = schema;
			ILogger logger = null;
			if(logger != null)
				logger.AddLog("[TranslateSQL]" + schema.ToString(), LoggerSeverities.Debug);
			// Create the regex objects we need to search for @table.fieldname@
			Regex rpars = new Regex(@"@[0-9a-zA-Z_\.]+@");
			Regex rparsOrig = new Regex (@"#[0-9a-zA-Z_\.]+#");
			// Replace all ocurrences of @table.fieldname@ by a SQL-parameter string.
			// The SQL-paramter sting depends on real DataProvider and is provided by it
			buf = rpars.Replace (ssql, new MatchEvaluator (this.ParameterMatchChanger));
			buf = rparsOrig.Replace (buf, new MatchEvaluator (this.ParameterMatchChanger));
			return buf;
		}
		/// <summary>
		/// Gets the list of all parameters created
		/// </summary>
		public ArrayList CreatedParameters
		{
			get {return _params;}
		}
	}
}

namespace OPS.Components.Data.OleDb 
{
	/// <summary>
	/// A IDataProviderSupport class for OleDb Data Provider
	/// </summary>
	public sealed class OleDbDataProviderSupport : IDataProviderSupport
	{
		private string _constr;
		
		public OleDbDataProviderSupport (string constring)  
		{
			_constr = constring;
		}
		//****** Implementation of IDataProviderSupport *******
		public IDbConnection GetNewConnection()
		{
			return new System.Data.OleDb.OleDbConnection(_constr);
		}

		public IDbCommand GetNewCommand() 
		{
			System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand();
			return cmd;
		}

		public IDbDataAdapter GetNewDataAdapter() 
		{
			return new System.Data.OleDb.OleDbDataAdapter();
		}

		public string TranslateSQL(string ssql, Hashtable schema, out ArrayList pars) 
		{
			DataProviderSupportHelper dpsh = new DataProviderSupportHelper (this);
			string ret =  dpsh.TranslateSQL(ssql, schema);
			pars = dpsh.CreatedParameters;
			return ret;
		}

		public DataTable FillDataTable(IDbCommand cmd)
		{
			System.Data.OleDb.OleDbDataAdapter da = new System.Data.OleDb.OleDbDataAdapter ();
			da.SelectCommand = (System.Data.OleDb.OleDbCommand)cmd;
			DataTable dt = new DataTable();
			da.Fill(dt);
			return dt;
		}

		public void Update (IDbDataAdapter da, DataSet ds, string table)
		{
			System.Data.OleDb.OleDbDataAdapter dda = da as System.Data.OleDb.OleDbDataAdapter;
			if (dda == null) throw new ArgumentException ("DataAdapter MUST be an OleDbDataAdapter","da");
			dda.Update (ds, table);
		}
		public void Update (IDbDataAdapter da, DataTable table) 
		{
			System.Data.OleDb.OleDbDataAdapter dda = da as System.Data.OleDb.OleDbDataAdapter;
			if (dda == null) throw new ArgumentException ("DataAdapter MUST be an OleDbDataAdapter","da");
			dda.Update (table);
		}

		/// <summary>
		/// Builds a full SQL Query (select + from + where + groupby + orderby).
		/// The SQL query fetchs a rowcount registers and has the order and sintaxys of correct database in each case 
		/// In OLEDB we have a very important problem: We don't know the real datasource (can be sybase, access, SQL Server, Oracle,...)
		/// That implies that we CANNOT build a query fetching only rowcount registers... So in that case the fetch must be included
		/// in the select or where statement (p.ex. in select statement in SQL Server and in where clausule in Oracle).
		/// </summary>
		/// <param name="select">Select part of SQL query</param>
		/// <param name="from">Name of the table to select</param>
		/// <param name="orderby">Order by clausule</param>
		/// <param name="where">Where clausule</param>
		/// <param name="groupby">Group By clausule</param>
		/// <param name="rowcount">Number of rows to fetch (NOT USED in OleDB)</param>
		/// <returns>The complete logical SQL Query (select + where + order by)</returns>
		public string BuildFullSQLQuery (string select, string from, string orderby, string where, string groupby, int rowcount)
		{
			StringBuilder ssql = new StringBuilder();
			ssql.Append ("SELECT ");
			ssql.Append (select);
			ssql.Append (" FROM ");
			ssql.Append (from);
			if (where!=null) 
			{
				ssql.Append (" WHERE ");
				ssql.Append (where);
			}
			if (groupby!=null) 
			{
				ssql.Append (" GROUP BY ");
				ssql.Append (groupby);
			}
			if (orderby!=null) 
			{
				ssql.Append (" ORDER BY ");
				ssql.Append (orderby);
			}
			return ssql.ToString();
		}

		/// <summary>
		/// Builds a full SQL Query (select + from + where + groupby + orderby)
		/// The SQL query fetchs (rowend-rowstart) registers starting at register rowstart.
		/// In OLEDB we have a very important issue: don't know the real datasource, so specific SQL query
		/// cannot be built.
		/// Calling this Method in OleDbDataProviderSupport is identical to call BuildFullSQLQuery of
		/// OleDbDataProviderSupport
		/// </summary>
		/// <param name="select">Select part of SQL query</param>
		/// <param name="from">Name of the table to select</param></param>
		/// <param name="orderby">Order by clausule</param>
		/// <param name="where">Where clausule</param>
		/// <param name="groupby">Group By clausule</param>
		/// <param name="rowstart">First register to fetch. NOT USED in OleDB</param>
		/// <param name="rowend">Last register to fetch. NOT USED in OleDB</param>
		/// <param name="pk">Name of the field which is the primary-key. NOT USED in OleDB</param>
		/// <returns>The complete SQL Query (select + where + order by)</returns>
		public string BuildFullPagedSQLQuery (string select, string from, string orderby, string where, string groupby, int rowstart, int rowend, string[] pk)
		{
			return BuildFullSQLQuery (select, from, orderby, where, groupby, -1);
		}

		/// <summary>
		/// Creates a derived IDbDataParameter for the table and fields specified
		/// </summary>
		/// <param name="table">Name of the table of the field asscoiated to the param</param>
		/// <param name="field">Name of the field associated to the parameter</param>
		/// <param name="nparam">Ordinal position of the parameter (0 is first)</param>
		/// <param name="tableInfo">Hashtable with the schema of the table. NOT used in OLEDB (parameters cannot be filled)</param>
		/// <param name="stringRep">Representation of the parameter in the SQL String (i.e. question-mark in OLEDB)</param>
		/// <param name="bOriginalVersion">If true the value of the parameter is bound to the original version of the value in the datarow</param>
		/// <returns>The newly created parameter object</returns>
		public IDataParameter CreateParameter (string table, string field, int nparam, Hashtable tableInfo,  out string stringRep, bool bOriginalVersion)
		{
			stringRep = "?";
			// Creates a EMPTY parameter (OLEDB params cannot be filled).
			System.Data.OleDb.OleDbParameter param = new System.Data.OleDb.OleDbParameter ();
			param.ParameterName = "param" + nparam;
			param.SourceColumn =  field;
			if (bOriginalVersion) param.SourceVersion = DataRowVersion.Original;
			return param;
		}
	}
}

namespace OPS.Components.Data.SqlClient 
{
	/// <summary>
	/// A IDataProviderSupport class for SQL Server 
	/// </summary>
	public sealed class SqlDataProviderSupport :  IDataProviderSupport 
	{
		private string _constr;
						
		public SqlDataProviderSupport (string constring)  
		{
			_constr = constring;
		}
		//****** Implementation of IDataProviderSupport *******
		public IDbConnection GetNewConnection()
		{
			return new System.Data.SqlClient.SqlConnection(_constr);
		}

		public IDbCommand GetNewCommand() 
		{
			System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
			return cmd;			
		}

		public IDbDataAdapter GetNewDataAdapter() 
		{
			return new System.Data.SqlClient.SqlDataAdapter();
		}

		public string TranslateSQL(string ssql, Hashtable schema, out ArrayList pars) 
		{
			DataProviderSupportHelper dpsh = new DataProviderSupportHelper (this);
			string ret =  dpsh.TranslateSQL(ssql, schema);
			pars = dpsh.CreatedParameters;
			return ret;
		}

		public DataTable FillDataTable(IDbCommand cmd)
		{
			System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter ();
			da.SelectCommand = (System.Data.SqlClient.SqlCommand)cmd;
			DataTable dt = new DataTable();
			da.Fill(dt);
			return dt;
		}

		public void Update (IDbDataAdapter da, DataSet ds, string table)
		{
			System.Data.SqlClient.SqlDataAdapter dda = da as System.Data.SqlClient.SqlDataAdapter;
			if (dda == null) throw new ArgumentException ("DataAdapter MUST be an SqlDataAdapter","da");
			dda.Update (ds, table);
		}
		public void Update (IDbDataAdapter da, DataTable table) 
		{
			System.Data.SqlClient.SqlDataAdapter dda = da as System.Data.SqlClient.SqlDataAdapter;
			if (dda == null) throw new ArgumentException ("DataAdapter MUST be an SqlDataAdapter","da");
			dda.Update (table);
		}

		/// <summary>
		/// Builds a full SQL Query (select + from + where + groupby + orderby).
		/// The SQL query fetchs a rowcount registers and has the order and sintaxys of correct database in each case 
		/// The TOP clausule in SELECT is used for SQL Server
		/// </summary>
		/// <param name="select">Select part of SQL query</param>
		/// <param name="from">Name of the table to select</param>
		/// <param name="orderby">Order by clausule</param>
		/// <param name="where">Where clausule</param>
		/// <param name="groupby">Group By clausule</param>
		/// <param name="rowcount">Number of rows to fetch</param>
		/// <returns>The complete logical SQL Query (select + where + order by) fetching only rowcount rows</returns>
		public string BuildFullSQLQuery (string select, string from, string orderby, string where, string groupby, int rowcount)
		{
			StringBuilder ssql = new StringBuilder();
			if (where == "") where = null;
			ssql.Append ("SELECT ");
			if (rowcount != -1) 
			{
				ssql.Append ("TOP ");
				ssql.Append (rowcount.ToString());
			}
			ssql.Append (select);
			ssql.Append (" FROM ");
			ssql.Append (from);
			if (where!=null) 
			{
				ssql.Append (" WHERE (");				
				ssql.Append (where);
				ssql.Append (')');

			}
			if (groupby!=null) 
			{
				ssql.Append (" GROUP BY ");
				ssql.Append (groupby);
			}
			if (orderby!=null) 
			{
				ssql.Append (" ORDER BY ");
				ssql.Append (orderby);
			}
			return ssql.ToString();
		}

		/// <summary>
		/// Builds a full SQL Query (select + where + orderby)
		/// The SQL query fetchs (rowend-rowstart) registers starting at register rowstart.
		/// That method helps the cobstruction of virtual filters
		/// In SQL-Server fetching a virtual page is acomplished by using a subquery, i.e:
		///		select top N * from  table where xxx not in (select top N*M * from table order by xxx asc) order by xxx asc
		// </summary>
		/// <param name="select">Select part of SQL query</param>
		/// <param name="from">Name of the table to select</param></param>
		/// <param name="orderby">Order by clausule</param>
		/// <param name="where">Where clausule</param>
		/// <param name="groupby">Group By clausule</param>
		/// <param name="rowstart">First register to fetch (after order by and where applied). 1 based!</param>
		/// <param name="rowend">Last register to fetch</param>
		/// <param name="pk">Name of the field which is the primary key</param>
		/// <returns>The complete SQL Query (select + where + order by) fetching the requiered registers</returns>
		public string BuildFullPagedSQLQuery (string select,string from, string orderby, string where, string groupby, int rowstart, int rowend, string[] pk)
		{
			StringBuilder ssql = new StringBuilder();

			if (orderby == null)
			{
				// for (int contador = 0;contador < pk.Length;contador ++)  { orderby = orderby + pk[contador] + " ASC"; }
				orderby = pk + " ASC";
			}
				
			if (where == "") where = null;
			ssql.Append ("SELECT TOP ");
			ssql.Append ((rowend - rowstart + 1).ToString());
			ssql.Append (' ');
			ssql.Append (select);
			ssql.Append (" FROM ");
			ssql.Append (from);
			// At this point we have: select top <pagesize> <fields> from <table>
			ssql.Append  (' ');
			ssql.Append ("WHERE ");
			if (where != null) 
			{
				ssql.Append ('(');
				ssql.Append (where);
				ssql.Append (") AND ");
			}
			ssql.Append  (pk);
			ssql.Append (" NOT IN (");
			// At this point we have: select top <pagesize> <fields> from <table> where (<where>) and <pk> not in (
			ssql.Append ("SELECT TOP ");
			ssql.Append ((rowstart - 1).ToString());
			ssql.Append (select);
			if (where != null) 
			{
				ssql.Append (" WHERE ");
				ssql.Append (where);
			}
			if (groupby != null) 
			{
				ssql.Append (" GROUP BY ");
				ssql.Append (groupby);
			}
			ssql.Append (" ORDER BY ");		
			ssql.Append (orderby);
			ssql.Append (')');
			// At this point we have:
			//	select top <pagesize> <fields> from <table> where (<where>) and <pk> not in 
			//		(select top <rowstart-1> <fields> from <table> group by <groupby> order by <order>)
			if (groupby != null) 
			{
				ssql.Append (" GROUP BY ");
				ssql.Append (groupby);
			}
			ssql.Append (" ORDER BY ");
			ssql.Append (orderby);
			
			// At this point we have:
			//	select top <pagesize> <fields> from <table> where (<where>) and <pk> not in 
			//		(select top <rowstart-1> <fields> from <table> group by <groupby> order by <order>) group by <groupby> order by <order>
			return ssql.ToString();
		}

		// That function MUST return the string representation of a parameter. Also, it creates a SqlParameter object and stores it
		/// <summary>
		/// Creates a derived IDbDataParameter for the table and fields specified
		/// </summary>
		/// <param name="table">Name of the table of the field asscoiated to the param</param>
		/// <param name="field">Name of the field associated to the parameter</param>
		/// <param name="nparam">Ordinal position of the parameter (0 is first)</param>
		/// <param name="tableInfo">Hashtable with the schema of the table</param>
		/// <param name="stringRep">Representation of the parameter in the SQL String (@param)</param>
		/// <param name="bOriginalVersion">If true the value of the parameter is bound to the original version of the value in the datarow</param>
		/// <returns>The newly created parameter object</returns>
		public IDataParameter CreateParameter (string table, string field, int nparam, Hashtable tableInfo,  out string stringRep, bool bOriginalVersion)
		{
			stringRep = "@param" + nparam;
			FieldSchemaInfo fieldInfo = (FieldSchemaInfo)tableInfo[field];
			// Creates a EMPTY parameter (OLEDB params cannot be filled).
			System.Data.SqlClient.SqlParameter param = new System.Data.SqlClient.SqlParameter();
			param.ParameterName = "param" + nparam;
			param.SourceColumn =  field;
			param.SqlDbType = ( System.Data.SqlDbType)fieldInfo.NativeType;
			if (fieldInfo.FieldSize!=-1) param.Size = fieldInfo.FieldSize;
			if (bOriginalVersion) param.SourceVersion = DataRowVersion.Original;
			return param;
		}
	}
}

namespace OPS.Components.Data.Oracle 
{
	public sealed class OracleDataProviderSupport : IDataProviderSupport
	{
		private ArrayList _pars;
		private string _constr;
				
		public OracleDataProviderSupport (string constring)  
		{
			_constr = constring;
			_pars = new ArrayList();
		}

		//****** Implementation of IDataProviderSupport *******
		public IDbConnection GetNewConnection()
		{
			return new OracleConnection(_constr);
		}
		public IDbCommand GetNewCommand() 
		{
			OracleCommand cmd = new OracleCommand();
			return cmd;
		}

		public IDbDataAdapter GetNewDataAdapter() 
		{
			return new OracleDataAdapter();
		}

		public string TranslateSQL(string ssql, Hashtable schema, out ArrayList pars) 
		{
			DataProviderSupportHelper dpsh = new DataProviderSupportHelper (this);
			string ret =  dpsh.TranslateSQL(ssql, schema);
			pars = dpsh.CreatedParameters;
			return ret;
		}
		
		public DataTable FillDataTable(IDbCommand cmd)
		{
			OracleDataAdapter da = new OracleDataAdapter ();
			da.SelectCommand = (OracleCommand)cmd;
			DataTable dt = new DataTable();
			da.Fill(dt);
			return dt;
		}

		public void Update (IDbDataAdapter da, DataSet ds, string table)
		{
			OracleDataAdapter dda = da as OracleDataAdapter;
			if (dda == null) throw new ArgumentException ("DataAdapter MUST be an OracleDataAdapter","da");
			dda.Update (ds, table);
		}
		public void Update (IDbDataAdapter da, DataTable table) 
		{
			OracleDataAdapter dda = da as OracleDataAdapter;
			if (dda == null) throw new ArgumentException ("DataAdapter MUST be an OracleDataAdapter","da");
			dda.Update (table);
		}

		/// <summary>
		/// Builds a full SQL Query (select + from + where + groupby + orderby).
		/// The SQL query fetchs a rowcount registers and has the order and sintaxys of correct database in each case 
		/// The rownum condition in where clausule is used for Oracle
		/// </summary>
		/// <param name="select">Select part of SQL query</param>
		/// <param name="from">Name of the table to select</param>
		/// <param name="orderby">Order by clausule</param>
		/// <param name="where">Where clausule</param>
		/// <param name="groupby">Group By clausule</param>
		/// <param name="rowcount">Number of rows to fetch</param>
		/// <returns>The complete logical SQL Query (select + where + order by) fetching only rowcount rows</returns>
		public string BuildFullSQLQuery (string select, string from, string orderby, string where, string groupby, int rowcount)
		{
			StringBuilder ssql = new StringBuilder();
			if (where == "") where = null;
			ssql.Append ("SELECT ");
			ssql.Append (select);
			ssql.Append (" FROM ");
			ssql.Append (from);
			if (where!=null) 
			{
				ssql.Append (" WHERE (");
				ssql.Append (where);
				ssql.Append (')');
				if (rowcount!=-1) 
				{
					ssql.Append (" AND rownum<=");
					ssql.Append (rowcount.ToString());
					ssql.Append (' ');
				}
			}
			else 
			{
				if (rowcount!=-1) 
				{
					ssql.Append (" WHERE rownum<=");
					ssql.Append (rowcount.ToString());
					ssql.Append (' ');
				}
			}

			if (groupby!=null) 
			{
				ssql.Append (" GROUP BY ");
				ssql.Append (groupby);
			}

			if (orderby!=null) 
			{
				ssql.Append (" ORDER BY ");
				ssql.Append (orderby);
			}
			return ssql.ToString();
		}


		/// <summary>
		/// Builds a full SQL Query (select + from + where + groupby + orderby)
		/// The SQL query fetchs (rowend-rowstart) registers starting at register rowstart.
		/// That method helps the cobstruction of virtual filters
		/// In Oracle fetching a virtual page is acomplished by using RANK() and a subquery
		// </summary>
		/// <param name="select">Fields to select (comma-separated)</param>
		/// <param name="from">Name of the table to select</param></param>
		/// <param name="orderby">Order by clausule</param>
		/// <param name="where">Where clausule</param>
		/// <param name="groupby">***NOT IMPLEMENTED***</param>
		/// <param name="rowstart">First register to fetch (after order by and where applied). 1 based</param>
		/// <param name="rowend">Last register to fetch</param>
		/// <param name="pk">Name of the fields which is the primary key</param>
		/// <returns>The complete SQL Query (select + where + order by) fetching the requiered registers</returns>
		public string BuildFullPagedSQLQuery (string select, string from, string orderby, string where, string groupby, int rowstart, int rowend, string[] pk)
		{
			StringBuilder ssql = new StringBuilder();
			
			// Use the built-in filed to order in case no selection found.
			if (orderby == null)
			{
				orderby = pk[0] + " ASC";
				for (int contador = 1;contador < pk.Length;contador ++) 
				{
					orderby = orderby + ", " + pk[contador] + " ASC";
				}
			}
			if (where == "") where = null;

//			if ((from != "OPERATIONS") && (from != "OPERATIONS_HIS") && (from != "ALARMS_HIS") 
//				&& (from != "FINES_HIS") && (from != "MSGS_HIS"))
//			{
				ssql.Append("SELECT " + select + ", RANK_VALUE ");
				ssql.Append("FROM ");
				ssql.Append("(SELECT " + select + ", RANK() OVER (ORDER BY " + orderby + ") AS RANK_VALUE ");
				ssql.Append("FROM " + from + " ");
				if (where != null) ssql.Append("WHERE (" + where + ")");
				ssql.Append(") ");

				ssql.Append("WHERE (RANK_VALUE < " + (rowend + 1).ToString() + " ");
				ssql.Append("AND RANK_VALUE > " + (rowstart - 1).ToString() + ") ");

				if (groupby != null) ssql.Append("GROUP BY " + groupby + " ");		
	
				ssql.Append("ORDER BY RANK_VALUE ASC");
//			}
//			else
//			{
//
//				// Custom action for table operations (high volume & no modify)
//				// We use a built-in order to speed up query OPE_RANK
//				// Only usable if there's no query selected from user
//				
//				ssql.Append ("SELECT ");
//				ssql.Append (select);
//			
//				// Let's think...
//				// A normal query (no filter) will have 
//				//OPE_VALID=1 AND OPE_DELETED=0 AND OPE_RANK > xxxxxx AND OPE_RANK < xxxxxx
//				// For a total of 3 AND's, a query filtered will have at least 4 AND's
//
//				string variable = "AND";
//				int suba = where.IndexOf(variable,0);
//				int subb = where.IndexOf(variable,suba+1);
//				int subc = where.IndexOf(variable,subb+1);
//				int subd = where.IndexOf(variable,subc+1);
//
//
//				if (subd > 0)
//				{
//					ssql.Append (", RANK_VALUE ");
//					ssql.Append (" FROM ");
//					ssql.Append ('(');
//					ssql.Append ("SELECT ");
//					ssql.Append (select);
//					ssql.Append (", RANK() ");
//					ssql.Append (" OVER ( ORDER BY ");									
//					ssql.Append (orderby);
//					ssql.Append (')');
//					ssql.Append (" AS RANK_VALUE");
//				}
//				//			}
//				//			else
//				//			{
//				//			}
//				ssql.Append (" FROM ");
//				ssql.Append (from);
//				// changes query to match where with Rank, first where, after rank...
//				if (where != null) 
//				{
//					ssql.Append (" WHERE ");
//					ssql.Append ('(');
//					ssql.Append (where);
//					ssql.Append (')');
//				}
//
//			
//				//			string variable2 = "OPE_VALID=1 AND OPE_DELETED=0";
//				//			string suba2 = where.Substring(0,variable2.Length-1);
//				//			string subb2 = variable2.Substring(0,variable2.Length-1);
//
//				//			if (suba2 != subb2 || from != "OPERATIONS")
//
//				//			if (where != null.Substring(0,variable2.Length-1) == variable2.Substring(0,variable2.Length-1) || from != "OPERATIONS")
//				//			{
//				if (subd > 0)
//				{
//					ssql.Append (')');
//					ssql.Append ("WHERE ");
//				
//					ssql.Append ('(');
//					ssql.Append ("RANK_VALUE < (");
//					ssql.Append ((rowend + 1).ToString());
//					ssql.Append (") AND RANK_VALUE > (");
//					ssql.Append ((rowstart - 1).ToString());
//					ssql.Append (')');
//					ssql.Append (')');
//				}
//				//			}
//				if (groupby != null) 
//				{
//					ssql.Append (" GROUP BY ");
//					ssql.Append (groupby);			
//				}
//				ssql.Append (" ORDER BY ");
//				ssql.Append (orderby);
//
//			}

			return ssql.ToString();
		}

		// That function MUST return the string representation of a parameter. Also, it creates a SqlParameter object and stores it
		/// <summary>
		/// Creates a derived IDbDataParameter for the table and fields specified
		/// </summary>
		/// <param name="table">Name of the table of the field asscoiated to the param</param>
		/// <param name="field">Name of the field associated to the parameter</param>
		/// <param name="nparam">Ordinal position of the parameter (0 is first)</param>
		/// <param name="tableInfo">Hashtable with the schema of the table</param>
		/// <param name="stringRep">Representation of the parameter in the SQL String (:param)</param>
		/// <param name="bOriginalVersion">If true the value of the parameter is bound to the original version of the value in the datarow</param>		
		/// <returns>The newly created parameter object</returns>
		public IDataParameter CreateParameter (string table, string field, int nparam, Hashtable tableInfo,  out string stringRep, bool bOriginalVersion)
		{
			stringRep = ":param" + nparam;
			FieldSchemaInfo fieldInfo = (FieldSchemaInfo)tableInfo[field];
            // Creates an  Oracle parameter (names :paramX) where X is the ordinal (0-based)
            OracleParameter param = new OracleParameter();
			param.ParameterName = "param" + nparam;
			param.SourceColumn =  field;
			param.OracleDbType = (OracleDbType)fieldInfo.NativeType;
			if (fieldInfo.FieldSize!=-1) param.Size = fieldInfo.FieldSize;
			if (bOriginalVersion) param.SourceVersion = DataRowVersion.Original;
			return param;
		}	
	}
}

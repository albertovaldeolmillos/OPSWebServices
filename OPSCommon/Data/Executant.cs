using System;
using System.Data;

namespace OPS.Components.Data
{

	/// <summary>
	/// Defines a set of functions to every-data component  must have
	/// </summary>
	public interface IExecutant
	{
		/// <summary>
		/// Gets a DataTable with ALL data (not sorted nor filter)
		/// </summary>
		/// <returns>A DataTable with Data</returns>
		DataTable GetData ();

		/// <summary>
		/// Gets a DataTable with All data, but only the fields specified
		/// </summary>
		/// <param name="fields">Array of fields to receive</param>
		/// <returns>A DataTable with Data</returns>
		DataTable GetData (string[] fields);

		/// <summary>
		/// Gets a DataTable with All data, but only the fields specified
		/// </summary>
		/// <param name="fields">Array of fields to receive</param>
		/// <param name="where">Where clausule (with NO WHERE keyword)</param>
		/// <param name="values">Values of the parameters (needed when sql where clausule are applied)</param>
		/// <returns>A DataTable with Data</returns>
		DataTable GetData (string[] fields, string where, object[] values);

		/// <summary>
		/// Gets a DataTable with All data, but only the fields specified
		/// </summary>
		/// <param name="fields">Array of fields to receive</param>
		/// <param name="where">Where clausule (with NO WHERE keyword)</param>
		/// <param name="orderby">Order by clausule (with NO ORDER BY keyword)</param>
		/// <param name="values">Values of the parameters (needed when sql where clausule are applied)</param>
		/// <returns>A DataTable with Data</returns>
		DataTable GetData (string[] fields, string where, string orderby, object [] values);

		/// <summary>
		/// Gets a Datatable with data specified on the rawsql query.
		/// </summary>
		/// <param name="rawsql">Full SQL query (select, from, where, orderby)</param>
		/// <param name="values">Values of the parameters (needed when sql where clausule are applied)</param>
		/// <returns>A DataTable with Data</returns>
		DataTable GetData (string rawsql, object [] values);

		/// <summary>
		/// Gets a DataTable starting at specified register and finishing at other register
		/// </summary>
		/// <param name="rowstart">1st register to fetch</param>
		/// <param name="rowend">last register to fetch</param>
		/// <returns>A DataTable with data</returns>
		DataTable GetPagedData (int rowstart, int rowend);

		/// <summary>
		/// Gets a DataTable starting at register at specified and finishing at other register
		/// </summary>
		/// <param name="fields">Array of fields </param>
		/// <param name="rowstart">1st register to fetch</param>
		/// <param name="rowend">Last register to fetch</param>
		/// <returns></returns>
		DataTable GetPagedData (string[] fields, int rowstart, int rowend);

		/// <summary>
		/// GGets a DataTable starting at register at specified and finishing at other register
		/// </summary>
		/// <param name="orderByField">Field for sorting</param>
		/// <param name="orderByASc">true if sort is ASCending</param>
		/// <param name="where">WHERE clausule</param>
		/// <param name="rowstart">1st register to fetch</param>
		/// <param name="rowend">Last register to fetch</param>
		/// <returns>DataTable with Data</returns>
		DataTable GetPagedData (string orderByField, bool orderByASc, string where, int rowstart, int rowend);

		/// <summary>
		/// Gets a DataTable starting at register at specified and finishing at other register
		/// </summary>
		/// <param name="fields">Array of fields to get</param>
		/// <param name="orderByField">Field for sorting</param>
		/// <param name="orderByASc">true if sort is ASCending</param>
		/// <param name="where">WHERE clausule</param>
		/// <param name="rowstart">1st register to fetch</param>
		/// <param name="rowend">Last register to fetch</param>
		/// <returns>DataTable with Data</returns>		
		DataTable GetPagedData(string[] fields, string orderByField, bool orderByASc, string where, int rowstart, int rowend);
	
		/// <summary>
		/// Saves the data to the DataSource.
		/// </summary>
		/// <param name="dt">DataTable containing the data to send back to Datasource</param>
		void SaveData (DataTable dt);
	}

	/// <summary>
	/// That class allows a easy-implementation of IExecutant, taking implementation of all overloadings.
	/// </summary>
	public abstract class CmpExecutantBase : IExecutant
	{
		public DataTable GetData () 
		{ 
			return GetData (null, null, null, null); 
		}
		public DataTable GetData (string[] fields)
		{
			return GetData (fields, null, null, null);
		}

		public DataTable GetData(string[] fields, string where, object[] values)
		{
			return GetData (fields, where, null, values);
		}

		public DataTable GetPagedData (int rowstart, int rowend) 
		{ 
			return GetPagedData (null,rowstart,rowend); 
		}
		public DataTable GetPagedData (string orderByField, bool orderByASc, string where, int rowstart, int rowend)
		{
			return GetPagedData(null, orderByField, orderByASc, where, rowstart, rowend);
		}

		public DataTable GetPagedData (string[] fields, int rowstart, int rowend)
		{
			return GetPagedData (fields, null, true, null, rowstart, rowend);
		}


		// Functions to implement at derived classes...
		public abstract DataTable GetData (string[] fields, string where, string orderby, object[] values);
		public abstract DataTable GetData (string rawsql, object[] values);
		public abstract DataTable GetPagedData(string[] fields, string orderByField, bool orderByASc, string where, int rowstart, int rowend);
		public abstract void SaveData (DataTable dt);
	}			
}

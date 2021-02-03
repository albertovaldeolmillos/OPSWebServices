using System;
using System.Data;




namespace OPS.Components.Data
{
	/// <summary>
	/// Database component for Statistics.
	/// </summary>
	public class StatisticsDB 
	{

		/// <summary>
		/// Executes a Query and fills a DataSet.
		/// </summary>
		/// <param name="select">Fields to SELECT.</param>
		/// <param name="from">Tables and joins clauses.</param>
		/// <param name="where">Filters to add to where. Null if none.</param>
		/// <param name="groupBy">Fields to GROUP BY. Null if none.</param>
		/// <param name="orderBy">Fields to ORDER BY. Null if none.</param>
		/// <returns>Returns a DataSet which contains a DataTable. The table name is "Results".</returns>
		public DataSet FillDataSet(
			string select, 
			string from, 
			string where, 
			string groupBy,
			string orderBy
			)
		{
			Database d = DatabaseFactory.GetDatabase();			

			return d.FillDataSet(select, from, orderBy, where, groupBy, "Results", -1);
		}

		/// <summary>
		/// Executes a Query and fills a DataSet.
		/// </summary>
		/// <param name="select">Fields to SELECT.</param>
		/// <param name="from">Tables and joins clauses.</param>
		/// <param name="where">Filters to add to where. Null if none.</param>
		/// <param name="groupBy">Fields to GROUP BY. Null if none.</param>
		/// <param name="orderBy">Fields to ORDER BY. Null if none.</param>
		/// <param name="values">Object array with parameters values.</param>
		/// <returns>Returns a DataSet which contains a DataTable. The table name is "Results".</returns>
		public DataSet FillDataSet(
			string select, 
			string from, 
			string where, 
			string groupBy,
			string orderBy,
			object[] values
			)
		{
			Database d = DatabaseFactory.GetDatabase();
			
			return d.FillDataSet(select, from, orderBy, where, groupBy, "Results", -1, values);
		}

		public DataSet FillDataSet(
			string table, 
			string select, 
			string from, 
			string where, 
			string groupBy,
			string orderBy,
			object[] values
			)
		{
			Database d = DatabaseFactory.GetDatabase();
			
			return d.FillDataSet(select, from, orderBy, where, groupBy, table, -1, values);
		}


		public void FillDataSet(
			ref DataSet dataSet,
			string table, 
			string select, 
			string from, 
			string where, 
			string groupBy,
			string orderBy,
			object[] values
			)
		{
			Database d = DatabaseFactory.GetDatabase();			
			d.FillDataSet(ref dataSet, select, from, orderBy, where, groupBy, table, -1, values);
		}

	}
}

using System;

using System.Data;
using System.Text;

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for PARAMETERS.
	/// </summary>
	public class CmpParametersDB : CmpGenericBase
	{
		public CmpParametersDB()
		{
			_standardFields		= new string[] {"PAR_ID", "PAR_DESCSHORT", "PAR_DESCLONG", "PAR_VALUE", "PAR_CATEGORY"};
			_standardPks		= new string[] {"PAR_ID"};
			_standardTableName	= "PARAMETERS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"PAR_VALID", "PAR_DELETED"};
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpParametersDB() {}
//
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"PAR_ID", "PAR_DESCSHORT", "PAR_VALUE", "PAR_CATEGORY"};
//			}
//			string[] pk = new string[] {"PAR_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby,values,"PARAMETERS","PARAMETERS",pk);
//
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//
//			if (fields == null) fields = new string[] {"PAR_ID", "PAR_DESCSHORT", "PAR_VALUE", "PAR_CATEGORY"};
//			string[] pk = new string[] {"PAR_ID"};
//			StringBuilder sb = base.ProcessFields(fields, pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"PARAMETERS","PARAMETERS",pk);
//		}
//
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO PARAMETERS (PAR_ID, PAR_DESCSHORT, PAR_VALUE, PAR_CATEGORY, PAR_VERSION) VALUES (@PARAMETERS.PAR_ID@, @PARAMETERS.PAR_DESCSHORT@, @PARAMETERS.PAR_VALUE@, @PARAMETERS.PAR_CATEGORY@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE PARAMETERS SET PAR_DESCSHORT = @PARAMETERS.PAR_DESCSHORT@, PAR_VALUE = @PARAMETERS.PAR_VALUE@, PAR_CATEGORY = @PARAMETERS.PAR_CATEGORY@ WHERE PAR_ID = @PARAMETERS.PAR_ID@", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM PARAMETERS WHERE PAR_ID = @PARAMETERS.PAR_ID@", false);
//			IDbConnection con = d.GetNewConnection();
//			con.Open();
//			da.InsertCommand.Connection = con;
//			da.UpdateCommand.Connection = con;
//			da.DeleteCommand.Connection = con;
//			d.UpdateDataSet(da,dt);
//			dt.AcceptChanges();
//			con.Close();
//
//		}
//		public override void GetForeignData(DataSet ds, string sTable) {}
//		public override string MainTable  {get { return "PARAMETERS"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM PARAMETERS";
//			if (where != null) 
//			{
//				sql = sql + " WHERE " + where;
//			}
//			return Convert.ToInt64(d.ExecuteScalar(sql, values));
//		}
//
//		public override Int64 LastPKValue
//		{
//			get 
//			{
//				Database d = DatabaseFactory.GetDatabase();
//				m.WaitOne();
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(PAR_ID),0) FROM PARAMETERS"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
//		// **************** END implementation of ICmpDataSource ************************

		/// <summary>
		/// Gets the value of a parameter identified by its PAR_DESCSHORT
		/// </summary>
		/// <param name="pdescshort">PAR_DESCSHORT of the parameter</param>
		/// <returns>A string containing the parameter value</returns>
		public string GetParameter (string pdescshort)
		{
			Database d = DatabaseFactory.GetDatabase();
			IDataReader dr = d.ExecQuery ("SELECT PAR_VALUE FROM PARAMETERS WHERE PAR_DESCSHORT = @PARAMETERS.PAR_DESCSHORT@",pdescshort);
			string parResult = "";
			if (dr.Read())
				parResult = dr.GetString(0);
			dr.Close();
			return parResult;
		}

		/// <summary>
		/// Sets the value of a parameter identified by its PAR_DESCSHORT
		/// </summary>
		/// <param name="pdescshort">PAR_DESCSHORT of the parameter</param>
		/// <param name="pvalue">Value of the parameter</param>
		public void SetParameter (string pdescshort, string pvalue)
		{
			Database d = DatabaseFactory.GetDatabase();
			d.ExecuteNonQuery("UPDATE PARAMETERS SET PAR_VALUE = @PARAMETERS.PAR_VALUE@ WHERE PAR_DESCSHORT = @PARAMETERS.PAR_DESCSHORT@", pvalue, pdescshort);
		}
	}
}

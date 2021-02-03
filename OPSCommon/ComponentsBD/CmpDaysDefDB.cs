namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for DAYS_DEF.
	/// </summary>
	public class CmpDaysDefDB : CmpGenericBase
	{
		public CmpDaysDefDB()
		{
			_standardFields		= new string[] {"DDAY_ID", "DDAY_CODE", "DDAY_DESCSHORT", "DDAY_DESCLONG"};
			_standardPks		= new string[] {"DDAY_ID"};
			_standardTableName	= "DAYS_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"DDAY_VALID", "DDAY_DELETED"};
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpDaysDefDB() {}
//
//		#region Implementation of CmpDataSourceAdapter
//
//		public override DataTable GetData (string[] fields, string where, string orderby, object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"DDAY_ID", "DDAY_DESCSHORT", "DDAY_DESCLONG", "DDAY_CODE"};
//			}
//			string[] pk = new string[] {"DDAY_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby, values, "DAYS_DEF", "DAYS_DEF", pk);
//		}
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			if (fields == null) fields = new string[] {"DDAY_ID", "DDAY_DESCSHORT", "DDAY_DESCLONG", "DDAY_CODE"};
//			string[] pk = new string[] {"DDAY_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"DAYS_DEF","DAYS_DEF",pk);
//		}
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO DAYS_DEF (DDAY_ID, DDAY_DESCSHORT, DDAY_DESCLONG, DDAY_CODE) VALUES (@DAYS_DEF.DDAY_ID@, @DAYS_DEF.DDAY_DESCSHORT@, @DAYS_DEF.DDAY_DESCLONG@, @DAYS_DEF.DDAY_CODE@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE DAYS_DEF SET DDAY_ID = @DAYS_DEF.DDAY_ID@, DDAY_DESCSHORT = @DAYS_DEF.DDAY_DESCSHORT@, DDAY_DESCLONG = @DAYS_DEF.DDAY_DESCLONG@, DDAY_CODE = @DAYS_DEF.DDAY_CODE@ WHERE DDAY_ID = #DAYS_DEF.DDAY_ID#", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM DAYS_DEF WHERE DDAY_ID = @DAYS_DEF.DDAY_ID@", false);
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
//		public override void GetForeignData(DataSet ds, string sTable) 
//		{
//		}
//		public override string MainTable  {get { return "DAYS_DEF"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM DAYS_DEF";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(DDAY_ID),0) FROM DAYS_DEF"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
//
//		#endregion
	}
}

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for STATUS_PROFILES.
	/// </summary>
	public class CmpStatusProfilesDB : CmpGenericBase
	{
		public CmpStatusProfilesDB()
		{
			_standardFields		= new string[] {"PSTA_ID", "PSTA_DESCSHORT", "PSTA_DESCLONG"};
			_standardPks		= new string[] {"PSTA_ID"};
			_standardTableName	= "STATUS_PROFILES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpStatusProfilesDB() {}
//
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"PSTA_ID","PSTA_DESCSHORT" ,"PSTA_DESCLONG"};
//			}
//			string[] pk = new string[] {"PSTA_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby,values,"STATUS_PROFILES","STATUS_PROFILES",pk);
//
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//
//			if (fields == null) fields = new string[] {"PSTA_ID", "PSTA_DESCSHORT" ,"PSTA_DESCLONG"};
//			string[] pk = new string[] {"PSTA_ID"};
//			StringBuilder sb = base.ProcessFields(fields, pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"STATUS_PROFILES","STATUS_PROFILES",pk);
//		}
//
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO STATUS_PROFILES (PSTA_ID, PSTA_DESCSHORT, PSTA_DESCLONG) VALUES (@STATUS_PROFILES.PSTA_ID@, @STATUS_PROFILES.PSTA_DESCSHORT@, @STATUS_PROFILES.PSTA_DESCLONG@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE STATUS_PROFILES SET PSTA_DESCSHORT = @STATUS_PROFILES.PSTA_DESCSHORT@, PSTA_DESCLONG = @STATUS_PROFILES.PSTA_DESCLONG@ WHERE PSTA_ID = @STATUS_PROFILES.PSTA_ID@", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM STATUS_PROFILES WHERE PSTA_ID = @STATUS_PROFILES.PSTA_ID@", false);
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
//		public override string MainTable  {get { return "STATUS_PROFILES"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM STATUS_PROFILES";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(PSTA_ID),0) FROM STATUS_PROFILES"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
	}
}
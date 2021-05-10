namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for MODULES.
	/// </summary>
	public class CmpModulesDB : CmpGenericBase
	{
		public CmpModulesDB()
		{
			_standardFields		= new string[] {"MOD_ID", "MOD_DESCSHORT", "MOD_DESCLONG", "MOD_ORDER", "MOD_LIT_ID"};
			_standardPks		= new string[] {"MOD_ID"};
			_standardTableName	= "MODULES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpModulesDB() {}
//
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"MOD_ID","MOD_DESCSHORT","MOD_LIT_ID","MOD_ORDER"};
//			}
//			string[] pk = new string[] {"MOD_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby,values,"MODULES","MODULES",pk);
//
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//
//			if (fields == null) fields = new string[] {"MOD_ID","MOD_DESCSHORT","MOD_LIT_ID","MOD_ORDER"};
//			string[] pk = new string[] {"MOD_ID"};
//			StringBuilder sb = base.ProcessFields(fields, pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"MODULES","MODULES",pk);
//		}
//
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO MODULES (MOD_ID, MOD_DESCSHORT, MOD_LIT_ID, MOD_ORDER) VALUES (@MODULES.MOD_ID@, @MODULES.MOD_DESCSHORT@, @MODULES.MOD_LIT_ID@ ,@MODULES.MOD_ORDER@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE MODULES SET MOD_DESCSHORT = @MODULES.MOD_DESCSHORT@, MOD_LIT_ID = @MODULES.MOD_LIT_ID@, MOD_ORDER = @MODULES.MOD_ORDER@ WHERE MOD_ID = @MODULES.MOD_ID@", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM MODULES WHERE MOD_ID = @MODULES.MOD_ID@", false);
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
//		public override string MainTable  {get { return "MODULES"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM MODULES";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(MOD_ID),0) FROM MODULES"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
	}
}

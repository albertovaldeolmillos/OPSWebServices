namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for VW_ALARMS.
	/// </summary>
	public class CmpAlarmsZoneDB : CmpGenericBase
	{
		public CmpAlarmsZoneDB()
		{
			_standardFields		= new string[] {"ALA_UNI_ID", "DALA_LIT_ID", "UNI_DESCSHORT"};
			_standardPks		= new string[0];
			_standardTableName	= "VW_ALARMS";
	
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpAlarmsZoneDB() {}
//
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"ALA_UNI_ID","DALA_LIT_ID","UNI_DESCSHORT"};
//			}
//			string[] pk = null;
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby,values,"VW_ALARMS","VW_ALARMS",pk);
//
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//
//			if (fields == null) fields = new string[] {"ALA_UNI_ID","DALA_LIT_ID","UNI_DESCSHORT"};
//			string[] pk = null;
//			StringBuilder sb = base.ProcessFields(fields, pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"VW_ALARMS","VW_ALARMS",pk);
//		}
//
//		public override void SaveData (DataTable dt) {}
//		public override void GetForeignData(DataSet ds, string sTable) {}
//		public override string MainTable  {get { return "VW_ALARMS"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM VW_ALARMS";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(ALA_UNI_ID),0) FROM VW_ALARMS"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
	}
}



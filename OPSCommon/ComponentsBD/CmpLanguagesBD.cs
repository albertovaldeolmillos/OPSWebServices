namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for LANGUAGES.
	/// </summary>
	public class CmpLanguagesDB : CmpGenericBase
	{
		public CmpLanguagesDB()
		{
			_standardFields		= new string[] {"LAN_ID", "LAN_DESCSHORT", "LAN_DESCLONG", "LAN_IMAGE"};
			_standardPks		= new string[] {"LAN_ID"};
			_standardTableName	= "LANGUAGES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"LAN_VALID", "LAN_DELETED"};
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpLenguagesBD() {}
//
//		// ********************* Implementation of CmpDataSourceAdapter ***************************		
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			string[] pk = new string[] {"LAN_ID"};
//			if (fields == null) 
//			{
//				fields = new string[] {"LAN_ID","LAN_DESCSHORT","LAN_DESCLONG","LAN_IMAGE"};
//			}
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby, values,"LANGUAGES","LANGUAGES", pk);
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			if (fields == null) 
//			{
//				fields = new string[] {"LAN_ID","LAN_DESCSHORT","LAN_DESCLONG","LAN_IMAGE"};
//			}
//			string[] pk = new string[] {"LAN_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(),orderByField,orderByAsc,where,rowstart,rowend,"LANGUAGES","LANGUAGES",pk);
//		}
//
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO LANGUAGES (LAN_ID, LAN_DESCSHORT, LAN_DESCLONG, LAN_IMAGE,) VALUES (@LANGUAGES.LAN_ID@, @LANGUAGES.LAN_DESCSHORT@, @LANGUAGES.LAN_DESCLONG@, @LANGUAGES.LAN_IMAGE@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE LANGUAGES SET LAN_DESCSHORT = @LANGUAGES.LAN_DESCSHORT@, LAN_DESCLONG = @LANGUAGES.LAN_DESCLONG@, LAN_IMAGE = @LANGUAGES.LAN_IMAGE@  WHERE LAN_ID = @LANGUAGES.LAN_ID@", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM LANGUAGES WHERE LAN_ID = @LANGUAGES.LAN_ID@", false);
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
//		public override string MainTable  {get { return "LANGUAGES"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM LANGUAGES";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(LAN_ID),0) FROM LANGUAGES"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
	}
}

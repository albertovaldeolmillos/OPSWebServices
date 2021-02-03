namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for PAYTYPES_DEF.
	/// </summary>
	public class CmpPaytypesDefDB : CmpGenericBase
	{
		public CmpPaytypesDefDB()
		{
			_standardFields		= new string[] {"DPAY_ID", "DPAY_DESCSHORT", "DPAY_DESCLONG"};
			_standardPks		= new string[] {"DPAY_ID"};
			_standardTableName	= "PAYTYPES_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"DPAY_VALID", "DPAY_DELETED"};
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpPaytypesDefDB() {}
//
//		// ********************* Implementation of CmpDataSourceAdapter ***************************		
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			string[] pk = new string[] {"DPAY_ID"};
//			if (fields == null) 
//			{
//				fields = new string[] {"DPAY_ID","DPAY_DESCSHORT"};
//			}
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby, values,"PAYTYPES_DEF","PAYTYPES_DEF", pk);
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			if (fields == null) 
//			{
//				fields = new string[] {"DPAY_ID","DPAY_DESCSHORT"};
//			}
//			string[] pk = new string[] {"DPAY_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(),orderByField,orderByAsc,where,rowstart,rowend,"PAYTYPES_DEF","PAYTYPES_DEF",pk);
//		}
//
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO PAYTYPES_DEF (DPAY_ID, DPAY_DESCSHORT) VALUES (@PAYTYPES_DEF.DPAY_ID@, @PAYTYPES_DEF.DPAY_DESCSHORT@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE PAYTYPES_DEF SET DPAY_DESCSHORT = @PAYTYPES_DEF.DPAY_DESCSHORT@ WHERE DPAY_ID = @PAYTYPES_DEF.DPAY_ID@", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM PAYTYPES_DEF WHERE DPAY_ID = @PAYTYPES_DEF.DPAY_ID@", false);
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
//		public override string MainTable  {get { return "PAYTYPES_DEF"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM PAYTYPES_DEF";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(DPAY_ID),0) FROM PAYTYPES_DEF"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
	}
}

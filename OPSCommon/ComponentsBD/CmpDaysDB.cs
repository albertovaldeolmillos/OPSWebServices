namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for DAYS.
	/// </summary>
	public class CmpDaysDB : CmpGenericBase
	{
		public CmpDaysDB()
		{
			_standardFields		= new string[] {"DAY_ID", "DAY_DDAY_ID", "DAY_DATE", "DAY_DESC"};
			_standardPks		= new string[] {"DAY_ID"};
			_standardTableName	= "DAYS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"DAY_VALID", "DAY_DELETED"};
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpDaysDB(){}
//
//		#region Implementation of CmpDataSourceAdapter
//
//		public override DataTable GetData (string[] fields, string where, string orderby, object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"DAY_ID","DAY_DDAY_ID","DAY_DATE","DAY_DESC"};
//			}
//			string[] pk = new string[] {"DAY_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby, values, "DAYS","DAYS",pk);
//		}
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			if (fields == null) fields = new string[] {"DAY_ID", "DAY_DDAY_ID", "DAY_DATE", "DAY_DESC"};
//			string[] pk = new string[] {"DAY_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"DAYS","DAYS",pk);
//		}
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO DAYS (DAY_ID, DAY_DDAY_ID, DAY_DATE, DAY_DESC) VALUES (@DAYS.DAY_ID@, @DAYS.DAY_DDAY_ID@, @DAYS.DAY_DATE@, @DAYS.DAY_DESC@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE DAYS SET DAY_ID = @DAYS.DAY_ID@, DAY_DDAY_ID = @DAYS.DAY_DDAY_ID@, DAY_DATE = @DAYS.DAY_DATE@, DAY_DESC= @DAYS.DAY_DESC@ WHERE DAY_ID = #DAYS.DAY_ID# AND DAY_DDAY_ID = #DAYS.DAY_DDAY_ID#", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM DAYS WHERE DAY_ID = @DAYS.DAY_ID@ AND DAY_DDAY_ID = @DAYS.DAY_DDAY_ID@", false);
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
//			// Add the table of languages to the DataSet
//			DataTable dtDaysDef = new CmpDayTypeDB().GetData();
//			ds.Tables.Add (dtDaysDef);
//			DataTable parent = ds.Tables[sTable];
//			ds.Relations.Add ((dtDaysDef.PrimaryKey)[0],parent.Columns["DAY_DDAY_ID"]);
//
//		}
//		public override string MainTable  {get { return "DAYS"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM DAYS";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(DAY_ID),0) FROM DAYS"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
//
//		#endregion
	}
}

//using System;
//using System.Data;
//
//using System.Text;
//
//namespace OPS.Components.Data
//{
//	/// <summary>
//	/// CmpHourBlocksDB
//	/// </summary>
//	public class CmpHourBlocksDB: CmpBase
//	{
//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpHourBlocksDB() {}
//
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"TIM_ID","TIM_DESC","TIM_INI","TIM_END"};
//			}
//			string[] pk = new string[] {"TIM_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby,values,"TIMETABLES","TIMETABLES",pk);
//
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//
//			if (fields == null) fields = new string[] {"TIM_ID","TIM_DESC","TIM_INI","TIM_END"};
//			string[] pk = new string[] {"TIM_ID"};
//			StringBuilder sb = base.ProcessFields(fields, pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"TIMETABLES","TIMETABLES",pk);
//		}
//
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO TIMETABLES (TIM_ID, TIM_DESC, TIM_INI,TIM_END) VALUES (@TIMETABLES.TIM_ID@, @TIMETABLES.TIM_DESC@, @TIMETABLES.TIM_INI@, @TIMETABLES.TIM_END@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE TIMETABLES SET TIM_DESC = @TIMETABLES.TIM_DESC@, TIM_INI = @TIMETABLES.TIM_INI@, TIM_END = @TIMETABLES.TIM_END@ WHERE TIM_ID = @TIMETABLES.TIM_ID@", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM TIMETABLES WHERE TIM_ID = @TIMETABLES.TIM_ID@", false);
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
//		public override string MainTable  {get { return "TIMETABLES"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM TIMETABLES";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(TIM_ID),0) FROM TIMETABLES"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
//	}
//}

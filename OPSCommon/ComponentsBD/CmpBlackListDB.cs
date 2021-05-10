//using System;
//using System.Data;
//using System.Text;
//
//
//namespace OPS.Components.Data
//{
//	/// <summary>
//	/// Summary description for CmpBlackListDB.
//	/// </summary>
//	public class CmpBlackListDB : CmpBase
//	{
//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpBlackListDB() {}
//
//		#region Implementation of ICmpDataSourceAdapter
//
//		public override DataTable GetData (string[] fields, string where, string orderby, object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"BLIS_ID","BLIS_DBLIS_ID","BLIS_CATEGORY","BLIS_DESCSHORT","BLIS_DESCLONG","BLIS_VALUE","BLIS_TYPE"};
//			}
//			string[] pk = new string[] {"BLIS_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby, values, "BLACK_LISTS","BLACK_LISTS",pk);
//		}
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			if (fields == null) fields = new string[] {"BLIS_ID","BLIS_DBLIS_ID","BLIS_CATEGORY","BLIS_DESCSHORT","BLIS_DESCLONG","BLIS_VALUE","BLIS_TYPE"};
//			string[] pk = new string[] {"BLIS_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"BLACK_LISTS","BLACK_LISTS",pk);
//		}
//		public override void SaveData (DataTable dt)
//		{
//				// No save data allowed by the moment....
//
//		}
//
//		public override void GetForeignData(System.Data.DataSet ds, string sTable)
//		{
//
//		}
//
//
//		public override string MainTable  {get { return "BLACK_LISTS"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM LISTS";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(BLIS_ID),0) FROM BLACK_LISTS"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
//		#endregion
//
//	}
//}

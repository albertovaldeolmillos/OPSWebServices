using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ROLES.
	/// </summary>
	public class CmpRolesDB : CmpGenericBase
	{
		public CmpRolesDB()
		{
			_standardFields		= new string[] {"ROL_ID", "ROL_DESCSHORT", "ROL_DESCLONG"};
			_standardPks		= new string[] {"ROL_ID"};
			_standardTableName	= "ROLES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
	
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"ROL_VALID", "ROL_DELETED"};
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpRolesDB() {}
//
//		// ********************* Implementation of CmpDataSourceAdapter ***************************
//
//
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"ROL_ID", "ROL_DESCSHORT"};
//			}
//			string[] pk = new string[] {"ROL_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby,values,"ROLES","ROLES",pk);
//
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"ROL_ID", "ROL_DESCSHORT"};
//			}
//			string[] pk = new string[] {"ROL_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);			
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"ROLES","ROLES",pk);
//		}
//
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO ROLES (ROL_ID, ROL_DESCSHORT) VALUES (@ROLES.ROL_ID@, @ROLES.ROL_DESCSHORT@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE ROLES SET ROL_DESCSHORT = @ROLES.ROL_DESCSHORT@ WHERE ROL_ID = @ROLES.ROL_ID@", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM ROLES WHERE ROL_ID = @ROLES.ROL_ID@", false);
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
//		public override string MainTable  {get { return "ROLES"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM ROLES";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(ROL_ID),0) FROM ROLES"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
		public DataTable GetDataByUser(int usrId)
		{	
			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = d.FillDataTable ("SELECT ROLES.ROL_ID,ROLES.ROL_DESCSHORT FROM ROLES INNER JOIN USERS ON  USERS.USR_ROL_ID = ROLES.ROL_ID WHERE USERS.USR_ID = @USERS.USR_ID@","ROLES",usrId);
			return dt;
		}
	}
}

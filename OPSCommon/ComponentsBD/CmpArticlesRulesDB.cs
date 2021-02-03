namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ARTICLES_RULES.
	/// </summary>
	public class CmpArticlesRulesDB : CmpGenericBase
	{
		public CmpArticlesRulesDB()
		{
			_standardFields		= new string[] {"RUL_ID", "RUL_DART_ID", "RUL_TAR_ID", "RUL_DGRP_ID", "RUL_CON_ID", "RUL_GRP_ID", "RUL_INIDATE", "RUL_ENDDATE"};
			_standardPks		= new string[] {"RUL_ID"};
			_standardTableName	= "ARTICLES_RULES";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"RUL_VALID", "RUL_DELETED"};
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpArticlesRulesDB() {}
//
//		#region Implementation of ICmpDataSourceAdapter
//		// ********************* Implementation of ICmpDataSourceAdapter ***************************
//
//		public override DataTable GetData (string[] fields, string where, string orderby, object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"RUL_ID","RUL_DART_ID","RUL_TAR_ID","RUL_DGRP_ID","RUL_CON_ID","RUL_GRP_ID", "RUL_INIDATE","RUL_ENDDATE"};
//			}
//			string[] pk = new string[] {"RUL_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby, values, "ARTICLES_RULES","ARTICLES_RULES",pk);
//		}
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			if (fields == null) fields = new string[] {"RUL_ID","RUL_DART_ID","RUL_TAR_ID","RUL_DGRP_ID","RUL_CON_ID","RUL_GRP_ID", "RUL_INIDATE","RUL_ENDDATE"};
//			string[] pk = new string[] {"RUL_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"ARTICLES_RULES","ARTICLES_RULES",pk);
//		}
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO ARTICLES_RULES (RUL_ID,RUL_DART_ID,RUL_TAR_ID,RUL_DGRP_ID,RUL_CON_ID,RUL_GRP_ID,RUL_INIDATE,RUL_ENDDATE) VALUES (@ARTICLES_RULES.RUL_ID@,@ARTICLES_RULES.RUL_DART_ID@,@ARTICLES_RULES.RUL_TAR_ID@,@ARTICLES_RULES.RUL_DGRP_ID@,@ARTICLES_RULES.RUL_CON_ID@,@ARTICLES_RULES.RUL_GRP_ID@, @ARTICLES_RULES.RUL_INIDATE@,@ARTICLES_RULES.RUL_ENDDATE@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE ARTICLES_RULES SET RUL_ID = @ARTICLES_RULES.RUL_ID@, RUL_DART_ID= @ARTICLES_RULES.RUL_DART_ID@, RUL_TAR_ID= @ARTICLES_RULES.RUL_TAR_ID@, RUL_DGRP_ID= @ARTICLES_RULES.RUL_DGRP_ID@,RUL_CON_ID=@ARTICLES_RULES.RUL_CON_ID@,RUL_GRP_ID= @ARTICLES_RULES.RUL_GRP_ID@, RUL_INIDATE = @ARTICLES_RULES.RUL_INIDATE@,RUL_ENDDATE=@ARTICLES_RULES.RUL_ENDDATE@ WHERE RUL_ID = #ARTICLES_RULES.RUL_ID#", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM ARTICLES_RULES WHERE RUL_ID = @ARTICLES_RULES.RUL_ID@", false);
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
//		public override string MainTable  {get { return "ARTICLES_RULES"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM ARTICLES_RULES";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(RUL_ID),0) FROM ARTICLES_RULES"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
//		#endregion
	}
}

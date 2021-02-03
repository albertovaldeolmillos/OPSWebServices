namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for LITERALS.
	/// </summary>
	public class CmpLiteralsDB : CmpGenericBase
	{
		public CmpLiteralsDB()
		{
			_standardFields		= new string[] {"LIT_ID", "LIT_LAN_ID", "LIT_DESCSHORT", "LIT_DESCLONG", "LIT_CATEGORY", "LIT_DLUNI_ID", "LIT_UNIQUE_ID"};
			_standardPks		= new string[] {"LIT_UNIQUE_ID"};
			_standardTableName	= "LITERALS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"LIT_VALID", "LIT_DELETED"};
		}

//		public CmpLiteralesDB(){}
//
//		// ********************* Implementation of CmpDataSourceAdapter ***************************
//
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"LIT_ID","LIT_LAN_ID","LIT_DESCSHORT","LIT_DESCLONG"};
//			}
//			string[] pk = new string[] {"LIT_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby,values,"LITERALS","LITERALS",pk);
//		}
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			if (fields == null) fields = new string[] {"LIT_ID", "LIT_LAN_ID", "LIT_DESCSHORT", "LIT_DESCLONG", "LIT_CATEGORY"};
//			string[] pk = new string[] {"LIT_ID","LIT_LAN_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"LITERALS","LITERALS",pk);
//		}
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO LITERALS (LIT_ID, LIT_LAN_ID, LIT_DESCSHORT, LIT_DESCLONG, LIT_CATEGORY) VALUES (@LITERALS.LIT_ID@, @LITERALS.LIT_LAN_ID@, @LITERALS.LIT_DESCSHORT@, @LITERALS.LIT_DESCLONG@, @LITERALS.LIT_CATEGORY@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE LITERALS SET LIT_ID = @LITERALS.LIT_ID@, LIT_LAN_ID = @LITERALS.LIT_LAN_ID@, LIT_DESCSHORT = @LITERALS.LIT_DESCSHORT@, LIT_DESCLONG= @LITERALS.LIT_DESCLONG@, LIT_CATEGORY= @LITERALS.LIT_CATEGORY@ WHERE LIT_ID = #LITERALS.LIT_ID# AND LIT_LAN_ID = #LITERALS.LIT_LAN_ID#", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM LITERALS WHERE LIT_ID = @LITERALS.LIT_ID@ AND LIT_LAN_ID = @LITERALS.LIT_LAN_ID@", false);
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
//			DataTable dtLanguages = new CmpLenguagesBD().GetData();
//			ds.Tables.Add (dtLanguages);
//			DataTable parent = ds.Tables[sTable];
//			ds.Relations.Add ((dtLanguages.PrimaryKey)[0],parent.Columns["LIT_LAN_ID"]);
//
//		}
//		public override string MainTable  {get { return "LITERALS"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM LITERALS";
//			if (where != null) 
//			{
//				sql = sql + " WHERE " + where;
//			}
//			return Convert.ToInt64(d.ExecuteScalar(sql, values));
//		}
//
//		public override Int64 LastPKValue
//		{
//			get {return -2L;}
//		}
	}
}

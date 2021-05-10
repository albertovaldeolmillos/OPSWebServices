namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for INTERVALS.
	/// </summary>
	public class CmpIntervalsDB : CmpGenericBase
	{
		public CmpIntervalsDB()
		{
			_standardFields		= new string[] {"INT_ID", "INT_STAR_ID", "INT_MINUTES", "INT_VALUE", "INT_VALID_INTERMEDIATE_POINTS", "INT_UNIQUE_ID"};
			_standardPks		= new string[] {"INT_UNIQUE_ID"};
			_standardTableName	= "INTERVALS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"INT_VALID", "INT_DELETED"};
		}

//		public CmpIntervalDB()
//		{
//		}
//
//		#region Implementation of CmpDataSourceAdapter
//
//		public override DataTable GetData (string[] fields, string where, string orderby, object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"INT_ID","INT_STAR_ID","INT_MINUTES","INT_VALUE"};
//			}
//			string[] pk = new string[] {"INT_ID","INT_STAR_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby, values, "DAYS","DAYS",pk);
//		}
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			if (fields == null) fields = new string[] {"INT_ID","INT_STAR_ID","INT_MINUTES","INT_VALUE"};
//			string[] pk = new string[] {"INT_ID","INT_STAR_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"INTERVALS","INTERVALS",pk);
//		}
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO INTERVALS (INT_ID, INT_STAR_ID, INT_MINUTES, INT_VALUE) VALUES (@INTERVALS.INT_ID@, @INTERVALS.INT_STAR_ID@, @INTERVALS.INT_MINUTES@, @INTERVALS.INT_VALUE@, @INTERVALS.INT_VERSION@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE INTERVALS SET INT_ID = @INTERVALS.INT_ID@, INT_STAR_ID= @INTERVALS.INT_STAR_ID@, INT_MINUTES= @INTERVALS.INT_MINUTES@, INT_VALUE= @INTERVALS.INT_VALUE@ WHERE INT_ID = #INTERVALS.INT_ID# AND INT_STAR_ID = #INTERVALS.INT_STAR_ID#", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM INTERVALS WHERE INT_ID= @INTERVALS.INT_ID@ AND INT_STAR_ID = @INTERVALS.INT_STAR_ID@", false);
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
//		public override string MainTable  {get { return "INTERVALS"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM INTERVALS";
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
//				return -2L;
//			}
//		}
//
//		#endregion
	}
}

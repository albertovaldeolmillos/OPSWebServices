namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for CONSTRAINTS.
	/// </summary>
	public class CmpConstraintsDB : CmpGenericBase
	{
		public CmpConstraintsDB()
		{
			_standardFields		= new string[] {"CON_ID", "CON_NUMBER", "CON_DCON_ID", "CON_DGRP_ID", "CON_GRP_ID", "CON_VALUE", "CON_UNIQUE_ID"};
			_standardPks		= new string[] {"CON_UNIQUE_ID"};
			_standardTableName	= "CONSTRAINTS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"CON_VALID", "CON_DELETED"};
		}

//		#region Implementation of ICmpDataSourceAdapter
//		// ********************* Implementation of ICmpDataSourceAdapter ***************************
//
//		public override DataTable GetData (string[] fields, string where, string orderby, object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"CON_ID","CON_NUMBER","CON_DGRP_ID","CON_GRP_ID","CON_DCON_ID", "CON_VALUE"};
//			}
//			string[] pk = new string[] {"CON_ID", "CON_NUMBER"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby, values, "CONSTRAINTS","CONSTRAINTS",pk);
//		}
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			fields = new string[] {"CON_ID","CON_NUMBER","CON_DGRP_ID","CON_GRP_ID","CON_DCON_ID", "CON_VALUE"};
//			string[] pk = new string[] {"CON_ID", "CON_NUMBER"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"CONSTRAINTS","CONSTRAINTS",pk);
//		}
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO CONSTRAINTS (CON_ID,CON_NUMBER,CON_DGRP_ID,CON_GRP_ID,CON_DCON_ID, CON_VALUE,CON_VERSION) VALUES (@CONSTRAINTS.CON_ID@,@CONSTRAINTS.CON_NUMBER@,@CONSTRAINTS.CON_DGRP_ID@,@CONSTRAINTS.CON_GRP_ID@,@CONSTRAINTS.CON_DCON_ID@, @CONSTRAINTS.CON_VALUE@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE CONSTRAINTS SET  CON_ID = @CONSTRAINTS.CON_ID@,CON_NUMBER=@CONSTRAINTS.CON_NUMBER@,CON_DGRP_ID=@CONSTRAINTS.CON_DGRP_ID@,CON_GRP_ID=@CONSTRAINTS.CON_DGRP_ID@,CON_DCON_ID=@CONSTRAINTS.CON_DCON_ID@, CON_VALUE=@CONSTRAINTS.CON_VALUE@ WHERE CON_ID = #CONSTRAINTS.CON_ID# AND CON_NUMBER = #CONSTRAINTS.CON_NUMBER#", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM CONSTRAINTS WHERE CON_ID = @CONSTRAINTS.CON_ID@ AND CON_NUMBER = @CONSTRAINTS.CON_NUMBER@", false);
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
//		public override string MainTable  {get { return "CONSTRAINTS"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM CONSTRAINTS";
//			if (where != null) 
//			{
//				sql = sql + " WHERE " + where;
//			}
//			return Convert.ToInt64(d.ExecuteScalar(sql, values));
//		}
//
//		public override Int64 LastPKValue
//		{
//			get { return -12L; }			// Multicolumn PK
//		}
//		#endregion
	}
}

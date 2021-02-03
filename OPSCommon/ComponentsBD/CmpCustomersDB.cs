namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for CUSTOMERS.
	/// </summary>
	public class CmpCustomersDB : CmpGenericBase
	{
		public CmpCustomersDB()
		{
			_standardFields		= new string[] {"CUS_ID", "CUS_NAME", "CUS_SURNAME1", "CUS_SURNAME2", "CUS_DNI", "CUS_PHONE", "CUS_ADDRESS"};
			_standardPks		= new string[] {"CUS_ID"};
			_standardTableName	= "CUSTOMERS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpCustomersDB() {}
//
//		// ****************************** ICmpDataSource implementation ************************
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"CUS_ID", "CUS_NAME", "CUS_SURNAME1", "CUS_SURNAME2", "CUS_DNI", "CUS_PHONE", "CUS_ADDRESS"};
//			}
//			string[] pk = new string[] {"CUS_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby,values,"CUSTOMERS","CUSTOMERS",pk);
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"CUS_ID", "CUS_NAME", "CUS_SURNAME1", "CUS_SURNAME2", "CUS_DNI", "CUS_PHONE", "CUS_ADDRESS"};
//			}
//			string[] pk = new string[] {"CUS_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"CUSTOMERS","CUSTOMERS",pk);
//		}
//
//		public override void SaveData (DataTable dt) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO CUSTOMERS (CUS_ID, CUS_NAME, CUS_SURNAME1, CUS_SURNAME2, CUS_DNI, CUS_PHONE, CUS_ADDRESS) VALUES (@CUSTOMERS.CUS_ID@, @CUSTOMERS.CUS_NAME@, @CUSTOMERS.CUS_SURNAME1@, @CUSTOMERS.CUS_SURNAME2@ ,@CUSTOMERS.CUS_DNI@, @CUSTOMERS.CUS_PHONE@, @CUSTOMERS.CUS_ADDRESS@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE CUSTOMERS SET CUS_NAME = @CUSTOMERS.CUS_NAME@, CUS_SURNAME1 = @CUSTOMERS.CUS_SURNAME1@, CUS_SURNAME2 = @CUSTOMERS.CUS_SURNAME2@, CUS_DNI = @CUSTOMERS.CUS_DNI@, CUS_PHONE = @CUSTOMERS.CUS_PHONE@, CUS_ADDRESS = @CUSTOMERS.CUS_ADDRESS@ WHERE CUS_ID = @CUSTOMERS.CUS_ID@", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM CUSTOMERS WHERE CUS_ID = @CUSTOMERS.CUS_ID@", false);
//			IDbConnection con = d.GetNewConnection();
//			con.Open();
//			da.InsertCommand.Connection = con;
//			da.UpdateCommand.Connection = con;
//			da.DeleteCommand.Connection = con;
//			d.UpdateDataSet(da,dt);
//			con.Close();
//		}
//
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM CUSTOMERS";
//			if (where != null) 
//			{
//				sql = sql + " WHERE " + where;
//			}
//			return Convert.ToInt64(d.ExecuteScalar(sql, values));
//		}
////		public Int64 GetMaximumPk ()
////		{
////			Database d = DatabaseFactory.GetDatabase();
////			m.WaitOne();
////			long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(CUS_ID),0) FROM CUSTOMERS"));
////			m.ReleaseMutex();
////			return l;
////		}
//
//		public override void GetForeignData(DataSet ds, string sTable)
//		{
//		}
//
//		public override string MainTable
//		{
//			get {return "CUSTOMERS";}
//		}
//		public override Int64 LastPKValue 
//		{
//			get 
//			{
//				Database d = DatabaseFactory.GetDatabase();
//				m.WaitOne();
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(CUS_ID),0) FROM CUSTOMERS"));
//				m.ReleaseMutex();
//				return l;
//			}		
//		}
	}
}

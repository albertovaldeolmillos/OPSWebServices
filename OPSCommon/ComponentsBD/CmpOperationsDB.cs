using System;
using System.Text;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS.
	/// </summary>
	public class CmpOperationsDB : CmpGenericBase
	{
		public CmpOperationsDB() 
		{
			_standardFields		= new string[] 
				{"OPE_ID", "OPE_DOPE_ID", "OPE_ART_ID", "OPE_GRP_ID", "OPE_UNI_ID", 
				"OPE_DPAY_ID", "OPE_INIDATE", "OPE_ENDDATE", "OPE_DURATION", "OPE_VALUE", "OPE_VEHICLEID", "OPE_MOBI_USER_ID", "OPE_DOPE_ID_VIS", "OPE_TICKETNUM"};
			_standardPks		= new string[] {"OPE_ID"};
			_standardTableName	= "OPERATIONS";
			_standardOrderByField	= "OPE_ID";
			_standardOrderByAsc		= "DESC";
		

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"OPE_DOPE_ID","OPE_ART_ID","OPE_GRP_ID","OPE_UNI_ID","OPE_DPAY_ID"} ;
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpOperationsDefDB,ComponentsBD","OPS.Components.Data.CmpArticlesDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"OPE_VALID", "OPE_DELETED"};
		}
		//static System.Threading.Mutex m = new System.Threading.Mutex();

		#region Specific Methods of CmpOperationsDB

		/// <summary>
		/// Inserts a new register in the OPERATIONS table.
		/// Returns 0 = 0K, -1 = ERR, 1 = OPERACION YA EXISTENTE
		/// </summary>
		/// <param name="dopeid">OPE_DOPE_ID value. Cannot be NULL</param>
		/// <param name="opeopeid">OPE_OPE_ID value. -1 for NULL</param>
		/// <param name="artid">OPE_ART_ID value. -1 for NULL</param>
		/// <param name="grpid">OPE_GRP_ID value. Cannot be NULL</param>
		/// <param name="uniid">OPE_UNI_ID value. Cannot be NULL</param>
		/// <param name="dpayid">OPE_DPAY_ID value. -1 for NULL</param>
		/// <param name="mov">OPE_MOVDATE value. DateTime.MinValue for NULL</param>
		/// <param name="ini">OPE_INIDATE value. DateTime.MinValue for NULL</param>
		/// <param name="end">OPE_ENDDATE value. DateTime.MinValue for NULL</param>
		/// <param name="duration">OPE_DURATION value. -1 for NULL</param>
		/// <param name="quantity">OPE_VALUE value. -1 for NULL</param>
		/// <param name="vehicleid">OPE_VEHICLEID value. May be NULL</param>
		/// 
		public int InsertOperation(int dopeid, int opeopeid, int artid, int grpid, int uniid, int dpayid, DateTime mov,
			DateTime ini, DateTime end, int duration, double quantity, string vehicleid,int dartid,int mobileUserId,
			int postpay, double dChipCardCredit, ulong ulChipCardId, double lFineNumber, long lFineType, int iRealDuration, 
			int quantityReturned, int iOpOnLine, int iTicketNumber, ref int nNewOperationsDB, out IDbTransaction tran)
		{

			tran=null;
			int nRdo = 0;
			if (ExistsOperation(dopeid, opeopeid, artid, grpid, uniid, dpayid, mov,
				ini, end, duration, quantity, vehicleid))
				return 1;

			Database d = DatabaseFactory.GetDatabase();
			//TimeSpan duration = end-ini;
			tran = null;
			IDbConnection con = null;
			//m.WaitOne();
			// Only one thread per time here....
			try 
			{
				con = d.GetNewConnection();
				con.Open();
				// Put the two operations inside a TRANSACTION because we want a block (updates to the
				// table cannot be allowed when we are reading the PK and inserting the data).
				tran =  con.BeginTransaction(IsolationLevel.Serializable);
				nNewOperationsDB = Convert.ToInt32(d.ExecuteScalar("select SEQ_OPERATIONS.NEXTVAL FROM DUAL", con, tran));
				string ssql = "INSERT INTO OPERATIONS "
					+ "(OPE_ID, OPE_DOPE_ID, OPE_OPE_ID, OPE_ART_ID, "
					+ "OPE_GRP_ID, OPE_UNI_ID, OPE_DPAY_ID, OPE_MOVDATE, "
					+ "OPE_INIDATE, OPE_ENDDATE, OPE_DURATION, OPE_VALUE, "
					+ "OPE_VEHICLEID,OPE_DART_ID, OPE_MOBI_USER_ID, OPE_POST_PAY, "
					+ "OPE_CHIPCARD_ID, OPE_CHIPCARD_CREDIT, OPE_FIN_ID, OPE_FIN_DFIN_ID, "
					+ "OPE_REALDURATION, OPE_VALUE_IN_RETURN, OPE_OP_ONLINE, OPE_TICKETNUM) VALUES "
					+ "(@OPERATIONS.OPE_ID@, @OPERATIONS.OPE_DOPE_ID@, @OPERATIONS.OPE_OPE_ID@, @OPERATIONS.OPE_ART_ID@, "
					+ "@OPERATIONS.OPE_GRP_ID@, @OPERATIONS.OPE_UNI_ID@, @OPERATIONS.OPE_DPAY_ID@, @OPERATIONS.OPE_MOVDATE@, "
					+ "@OPERATIONS.OPE_INIDATE@, @OPERATIONS.OPE_ENDDATE@, @OPERATIONS.OPE_DURATION@, @OPERATIONS.OPE_VALUE@, "
					+ "@OPERATIONS.OPE_VEHICLEID@,@OPERATIONS.OPE_DART_ID@,@OPERATIONS.OPE_MOBI_USER_ID@,@OPERATIONS.OPE_POST_PAY@, "
					+ "@OPERATIONS.OPE_CHIPCARD_ID@,@OPERATIONS.OPE_CHIPCARD_CREDIT@,@OPERATIONS.OPE_FIN_ID@,@OPERATIONS.OPE_FIN_DFIN_ID@, "
					+ "@OPERATIONS.OPE_REALDURATION@,@OPERATIONS.OPE_VALUE_IN_RETURN@,@OPERATIONS.OPE_OP_ONLINE@,@OPERATIONS.OPE_TICKETNUM@)";

				d.ExecuteNonQuery (ssql, con, tran,
					nNewOperationsDB, 
					dopeid, 
					(opeopeid==-1 ? DBNull.Value : (object)opeopeid), 
					(artid==-1 ? DBNull.Value : (object)artid), 
					grpid, 
					uniid, 
					(dpayid==-1 ? DBNull.Value : (object)dpayid), 
					(mov==DateTime.MinValue ? DBNull.Value : (object)mov), 
					(ini==DateTime.MinValue ? DBNull.Value : (object)ini),
					(end==DateTime.MinValue ? DBNull.Value : (object)end), 
					(duration==-1 ? DBNull.Value : (object)duration), 
					(quantity==-1 ? DBNull.Value : (object)quantity), 
					(vehicleid==null ? DBNull.Value : (object)vehicleid),
					(dartid == -1 ? DBNull.Value : (object)dartid),
					(mobileUserId == -1 ? DBNull.Value : (object)mobileUserId),
					(postpay == -1 ? DBNull.Value : (object)postpay),
					(ulChipCardId ==  0 ? DBNull.Value : (object)ulChipCardId),
					(dChipCardCredit == -1 ? DBNull.Value : (object)dChipCardCredit),
					(lFineNumber == -1 ? DBNull.Value : (object)lFineNumber),
					(lFineType == -1 ? DBNull.Value : (object)lFineType),
					(iRealDuration == -1 ? DBNull.Value : (object)iRealDuration), 
					(quantityReturned == -1 ? DBNull.Value : (object)quantityReturned),
					(iOpOnLine == -1 ? DBNull.Value : (object)iOpOnLine),
					(iTicketNumber == -1 ? DBNull.Value : (object)iTicketNumber));

				nRdo = 0;
			}
			catch (Exception e)
			{
				nRdo = -1;
				// OOOOppps.... some error... do a rollback 
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				//m.ReleaseMutex();
			}

			return nRdo;
		}

		public DataTable GetAllData (string[] fields, string where, string orderby, object[] values) 
		{
			Database d = DatabaseFactory.GetDatabase ();
			if (fields == null) 
			{
										
				fields = new string[] {"OPE_ID", "OPE_DOPE_ID",	"OPE_OPE_ID",
										  "OPE_ART_ID", "OPE_GRP_ID", "OPE_UNI_ID", "OPE_DPAY_ID", 
										  "OPE_MOVDATE", "OPE_INIDATE" , "OPE_ENDDATE" , "OPE_DURATION", 
										  "OPE_VALUE", "OPE_VEHICLEID"};
			}
			string[] pk = new string[] {"OPE_ID"};
			StringBuilder sb = base.ProcessFields(fields,pk);
			return base.DoGetData (sb.ToString(),where, orderby,values,"OPERATIONS","OPERATIONS",pk);
		}

		public DataTable GetAllData (string[] fields, string where, object[] values) 
		{
			Database d = DatabaseFactory.GetDatabase ();
			if (fields == null) 
			{
										
				fields = new string[] {"OPE_ID", "OPE_DOPE_ID",	"OPE_OPE_ID",
										  "OPE_ART_ID", "OPE_GRP_ID", "OPE_UNI_ID", "OPE_DPAY_ID", 
										  "OPE_MOVDATE", "OPE_INIDATE" , "OPE_ENDDATE" , "OPE_DURATION", 
										  "OPE_VALUE", "OPE_VEHICLEID"};
			}
			string[] pk = new string[] {"OPE_ID"};
			StringBuilder sb = base.ProcessFields(fields, pk);
			return base.DoGetData (sb.ToString(), where, null, values, "OPERATIONS", "OPERATIONS", pk);
		}

		/// <summary>
		/// Verifies if an operation exists in the OPERATIONS table.
		/// </summary>
		/// <param name="dopeid">OPE_DOPE_ID value. -1 for NULL</param>
		/// <param name="opeopeid">OPE_OPE_ID value. -1 for NULL</param>
		/// <param name="artid">OPE_ART_ID value. -1 for NULL</param>
		/// <param name="grpid">OPE_GRP_ID value. -1 for NULL</param>
		/// <param name="uniid">OPE_UNI_ID value. -1 for NULL</param>
		/// <param name="dpayid">OPE_DPAY_ID value. -1 for NULL</param>
		/// <param name="mov">OPE_MOVDATE value. DateTime.MinValue for NULL</param>
		/// <param name="ini">OPE_INIDATE value. DateTime.MinValue for NULL</param>
		/// <param name="end">OPE_ENDDATE value. DateTime.MinValue for NULL</param>
		/// <param name="duration">OPE_DURATION value. -1 for NULL</param>
		/// <param name="quantity">OPE_VALUE value. -1 for NULL</param>
		/// <param name="vehicleid">OPE_VEHICLEID value. May be NULL</param>
		public bool ExistsOperation (int dopeid, int opeopeid, int artid, int grpid, int uniid, int dpayid, DateTime mov,
			DateTime ini, DateTime end, int duration, double quantity, string vehicleid)
		{
			string sql = "SELECT COUNT(*) FROM OPERATIONS "
				+ "WHERE " + BuildCondition("OPERATIONS", "OPE_DOPE_ID", dopeid==-1)
				+ "AND " + BuildCondition("OPERATIONS", "OPE_OPE_ID", opeopeid==-1)
				+ "AND " + BuildCondition("OPERATIONS", "OPE_ART_ID", artid==-1)
				+ "AND " + BuildCondition("OPERATIONS", "OPE_GRP_ID", grpid==-1)
				+ "AND " + BuildCondition("OPERATIONS", "OPE_UNI_ID", uniid==-1)
				+ "AND " + BuildCondition("OPERATIONS", "OPE_DPAY_ID", dpayid==-1)
				+ "AND " + BuildCondition("OPERATIONS", "OPE_MOVDATE", mov==DateTime.MinValue)
				+ "AND " + BuildCondition("OPERATIONS", "OPE_INIDATE", ini==DateTime.MinValue)
				+ "AND " + BuildCondition("OPERATIONS", "OPE_ENDDATE", end==DateTime.MinValue)
				+ "AND " + BuildCondition("OPERATIONS", "OPE_DURATION", duration==-1)
				+ "AND " + BuildCondition("OPERATIONS", "OPE_VALUE", quantity==-1)
				+ "AND " + BuildCondition("OPERATIONS", "OPE_VEHICLEID", vehicleid==null);

			Array values = Array.CreateInstance(typeof(object), 0);
			values = BuildValue(values, dopeid, dopeid==-1);
			values = BuildValue(values, opeopeid, opeopeid==-1);
			values = BuildValue(values, artid, artid==-1);
			values = BuildValue(values, grpid, grpid==-1);
			values = BuildValue(values, uniid, uniid==-1);
			values = BuildValue(values, dpayid, dpayid==-1);
			values = BuildValue(values, mov, mov==DateTime.MinValue);
			values = BuildValue(values, ini, ini==DateTime.MinValue);
			values = BuildValue(values, end, end==DateTime.MinValue);
			values = BuildValue(values, duration, duration==-1);
			values = BuildValue(values, quantity, quantity==-1);
			values = BuildValue(values, vehicleid, vehicleid==null);

			Database d = null;
			d = DatabaseFactory.GetDatabase();

			if( d == null )
			{
				throw new Exception("CmpOperationsDB:ExistsOperation:Pointer to DataBase NULL");
			}
			return Convert.ToInt32(d.ExecuteScalar(sql, (object[])values)) > 0;
		}

		private string BuildCondition(string table, string field, bool isNull)
		{
			return " (" + field + " " + (isNull ? "IS NULL) " : "= @" + table + "." + field + "@) ");
		}

		private Array BuildValue(Array a, object o, bool isNull)
		{
			if (isNull)
				return a;
			
			Array valuesTemp = Array.CreateInstance(typeof(object), a.Length + 1);
			a.CopyTo(valuesTemp, 0);
			valuesTemp.SetValue(o, valuesTemp.Length-1);
			return valuesTemp;
		}

		#endregion
	}
}

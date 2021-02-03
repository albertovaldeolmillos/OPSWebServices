using System;
using System.Data;
using System.Collections;
using System.Configuration;
using OPS.Comm;
using Oracle.ManagedDataAccess.Client;
//using Oracle.DataAccess.Client;

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ALARMS.
	/// </summary>
	public class CmpAlarmsDB : CmpGenericBase
	{
		//private static System.Threading.Mutex m = new System.Threading.Mutex();

		public CmpAlarmsDB()
		{
			_standardFields		= new string[] { "ALA_ID", "ALA_DALA_ID", "ALA_UNI_ID", "ALA_INIDATE" };
			_standardPks		= new string[] { "ALA_ID" };
			_standardTableName	= "ALARMS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}

		#region Implementation of ICmpDataSourceAdapter
//		// ********************* Implementation of ICmpDataSourceAdapter ***************************
//
//		public override DataTable GetData (string[] fields, string where, string orderby, object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"ALA_ID","ALA_DALA_ID","ALA_UNI_ID","ALA_INIDATE"};
//			}
//			string[] pk = new string[] {"ALA_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby, values, "ALARMS","ALARMS",pk);
//		}
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			if (fields == null) fields = new string[] {"ALA_ID", "ALA_DALA_ID", "ALA_UNI_ID", "ALA_INIDATE"};
//			string[] pk = new string[] {"ALA_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"ALARMS","ALARMS",pk);
//		}
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO ALARMS (ALA_ID, ALA_DALA_ID, ALA_INIDATE, ALA_UNI_ID) VALUES (@ALARMS.ALA_ID@, @ALARMS.ALA_DALA_ID@,  @ALARMS.ALA_INIDATE@, @ALARMS.ALA_UNI_ID@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE ALARMS SET ALA_ID = @ALARMS.ALA_ID@, ALA_DALA_ID = @ALARMS.ALA_DALA_ID@, ALA_INIDATE = @ALARMS.ALA_INIDATE@, ALA_UNI_ID= @ALARMS.ALA_UNI_ID@ WHERE ALA_ID = #ALARMS.ALA_ID# AND ALA_DALA_ID = #ALARMS.ALA_DALA_ID#", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM ALARMS WHERE ALA_ID = @ALARMS.ALA_ID@ AND ALA_DALA_ID = @ALARMS.ALA_DALA_ID@", false);
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
//			GetForeignData(ds,sTable,null);
//
//		}
//
//
//		public override void GetForeignData(DataSet ds, string sTable, OTS.Framework.Globalization.IResourceManager rm) 
//		{
//			// Add the table of languages to the DataSet
//			string[] fields = new string[] {"DALA_ID","DALA_LIT_ID"};
//			DataTable dtAlarmsDef = new CmpAlarmsDefDB().GetData(fields);
//			if (rm!=null) 
//			{
//				rm.GlobalizeTable(dtAlarmsDef, "DALA_LIT_ID", "DALA_DESC", true);
//			}
//			
//			ds.Tables.Add (dtAlarmsDef);
//			DataTable parent = ds.Tables[sTable];
//			ds.Relations.Add ((dtAlarmsDef.PrimaryKey)[0],parent.Columns["ALA_DALA_ID"]);
//
//			string[] sgroups = new string[2];
//			sgroups[0]	= "UNI_ID";
//			sgroups[1]	= "UNI_DESCSHORT";
//
//			// Add the table of UNITS to the DataSet
//			DataTable dtUnits = new CmpUnitsDB().GetData(sgroups,null,null,null);
//			ds.Tables.Add (dtUnits);
//			ds.Relations.Add ((dtUnits.PrimaryKey)[0],parent.Columns["ALA_UNI_ID"]);
//		}
//
//		public override string MainTable  {get { return "ALARMS"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM ALARMS";
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
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(ALA_ID),0) FROM ALARMS"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
		#endregion

		#region Extra methods accessible only through a CmpAlarmsDB reference
		/// <summary>
		/// Inserts a new Alarm into the alarms table
		/// </summary>
		/// <param name="type">Type of alarm (FK to ALA_DALA_ID)</param>
		/// <param name="device">ID of the devide (FK to UNITS)</param>
		public void InsertAlarm (int type, int device,DateTime date)
		{

			Database d = DatabaseFactory.GetDatabase();
			long id = GetNewAlaPk();
			string ssql = "INSERT INTO ALARMS (ALA_ID, ALA_DALA_ID, ALA_UNI_ID, ALA_INIDATE) VALUES (";
			ssql = ssql + "@ALARMS.ALA_ID@, @ALARMS.ALA_DALA_ID@, @ALARMS.ALA_UNI_ID@, @ALARMS.ALA_INIDATE@)";
			d.ExecuteNonQuery (ssql, id, type, device, date);
		}

		/// <summary>
		/// Deletes an alarm from the DataBase
		/// The alarm is NOT deleted from the table: the field ALA_ENDDATE is set to NOT NULL
		/// </summary>
		/// <param name="id">Id of the alarm (PK)</param>
//		public void DeleteAlarm (long id)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string ssql = "UPDATE ALARMS SET ALA_ENDDATE = @ALARMS.ALA_ENDDATE@ WHERE ALA_ID=@ALARMS.ALA_ID@";
//			d.ExecuteNonQuery (ssql, DateTime.Now, id);
//		}

		public DataTable GetDevicesWithAlarms()
		{
			Database d = DatabaseFactory.GetDatabase();
			string ssql = "select ALA_UNI_ID from ALARMS group by (ALA_UNI_ID)";
			return d.FillDataTable (ssql, "DEVICES_ALARMS");
		}


		/// <summary>
		/// Get active alarms from Unit Id
		/// </summary>
		/// <param name="iUnitId">Unit Id</param>
		/// <returns>Hashtable with alarmas</returns>
		public bool GetAlarmsFromUnit(int iUnitId, out Hashtable htAlarms, out DateTime dtMaxDate )
		{

			bool bRet=true;
			htAlarms=null;
			dtMaxDate = DateTime.MinValue;
			Database d = DatabaseFactory.GetDatabase();
			IDbConnection con = null;
			OracleDataReader dr = null;
			try 
			{
				htAlarms = new Hashtable();
				con =  d.GetNewConnection();
				con.Open();				
				OracleCommand cmd = new OracleCommand("select ALA_ID,ALA_DALA_ID,TO_CHAR(ALA_INIDATE,'hh24missddmmyy') ALA_INIDATE from ALARMS where ALA_UNI_ID = " + 
					iUnitId + " order by ALA_DALA_ID ASC");
				cmd.Connection =(OracleConnection)con;
				dr = cmd.ExecuteReader();
				while (dr.Read()) 
				{
					string sALA_ID		= dr["ALA_ID"].ToString();
					string sALA_DALA_ID = dr["ALA_DALA_ID"].ToString();
					DateTime dtALA_INIDATE = OPS.Comm.Dtx.StringToDtx(dr["ALA_INIDATE"].ToString());
					htAlarms.Add( sALA_ID, sALA_DALA_ID);
					
					if (dtALA_INIDATE>dtMaxDate)
					{
						dtMaxDate=dtALA_INIDATE;
					}					
				}				

			}
			catch (Exception)
			{
				bRet=false;
			}
			finally 
			{
				try 
				{ 
					con.Close(); 
					con.Dispose(); 
					dr.Close();
					dr.Dispose();
				} 
				catch (Exception) 
				{ 
				}
			}

			return bRet;





		}

		/// <summary>
		/// Processes the alarms for the unit specified
		/// </summary>
		/// <param name="alarmsToInsert">Alarms to insert in ALARMS table</param>
		/// <param name="alarmsToDelete">Alarms to move from the ALARMS table to ALARMS_HIS table</param>
		/// <param name="uniid">ID of the unit</param>
		/// <returns>true if succesful, false if some error (no operation is done if false is returned)</returns>
		public bool ProcessAlarms (ArrayList alarmsToInsert, ArrayList alarmsToDelete, int uniid, DateTime date)
		{

			// CFE --- Hago una llamada a generica para verificar el select correcto.
			//Hashtable tableSchema = CFETest_GetAlarmsFromUnit( uniid );


			Database d = DatabaseFactory.GetDatabase();
			IDbConnection con = null;
			try 
			{
				con =  d.GetNewConnection();
				con.Open();
				InsertAlarms(alarmsToInsert, uniid, con,date);
				DeleteAlarms(alarmsToDelete, uniid, con,date);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally 
			{
				try { con.Close(); con.Dispose(); } 
				catch (Exception) { }
			}
		}
		#endregion

		#region Protected helper methods
		/// <summary>
		/// Delete ALL alarms contained in arraylist alarms (for the UNIT specified)
		/// </summary>
		/// <param name="alarms">ArrayList of ints (alarms ==> ALA_DALA_ID)</param>
		/// <param name="uniid">UNI_ID of the unit</param>
		/// <param name="con">Connection to use</param>
		/// <param name="tran">Transaction context to use</param>
		/// <returns>true if succesful, false if error</returns>
		protected void DeleteAlarms (ArrayList alarms, int uniid, IDbConnection con, DateTime date)
		{
			Database d = DatabaseFactory.GetDatabase();
			IDbDataAdapter da = d.GetDataAdapter ();
			try 
			{
				//m.WaitOne();
				// Get all the current alarms
				DataTable dtAlarms = d.FillDataTable(
					"SELECT ALA_ID, ALA_DALA_ID, ALA_UNI_ID, ALA_INIDATE "
					+ "FROM ALARMS WHERE ALA_UNI_ID = @ALARMS.ALA_UNI_ID@",
					"ALARMS", uniid);
			
				// Foreach alarm check if we have to delete it  (==> set ALA_ENDDATE to Now)
				foreach (DataRow dr in  dtAlarms.Rows)
				{
					// cfe -- evito que el error en el procesamiento de una alarma afecte al resto
					try
					{
						int ialarm = Convert.ToInt32(dr["ALA_DALA_ID"]);
						if (alarms.Contains (ialarm))
						{
							
							// Insert the alarm into alarm_his...
							InsertAlarmHistory(dr, con,date);
							// ... and delete it from DataTable
							dr.Delete();
						}
					}
					catch(Exception)
					{
					
					}
				}
				// Update the database
				da.DeleteCommand = d.PrepareCommand ("DELETE FROM ALARMS WHERE ALA_ID = @ALARMS.ALA_ID@", false);
				da.DeleteCommand.Connection = con;
				d.UpdateDataSet (da, dtAlarms);
				dtAlarms.AcceptChanges();
			}
			finally 
			{
				//m.ReleaseMutex();
			}
		}

		/// <summary>
		/// Insert a ALARM into ALARMS_HIS table
		/// </summary>
		/// <param name="dr">DataRow with Alarm info</param>
		/// <param name="con">Connection to use</param>
		/// <param name="tran">Transaction context to use</param>

		protected void InsertAlarmHistory (DataRow dr, IDbConnection con,DateTime date)
		{
			Database d = DatabaseFactory.GetDatabase();
			d.ExecuteNonQuery ("INSERT INTO ALARMS_HIS (HALA_ID, HALA_DALA_ID,HALA_UNI_ID, HALA_INIDATE, HALA_ENDDATE) VALUES (@ALARMS_HIS.HALA_ID@, @ALARMS_HIS.HALA_DALA_ID@,@ALARMS_HIS.HALA_UNI_ID@, @ALARMS_HIS.HALA_INIDATE@, @ALARMS_HIS.HALA_ENDDATE@)",con,
				GetNewAlaHisPk(), dr["ALA_DALA_ID"], dr["ALA_UNI_ID"], dr["ALA_INIDATE"], date);
		}

		/// <summary>
		/// Retrieves a new PK for ALARMS_HIS.
		/// That method is not intrinsic thread-safe, so should be called in a thread-safe scenario...
		/// </summary>
		/// <returns>A new valid PK for ALARMS_HIS</returns>
		protected int GetNewAlaHisPk()
		{
			Database d = DatabaseFactory.GetDatabase();
			int id = Convert.ToInt32(d.ExecuteScalar("select SEQ_ALARMS_HIS.NEXTVAL FROM DUAL"));
			return id;
		}

		protected int GetNewAlaPk()
		{
			Database d = DatabaseFactory.GetDatabase();
			int id = Convert.ToInt32(d.ExecuteScalar("select SEQ_ALARMS.NEXTVAL FROM DUAL"));
			return id;
		}
		/// <summary>
		/// Insert a set of alarms of a unit.
		/// Do it inside a transaction
		/// </summary>
		/// <param name="alarms">Array of DALA_ID (alarms) to set</param>
		/// <param name="uniid">Unit which set that alarms</param>
		/// <param name="con">Connection object to use</param>
		/// <param name="tran">Transaction context to use</param>
		/// <returns></returns>
		protected void InsertAlarms (ArrayList alarms, int uniid, IDbConnection con, DateTime date)
		{
			Database d = DatabaseFactory.GetDatabase();
			//m.WaitOne();
			try 
			{
				string ssql = "INSERT INTO ALARMS (ALA_ID, ALA_DALA_ID, ALA_UNI_ID, ALA_INIDATE) "
					+ "VALUES (@ALARMS.ALA_ID@, @ALARMS.ALA_DALA_ID@, @ALARMS.ALA_UNI_ID@, @ALARMS.ALA_INIDATE@)";
				object[] values = new object[5];
				values[2] = uniid;
				
				AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				double nDifHour=0;
				try
				{
					nDifHour= (double) appSettings.GetValue   ("HOUR_DIFFERENCE",typeof(double));
				}
				catch
				{
					nDifHour=0;
				}				

				foreach (object alarm in alarms)
				{
					values[0] = GetNewAlaPk();
					values[1] = (int)alarm;
					values[3] = date;
					// cfe -- evito que el error en una alarma impida el funcionamiento del resto
					try
					{
						d.ExecuteNonQuery (ssql, con, values);
					}
					catch(Exception)
					{
					}

				}
			}
			finally 
			{
				//m.ReleaseMutex();
			}
		}
		#endregion
	}
}

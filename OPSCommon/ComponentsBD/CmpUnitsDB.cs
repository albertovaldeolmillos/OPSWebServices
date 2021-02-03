using System;
using System.Data;

using OPS.Comm;
using System.Configuration;

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for UNITS.
	/// </summary>
	public class CmpUnitsDB : CmpGenericBase
	{
		public CmpUnitsDB() 
		{
			_standardFields		= new string[] {"UNI_ID", "UNI_DPUNI_ID", "UNI_DESCSHORT", "UNI_DESCLONG", "UNI_POSX", "UNI_POSY", "UNI_MAXSEATS", "UNI_STR_ID", "UNI_IP", "UNI_APPVERSION", "UNI_REPVERSION", "UNI_DATE", "UNI_DSTA_ID", "UNI_DSTA_ID_FIRST_DATE", "UNI_MAIL_SENT"};
			_standardPks		= new string[] {"UNI_ID"};
			_standardTableName	= "UNITS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
	
			_standardRelationFileds	= new string[0]; //{"UNI_STR_ID","UNI_DPUNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpStreetsDB,ComponentsBD","OPS.Components.Data.CmpUnitsPhyDefDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"UNI_VALID", "UNI_DELETED"};
		}

		public int GetIPFromUnit(int idUnit,ref string szIP)
		{
			int nRes = -1;
			szIP = "";
			Database d = DatabaseFactory.GetDatabase();
			IDbConnection con = d.GetNewConnection();
			con.Open();

			string ssql = "SELECT  UNI_IP FROM UNITS "
				+ "WHERE UNI_ID = @UNITS.UNI_ID@";

			DataTable dtUnit = d.FillDataTable(ssql,"UNITS",idUnit);

			foreach (DataRow dr in  dtUnit.Rows)
			{
				szIP = Convert.ToString(dr["UNI_IP"]);
				nRes++;
			}

			con.Close();
			
			return nRes;
		}

		public long GetMinutesFromLastUpdateUnit( int idUnit )
		{
			Database d = DatabaseFactory.GetDatabase();
			IDbConnection con = d.GetNewConnection();
			con.Open();

			string ssql = "SELECT TRUNC(NVL(SYSDATE - UNI_DATE, -1)*24*60) FROM UNITS "
				+ "WHERE UNI_ID = @UNITS.UNI_ID@";

			long nRes = (long) d.ExecuteScalar(ssql, idUnit);


			con.Close();
			
			return nRes;
		}

		public void UpdateStatus(int id, int status)
		{
			Database d = DatabaseFactory.GetDatabase();
			IDbConnection con = d.GetNewConnection();
			con.Open();
			string ssql = "UPDATE UNITS SET UNI_DSTA_ID = @UNITS.UNI_DSTA_ID@ "
				+ "WHERE UNI_ID = @UNITS.UNI_ID@";
			d.ExecuteNonQuery(ssql, con, status, id);  
			con.Close();
		}
		// Updates the to Unit IP 
		// TABLE UNITS
		// FIELD UNI_IP

		public int UpdateIP(int id, string szIPD)
		{
			Database d = null;
			ILogger localLogger = null;
			IDbTransaction tran = null;
			IDbConnection con = null;
			int res = -1;

			try
			{
				//UPDATE  UNITS SET UNI_IP ='192.168.1.100' WHERE UNI_ID = 10001
				
				d = DatabaseFactory.GetDatabase();
				localLogger = DatabaseFactory.Logger;
				if(localLogger != null)
					localLogger.AddLog("[CmpUnitsDB]: Getting Connection...", LoggerSeverities.Debug);

				con = d.GetNewConnection();

				if(localLogger != null)
					localLogger.AddLog("[CmpUnitsDB]: Opening...", LoggerSeverities.Debug);

				con.Open();
				
				
				tran =  con.BeginTransaction(IsolationLevel.Serializable);
				
				string ssql = "UPDATE  UNITS SET UNI_IP = @UNITS.UNI_IP@, UNI_DATE = @UNITS.UNI_DATE@ "
					+ "WHERE UNI_ID = @UNITS.UNI_ID@";

				if(localLogger != null)
					localLogger.AddLog("[CmpUnitsDB]: Executing...", LoggerSeverities.Debug);

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

				res = d.ExecuteNonQuery(ssql, con,tran,  szIPD, DateTime.Now.AddHours(nDifHour).ToString(), id);  
				if(res ==1)
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpUnitsDB]: Commit", LoggerSeverities.Debug);
					tran.Commit();
				}
				else
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpUnitsDB]: RollBack", LoggerSeverities.Debug);
					tran.Rollback();
				}
						
			}
			catch (Exception e)
			{
				// OOOOppps.... some error... do a rollback 
				tran.Rollback();
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpUnitsDB]: Closing...", LoggerSeverities.Debug);
					con.Close();
				}  
				catch (Exception) { }
				//m.ReleaseMutex();
			}
			return res;
		}
	}
}


using System;
using System.Data;

using OPS.Comm;



namespace OPS.Components.Data
{
	/// <summary>
	/// 
	/// </summary>
	public class CmpCommandsDB : CmpGenericBase
	{
		#region CONSTANTS 
		/// <summary>
		///  CONSTANTS FOR IDENTIFY MESSAGES
		/// </summary>
		public const int C_COLLECTING_M20	= 20;
		public const int C_GETSTATE_M21		= 21;
		public const int C_RESET_M22		= 22;
		public const int C_OUTMESSAGE_M24	= 24;
		public const int C_GETVERSION_M26	= 26;
		public const int C_GETHOUR_M27		= 27;
		public const int C_SETHOUR_M28		= 28;
		public const int C_SETSTATE_M29		= 29;
		public const int C_INITFTP_M30		= 30;

		/// <summary>
		/// CONSTANTS FOR STATUS FOR COMMANDS
		/// </summary>
		 
		public const int C_INSERTED			= 1;
		public const int C_EXECUTING		= 2;
		public const int C_RES_OK			= 3;
		public const int C_RES_NOK			= 4;
		public const int C_RES_PROCESSED	= 5;

		#endregion


		public CmpCommandsDB()
		{
			_standardFields		= new string[] {"CMD_ID", "CMD_UNI_ID", "CMD_PARAM", 
												   "CMD_IP", "CMD_STATUS", "CMD_RESULT", "CMD_DATE_INSERT", 
												   "CMD_COMMAND_ID"};
			_standardPks		= new string[] {"CMD_ID"};
			_standardTableName	= "COMMANDS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
	
			_standardRelationFileds	= new string[0]; 
			_standardRelationTables	= new string[0]; 
			_stValidDeleted			= new string[0]; 
		}

		/// <summary>
		/// Insert a Command in the DataBase
		/// </summary>
		/// <param name="idCmdDef"></param>
		/// <param name="idUnit"></param>
		/// <returns></returns>
		public int InsertCommand(int idCmdDef,int idUnit,int idCC,string szIP, string szCommand)
		{
			Database d = null;
			ILogger localLogger = null;
			IDbTransaction tran = null;
			IDbConnection con = null;
			int res = -1;
			try
			{	
				d = DatabaseFactory.GetDatabase();
				localLogger = DatabaseFactory.Logger;

				if(localLogger != null)
					localLogger.AddLog("[CmpCommandsDB::InsertCommand]: Getting Connection...", LoggerSeverities.Debug);

				con = d.GetNewConnection();

				if(localLogger != null)
					localLogger.AddLog("[CmpCommandsDB::InsertCommand]: IOpening...", LoggerSeverities.Debug);

				con.Open();
				
				int cmdID =  Convert.ToInt32(d.ExecuteScalar("SELECT NVL(MAX(CMD_ID),0)+1 FROM COMMANDS", new object[] {}));
				
				tran =  con.BeginTransaction(IsolationLevel.Serializable);
				
				string ssql = "INSERT INTO COMMANDS(CMD_ID,CMD_UNI_ID,CMD_PARAM,CMD_IP,CMD_STATUS,CMD_RESULT,CMD_DATE_INSERT,CMD_COMMAND_ID) VALUES ";
				ssql = ssql + "(@COMMANDS.CMD_ID@,@COMMANDS.CMD_UNI_ID@,@COMMANDS.CMD_PARAM@,@COMMANDS.CMD_IP@,1,'INSERTED',SYSDATE,@COMMANDS.CMD_COMMAND_ID@)";

				if(localLogger != null)
					localLogger.AddLog("[CmpCommandsDB::InsertCommand]: Executing...", LoggerSeverities.Debug);

				
				res = d.ExecuteNonQuery(ssql, con,tran, cmdID,idUnit,szCommand,szIP,idCmdDef);  
				if(res ==1)
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCommandsDB::InsertCommand]: Commit", LoggerSeverities.Debug);
					tran.Commit();
				}
				else
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCommandsDB::InsertCommand]: RollBack", LoggerSeverities.Debug);
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
						localLogger.AddLog("[CmpCommandsDB::InsertCommand]: Closing...", LoggerSeverities.Debug);
					con.Close();
				}  
				catch (Exception) 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCommandsDB::InsertCommand]: finally", LoggerSeverities.Debug);
				}
				
			}
			return res;
		}

	



		/// <summary>
		/// Get the Status from table Commands
		/// </summary>
		/// <param name="idUnit"></param>
		/// <returns></returns>
		public int GetStatusCmdFromUnit(int idUnit,int idCommandDef,ref string szResult,ref int nStatus,ref DateTime dtTime,ref int nCmdID)
		{
			int nRes = -1;
			IDbConnection con = null;
			ILogger localLogger = null;
			
			try
			{
				localLogger = DatabaseFactory.Logger;
				szResult = "";
				nStatus = -1;
				dtTime = DateTime.MinValue;
				
				// Getting Databasse, open connection 
				Database d = DatabaseFactory.GetDatabase();
				con = d.GetNewConnection();
				con.Open();
				
				string ssql = "SELECT  CMD_RESULT,CMD_STATUS,CMD_DATE_RESULT,CMD_ID FROM COMMANDS "
					+ "WHERE CMD_ID = "
					+ "(SELECT NVL(MAX(CMD_ID),0) FROM COMMANDS WHERE CMD_COMMAND_ID = @COMMANDS.CMD_COMMAND_ID@)"
					+ "AND CMD_UNI_ID = @COMMANDS.CMD_UNI_ID@";
				
 

				DataTable dtUnit = d.FillDataTable(ssql,"COMMANDS",idCommandDef,idUnit);

				foreach (DataRow dr in  dtUnit.Rows)
				{
					if(!(dr["CMD_RESULT"] == DBNull.Value))
						szResult = Convert.ToString(dr["CMD_RESULT"]);
					if(!(dr["CMD_STATUS"]  == DBNull.Value))
						nStatus = Convert.ToInt32(dr["CMD_STATUS"]);
					if(!(dr["CMD_DATE_RESULT"]  == DBNull.Value))
						dtTime = Convert.ToDateTime(dr["CMD_DATE_RESULT"]);
					if(!(dr["CMD_ID"] == DBNull.Value))
						nCmdID = Convert.ToInt32(dr["CMD_ID"]);

					nRes++;
				}

				con.Close();
			}
			catch (Exception e)
			{
				// Some error... 
				if(localLogger != null)
					localLogger.AddLog("[CmpCommandsDB::InsertCommand]: Closing...", LoggerSeverities.Debug);
				con.Close();
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... 
				try 
				{
					con.Close();
				}  
				catch (Exception)
				{ 
				
				}
				
			}
			return nRes;
			
		}


		public int UpdateStatus(int idCmd, int nStatus)
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
					localLogger.AddLog("[CmpCommandsDB]: Getting Connection...", LoggerSeverities.Debug);

				con = d.GetNewConnection();

				if(localLogger != null)
					localLogger.AddLog("[CmpCommandsDB]: Opening...", LoggerSeverities.Debug);

				con.Open();
				
				
				tran =  con.BeginTransaction(IsolationLevel.Serializable);
				
				/*
				//
					szSQL.Format( _T("UPDATE COMMANDS SET CMD_STATUS = %d ,CMD_RESULT='%s',CMD_DATE_RESULT=TO_DATE('%s','DD/MM/YY HH24:MI:SS') ")
			_T(" WHERE CMD_ID=%d"),C_FINALIZADOOK, pCommandDB->m_szResult,szDateWork,pCommandDB->m_nCommandID );

				*/
				string ssql = "UPDATE  COMMANDS SET CMD_STATUS = @COMMANDS.CMD_STATUS@ "
					+ " WHERE CMD_ID = @COMMANDS.CMD_ID@";

				if(localLogger != null)
					localLogger.AddLog("[CmpCommandsDB]: Executing...", LoggerSeverities.Debug);

				res = d.ExecuteNonQuery(ssql, con,tran,  nStatus,idCmd );  
				if(res ==1)
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCommandsDB]: Commit", LoggerSeverities.Debug);
					tran.Commit();
				}
				else
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCommandsDB]: RollBack", LoggerSeverities.Debug);
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
						localLogger.AddLog("[CmpCommandsDB]: Closing...", LoggerSeverities.Debug);
					con.Close();
				}  
				catch (Exception) { }
				//m.ReleaseMutex();
			}
			return res;
		}

	}
}

using System;
using System.Data;

using OPS.Comm;
using System.Configuration;


namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for CmpClockInDB.
	/// </summary>
	public class CmpClockInDB : CmpGenericBase
	{
		public CmpClockInDB()
		{
			
			_standardFields			= new string[] {	"CI_ID",    
													   "CI_USR_ID",
													   "CI_UNI_ID", 
													   "CI_DATE"  };
			_standardPks			= new string[] { "CI_ID" };
			_standardTableName		= "CLOCK_IN";
			_standardOrderByField	= "CI_DATE";
			_standardOrderByAsc		= "DESC";
	
			_standardRelationFileds	= new string[0]; 
			_standardRelationTables	= new string[0]; 
			_stValidDeleted			= new string[0]; 
		}

		/// <summary>
		///
		/// </summary>
		/// <param name=""></param>
		/// <returns></returns>
		public int Insert (int iUserID, int iUniID, DateTime dtDate)
		{
			Database d = null;					// Database
			ILogger localLogger = null;			// Logger to trace
			IDbTransaction tran = null;			// Transacition
			IDbConnection con = null;			// Connection
			int res = -1;

			try
			{
				// Getting Database
				d = DatabaseFactory.GetDatabase();
				if( d == null )
					return  -1;

				// Getting Logger
				localLogger = DatabaseFactory.Logger;
				if(localLogger != null)
					localLogger.AddLog("[CmpClockInDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = d.GetNewConnection();
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpClockInDB  ]: Opening...", LoggerSeverities.Debug);

				//  Open connection
				con.Open();
				
				tran =  con.BeginTransaction(IsolationLevel.Serializable);

				if( tran == null)
					return -3;
 
				int iNumRegs = Convert.ToInt32(d.ExecuteScalar(string.Format("select count(*) from clock_in where ci_usr_id={0} and ci_date=to_date('{1}','hh24missddmmyy')",iUserID,OPS.Comm.Dtx.DtxToString(dtDate)), con, tran));
				
				if (iNumRegs==0)
				{

					string ssql = " INSERT INTO CLOCK_IN (CI_USR_ID, CI_UNI_ID, CI_DATE) " +
						" VALUES (@CLOCK_IN.CI_USR_ID@,@CLOCK_IN.CI_UNI_ID@,@CLOCK_IN.CI_DATE@) ";

					
					if(localLogger != null)
					{
						localLogger.AddLog("[CmpClockInDB  ]: Executing...", LoggerSeverities.Debug);
						localLogger.AddLog("[CmpClockInDB  ]" + ssql, LoggerSeverities.Debug);

					}
					
					res = d.ExecuteNonQuery(ssql, con, tran, iUserID, iUniID, dtDate);  
					if(res ==1)
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpClockInDB  ]: Commit", LoggerSeverities.Debug);
						tran.Commit();
					}
					else
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpClockInDB  ]: RollBack", LoggerSeverities.Debug);
						tran.Rollback();
					}
				}
				else
				{
					res=1;
				}
						
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpClockInDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				if( tran != null)
					tran.Rollback();
				throw e;			// Propagate the error back!
			}
			finally 
			{
							// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpClockInDB  ]: Closing...", LoggerSeverities.Debug);
					if( con!= null )
						con.Close();
				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpClockInDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
				
			}
			return res;
 
}
	}
}

using System;
using System.Data;
using System.Collections;

using System.Text;
using OPS.Comm;

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for PDM_MESSAGES_STAT.
	/// </summary>
	public class CmpPDMMessagesStatDB: CmpGenericBase
	{
		public int _iStatisticsCount = 5;
		public int _fStatisticsCount = 2;

		public CmpPDMMessagesStatDB()
		{
			_standardFields		= new string[] { "PMS_ID", "PMS_DATE", "PMS_UNIT", "PMS_MESSAGE", 
												 "PMS_IVALUE1","PMS_IVALUE2", "PMS_IVALUE3", "PMS_IVALUE4", 
												 "PMS_IVALUE5","PMS_RVALUE1", "PMS_RVALUE2" };
			_standardPks		= new string[] { "PMS_ID" };
			_standardTableName	= "PDM_MESSAGES_STAT";
			_standardOrderByField	= "PMS_UNIT";
			_standardOrderByAsc		= "DESC";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="strUnitId">Unit identificator (must exists in groups table)</param>
		/// <param name="dtDateInsert">Date when the statistic was generates (not date when the statistics
		///	was sent</param>
		/// <param name="strMessage">Message to which the statistic is referring to</param>
		/// <param name="lValues">Values for the integer statistics (must be only 5)</param>
		/// <param name="fValues">Values for the real statistics (must be only 2)</param>
		/// <returns></returns>
		public int Insert ( int iUnitId, DateTime dtDateInsert, string strMessage, long[] lValues,
							float[] fValues )
		{
			Database d = null;					// Database
			ILogger localLogger = null;			// Logger to trace
			IDbTransaction tran = null;			// Transacition
			IDbConnection con = null;			// Connection
			int res = -1;

			try
			{
				// Check the parameters: TODO: Check if the date is valid
				if( strMessage.Length == 0 )
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpPDMMessagesStatDB]: Some input parameter is not valid", LoggerSeverities.Debug);
					return -1;
				}

				// Getting Database
				d = DatabaseFactory.GetDatabase();
				if( d == null )
					return  -1;

				// Getting Logger
				localLogger = DatabaseFactory.Logger;
				if(localLogger != null)
					localLogger.AddLog("[CmpPDMMessagesStatDB]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = d.GetNewConnection();
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpPDMMessagesStatDB]: Opening...", LoggerSeverities.Debug);

				//  Open connection
				con.Open();
				
				tran =  con.BeginTransaction(IsolationLevel.Serializable);

				if( tran == null)
					return -3;

				long lMax = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(PMS_ID),0) FROM PDM_MESSAGES_STAT"));
				
				string ssql =	"INSERT INTO PDM_MESSAGES_STAT (PMS_ID, PMS_DATE, PMS_UNIT, PMS_MESSAGE, "+
								"PMS_IVALUE1,PMS_IVALUE2, PMS_IVALUE3, PMS_IVALUE4, PMS_IVALUE5, PMS_RVALUE1, "+
								"PMS_RVALUE2) VALUES (@PDM_MESSAGES_STAT.PMS_DATE@,@PDM_MESSAGES_STAT.PMS_UNIT@, "+
								"@PDM_MESSAGES_STAT.PMS_MESSAGE@,@PDM_MESSAGES_STAT.PMS_IVALUE1@, " +
								"@PDM_MESSAGES_STAT.PMS_IVALUE2@ ,@PDM_MESSAGES_STAT.PMS_IVALUE3@, "+
								"@PDM_MESSAGES_STAT.PMS_IVALUE4@ ,@PDM_MESSAGES_STAT.PMS_IVALUE5@, "+
								"@PDM_MESSAGES_STAT.PMS_RVALUE1@ ,@PDM_MESSAGES_STAT.PMS_RVALUE2@) ";

				
				if(localLogger != null)
				{
					localLogger.AddLog("[CmpPDMMessagesStatDB]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpPDMMessagesStatDB]" + ssql, LoggerSeverities.Debug);
					localLogger.AddLog("[CmpPDMMessagesStatDB] Unit ID " + iUnitId.ToString(), LoggerSeverities.Debug);
					localLogger.AddLog("[CmpPDMMessagesStatDB] Message " + strMessage, LoggerSeverities.Debug);
                    localLogger.AddLog("[CmpPDMMessagesStatDB] Date " + dtDateInsert.ToString("F"), LoggerSeverities.Debug);
				}
				
				res = d.ExecuteNonQuery(ssql, con,tran, lMax, dtDateInsert, iUnitId, strMessage,
										lValues[0], lValues[1], lValues[2], lValues[3], lValues[4],
										fValues[0], fValues[1] );
				
				if(res ==1)
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpPDMMessagesStatDB]: Commit", LoggerSeverities.Debug);
					tran.Commit();
				}
				else
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpPDMMessagesStatDB]: RollBack", LoggerSeverities.Debug);
					tran.Rollback();
				}
						
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpPDMMessagesStatDB]: RollBack" + e.Message, LoggerSeverities.Debug);					
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
						localLogger.AddLog("[CmpPDMMessagesStatDB]: Closing...", LoggerSeverities.Debug);
					if( con!= null )
						con.Close();
				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpPDMMessagesStatDB]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
				
			}
			return res;
 
		}
	}
}

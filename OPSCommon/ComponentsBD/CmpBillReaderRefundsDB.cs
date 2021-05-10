using System;
using System.Data;

using OPS.Comm;
using System.Configuration;


namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for CmpBillReaderRefundsDB.
	/// </summary>
	public class CmpBillReaderRefundsDB : CmpGenericBase
	{
		public CmpBillReaderRefundsDB()
		{
			
			_standardFields			= new string[] {   "RBILL_ID",    
													   "RBILL_UNI_ID", 
													   "RBILL_DATE",
													   "RBILL_VALUE"};
			_standardPks			= new string[] { "RBILL_ID" };
			_standardTableName		= "BILLREADER_REFUNDS";
			_standardOrderByField	= "RBILL_DATE";
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
		public int Insert (int iUniID, DateTime dtDate, int iValue)
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
					localLogger.AddLog("[CmpBillReaderRefundsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = d.GetNewConnection();
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpBillReaderRefundsDB  ]: Opening...", LoggerSeverities.Debug);

				//  Open connection
				con.Open();
				
				tran =  con.BeginTransaction(IsolationLevel.Serializable);

				if( tran == null)
					return -3;
 
				int iNumRegs = Convert.ToInt32(d.ExecuteScalar(string.Format("select count(*) from BILLREADER_REFUNDS where RBILL_UNI_ID={0} and RBILL_DATE=to_date('{1}','hh24missddmmyy')",iUniID,OPS.Comm.Dtx.DtxToString(dtDate)), con, tran));
				
				if (iNumRegs==0)
				{

					string ssql = " INSERT INTO BILLREADER_REFUNDS (RBILL_UNI_ID, RBILL_DATE, RBILL_VALUE) " +
						" VALUES (@BILLREADER_REFUNDS.RBILL_UNI_ID@,@BILLREADER_REFUNDS.RBILL_DATE@,@BILLREADER_REFUNDS.RBILL_VALUE@) ";

					
					if(localLogger != null)
					{
						localLogger.AddLog("[CmpBillReaderRefundsDB  ]: Executing...", LoggerSeverities.Debug);
						localLogger.AddLog("[CmpBillReaderRefundsDB  ]" + ssql, LoggerSeverities.Debug);

					}
					
					res = d.ExecuteNonQuery(ssql, con, tran, iUniID, dtDate, iValue);  
					if(res ==1)
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpBillReaderRefundsDB  ]: Commit", LoggerSeverities.Debug);
						tran.Commit();
					}
					else
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpBillReaderRefundsDB  ]: RollBack", LoggerSeverities.Debug);
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
					localLogger.AddLog("[CmpBillReaderRefundsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
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
						localLogger.AddLog("[CmpBillReaderRefundsDB  ]: Closing...", LoggerSeverities.Debug);
					if( con!= null )
						con.Close();
				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpBillReaderRefundsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
				
			}
			return res;
 
}
	}
}

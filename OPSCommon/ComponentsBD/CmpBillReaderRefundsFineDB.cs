using System;
using System.Data;

using OPS.Comm;
using System.Configuration;


namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for CmpBillReaderRefundsDB.
	/// </summary>
	public class CmpBillReaderRefundsFineDB : CmpGenericBase
	{
		public CmpBillReaderRefundsFineDB()
		{
			
			_standardFields			= new string[] {   "RBILLF_ID",    
													   "RBILLF_UNI_ID", 
													   "RBILLF_DATE",
													   "RBILLF_VALUE",
													   "RBILLF_FIN_ID",
													   "RBILLF_DFIN_ID"};
			_standardPks			= new string[] { "RBILLF_ID" };
			_standardTableName		= "BILLREADER_REFUNDSFINE";
			_standardOrderByField	= "RBILLF_DATE";
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
		public int Insert (int iUniID, DateTime dtDate, int iValue, string sFineNumber, long lFineDef)
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
					localLogger.AddLog("[CmpBillReaderRefundsFineDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = d.GetNewConnection();
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpBillReaderRefundsFineDB  ]: Opening...", LoggerSeverities.Debug);

				//  Open connection
				con.Open();
				
				tran =  con.BeginTransaction(IsolationLevel.Serializable);

				if( tran == null)
					return -3;
 
				int iNumRegs = Convert.ToInt32(d.ExecuteScalar(string.Format("select count(*) from BILLREADER_REFUNDSFINE where RBILLF_UNI_ID={0} and RBILLF_DATE=to_date('{1}','hh24missddmmyy')",iUniID,OPS.Comm.Dtx.DtxToString(dtDate)), con, tran));
				
				if (iNumRegs==0)
				{


					string ssql = " INSERT INTO BILLREADER_REFUNDSFINE (RBILLF_UNI_ID, RBILLF_DATE, RBILLF_VALUE, RBILLF_FIN_ID, RBILLF_DFIN_ID) " +
						" VALUES (@BILLREADER_REFUNDSFINE.RBILLF_UNI_ID@,@BILLREADER_REFUNDSFINE.RBILLF_DATE@,@BILLREADER_REFUNDSFINE.RBILLF_VALUE@,@BILLREADER_REFUNDSFINE.RBILLF_FIN_ID@,@BILLREADER_REFUNDSFINE.RBILLF_DFIN_ID@) ";

					
					if(localLogger != null)
					{
						localLogger.AddLog("[CmpBillReaderRefundsFineDB  ]: Executing...", LoggerSeverities.Debug);
						localLogger.AddLog("[CmpBillReaderRefundsFineDB  ]" + ssql, LoggerSeverities.Debug);

					}
					
					res = d.ExecuteNonQuery(ssql, con, tran,	iUniID, dtDate, iValue, 
																(sFineNumber == null ? DBNull.Value : (object)Convert.ToInt64(sFineNumber)),
																(lFineDef == -1 ? DBNull.Value : (object)lFineDef)); 
 
					


					if(res ==1)
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpBillReaderRefundsFineDB  ]: Commit", LoggerSeverities.Debug);
						tran.Commit();
					}
					else
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpBillReaderRefundsFineDB  ]: RollBack", LoggerSeverities.Debug);
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
					localLogger.AddLog("[CmpBillReaderRefundsFineDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
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
						localLogger.AddLog("[CmpBillReaderRefundsFineDB  ]: Closing...", LoggerSeverities.Debug);
					if( con!= null )
						con.Close();
				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpBillReaderRefundsFineDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
				
			}
			return res;
 
}
	}
}

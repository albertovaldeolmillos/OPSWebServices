using System;
using System.Data;

using OPS.Comm;
using System.Configuration;

namespace OPS.Components.Data
{
	/// <summary>
	/// Descripción breve de CmpCreditCardsDataDB.
	/// </summary>
	public class CmpCreditCardsDataDB : CmpGenericBase
	{
		public CmpCreditCardsDataDB()
		{
			/*
				CCD_ID          NUMBER not null,
				CCD_OPE_ID      NUMBER not null,
				CCD_INS_DATE    DATE not null,
				CCD_TRANS_ID    VARCHAR2(100),
				CCD_NUMBER      VARCHAR2(100),
				CCD_NAME        VARCHAR2(100),
				CCD_EXP_DATE    VARCHAR2(10),
				CCD_QUANTITY    NUMBER,
				CCD_BATCH       VARCHAR2(30),
				CCD_INVOICE     NUMBER,
				CCD_RESPONSE_ID VARCHAR2(100),
				CCD_TYPE        VARCHAR2(50),
				CCD_RESULT      VARCHAR2(100),
				CCD_RESULT_CODE VARCHAR2(20),
				CDD_STATUS		NUMBER
			*/

			_standardFields			= new string[] {	"CCD_ID", "CCD_OPE_ID", "CCD_INS_DATE", 
													    "CCD_TRANS_ID", "CCD_NUMBER", "CCD_NAME", 
													    "CCD_EXP_DATE", "CCD_QUANTITY", "CCD_BATCH",
														"CCD_INVOICE", "CCD_RESPONSE_ID", "CCD_TYPE", 
														"CCD_RESULT", "CCD_RESULT_CODE", "CCD_STATUS" };
			_standardPks			= new string[] { "CCD_ID" };
			_standardTableName		= "CREDIT_CARDS_DATA";
			_standardOrderByField	= "CCD_INS_DATE";
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
		/// 
		public int Insert (IDbTransaction tran, int nCCDOpeID, string sCCDTransId, string sCCDNumber, string sCCDName, 
			string sCCDExpDate, double dCCDQuantity, string sCCDBatch, ulong ulCCDInvoice, string sCCDResponseId,
			string sCCDType, string sCCDResult, string sCCDResultCode, int iStatus)
		{
			Database d = null;					// Database
			ILogger localLogger = null;			// Logger to trace
			IDbConnection con = null;	
			int res = -1;

			try
			{
				// Getting Database
				d = DatabaseFactory.GetDatabase();
				if( d == null )
					return  -1;

				if( tran == null)
					return -3;
				
				// Getting Logger
				localLogger = DatabaseFactory.Logger;

				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = tran.Connection;
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: Opening...", LoggerSeverities.Debug);

				int nNewCCDId = Convert.ToInt32(d.ExecuteScalar("select SEQ_CREDIT_CARDS_DATA.NEXTVAL FROM DUAL", con, tran));
				
				string ssql = " INSERT INTO CREDIT_CARDS_DATA (CCD_ID, CCD_INS_DATE, CCD_OPE_ID, CCD_TRANS_ID, CCD_NUMBER, " +
					" CCD_NAME, CCD_EXP_DATE, CCD_QUANTITY, CCD_BATCH, CCD_INVOICE, CCD_RESPONSE_ID, CCD_TYPE, CCD_RESULT, CCD_RESULT_CODE, CCD_STATUS) " +
					" VALUES (@CREDIT_CARDS_DATA.CCD_ID@,@CREDIT_CARDS_DATA.CCD_INS_DATE@,@CREDIT_CARDS_DATA.CCD_OPE_ID@,@CREDIT_CARDS_DATA.CCD_TRANS_ID@, " +
					" @CREDIT_CARDS_DATA.CCD_NUMBER@,@CREDIT_CARDS_DATA.CCD_NAME@,@CREDIT_CARDS_DATA.CCD_EXP_DATE@,@CREDIT_CARDS_DATA.CCD_QUANTITY@, " +
					" @CREDIT_CARDS_DATA.CCD_BATCH@,@CREDIT_CARDS_DATA.CCD_INVOICE@,@CREDIT_CARDS_DATA.CCD_RESPONSE_ID@,@CREDIT_CARDS_DATA.CCD_TYPE@, " +
					" @CREDIT_CARDS_DATA.CCD_RESULT@,@CREDIT_CARDS_DATA.CCD_RESULT_CODE@,@CREDIT_CARDS_DATA.CCD_STATUS@) ";

				if(localLogger != null)
				{
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpCreditCardsDataDB  ]" + ssql, LoggerSeverities.Debug);
				}
				
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
							
				

				localLogger.AddLog("[CmpCreditCardsDataDB  ] ID: " + nNewCCDId.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] Date: " + DateTime.Now.AddHours(nDifHour).ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] Ope Id: " + nCCDOpeID.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] CCD Trans Id: " + sCCDTransId, LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] CCD Number: " + sCCDNumber, LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] CCD Name: " + sCCDName, LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] Exp Date: " + sCCDExpDate, LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] Quantity: " + dCCDQuantity.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] Batch: " + sCCDBatch, LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] Invoice: " + ulCCDInvoice.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] Response Id: " + sCCDResponseId.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] CCD Type: " + sCCDType.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] CCD Result: " + sCCDResult.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] CCD Result Code: " + sCCDResultCode.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] CCD STATUS: " + iStatus.ToString(), LoggerSeverities.Debug);

				res = d.ExecuteNonQuery(ssql, con, tran, 
					nNewCCDId, DateTime.Now.AddHours(nDifHour), 
					(nCCDOpeID == -1 ? DBNull.Value : (object)nCCDOpeID),
					sCCDTransId, sCCDNumber, sCCDName, sCCDExpDate, 
					dCCDQuantity, sCCDBatch, ulCCDInvoice, sCCDResponseId, sCCDType, sCCDResult, sCCDResultCode,
					(iStatus == -1 ? DBNull.Value : (object)iStatus));
				if(res ==1)
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsDataDB  ]: Commit", LoggerSeverities.Debug);
				}
				else
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsDataDB  ]: RollBack", LoggerSeverities.Debug);
				}		
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsDataDB  ]: Closing...", LoggerSeverities.Debug);

				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsDataDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
			}

			return res;
		}

		public int UpdateStatus (IDbTransaction tran, string sCCDTransId, int nCCDOpeID,int iStatus)
		{
			Database d = null;					// Database
			ILogger localLogger = null;			// Logger to trace
			IDbConnection con = null;	
			int res = -1;

			try
			{
				// Getting Database
				d = DatabaseFactory.GetDatabase();
				if( d == null )
					return  -1;

				if( tran == null)
					return -3;
				
				// Getting Logger
				localLogger = DatabaseFactory.Logger;

				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = tran.Connection;
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: Opening...", LoggerSeverities.Debug);
				
				string ssql = "UPDATE CREDIT_CARDS_DATA SET CCD_OPE_ID=@CREDIT_CARDS_DATA.CCD_OPE_ID@, CCD_STATUS=@CREDIT_CARDS_DATA.CCD_STATUS@ "+
							  "WHERE CCD_TRANS_ID=@CREDIT_CARDS_DATA.CCD_TRANS_ID@ ";

				if(localLogger != null)
				{
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpCreditCardsDataDB  ]" + ssql, LoggerSeverities.Debug);
				}
				
			
				

				localLogger.AddLog("[CmpCreditCardsDataDB  ] CCD Trans Id: " + sCCDTransId, LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] CCD STATUS: " + iStatus.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] Ope Id: " + nCCDOpeID.ToString(), LoggerSeverities.Debug);


				res = d.ExecuteNonQuery(ssql, con, tran, 
										nCCDOpeID, iStatus,sCCDTransId);
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: Commit", LoggerSeverities.Debug);
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsDataDB  ]: Closing...", LoggerSeverities.Debug);

				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsDataDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
			}

			return res;
		}
	
		public int UpdateStatus (IDbTransaction tran, string sCCDTransId ,int iStatus)
		{
			Database d = null;					// Database
			ILogger localLogger = null;			// Logger to trace
			IDbConnection con = null;	
			int res = -1;

			try
			{
				// Getting Database
				d = DatabaseFactory.GetDatabase();
				if( d == null )
					return  -1;

				if( tran == null)
					return -3;
				
				// Getting Logger
				localLogger = DatabaseFactory.Logger;

				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = tran.Connection;
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: Opening...", LoggerSeverities.Debug);
				
				string ssql = "UPDATE CREDIT_CARDS_DATA SET  CCD_STATUS=@CREDIT_CARDS_DATA.CCD_STATUS@ "+
							  "WHERE CCD_TRANS_ID=@CREDIT_CARDS_DATA.CCD_TRANS_ID@ ";

				if(localLogger != null)
				{
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpCreditCardsDataDB  ]" + ssql, LoggerSeverities.Debug);
				}
				
			
				

				localLogger.AddLog("[CmpCreditCardsDataDB  ] CCD Trans Id: " + sCCDTransId, LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardsDataDB  ] CCD STATUS: " + iStatus.ToString(), LoggerSeverities.Debug);


				res = d.ExecuteNonQuery(ssql, con, tran, 
										iStatus,sCCDTransId);
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: Commit", LoggerSeverities.Debug);
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsDataDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsDataDB  ]: Closing...", LoggerSeverities.Debug);

				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsDataDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
			}

			return res;
		}
	

	

	}
}

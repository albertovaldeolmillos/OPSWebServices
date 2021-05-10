using System;
using System.Data;

using OPS.Comm;
using System.Configuration;


namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for CmpCreditCardDB.
	/// </summary>
	public class CmpCreditCardDB : CmpGenericBase
	{
		public CmpCreditCardDB()
		{
			/*
			    CC_ID			NUMBER			N			Identificador único de registro
				CC_INS_DATE		DATE			N			Fecha de inserción del registro
				CC_OPE_ID		NUMBER			N			Operación asociada
				CC_NUMBER		VARCHAR2(20)	N			Número de tarjeta
				CC_NAME			VARCHAR2(40)	N			Nombre del titular de la tarjeta
				CC_EXPRTN_DATE	DATE			N			Fecha de caducidad
				CC_STATE		NUMBER			Y			Identificador de estado
			*/
			_standardFields			= new string[] {	"CC_ID", "CC_INS_DATE", "CC_OPE_ID", 
														"CC_NUMBER", "CC_NAME",
														"CC_EXPRTN_DATE", "CC_STATE", "CC_CODSERV" };
			_standardPks			= new string[] { "CC_ID" };
			_standardTableName		= "CREDIT_CARDS";
			_standardOrderByField	= "CC_INS_DATE";
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
		public int Insert (IDbTransaction tran,int nOpeID, string sCCNumber, string sCCName, DateTime dtExprtnDate, 
			int nCCState,string strCCCodServ, string strCCDiscData)
		{
			return Insert(tran,nOpeID, sCCNumber, sCCName, dtExprtnDate, nCCState,strCCCodServ, strCCDiscData,0);
		}

		public int Insert (IDbTransaction tran, int nOpeID, string sCCNumber, string sCCName, DateTime dtExprtnDate, 
							int nCCState,string strCCCodServ, string strCCDiscData,int iExported)
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
					localLogger.AddLog("[CmpCreditCardDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = tran.Connection;
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardDB  ]: Opening...", LoggerSeverities.Debug);

				//  Open connection
				//con.Open();
				
				//tran =  con.BeginTransaction(IsolationLevel.Serializable);

				// Se 
				int nNewCC = Convert.ToInt32(d.ExecuteScalar("select SEQ_CREDIT_CARDS.NEXTVAL FROM DUAL", con, tran));
				
				string ssql = " INSERT INTO CREDIT_CARDS (CC_ID, CC_INS_DATE, CC_OPE_ID, CC_NUMBER, CC_NAME, " +
					"CC_EXPRTN_DATE, CC_STATE, CC_CODSERV, CC_DISC_DATA, CC_4BEXPORT) " +
					" VALUES (@CREDIT_CARDS.CC_ID@,@CREDIT_CARDS.CC_INS_DATE@,@CREDIT_CARDS.CC_OPE_ID@,@CREDIT_CARDS.CC_NUMBER@, " +
					" @CREDIT_CARDS.CC_NAME@ ,@CREDIT_CARDS.CC_EXPRTN_DATE@ ,@CREDIT_CARDS.CC_STATE@ ,@CREDIT_CARDS.CC_CODSERV@,@CREDIT_CARDS.CC_DISC_DATA@,@CREDIT_CARDS.CC_4BEXPORT@) ";

				
				if(localLogger != null)
				{
					localLogger.AddLog("[CmpCreditCardDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpCreditCardDB  ]" + ssql, LoggerSeverities.Debug);
					localLogger.AddLog("[CmpCreditCardDB  ] OperationsID: " + nOpeID.ToString(), LoggerSeverities.Debug);
					try
					{
						if (sCCNumber.Length>=8)
						{
							string strCCBegin=sCCNumber.Substring(0,4);
							string strCCEnd=sCCNumber.Substring(sCCNumber.Length-4,4);
							localLogger.AddLog("[CmpCreditCardDB  ] PAN: " + strCCBegin+strCCEnd.PadLeft(sCCNumber.Length-4,'*'), LoggerSeverities.Debug);
						}
					}
					catch
					{
					}

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
				
				int iCodServ;

				if (strCCCodServ=="")
				{
					iCodServ=999;
				}
				else
				{
					try
					{
						iCodServ = Convert.ToInt32(strCCCodServ);
					}
					catch
					{
						iCodServ=999;
					}
				}

				//DateTime dtNow = new DateTime();
				
				localLogger.AddLog("[CmpCreditCardDB  ] ID: " + nNewCC.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardDB  ] Date: " + DateTime.Now.AddHours(nDifHour).ToString(), LoggerSeverities.Debug);
				if (sCCName.Length>=8)
				{
					string strCCBegin=sCCName.Substring(0,4);
					string strCCEnd=sCCName.Substring(sCCName.Length-4,4);
					localLogger.AddLog("[CmpCreditCardDB  ] CC Name: " + strCCBegin+strCCEnd.PadLeft(sCCName.Length-4,'*'), LoggerSeverities.Debug);
				}
				localLogger.AddLog("[CmpCreditCardDB  ] Exp Date: " + dtExprtnDate.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardDB  ] State: " + nCCState.ToString(), LoggerSeverities.Debug);
				localLogger.AddLog("[CmpCreditCardDB  ] CodServ: " + iCodServ.ToString(), LoggerSeverities.Debug);

				if (strCCDiscData.Length>=8)
				{
					string strCCBegin=strCCDiscData.Substring(0,4);
					string strCCEnd=strCCDiscData.Substring(strCCDiscData.Length-4,4);
					localLogger.AddLog("[CmpCreditCardDB  ] CCDiscData: " + strCCBegin+strCCEnd.PadLeft(strCCDiscData.Length-4,'*'), LoggerSeverities.Debug);
				}

				localLogger.AddLog("[CmpCreditCardDB  ] Exported: " + iExported.ToString(), LoggerSeverities.Debug);

				res = d.ExecuteNonQuery(ssql, con, tran, 
					nNewCC, DateTime.Now.AddHours(nDifHour), nOpeID, sCCNumber, sCCName, dtExprtnDate, nCCState, iCodServ,strCCDiscData,iExported);  
				if(res ==1)
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardDB  ]: Commit", LoggerSeverities.Debug);
				}
				else
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardDB  ]: RollBack", LoggerSeverities.Debug);
				}
						
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
							// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardDB  ]: Closing...", LoggerSeverities.Debug);

				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
				
			}
			return res;
 
}
	}
}

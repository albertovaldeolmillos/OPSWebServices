using System;
using System.Data;

using OPS.Comm;
using System.Configuration;
using System.Security.Cryptography;

namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for CmpCreditCardsTransactionsDB.
	/// </summary>
	public class CmpCreditCardsTransactionsDB : CmpGenericBase
	{
		public CmpCreditCardsTransactionsDB()
		{
			
			_standardFields			= new string[] {"CCT_ID",          
													"CCT_TRANS_ID",    
													"CCT_INS_DATE",    
													"CCT_OPE_ID",      
													"CCT_NUMBER",      
													"CCT_NAME",        
													"CCT_EXPRTN_DATE", 
													"CCT_STATE",       
													"CCT_STATE_DATE",  
													"CCT_CODSERV",     
													"CCT_DISC_DATA",
												    "CCT_UNI_ID",    
												    "CCT_OPER_INFO" };	

			_standardPks			= new string[] { "CCT_ID" };
			_standardTableName		= "CREDIT_CARDS_TRANSACTIONS";
			_standardOrderByField	= "CCT_INS_DATE";
			_standardOrderByAsc		= "DESC";
	
			_standardRelationFileds	= new string[0]; 
			_standardRelationTables	= new string[0]; 
			_stValidDeleted			= new string[0]; 
			
		}

		public static int TRANS_INSERTED=10;
		public static int TRANS_UNCOMMITTED=20;
		public static int TRANS_BEFORE_COMMMIT=30;
		public static int TRANS_COMMITTED=40;
		public static int TRANS_REJECTED=50;	
		public static int TRANS_BEFORE_VOID=60;		
		public static int TRANS_VOID=70;
		public static int TRANS_VOID_REJECTED=80;
		public static int TRANS_BEFORE_REFUND=90;
		public static int TRANS_REFUNDED=100;
		public static int TRANS_REFUND_REJECTED=110;


		internal const string KEY_MESSAGE_TCP_0	= "75o73K3%0=53?73*h>7*32<5";
		internal const string KEY_MESSAGE_TCP_1	= "35s03!*3!8H3j33*53)73*lf";
		internal const string KEY_MESSAGE_TCP_2	= "7*32z5$8j07!3*35f5%73(30";
		internal const string KEY_MESSAGE_TCP_3	= "*5%57*3j3!*50,73*3(65k3%";
		internal const string KEY_MESSAGE_TCP_4	= "3!*50g73*5=57*3j$8j07!3*";
		internal const string KEY_MESSAGE_TCP_5	= "j07!(*h>7*32<5y8n%=!g5/&";
		internal const string KEY_MESSAGE_TCP_6	= "!8H37t3*5*3(65k3%57*3j3!";
		internal const string KEY_MESSAGE_TCP_7	= "253)73*lf5%73(30*32z5$8j";




		public int InsertAuthTrans (string strTransId, DateTime dtDate, string sCCNumber, string sCCName, DateTime dtExprtnDate, 
			string strCCCodServ, string strCCDiscData,int nQuantity,int nUniId, string strOperInfo, out int nTransId)
		{

			Database d = DatabaseFactory.GetDatabase();
			IDbTransaction tran = null;
			IDbConnection con = null;
			int res=-1;
			nTransId = -1;

			try 
			{
				con = d.GetNewConnection();
				con.Open();
				tran =  con.BeginTransaction(IsolationLevel.Serializable);
				
				res= InsertAuthTrans(tran, strTransId,  dtDate,  sCCNumber,  sCCName,  dtExprtnDate, 
									strCCCodServ,  strCCDiscData, nQuantity, nUniId,  strOperInfo, out  nTransId);

				if(res ==1)
				{
					tran.Commit();
				}
				else
				{
					tran.Rollback();
				}
			}
			catch
			{
				tran.Rollback();
			}
			finally
			{
				if (tran!=null)
				{
					tran.Dispose();	
				}

				if (con!=null)
				{		
					con.Close();
					con.Dispose();
				}

			}

			return res;

		}


		public int InsertInsertedTrans (DateTime dtDate, string sCCNumber, string sCCName, DateTime dtExprtnDate, 
			string strCCCodServ, string strCCDiscData,int nOpeId,int nQuantity,int nUniId, string strOperInfo, out int nTransId)
		{

			Database d = DatabaseFactory.GetDatabase();
			IDbTransaction tran = null;
			IDbConnection con = null;
			int res=-1;
			nTransId = -1;

			try 
			{
				con = d.GetNewConnection();
				con.Open();
				tran =  con.BeginTransaction(IsolationLevel.Serializable);
				
				res=  InsertInsertedTrans (tran, dtDate,  sCCNumber,  sCCName,  dtExprtnDate, 
					strCCCodServ,  strCCDiscData, nOpeId, nQuantity, nUniId,  strOperInfo, out  nTransId);

				if(res ==1)
				{
					tran.Commit();
				}
				else
				{
					tran.Rollback();
				}
			}
			catch
			{
				tran.Rollback();
			}
			finally
			{
				if (tran!=null)
				{
					tran.Dispose();	
				}

				if (con!=null)
				{		
					con.Close();
					con.Dispose();
				}

			}

			return res;


		}

		public int InsertCommitTrans (string strTransId, DateTime dtDate,int nOpeId,int nQuantity,int nUniId, string strOperInfo, out int nTransId)
		{

			Database d = DatabaseFactory.GetDatabase();
			IDbTransaction tran = null;
			IDbConnection con = null;
			int res=-1;
			nTransId = -1;

			try 
			{
				con = d.GetNewConnection();
				con.Open();
				tran =  con.BeginTransaction(IsolationLevel.Serializable);
				
				res=  InsertCommitTrans (tran, strTransId,  dtDate, nOpeId, nQuantity, nUniId,  strOperInfo, out  nTransId);

				if(res ==1)
				{
					tran.Commit();
				}
				else
				{
					tran.Rollback();
				}
			}
			catch
			{
				tran.Rollback();
			}
			finally
			{
				if (tran!=null)
				{
					tran.Dispose();	
				}

				if (con!=null)
				{		
					con.Close();
					con.Dispose();
				}

			}

			return res;
		}

		public int UpdateBeforeCommitTrans (int nTransId,int nOpeId)
		{
			Database d = DatabaseFactory.GetDatabase();
			IDbTransaction tran = null;
			IDbConnection con = null;
			int res=-1;
			nTransId = -1;

			try 
			{
				con = d.GetNewConnection();
				con.Open();
				tran =  con.BeginTransaction(IsolationLevel.Serializable);
				
				res=   UpdateBeforeCommitTrans (tran,  nTransId, nOpeId);

				if(res ==1)
				{
					tran.Commit();
				}
				else
				{
					tran.Rollback();
				}
			}
			catch
			{
				tran.Rollback();
			}
			finally
			{
				if (tran!=null)
				{
					tran.Dispose();	
				}

				if (con!=null)
				{		
					con.Close();
					con.Dispose();
				}

			}

			return res;
		}










		public int InsertAuthTrans (IDbTransaction tran,string strTransId, DateTime dtDate, string sCCNumber, string sCCName, DateTime dtExprtnDate, 
			string strCCCodServ, string strCCDiscData,int nQuantity,int nUniId, string strOperInfo, out int nTransId)
		{

			return Insert(tran,strTransId, dtDate, sCCNumber, sCCName, dtExprtnDate, 
						  strCCCodServ, strCCDiscData,TRANS_UNCOMMITTED,nQuantity,-1, nUniId, strOperInfo, out nTransId);
		}


		public int InsertInsertedTrans (IDbTransaction tran,DateTime dtDate, string sCCNumber, string sCCName, DateTime dtExprtnDate, 
			string strCCCodServ, string strCCDiscData,int nOpeId,int nQuantity,int nUniId, string strOperInfo, out int nTransId)
		{

			int res=-1;
			if (Insert(tran,"", dtDate, sCCNumber, sCCName, dtExprtnDate, 
				strCCCodServ, strCCDiscData,TRANS_INSERTED,nQuantity,nOpeId, nUniId, strOperInfo, out nTransId)==1)
			{
				OPS.Components.Data.CmpCreditCardDB cmpCreditCard = null;
				cmpCreditCard = new OPS.Components.Data.CmpCreditCardDB();
				res=cmpCreditCard.Insert(tran,nOpeId,sCCNumber, sCCName, dtExprtnDate, 0, strCCCodServ, strCCDiscData,1);
			}
			
			return res;


		}

		public int InsertCommitTrans (IDbTransaction tran,string strTransId, DateTime dtDate,int nOpeId,int nQuantity,int nUniId, string strOperInfo, out int nTransId)
		{

			return Insert(tran,strTransId, dtDate, "", "", new DateTime(1900,1,1,0,0,0), 
				"", "",TRANS_COMMITTED,nQuantity,nOpeId, nUniId, strOperInfo, out nTransId);
		}

		public int UpdateBeforeCommitTrans (IDbTransaction tran,int nTransId,int nOpeId)
		{
			int res=-1;
			string sCCNumber;
			string sCCName; 
			DateTime dtExprtnDate;
			int iCCCodServ;
			string strCCDiscData;


			if (Update (nTransId,TRANS_UNCOMMITTED, TRANS_BEFORE_COMMMIT,nOpeId,"")==1)
			{
				OPS.Components.Data.CmpCreditCardDB cmpCreditCard = null;
				cmpCreditCard = new OPS.Components.Data.CmpCreditCardDB();

				if (GetCardData(nTransId,out sCCNumber, out sCCName, out dtExprtnDate, out iCCCodServ, out strCCDiscData)==1)
				{
					res=cmpCreditCard.Insert(tran,nOpeId,sCCNumber, sCCName, dtExprtnDate, 0, iCCCodServ.ToString(), strCCDiscData,1);

				}
			}

			return res;
		}


		public int UpdateConfTrans (int nTransId)
		{
			int res=-1;
			res=Update (nTransId, TRANS_COMMITTED,-1,"");
			return res;
		}


		public int UpdateConfTrans (int nTransId, string strTransId)
		{
			int res=-1;
			res=Update (nTransId, TRANS_COMMITTED,-1,strTransId);
			return res;
		}


		public int UpdateRejectedTrans (int nTransId)
		{
			int res=-1;
			res=Update (nTransId, TRANS_REJECTED,-1,"");
			return res;
		}

		public int UpdateBeforeVoidTrans (int nTransId)
		{
			int res=-1;
			res=Update (nTransId, TRANS_BEFORE_VOID,-1,"");
			return res;
		}

		public int UpdateVoidTrans (int nTransId)
		{
			int res=-1;
			res=Update (nTransId, TRANS_VOID,-1,"");
			return res;
		}

		public int UpdateVoidRejectedTrans (int nTransId)
		{
			int res=-1;
			res=Update (nTransId, TRANS_VOID_REJECTED,-1,"");
			return res;
		}




		public int UpdateBeforeRefundTrans (int nTransId)
		{
			int res=-1;
			res=Update (nTransId, TRANS_BEFORE_REFUND,-1,"");
			return res;
		}

		public int UpdateRefundedTrans (int nTransId)
		{
			int res=-1;
			res=Update (nTransId, TRANS_REFUNDED,-1,"");
			return res;
		}


		public int UpdateRefundRejectedTrans (int nTransId)
		{
			int res=-1;
			res=Update (nTransId, TRANS_REFUND_REJECTED,-1,"");
			return res;
		}

		public bool ExistTransaction(DateTime dtDate,int nUniId)
		{
			Database d = DatabaseFactory.GetDatabase();
			int iNumRegs = Convert.ToInt32(d.ExecuteScalar(string.Format("select count(*) from CREDIT_CARDS_TRANSACTIONS where CCT_UNI_ID={0} and CCT_INS_DATE=to_date('{1}','hh24missddmmyy')",nUniId,OPS.Comm.Dtx.DtxToString(dtDate))));
			return (iNumRegs==1);
		}


		public bool ExistTransaction(int nTransId, DateTime dtDate,int nUniId)
		{
			Database d = DatabaseFactory.GetDatabase();
			int iNumRegs = Convert.ToInt32(d.ExecuteScalar(string.Format("select count(*) from CREDIT_CARDS_TRANSACTIONS where CCT_UNI_ID={0} and CCT_ID={1} and CCT_INS_DATE=to_date('{2}','hh24missddmmyy')",nUniId,nTransId,OPS.Comm.Dtx.DtxToString(dtDate))));
			return (iNumRegs==1);
		}

		public string GetTransactionId(int nTransId)
		{
			Database d = DatabaseFactory.GetDatabase();
			return ((string)d.ExecuteScalar(string.Format("select CCT_TRANS_ID from CREDIT_CARDS_TRANSACTIONS where CCT_ID={0}",nTransId)));
		}



		private int Insert (IDbTransaction tran,string strTransId, DateTime dtDate, string sCCNumber, string sCCName, DateTime dtExprtnDate, 
			string strCCCodServ, string strCCDiscData,int nState,int nQuantity,int nOpeId, int nUniId, string strOperInfo, out int nTransId)
		{
			Database d = null;					// Database
			ILogger localLogger = null;			// Logger to trace
			IDbConnection con = null;			// Connection
			int res = -1;
			nTransId=-1;

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
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = tran.Connection;
				
				if( con == null)
					return -2;

				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Opening...", LoggerSeverities.Debug);
 
				int iNumRegs = Convert.ToInt32(d.ExecuteScalar(string.Format("select count(*) from CREDIT_CARDS_TRANSACTIONS where CCT_UNI_ID={0} and CCT_INS_DATE=to_date('{1}','hh24missddmmyy')",nUniId,OPS.Comm.Dtx.DtxToString(dtDate)), con, tran));
				
				if (iNumRegs==0)
				{

					nTransId = Convert.ToInt32(d.ExecuteScalar("select SEQ_CC_TRANSACTIONS.NEXTVAL FROM DUAL"));

					string ssql = " INSERT INTO CREDIT_CARDS_TRANSACTIONS ("+
																		   "CCT_ID,"+      
																		   "CCT_TRANS_ID,"+
																		   "CCT_INS_DATE,"+
																		   "CCT_OPE_ID,"+  
																		   "CCT_QUANTITY,"+  
																		   "CCT_NUMBER,"+  
																		   "CCT_NAME,"+    
																		   "CCT_EXPRTN_DATE,"+
																		   "CCT_STATE,"+   
																		   "CCT_STATE_DATE,"+
																		   "CCT_CODSERV,"+ 
																		   "CCT_DISC_DATA,"+
																		   "CCT_UNI_ID,"+  
																		   "CCT_OPER_INFO) VALUES ("+
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_ID@,"+      
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_TRANS_ID@,"+
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_INS_DATE@,"+
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_OPE_ID@,"+  
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_QUANTITY@,"+  
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_NUMBER@,"+  
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_NAME@,"+    
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_EXPRTN_DATE@,"+
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_STATE@,"+   
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_STATE_DATE@,"+
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_CODSERV@,"+ 
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_DISC_DATA@,"+
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_UNI_ID@,"+  
																			"@CREDIT_CARDS_TRANSACTIONS.CCT_OPER_INFO@) ";



					if(localLogger != null)
					{
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Executing...", LoggerSeverities.Debug);
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]" + ssql, LoggerSeverities.Debug);
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ] TransId: " + strTransId, LoggerSeverities.Debug);
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ] OperInfo: " + strOperInfo, LoggerSeverities.Debug);
						try
						{
							if (sCCNumber.Length>=8)
							{
								string strCCBegin=sCCNumber.Substring(0,4);
								string strCCEnd=sCCNumber.Substring(sCCNumber.Length-4,4);
								localLogger.AddLog("[CmpCreditCardsTransactionsDB  ] PAN: " + strCCBegin+strCCEnd.PadLeft(sCCNumber.Length-4,'*'), LoggerSeverities.Debug);
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
					res = d.ExecuteNonQuery(ssql, con, tran, 
						nTransId, 
						strTransId,
						dtDate, 
						(nOpeId==-1 ? DBNull.Value : (object)nOpeId),
						nQuantity,
						(sCCNumber=="" ? DBNull.Value : (object)sCCNumber),
						(sCCName=="" ? DBNull.Value : (object)sCCName),
						(dtExprtnDate.Year==1900 ? DBNull.Value : (object)dtExprtnDate),
						nState, 
						DateTime.Now.AddHours(nDifHour),
						(iCodServ==-1 ? DBNull.Value : (object)iCodServ),
						(strCCDiscData=="" ? DBNull.Value : (object)strCCDiscData),
						nUniId,
						(((strOperInfo==null)||(strOperInfo=="")) ? DBNull.Value : (object)strOperInfo));
						
						
						
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
				else
				{
					res=1;
				}
						
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
							// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Closing...", LoggerSeverities.Debug);

				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
				
			}
			return res;
 
		}
	


		private int Update (int nTransId, int nState,int nOpeId,string strTransId)
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
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = d.GetNewConnection();
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Opening...", LoggerSeverities.Debug);

				//  Open connection
				con.Open();
				
				tran =  con.BeginTransaction(IsolationLevel.Serializable);

				if( tran == null)
					return -3;
 
				int iNumRegs = Convert.ToInt32(d.ExecuteScalar(string.Format("select count(*) from CREDIT_CARDS_TRANSACTIONS where CCT_ID={0}",nTransId), con, tran));
				
				if (iNumRegs==1)
				{


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

					string ssql = string.Format("update CREDIT_CARDS_TRANSACTIONS set " +
												((nOpeId==-1) ? "" : "CCT_OPE_ID="+nOpeId.ToString()+",")+							
												((strTransId=="") ? "" : "CCT_TRANS_ID='"+strTransId+"',")+							
												"CCT_STATE={0},"+
												"CCT_STATE_DATE=to_date('{1}','hh24missddmmyy') "+
												"where CCT_ID={2}",
													nState,OPS.Comm.Dtx.DtxToString(DateTime.Now.AddHours(nDifHour)),
													nTransId);



					if(localLogger != null)
					{
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Executing...", LoggerSeverities.Debug);
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]" + ssql, LoggerSeverities.Debug);
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ] TransId: " + nTransId.ToString(), LoggerSeverities.Debug);
					}
										
							
				

					//DateTime dtNow = new DateTime();
					res = d.ExecuteNonQuery(ssql, con, tran);
						
						
					if(res ==1)
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpCreditCardDB  ]: Commit", LoggerSeverities.Debug);
						tran.Commit();
					}
					else
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpCreditCardDB  ]: RollBack", LoggerSeverities.Debug);
						tran.Rollback();
					}
				}
				else
				{
					res=0;
				}
						
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
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
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Closing...", LoggerSeverities.Debug);
					if( con!= null )
						con.Close();
				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
				
			}
			return res;
 
		}


		private int Update (int nTransId, int nSearchState,int nState,int nOpeId,string strTransId)
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
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = d.GetNewConnection();
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Opening...", LoggerSeverities.Debug);

				//  Open connection
				con.Open();
				
				tran =  con.BeginTransaction(IsolationLevel.Serializable);

				if( tran == null)
					return -3;
 
				int iNumRegs = Convert.ToInt32(d.ExecuteScalar(string.Format("select count(*) from CREDIT_CARDS_TRANSACTIONS where CCT_ID={0}",nTransId), con, tran));
				
				if (iNumRegs==1)
				{


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

					string ssql = string.Format("update CREDIT_CARDS_TRANSACTIONS set " +
						((nOpeId==-1) ? "" : "CCT_OPE_ID="+nOpeId.ToString()+",")+							
						((strTransId=="") ? "" : "CCT_TRANS_ID='"+strTransId+"',")+							
						"CCT_STATE={0},"+
						"CCT_STATE_DATE=to_date('{1}','hh24missddmmyy') "+
						"where CCT_ID={2} and CCT_STATE = {3}",
						nState,OPS.Comm.Dtx.DtxToString(DateTime.Now.AddHours(nDifHour)),
						nTransId,nSearchState);



					if(localLogger != null)
					{
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Executing...", LoggerSeverities.Debug);
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]" + ssql, LoggerSeverities.Debug);
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ] TransId: " + nTransId.ToString(), LoggerSeverities.Debug);
					}
										
							
				

					//DateTime dtNow = new DateTime();
					res = d.ExecuteNonQuery(ssql, con, tran);
						
						
					if(res ==1)
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpCreditCardDB  ]: Commit", LoggerSeverities.Debug);
						tran.Commit();
					}
					else
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpCreditCardDB  ]: RollBack", LoggerSeverities.Debug);
						tran.Rollback();
					}
				}
				else
				{
					res=0;
				}
						
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
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
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Closing...", LoggerSeverities.Debug);
					if( con!= null )
						con.Close();
				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
				
			}
			return res;
 
		}


		public int UpdateError (int nTransId, int iErrorCode, string strErrorMsg, int iRetries)
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
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = d.GetNewConnection();
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Opening...", LoggerSeverities.Debug);

				//  Open connection
				con.Open();
				
				tran =  con.BeginTransaction(IsolationLevel.Serializable);

				if( tran == null)
					return -3;
 
				int iNumRegs = Convert.ToInt32(d.ExecuteScalar(string.Format("select count(*) from CREDIT_CARDS_TRANSACTIONS where CCT_ID={0}",nTransId), con, tran));
				
				if (iNumRegs==1)
				{


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

					string ssql = string.Format("update CREDIT_CARDS_TRANSACTIONS set " +
						((iErrorCode==-1) ? "" : "CCT_ERRORCODE="+iErrorCode.ToString()+",")+							
						((strErrorMsg=="") ? "" : "CCT_ERRORMSG='"+strErrorMsg+"',")+							
						"CCT_CURRRETRIES={0},"+
						"CCT_STATE_DATE=to_date('{1}','hh24missddmmyy') "+	
						"where CCT_ID={2}",
						iRetries,OPS.Comm.Dtx.DtxToString(DateTime.Now.AddHours(nDifHour)),nTransId);



					if(localLogger != null)
					{
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Executing...", LoggerSeverities.Debug);
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]" + ssql, LoggerSeverities.Debug);
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ] TransId: " + nTransId.ToString(), LoggerSeverities.Debug);
					}
										
							
				

					//DateTime dtNow = new DateTime();
					res = d.ExecuteNonQuery(ssql, con, tran);
						
						
					if(res ==1)
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpCreditCardDB  ]: Commit", LoggerSeverities.Debug);
						tran.Commit();
					}
					else
					{
						if(localLogger != null)
							localLogger.AddLog("[CmpCreditCardDB  ]: RollBack", LoggerSeverities.Debug);
						tran.Rollback();
					}
				}
				else
				{
					res=0;
				}
						
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
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
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Closing...", LoggerSeverities.Debug);
					if( con!= null )
						con.Close();
				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
				
			}
			return res;
 
		}



		public int GetCardData (int nTransId, out string sCCNumber, out string sCCName, out DateTime dtExprtnDate, 
								 out  int iCCCodServ, out string strCCDiscData)
		{
			Database d = null;					// Database
			ILogger localLogger = null;			// Logger to trace
			DataTable dt=null;
			int res = -1;
			sCCNumber= "";
			sCCName= "";
			dtExprtnDate= DateTime.Now;
			iCCCodServ= -999;
			strCCDiscData= "";


			try
			{

				// Getting Database
				d = DatabaseFactory.GetDatabase();
				if( d == null )
					return  -1;

				// Getting Logger
				localLogger = DatabaseFactory.Logger;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Opening...", LoggerSeverities.Debug);
 
				dt =d.FillDataTable(string.Format("select "+
												"CCT_NUMBER,"+  
												"CCT_NAME,"+    
												"TO_CHAR(CCT_EXPRTN_DATE,'hh24missddmmyy') CCT_EXPRTN_DATE,"+
												"CCT_CODSERV,"+ 
												"CCT_DISC_DATA "+					
												"from CREDIT_CARDS_TRANSACTIONS "+
												"where CCT_ID={0}",nTransId),"CARDS");


			

				if (dt.Rows.Count==1)
				{
					sCCNumber= Decrypt(nTransId.ToString(),dt.Rows[0].ItemArray[dt.Columns.IndexOf("CCT_NUMBER")].ToString());
					sCCName= Decrypt(nTransId.ToString(),dt.Rows[0].ItemArray[dt.Columns.IndexOf("CCT_NAME")].ToString());
					dtExprtnDate= OPS.Comm.Dtx.StringToDtx(dt.Rows[0].ItemArray[dt.Columns.IndexOf("CCT_EXPRTN_DATE")].ToString());
					iCCCodServ= Convert.ToInt32(dt.Rows[0].ItemArray[dt.Columns.IndexOf("CCT_CODSERV")].ToString());
					strCCDiscData= Decrypt(nTransId.ToString(),dt.Rows[0].ItemArray[dt.Columns.IndexOf("CCT_DISC_DATA")].ToString());
					res=1;
			
				}
						
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: Closing...", LoggerSeverities.Debug);
				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpCreditCardsTransactionsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
				
			}
			return res;
 
		}

		public string Decrypt(string strCCID, string strCCNumber)
		{
			string strEdtCC_NUMBER = strCCNumber;
			string strEdtCC_ID = strCCID;
			string strEdtDesencriptado;

			try
			{
				string strKey = GetKeyToApply(strEdtCC_ID);

				if ((strEdtCC_NUMBER.Substring(0,6)=="%&!()=") && (strEdtCC_NUMBER.Substring(strEdtCC_NUMBER.Length-6,6)=="%&!()="))
				{
					strEdtCC_NUMBER = strEdtCC_NUMBER.Substring(6,strEdtCC_NUMBER.Length-12);
				}

				byte [] byEncrypt = HexString_To_Bytes(strEdtCC_NUMBER);

				TripleDESCryptoServiceProvider TripleDesProvider=  new TripleDESCryptoServiceProvider();
				int sizeKey = System.Text.Encoding.Default.GetByteCount (strKey);
				byte [] byKey;
				byKey = new byte[sizeKey];	
				System.Text.Encoding.Default.GetBytes(strKey,0, strKey.Length,byKey, 0);
				TripleDesProvider.Mode=CipherMode.ECB;
				TripleDesProvider.Key=byKey;
				Array.Clear(TripleDesProvider.IV,0,TripleDesProvider.IV.Length);
						
				OPS.Comm.Cryptography.TripleDes.OPSTripleDesEncryptor OPSTripleDesEnc= new OPS.Comm.Cryptography.TripleDes.OPSTripleDesEncryptor(TripleDesProvider);
				byte [] byDecrypt;

				byDecrypt = OPSTripleDesEnc.Desencriptar(byEncrypt); 

				strEdtDesencriptado = GetDataAsString(byDecrypt,0);

				char [] chTrim=  new char[1];
				chTrim[0] = '\0';

				strEdtDesencriptado = strEdtDesencriptado.TrimEnd(chTrim);

			}
			catch
			{				
				strEdtDesencriptado = "Error";
			}

			return strEdtDesencriptado;
	
		}


		private byte[] HexString_To_Bytes(string strInput)
		{
			// i variable used to hold position in string
			int i = 0;
			// x variable used to hold byte array element position
			int x = 0;
			// allocate byte array based on half of string length
			byte[] bytes = new byte[(strInput.Length) / 2];
			// loop through the string - 2 bytes at a time converting it to decimal equivalent and store in byte array
			while (strInput.Length > i + 1)
			{
				long lngDecimal = Convert.ToInt32(strInput.Substring(i, 2), 16);
				bytes[x] = Convert.ToByte(lngDecimal);
				i = i + 2;
				++x;
			}
			// return the finished byte array of decimal values
			return bytes;
		}

		///
		/// Convert byte_array to string
		///
		///
		private string Bytes_To_HexString(byte[] bytes_Input)
		{
			// convert the byte array back to a true string
			string strTemp = "";
			for (int x = 0; x <= bytes_Input.GetUpperBound(0); x++)
			{
				int number = int.Parse(bytes_Input[x].ToString());
				strTemp += number.ToString("X").PadLeft(2, '0');
			}
			// return the finished string of hex values
			return strTemp;
		}

		protected string GetDataAsString(byte[] data, int ignoreLastBytes)
		{
			if (data.Length < ignoreLastBytes)
				return "";

			System.Text.Decoder utf8Decoder = System.Text.Encoding.UTF8.GetDecoder();
			int charCount = utf8Decoder.GetCharCount(data, 0, (data.Length  - ignoreLastBytes));
			char[] recievedChars = new char[charCount];
			utf8Decoder.GetChars(data, 0, data.Length - ignoreLastBytes, recievedChars, 0);
			String recievedString = new String(recievedChars);
			return recievedString;
		}


		protected static string GetKeyToApply(string key)
		{
			string strRes=KEY_MESSAGE_TCP_5;
			int iSum=0;
			int iMod;

			if (key.Length == 0)
			{
				strRes=KEY_MESSAGE_TCP_5;
			}
			else if (key.Length >= 24)
			{
				strRes = key.Substring(0,24);
			}
			else
			{
				for(int i=0; i<key.Length;i++)
				{
					iSum+=Convert.ToInt32(key[i]);
					
				}

				iMod=iSum%8;

				switch(iMod)
				{
					case 0:
						strRes=KEY_MESSAGE_TCP_0;
						break;
					case 1:
						strRes=KEY_MESSAGE_TCP_1;
						break;
					case 2:
						strRes=KEY_MESSAGE_TCP_2;
						break;
					case 3:
						strRes=KEY_MESSAGE_TCP_3;
						break;
					case 4:
						strRes=KEY_MESSAGE_TCP_4;
						break;
					case 5:
						strRes=KEY_MESSAGE_TCP_5;
						break;
					case 6:
						strRes=KEY_MESSAGE_TCP_6;
						break;
					case 7:
						strRes=KEY_MESSAGE_TCP_7;
						break;

					default:
						strRes=KEY_MESSAGE_TCP_0;
						break;
				}


				strRes = key + strRes.Substring(0,24-key.Length);


			}


			return strRes;

		}
	

	
	}


}

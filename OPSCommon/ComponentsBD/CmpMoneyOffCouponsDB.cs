using System;
using System.Data;
using OPS.Comm;
using System.Configuration;

namespace OPS.Components.Data
{


	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS.
	/// </summary>
	public class CmpMoneyOffCouponsDB: CmpGenericBase
	{
		public enum MoneyOffCouponsStates
		{
			PendingActivation = 0,
			Actived = 1,
			Used = 2,
			Cancelled=3
		}

		public CmpMoneyOffCouponsDB() 
		{
			_standardFields		= new string[] {"COUP_ID", "COUP_CODE",  "COUP_BCOUP_ID",  "COUP_CCOUP_ID",  "COUP_CREATION_DATE",  "COUP_START_DATE",  "COUP_EXP_DATE",  "COUP_STATE",  "COUP_DISCOUNT_TIME",
												   "COUP_DISCOUNT_MONEY", "COUP_ACT_DATE",  "COUP_USE_DATE",  "COUP_CANCEL_DATE",  "COUP_USE_VEHICLEID",  "COUP_DPAY_ID" };
			_standardPks		= new string[] {"COUP_ID"};
			_standardTableName	= "MONEYOFF_COUPON";
			_standardOrderByField	= "COUP_ID";
			_standardOrderByAsc		= "ASC";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"GRP_DGRP_ID","GRP_RELATED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpVWGroupsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}


		public bool ExistCode (IDbTransaction tran, string strCode)
		{
			Database d = DatabaseFactory.GetDatabase();					
			int iNumRows = Convert.ToInt32(d.ExecuteScalar(string.Format("select count(*) from MONEYOFF_COUPON where COUP_CODE='{0}'", strCode),tran));
			return (iNumRows>0);
		}


		public uint GetMaximumPk ()
		{
			Database d = DatabaseFactory.GetDatabase();
			uint uiRes = Convert.ToUInt32(d.ExecuteScalar("SELECT NVL(MAX(COUP_ID),0) FROM MONEYOFF_COUPON"));
			return uiRes;
		}

		public uint GetMaximumPk (IDbTransaction tran)
		{
			Database d = DatabaseFactory.GetDatabase();
			uint uiRes = Convert.ToUInt32(d.ExecuteScalar("SELECT NVL(MAX(COUP_ID),0) FROM MONEYOFF_COUPON",tran));
			return uiRes;
		}

		public int InsertCoupon (IDbTransaction tran, uint uiId, string strCode ,int iNumberOfMinutesGiven,int iNumberOfCentsGiven,uint uiIdBookofCoupons )
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
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = tran.Connection;
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Opening...", LoggerSeverities.Debug);
				
				string ssql="";

				
				ssql =	"INSERT INTO MONEYOFF_COUPON (COUP_ID, COUP_CODE, COUP_BCOUP_ID,COUP_DISCOUNT_TIME,COUP_DISCOUNT_MONEY) "+
					"VALUES (@MONEYOFF_COUPON.COUP_ID@,@MONEYOFF_COUPON.COUP_CODE@,@MONEYOFF_COUPON.COUP_BCOUP_ID@, @MONEYOFF_COUPON.COUP_DISCOUNT_TIME@, @MONEYOFF_COUPON.COUP_DISCOUNT_MONEY@ )";


				if(localLogger != null)
				{
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]" + ssql, LoggerSeverities.Debug);
				}
														
				res = d.ExecuteNonQuery(ssql, con, tran, uiId,strCode ,uiIdBookofCoupons,
					(iNumberOfMinutesGiven == -1 ? DBNull.Value : (object)iNumberOfMinutesGiven),
					(iNumberOfCentsGiven == -1 ? DBNull.Value : (object)iNumberOfCentsGiven));


				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Commit", LoggerSeverities.Debug);



			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Closing...", LoggerSeverities.Debug);

				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
			}

			return res;
		}

		public int InsertCoupon (uint uiId, string strCode, int iNumberOfMinutesGiven, int iNumberOfCentsGiven, uint uiIdBookofCoupons )
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
				
				// Getting Logger
				localLogger = DatabaseFactory.Logger;

				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = d.GetNewConnection();
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Opening...", LoggerSeverities.Debug);
				
				string ssql="";

				ssql =	"INSERT INTO MONEYOFF_COUPON (COUP_ID, COUP_CODE, COUP_BCOUP_ID,COUP_DISCOUNT_TIME,COUP_DISCOUNT_MONEY) "+
					"VALUES (@MONEYOFF_COUPON.COUP_ID@,@MONEYOFF_COUPON.COUP_CODE@,@MONEYOFF_COUPON.COUP_BCOUP_ID@, @MONEYOFF_COUPON.COUP_DISCOUNT_TIME@, @MONEYOFF_COUPON.COUP_DISCOUNT_MONEY@ )";

				if(localLogger != null)
				{
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]" + ssql, LoggerSeverities.Debug);
				}
				
				con.Open();
				res = d.ExecuteNonQuery(ssql, con, uiId, strCode, uiIdBookofCoupons,
					(iNumberOfMinutesGiven == -1 ? DBNull.Value : (object)iNumberOfMinutesGiven),
					(iNumberOfCentsGiven == -1 ? DBNull.Value : (object)iNumberOfCentsGiven));

				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Coupon inserted OK", LoggerSeverities.Debug);
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Error inserting coupon" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
				con.Close();
			}

			return res;
		}

		public int DeleteCoupon( uint uiId )
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
				
				// Getting Logger
				localLogger = DatabaseFactory.Logger;

				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = d.GetNewConnection();
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Opening...", LoggerSeverities.Debug);
				
				string ssql="";

				ssql ="DELETE MONEYOFF_COUPON WHERE MONEYOFF_COUPON.COUP_ID = @MONEYOFF_COUPON.COUP_ID@";

				if(localLogger != null)
				{
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]" + ssql, LoggerSeverities.Debug);
				}
				
				con.Open();
				res = d.ExecuteNonQuery(ssql, con, uiId);

				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Coupon deleted OK", LoggerSeverities.Debug);
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Error deleting coupon" + e.Message, LoggerSeverities.Debug);					
			}
			finally 
			{
				con.Close();
			}

			return res;
		}

		public int UpdateState (IDbTransaction tran, uint uiId ,int iState, DateTime startDate, DateTime expDate, 
								DateTime actDate, DateTime useDate, DateTime cancelDate, 
								uint uiCustomerId, string strUsePlate, int iDPayId)
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
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = tran.Connection;
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Opening...", LoggerSeverities.Debug);
				
				string ssql="";

				switch ((MoneyOffCouponsStates)iState)
				{

					case MoneyOffCouponsStates.Actived:  //Active
						ssql =	"UPDATE MONEYOFF_COUPON SET	COUP_STATE=@MONEYOFF_COUPON.COUP_STATE@, "+
															"COUP_START_DATE=@MONEYOFF_COUPON.COUP_START_DATE@, "+
															"COUP_EXP_DATE=@MONEYOFF_COUPON.COUP_EXP_DATE@, "+
															"COUP_ACT_DATE=@MONEYOFF_COUPON.COUP_ACT_DATE@, "+
															"COUP_CCOUP_ID=@MONEYOFF_COUPON.COUP_CCOUP_ID@ "+
								"WHERE COUP_ID=@MONEYOFF_COUPON.COUP_ID@ ";

						break;

					case MoneyOffCouponsStates.Used: //Used

						ssql =	"UPDATE MONEYOFF_COUPON SET	COUP_STATE=@MONEYOFF_COUPON.COUP_STATE@, "+
															"COUP_USE_DATE=@MONEYOFF_COUPON.COUP_USE_DATE@, "+
															"COUP_USE_VEHICLEID=@MONEYOFF_COUPON.COUP_USE_VEHICLEID@, "+
															"COUP_DPAY_ID=@MONEYOFF_COUPON.COUP_DPAY_ID@ "+
								"WHERE COUP_ID=@MONEYOFF_COUPON.COUP_ID@ ";						
						break;

					case MoneyOffCouponsStates.Cancelled:

						ssql =	"UPDATE MONEYOFF_COUPON SET	COUP_STATE=@MONEYOFF_COUPON.COUP_STATE@, "+
															"COUP_CANCEL_DATE=@MONEYOFF_COUPON.COUP_CANCEL_DATE@ "+
								"WHERE COUP_ID=@MONEYOFF_COUPON.COUP_ID@ ";		
						break;

					case MoneyOffCouponsStates.PendingActivation:
					default:

						ssql =	"UPDATE MONEYOFF_COUPON SET	COUP_STATE=@MONEYOFF_COUPON.COUP_STATE@, "+
															"COUP_START_DATE=NULL, "+
															"COUP_EXP_DATE=NULL, "+
															"COUP_ACT_DATE=NULL, "+
															"COUP_CANCEL_DATE=NULL "+
							"WHERE COUP_ID=@MONEYOFF_COUPON.COUP_ID@ ";

						break;


				}



				if(localLogger != null)
				{
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]" + ssql, LoggerSeverities.Debug);
				}
				
			
							
				localLogger.AddLog("[CmpMoneyOffCouponsDB  ] COUP_STATE: " + iState.ToString(), LoggerSeverities.Debug);

				switch ((MoneyOffCouponsStates)iState)
				{

					case MoneyOffCouponsStates.Actived:  //Active						
						res = d.ExecuteNonQuery(ssql, con, tran, iState, startDate,expDate ,actDate,
							(uiCustomerId == 0 ? DBNull.Value : (object)uiCustomerId), uiId);
						break;

					case MoneyOffCouponsStates.Used: //Used
						res = d.ExecuteNonQuery(ssql, con, tran, iState, useDate,((strUsePlate==null)||(strUsePlate.Length==0))?DBNull.Value: (object)strUsePlate ,(iDPayId==-1)?DBNull.Value: (object)iDPayId, uiId);
						break;

					case MoneyOffCouponsStates.Cancelled:						
						res = d.ExecuteNonQuery(ssql, con, tran, iState, cancelDate, uiId);
						break;

					case MoneyOffCouponsStates.PendingActivation:
					default:
						res = d.ExecuteNonQuery(ssql, con, tran, iState, uiId);
						break;
				}


				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Commit", LoggerSeverities.Debug);
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: Closing...", LoggerSeverities.Debug);

				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffCouponsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
			}

			return res;
		}

	}


}

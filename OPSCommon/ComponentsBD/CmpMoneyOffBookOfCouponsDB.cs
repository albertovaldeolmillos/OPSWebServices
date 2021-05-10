using System;
using System.Data;
using OPS.Comm;
using System.Configuration;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS.
	/// </summary>
	public class CmpMoneyOffBookOfCouponsDB: CmpGenericBase
	{
		public CmpMoneyOffBookOfCouponsDB() 
		{
			_standardFields		= new string[] {"BCOUP_ID", "BCOUP_COUP_MIN_ID",  "BCOUP_COUP_MAX_ID"};
			_standardPks		= new string[] {"BCOUP_ID"};
			_standardTableName	= "MONEYOFF_BOOKOFCOUPONS";
			_standardOrderByField	= "BCOUP_ID";
			_standardOrderByAsc		= "ASC";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"GRP_DGRP_ID","GRP_RELATED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpVWGroupsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}

		public uint GetMaximumPk ()
		{
			Database d = DatabaseFactory.GetDatabase();
			uint uiRes = Convert.ToUInt32(d.ExecuteScalar("SELECT NVL(MAX(BCOUP_ID),0) FROM MONEYOFF_BOOKOFCOUPONS"));
			return uiRes;
		}
		
		
		public uint GetMaximumPk (IDbTransaction tran)
		{
			Database d = DatabaseFactory.GetDatabase();
			uint uiRes = Convert.ToUInt32(d.ExecuteScalar("SELECT NVL(MAX(BCOUP_ID),0) FROM MONEYOFF_BOOKOFCOUPONS",tran));
			return uiRes;
		}

		public int InsertBookOfCoupons (IDbTransaction tran, uint uiId)
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
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = tran.Connection;
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: Opening...", LoggerSeverities.Debug);
				
				string ssql="";

				
				ssql =	"INSERT INTO MONEYOFF_BOOKOFCOUPONS (BCOUP_ID) "+
					"VALUES (@MONEYOFF_BOOKOFCOUPONS.BCOUP_ID@)";


				if(localLogger != null)
				{
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]" + ssql, LoggerSeverities.Debug);
				}
				
			
											
				res = d.ExecuteNonQuery(ssql, con, tran, uiId);


				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: Commit", LoggerSeverities.Debug);
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: Closing...", LoggerSeverities.Debug);

				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
			}

			return res;
		}


		public int UpdateBookOfCoupons (IDbTransaction tran, uint uiId, uint uiIdMinIdCoupon, uint uiIdMaxIdCoupon )
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
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = tran.Connection;
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: Opening...", LoggerSeverities.Debug);
				
				string ssql="";

				
				ssql =	"update MONEYOFF_BOOKOFCOUPONS set BCOUP_COUP_MIN_ID=@MONEYOFF_BOOKOFCOUPONS.BCOUP_COUP_MIN_ID@, "+
					"BCOUP_COUP_MAX_ID=@MONEYOFF_BOOKOFCOUPONS.BCOUP_COUP_MAX_ID@  "+
					"WHERE  BCOUP_ID=@MONEYOFF_BOOKOFCOUPONS.BCOUP_ID@";


				if(localLogger != null)
				{
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]" + ssql, LoggerSeverities.Debug);
				}
				
			
											
				res = d.ExecuteNonQuery(ssql, con, tran, uiIdMinIdCoupon,uiIdMaxIdCoupon,uiId );


				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: Commit", LoggerSeverities.Debug);
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: Closing...", LoggerSeverities.Debug);

				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffBookOfCouponsDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
			}

			return res;
		}




	}
}


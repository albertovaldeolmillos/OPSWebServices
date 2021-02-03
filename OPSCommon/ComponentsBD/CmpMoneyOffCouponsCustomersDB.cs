using System;
using System.Data;
using OPS.Comm;
using System.Configuration;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS.
	/// </summary>
	public class CmpMoneyOffCouponsCustomersDB: CmpGenericBase
	{
		public CmpMoneyOffCouponsCustomersDB() 
		{
			_standardFields		= new string[] {"CCOUP_ID", "CCOUP_DESCRIPTION","CCOUP_ADDRESS",
												"CCOUP_CITY","CCOUP_ZIPCODE","CCOUP_EMAIL","CCOUP_TELEPHONE"};
			_standardPks		= new string[] {"CCOUP_ID"};
			_standardTableName	= "MONEYOFF_CUSTOMERS";
			_standardOrderByField	= "CCOUP_ID";
			_standardOrderByAsc		= "ASC";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"GRP_DGRP_ID","GRP_RELATED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpVWGroupsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}

		public uint GetMaximumPk ()
		{
			Database d = DatabaseFactory.GetDatabase();
			uint uiRes = Convert.ToUInt32(d.ExecuteScalar("SELECT NVL(MAX(CCOUP_ID),0) FROM MONEYOFF_BOOKOFCOUPONS"));
			return uiRes;
		}

		public uint GetMaximumPk (IDbTransaction tran)
		{
			Database d = DatabaseFactory.GetDatabase();
			uint uiRes = Convert.ToUInt32(d.ExecuteScalar("SELECT NVL(MAX(CCOUP_ID),0) FROM MONEYOFF_BOOKOFCOUPONS",tran));
			return uiRes;
		}


		public int InsertCouponsCustomer (IDbTransaction tran, uint uiId,string strDescription,string strAddress, 
										string strCity, string strZipcode, string strEmail, string strTelephone)
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
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = tran.Connection;
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: Opening...", LoggerSeverities.Debug);
				
				string ssql="";

				
				ssql =	"INSERT INTO MONEYOFF_CUSTOMERS (CCOUP_ID,CCOUP_DESCRIPTION,CCOUP_ADDRESS,CCOUP_CITY,CCOUP_ZIPCODE,CCOUP_EMAIL,CCOUP_TELEPHONE) "+
					"VALUES (@MONEYOFF_CUSTOMERS.CCOUP_ID@,@MONEYOFF_CUSTOMERS.CCOUP_DESCRIPTION@,"+
					"VALUES (@MONEYOFF_CUSTOMERS.CCOUP_ADDRESS@,@MONEYOFF_CUSTOMERS.CCOUP_CITY@,"+
					"VALUES (@MONEYOFF_CUSTOMERS.CCOUP_ZIPCODE@,@MONEYOFF_CUSTOMERS.CCOUP_EMAIL@,"+
					"VALUES (@MONEYOFF_CUSTOMERS.CCOUP_TELEPHONE@)";
					


				if(localLogger != null)
				{
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]" + ssql, LoggerSeverities.Debug);
				}
				
			
											
				res = d.ExecuteNonQuery(ssql, con, tran, uiId, strDescription,strAddress, strCity,  strZipcode,  strEmail,  strTelephone);


				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: Commit", LoggerSeverities.Debug);
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: Closing...", LoggerSeverities.Debug);

				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
			}

			return res;
		}


		public int UpdateCouponsCustomer (IDbTransaction tran, uint uiId,string strDescription,string strAddress, 
										string strCity, string strZipcode, string strEmail, string strTelephone)
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
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: Getting Connection...", LoggerSeverities.Debug);
				
				// Getting connection
				con = tran.Connection;
				
				if( con == null)
					return -2;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: Opening...", LoggerSeverities.Debug);
				
				string ssql="";

				ssql =	"update MONEYOFF_CUSTOMERS set CCOUP_DESCRIPTION=@MONEYOFF_CUSTOMERS.CCOUP_DESCRIPTION@, "+
							"CCOUP_ADDRESS=@MONEYOFF_CUSTOMERS.CCOUP_ADDRESS@, "+
							"CCOUP_DESCRIPTION=@MONEYOFF_CUSTOMERS.CCOUP_DESCRIPTION@, "+
							"CCOUP_CITY=@MONEYOFF_CUSTOMERS.CCOUP_CITY@, "+
							"CCOUP_ZIPCODE=@MONEYOFF_CUSTOMERS.CCOUP_ZIPCODE@, "+
							"CCOUP_EMAIL=@MONEYOFF_CUSTOMERS.CCOUP_EMAIL@, "+
							"CCOUP_TELEPHONE=@MONEYOFF_CUSTOMERS.CCOUP_TELEPHONE@ "+
							"WHERE  CCOUP_ID=@MONEYOFF_CUSTOMERS.CCOUP_ID@";


				if(localLogger != null)
				{
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: Executing...", LoggerSeverities.Debug);
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]" + ssql, LoggerSeverities.Debug);
				}
				
			
											
				res = d.ExecuteNonQuery(ssql, con, tran, strDescription, strAddress, strCity,  strZipcode,  strEmail,  strTelephone, uiId);


				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: Commit", LoggerSeverities.Debug);
			}
			catch (Exception e)
			{
				res = -1;
				if(localLogger != null)
					localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: Closing...", LoggerSeverities.Debug);

				}  
				catch (Exception e)
				{
					res = -1;
					if(localLogger != null)
						localLogger.AddLog("[CmpMoneyOffCouponsCustomersDB  ]: RollBack" + e.Message, LoggerSeverities.Debug);					
				}
			}

			return res;
		}





	}
}


using System;
using System.Text;
using System.Data;



namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS.
	/// </summary>
	public class CmpMobileOrdersDB : CmpGenericBase
	{
		public CmpMobileOrdersDB() 
		{
			_standardFields		= new string[] 
				{
					"MO_ID",                
					"MO_MU_ID",             
					"MO_DATE",              
					"MO_HORA",              
					"MO_AMOUNT",            
					"MO_CURRENCY",          
					"MO_MERCHANT_CODE",     
					"MO_TERMINAL",          
					"MO_SIGNATURE",         
					"MO_RESPONSE",          
					"MO_MERCHANT_DATA",     
					"MO_SECURE_PAYMENT",    
					"MO_TRANSACTION_TYPE",  
					"MO_CARD_COUNTRY",      
					"MO_AUTHORISATION_CODE",
					"MO_LANGUAGE",          
					"MO_CARD_TYPE",         
					"MO_TIMESTAMP"         
				};
			_standardPks		= new string[] {"MO_ID"};
			_standardTableName	= "MOBILE_ORDERS";
			_standardOrderByField	= "MO_TIMESTAMP";
			_standardOrderByAsc		= "ASC";
		

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"OPE_DOPE_ID","OPE_ART_ID","OPE_GRP_ID","OPE_UNI_ID","OPE_DPAY_ID"} ;
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpOperationsDefDB,ComponentsBD","OPS.Components.Data.CmpArticlesDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}

using System;
using System.Text;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS.
	/// </summary>
	public class CmpMobileQuotesDB : CmpGenericBase
	{
		public CmpMobileQuotesDB() 
		{
			_standardFields		= new string[] 
				{
					"MQ_ID",                     
					"MQ_MU_ID",                                        
					"MQ_DATE",                                    
					"MQ_QTY",                                    
					"MQ_RESULT",                                         
					"MQ_MONTH",                                       
					"MQ_YEAR"                                 
				};
			_standardPks		= new string[] {"MQ_ID"};
			_standardTableName	= "MOBILE_QUOTES";
			_standardOrderByField	= "MQ_DATE";
			_standardOrderByAsc		= "ASC";
		

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"OPE_DOPE_ID","OPE_ART_ID","OPE_GRP_ID","OPE_UNI_ID","OPE_DPAY_ID"} ;
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpOperationsDefDB,ComponentsBD","OPS.Components.Data.CmpArticlesDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}

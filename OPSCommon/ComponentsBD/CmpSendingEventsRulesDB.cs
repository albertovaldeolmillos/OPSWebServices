using System;
using System.Text;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS.
	/// </summary>
	public class CmpSendingEventRulesDB : CmpGenericBase
	{
		public CmpSendingEventRulesDB() 
		{
			_standardFields		= new string[] 
				{
					"SEEVR_ID",     
					"SEEVR_TEV_ID",  
					"SEEVR_DL_ID",   
					"SEEVR_GRP_ID",  
					"SEEVR_TIM_ID",  
					"SEEVR_DDAY_ID", 
					"SEEVR_DATEINI", 
					"SEEVR_DATEEND", 
					"SEEVR_UNI_ID", 
					"SEEVR_DAY_ID",  
					"SEEVR_DALA_ID", 
					"SEEVR_DALV_ID" 
				};
			_standardPks		= new string[] {"SEEVR_ID"} ;
			_standardTableName	= "SENDING_EVENTS_RULES";
			_standardOrderByField	= "SEEVR_ID";
			_standardOrderByAsc		= "ASC";
		

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"OPE_DOPE_ID","OPE_ART_ID","OPE_GRP_ID","OPE_UNI_ID","OPE_DPAY_ID"} ;
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpOperationsDefDB,ComponentsBD","OPS.Components.Data.CmpArticlesDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}


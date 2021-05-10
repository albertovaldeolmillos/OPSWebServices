using System;
using System.Text;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS.
	/// </summary>
	public class CmpMapAlarmHisDB : CmpGenericBase
	{
		public CmpMapAlarmHisDB() 
		{
			_standardFields		= new string[] 
				{
					"ALXH_ID",                     
					"ALXH_TIMESTAMP_INI",                                        
					"ALXH_TIMESTAMP_END",                                    
					"ALXH_ALS_ID",                                    
					"ALXH_ALD_ID",                                         
					"ALXH_ALL_ID",                                       
					"ALXH_GUA_ID",                                 
					"ALXH_UNI_ID"                                    
				};
			_standardPks		= new string[] {"ALXH_ID"};
			_standardTableName	= "MAP_ALARMS_HIS";
			_standardOrderByField	= "ALXH_TIMESTAMP_INI";
			_standardOrderByAsc		= "DESC";
		

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"OPE_DOPE_ID","OPE_ART_ID","OPE_GRP_ID","OPE_UNI_ID","OPE_DPAY_ID"} ;
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpOperationsDefDB,ComponentsBD","OPS.Components.Data.CmpArticlesDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}

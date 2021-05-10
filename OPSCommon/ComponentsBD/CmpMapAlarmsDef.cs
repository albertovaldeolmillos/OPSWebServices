using System;
using System.Text;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS.
	/// </summary>
	public class CmpMapAlarmsDefDB : CmpGenericBase
	{
		public CmpMapAlarmsDefDB() 
		{
			_standardFields		= new string[] 
				{
					"ALD_ID",                     
					"ALD_NAME",                                        
					"ALD_NAME_LIT_ID"                                    
				};
			_standardPks		= new string[] {"ALD_ID"};
			_standardTableName	= "MAP_ALARMS_DEF";
			_standardOrderByField	= "ALD_ID";
			_standardOrderByAsc		= "ASC";
		

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"OPE_DOPE_ID","OPE_ART_ID","OPE_GRP_ID","OPE_UNI_ID","OPE_DPAY_ID"} ;
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpOperationsDefDB,ComponentsBD","OPS.Components.Data.CmpArticlesDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}

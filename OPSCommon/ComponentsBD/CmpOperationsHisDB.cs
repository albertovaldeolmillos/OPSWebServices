namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS_HIS.
	/// </summary>
	public class CmpOperationsHisDB : CmpGenericBase
	{
		public CmpOperationsHisDB()
		{
			_standardFields		= new string[]
				{"HOPE_ID", "HOPE_DOPE_ID", "HOPE_HOPE_ID", "HOPE_ART_ID", "HOPE_GRP_ID",
				"HOPE_UNI_ID", "HOPE_DPAY_ID", "HOPE_MOVDATE", "HOPE_INIDATE", "HOPE_ENDDATE",
				"HOPE_DURATION", "HOPE_VALUE", "HOPE_VEHICLEID"};
			_standardPks		= new string[] {"OPE_ID"};
			_standardTableName	= "OPERATIONS_HIS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
				//{ "HOPE_DOPE_ID",
				//"HOPE_ART_ID",
				//"HOPE_GRP_ID",
				//"HOPE_UNI_ID",
				//"HOPE_DPAY_ID" };
			_standardRelationTables	= new string[0];
				//{ "OPS.Components.Data.CmpOperationsDefDB,ComponentsBD",
				//"OPS.Components.Data.CmpArticlesDB,ComponentsBD",
				//"OPS.Components.Data.CmpGroupsDB,ComponentsBD",
				//"OPS.Components.Data.CmpUnitsDB,ComponentsBD",
				//"OPS.Components.Data.CmpPayTypesDefDB,ComponentsBD" };
			_stValidDeleted			= new string[0];
		}
	}
}

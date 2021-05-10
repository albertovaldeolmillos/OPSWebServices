namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for STATUS.
	/// </summary>
	public class CmpStatusDB : CmpGenericBase
	{
		public CmpStatusDB()
		{
			_standardFields		= new string[] {"STA_ID", "STA_DGRP_ID", "STA_GRP_ID", "STA_DDAY_ID", "STA_DAY_ID", "STA_UNI_ID", "STA_TIM_ID", "STA_DSTA_ID", "STA_PSTA_ID"};
			_standardPks		= new string[] {"STA_ID"};
			_standardTableName	= "STATUS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"STA_VALID", "STA_DELETED"};
		}
	}
}



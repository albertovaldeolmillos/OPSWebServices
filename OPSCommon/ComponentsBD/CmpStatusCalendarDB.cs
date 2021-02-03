namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for VW_CALENDAR_STATUS.
	/// </summary>
	public class CmpStatusCalendarDB : CmpGenericBase
	{
		public CmpStatusCalendarDB()
		{
			_standardFields		= new string[] {"GRP_ID", "GRP_DESCSHORT", "DDAY_DESCSHORT", "TIM_DESC", "DGRP_DESCSHORT", "DAY_DATE", "DAY_DESC", "DSTA_DESCSHORT", "STA_ID"};
			_standardPks		= new string[] {"STA_ID"};
			_standardTableName	= "VW_CALENDAR_STATUS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}
	}
}

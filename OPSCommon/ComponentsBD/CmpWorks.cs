namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS.
	/// </summary>
	public class CmpWorks: CmpGenericBase
	{
		public CmpWorks() 
		{
			_standardFields		= new string[] {"WORK_ID", "WORK_SS_ID",  "WORK_PDA_ID",  "WORK_USR_ID",  "WORK_UNI_ID",  "WORK_NUM_PARK_SPACES",  "WORK_REMARKS",  "WORK_INI_DATE",  "WORK_END_DATE"};
			_standardPks		= new string[] {"WORK_ID"};
			_standardTableName	= "WORKS";
			_standardOrderByField	= "WORK_ID";
			_standardOrderByAsc		= "DESC";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"GRP_DGRP_ID","GRP_RELATED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpVWGroupsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}


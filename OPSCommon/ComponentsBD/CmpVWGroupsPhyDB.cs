namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for VW_GROUPS_PHY.
	/// </summary>
	public class CmpVWGroupsPhyDB: CmpGenericBase
	{
		public CmpVWGroupsPhyDB()
		{
			_standardFields		= new string[] { "GRP_ID", "DGRP_PHYORDER", "CGRP_TYPE", "CGRP_CHILD", "GRP_DGRP_ID" };
			_standardPks		= new string[] { "GRP_ID", "DGRP_PHYORDER", "CGRP_TYPE", "CGRP_CHILD", "GRP_DGRP_ID" };
			_standardTableName	= "VW_GROUPS_PHY";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}
	}
}
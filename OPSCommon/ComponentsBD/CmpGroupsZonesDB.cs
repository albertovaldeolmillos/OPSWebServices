namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS.
	/// </summary>
	public class CmpGroupsZonesDB: CmpGenericBase
	{
		public CmpGroupsZonesDB() 
		{
			_standardFields		= new string[] {"GRP_ID", "GRP_DGRP_ID", "GRP_DESCSHORT", "GRP_DESCLONG", "GRP_RELATED", "GRP_POSX", "GRP_POSY", "GRP_PATH"};
			_standardPks		= new string[] {"GRP_ID"};
			_standardTableName	= "VW_GROUPS_ZONES";
			_standardOrderByField	= "GRP_DESCSHORT";
			_standardOrderByAsc		= "ASC";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"GRP_DGRP_ID","GRP_RELATED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpVWGroupsDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"GRP_VALID", "GRP_DELETED"};
		}
	}
}


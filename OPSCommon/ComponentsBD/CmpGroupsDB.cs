namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS.
	/// </summary>
	public class CmpGroupsDB: CmpGenericBase
	{
		public CmpGroupsDB() 
		{
			_standardFields		= new string[] {"GRP_ID", "GRP_DGRP_ID", "GRP_DESCSHORT", "GRP_DESCLONG", "GRP_RELATED", "GRP_POSX", "GRP_POSY", "GRP_PATH", "GRP_COLOUR", "GRP_MOB_LIT_ID"};
			_standardPks		= new string[] {"GRP_ID"};
			_standardTableName	= "GROUPS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"GRP_DGRP_ID","GRP_RELATED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpVWGroupsDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"GRP_VALID", "GRP_DELETED"};
		}
	}
}


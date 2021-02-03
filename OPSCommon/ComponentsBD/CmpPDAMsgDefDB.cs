namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS.
	/// </summary>
	public class CmpPDAMsgDefDB: CmpGenericBase
	{
		public CmpPDAMsgDefDB() 
		{
			_standardFields		= new string[] {"PDAMSGD_ID", "PDAMSGD_DESCSHORT",  "PDAMSGD_DESCLONG",  "PDAMSGD_LIT_ID"};
			_standardPks		= new string[] {"PDAMSGD_ID"};
			_standardTableName	= "PDAMSG_DEF";
			_standardOrderByField	= "PDAMSGD_ID";
			_standardOrderByAsc		= "ASC";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"GRP_DGRP_ID","GRP_RELATED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpVWGroupsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}


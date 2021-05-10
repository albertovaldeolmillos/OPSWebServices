namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS.
	/// </summary>
	public class CmpPDAMsgDB: CmpGenericBase
	{
		public CmpPDAMsgDB() 
		{
			_standardFields		= new string[] {"PDAMSG_ID", "PDAMSG_SRC_UNI_ID", "PDAMSG_DST_UNI_ID", "PDAMSG_USR_ID",  "PDAMSG_TEXT",  "PDAMSG_DATE",  "PDAMSG_STATE"};
			_standardPks		= new string[] {"PDAMSG_ID","PDAMSG_SRC_UNI_ID"};
			_standardTableName	= "V_PDA_MSGS";
			_standardOrderByField	= "PDAMSG_DATE";
			_standardOrderByAsc		= "DESC";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"GRP_DGRP_ID","GRP_RELATED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpVWGroupsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}


using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS_DEF.
	/// </summary>
	public class CmpGroupsDefDB: CmpGenericBase
	{
		public CmpGroupsDefDB() 
		{
			_standardFields		= new string[] {"DGRP_ID", "DGRP_DESCSHORT", "DGRP_DESCLONG", "DGRP_DGRP_ID_VISIBLELEVEL", "DGRP_PHYORDER"};
			_standardPks		= new string[] {"DGRP_ID"};
			_standardTableName	= "GROUPS_DEF";
			_standardOrderByField	= "";		
			_standardOrderByAsc		= "";		
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"DGRP_VALID", "DGRP_DELETED"};
		}

		/// <summary>
		/// Get the GroupDef by a group id
		/// </summary>
		/// <param name="rid">Id of the group</param>
		/// <returns>A DataTable with the GroupDef row</returns>
		public DataTable GetGroupDefByGroup (int groupId)
		{	
			Database d = DatabaseFactory.GetDatabase();
			return d.FillDataTable("SELECT GROUPS_DEF.DGRP_ID, GROUPS_DEF.DGRP_PHYORDER "
				+ "FROM GROUPS_DEF INNER JOIN GROUPS ON GROUPS_DEF.DGRP_ID = GROUPS.GRP_DGRP_ID "
				+ "WHERE GROUPS.GRP_ID = @GROUPS.GRP_ID@", "GROUPS_DEF", groupId);
		}
	}
}

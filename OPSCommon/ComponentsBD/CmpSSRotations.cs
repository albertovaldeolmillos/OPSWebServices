namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS.
	/// </summary>
	public class CmpSSRotations: CmpGenericBase
	{
		public CmpSSRotations() 
		{
			_standardFields		= new string[] {"SSR_ID","SSR_SS_ID", "SSR_INI_DATE", "SSR_END_DATE", "SSR_USR_ID", "SSR_UNI_ID","SSR_RESI_NO_TICKET","SSR_RESI_TICKET", "SSR_NO_RESI_NO_TICKET","SSR_NO_RESI_TICKET",   "SSR_SPECIAL_VEHICLE",  "SSR_MINUSVALID", "SSR_ZONE_ID", "SSR_SECTOR_ID", "SSR_ROUTE_ID" };
			_standardPks		= new string[] {"SSR_ID"};
			_standardTableName	= "V_SS_ROTATIONS";
			_standardOrderByField	= "SSR_INI_DATE";
			_standardOrderByAsc		= "DESC";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"GRP_DGRP_ID","GRP_RELATED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpVWGroupsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}


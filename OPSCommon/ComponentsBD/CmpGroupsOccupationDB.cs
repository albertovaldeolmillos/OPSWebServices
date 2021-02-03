namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS_OCCUPATION.
	/// </summary>
	public class CmpGroupsOccupationDB : CmpGenericBase
	{
		public CmpGroupsOccupationDB()
		{
			_standardFields		= new string[] {"OGRP_ID", "OGRP_GRP_ID", "OGRP_LEVEL", "OGPR_INIDATE", "OGPR_ENDDATE"};
			_standardPks		= new string[] {"OGRP_ID"};
			_standardTableName	= "GROUPS_OCCUPATION";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}
	}
}

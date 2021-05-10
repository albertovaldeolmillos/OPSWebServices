namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for STREETS.
	/// </summary>
	public class CmpRoutesDB : CmpGenericBase
	{


		public CmpRoutesDB() 
		{
			_standardFields				= new string[] { "CGRP_UNIQUE_ID", "CGRP_ID", "CGRP_CHILD", "CGRP_ORDER"};
			_standardPks				= new string[] {"CGRP_UNIQUE_ID"};
			_standardTableName			= "V_GROUPS_CHILDS_ROUTES";
			_standardOrderByField		= "CGRP_CHILD";
			_standardOrderByAsc			= "ASC";
		
			_standardRelationFileds		= new string[0]; 
			_standardRelationTables		= new string[0];
			_stValidDeleted				= new string[] {"CGRP_VALID", "CGRP_DELETED"};
		}
	}
}
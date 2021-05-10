namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for REPLICATIONS_PDA.
	/// </summary>
	public class CmpReplicationsPdaDB : CmpGenericBase
	{
		public CmpReplicationsPdaDB()
		{
			_standardFields		= new string[] {"RPDA_ID", "RPDA_UNI_ID", "RPDA_GRP_ID"};
			_standardPks		= new string[] {"RPDA_ID"};
			_standardTableName	= "REPLICATIONS_PDA";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds		= new string[0]; //{"RPDA_UNI_ID","RPDA_GRP_ID"};
			_standardRelationTables		= new string[0]; //{"OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD"};
			_stValidDeleted				= new string[0];
		}
	}
}



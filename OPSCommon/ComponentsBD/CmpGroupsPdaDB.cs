namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS_OCCUPATION.
	/// </summary>
	public class CmpGroupsPdaDB: CmpGenericBase
	{
		public CmpGroupsPdaDB() 
		{
			_standardFields		= new string[] {"GPDA_ID", "GPDA_GRP_ID", "GPDA_GRP_ID_CHILD", "GPDA_DESC"};
			_standardPks		= new string[] {"GPDA_ID"};
			_standardTableName	= "GROUPS_PDA";
			_standardOrderByField	= "";		
			_standardOrderByAsc		= "";		
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"GPDA_VALID", "GPDA_DELETED"};
		}
	}
}



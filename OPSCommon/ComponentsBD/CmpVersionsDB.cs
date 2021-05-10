namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for VERSIONS.
	/// </summary>
	public class CmpVersionsDB : CmpGenericBase
	{
		public CmpVersionsDB()
		{
			_standardFields		= new string[] {"VER_ID", "VER_TABLE", "VER_VALUE"};
			_standardPks		= new string[] {"VER_ID"};
			_standardTableName	= "VERSIONS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}
	}
}


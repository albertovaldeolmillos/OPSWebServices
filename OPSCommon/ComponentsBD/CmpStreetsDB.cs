namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for STREETS.
	/// </summary>
	public class CmpStreetsDB : CmpGenericBase
	{
		public CmpStreetsDB() 
		{
			_standardFields				= new string[] { "STR_ID", "STR_DESC", "STR_MIN", "STR_MAX", "STR_PROVINCIA", "STR_MUNICIPIO", "STR_TIPOVIA" };
			_standardPks				= new string[] { "STR_ID" };
			_standardTableName			= "STREETS";
			_standardOrderByField		= "";
			_standardOrderByAsc			= "";
		
			_standardRelationFileds		= new string[0];
			_standardRelationTables		= new string[0];
			_stValidDeleted				= new string[] {"STR_VALID", "STR_DELETED"};
		}
	}
}
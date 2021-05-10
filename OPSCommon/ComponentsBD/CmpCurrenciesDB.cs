namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for CURRENCIES.
	/// </summary>
	public class CmpCurrenciesDB: CmpGenericBase
	{
		public CmpCurrenciesDB()
		{
			_standardFields		= new string[] {"CUR_ID", "CUR_DEFAULT", "CUR_DESCSHORT", "CUR_DESCLONG", "CUR_SYMBOL", "CUR_INTPRECISION", "CUR_FORMAT"};
			_standardPks		= new string[] {"CUR_ID"};
			_standardTableName	= "CURRENCIES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"CUR_VALID", "CUR_DELETED"};
		}
	}
}

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for BLACK_LISTS_DEF.
	/// </summary>
	public class CmpBlackListsDefDB: CmpGenericBase
	{
		public CmpBlackListsDefDB()
		{
			_standardFields		= new string[] {"DBLIS_ID", "DBLIS_DESCSHORT", "DBLIS_DESCLONG"};
			_standardPks		= new string[] {"DBLIS_ID"};
			_standardTableName	= "BLACK_LISTS_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"DBLIS_VALID", "DBLIS_DELETED"};
		}
	}
}

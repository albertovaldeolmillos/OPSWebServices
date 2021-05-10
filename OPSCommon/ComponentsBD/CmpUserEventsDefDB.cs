namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for USER_EVENTS_DEF.
	/// </summary>
	public class CmpUserEventsDefDB : CmpGenericBase
	{
		public CmpUserEventsDefDB()
		{
			_standardFields		= new string[] {"DUE_ID", "DUE_DESCSHORT", "DUE_DESCLONG", "DUE_LIT_ID"};
			_standardPks		= new string[] {"DUE_ID"};
			_standardTableName	= "USER_EVENTS_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"DUE_VALID", "DUE_DELETED"};
		}
	}
}